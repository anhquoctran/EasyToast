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
	public int HorizontalMargin { get; init; } = ToastLayoutMetrics.Default.HorizontalMargin;
	public int VerticalMargin { get; init; } = ToastLayoutMetrics.Default.VerticalMargin;
	public int ToastWidth { get; init; } = ToastLayoutMetrics.Default.ToastWidth;
	public int ToastHeight { get; init; } = ToastLayoutMetrics.Default.ToastHeight;
	public int StackGap { get; init; } = ToastLayoutMetrics.Default.StackGap;
	public bool PauseOnHover { get; init; } = true;
	public bool PlaySound { get; init; } = true;
	public bool HideImagePanelWhenEmpty { get; init; } = true;

	internal ToastLayoutMetrics ToLayoutMetrics() => new()
	{
		ToastWidth = ToastWidth,
		ToastHeight = ToastHeight,
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

	internal int ResolveDurationMs(Duration duration) => duration switch
	{
		Duration.Long => LongDurationMs,
		_ => ShortDurationMs
	};
}
