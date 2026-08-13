namespace FuzzyToast;

/// <summary>Lifecycle state of a <see cref="ToastHandle"/>.</summary>
public enum ToastHandleState
{
	/// <summary>The toast is on screen (or being shown).</summary>
	Visible,

	/// <summary>The toast has been closed (timer, user, <see cref="ToastHandle.Dismiss"/>, or capacity victim).</summary>
	Dismissed,

	/// <summary>
	/// Never shown — rejected by <see cref="ToastOverflowPolicy.DropNewest"/>.
	/// <see cref="ToastHandle.WhenDismissed"/> is already completed.
	/// </summary>
	RejectedCapacity
}
