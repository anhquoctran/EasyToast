using System.Drawing;

namespace FuzzyToast;

/// <summary>
/// Image path and size validation (pure rules; file IO for path checks).
/// </summary>
public static class ImageValidation
{
	/// <summary>
	/// Thumbnail must meet minimum size on <b>both</b> width and height (v1 used OR — too loose).
	/// </summary>
	public static bool ValidateImageSize(Image? image, int minWidth = 64, int minHeight = 64)
	{
		if (image is null)
			return false;

		try
		{
			return image.Width >= minWidth && image.Height >= minHeight;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Accepts PNG and JPEG signatures, including common JPEG APP0/APP1 (E0/E1) and generic SOI.
	/// </summary>
	public static bool ValidateImagePath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			return false;

		try
		{
			var bytes = File.ReadAllBytes(path);
			return IsPng(bytes) || IsJpeg(bytes);
		}
		catch
		{
			return false;
		}
	}

	public static bool IsPng(ReadOnlySpan<byte> bytes)
	{
		// 89 50 4E 47
		ReadOnlySpan<byte> png = [137, 80, 78, 71];
		return bytes.Length >= png.Length && bytes[..png.Length].SequenceEqual(png);
	}

	public static bool IsJpeg(ReadOnlySpan<byte> bytes)
	{
		// SOI + any marker: FF D8 FF ..
		if (bytes.Length < 3)
			return false;
		return bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
	}
}
