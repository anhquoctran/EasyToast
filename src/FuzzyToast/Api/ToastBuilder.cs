using System.Drawing;

namespace FuzzyToast;

/// <summary>
/// Fluent builder obtained from <see cref="ToastManager.Create"/>.
/// Call <see cref="Show"/> / <see cref="ShowAsync"/> to display, or <see cref="Build"/> to inspect options.
/// </summary>
/// <example>
/// <code>
/// manager.Create()
///     .SetCaption("Saved")
///     .SetTheme(ToastTheme.SuccessDark)
///     .Show();
/// </code>
/// </example>
public sealed class ToastBuilder
{
	private readonly ToastManager _manager;
	private string _caption = string.Empty;
	private string _description = string.Empty;
	private Duration _duration = Duration.Short;
	private Animation _animation = Animation.Fade;
	private ToastPosition _position = ToastPosition.BottomRight;
	private ToastTheme _theme = ToastTheme.Dark;
	private ColorScheme? _customColors;
	private CloseStyle _closeStyle = CloseStyle.ButtonAndClickEntire;
	private bool _muted;
	private Image? _thumbnail;
	private bool _ownsThumbnail;
	private object? _tag;
	private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);
	private bool _enableInput;
	private string _inputPlaceholder = string.Empty;
	private string _inputDefaultText = string.Empty;
	private string _submitButtonText = "OK";
	private bool _allowEmptySubmit;
	private int? _durationMs;

	internal ToastBuilder(ToastManager manager) => _manager = manager;

	/// <summary>Sets the title line. <see langword="null"/> becomes empty.</summary>
	public ToastBuilder SetCaption(string caption)
	{
		_caption = caption ?? string.Empty;
		return this;
	}

	/// <summary>Sets the secondary line (trimmed). <see langword="null"/> clears it.</summary>
	public ToastBuilder SetDescription(string? description)
	{
		_description = description?.Trim() ?? string.Empty;
		return this;
	}

	/// <summary>Sets the preset auto-dismiss duration.</summary>
	public ToastBuilder SetDuration(Duration duration)
	{
		_duration = duration;
		return this;
	}

	/// <summary>Sets the appear / dismiss animation.</summary>
	public ToastBuilder SetAnimation(Animation animation)
	{
		_animation = animation;
		return this;
	}

	/// <summary>Sets the corner stack.</summary>
	public ToastBuilder SetPosition(ToastPosition position)
	{
		_position = position;
		return this;
	}

	/// <summary>Sets a built-in theme.</summary>
	public ToastBuilder SetTheme(ToastTheme theme)
	{
		_theme = theme;
		return this;
	}

	/// <summary>Uses <see cref="ToastTheme.Custom"/> with the given RGB pair.</summary>
	public ToastBuilder SetCustomColors(Color background, Color foreground)
	{
		_theme = ToastTheme.Custom;
		_customColors = new ColorScheme(background, foreground);
		return this;
	}

	/// <summary>Uses <see cref="ToastTheme.Custom"/> with an existing <see cref="ColorScheme"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="scheme"/> is <see langword="null"/>.</exception>
	public ToastBuilder SetCustomColors(ColorScheme scheme)
	{
		_theme = ToastTheme.Custom;
		_customColors = scheme ?? throw new ArgumentNullException(nameof(scheme));
		return this;
	}

	/// <summary>Sets how the user can dismiss the toast.</summary>
	public ToastBuilder SetCloseStyle(CloseStyle style)
	{
		_closeStyle = style;
		return this;
	}

	/// <summary>Mutes the notification sound. Pass <see langword="false"/> to allow sound.</summary>
	public ToastBuilder SetMuting(bool muted = true)
	{
		_muted = muted;
		return this;
	}

	/// <summary>Sets the left thumbnail. When <paramref name="ownsImage"/> is <see langword="true"/>, the image is disposed on close.</summary>
	public ToastBuilder SetThumbnail(Image? image, bool ownsImage = false)
	{
		_thumbnail = image;
		_ownsThumbnail = ownsImage;
		return this;
	}

	/// <summary>Attaches an arbitrary payload available on click/hover/submit as <c>e.Tag</c>.</summary>
	public ToastBuilder SetTag(object? tag)
	{
		_tag = tag;
		return this;
	}

	/// <summary>Alias of <see cref="SetTag"/>.</summary>
	public ToastBuilder SetData(object? data) => SetTag(data);

	/// <summary>Sets one metadata entry. Empty keys throw; keys longer than <see cref="ToastLimits.MaxMetadataKeyLength"/> throw.</summary>
	public ToastBuilder SetMetadata(string key, object? value)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Metadata key is required.", nameof(key));
		if (key.Length > ToastLimits.MaxMetadataKeyLength)
			throw new ArgumentException($"Metadata key must be <= {ToastLimits.MaxMetadataKeyLength} characters.", nameof(key));
		_metadata[key] = value;
		return this;
	}

	/// <summary>Merges metadata entries (overwrites existing keys). Blank keys are skipped.</summary>
	public ToastBuilder SetMetadata(IEnumerable<KeyValuePair<string, object?>> entries)
	{
		FuzzyToast.Internal.Guard.NotNull(entries, nameof(entries));
		foreach (var pair in entries)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			_metadata[pair.Key] = pair.Value;
		}
		return this;
	}

	/// <summary>Alias of <see cref="SetMetadata(string, object?)"/>.</summary>
	public ToastBuilder SetExtData(string key, object? value) => SetMetadata(key, value);

	/// <summary>Alias of <see cref="SetMetadata(IEnumerable{KeyValuePair{string, object?}})"/>.</summary>
	public ToastBuilder SetExtData(IEnumerable<KeyValuePair<string, object?>> entries) => SetMetadata(entries);

	/// <summary>
	/// Enables the text box + submit button. Defaults to no auto-dismiss (<c>DurationMs = 0</c>).
	/// </summary>
	public ToastBuilder EnableInput(
		string? placeholder = null,
		string? defaultText = null,
		string submitButtonText = "OK",
		bool allowEmptySubmit = false)
	{
		_enableInput = true;
		_inputPlaceholder = placeholder?.Trim() ?? string.Empty;
		_inputDefaultText = defaultText ?? string.Empty;
		_submitButtonText = string.IsNullOrWhiteSpace(submitButtonText) ? "OK" : submitButtonText.Trim();
		_allowEmptySubmit = allowEmptySubmit;
		if (_duration is not Duration.Input && _durationMs is null)
			_duration = Duration.Input;
		// Stay open until user acts unless caller set an explicit timeout.
		_durationMs ??= 0;
		return this;
	}

	/// <summary>Turns input mode on or off without resetting placeholder / button text.</summary>
	public ToastBuilder SetInputable(bool enabled = true)
	{
		_enableInput = enabled;
		if (enabled && _duration is not Duration.Input && _durationMs is null)
			_duration = Duration.Input;
		return this;
	}

	/// <summary>Overrides auto-dismiss with an explicit millisecond value. <c>0</c> means stay open.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="milliseconds"/> is negative.</exception>
	public ToastBuilder SetDurationMs(int milliseconds)
	{
		if (milliseconds < 0)
			throw new ArgumentOutOfRangeException(nameof(milliseconds));
		_durationMs = milliseconds;
		return this;
	}

	/// <summary>Materializes the current configuration without showing a toast.</summary>
	public ToastOptions Build() => ToOptions();

	/// <summary>Validates and shows the toast on the owning <see cref="ToastManager"/>.</summary>
	public ToastHandle Show() => _manager.Show(ToOptions());

	/// <summary>Shows the toast asynchronously. Completes when shown or rejected, not when dismissed.</summary>
	public Task<ToastHandle> ShowAsync(CancellationToken cancellationToken = default)
		=> _manager.ShowAsync(ToOptions(), cancellationToken);

	private ToastOptions ToOptions() => new()
	{
		Caption = _caption,
		Description = _description,
		Duration = _duration,
		Animation = _animation,
		Position = _position,
		Theme = _theme,
		CustomColors = _customColors,
		CloseStyle = _closeStyle,
		IsMuted = _muted,
		Thumbnail = _thumbnail,
		OwnsThumbnail = _ownsThumbnail,
		Tag = _tag,
		Metadata = ToastOptions.FreezeMetadata(_metadata),
		EnableInput = _enableInput,
		InputPlaceholder = _inputPlaceholder,
		InputDefaultText = _inputDefaultText,
		SubmitButtonText = _submitButtonText,
		AllowEmptySubmit = _allowEmptySubmit,
		DurationMs = _durationMs
	};
}
