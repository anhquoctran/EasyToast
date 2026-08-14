namespace FuzzyToast;

/// <summary>
/// Static entry point providing a default ToastManager for quick scenarios.
/// Use this for simple applications that don't need per-form manager instances.
/// For production apps, prefer creating explicit ToastManager instances per form.
/// </summary>
public static class ToastService
{
	private static ToastManager? _defaultManager;
	private static readonly object _lock = new();

	/// <summary>
	/// Gets or creates the default ToastManager bound to the specified form.
	/// Only one default manager can exist at a time.
	/// </summary>
	public static ToastManager Default
	{
		get
		{
			if (_defaultManager == null || _defaultManager.IsDisposed)
			{
				throw new InvalidOperationException(
					"Default ToastManager has not been initialized. " +
					"Call InitializeDefault() with your main form first.");
			}
			return _defaultManager;
		}
	}

	/// <summary>
	/// Initializes the default ToastManager with the specified owner form.
	/// Call this once at application startup with your main form.
	/// </summary>
	/// <param name="mainForm">The main application form.</param>
	/// <param name="options">Optional custom manager options.</param>
	/// <returns>The initialized ToastManager instance.</returns>
	public static ToastManager InitializeDefault(Form mainForm, ToastManagerOptions? options = null)
	{
		Guard.NotNull(mainForm, nameof(mainForm));
		
		lock (_lock)
		{
			_defaultManager?.Dispose();
			_defaultManager = new ToastManager(mainForm, options);
			return _defaultManager;
		}
	}

	/// <summary>
	/// Shows a toast using the default manager.
	/// </summary>
	/// <param name="caption">The toast title.</param>
	/// <param name="description">Optional description.</param>
	/// <returns>The toast handle.</returns>
	public static ToastHandle Show(string caption, string? description = null)
	{
		var manager = Default;
		if (string.IsNullOrEmpty(description))
			return manager.Create().SetCaption(caption).Show();
		return manager.Create().SetCaption(caption).SetDescription(description).Show();
	}

	/// <summary>
	/// Shows a toast with custom configuration using the default manager.
	/// </summary>
	/// <param name="configure">Action to configure the toast builder.</param>
	/// <returns>The toast handle.</returns>
	public static ToastHandle Show(Action<ToastBuilder> configure)
	{
		Guard.NotNull(configure, nameof(configure));
		var manager = Default;
		var builder = manager.Create();
		configure(builder);
		return builder.Show();
	}

	/// <summary>
	/// Shows an inputable toast using the default manager.
	/// </summary>
	/// <param name="caption">The toast title.</param>
	/// <param name="onSubmit">Callback when user submits text.</param>
	/// <param name="placeholder">Optional placeholder text.</param>
	/// <returns>The toast handle.</returns>
	public static ToastHandle ShowInput(string caption, Action<string> onSubmit, string? placeholder = null)
	{
		Guard.NotNull(onSubmit, nameof(onSubmit));
		var manager = Default;
		var toast = manager.Create()
			.SetCaption(caption)
			.EnableInput(placeholder);
		toast.Show();
		toast.Handle!.Submitted += (_, e) => onSubmit(e.Text);
		return toast.Handle!;
	}

	/// <summary>
	/// Dismisses all visible toasts from the default manager.
	/// </summary>
	public static void DismissAll()
	{
		_defaultManager?.DismissAll();
	}

	/// <summary>
	/// Resets the default manager, disposing any existing instance.
	/// Call this when switching main forms or during cleanup.
	/// </summary>
	public static void Reset()
	{
		lock (_lock)
		{
			_defaultManager?.Dispose();
			_defaultManager = null;
		}
	}

	/// <summary>
	/// Checks if the default manager is initialized and active.
	/// </summary>
	public static bool IsInitialized => _defaultManager != null && !_defaultManager.IsDisposed;
}
