using System.Collections.ObjectModel;
using System.Drawing;

namespace FuzzyToast;

/// <summary>Immutable configuration for a single toast.</summary>
public sealed class ToastOptions
{
	private static readonly IReadOnlyDictionary<string, object?> EmptyMetadata =
		new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(0));

	public string Caption { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public Duration Duration { get; init; } = Duration.Short;
	public Animation Animation { get; init; } = Animation.Fade;
	public ToastPosition Position { get; init; } = ToastPosition.BottomRight;
	public ToastTheme Theme { get; init; } = ToastTheme.Dark;
	public ColorScheme? CustomColors { get; init; }
	public CloseStyle CloseStyle { get; init; } = CloseStyle.ButtonAndClickEntire;
	public bool IsMuted { get; init; }
	public Image? Thumbnail { get; init; }
	public bool OwnsThumbnail { get; init; }

	/// <summary>Arbitrary user payload (entity id, DTO, etc.). Delivered on click via <see cref="ToastInteractionEventArgs.Tag"/>.</summary>
	public object? Tag { get; init; }

	/// <summary>
	/// Key/value extension metadata. Delivered on click via <see cref="ToastInteractionEventArgs.Metadata"/>.
	/// Stored as a read-only snapshot (copied at build time).
	/// </summary>
	public IReadOnlyDictionary<string, object?> Metadata { get; init; } = EmptyMetadata;

	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(Caption))
			throw new ArgumentException("Caption is required.", nameof(Caption));
		if (Theme == ToastTheme.Custom && CustomColors is null)
			throw new ArgumentException("CustomColors required when Theme is Custom.", nameof(CustomColors));
	}

	/// <summary>Create a frozen metadata dictionary for options (null-safe).</summary>
	public static IReadOnlyDictionary<string, object?> FreezeMetadata(
		IEnumerable<KeyValuePair<string, object?>>? entries)
	{
		if (entries is null)
			return EmptyMetadata;

		var map = new Dictionary<string, object?>(StringComparer.Ordinal);
		foreach (var (key, value) in entries)
		{
			if (string.IsNullOrWhiteSpace(key))
				continue;
			map[key] = value;
		}

		return map.Count == 0
			? EmptyMetadata
			: new ReadOnlyDictionary<string, object?>(map);
	}
}
