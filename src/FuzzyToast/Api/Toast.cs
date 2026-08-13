using System.Drawing;
using System.Windows.Forms;
using FuzzyToast.Internal;

namespace FuzzyToast;

/// <summary>
/// Android-style toast entry point: <c>Toast.Build(owner, "Hello").Show()</c>.
/// Backed by a per-owner <see cref="ToastManager"/> (v2 layout, capacity, themes).
/// </summary>
public sealed class Toast
{
	private readonly Control _owner;
	private string _caption = string.Empty;
	private string _description = string.Empty;
	private Duration _duration = Duration.LENGTH_SHORT;
	private Animation _animation = Animation.FADE;
	private ToastPosition _position = ToastPosition.BottomRight;
	private ToastTheme _theme = ToastTheme.Dark;
	private ColorScheme? _customColors;
	private CloseStyle _closeStyle = CloseStyle.ButtonAndClickEntire;
	private bool _isMuted;
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
	private ToastHandle? _handle;

	private Toast(Control owner)
	{
		_owner = owner;
	}

	/// <summary>Unique id after <see cref="Show"/>; empty before show.</summary>
	public string Guid => _handle?.Id ?? string.Empty;

	/// <summary>Handle created by the last <see cref="Show"/> / <see cref="ShowAsync"/>; null if not shown or rejected without store.</summary>
	public ToastHandle? Handle => _handle;

	public string Caption
	{
		get => _caption;
		set => _caption = value ?? string.Empty;
	}

	public string Description
	{
		get => _description;
		set => _description = value?.Trim() ?? string.Empty;
	}

	public Duration Duration
	{
		get => _duration;
		set => _duration = value;
	}

	public Animation Animation
	{
		get => _animation;
		set => _animation = value;
	}

	public ToastPosition Position
	{
		get => _position;
		set => _position = value;
	}

	public ToastTheme Theme
	{
		get => _theme;
		set => _theme = value;
	}

	public bool IsMuted
	{
		get => _isMuted;
		set => _isMuted = value;
	}

	public Image? Thumbnail
	{
		get => _thumbnail;
		set => _thumbnail = value;
	}

	/// <summary>Arbitrary user data (same as <see cref="SetTag"/>).</summary>
	public object? Tag
	{
		get => _tag;
		set => _tag = value;
	}

	/// <summary>Snapshot of key/value metadata set via <see cref="SetMetadata"/> / <see cref="SetExtData"/>.</summary>
	public IReadOnlyDictionary<string, object?> Metadata => _metadata;

	/// <summary>Click on toast body. <see cref="ToastInteractionEventArgs"/> exposes Tag + Metadata.</summary>
	public event EventHandler<ToastInteractionEventArgs>? OnClick;

	/// <summary>Pointer hover. Args include Tag + Metadata.</summary>
	public event EventHandler<ToastInteractionEventArgs>? OnHover;

	/// <summary>User submitted text from an inputable toast (<see cref="EnableInput"/>).</summary>
	public event EventHandler<ToastSubmittedEventArgs>? OnSubmit;

	public event EventHandler? OnClosed;

	#region Fluent setters (optional; Build overloads cover common cases)

	public Toast SetCaption(string caption)
	{
		Caption = caption;
		return this;
	}

	public Toast SetDescription(string? description)
	{
		Description = description ?? string.Empty;
		return this;
	}

	public Toast SetDuration(Duration duration)
	{
		Duration = duration;
		return this;
	}

	public Toast SetAnimation(Animation animation)
	{
		Animation = animation;
		return this;
	}

	public Toast SetPosition(ToastPosition position)
	{
		Position = position;
		return this;
	}

	public Toast SetTheme(ToastTheme theme)
	{
		Theme = theme;
		return this;
	}

	public Toast SetCustomColors(Color background, Color foreground)
	{
		_theme = ToastTheme.Custom;
		_customColors = new ColorScheme(background, foreground);
		return this;
	}

	public Toast SetCloseStyle(CloseStyle style)
	{
		_closeStyle = style;
		return this;
	}

	public Toast SetMuting(bool muted = true)
	{
		IsMuted = muted;
		return this;
	}

	public Toast SetThumbnail(Image? image, bool ownsImage = false)
	{
		_thumbnail = image;
		_ownsThumbnail = ownsImage;
		return this;
	}

	/// <summary>Attach a single arbitrary payload (id, DTO, etc.). Available on click as <c>e.Tag</c>.</summary>
	public Toast SetTag(object? tag)
	{
		_tag = tag;
		return this;
	}

	/// <summary>Alias of <see cref="SetTag"/> for “user data / ext data” naming.</summary>
	public Toast SetData(object? data) => SetTag(data);

