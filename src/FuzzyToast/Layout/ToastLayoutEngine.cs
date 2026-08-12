using System.Drawing;

namespace FuzzyToast.Layout;

/// <summary>
/// Pure toast positioning. Uses per-position <paramref name="stackIndex"/> (0 = oldest at corner).
/// Never uses a global toast count for a single corner (v1 BottomRight bug).
/// </summary>
public static class ToastLayoutEngine
{
	/// <summary>
	/// Compute top-left screen coordinates for a toast in a stack.
	/// </summary>
	/// <param name="position">Corner stack.</param>
	/// <param name="stackIndex">0 = oldest at the anchor corner; larger = further from corner.</param>
	/// <param name="metrics">Size and margins.</param>
	/// <param name="area">Working area for the target screen.</param>
	public static Point ComputeLocation(
		ToastPosition position,
		int stackIndex,
		ToastLayoutMetrics metrics,
		ScreenWorkingArea area)
	{
		ArgumentNullException.ThrowIfNull(metrics);
		if (stackIndex < 0)
			throw new ArgumentOutOfRangeException(nameof(stackIndex));

		var h = metrics.HorizontalMargin;
		var v = metrics.VerticalMargin;
		var w = metrics.ToastWidth;
		var th = metrics.ToastHeight;
		var stride = metrics.EffectiveStackStride;

		return position switch
		{
			ToastPosition.TopLeft => new Point(
				area.Left + h,
				area.Top + v + stackIndex * stride),
			ToastPosition.TopRight => new Point(
				area.Right - w - h,
				area.Top + v + stackIndex * stride),
			ToastPosition.BottomLeft => new Point(
				area.Left + h,
				area.Bottom - th - v - stackIndex * stride),
			ToastPosition.BottomRight => new Point(
				area.Right - w - h,
				area.Bottom - th - v - stackIndex * stride),
			_ => throw new ArgumentOutOfRangeException(nameof(position))
		};
	}

	/// <summary>Locations for a full stack of <paramref name="count"/> toasts at one corner.</summary>
	public static IReadOnlyList<Point> ComputeStack(
		ToastPosition position,
		int count,
		ToastLayoutMetrics metrics,
		ScreenWorkingArea area)
	{
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count));

		var list = new Point[count];
		for (var i = 0; i < count; i++)
			list[i] = ComputeLocation(position, i, metrics, area);
		return list;
	}
}
