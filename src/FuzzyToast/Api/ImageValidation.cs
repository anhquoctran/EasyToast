using System.Drawing;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace FuzzyToast;

/// <summary>
/// Image path and size validation (pure rules; file IO for path checks).
/// Never decodes pixels from disk — only magic-byte probes — to avoid GDI+ parser bugs.
/// </summary>
public static class ImageValidation
{
	private static readonly string[] ReservedDeviceNames =
	[
		"CON", "PRN", "AUX", "NUL",
		"COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
		"LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
	];

	/// <summary>
	/// Thumbnail must meet minimum size on <b>both</b> width and height (v1 used OR — too loose)
	/// and stay under <see cref="ToastLimits.MaxImageDimension"/> unless overridden.
	/// </summary>
	public static bool ValidateImageSize(
		Image? image,
		int minWidth = 64,
		int minHeight = 64,
		int maxWidth = ToastLimits.MaxImageDimension,
		int maxHeight = ToastLimits.MaxImageDimension)
	{
		if (image is null)
			return false;

		try
		{
			var w = image.Width;
			var h = image.Height;
			return w >= minWidth && h >= minHeight && w <= maxWidth && h <= maxHeight;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Accepts PNG and JPEG signatures, including common JPEG APP0/APP1 (E0/E1) and generic SOI.
	/// Reads only the first 8 bytes. Rejects oversized files, reserved device names, and reparse points.
	/// </summary>
	public static bool ValidateImagePath(string? path)
	{
		if (!TryGetSafeExistingFile(path, out var fullPath))
			return false;

		try
		{
			using var stream = new FileStream(
				fullPath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: 16,
				FileOptions.SequentialScan);

			var header = new byte[8];
			var read = stream.Read(header, 0, header.Length);
			return IsPng(header, read) || IsJpeg(header, read);
		}
		catch
		{
			return false;
		}
	}

	public static bool IsPng(byte[]? bytes) =>
		bytes is not null && IsPng(bytes, bytes.Length);

	public static bool IsJpeg(byte[]? bytes) =>
		bytes is not null && IsJpeg(bytes, bytes.Length);

	public static bool IsPng(ReadOnlySpan<byte> bytes) => IsPng(bytes, bytes.Length);

	public static bool IsJpeg(ReadOnlySpan<byte> bytes) => IsJpeg(bytes, bytes.Length);

	private static bool IsPng(ReadOnlySpan<byte> bytes, int length)
	{
		// 89 50 4E 47
		return length >= 4
			&& bytes[0] == 137
			&& bytes[1] == 80
			&& bytes[2] == 78
			&& bytes[3] == 71;
	}

	private static bool IsJpeg(ReadOnlySpan<byte> bytes, int length)
	{
		// SOI + any marker: FF D8 FF ..
		return length >= 3
			&& bytes[0] == 0xFF
			&& bytes[1] == 0xD8
			&& bytes[2] == 0xFF;
	}

	internal static bool TryGetSafeExistingFile(
		string? path,
#if NET5_0_OR_GREATER
		[NotNullWhen(true)]
#endif
		out string? fullPath)
	{
		fullPath = null;
		if (string.IsNullOrWhiteSpace(path))
			return false;

		var candidate = path!;
		try
		{
			if (candidate.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				return false;

			var full = Path.GetFullPath(candidate);
			if (full.Length >= 260 && !full.StartsWith(@"\\?\", StringComparison.Ordinal))
				return false;

			if (full.StartsWith(@"\\.\", StringComparison.Ordinal))
				return false;

			var fileName = Path.GetFileName(full);
			var stem = Path.GetFileNameWithoutExtension(fileName);
			if (IsReservedDeviceName(stem) || IsReservedDeviceName(fileName))
				return false;

			if (!File.Exists(full))
				return false;

			var info = new FileInfo(full);
			if ((info.Attributes & FileAttributes.ReparsePoint) != 0)
				return false;
			if (info.Length < 3 || info.Length > ToastLimits.MaxImageFileBytes)
				return false;

			fullPath = full;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool IsReservedDeviceName(string? name)
	{
		if (string.IsNullOrEmpty(name))
			return false;
		foreach (var reserved in ReservedDeviceNames)
		{
			if (string.Equals(name, reserved, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}
}