	/// <summary>Set or replace one metadata entry. Available on click as <c>e.Metadata[key]</c> / <c>e[key]</c>.</summary>
	public Toast SetMetadata(string key, object? value)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Metadata key is required.", nameof(key));
		if (key.Length > ToastLimits.MaxMetadataKeyLength)
			throw new ArgumentException($"Metadata key must be <= {ToastLimits.MaxMetadataKeyLength} characters.", nameof(key));
		_metadata[key] = value;
		return this;
	}

	/// <summary>Merge multiple metadata entries (overwrites existing keys).</summary>
	public Toast SetMetadata(IEnumerable<KeyValuePair<string, object?>> entries)
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
	public Toast SetExtData(string key, object? value) => SetMetadata(key, value);

	/// <summary>Alias of <see cref="SetMetadata(IEnumerable{KeyValuePair{string, object?}})"/>.</summary>
	public Toast SetExtData(IEnumerable<KeyValuePair<string, object?>> entries) => SetMetadata(entries);

	/// <summary>
	/// Enable quick-input mode: text box + submit button.
	/// By default stays open until Submit/Esc/close (<c>DurationMs = 0</c>).
	/// Call <see cref="SetDurationMs"/> if you want a safety auto-timeout.
	/// </summary>
	public Toast EnableInput(
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

	/// <summary>Disable or enable input mode without changing other input settings.</summary>
	public Toast SetInputable(bool enabled = true)
	{
		_enableInput = enabled;
		if (enabled && _duration is not Duration.Input && _durationMs is null)
			_duration = Duration.Input;
		return this;
	}

	/// <summary>
	/// Override auto-dismiss duration in milliseconds.
	/// Use <c>0</c> to disable auto-dismiss (toast stays until Submit / Esc / close).
	/// </summary>
	public Toast SetDurationMs(int milliseconds)
	{
		if (milliseconds < 0)
			throw new ArgumentOutOfRangeException(nameof(milliseconds));
		_durationMs = milliseconds;
		return this;
	}

	#endregion

	/// <summary>Display the toast (Android-style <c>show()</c>).</summary>
	public void Show()
	{
		var manager = ToastManagerRegistry.GetOrCreate(_owner);
		_handle = manager.Show(ToOptions());
		WireHandle(_handle);
	}

	/// <summary>
	/// Display asynchronously. Completes when the toast is shown (or rejected), not when dismissed.
	/// </summary>
	public async Task ShowAsync(CancellationToken cancellationToken = default)
	{
		var manager = ToastManagerRegistry.GetOrCreate(_owner);
		_handle = await manager.ShowAsync(ToOptions(), cancellationToken).ConfigureAwait(true);
		WireHandle(_handle);
	}

	/// <summary>
	/// Dismiss if showing. No-op if not yet shown or already closed (does not throw).
	/// </summary>
	public void Cancel() => _handle?.Dismiss();

	public void Dismiss() => Cancel();

	private void WireHandle(ToastHandle handle)
	{
		handle.Clicked += (_, e) => OnClick?.Invoke(this, e);
		handle.Hovered += (_, e) => OnHover?.Invoke(this, e);
		handle.Submitted += (_, e) => OnSubmit?.Invoke(this, e);
		handle.Dismissed += (_, e) => OnClosed?.Invoke(this, e);
	}

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
		IsMuted = _isMuted,
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

	private static Control RequireControl(IWin32Window window)
	{
		FuzzyToast.Internal.Guard.NotNull(window, nameof(window));
		if (window is Control control)
		{
			if (control.IsDisposed)
				throw new ObjectDisposedException(control.Name);
			return control;
		}

		throw new ArgumentException(
			"Toast owner must be a System.Windows.Forms.Control (typically your Form).",
			nameof(window));
	}

	#region Build overloads (Android-style factory — same surface as FuzzyToast 1.x)

	/// <summary>Build a simplest toast with caption only.</summary>
	public static Toast Build(IWin32Window window, string caption)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Description = string.Empty
		};
	}

	/// <summary>Build a toast with caption and description.</summary>
	public static Toast Build(IWin32Window window, string caption, string description)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Description = description?.Trim() ?? string.Empty
		};
	}

	/// <summary>Build with duration and animation.</summary>
	public static Toast Build(IWin32Window window, string caption, Duration duration, Animation animation)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Duration = duration,
			Animation = animation
		};
	}

	/// <summary>Build with description and duration.</summary>
	public static Toast Build(IWin32Window window, string caption, string description, Duration duration)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Description = description?.Trim() ?? string.Empty,
			Duration = duration
		};
	}

	/// <summary>Build with animation, duration, and mute flag.</summary>
	public static Toast Build(IWin32Window window, string caption, Animation animation, Duration duration, bool muting)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Animation = animation,
			Duration = duration,
			IsMuted = muting
		};
	}

	/// <summary>Build with custom animation.</summary>
	public static Toast Build(IWin32Window window, string caption, Animation animation)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Animation = animation
		};
	}

	/// <summary>Build with mute flag.</summary>
	public static Toast Build(IWin32Window window, string caption, bool muting)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			IsMuted = muting
		};
	}

	/// <summary>Build with thumbnail, duration, and animation.</summary>
	public static Toast Build(IWin32Window window, string caption, Image thumbnail, Duration duration, Animation animation)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration,
			Animation = animation
		};
	}

	/// <summary>Build with thumbnail, duration, animation, and mute.</summary>
	public static Toast Build(IWin32Window window, string caption, Image thumbnail, Duration duration,
		Animation animation, bool muting)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration,
			Animation = animation,
			IsMuted = muting
		};
	}

	/// <summary>Build with thumbnail.</summary>
	public static Toast Build(IWin32Window window, string caption, Image thumbnail)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail
		};
	}

	/// <summary>Build with thumbnail and duration.</summary>
	public static Toast Build(IWin32Window window, string caption, Image thumbnail, Duration duration)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration
		};
	}

	/// <summary>Build with duration only (Android LENGTH_SHORT / LENGTH_LONG style).</summary>
	public static Toast Build(IWin32Window window, string caption, Duration duration)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Duration = duration
		};
	}

	/// <summary>Build with theme.</summary>
	public static Toast Build(IWin32Window window, string caption, ToastTheme theme)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Theme = theme
		};
	}

	#endregion
}
