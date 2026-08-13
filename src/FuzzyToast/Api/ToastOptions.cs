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
		if ((Caption ?? string.Empty).Length > ToastLimits.MaxCaptionLength)
			throw new ArgumentException($"Caption must be <= {ToastLimits.MaxCaptionLength} characters.", nameof(Caption));
		if ((Description ?? string.Empty).Length > ToastLimits.MaxDescriptionLength)
			throw new ArgumentException($"Description must be <= {ToastLimits.MaxDescriptionLength} characters.", nameof(Description));
		if (Theme == ToastTheme.Custom && CustomColors is null)
			throw new ArgumentException("CustomColors required when Theme is Custom.", nameof(CustomColors));
		if (DurationMs is < 0)
			throw new ArgumentOutOfRangeException(nameof(DurationMs), "DurationMs must be >= 0 when set (0 = no auto-dismiss).");
		if (DurationMs > ToastLimits.MaxDurationMs)
			throw new ArgumentOutOfRangeException(nameof(DurationMs), $"DurationMs must be <= {ToastLimits.MaxDurationMs}.");
		if (EnableInput && string.IsNullOrWhiteSpace(SubmitButtonText))
			throw new ArgumentException("SubmitButtonText is required when EnableInput is true.", nameof(SubmitButtonText));
		if ((SubmitButtonText ?? string.Empty).Length > ToastLimits.MaxSubmitButtonTextLength)
			throw new ArgumentException($"SubmitButtonText must be <= {ToastLimits.MaxSubmitButtonTextLength} characters.", nameof(SubmitButtonText));
		if ((InputPlaceholder ?? string.Empty).Length > ToastLimits.MaxInputTextLength)
			throw new ArgumentException($"InputPlaceholder must be <= {ToastLimits.MaxInputTextLength} characters.", nameof(InputPlaceholder));
		if ((InputDefaultText ?? string.Empty).Length > ToastLimits.MaxInputTextLength)
			throw new ArgumentException($"InputDefaultText must be <= {ToastLimits.MaxInputTextLength} characters.", nameof(InputDefaultText));
		if (Thumbnail is not null
			&& !ImageValidation.ValidateImageSize(
				Thumbnail,
				ToastLimits.MinImageDimension,
				ToastLimits.MinImageDimension,
				ToastLimits.MaxImageDimension,
				ToastLimits.MaxImageDimension))
		{
			throw new ArgumentException(
				$"Thumbnail dimensions must be between {ToastLimits.MinImageDimension} and {ToastLimits.MaxImageDimension} px.",
				nameof(Thumbnail));
		}

		if (Metadata.Count > ToastLimits.MaxMetadataEntries)
			throw new ArgumentException($"Metadata cannot exceed {ToastLimits.MaxMetadataEntries} entries.", nameof(Metadata));
		foreach (var pair in Metadata)
		{
			if (pair.Key.Length > ToastLimits.MaxMetadataKeyLength)
				throw new ArgumentException($"Metadata key must be <= {ToastLimits.MaxMetadataKeyLength} characters.", nameof(Metadata));
		}
	}

	public static IReadOnlyDictionary<string, object?> FreezeMetadata(
		IEnumerable<KeyValuePair<string, object?>>? entries)
	{
		if (entries is null)
			return EmptyMetadata;

		var map = new Dictionary<string, object?>(StringComparer.Ordinal);
		foreach (var pair in entries)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			if (pair.Key.Length > ToastLimits.MaxMetadataKeyLength)
				continue;
			if (map.Count >= ToastLimits.MaxMetadataEntries && !map.ContainsKey(pair.Key))
				break;
			map[pair.Key] = pair.Value;
		}

		return map.Count == 0
			? EmptyMetadata
			: new ReadOnlyDictionary<string, object?>(map);
	}
}
