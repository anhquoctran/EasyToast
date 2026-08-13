namespace FuzzyToast;

/// <summary>
/// Hard limits that keep toast content and image probes from becoming a resource-exhaustion vector.
/// </summary>
public static class ToastLimits
{
	public const int MaxCaptionLength = 1024;
	public const int MaxDescriptionLength = 4096;
	public const int MaxInputTextLength = 2000;
	public const int MaxSubmitButtonTextLength = 32;
	public const int MaxMetadataEntries = 64;
	public const int MaxMetadataKeyLength = 128;
	public const int MaxDurationMs = 86_400_000;
	public const int MaxImageDimension = 4096;
	public const int MinImageDimension = 1;
	public const long MaxImageFileBytes = 8L * 1024 * 1024;
}
