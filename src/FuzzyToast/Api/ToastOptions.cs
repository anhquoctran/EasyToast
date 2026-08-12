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

	/// <summary>Arbitrary user payload (entity id, DTO, etc.).</summary>
	public object? Tag { get; init; }

	/// <summary>Key/value extension metadata (snapshot).</summary>
	public IReadOnlyDictionary<string, object?> Metadata { get; init; } = EmptyMetadata;

	// --- Inputable toast (v3) ---

	/// <summary>When true, toast shows a text box + submit button for quick input.</summary>
	public bool EnableInput { get; init; }

	/// <summary>Placeholder text inside the input box.</summary>
	public string InputPlaceholder { get; init; } = string.Empty;

	/// <summary>Initial text in the input box.</summary>
	public string InputDefaultText { get; init; } = string.Empty;

	/// <summary>Label for the submit button (default "OK").</summary>
	public string SubmitButtonText { get; init; } = "OK";

	/// <summary>If false, empty input cannot be submitted.</summary>
	public bool AllowEmptySubmit { get; init; }

	/// <summary>
	/// Optional absolute duration in milliseconds (overrides <see cref="Duration"/> presets).
	/// <list type="bullet">
	/// <item><c>null</c> — use preset (<see cref="Duration"/> / <see cref="ToastManagerOptions.InputDurationMs"/> for input)</item>
	/// <item><c>0</c> — no auto-dismiss (stay until Submit / Esc / close)</item>
	/// <item><c>&gt; 0</c> — dismiss after this many ms</item>
	/// </list>
	/// </summary>
	public int? DurationMs { get; init; }

	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(Caption))
			throw new ArgumentException("Caption is required.", nameof(Caption));
		if (Theme == ToastTheme.Custom && CustomColors is null)
			throw new ArgumentException("CustomColors required when Theme is Custom.", nameof(CustomColors));
		if (DurationMs is < 0)
			throw new ArgumentOutOfRangeException(nameof(DurationMs), "DurationMs must be >= 0 when set (0 = no auto-dismiss).");
		if (EnableInput && string.IsNullOrWhiteSpace(SubmitButtonText))
			throw new ArgumentException("SubmitButtonText is required when EnableInput is true.", nameof(SubmitButtonText));
	}

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
