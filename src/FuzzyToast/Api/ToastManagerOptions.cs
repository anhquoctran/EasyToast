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

	/// <summary>Default wait while an inputable toast is open (user typing). Default 30 seconds.</summary>
	public int InputDurationMs { get; init; } = 30_000;

	/// <summary>Extra height (at 96 DPI) added for the input row + submit button.</summary>
	public int InputExtraHeight { get; init; } = 56;

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
		var height = ToastHeight + (inputable ? Math.Max(0, InputExtraHeight) : 0);
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

	/// <summary>Resolve auto-dismiss duration for the given options.</summary>
	internal int ResolveDurationMs(ToastOptions options)
	{
		if (options.DurationMs is int explicitMs && explicitMs > 0)
			return explicitMs;

		if (options.EnableInput || options.Duration is Duration.Input)
			return Math.Max(1, InputDurationMs);

		return options.Duration switch
		{
			Duration.Long => LongDurationMs,
			_ => ShortDurationMs
		};
	}
}
