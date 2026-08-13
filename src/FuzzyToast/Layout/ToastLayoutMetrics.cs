namespace FuzzyToast.Layout;

/// <summary>
/// Layout metrics for toast sizing and stacking (96 DPI baseline).
/// Compact defaults — avoid large empty regions in both normal and inputable toasts.
/// </summary>
public sealed class ToastLayoutMetrics
{
	/// <summary>Outer toast width at 96 DPI.</summary>
	public required int ToastWidth { get; init; }

	/// <summary>Outer toast height at 96 DPI.</summary>
	public required int ToastHeight { get; init; }

	/// <summary>Inset from the working-area left/right edge.</summary>
	public required int HorizontalMargin { get; init; }

	/// <summary>Inset from the working-area top/bottom edge.</summary>
	public required int VerticalMargin { get; init; }

	/// <summary>Minimum interactive target (close / submit).</summary>
	public int MinTouchTargetPx { get; init; } = 36;

	/// <summary>Close-button hit target size (pixels at 96 DPI).</summary>
	public int CloseButtonSize { get; init; } = 36;

	/// <summary>Thumbnail box size (pixels at 96 DPI).</summary>
	public int ThumbnailSize { get; init; } = 80;

	/// <summary>Inner padding left of the content.</summary>
	public int ContentPaddingLeft { get; init; } = 14;

	/// <summary>Inner padding right of the content.</summary>
	public int ContentPaddingRight { get; init; } = 12;

	/// <summary>Inner padding above the content.</summary>
	public int ContentPaddingTop { get; init; } = 10;

	/// <summary>Inner padding below the content.</summary>
	public int ContentPaddingBottom { get; init; } = 8;

	/// <summary>Vertical gap between caption and description.</summary>
	public int CaptionDescriptionGap { get; init; } = 4;

	/// <summary>Minimum caption band height.</summary>
	public int CaptionMinHeight { get; init; } = 28;

	/// <summary>Minimum description band height.</summary>
	public int DescriptionMinHeight { get; init; } = 28;

	/// <summary>Gap between stacked toasts.</summary>
	public int StackGap { get; init; } = 10;

	/// <summary>
	/// Compact default: ~380×96 — caption + one description line, touchable close.
	/// Inputable adds <see cref="ToastManagerOptions.InputExtraHeight"/> on top.
	/// </summary>
	public static ToastLayoutMetrics Default { get; } = new()
	{
		// Outer size includes contentShell parent padding (~12/10 each side).
		ToastWidth = 380,
		ToastHeight = 100,
		HorizontalMargin = 12,
		VerticalMargin = 10,
		MinTouchTargetPx = 36,
		CloseButtonSize = 36,
		ThumbnailSize = 80,
		ContentPaddingLeft = 12,
		ContentPaddingRight = 12,
		ContentPaddingTop = 10,
		ContentPaddingBottom = 10,
		CaptionDescriptionGap = 4,
		CaptionMinHeight = 28,
		DescriptionMinHeight = 28,
		StackGap = 10
	};

	/// <summary>Distance between successive toast origins in a stack.</summary>
	public int EffectiveStackStride =>
		ToastHeight + (StackGap > 0 ? StackGap : VerticalMargin);
}
