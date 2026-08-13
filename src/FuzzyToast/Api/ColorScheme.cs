using System.Drawing;

namespace FuzzyToast;

/// <summary>
/// Immutable background/foreground pair. Channels are always R,G,B in standard order.
/// </summary>
public sealed class ColorScheme : IEquatable<ColorScheme>
{
	public Color Background { get; }
	public Color Foreground { get; }

	public ColorScheme(Color background, Color foreground)
	{
		Background = background;
		Foreground = foreground;
	}

	/// <summary>Standard channel order: r, g, b for background then foreground.</summary>
	public ColorScheme(byte rBg, byte gBg, byte bBg, byte rFg, byte gFg, byte bFg)
		: this(Color.FromArgb(rBg, gBg, bBg), Color.FromArgb(rFg, gFg, bFg))
	{
	}

	public bool Equals(ColorScheme? other) =>
		other is not null
		&& Background.ToArgb() == other.Background.ToArgb()
		&& Foreground.ToArgb() == other.Foreground.ToArgb();

	public override bool Equals(object? obj) => Equals(obj as ColorScheme);

	public override int GetHashCode()
	{
		unchecked
		{
			return (Background.ToArgb() * 397) ^ Foreground.ToArgb();
		}
	}
}

/// <summary>Built-in toast color schemes (true RGB).</summary>
public static class ThemeCatalog
{
	public static ColorScheme Resolve(ToastTheme theme, ColorScheme? custom = null) => theme switch
	{
		ToastTheme.Dark => new ColorScheme(33, 33, 33, 255, 255, 255),
		ToastTheme.Light => new ColorScheme(255, 255, 255, 33, 33, 33),
		ToastTheme.PrimaryLight => new ColorScheme(33, 150, 243, 255, 255, 255),
		ToastTheme.SuccessLight => new ColorScheme(76, 175, 80, 255, 255, 255),
		ToastTheme.WarningLight => new ColorScheme(255, 152, 0, 255, 255, 255),
		ToastTheme.ErrorLight => new ColorScheme(213, 0, 0, 255, 255, 255),
		ToastTheme.PrimaryDark => new ColorScheme(33, 33, 33, 33, 150, 243),
		ToastTheme.SuccessDark => new ColorScheme(33, 33, 33, 76, 175, 80),
		ToastTheme.WarningDark => new ColorScheme(33, 33, 33, 255, 152, 0),
		ToastTheme.ErrorDark => new ColorScheme(33, 33, 33, 213, 0, 0),
		ToastTheme.Custom => custom
			?? throw new InvalidOperationException(
				"ToastTheme.Custom requires a ColorScheme. Use custom colors on the builder."),
		_ => throw new ArgumentOutOfRangeException(nameof(theme))
	};
}

/// <summary>Toast visual theme (v2 naming).</summary>
public enum ToastTheme
{
	Dark,
	Light,
	PrimaryLight,
	SuccessLight,
	WarningLight,
	ErrorLight,
	PrimaryDark,
	SuccessDark,
	WarningDark,
	ErrorDark,
	Custom
}
