namespace FuzzyToast;

/// <summary>
/// Screen corner used as the stack anchor. Each corner has its own stack and capacity count.
/// </summary>
public enum ToastPosition
{
	/// <summary>Top-left of the working area; newer toasts stack downward.</summary>
	TopLeft,

	/// <summary>Top-right of the working area; newer toasts stack downward.</summary>
	TopRight,

	/// <summary>Bottom-left of the working area; newer toasts stack upward.</summary>
	BottomLeft,

	/// <summary>Bottom-right of the working area (default); newer toasts stack upward.</summary>
	BottomRight
}
