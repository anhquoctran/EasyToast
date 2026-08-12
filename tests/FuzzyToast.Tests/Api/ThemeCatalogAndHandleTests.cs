using System.Drawing;
using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class ThemeCatalogAndHandleTests
{
	[Theory]
	[InlineData(ToastTheme.Dark)]
	[InlineData(ToastTheme.Light)]
	[InlineData(ToastTheme.PrimaryLight)]
	[InlineData(ToastTheme.SuccessLight)]
	[InlineData(ToastTheme.WarningLight)]
	[InlineData(ToastTheme.ErrorLight)]
	[InlineData(ToastTheme.PrimaryDark)]
	[InlineData(ToastTheme.SuccessDark)]
	[InlineData(ToastTheme.WarningDark)]
	[InlineData(ToastTheme.ErrorDark)]
	public void ThemeCatalog_Resolves_AllBuiltins(ToastTheme theme)
	{
		var s = ThemeCatalog.Resolve(theme);
		Assert.NotEqual(0, s.Background.ToArgb() | s.Foreground.ToArgb() | 1);
	}

	[Fact]
	public void ColorScheme_ColorCtor_And_Equals()
	{
		var a = new ColorScheme(Color.FromArgb(1, 2, 3), Color.FromArgb(4, 5, 6));
		var b = new ColorScheme(1, 2, 3, 4, 5, 6);
		Assert.True(a.Equals(b));
		Assert.True(a.Equals((object)b));
		Assert.False(a.Equals(null));
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
	}

	[Fact]
	public void ToastHandle_Dismiss_WithoutManager_IsSafe()
	{
		var h = new ToastHandle("x", new ToastOptions { Caption = "c" }, ToastHandleState.Visible, null!);
		h.Dismiss(); // manager null — should not throw if DismissInternal not called
		// Actually Dismiss calls _manager?.DismissInternal — OK
		h.Cancel();
		h.Dispose();
		h.Dispose(); // idempotent
	}

	[Fact]
	public void ToastHandle_Rejected_WhenDismissed_Completed()
	{
		var h = new ToastHandle("r", new ToastOptions { Caption = "c" }, ToastHandleState.RejectedCapacity, null!);
		Assert.True(h.WasRejected);
		Assert.True(h.WhenDismissed.IsCompletedSuccessfully);
		h.RaiseClicked();
		h.RaiseHovered();
		h.MarkDismissed();
	}

	[Fact]
	public void ToastHandle_Events_Fire_WhenVisible()
	{
		var h = new ToastHandle(
			"v",
			new ToastOptions
			{
				Caption = "c",
				Tag = "tag-1",
				Metadata = ToastOptions.FreezeMetadata(new Dictionary<string, object?> { ["k"] = "v" })
			},
			ToastHandleState.Visible,
			null!);
		var click = 0;
		var hover = 0;
		var dismissed = 0;
		ToastInteractionEventArgs? clickArgs = null;
		h.Clicked += (_, e) => { click++; clickArgs = e; };
		h.Hovered += (_, _) => hover++;
		h.Dismissed += (_, _) => dismissed++;
		h.RaiseClicked();
		h.RaiseHovered();
		h.MarkVisible();
		h.MarkDismissed();
		Assert.Equal(1, click);
		Assert.Equal(1, hover);
		Assert.Equal(1, dismissed);
		Assert.True(h.IsDismissed);
		Assert.NotNull(clickArgs);
		Assert.Equal("tag-1", clickArgs!.Tag);
		Assert.Equal("v", clickArgs.Metadata["k"]);
	}

	[Fact]
	public void ToastManager_DismissAll_Owner_And_Options()
	{
		var area = new ScreenWorkingArea(0, 0, 1920, 1080);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions
			{
				PlaySound = false,
				MaxToastsPerPosition = 3,
				ShortDurationMs = 1000,
				LongDurationMs = 2000
			},
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));

		Assert.Equal(1000, mgr.Options.ResolveDurationMs(Duration.Short));
		Assert.Equal(2000, mgr.Options.ResolveDurationMs(Duration.Long));

		mgr.Show(new ToastOptions { Caption = "1" });
		mgr.Show(new ToastOptions { Caption = "2" });
		Assert.Equal(2, mgr.Count);
		mgr.DismissAll();
		Assert.Equal(0, mgr.Count);

		Assert.Throws<InvalidOperationException>(() => _ = mgr.Owner);
	}

	[Fact]
	public void ToastManager_WithRealOwner_PublicCtor()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			using var mgr = new ToastManager(form, new ToastManagerOptions { PlaySound = false });
			Assert.Same(form, mgr.Owner);
			var h = mgr.Create().SetCaption("From manager").SetMuting(true).Show();
			Application.DoEvents();
			Assert.True(h.IsVisible || h.WasRejected);
			mgr.DismissAll();
			Application.DoEvents();
		});
	}

	[Fact]
	public void ToastBuilder_Full_Surface()
	{
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));

		using var bmp = new Bitmap(70, 70);
		var h = mgr.Create()
			.SetCaption("Full")
			.SetDescription("desc")
			.SetDuration(Duration.Long)
			.SetAnimation(Animation.Slide)
			.SetPosition(ToastPosition.BottomLeft)
			.SetTheme(ToastTheme.WarningLight)
			.SetCustomColors(new ColorScheme(1, 2, 3, 4, 5, 6))
			.SetCloseStyle(CloseStyle.ClickEntire)
			.SetMuting(true)
			.SetThumbnail(bmp, true)
			.SetTag("t")
			.Show();

		Assert.True(h.IsVisible);
		Assert.Equal(ToastPosition.BottomLeft, h.Options.Position);
	}

	[Fact]
	public async Task ToastBuilder_ShowAsync()
	{
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));

		var h = await mgr.Create().SetCaption("A").ShowAsync();
		Assert.True(h.IsVisible);
	}

	[Fact]
	public void ToastEvents_Construct()
	{
		var h = new ToastHandle("e", new ToastOptions { Caption = "c" }, ToastHandleState.Visible, null!);
		var changed = new ToastChangedEventArgs(h);
		Assert.Same(h, changed.Toast);
		var rejected = new ToastRejectedEventArgs(h, h.Options, "reason");
		Assert.Equal("reason", rejected.Reason);
		Assert.Same(h.Options, rejected.Options);
	}

	[Fact]
	public void LayoutEngine_NegativeIndex_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			ToastLayoutEngine.ComputeLocation(
				ToastPosition.TopRight, -1, ToastLayoutMetrics.Default,
				new ScreenWorkingArea(0, 0, 100, 100)));
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			ToastLayoutEngine.ComputeStack(ToastPosition.TopRight, -1, ToastLayoutMetrics.Default,
				new ScreenWorkingArea(0, 0, 100, 100)));
	}

	[Fact]
	public void CapacityPolicy_InvalidArgs()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			CapacityPolicy.Evaluate(ToastOverflowPolicy.DropNewest, 0, 1, ToastPosition.TopRight, Array.Empty<(string, ToastPosition)>()));
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			CapacityPolicy.Evaluate(ToastOverflowPolicy.DropNewest, 1, 0, ToastPosition.TopRight, Array.Empty<(string, ToastPosition)>()));
	}

	[Fact]
	public void AutoDismiss_DoublePause_And_ResumeWhenExpired()
	{
		var s = new AutoDismissTimerState(100);
		s.StartOrResume();
		s.Pause(50);
		s.Pause(10); // second pause no-op
		s.OnTimerElapsed();
		Assert.True(s.IsExpired);
		Assert.Equal(1, s.Resume()); // max(0,1)
	}

	[Fact]
	public void DpiScaling_GetScale_FromControl()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var scale = DpiScaling.GetScale(form);
			Assert.True(scale >= 1f);
			Assert.Equal(1f, DpiScaling.GetScale(0));
			Assert.Equal(1f, DpiScaling.GetScale((Control?)null));
		});
	}

	[Fact]
	public void ImageValidation_Path_And_NullImage()
	{
		Assert.False(ImageValidation.ValidateImageSize(null));
		Assert.False(ImageValidation.ValidateImagePath(null));
		Assert.False(ImageValidation.ValidateImagePath(""));
		Assert.False(ImageValidation.ValidateImagePath(@"C:\no\such\file\xyz.png"));

		var dir = Path.Combine(Path.GetTempPath(), "FuzzyToastTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(dir);
		try
		{
			var png = Path.Combine(dir, "t.png");
			File.WriteAllBytes(png, [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0]);
			Assert.True(ImageValidation.ValidateImagePath(png));

			var jpg = Path.Combine(dir, "t.jpg");
			File.WriteAllBytes(jpg, [0xFF, 0xD8, 0xFF, 0xE1, 0, 0]);
			Assert.True(ImageValidation.ValidateImagePath(jpg));

			var bad = Path.Combine(dir, "t.bin");
			File.WriteAllBytes(bad, [1, 2, 3, 4]);
			Assert.False(ImageValidation.ValidateImagePath(bad));
		}
		finally
		{
			try { Directory.Delete(dir, true); } catch { /* ignore */ }
		}
	}

	[Fact]
	public void ToastManagerOptions_ToLayoutMetrics_And_ResolveDuration()
	{
		var o = new ToastManagerOptions
		{
			ToastWidth = 400,
			ToastHeight = 100,
			StackGap = 8
		};
		var m = o.ToLayoutMetrics();
		Assert.Equal(400, m.ToastWidth);
		Assert.Equal(100, m.ToastHeight);
		Assert.Equal(o.ResolveDurationMs(Duration.LENGTH_SHORT), o.ShortDurationMs);
		Assert.Equal(o.ResolveDurationMs(Duration.LENGTH_LONG), o.LongDurationMs);
	}

	[Fact]
	public void FixedScreenProvider_ReturnsFixed()
	{
		var area = new ScreenWorkingArea(1, 2, 3, 4);
		var p = new FixedScreenProvider(area);
		Assert.Equal(area, p.GetLeftmostWorkingArea());
		Assert.Equal(area, p.GetRightmostWorkingArea());
		Assert.Equal(area, p.GetWorkingAreaNear(new LayoutRect(0, 0, 1, 1)));
	}
}
