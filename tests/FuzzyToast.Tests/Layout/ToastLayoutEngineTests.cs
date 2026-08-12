using System.Drawing;
using FuzzyToast.Layout;
using Xunit;

namespace FuzzyToast.Tests;

public class ToastLayoutEngineTests
{
	private static readonly ScreenWorkingArea Area = new(0, 0, 1920, 1080);
	private static readonly ToastLayoutMetrics Metrics = ToastLayoutMetrics.Default;

	[Fact]
	public void T06_BottomRight_Index0_AtBottomRightAnchor()
	{
		var p = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomRight, 0, Metrics, Area);

		Assert.Equal(Area.Right - Metrics.ToastWidth - Metrics.HorizontalMargin, p.X);
		Assert.Equal(Area.Bottom - Metrics.ToastHeight - Metrics.VerticalMargin, p.Y);
	}

	[Fact]
	public void T07_BottomRight_Index1_OneStrideUp()
	{
		var p0 = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomRight, 0, Metrics, Area);
		var p1 = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomRight, 1, Metrics, Area);

		Assert.Equal(p0.X, p1.X);
		Assert.Equal(p0.Y - Metrics.EffectiveStackStride, p1.Y);
	}

	[Fact]
	public void T08_TopRight_GrowsDownward()
	{
		var p0 = ToastLayoutEngine.ComputeLocation(ToastPosition.TopRight, 0, Metrics, Area);
		var p1 = ToastLayoutEngine.ComputeLocation(ToastPosition.TopRight, 1, Metrics, Area);

		Assert.Equal(Area.Right - Metrics.ToastWidth - Metrics.HorizontalMargin, p0.X);
		Assert.Equal(Area.Top + Metrics.VerticalMargin, p0.Y);
		Assert.Equal(p0.Y + Metrics.EffectiveStackStride, p1.Y);
	}

	[Fact]
	public void T09_BottomRight_UsesPerPositionIndex_NotGlobalCount()
	{
		// Even if "other" toasts exist conceptually, stackIndex alone drives location.
		var atIndex2 = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomRight, 2, Metrics, Area);
		var expectedY = Area.Bottom - Metrics.ToastHeight - Metrics.VerticalMargin
			- 2 * Metrics.EffectiveStackStride;
		Assert.Equal(expectedY, atIndex2.Y);
	}

	[Fact]
	public void T09b_TopLeft_Index0_AtTopLeftAnchor()
	{
		var p = ToastLayoutEngine.ComputeLocation(ToastPosition.TopLeft, 0, Metrics, Area);

		Assert.Equal(Area.Left + Metrics.HorizontalMargin, p.X);
		Assert.Equal(Area.Top + Metrics.VerticalMargin, p.Y);
	}

	[Fact]
	public void T09c_BottomLeft_StackGrowsUp()
	{
		var p0 = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomLeft, 0, Metrics, Area);
		var p1 = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomLeft, 1, Metrics, Area);

		Assert.Equal(Area.Left + Metrics.HorizontalMargin, p0.X);
		Assert.Equal(Area.Bottom - Metrics.ToastHeight - Metrics.VerticalMargin, p0.Y);
		Assert.Equal(p0.Y - Metrics.EffectiveStackStride, p1.Y);
	}

	[Fact]
	public void T09d_FourCorners_IndependentAnchors()
	{
		var tl = ToastLayoutEngine.ComputeLocation(ToastPosition.TopLeft, 0, Metrics, Area);
		var tr = ToastLayoutEngine.ComputeLocation(ToastPosition.TopRight, 0, Metrics, Area);
		var bl = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomLeft, 0, Metrics, Area);
		var br = ToastLayoutEngine.ComputeLocation(ToastPosition.BottomRight, 0, Metrics, Area);

		Assert.True(tl.X < tr.X);
		Assert.True(bl.X < br.X);
		Assert.True(tl.Y < bl.Y);
		Assert.True(tr.Y < br.Y);
		Assert.Equal(tl.Y, tr.Y);
		Assert.Equal(bl.Y, br.Y);
	}

	[Fact]
	public void ComputeStack_ReturnsCountLocations()
	{
		var stack = ToastLayoutEngine.ComputeStack(ToastPosition.TopRight, 3, Metrics, Area);
		Assert.Equal(3, stack.Count);
		Assert.Equal(
			ToastLayoutEngine.ComputeLocation(ToastPosition.TopRight, 2, Metrics, Area),
			stack[2]);
	}

	[Fact]
	public void DefaultMetrics_AreTouchableAndSpacious()
	{
		Assert.True(Metrics.CloseButtonSize >= Metrics.MinTouchTargetPx);
		Assert.True(Metrics.MinTouchTargetPx >= 44);
		Assert.True(Metrics.CaptionDescriptionGap >= 8);
		Assert.True(Metrics.ToastHeight >= 140);
		Assert.True(Metrics.ToastWidth >= 420);
		Assert.True(Metrics.ContentPaddingLeft >= 12);
		Assert.True(Metrics.StackGap >= 8);
	}
}
