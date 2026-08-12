using System.Drawing;

namespace FuzzyToast;

/// <summary>Fluent builder for <see cref="ToastOptions"/> and show helpers.</summary>
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

	public ToastBuilder SetCaption(string caption)
	{
		_caption = caption ?? string.Empty;
		return this;
	}

	public ToastBuilder SetDescription(string? description)
	{
		_description = description?.Trim() ?? string.Empty;
		return this;
	}

	public ToastBuilder SetDuration(Duration duration)
	{
		_duration = duration;
		return this;
	}

	public ToastBuilder SetAnimation(Animation animation)
	{
		_animation = animation;
		return this;
	}

	public ToastBuilder SetPosition(ToastPosition position)
	{
		_position = position;
		return this;
	}

	public ToastBuilder SetTheme(ToastTheme theme)
	{
		_theme = theme;
		return this;
	}

	public ToastBuilder SetCustomColors(Color background, Color foreground)
	{
		_theme = ToastTheme.Custom;
		_customColors = new ColorScheme(background, foreground);
		return this;
	}

	public ToastBuilder SetCustomColors(ColorScheme scheme)
	{
		_theme = ToastTheme.Custom;
		_customColors = scheme ?? throw new ArgumentNullException(nameof(scheme));
		return this;
	}

	public ToastBuilder SetCloseStyle(CloseStyle style)
	{
		_closeStyle = style;
		return this;
	}

	public ToastBuilder SetMuting(bool muted = true)
	{
		_muted = muted;
		return this;
	}

	public ToastBuilder SetThumbnail(Image? image, bool ownsImage = false)
	{
		_thumbnail = image;
		_ownsThumbnail = ownsImage;
		return this;
	}

	public ToastBuilder SetTag(object? tag)
	{
		_tag = tag;
		return this;
	}

	/// <summary>Alias of <see cref="SetTag"/>.</summary>
	public ToastBuilder SetData(object? data) => SetTag(data);

	public ToastBuilder SetMetadata(string key, object? value)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Metadata key is required.", nameof(key));
		_metadata[key] = value;
		return this;
	}

	public ToastBuilder SetMetadata(IEnumerable<KeyValuePair<string, object?>> entries)
	{
		ArgumentNullException.ThrowIfNull(entries);
		foreach (var (key, value) in entries)
		{
			if (string.IsNullOrWhiteSpace(key))
				continue;
			_metadata[key] = value;
		}
		return this;
	}

	public ToastBuilder SetExtData(string key, object? value) => SetMetadata(key, value);

	public ToastBuilder SetExtData(IEnumerable<KeyValuePair<string, object?>> entries) => SetMetadata(entries);

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
		return this;
	}

	public ToastBuilder SetInputable(bool enabled = true)
	{
		_enableInput = enabled;
		if (enabled && _duration is not Duration.Input && _durationMs is null)
			_duration = Duration.Input;
		return this;
	}

	public ToastBuilder SetDurationMs(int milliseconds)
	{
		if (milliseconds < 1)
			throw new ArgumentOutOfRangeException(nameof(milliseconds));
		_durationMs = milliseconds;
		return this;
	}

	public ToastOptions Build() => ToOptions();

	public ToastHandle Show() => _manager.Show(ToOptions());

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
