namespace FuzzyToast;

/// <summary>Live runtime handle for a toast (or a rejected show attempt).</summary>
public sealed class ToastHandle : IDisposable
{
	private readonly TaskCompletionSource<bool> _dismissedTcs =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	private ToastManager? _manager;
	private ToastHandleState _state;
	private bool _disposed;

	internal ToastHandle(string id, ToastOptions options, ToastHandleState state, ToastManager? manager)
	{
		Id = id;
		Options = options;
		_state = state;
		_manager = manager;
		if (state == ToastHandleState.RejectedCapacity || state == ToastHandleState.Dismissed)
			_dismissedTcs.TrySetResult(true);
	}

	/// <summary>Stable identifier for this show attempt (hex GUID without dashes).</summary>
	public string Id { get; }

	/// <summary>Options snapshot used for this toast.</summary>
	public ToastOptions Options { get; }

	/// <summary>Current lifecycle state.</summary>
	public ToastHandleState State => _state;

	/// <summary><see langword="true"/> when <see cref="State"/> is <see cref="ToastHandleState.Visible"/>.</summary>
	public bool IsVisible => _state == ToastHandleState.Visible;

	/// <summary><see langword="true"/> when the toast has closed.</summary>
	public bool IsDismissed => _state == ToastHandleState.Dismissed;

	/// <summary><see langword="true"/> when show was rejected by capacity policy.</summary>
	public bool WasRejected => _state == ToastHandleState.RejectedCapacity;

	/// <summary>
	/// Completes when dismissed. For <see cref="ToastHandleState.RejectedCapacity"/>, completes immediately (RanToCompletion).
	/// </summary>
	public Task WhenDismissed => _dismissedTcs.Task;

	/// <summary>Raised when the toast body is clicked. Args include <see cref="ToastOptions.Tag"/> and metadata.</summary>
	public event EventHandler<ToastInteractionEventArgs>? Clicked;

	/// <summary>Raised when the pointer hovers the toast. Args include tag/metadata.</summary>
	public event EventHandler<ToastInteractionEventArgs>? Hovered;

	/// <summary>Raised when the user submits input on an inputable toast (before dismiss).</summary>
	public event EventHandler<ToastSubmittedEventArgs>? Submitted;

	/// <summary>Raised once when transitioning to <see cref="ToastHandleState.Dismissed"/>. Not raised for rejected handles.</summary>
	public event EventHandler? Dismissed;

	/// <summary>Last submitted text (if any).</summary>
	public string? SubmittedText { get; private set; }

	/// <summary>Closes the toast if it is visible. Safe no-op when already closed or rejected.</summary>
	public void Dismiss()
	{
		if (_state != ToastHandleState.Visible)
			return;
		_manager?.DismissInternal(this);
	}

	/// <summary>Obsolete alias of <see cref="Dismiss"/>. Does not throw when the toast is not shown.</summary>
	[Obsolete("Use Dismiss(). No longer throws when not shown.")]
	public void Cancel() => Dismiss();

	/// <summary>Dismisses if visible and detaches from the manager. Idempotent.</summary>
	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;
		Dismiss();
		_manager = null;
	}

	internal void MarkVisible() => _state = ToastHandleState.Visible;

	internal void MarkDismissed()
	{
		if (_state == ToastHandleState.Dismissed)
			return;
		_state = ToastHandleState.Dismissed;
		try { Dismissed?.Invoke(this, EventArgs.Empty); } catch { /* host errors ignored */ }
		_dismissedTcs.TrySetResult(true);
	}

	internal void RaiseClicked()
	{
		if (_state != ToastHandleState.Visible)
			return;
		try
		{
			Clicked?.Invoke(this, new ToastInteractionEventArgs(this));
		}
		catch
		{
			/* host errors ignored */
		}
	}

	internal void RaiseHovered()
	{
		if (_state != ToastHandleState.Visible)
			return;
		try
		{
			Hovered?.Invoke(this, new ToastInteractionEventArgs(this));
		}
		catch
		{
			/* host errors ignored */
		}
	}

	internal void RaiseSubmitted(string inputText)
	{
		if (_state != ToastHandleState.Visible)
			return;

		SubmittedText = inputText ?? string.Empty;
		try
		{
			Submitted?.Invoke(this, new ToastSubmittedEventArgs(this, SubmittedText));
		}
		catch
		{
			/* host errors ignored */
		}
	}
}
