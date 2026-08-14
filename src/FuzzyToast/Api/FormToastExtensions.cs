using System.Drawing;
using System.Windows.Forms;
using FuzzyToast.Internal;

namespace FuzzyToast;

/// <summary>
/// Form helpers that show a toast without <c>Toast.Build(this, …).Show()</c>.
/// </summary>
/// <example>
/// <code>
/// this.ShowToast("Hello");
/// this.ShowSuccess("Saved", "All changes written.");
/// this.ShowError("Could not save");
/// this.Toast("Order #42").SetTheme(ToastTheme.PrimaryDark).Show();
/// </code>
/// </example>
public static class FormToastExtensions
{
	/// <summary>Shows a toast with a caption only.</summary>
	/// <param name="form">Owner form. Must not be disposed.</param>
	/// <param name="caption">Title text.</param>
	/// <returns>Handle from <see cref="Toast.Show"/>.</returns>
	public static ToastHandle ShowToast(this Form form, string caption)
		=> ShowCore(form, caption);

	/// <summary>Shows a toast with caption and description.</summary>
	public static ToastHandle ShowToast(this Form form, string caption, string description)
		=> ShowCore(form, caption, description);

	/// <summary>Shows a toast with a preset duration.</summary>
	public static ToastHandle ShowToast(this Form form, string caption, Duration duration)
		=> ShowCore(form, caption, duration: duration);

	/// <summary>Shows a toast with a built-in theme.</summary>
	public static ToastHandle ShowToast(this Form form, string caption, ToastTheme theme)
		=> ShowCore(form, caption, theme: theme);

	/// <summary>Shows a toast with caption, description, and duration.</summary>
	public static ToastHandle ShowToast(this Form form, string caption, string description, Duration duration)
		=> ShowCore(form, caption, description, duration);

	/// <summary>Shows a toast with caption, description, and theme.</summary>
	public static ToastHandle ShowToast(this Form form, string caption, string description, ToastTheme theme)
		=> ShowCore(form, caption, description, theme: theme);

	/// <summary>Shows a toast with caption, description, duration, and theme.</summary>
	public static ToastHandle ShowToast(
		this Form form,
		string caption,
		string description,
		Duration duration,
		ToastTheme theme)
		=> ShowCore(form, caption, description, duration, theme);

	/// <summary>Shows a toast with a left thumbnail.</summary>
	public static ToastHandle ShowToast(this Form form, string caption, Image thumbnail)
		=> ShowCore(form, caption, thumbnail: thumbnail);

	/// <summary>
	/// Shows a toast after applying extra fluent configuration
	/// (theme, position, mute, events, input, …).
	/// </summary>
	/// <param name="form">Owner form.</param>
	/// <param name="caption">Title text.</param>
	/// <param name="configure">Called before <see cref="Toast.Show"/>. Must not be <see langword="null"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
	public static ToastHandle ShowToast(this Form form, string caption, Action<Toast> configure)
	{
		Guard.NotNull(configure, nameof(configure));
		return ShowCore(form, caption, configure: configure);
	}

	/// <summary>Shows a toast with description, then applies extra fluent configuration.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
	public static ToastHandle ShowToast(
		this Form form,
		string caption,
		string description,
		Action<Toast> configure)
	{
		Guard.NotNull(configure, nameof(configure));
		return ShowCore(form, caption, description, configure: configure);
	}

	/// <summary>Shows a success-themed toast (<see cref="ToastTheme.SuccessDark"/>).</summary>
	public static ToastHandle ShowSuccess(
		this Form form,
		string caption,
		string? description = null,
		Duration duration = Duration.Short)
		=> ShowCore(form, caption, description, duration, ToastTheme.SuccessDark);

	/// <summary>Shows an error-themed toast (<see cref="ToastTheme.ErrorDark"/>).</summary>
	public static ToastHandle ShowError(
		this Form form,
		string caption,
		string? description = null,
		Duration duration = Duration.Short)
		=> ShowCore(form, caption, description, duration, ToastTheme.ErrorDark);

	/// <summary>Shows a warning-themed toast (<see cref="ToastTheme.WarningDark"/>).</summary>
	public static ToastHandle ShowWarning(
		this Form form,
		string caption,
		string? description = null,
		Duration duration = Duration.Short)
		=> ShowCore(form, caption, description, duration, ToastTheme.WarningDark);

	/// <summary>Shows an info-themed toast (<see cref="ToastTheme.PrimaryDark"/>).</summary>
	public static ToastHandle ShowInfo(
		this Form form,
		string caption,
		string? description = null,
		Duration duration = Duration.Short)
		=> ShowCore(form, caption, description, duration, ToastTheme.PrimaryDark);

	/// <summary>
	/// Creates a toast bound to this form. Call <see cref="Toast.Show"/> to display it.
	/// Same as <see cref="Toast.Build(IWin32Window, string)"/>.
	/// </summary>
	public static Toast Toast(this Form form, string caption)
	{
		Guard.NotNull(form, nameof(form));
		return FuzzyToast.Toast.Build(form, caption);
	}

	/// <summary>
	/// Creates a toast with caption and description. Call <see cref="Toast.Show"/> to display it.
	/// Same as <see cref="Toast.Build(IWin32Window, string, string)"/>.
	/// </summary>
	public static Toast Toast(this Form form, string caption, string description)
	{
		Guard.NotNull(form, nameof(form));
		return FuzzyToast.Toast.Build(form, caption, description);
	}

	private static ToastHandle ShowCore(
		Form form,
		string caption,
		string? description = null,
		Duration? duration = null,
		ToastTheme? theme = null,
		Image? thumbnail = null,
		Action<Toast>? configure = null)
	{
		Guard.NotNull(form, nameof(form));

		var toast = FuzzyToast.Toast.Build(form, caption);
		if (description is not null)
		{
			toast.SetDescription(description);
		}

		if (duration is not null)
		{
			toast.SetDuration(duration.Value);
		}

		if (theme is not null)
		{
			toast.SetTheme(theme.Value);
		}

		if (thumbnail is not null)
		{
			toast.SetThumbnail(thumbnail);
		}

		configure?.Invoke(toast);
		toast.Show();
		return toast.Handle!;
	}
}
