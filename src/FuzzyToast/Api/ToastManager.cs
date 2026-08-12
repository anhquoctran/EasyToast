using System.Drawing;
using System.Windows.Forms;
using FuzzyToast.Internal;
using FuzzyToast.Layout;

namespace FuzzyToast;

/// <summary>
/// Instance-scoped toast manager for one owner control. Preferred: one manager per owner.
/// </summary>
public sealed class ToastManager : IDisposable
{
	private readonly Control? _owner;
	private readonly IScreenProvider _screen;
	private readonly IUiMarshaler _marshaler;
	private readonly Func<ToastOptions, ToastHandle, IToastView> _viewFactory;
	private readonly List<ActiveToast> _active = new();
	private readonly object _gate = new();
	private bool _disposed;
	private long _sequence;

	public ToastManager(Control owner, ToastManagerOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(owner);
		if (owner.IsDisposed)
		{
			throw new ObjectDisposedException(nameof(owner));
		}

		_owner = owner;
		Options = options ?? new ToastManagerOptions();
		_screen = new WinFormsScreenProvider(owner);
		_marshaler = new WinFormsUiMarshaler(owner);
		_viewFactory = (_, handle) => new ToastForm(handle);
		owner.Disposed += OwnerOnDisposed;
		// Share with Toast.Build(owner, …) so one stack / one event stream per form.
		ToastManagerRegistry.Register(owner, this);
	}

	/// <summary>Test constructor: owner may be null; inject screen, marshaler, and view factory.</summary>
	internal ToastManager(
		Control? owner,
		ToastManagerOptions options,
		IScreenProvider screenProvider,
		IUiMarshaler marshaler,
		Func<ToastOptions, ToastHandle, IToastView> viewFactory)
	{
		_owner = owner;
		Options = options ?? throw new ArgumentNullException(nameof(options));
		_screen = screenProvider ?? throw new ArgumentNullException(nameof(screenProvider));
		_marshaler = marshaler ?? throw new ArgumentNullException(nameof(marshaler));
		_viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
		owner?.Disposed += OwnerOnDisposed;
	}

	public Control Owner =>
		_owner ?? throw new InvalidOperationException("This manager was created without an owner (test mode).");

	public ToastManagerOptions Options { get; }
	public bool IsDisposed => _disposed;

	public IReadOnlyList<ToastHandle> ActiveToasts
	{
		get
		{
			lock (_gate)
				return _active.Select(a => a.Handle).ToList();
		}
	}

	public int Count
	{
		get
		{
			lock (_gate)
				return _active.Count;
		}
	}

	public event EventHandler<ToastChangedEventArgs>? ToastAdded;
	public event EventHandler<ToastChangedEventArgs>? ToastRemoved;
	public event EventHandler? CollectionCleared;
	public event EventHandler<ToastRejectedEventArgs>? ToastRejected;

	public ToastBuilder Create() => new(this);

	public ToastHandle Show(ToastOptions options)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(options);

