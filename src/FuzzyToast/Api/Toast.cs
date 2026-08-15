using FuzzyToast.Internal;

namespace FuzzyToast;

/// <summary>
/// Android-style toast entry point: <c>Toast.MakeText(owner, "Hello").Show()</c>.
/// Backed by a per-owner <see cref="ToastManager"/> (v2 layout, capacity, themes).
/// </summary>
public sealed class Toast
{
	private readonly Control? _owner;
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
	private bool _showProgressBar;
	private ToastHandle? _handle;

	private Toast(Control? owner)
	{
		_owner = owner;
	}

	/// <summary>Unique id after <see cref="Show"/>; empty before show.</summary>
	public string Guid => _handle?.Id ?? string.Empty;

	/// <summary>Handle created by the last <see cref="Show"/> / <see cref="ShowAsync"/>; null if not shown or rejected without store.</summary>
	public ToastHandle? Handle => _handle;

	/// <summary>Title line shown in bold. Required before <see cref="Show"/>.</summary>
	public string Caption
	{
		get => _caption;
		set => _caption = value ?? string.Empty;
	}

	/// <summary>Optional secondary line. Leading/trailing whitespace is trimmed.</summary>
	public string Description
	{
		get => _description;
		set => _description = value?.Trim() ?? string.Empty;
	}

	/// <summary>Preset auto-dismiss length. Overridden by <see cref="SetDurationMs"/>.</summary>
	public Duration Duration
	{
		get => _duration;
		set => _duration = value;
	}

	/// <summary>Appear / dismiss animation. Default is <see cref="Animation.Fade"/>.</summary>
	public Animation Animation
	{
		get => _animation;
		set => _animation = value;
	}

	/// <summary>Corner stack. Each corner is independent.</summary>
	public ToastPosition Position
	{
		get => _position;
		set => _position = value;
	}

	/// <summary>Built-in palette. Use <see cref="SetCustomColors"/> for <see cref="ToastTheme.Custom"/>.</summary>
	public ToastTheme Theme
	{
		get => _theme;
		set => _theme = value;
	}

	/// <summary>When <see langword="true"/>, the notification sound is not played.</summary>
	public bool IsMuted
	{
		get => _isMuted;
		set => _isMuted = value;
	}

	/// <summary>Optional thumbnail shown on the left. Must stay within <see cref="ToastLimits"/> if set.</summary>
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

	/// <summary>Snapshot of key/value metadata set via <see cref="SetMetadata(string, object?)"/> / <see cref="SetExtData(string, object?)"/>.</summary>
	public IReadOnlyDictionary<string, object?> Metadata => _metadata;

	/// <summary>When true, displays a progress bar that slides to 0 over the duration.</summary>
	public bool ShowProgressBar
	{
		get => _showProgressBar;
		set => _showProgressBar = value;
	}

	/// <summary>Click on toast body. <see cref="ToastInteractionEventArgs"/> exposes Tag + Metadata.</summary>
	public event EventHandler<ToastInteractionEventArgs>? OnClick;

	/// <summary>Pointer hover. Args include Tag + Metadata.</summary>
	public event EventHandler<ToastInteractionEventArgs>? OnHover;

	/// <summary>User submitted text from an inputable toast (<see cref="EnableInput"/>).</summary>
	public event EventHandler<ToastSubmittedEventArgs>? OnSubmit;

	/// <summary>Raised after the toast is dismissed (timer, user, or <see cref="Dismiss"/>).</summary>
	public event EventHandler? OnClosed;

	#region Fluent setters (optional; Build overloads cover common cases)

	/// <summary>Sets <see cref="Caption"/> and returns this instance for chaining.</summary>
	/// <param name="caption">Title text. <see langword="null"/> becomes empty.</param>
	public Toast SetCaption(string caption)
	{
		Caption = caption;
		return this;
	}

	/// <summary>Sets <see cref="Description"/> and returns this instance.</summary>
	/// <param name="description">Secondary text; trimmed. <see langword="null"/> clears it.</param>
	public Toast SetDescription(string? description)
	{
		Description = description ?? string.Empty;
		return this;
	}

	/// <summary>Sets the preset <see cref="Duration"/>.</summary>
	public Toast SetDuration(Duration duration)
	{
		Duration = duration;
		return this;
	}

	/// <summary>Sets the appear / dismiss <see cref="Animation"/>.</summary>
	public Toast SetAnimation(Animation animation)
	{
		Animation = animation;
		return this;
	}

	/// <summary>Sets the corner <see cref="Position"/>.</summary>
	public Toast SetPosition(ToastPosition position)
	{
		Position = position;
		return this;
	}

	/// <summary>Sets the built-in <see cref="Theme"/>.</summary>
	public Toast SetTheme(ToastTheme theme)
	{
		Theme = theme;
		return this;
	}

	/// <summary>Uses <see cref="ToastTheme.Custom"/> with the given background and foreground colors.</summary>
	public Toast SetCustomColors(Color background, Color foreground)
	{
		_theme = ToastTheme.Custom;
		_customColors = new ColorScheme(background, foreground);
		return this;
	}

