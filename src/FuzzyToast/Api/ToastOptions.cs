using System.Collections.ObjectModel;
using System.Drawing;

namespace FuzzyToast;

/// <summary>Immutable configuration for a single toast.</summary>
public sealed class ToastOptions
{
	private static readonly IReadOnlyDictionary<string, object?> EmptyMetadata =
		new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(0));

	/// <summary>Title line. Required (non-whitespace) and at most <see cref="ToastLimits.MaxCaptionLength"/> characters.</summary>
	public string Caption { get; init; } = string.Empty;

	/// <summary>Optional secondary line, at most <see cref="ToastLimits.MaxDescriptionLength"/> characters.</summary>
	public string Description { get; init; } = string.Empty;

	/// <summary>Preset auto-dismiss. Ignored when <see cref="DurationMs"/> is set.</summary>
	public Duration Duration { get; init; } = Duration.Short;

	/// <summary>Appear / dismiss animation.</summary>
	public Animation Animation { get; init; } = Animation.Fade;

	/// <summary>Corner stack for this toast.</summary>
	public ToastPosition Position { get; init; } = ToastPosition.BottomRight;

	/// <summary>Built-in theme. <see cref="ToastTheme.Custom"/> requires <see cref="CustomColors"/>.</summary>
	public ToastTheme Theme { get; init; } = ToastTheme.Dark;

	/// <summary>Palette used when <see cref="Theme"/> is <see cref="ToastTheme.Custom"/>.</summary>
	public ColorScheme? CustomColors { get; init; }

	/// <summary>How the user can dismiss the toast.</summary>
	public CloseStyle CloseStyle { get; init; } = CloseStyle.ButtonAndClickEntire;

	/// <summary>When <see langword="true"/>, no notification sound is played.</summary>
	public bool IsMuted { get; init; }

	/// <summary>Optional left thumbnail. Dimensions must stay within <see cref="ToastLimits"/>.</summary>
	public Image? Thumbnail { get; init; }

	/// <summary>When <see langword="true"/>, the toast disposes <see cref="Thumbnail"/> after close.</summary>
	public bool OwnsThumbnail { get; init; }

	/// <summary>Arbitrary user payload (entity id, DTO, etc.).</summary>
	public object? Tag { get; init; }

	/// <summary>Key/value extension metadata (snapshot).</summary>
	public IReadOnlyDictionary<string, object?> Metadata { get; init; } = EmptyMetadata;

	/// <summary>When true, displays a progress bar that slides to 0 over the duration.</summary>
	public bool ShowProgressBar { get; init; }

	/// <summary>
	/// Optional group identifier for toast grouping and queue management.
	/// Toasts with the same GroupId can be grouped together or managed as a batch.
	/// </summary>
	public string? GroupId { get; init; }

	/// <summary>
	/// Optional list of interactive actions displayed as buttons on the toast.
	/// Actions allow users to perform quick operations without opening the app.
	/// </summary>
	public IReadOnlyList<ToastAction>? Actions { get; init; }

	/// <summary>
	/// Optional custom content to display in the toast instead of standard caption/description.
	/// Use this for rich media, markdown rendering, or custom WinForms controls.
	/// </summary>
	public ToastContent? CustomContent { get; init; }

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

	/// <summary>
	/// Throws if caption, theme, duration, input fields, thumbnail, or metadata violate
	/// <see cref="ToastLimits"/> or other invariants. Called automatically by <see cref="ToastManager.Show"/>.
	/// </summary>
	/// <exception cref="ArgumentException">A required field is missing or a string/image exceeds limits.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><see cref="DurationMs"/> is negative or above the max.</exception>
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

	/// <summary>
	/// Copies <paramref name="entries"/> into an immutable dictionary.
	/// Blank keys are skipped; oversize keys are skipped; extra entries beyond
	/// <see cref="ToastLimits.MaxMetadataEntries"/> are dropped.
	/// </summary>
	/// <param name="entries">Source pairs; <see langword="null"/> yields an empty snapshot.</param>
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
