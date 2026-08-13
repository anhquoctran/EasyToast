namespace FuzzyToast;

/// <summary>
/// How the toast appears and leaves the screen.
/// Android-style aliases <see cref="FADE"/> and <see cref="SLIDE"/> match FuzzyToast 1.x.
/// </summary>
public enum Animation
{
	/// <summary>Slides in from the screen edge and slides out on dismiss.</summary>
	Slide = 0,

	/// <summary>Fades opacity in and out (default).</summary>
	Fade = 1,

	/// <summary>Alias for <see cref="Slide"/>.</summary>
	SLIDE = Slide,

	/// <summary>Alias for <see cref="Fade"/>.</summary>
	FADE = Fade
}