	/// <summary>Sets how the user can dismiss the toast.</summary>
	public Toast SetCloseStyle(CloseStyle style)
	{
		_closeStyle = style;
		return this;
	}

	/// <summary>Mutes the notification sound. Pass <see langword="false"/> to allow sound again.</summary>
	/// <param name="muted">Default <see langword="true"/>.</param>
	public Toast SetMuting(bool muted = true)
	{
		IsMuted = muted;
		return this;
	}

	/// <summary>Sets the left thumbnail.</summary>
	/// <param name="image">Image to display; <see langword="null"/> hides the thumbnail panel.</param>
	/// <param name="ownsImage">When <see langword="true"/>, the toast disposes <paramref name="image"/> after close.</param>
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
	/// <param name="placeholder">Cue text inside the empty text box.</param>
	/// <param name="defaultText">Initial contents of the text box.</param>
	/// <param name="submitButtonText">Button label (blank falls back to <c>OK</c>).</param>
	/// <param name="allowEmptySubmit">When <see langword="false"/>, empty/whitespace submit is ignored.</param>
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
	/// <param name="milliseconds">Must be ≥ 0 and ≤ <see cref="ToastLimits.MaxDurationMs"/> when shown.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="milliseconds"/> is negative.</exception>
	public Toast SetDurationMs(int milliseconds)
	{
		if (milliseconds < 0)
			throw new ArgumentOutOfRangeException(nameof(milliseconds));
		_durationMs = milliseconds;
		return this;
	}

	/// <summary>Enables or disables the auto-dismiss progress bar.</summary>
	public Toast SetShowProgressBar(bool show = true)
	{
		_showProgressBar = show;
		return this;
	}

	#endregion

	/// <summary>
	/// Displays the toast on the owner's stack (Android-style <c>show()</c>).
	/// Uses the shared <see cref="ToastManager"/> for <c>owner</c>.
	/// </summary>
	/// <exception cref="ArgumentException">Caption is missing or options fail <see cref="ToastOptions.Validate"/>.</exception>
	public void Show()
	{
		var manager = _owner != null ? ToastManagerRegistry.GetOrCreate(_owner) : ToastManager.Default;
		_handle = manager.Show(ToOptions());
		WireHandle(_handle);
	}

	/// <summary>
	/// Displays the toast asynchronously. The task completes when the toast is <em>shown</em>
	/// (or rejected), not when it is dismissed — use <see cref="ToastHandle.WhenDismissed"/> for that.
	/// </summary>
	/// <param name="cancellationToken">If cancelled after show, the toast is dismissed.</param>
	public async Task ShowAsync(CancellationToken cancellationToken = default)
	{
		var manager = _owner != null ? ToastManagerRegistry.GetOrCreate(_owner) : ToastManager.Default;
		_handle = await manager.ShowAsync(ToOptions(), cancellationToken).ConfigureAwait(true);
		WireHandle(_handle);
	}

	/// <summary>
	/// Dismiss if showing. No-op if not yet shown or already closed (does not throw).
	/// </summary>
	public void Cancel() => _handle?.Dismiss();

	/// <summary>Same as <see cref="Cancel"/>.</summary>
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
		DurationMs = _durationMs,
		ShowProgressBar = _showProgressBar
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

