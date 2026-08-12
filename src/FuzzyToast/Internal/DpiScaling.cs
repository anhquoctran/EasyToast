using System.Windows.Forms;
using FuzzyToast.Layout;

namespace FuzzyToast.Internal;

/// <summary>
/// DPI helpers for Windows 10/11 Per-Monitor and system DPI (WinForms DeviceDpi).
/// Metrics in options are authored at 96 DPI and scaled at show time.
/// </summary>
internal static class DpiScaling
{
	public const int BaselineDpi = 96;

	public static float GetScale(Control? control)
	{
		try
		{
			var dpi = control is { IsDisposed: false } ? control.DeviceDpi : BaselineDpi;
			if (dpi <= 0)
				dpi = BaselineDpi;
			return dpi / (float)BaselineDpi;
		}
		catch
		{
			return 1f;
		}
	}

	public static float GetScale(int deviceDpi)
	{
		if (deviceDpi <= 0)
			deviceDpi = BaselineDpi;
		return deviceDpi / (float)BaselineDpi;
	}

	public static int Scale(int value, float scale) =>
		Math.Max(1, (int)Math.Round(value * scale, MidpointRounding.AwayFromZero));

	public static ToastLayoutMetrics ScaleMetrics(ToastLayoutMetrics metrics, float scale)
	{
		if (Math.Abs(scale - 1f) < 0.001f)
			return metrics;

		return new ToastLayoutMetrics
		{
			ToastWidth = Scale(metrics.ToastWidth, scale),
			ToastHeight = Scale(metrics.ToastHeight, scale),
			HorizontalMargin = Scale(metrics.HorizontalMargin, scale),
			VerticalMargin = Scale(metrics.VerticalMargin, scale),
			StackGap = Scale(metrics.StackGap, scale),
			MinTouchTargetPx = Scale(metrics.MinTouchTargetPx, scale),
			CloseButtonSize = Scale(metrics.CloseButtonSize, scale),
			ThumbnailSize = Scale(metrics.ThumbnailSize, scale),
			ContentPaddingLeft = Scale(metrics.ContentPaddingLeft, scale),
			ContentPaddingRight = Scale(metrics.ContentPaddingRight, scale),
			ContentPaddingTop = Scale(metrics.ContentPaddingTop, scale),
			ContentPaddingBottom = Scale(metrics.ContentPaddingBottom, scale),
			CaptionDescriptionGap = Scale(metrics.CaptionDescriptionGap, scale),
			CaptionMinHeight = Scale(metrics.CaptionMinHeight, scale),
			DescriptionMinHeight = Scale(metrics.DescriptionMinHeight, scale)
		};
	}
}
