namespace FuzzyToast;

/// <summary>
/// Duration presets (Android-style names <see cref="LENGTH_SHORT"/> / <see cref="LENGTH_LONG"/>).
/// </summary>
public enum Duration
{
	/// <summary>Short duration (~2s). Same as <see cref="LENGTH_SHORT"/>.</summary>
	Short = 0,

	/// <summary>Long duration (~3s). Same as <see cref="LENGTH_LONG"/>.</summary>
	Long = 1,

	/// <summary>
	/// Extended wait for inputable toasts (default via <see cref="ToastManagerOptions.InputDurationMs"/>).
	/// Same as <see cref="LENGTH_INPUT"/>.
	/// </summary>
	Input = 2,

	/// <summary>Android-style alias for <see cref="Short"/>.</summary>
	LENGTH_SHORT = Short,

	/// <summary>Android-style alias for <see cref="Long"/>.</summary>
	LENGTH_LONG = Long,

	/// <summary>Alias for <see cref="Input"/> — long wait while the user types.</summary>
	LENGTH_INPUT = Input
}
