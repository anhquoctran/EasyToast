namespace FuzzyToast;

/// <summary>Live runtime handle for a toast (or a rejected show attempt).</summary>
public sealed class ToastHandle : IDisposable
{
	private readonly TaskCompletionSource _dismissedTcs =
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
			_dismissedTcs.TrySetResult();
	}

	public string Id { get; }
	public ToastOptions Options { get; }
	public ToastHandleState State => _state;

	public bool IsVisible => _state == ToastHandleState.Visible;
	public bool IsDismissed => _state == ToastHandleState.Dismissed;
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

	public event EventHandler? Dismissed;

	/// <summary>Last submitted text (if any).</summary>
	public string? SubmittedText { get; private set; }

	public void Dismiss()
	{
		if (_state != ToastHandleState.Visible)
			return;
		_manager?.DismissInternal(this);
	}

	[Obsolete("Use Dismiss(). No longer throws when not shown.")]
	public void Cancel() => Dismiss();

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
		_dismissedTcs.TrySetResult();
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
