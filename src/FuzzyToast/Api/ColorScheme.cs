using System.Drawing;

namespace FuzzyToast;

/// <summary>
/// Immutable background/foreground pair. Channels are always R,G,B in standard order.
/// </summary>
public sealed class ColorScheme : IEquatable<ColorScheme>
{
	/// <summary>Background fill of the toast card.</summary>
	public Color Background { get; }

	/// <summary>Caption / close-button foreground color.</summary>
	public Color Foreground { get; }

	/// <param name="background">Card background.</param>
	/// <param name="foreground">Text and glyph color.</param>
	public ColorScheme(Color background, Color foreground)
	{
		Background = background;
		Foreground = foreground;
	}

	/// <summary>Builds a scheme from raw RGB channels (background then foreground).</summary>
	/// <param name="rBg">Background red (0–255).</param>
	/// <param name="gBg">Background green (0–255).</param>
	/// <param name="bBg">Background blue (0–255).</param>
	/// <param name="rFg">Foreground red (0–255).</param>
	/// <param name="gFg">Foreground green (0–255).</param>
	/// <param name="bFg">Foreground blue (0–255).</param>
	public ColorScheme(byte rBg, byte gBg, byte bBg, byte rFg, byte gFg, byte bFg)
		: this(Color.FromArgb(rBg, gBg, bBg), Color.FromArgb(rFg, gFg, bFg))
	{
	}

	/// <inheritdoc />
	public bool Equals(ColorScheme? other) =>
		other is not null
		&& Background.ToArgb() == other.Background.ToArgb()
		&& Foreground.ToArgb() == other.Foreground.ToArgb();

	/// <inheritdoc />
	public override bool Equals(object? obj) => Equals(obj as ColorScheme);

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (Background.ToArgb() * 397) ^ Foreground.ToArgb();
		}
	}
}

/// <summary>Resolves built-in <see cref="ToastTheme"/> values to RGB <see cref="ColorScheme"/> instances.</summary>
public static class ThemeCatalog
{
	/// <summary>
	/// Returns the palette for <paramref name="theme"/>.
	/// When <paramref name="theme"/> is <see cref="ToastTheme.Custom"/>, <paramref name="custom"/> is required.
	/// </summary>
	/// <param name="theme">Built-in or custom theme.</param>
	/// <param name="custom">Required when <paramref name="theme"/> is <see cref="ToastTheme.Custom"/>.</param>
	/// <exception cref="InvalidOperationException">Custom theme without a scheme.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Unknown theme value.</exception>
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

/// <summary>Built-in visual themes. Use <see cref="Custom"/> with <see cref="ColorScheme"/> for your own colors.</summary>
public enum ToastTheme
{
	/// <summary>Dark grey card, white text (default).</summary>
	Dark,

	/// <summary>White card, dark text.</summary>
	Light,

	/// <summary>Material blue background, white text.</summary>
	PrimaryLight,

	/// <summary>Green background, white text.</summary>
	SuccessLight,

	/// <summary>Orange background, white text.</summary>
	WarningLight,

	/// <summary>Red background, white text.</summary>
	ErrorLight,

	/// <summary>Dark card, blue accent text.</summary>
	PrimaryDark,

	/// <summary>Dark card, green accent text.</summary>
	SuccessDark,

	/// <summary>Dark card, orange accent text.</summary>
	WarningDark,

	/// <summary>Dark card, red accent text.</summary>
	ErrorDark,

	/// <summary>Caller-supplied <see cref="ColorScheme"/> via <c>SetCustomColors</c>.</summary>
	Custom
}
