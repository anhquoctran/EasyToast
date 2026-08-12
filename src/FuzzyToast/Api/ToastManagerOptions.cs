using FuzzyToast.Layout;

namespace FuzzyToast;

/// <summary>Manager-level defaults for capacity, layout, duration, and UX.</summary>
public sealed class ToastManagerOptions
{
	public int MaxToasts { get; init; } = 6;
	public int MaxToastsPerPosition { get; init; } = 3;
	public ToastOverflowPolicy OverflowPolicy { get; init; } = ToastOverflowPolicy.DropNewest;
	public int ShortDurationMs { get; init; } = 2000;
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

	public int HorizontalMargin { get; init; } = ToastLayoutMetrics.Default.HorizontalMargin;
	public int VerticalMargin { get; init; } = ToastLayoutMetrics.Default.VerticalMargin;
	public int ToastWidth { get; init; } = ToastLayoutMetrics.Default.ToastWidth;
	public int ToastHeight { get; init; } = ToastLayoutMetrics.Default.ToastHeight;
	public int StackGap { get; init; } = ToastLayoutMetrics.Default.StackGap;
	public bool PauseOnHover { get; init; } = true;
	public bool PlaySound { get; init; } = true;
	public bool HideImagePanelWhenEmpty { get; init; } = true;

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
