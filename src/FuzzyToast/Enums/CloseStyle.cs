namespace FuzzyToast;

/// <summary>
/// How the user can dismiss a visible toast.
/// Inputable toasts always keep a close button so typed text is not lost by an accidental body click.
/// </summary>
public enum CloseStyle
{
	/// <summary>Click anywhere on the toast body to dismiss. Close button is hidden (except in input mode).</summary>
	ClickEntire,

	/// <summary>Only the ✕ button dismisses the toast. Body clicks raise <see cref="Toast.OnClick"/> without closing.</summary>
	Button,

	/// <summary>Either the ✕ button or a body click dismisses (default).</summary>
	ButtonAndClickEntire
}