		ToastHandle? result = null;
		Exception? error = null;
		_marshaler.Invoke(() =>
		{
			try { result = ShowCore(options); }
			catch (Exception ex) { error = ex; }
		});
		if (error is not null)
			throw error;
		return result!;
	}

	public async Task<ToastHandle> ShowAsync(ToastOptions options, CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(options);

		ToastHandle handle;
		if (_marshaler.InvokeRequired)
		{
			var tcs = new TaskCompletionSource<ToastHandle>(TaskCreationOptions.RunContinuationsAsynchronously);
			await _marshaler.InvokeAsync(() =>
			{
				try { tcs.TrySetResult(ShowCore(options)); }
				catch (Exception ex) { tcs.TrySetException(ex); }
			}).ConfigureAwait(false);
			handle = await tcs.Task.ConfigureAwait(false);
		}
		else
		{
			handle = ShowCore(options);
		}

		if (handle.IsVisible && cancellationToken.CanBeCanceled)
		{
			if (cancellationToken.IsCancellationRequested)
				handle.Dismiss();
			else
				cancellationToken.Register(() => handle.Dismiss());
		}

		return handle;
	}

	public void DismissAll()
	{
		if (_disposed)
			return;

		_marshaler.Invoke(() =>
		{
			List<ActiveToast> snapshot;
			lock (_gate)
				snapshot = _active.ToList();
			foreach (var item in snapshot)
				item.View.BeginDismiss();
		});
	}

	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;

		if (_owner is not null)
			_owner.Disposed -= OwnerOnDisposed;

		try
		{
			_marshaler.Invoke(() =>
			{
				List<ActiveToast> snapshot;
				lock (_gate)
					snapshot = _active.ToList();
				foreach (var item in snapshot)
				{
					try { item.View.BeginDismiss(); } catch { /* ignore */ }
					try { item.View.Dispose(); } catch { /* ignore */ }
				}

				lock (_gate)
					_active.Clear();
				CollectionCleared?.Invoke(this, EventArgs.Empty);
			});
		}
		catch
		{
			// owner may already be gone
		}
	}

	internal void DismissInternal(ToastHandle handle)
	{
		if (_disposed)
			return;

		_marshaler.Invoke(() =>
		{
			ActiveToast? entry;
			lock (_gate)
				entry = _active.FirstOrDefault(a => a.Handle.Id == handle.Id);
			entry?.View.BeginDismiss();
		});
	}

	private ToastHandle ShowCore(ToastOptions options)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		options.Validate();

		var activeSnapshot = SnapshotActiveOldestFirst();
		var decision = CapacityPolicy.Evaluate(
			Options.OverflowPolicy,
			Options.MaxToasts,
			Options.MaxToastsPerPosition,
			options.Position,
			activeSnapshot);

		switch (decision.Action)
		{
			case CapacityAction.RejectNewest:
			{
				var rejected = new ToastHandle(NewId(), options, ToastHandleState.RejectedCapacity, this);
				ToastRejected?.Invoke(this, new ToastRejectedEventArgs(rejected, options, decision.Reason));
				return rejected;
			}
			case CapacityAction.Throw:
				throw new InvalidOperationException($"Toast capacity exceeded: {decision.Reason}");
			case CapacityAction.RemoveVictimThenAllow:
				if (decision.VictimId is not null)
					RemoveVictimSync(decision.VictimId);
				break;
			case CapacityAction.Allow:
				break;
		}

		var id = NewId();
		var handle = new ToastHandle(id, options, ToastHandleState.Visible, this);
		var scheme = ThemeCatalog.Resolve(options.Theme, options.CustomColors);
		var durationMs = Options.ResolveDurationMs(options);
		var view = _viewFactory(options, handle);

		view.Closed += (_, _) => OnViewClosed(handle, view);
		view.Clicked += (_, _) => handle.RaiseClicked();
		view.Hovered += (_, _) => handle.RaiseHovered();
		view.Submitted += (_, text) => handle.RaiseSubmitted(text);
		// Form dismisses itself after raising Submitted.

		view.Apply(options, scheme, durationMs, Options.PauseOnHover, Options.PlaySound && !options.IsMuted);

		int stackIndex;
		lock (_gate)
		{
			stackIndex = _active.Count(a => a.Position == options.Position);
			_active.Add(new ActiveToast(handle, view, options.Position, Interlocked.Increment(ref _sequence)));
		}

		var metrics = ResolveMetrics(options.EnableInput);
		var area = GetArea(options.Position);
		var location = ToastLayoutEngine.ComputeLocation(options.Position, stackIndex, metrics, area);
		view.SetBounds(new Rectangle(location, new Size(metrics.ToastWidth, metrics.ToastHeight)));
		view.Show(_owner);

		ToastAdded?.Invoke(this, new ToastChangedEventArgs(handle));
		return handle;
	}

	private ToastLayoutMetrics ResolveMetrics(bool inputable = false)
	{
		var scale = DpiScaling.GetScale(_owner);
		return DpiScaling.ScaleMetrics(Options.ToLayoutMetrics(inputable), scale);
	}

	private void RemoveVictimSync(string victimId)
	{
		ActiveToast? victim;
		lock (_gate)
		{
			var idx = _active.FindIndex(a => a.Handle.Id == victimId);
			if (idx < 0)
				return;
			victim = _active[idx];
			_active.RemoveAt(idx);
		}

		if (victim.Handle.State == ToastHandleState.Visible)
			victim.Handle.MarkDismissed();

		try { victim.View.BeginDismiss(); } catch { /* ignore */ }
		try { victim.View.Dispose(); } catch { /* ignore */ }

		ToastRemoved?.Invoke(this, new ToastChangedEventArgs(victim.Handle));
		Reflow(victim.Position);
	}

	private void OnViewClosed(ToastHandle handle, IToastView view)
	{
		bool removed;
		ToastPosition? position = null;
		lock (_gate)
		{
			var idx = _active.FindIndex(a => a.Handle.Id == handle.Id);
			if (idx < 0)
			{
				removed = false;
			}
			else
			{
				position = _active[idx].Position;
				_active.RemoveAt(idx);
				removed = true;
			}
		}

		if (!removed)
			return;

		if (handle.State == ToastHandleState.Visible)
			handle.MarkDismissed();

		try { view.Dispose(); } catch { /* ignore */ }

		ToastRemoved?.Invoke(this, new ToastChangedEventArgs(handle));

		if (position is not null)
			Reflow(position.Value);

		lock (_gate)
		{
			if (_active.Count == 0)
				CollectionCleared?.Invoke(this, EventArgs.Empty);
		}
	}

	private void Reflow(ToastPosition position)
	{
		List<ActiveToast> stack;
		lock (_gate)
			stack = _active.Where(a => a.Position == position).OrderBy(a => a.Sequence).ToList();

		// Reflow using non-input height as baseline; active input toasts keep their bounds
		// until dismissed (stride uses max height among stack for spacing safety).
		var metrics = ResolveMetrics(inputable: stack.Any(a => a.Handle.Options.EnableInput));
		var area = GetArea(position);
		for (var i = 0; i < stack.Count; i++)
		{
			var itemMetrics = ResolveMetrics(stack[i].Handle.Options.EnableInput);
			// Use tallest stride for vertical stack spacing consistency.
			var strideMetrics = metrics.ToastHeight >= itemMetrics.ToastHeight ? metrics : itemMetrics;
			var loc = ToastLayoutEngine.ComputeLocation(position, i, strideMetrics, area);
			stack[i].View.SetBounds(new Rectangle(loc, new Size(itemMetrics.ToastWidth, itemMetrics.ToastHeight)));
		}
	}

	/// <summary>
	/// Working area for stacking. Uses the owner's monitor on Windows 10/11 multi-monitor
	/// so toasts appear on the same screen as the host form (taskbar-aware WorkingArea).
	/// </summary>
	private ScreenWorkingArea GetArea(ToastPosition position)
	{
		if (_screen is WinFormsScreenProvider win)
			return win.GetOwnerOrPrimaryWorkingArea();

		// Test fakes / non-WinForms providers: keep left/right extremes.
		return position switch
		{
			ToastPosition.TopLeft or ToastPosition.BottomLeft => _screen.GetLeftmostWorkingArea(),
			_ => _screen.GetRightmostWorkingArea()
		};
	}

	private List<(string Id, ToastPosition Position)> SnapshotActiveOldestFirst()
	{
		lock (_gate)
			return _active.OrderBy(a => a.Sequence).Select(a => (a.Handle.Id, a.Position)).ToList();
	}

	private static string NewId() => Guid.NewGuid().ToString("N");

	private void OwnerOnDisposed(object? sender, EventArgs e) => Dispose();

	private sealed class ActiveToast
	{
		public ActiveToast(ToastHandle handle, IToastView view, ToastPosition position, long sequence)
		{
			Handle = handle;
			View = view;
			Position = position;
			Sequence = sequence;
		}

		public ToastHandle Handle { get; }
		public IToastView View { get; }
		public ToastPosition Position { get; }
		public long Sequence { get; }
	}
}
