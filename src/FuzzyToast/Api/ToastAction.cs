namespace FuzzyToast;

/// <summary>
/// Represents an interactive action button that can be displayed on a toast.
/// Actions allow users to perform quick operations without opening the application.
/// </summary>
public sealed class ToastAction
{
	/// <summary>
	/// Creates a new toast action with the specified label and callback.
	/// </summary>
	/// <param name="label">The text displayed on the action button.</param>
	/// <param name="onClick">Callback invoked when the user clicks the action.</param>
	public ToastAction(string label, Action<ToastHandle>? onClick = null)
	{
		Label = label ?? throw new ArgumentNullException(nameof(label));
		OnClick = onClick;
	}

	/// <summary>
	/// The text displayed on the action button.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Optional callback invoked when the user clicks this action.
	/// The ToastHandle provides access to toast metadata and state.
	/// </summary>
	public Action<ToastHandle>? OnClick { get; }

	/// <summary>
	/// Optional icon or image to display alongside the action label.
	/// </summary>
	public Image? Icon { get; set; }

	/// <summary>
	/// Gets or sets whether this action is the default (triggered by Enter key).
	/// Only one action per toast should be marked as default.
	/// </summary>
	public bool IsDefault { get; set; }

	/// <summary>
	/// Gets or sets the style of the action button.
	/// </summary>
	public ToastActionStyle Style { get; set; } = ToastActionStyle.Normal;

	/// <summary>
	/// Gets or sets custom data associated with this action.
	/// Available in the OnClick callback via ToastHandle.
	/// </summary>
	public object? Tag { get; set; }
}

/// <summary>
/// Defines the visual style of a toast action button.
/// </summary>
public enum ToastActionStyle
{
	/// <summary>
	/// Normal action with standard appearance.
	/// </summary>
	Normal,

	/// <summary>
	/// Destructive action (e.g., Delete, Remove) typically shown in red.
	/// </summary>
	Destructive,

	/// <summary>
	/// Affirmative action (e.g., OK, Confirm) typically highlighted.
	/// </summary>
	Affirmative
}
