namespace FuzzyToast.Layout;

/// <summary>
/// Layout and touchable UI metrics for toast sizing, stacking, and content spacing.
/// Defaults target 96 DPI; scale later via DPI factor if needed.
/// </summary>
public sealed class ToastLayoutMetrics
{
	public required int ToastWidth { get; init; }
	public required int ToastHeight { get; init; }
	public required int HorizontalMargin { get; init; }
	public required int VerticalMargin { get; init; }

	/// <summary>Minimum interactive target (close button). WCAG / Material-aligned.</summary>
	public int MinTouchTargetPx { get; init; } = 44;

	public int CloseButtonSize { get; init; } = 44;
	public int ThumbnailSize { get; init; } = 96;
	public int ContentPaddingLeft { get; init; } = 16;
	public int ContentPaddingRight { get; init; } = 12;
	public int ContentPaddingTop { get; init; } = 12;
	public int ContentPaddingBottom { get; init; } = 12;

	/// <summary>Vertical gap between caption and description (must not look cramped).</summary>
	public int CaptionDescriptionGap { get; init; } = 8;

	public int CaptionMinHeight { get; init; } = 32;
	public int DescriptionMinHeight { get; init; } = 40;

	/// <summary>Gap between stacked toasts. When ≤ 0, <see cref="VerticalMargin"/> is used.</summary>
	public int StackGap { get; init; } = 12;

	/// <summary>
	/// Default metrics: roomy text, touchable close (44×44), not cramped.
	/// </summary>
	public static ToastLayoutMetrics Default { get; } = new()
	{
		ToastWidth = 420,
		ToastHeight = 140,
		HorizontalMargin = 16,
		VerticalMargin = 12,
		MinTouchTargetPx = 44,
		CloseButtonSize = 44,
		ThumbnailSize = 96,
		ContentPaddingLeft = 16,
		ContentPaddingRight = 12,
		ContentPaddingTop = 12,
		ContentPaddingBottom = 12,
		CaptionDescriptionGap = 8,
		CaptionMinHeight = 32,
		DescriptionMinHeight = 40,
		StackGap = 12
	};

	/// <summary>Distance between successive toast origins in a stack.</summary>
	public int EffectiveStackStride =>
		ToastHeight + (StackGap > 0 ? StackGap : VerticalMargin);
}
