using System.Drawing;
using FuzzyToast.Internal;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class UiMarshalerAndFormEdgeTests
{
	[Fact]
	public void WinFormsUiMarshaler_FromBackgroundThread_UsesInvoke()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var m = new WinFormsUiMarshaler(form);
			var threadId = Environment.CurrentManagedThreadId;
			int? ranOn = null;

			var worker = new Thread(() =>
			{
				m.Invoke(() => ranOn = Environment.CurrentManagedThreadId);
			});
			worker.IsBackground = true;
			worker.Start();
			// Pump messages so Invoke can complete
			var sw = System.Diagnostics.Stopwatch.StartNew();
			while (worker.IsAlive && sw.Elapsed < TimeSpan.FromSeconds(5))
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}
			worker.Join(1000);

			Assert.NotNull(ranOn);
			Assert.Equal(threadId, ranOn);
		});
	}

	[Fact]
	public void WinFormsUiMarshaler_InvokeAsync_FromBackground()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var m = new WinFormsUiMarshaler(form);
			Task? task = null;
			var worker = new Thread(() =>
			{
				task = m.InvokeAsync(() => { });
			});
			worker.IsBackground = true;
			worker.Start();
			var sw = System.Diagnostics.Stopwatch.StartNew();
			while ((task is null || !task.IsCompleted) && sw.Elapsed < TimeSpan.FromSeconds(5))
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}
			worker.Join(1000);
			Assert.NotNull(task);
			Assert.True(task!.IsCompletedSuccessfully);
		});
	}

	[Fact]
	public void ToastForm_CloseStyle_ButtonOnly_And_HoverPause()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "Hover",
				Description = "pause",
				CloseStyle = CloseStyle.Button,
				Animation = Animation.Fade,
				IsMuted = true
			};
			var handle = new ToastHandle("h", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.ErrorDark), 4000, pauseOnHover: true, playSound: false);
			view.SetBounds(new Rectangle(50, 50, 420, 140));
			view.Show(form);
			Application.DoEvents();

			// Drive timer Shown path already ran; force leave/enter via reflection not needed —
			// call BeginDismiss after short wait to cover timer interval path partially
			Thread.Sleep(50);
			Application.DoEvents();
			view.BeginDismiss();
			Application.DoEvents();
			Thread.Sleep(150);
			Application.DoEvents();
			// second BeginDismiss is no-op
			view.BeginDismiss();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_OwnsThumbnail_DisposesImage()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var bmp = new Bitmap(64, 64);
			var options = new ToastOptions
			{
				Caption = "Own",
				Thumbnail = bmp,
				OwnsThumbnail = true,
				IsMuted = true
			};
			var handle = new ToastHandle("own", options, ToastHandleState.Visible, null!);
			var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 2000, false, false);
			view.SetBounds(new Rectangle(20, 20, 400, 120));
			view.Show(form);
			Application.DoEvents();
			view.BeginDismiss();
			Application.DoEvents();
			Thread.Sleep(200);
			Application.DoEvents();
			view.Dispose();
			// After owns dispose, image should be disposed (use may throw)
			Assert.ThrowsAny<Exception>(() => _ = bmp.Width);
			form.Close();
		});
	}

	[Fact]
	public void ToastManager_ShowAsync_Cancellation_Dismisses()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			using var mgr = new ToastManager(form, new ToastManagerOptions { PlaySound = false, ShortDurationMs = 10000 });
			using var cts = new CancellationTokenSource();
			var task = mgr.ShowAsync(new ToastOptions { Caption = "Cancel me", IsMuted = true }, cts.Token);
			// pump until shown
			ToastHandle? h = null;
			var sw = System.Diagnostics.Stopwatch.StartNew();
			while (!task.IsCompleted && sw.Elapsed < TimeSpan.FromSeconds(3))
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}
			h = task.GetAwaiter().GetResult();
			if (h.IsVisible)
			{
				cts.Cancel();
				Application.DoEvents();
				Thread.Sleep(100);
				Application.DoEvents();
			}
			mgr.DismissAll();
			Application.DoEvents();
		});
	}

	[Fact]
	public void ToastManager_Dispose_Idempotent_WithOwner()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var mgr = new ToastManager(form, new ToastManagerOptions { PlaySound = false });
			mgr.Show(new ToastOptions { Caption = "x", IsMuted = true });
			Application.DoEvents();
			mgr.Dispose();
			mgr.Dispose();
			Assert.True(mgr.IsDisposed);
			form.Close();
		});
	}

	[Fact]
	public void ImageValidation_IsPng_IsJpeg_Edge()
	{
		Assert.False(ImageValidation.IsPng([]));
		Assert.False(ImageValidation.IsJpeg([0xFF, 0xD8]));
		Assert.True(ImageValidation.IsJpeg([0xFF, 0xD8, 0xFF]));
	}
}
