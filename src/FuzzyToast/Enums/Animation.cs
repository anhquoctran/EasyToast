namespace FuzzyToast;

/// <summary>
/// Toast transition animation. Android-era names <see cref="FADE"/> / <see cref="SLIDE"/> are preferred aliases.
/// Numeric values: Slide/SLIDE = 0, Fade/FADE = 1.
/// </summary>
public enum Animation
{
	Slide = 0,
	Fade = 1,

	/// <summary>Alias for <see cref="Slide"/>.</summary>
	SLIDE = Slide,

	/// <summary>Alias for <see cref="Fade"/>.</summary>
	FADE = Fade
}
