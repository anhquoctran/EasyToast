using System.Drawing;
using Xunit;

namespace FuzzyToast.Tests;

public class SecurityAndLimitsTests
{
	[Fact]
	public void ValidateImagePath_DoesNotReadWholeFile_AndRejectsOversize()
	{
		var dir = Path.Combine(Path.GetTempPath(), "FuzzyToastSec_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(dir);
		try
		{
			var huge = Path.Combine(dir, "huge.png");
			using (var fs = new FileStream(huge, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				fs.Write([137, 80, 78, 71, 13, 10, 26, 10], 0, 8);
				fs.SetLength(ToastLimits.MaxImageFileBytes + 1);
			}

			Assert.False(ImageValidation.ValidateImagePath(huge));

			var ok = Path.Combine(dir, "ok.png");
			File.WriteAllBytes(ok, [137, 80, 78, 71, 13, 10, 26, 10]);
			Assert.True(ImageValidation.ValidateImagePath(ok));
		}
		finally
		{
			try { Directory.Delete(dir, true); } catch { /* ignore */ }
		}
	}

	[Fact]
	public void ValidateImagePath_RejectsReservedDeviceAndInvalidPath()
	{
		Assert.False(ImageValidation.ValidateImagePath("CON"));
		Assert.False(ImageValidation.ValidateImagePath(@"C:\NUL.png"));
		Assert.False(ImageValidation.ValidateImagePath("not-a-file.png"));
	}

	[Fact]
	public void ValidateImageSize_RejectsOversizedBitmap()
	{
		using var huge = new Bitmap(ToastLimits.MaxImageDimension + 1, 64);
		Assert.False(ImageValidation.ValidateImageSize(huge, minWidth: 1, minHeight: 1));
		using var ok = new Bitmap(64, 64);
		Assert.True(ImageValidation.ValidateImageSize(ok));
	}

	[Fact]
	public void ToastOptions_RejectsOversizedCaptionAndDuration()
	{
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = new string('x', ToastLimits.MaxCaptionLength + 1) }.Validate());
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new ToastOptions { Caption = "ok", DurationMs = ToastLimits.MaxDurationMs + 1 }.Validate());
	}

	[Fact]
	public void ToastOptions_RejectsOversizedThumbnail()
	{
		using var bmp = new Bitmap(ToastLimits.MaxImageDimension + 1, 10);
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", Thumbnail = bmp }.Validate());
	}

	[Fact]
	public void FreezeMetadata_CapsEntriesAndLongKeys()
	{
		var entries = new List<KeyValuePair<string, object?>>();
		for (var i = 0; i < ToastLimits.MaxMetadataEntries + 10; i++)
			entries.Add(new KeyValuePair<string, object?>("k" + i, i));
		entries.Add(new KeyValuePair<string, object?>(new string('k', ToastLimits.MaxMetadataKeyLength + 1), "nope"));

		var frozen = ToastOptions.FreezeMetadata(entries);
		Assert.Equal(ToastLimits.MaxMetadataEntries, frozen.Count);
		Assert.DoesNotContain(frozen.Keys, k => k.Length > ToastLimits.MaxMetadataKeyLength);
	}

	[Fact]
	public void SetMetadata_RejectsOversizedKey()
	{
		using var form = new Form();
		var toast = Toast.Build(form, "x");
		Assert.Throws<ArgumentException>(() =>
			toast.SetMetadata(new string('k', ToastLimits.MaxMetadataKeyLength + 1), 1));
	}

	[Fact]
	public void ToastLimits_ArePositive()
	{
		Assert.True(ToastLimits.MaxCaptionLength > 0);
		Assert.True(ToastLimits.MaxImageFileBytes > 8);
		Assert.True(ToastLimits.MaxImageDimension >= 64);
	}

	[Fact]
	public void IsPng_IsJpeg_ByteArrayOverloads()
	{
		Assert.False(ImageValidation.IsPng((byte[]?)null));
		Assert.False(ImageValidation.IsJpeg((byte[]?)null));
		Assert.True(ImageValidation.IsPng([137, 80, 78, 71]));
		Assert.True(ImageValidation.IsJpeg([0xFF, 0xD8, 0xFF, 0xE0]));
	}

	[Fact]
	public void ValidateImagePath_RejectsTinyFile_InvalidChars_AndDriveRoot()
	{
		var dir = Path.Combine(Path.GetTempPath(), "FuzzyToastSec2_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(dir);
		try
		{
			var tiny = Path.Combine(dir, "tiny.bin");
			File.WriteAllBytes(tiny, [1, 2]);
			Assert.False(ImageValidation.ValidateImagePath(tiny));
		}
		finally
		{
			try { Directory.Delete(dir, true); } catch { /* ignore */ }
		}

		Assert.False(ImageValidation.ValidateImagePath("bad|name.png"));
		Assert.False(ImageValidation.ValidateImagePath(@"C:\"));
		Assert.False(ImageValidation.ValidateImagePath(new string('a', 300)));
	}

	[Fact]
	public void ToastOptions_RejectsOtherOversizedFields()
	{
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", Description = new string('d', ToastLimits.MaxDescriptionLength + 1) }.Validate());
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", EnableInput = true, SubmitButtonText = "  " }.Validate());
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", SubmitButtonText = new string('s', ToastLimits.MaxSubmitButtonTextLength + 1) }.Validate());
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", InputPlaceholder = new string('p', ToastLimits.MaxInputTextLength + 1) }.Validate());
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", InputDefaultText = new string('t', ToastLimits.MaxInputTextLength + 1) }.Validate());

		var meta = new Dictionary<string, object?>();
		for (var i = 0; i < ToastLimits.MaxMetadataEntries + 1; i++)
			meta["k" + i] = i;
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "ok", Metadata = meta }.Validate());

		Assert.Throws<ArgumentException>(() =>
			new ToastOptions
			{
				Caption = "ok",
				Metadata = new Dictionary<string, object?> { [new string('k', ToastLimits.MaxMetadataKeyLength + 1)] = 1 }
			}.Validate());
	}

	[Fact]
	public void ToastBuilder_RejectsOversizedMetadataKey()
	{
		var area = new FuzzyToast.Layout.ScreenWorkingArea(0, 0, 100, 100);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FuzzyToast.Internal.FixedScreenProvider(area),
			new FuzzyToast.Internal.ImmediateUiMarshaler(),
			(opts, handle) => new FuzzyToast.Tests.Support.FakeToastView(handle));
		Assert.Throws<ArgumentException>(() =>
			mgr.Create().SetMetadata(new string('k', ToastLimits.MaxMetadataKeyLength + 1), 1));
	}
}
