namespace FuzzyToast;

/// <summary>
/// Behavior when the manager cannot accept another toast under capacity rules.
/// </summary>
public enum ToastOverflowPolicy
{
	/// <summary>Reject the new toast (default).</summary>
	DropNewest,

	/// <summary>Dismiss the policy-selected victim, then show the new toast.</summary>
	DropOldest,

	/// <summary>Throw <see cref="InvalidOperationException"/>.</summary>
	Throw
}
