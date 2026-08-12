using System.Drawing;
using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class ToastFormAndPlatformTests
{
	[Fact]
	public void ToastForm_Apply_Show_Dismiss_CoversUiPath()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "UI",
				Description = "Coverage",
				Animation = Animation.Fade,
				CloseStyle = CloseStyle.ButtonAndClickEntire,
				IsMuted = true,
				Theme = ToastTheme.PrimaryDark
			};
			var handle = new ToastHandle("id1", options, ToastHandleState.Visible, manager: null!);
			// manager null only for view unit path — Dismiss uses manager if set
			using var view = new ToastForm(handle);
			var scheme = ThemeCatalog.Resolve(ToastTheme.PrimaryDark);
			view.Apply(options, scheme, durationMs: 5000, pauseOnHover: true, playSound: false);
			view.SetBounds(new Rectangle(100, 100, 420, 140));
			view.Show(form);
			Application.DoEvents();

			// Hover pause path
			view.GetType(); // keep ref
			// Simulate mouse enter/leave via reflection-free public surface: BeginDismiss
			view.BeginDismiss();
			Application.DoEvents();
			for (var i = 0; i < 40 && !view.IsDisposed; i++)
			{
				Thread.Sleep(25);
				Application.DoEvents();
			}

			form.Close();
			Application.DoEvents();
		});
	}

	[Fact]
	public void ToastForm_Slide_And_ClickEntire_And_Thumbnail()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			using var bmp = new Bitmap(64, 64);
			var options = new ToastOptions
			{
				Caption = "Slide",
				Description = "img",
				Animation = Animation.Slide,
				CloseStyle = CloseStyle.ClickEntire,
				Thumbnail = bmp,
				OwnsThumbnail = false,
				IsMuted = true
			};
			var handle = new ToastHandle("id2", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Light), 3000, pauseOnHover: false, playSound: false);
			view.SetBounds(new Rectangle(120, 120, 420, 140));
			view.Show(form);
			Application.DoEvents();
			view.BeginDismiss();
			Application.DoEvents();
			Thread.Sleep(100);
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_PlaySound_BestEffort()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions { Caption = "Sound", IsMuted = false, Animation = Animation.Fade };
			var handle = new ToastHandle("id3", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			// playSound true exercises TryPlaySound
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 2000, false, playSound: true);
			view.SetBounds(new Rectangle(10, 10, 400, 120));
			view.Show(form);
			Application.DoEvents();
			view.BeginDismiss();
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void WinFormsScreenProvider_ReturnsAreas()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var sp = new WinFormsScreenProvider(form);
			var owner = sp.GetOwnerOrPrimaryWorkingArea();
			Assert.True(owner.Width > 0);
			Assert.True(owner.Height > 0);

			var left = sp.GetLeftmostWorkingArea();
			var right = sp.GetRightmostWorkingArea();
			Assert.True(left.Width > 0);
			Assert.True(right.Width > 0);

			var near = sp.GetWorkingAreaNear(new LayoutRect(owner.Left + 10, owner.Top + 10, 50, 50));
			Assert.True(near.Width > 0);

			var noOwner = new WinFormsScreenProvider(null);
			Assert.True(noOwner.GetOwnerOrPrimaryWorkingArea().Width > 0);
			Assert.True(noOwner.GetLeftmostWorkingArea().Width > 0);
			Assert.True(noOwner.GetRightmostWorkingArea().Width > 0);
		});
	}

	[Fact]
	public void WinFormsUiMarshaler_Invoke_And_Async()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var m = new WinFormsUiMarshaler(form);
			var ran = false;
			m.Invoke(() => ran = true);
			Assert.True(ran);

			var ran2 = false;
			m.InvokeAsync(() => ran2 = true).GetAwaiter().GetResult();
			Assert.True(ran2);
			Assert.False(m.InvokeRequired);
		});
	}

	[Fact]
	public void ToastManagerRegistry_GetOrCreate_SameInstance()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var a = ToastManagerRegistry.GetOrCreate(form);
			var b = ToastManagerRegistry.GetOrCreate(form);
			Assert.Same(a, b);

			var custom = new ToastManager(form, new ToastManagerOptions { PlaySound = false });
			ToastManagerRegistry.Register(form, custom);
			Assert.Same(custom, ToastManagerRegistry.GetOrCreate(form));
		});
	}

	[Fact]
	public void ScreenWorkingArea_WidthHeight()
	{
		var a = new ScreenWorkingArea(10, 20, 110, 220);
		Assert.Equal(100, a.Width);
		Assert.Equal(200, a.Height);
	}
}
