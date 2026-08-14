using System.Drawing;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

/// <summary>Broad coverage of public Toast.MakeText overloads and fluent API (STA).</summary>
public class ToastApiCoverageTests
{
	[Fact]
	public void All_Build_Overloads_Construct()
	{
		StaHelper.Run(() =>
		{
			using var form = new Form();
			using var bmp = new Bitmap(80, 80);

			Assert.NotNull(Toast.MakeText(form, "a"));
			Assert.NotNull(Toast.MakeText(form, "a", "d"));
			Assert.NotNull(Toast.MakeText(form, "a", Duration.LENGTH_SHORT));
			Assert.NotNull(Toast.MakeText(form, "a", Duration.LENGTH_LONG, Animation.FADE));
			Assert.NotNull(Toast.MakeText(form, "a", "d", Duration.LENGTH_LONG));
			Assert.NotNull(Toast.MakeText(form, "a", Animation.SLIDE, Duration.LENGTH_SHORT, true));
			Assert.NotNull(Toast.MakeText(form, "a", Animation.FADE));
			Assert.NotNull(Toast.MakeText(form, "a", true));
			Assert.NotNull(Toast.MakeText(form, "a", bmp, Duration.LENGTH_SHORT, Animation.SLIDE));
			Assert.NotNull(Toast.MakeText(form, "a", bmp, Duration.LENGTH_SHORT, Animation.FADE, true));
			Assert.NotNull(Toast.MakeText(form, "a", bmp));
			Assert.NotNull(Toast.MakeText(form, "a", bmp, Duration.LENGTH_LONG));
			Assert.NotNull(Toast.MakeText(form, "a", ToastTheme.SuccessLight));
		});
	}

	[Fact]
	public void Show_And_Dismiss_On_Real_Form()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			try
			{
				var toast = Toast.MakeText(form, "Coverage toast", "description")
					.SetTheme(ToastTheme.Dark)
					.SetPosition(ToastPosition.BottomRight)
					.SetAnimation(Animation.FADE)
					.SetDuration(Duration.LENGTH_SHORT)
					.SetMuting(true)
					.SetCloseStyle(CloseStyle.ButtonAndClickEntire)
					.SetTag("tag");

				var clicked = 0;
				var closed = 0;
				toast.OnClick += (_, _) => clicked++;
				toast.OnClosed += (_, _) => closed++;

				toast.Show();
				Application.DoEvents();
				Assert.False(string.IsNullOrEmpty(toast.Guid));
				Assert.NotNull(toast.Handle);
				Assert.True(toast.Handle!.IsVisible || toast.Handle.WasRejected);

				if (toast.Handle.IsVisible)
				{
					toast.Handle.RaiseClicked(); // via public path: Dismiss
					toast.Dismiss();
					Application.DoEvents();
					// allow close pipeline
					for (var i = 0; i < 20 && toast.Handle.IsVisible; i++)
					{
						Thread.Sleep(50);
						Application.DoEvents();
					}
				}

				toast.Cancel(); // no-op after dismiss
				_ = clicked;
				_ = closed;
			}
			finally
			{
				form.Close();
				Application.DoEvents();
			}
		});
	}

	[Fact]
	public void ShowAsync_Completes()
	{
		StaHelper.Run(async () =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			try
			{
				var toast = Toast.MakeText(form, "Async coverage").SetMuting(true);
				await toast.ShowAsync();
				Application.DoEvents();
				Assert.NotNull(toast.Handle);
				toast.Dismiss();
				Application.DoEvents();
			}
			finally
			{
				form.Close();
			}
		});
	}

	[Fact]
	public void Fluent_Setters_Chain()
	{
		StaHelper.Run(() =>
		{
			using var form = new Form();
			using var bmp = new Bitmap(64, 64);
			var t = Toast.MakeText(form, "x")
				.SetCaption("C")
				.SetDescription("D")
				.SetDuration(Duration.Long)
				.SetAnimation(Animation.Slide)
				.SetPosition(ToastPosition.TopLeft)
				.SetTheme(ToastTheme.Custom)
				.SetCustomColors(Color.Red, Color.White)
				.SetCloseStyle(CloseStyle.Button)
				.SetMuting()
				.SetThumbnail(bmp, ownsImage: false)
				.SetTag(99);

			Assert.Equal("C", t.Caption);
			Assert.Equal("D", t.Description);
			Assert.Equal(Duration.Long, t.Duration);
			Assert.Equal(Animation.Slide, t.Animation);
			Assert.Equal(ToastPosition.TopLeft, t.Position);
			Assert.Equal(ToastTheme.Custom, t.Theme);
			Assert.True(t.IsMuted);
			Assert.Same(bmp, t.Thumbnail);
		});
	}

	[Fact]
	public void Build_Requires_Control_Owner()
	{
		var fake = new NonControlWindow();
		Assert.Throws<ArgumentException>(() => Toast.MakeText(fake, "x"));
	}

	private sealed class NonControlWindow : IWin32Window
	{
		public IntPtr Handle => IntPtr.Zero;
	}
}
