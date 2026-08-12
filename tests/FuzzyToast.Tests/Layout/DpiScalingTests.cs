using FuzzyToast.Internal;
using FuzzyToast.Layout;
using Xunit;

namespace FuzzyToast.Tests;

public class DpiScalingTests
{
	[Theory]
	[InlineData(96, 1f)]
	[InlineData(120, 1.25f)]
	[InlineData(144, 1.5f)]
	[InlineData(192, 2f)]
	public void GetScale_FromDeviceDpi(int dpi, float expected)
	{
		Assert.Equal(expected, DpiScaling.GetScale(dpi), precision: 3);
	}

	[Fact]
	public void ScaleMetrics_At150Percent_ScalesSizeAndMargins()
	{
		var m = DpiScaling.ScaleMetrics(ToastLayoutMetrics.Default, 1.5f);
		Assert.Equal(DpiScaling.Scale(ToastLayoutMetrics.Default.ToastWidth, 1.5f), m.ToastWidth);
		Assert.Equal(DpiScaling.Scale(ToastLayoutMetrics.Default.ToastHeight, 1.5f), m.ToastHeight);
		Assert.Equal(DpiScaling.Scale(ToastLayoutMetrics.Default.CloseButtonSize, 1.5f), m.CloseButtonSize);
		Assert.True(m.CloseButtonSize >= 44);
	}

	[Fact]
	public void ScaleMetrics_At100Percent_IsIdentity()
	{
		var m = DpiScaling.ScaleMetrics(ToastLayoutMetrics.Default, 1f);
		Assert.Equal(ToastLayoutMetrics.Default.ToastWidth, m.ToastWidth);
		Assert.Equal(ToastLayoutMetrics.Default.ToastHeight, m.ToastHeight);
	}
}
