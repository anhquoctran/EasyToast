namespace FuzzyToast;

/// <summary>
/// Hard limits that keep toast content and image probes from becoming a resource-exhaustion vector.
/// <see cref="ToastOptions.Validate"/> and <see cref="ImageValidation"/> enforce these values.
/// </summary>
public static class ToastLimits
{
	/// <summary>Maximum length of <see cref="ToastOptions.Caption"/> (characters).</summary>
	public const int MaxCaptionLength = 1024;

	/// <summary>Maximum length of <see cref="ToastOptions.Description"/> (characters).</summary>
	public const int MaxDescriptionLength = 4096;

	/// <summary>Maximum length of input placeholder, default text, and typed submit text.</summary>
	public const int MaxInputTextLength = 2000;

	/// <summary>Maximum length of the submit-button label.</summary>
	public const int MaxSubmitButtonTextLength = 32;

	/// <summary>Maximum number of metadata entries kept after <see cref="ToastOptions.FreezeMetadata"/>.</summary>
	public const int MaxMetadataEntries = 64;

	/// <summary>Maximum length of a metadata key (characters).</summary>
	public const int MaxMetadataKeyLength = 128;

	/// <summary>Maximum explicit auto-dismiss duration (24 hours, in milliseconds).</summary>
	public const int MaxDurationMs = 86_400_000;

	/// <summary>Maximum thumbnail width or height in pixels.</summary>
	public const int MaxImageDimension = 4096;

	/// <summary>Minimum thumbnail width or height in pixels when a thumbnail is supplied.</summary>
	public const int MinImageDimension = 1;

	/// <summary>Maximum file size accepted by <see cref="ImageValidation.ValidateImagePath"/> (8 MiB).</summary>
	public const long MaxImageFileBytes = 8L * 1024 * 1024;
}
