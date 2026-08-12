using System.Drawing;
using Xunit;

namespace FuzzyToast.Tests;

public class ImageValidationTests
{
	[Fact]
	public void T17_ValidateImageSize_RequiresBothDimensions()
	{
		using var tooNarrow = new Bitmap(64, 32);
		using var tooShort = new Bitmap(32, 64);
		using var ok = new Bitmap(64, 64);

		Assert.False(ImageValidation.ValidateImageSize(tooNarrow));
		Assert.False(ImageValidation.ValidateImageSize(tooShort));
		Assert.True(ImageValidation.ValidateImageSize(ok));
	}

	[Fact]
	public void T18_IsJpeg_AcceptsExifApp1()
	{
		// FF D8 FF E1 — common EXIF JPEG
		ReadOnlySpan<byte> exif = [0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x10];
		Assert.True(ImageValidation.IsJpeg(exif));
	}

	[Fact]
	public void IsJpeg_AcceptsJfifApp0()
	{
		ReadOnlySpan<byte> jfif = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
		Assert.True(ImageValidation.IsJpeg(jfif));
	}

	[Fact]
	public void IsPng_RequiresSignature()
	{
		ReadOnlySpan<byte> png = [137, 80, 78, 71, 13, 10, 26, 10];
		Assert.True(ImageValidation.IsPng(png));
		Assert.False(ImageValidation.IsPng([1, 2, 3, 4]));
	}
}