	/// <summary>Creates a toast with a caption only. Call <see cref="Show"/> to display it.</summary>
	/// <param name="window">Owner form (must be a <see cref="Control"/>).</param>
	/// <param name="caption">Title text.</param>
	public static Toast MakeText(IWin32Window window, string caption)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Description = string.Empty
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption) => MakeText(window, caption);

	/// <summary>Build a toast with caption and description.</summary>
	public static Toast MakeText(IWin32Window window, string caption, string description)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Description = description?.Trim() ?? string.Empty
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, string description) => MakeText(window, caption, description);

	/// <summary>Build with duration and animation.</summary>
	public static Toast MakeText(IWin32Window window, string caption, Duration duration, Animation animation)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Duration = duration,
			Animation = animation
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Duration duration, Animation animation) => MakeText(window, caption, duration, animation);

	/// <summary>Build with description and duration.</summary>
	public static Toast MakeText(IWin32Window window, string caption, string description, Duration duration)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Description = description?.Trim() ?? string.Empty,
			Duration = duration
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, string description, Duration duration) => MakeText(window, caption, description, duration);

	/// <summary>Build with animation, duration, and mute flag.</summary>
	public static Toast MakeText(IWin32Window window, string caption, Animation animation, Duration duration, bool muting)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Animation = animation,
			Duration = duration,
			IsMuted = muting
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Animation animation, Duration duration, bool muting) => MakeText(window, caption, animation, duration, muting);

	/// <summary>Build with custom animation.</summary>
	public static Toast MakeText(IWin32Window window, string caption, Animation animation)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Animation = animation
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Animation animation) => MakeText(window, caption, animation);

	/// <summary>Build with mute flag.</summary>
	public static Toast MakeText(IWin32Window window, string caption, bool muting)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			IsMuted = muting
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, bool muting) => MakeText(window, caption, muting);

	/// <summary>Build with thumbnail, duration, and animation.</summary>
	public static Toast MakeText(IWin32Window window, string caption, Image thumbnail, Duration duration, Animation animation)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration,
			Animation = animation
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Image thumbnail, Duration duration, Animation animation) => MakeText(window, caption, thumbnail, duration, animation);

	/// <summary>Build with thumbnail, duration, animation, and mute.</summary>
	public static Toast MakeText(IWin32Window window, string caption, Image thumbnail, Duration duration,
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

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
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
	public static Toast MakeText(IWin32Window window, string caption, Image thumbnail)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Image thumbnail) => MakeText(window, caption, thumbnail);

	/// <summary>Build with thumbnail and duration.</summary>
	public static Toast MakeText(IWin32Window window, string caption, Image thumbnail, Duration duration)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Image thumbnail, Duration duration) => MakeText(window, caption, thumbnail, duration);

	/// <summary>Build with duration only (Android LENGTH_SHORT / LENGTH_LONG style).</summary>
	public static Toast MakeText(IWin32Window window, string caption, Duration duration)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Duration = duration
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, Duration duration) => MakeText(window, caption, duration);

	/// <summary>Build with theme.</summary>
	public static Toast MakeText(IWin32Window window, string caption, ToastTheme theme)
	{
		return new Toast(RequireControl(window))
		{
			Caption = caption ?? string.Empty,
			Theme = theme
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(IWin32Window window, string caption, ToastTheme theme) => MakeText(window, caption, theme);

	#endregion

	#region Build overloads (Global default manager)

	/// <summary>Creates a toast using the global <see cref="ToastManager.Default"/>. Call <see cref="Show"/> to display it.</summary>
	public static Toast MakeText(string caption)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Description = string.Empty
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption) => MakeText(caption);

	/// <summary>Build a global toast with caption and description.</summary>
	public static Toast MakeText(string caption, string description)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Description = description?.Trim() ?? string.Empty
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, string description) => MakeText(caption, description);

	/// <summary>Build a global toast with duration and animation.</summary>
	public static Toast MakeText(string caption, Duration duration, Animation animation)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Duration = duration,
			Animation = animation
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Duration duration, Animation animation) => MakeText(caption, duration, animation);

	/// <summary>Build a global toast with description and duration.</summary>
	public static Toast MakeText(string caption, string description, Duration duration)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Description = description?.Trim() ?? string.Empty,
			Duration = duration
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, string description, Duration duration) => MakeText(caption, description, duration);

	/// <summary>Build a global toast with animation, duration, and mute flag.</summary>
	public static Toast MakeText(string caption, Animation animation, Duration duration, bool muting)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Animation = animation,
			Duration = duration,
			IsMuted = muting
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Animation animation, Duration duration, bool muting) => MakeText(caption, animation, duration, muting);

	/// <summary>Build a global toast with custom animation.</summary>
	public static Toast MakeText(string caption, Animation animation)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Animation = animation
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Animation animation) => MakeText(caption, animation);

	/// <summary>Build a global toast with mute flag.</summary>
	public static Toast MakeText(string caption, bool muting)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			IsMuted = muting
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, bool muting) => MakeText(caption, muting);

	/// <summary>Build a global toast with thumbnail, duration, and animation.</summary>
	public static Toast MakeText(string caption, Image thumbnail, Duration duration, Animation animation)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration,
			Animation = animation
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Image thumbnail, Duration duration, Animation animation) => MakeText(caption, thumbnail, duration, animation);

	/// <summary>Build a global toast with thumbnail, duration, animation, and mute.</summary>
	public static Toast MakeText(string caption, Image thumbnail, Duration duration, Animation animation, bool muting)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration,
			Animation = animation,
			IsMuted = muting
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Image thumbnail, Duration duration, Animation animation, bool muting) => MakeText(caption, thumbnail, duration, animation, muting);

	/// <summary>Build a global toast with thumbnail.</summary>
	public static Toast MakeText(string caption, Image thumbnail)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Image thumbnail) => MakeText(caption, thumbnail);

	/// <summary>Build a global toast with thumbnail and duration.</summary>
	public static Toast MakeText(string caption, Image thumbnail, Duration duration)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Thumbnail = thumbnail,
			Duration = duration
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Image thumbnail, Duration duration) => MakeText(caption, thumbnail, duration);

	/// <summary>Build a global toast with duration only.</summary>
	public static Toast MakeText(string caption, Duration duration)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Duration = duration
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, Duration duration) => MakeText(caption, duration);

	/// <summary>Build a global toast with theme.</summary>
	public static Toast MakeText(string caption, ToastTheme theme)
	{
		return new Toast(null)
		{
			Caption = caption ?? string.Empty,
			Theme = theme
		};
	}

	[Obsolete("Use MakeText instead. This method will be removed in the next major version.", false)]
	public static Toast Build(string caption, ToastTheme theme) => MakeText(caption, theme);
	#endregion
}
