using System.Drawing;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class FormToastExtensionsTests
{
	[Fact]
	public void ShowToast_NullForm_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => FormToastExtensions.ShowToast(null!, "Hi"));
	}

	[Fact]
	public void Toast_NullForm_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => FormToastExtensions.Toast(null!, "Hi"));
		Assert.Throws<ArgumentNullException>(() => FormToastExtensions.Toast(null!, "Hi", "Desc"));
	}

	[Fact]
	public void ShowToast_NullConfigure_Throws()
	{
		StaHelper.Run(() =>
		{
			using var form = new Form();
			Assert.Throws<ArgumentNullException>(() => form.ShowToast("Hi", configure: null!));
			Assert.Throws<ArgumentNullException>(() => form.ShowToast("Hi", "Desc", configure: null!));
		});
	}

	[Fact]
	public void Toast_Factory_DoesNotShow()
	{
		StaHelper.Run(() =>
		{
			using var form = new Form();
			var toast = form.Toast("Cap");
			Assert.Equal("Cap", toast.Caption);
			Assert.Null(toast.Handle);
			Assert.Equal(string.Empty, toast.Guid);

			var withDesc = form.Toast("Cap", "Desc");
			Assert.Equal("Desc", withDesc.Description);
			Assert.Null(withDesc.Handle);
		});
	}

	[Fact]
	public void ShowToast_Overloads_SetOptionsAndReturnHandle()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			using var bmp = new Bitmap(48, 48);
			try
			{
				AssertShown(form.ShowToast("Hello"), "Hello", "", Duration.Short, ToastTheme.Dark);
				AssertShown(form.ShowToast("Saved", "Written"), "Saved", "Written", Duration.Short, ToastTheme.Dark);
				AssertShown(form.ShowToast("Long", Duration.Long), "Long", "", Duration.Long, ToastTheme.Dark);
				AssertShown(form.ShowToast("Themed", ToastTheme.Light), "Themed", "", Duration.Short, ToastTheme.Light);
				AssertShown(
					form.ShowToast("A", "B", Duration.Long),
					"A", "B", Duration.Long, ToastTheme.Dark);
				AssertShown(
					form.ShowToast("A", "B", ToastTheme.PrimaryLight),
					"A", "B", Duration.Short, ToastTheme.PrimaryLight);
				AssertShown(
					form.ShowToast("A", "B", Duration.Long, ToastTheme.WarningLight),
					"A", "B", Duration.Long, ToastTheme.WarningLight);

				var thumb = form.ShowToast("Pic", bmp);
				Assert.Same(bmp, thumb.Options.Thumbnail);
				AssertShown(thumb, "Pic", "", Duration.Short, ToastTheme.Dark);

				var configured = form.ShowToast("Cfg", t => t
					.SetTheme(ToastTheme.SuccessLight)
					.SetMuting()
					.SetPosition(ToastPosition.TopLeft));
				AssertShown(configured, "Cfg", "", Duration.Short, ToastTheme.SuccessLight);
				Assert.True(configured.Options.IsMuted);
				Assert.Equal(ToastPosition.TopLeft, configured.Options.Position);

				var configuredDesc = form.ShowToast("Cfg2", "More", t => t.SetMuting());
				AssertShown(configuredDesc, "Cfg2", "More", Duration.Short, ToastTheme.Dark);
				Assert.True(configuredDesc.Options.IsMuted);
			}
			finally
			{
				DismissAll(form);
				form.Close();
				Application.DoEvents();
			}
		});
	}

	[Fact]
	public void SemanticHelpers_UseExpectedThemes()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			try
			{
				AssertShown(form.ShowSuccess("Ok"), "Ok", "", Duration.Short, ToastTheme.SuccessDark);
				AssertShown(
					form.ShowSuccess("Ok", "Done", Duration.Long),
					"Ok", "Done", Duration.Long, ToastTheme.SuccessDark);
				AssertShown(form.ShowError("Fail"), "Fail", "", Duration.Short, ToastTheme.ErrorDark);
				AssertShown(
					form.ShowError("Fail", "No disk", Duration.Long),
					"Fail", "No disk", Duration.Long, ToastTheme.ErrorDark);
				AssertShown(form.ShowWarning("Care"), "Care", "", Duration.Short, ToastTheme.WarningDark);
				AssertShown(
					form.ShowWarning("Care", "Almost", Duration.Long),
					"Care", "Almost", Duration.Long, ToastTheme.WarningDark);
				AssertShown(form.ShowInfo("Note"), "Note", "", Duration.Short, ToastTheme.PrimaryDark);
				AssertShown(
					form.ShowInfo("Note", "Read me", Duration.Long),
					"Note", "Read me", Duration.Long, ToastTheme.PrimaryDark);
			}
			finally
			{
				DismissAll(form);
				form.Close();
				Application.DoEvents();
			}
		});
	}

	[Fact]
	public void ShowToast_DisposedForm_Throws()
	{
		StaHelper.Run(() =>
		{
			var form = new Form();
			form.Dispose();
			Assert.Throws<ObjectDisposedException>(() => form.ShowToast("Hi"));
		});
	}

	[Fact]
	public void ShowToast_EmptyCaption_Throws()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			try
			{
				Assert.Throws<ArgumentException>(() => form.ShowToast(""));
			}
			finally
			{
				form.Close();
				Application.DoEvents();
			}
		});
	}

	private static void AssertShown(
		ToastHandle handle,
		string caption,
		string description,
		Duration duration,
		ToastTheme theme)
	{
		Application.DoEvents();
		Assert.Equal(caption, handle.Options.Caption);
		Assert.Equal(description, handle.Options.Description);
		Assert.Equal(duration, handle.Options.Duration);
		Assert.Equal(theme, handle.Options.Theme);
		Assert.True(handle.IsVisible || handle.WasRejected);
		handle.Dismiss();
		Application.DoEvents();
	}

	private static void DismissAll(Form form)
	{
		try
		{
			var manager = FuzzyToast.Internal.ToastManagerRegistry.GetOrCreate(form);
			foreach (var handle in manager.ActiveToasts.ToArray())
				handle.Dismiss();
		}
		catch
		{
			// Owner already gone.
		}

		Application.DoEvents();
	}
}
