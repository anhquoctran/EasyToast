using System.Drawing;
using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class ToastBuilderTests
{
	private static ToastManager CreateManager()
	{
		var area = new ScreenWorkingArea(0, 0, 1920, 1080);
		return new ToastManager(
			owner: null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));
	}

	[Fact]
	public void T13_Builder_MapsThemeAndPosition()
	{
		using var mgr = CreateManager();
		var options = mgr.Create()
			.SetCaption("Hello")
			.SetDescription("World")
			.SetTheme(ToastTheme.SuccessDark)
			.SetPosition(ToastPosition.TopLeft)
			.SetAnimation(Animation.Slide)
			.SetDuration(Duration.Long)
			.SetCloseStyle(CloseStyle.Button)
			.SetMuting(true)
			.SetTag(42)
			.Build();

		Assert.Equal("Hello", options.Caption);
		Assert.Equal("World", options.Description);
		Assert.Equal(ToastTheme.SuccessDark, options.Theme);
		Assert.Equal(ToastPosition.TopLeft, options.Position);
		Assert.Equal(Animation.Slide, options.Animation);
		Assert.Equal(Duration.Long, options.Duration);
		Assert.Equal(CloseStyle.Button, options.CloseStyle);
		Assert.True(options.IsMuted);
		Assert.Equal(42, options.Tag);
	}

	[Fact]
	public void Builder_Show_ReturnsVisibleHandle()
	{
		using var mgr = CreateManager();
		var h = mgr.Create().SetCaption("Hi").Show();
		Assert.True(h.IsVisible);
		Assert.Equal(1, mgr.Count);
	}

	[Fact]
	public void SetCustomColors_SetsThemeCustom()
	{
		using var mgr = CreateManager();
		var o = mgr.Create()
			.SetCaption("X")
			.SetCustomColors(Color.Red, Color.White)
			.Build();
		Assert.Equal(ToastTheme.Custom, o.Theme);
		Assert.NotNull(o.CustomColors);
	}
}
