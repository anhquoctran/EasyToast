using FuzzyToast.Layout;

namespace FuzzyToast;

/// <summary>Manager-level defaults for capacity, layout, duration, and UX.</summary>
public sealed class ToastManagerOptions
{
	/// <summary>Maximum visible toasts across all corners (default 6).</summary>
	public int MaxToasts { get; init; } = 6;

	/// <summary>Maximum visible toasts in a single corner (default 3).</summary>
	public int MaxToastsPerPosition { get; init; } = 3;

	/// <summary>What happens when <see cref="MaxToasts"/> or <see cref="MaxToastsPerPosition"/> is reached.</summary>
	public ToastOverflowPolicy OverflowPolicy { get; init; } = ToastOverflowPolicy.DropNewest;

	/// <summary>Auto-dismiss for <see cref="Duration.Short"/> when <see cref="ToastOptions.DurationMs"/> is unset (default 2000).</summary>
	public int ShortDurationMs { get; init; } = 2000;

	/// <summary>Auto-dismiss for <see cref="Duration.Long"/> when <see cref="ToastOptions.DurationMs"/> is unset (default 3000).</summary>
	public int LongDurationMs { get; init; } = 3000;

	/// <summary>
	/// Default wait while an inputable toast is open (user typing).
	/// Default <b>5 minutes</b>. Use <c>DurationMs = 0</c> on options for no auto-dismiss.
	/// </summary>
	public int InputDurationMs { get; init; } = 300_000;

	/// <summary>Extra height for the input row only (used if <see cref="InputToastHeight"/> is not set).</summary>
	public int InputExtraHeight { get; init; } = 36;

	/// <summary>
	/// Total height for inputable toasts (parent padding + caption + description + input).
	/// Includes room for contentShell inset so children are not flush to the border.
	/// </summary>
	public int InputToastHeight { get; init; } = 132;

	/// <summary>Inset from the working-area left/right edge, in 96-DPI pixels.</summary>
	public int HorizontalMargin { get; init; } = ToastLayoutMetrics.Default.HorizontalMargin;

	/// <summary>Inset from the working-area top/bottom edge, in 96-DPI pixels.</summary>
	public int VerticalMargin { get; init; } = ToastLayoutMetrics.Default.VerticalMargin;

	/// <summary>Toast width at 96 DPI (scaled at show time).</summary>
	public int ToastWidth { get; init; } = ToastLayoutMetrics.Default.ToastWidth;

	/// <summary>Toast height at 96 DPI for non-input toasts.</summary>
	public int ToastHeight { get; init; } = ToastLayoutMetrics.Default.ToastHeight;

	/// <summary>Gap between stacked toasts at 96 DPI.</summary>
	public int StackGap { get; init; } = ToastLayoutMetrics.Default.StackGap;

	/// <summary>When <see langword="true"/>, hover pauses the auto-dismiss countdown (non-input toasts).</summary>
	public bool PauseOnHover { get; init; } = true;

	/// <summary>When <see langword="true"/>, play the built-in sound unless the toast is muted.</summary>
	public bool PlaySound { get; init; } = true;

	/// <summary>When <see langword="true"/>, collapse the thumbnail column if no image is set.</summary>
	public bool HideImagePanelWhenEmpty { get; init; } = true;

	/// <summary>
	/// When true, enables Dark Mode detection and automatic theme switching on Windows 10/11.
	/// The toast will follow the system app mode setting.
	/// </summary>
	public bool EnableDarkModeDetection { get; init; } = true;

	/// <summary>
	/// Gets or sets the default group policy for toast grouping.
	/// When enabled, toasts with the same GroupId are grouped together in the stack.
	/// </summary>
	public bool EnableGrouping { get; init; } = false;

	/// <summary>
	/// Maximum number of toasts per group when grouping is enabled.
	/// Excess toasts in a group are collapsed into a "+N more" indicator.
	/// </summary>
	public int MaxToastsPerGroup { get; init; } = 5;

	internal ToastLayoutMetrics ToLayoutMetrics(bool inputable = false)
	{
		// Inputable: use dedicated compact height (not base+extra which left empty space).
		var height = inputable
			? Math.Max(100, InputToastHeight)
			: ToastHeight;
		return new ToastLayoutMetrics
		{
			ToastWidth = ToastWidth,
			ToastHeight = height,
			HorizontalMargin = HorizontalMargin,
			VerticalMargin = VerticalMargin,
			StackGap = StackGap,
			MinTouchTargetPx = ToastLayoutMetrics.Default.MinTouchTargetPx,
			CloseButtonSize = ToastLayoutMetrics.Default.CloseButtonSize,
			ThumbnailSize = ToastLayoutMetrics.Default.ThumbnailSize,
			ContentPaddingLeft = ToastLayoutMetrics.Default.ContentPaddingLeft,
			ContentPaddingRight = ToastLayoutMetrics.Default.ContentPaddingRight,
			ContentPaddingTop = ToastLayoutMetrics.Default.ContentPaddingTop,
			ContentPaddingBottom = ToastLayoutMetrics.Default.ContentPaddingBottom,
			CaptionDescriptionGap = ToastLayoutMetrics.Default.CaptionDescriptionGap,
			CaptionMinHeight = ToastLayoutMetrics.Default.CaptionMinHeight,
			DescriptionMinHeight = ToastLayoutMetrics.Default.DescriptionMinHeight
		};
	}

	/// <summary>
	/// Resolve auto-dismiss duration for the given options.
	/// Returns <c>0</c> when the toast should stay open until user action.
	/// </summary>
	internal int ResolveDurationMs(ToastOptions options)
	{
		// Explicit 0 => no auto-dismiss (inputable toasts often want this).
		if (options.DurationMs is 0)
			return 0;

		if (options.DurationMs is int explicitMs && explicitMs > 0)
			return explicitMs;

		if (options.EnableInput || options.Duration is Duration.Input)
			return Math.Max(0, InputDurationMs);

		return options.Duration switch
		{
			Duration.Long => LongDurationMs,
			Duration.Input => Math.Max(0, InputDurationMs),
			_ => ShortDurationMs
		};
	}
}
