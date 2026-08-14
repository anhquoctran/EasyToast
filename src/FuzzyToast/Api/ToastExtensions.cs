using System.Windows.Forms;

namespace FuzzyToast;

/// <summary>
/// Extension methods for Windows Forms to simplify toast notifications.
/// Provides fluent API directly on Form and Control instances.
/// </summary>
public static class ToastExtensions
{
    /// <summary>
    /// Shows a simple toast notification with caption only.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowToast(this Form form, string caption)
    {
        Guard.NotNull(form, nameof(form));
        return Toast.Build(form, caption).Show();
    }

    /// <summary>
    /// Shows a toast notification with caption and description.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <param name="description">The secondary text.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowToast(this Form form, string caption, string description)
    {
        Guard.NotNull(form, nameof(form));
        return Toast.Build(form, caption, description).Show();
    }

    /// <summary>
    /// Shows a toast notification with custom duration.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <param name="duration">The display duration.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowToast(this Form form, string caption, Duration duration)
    {
        Guard.NotNull(form, nameof(form));
        return Toast.Build(form, caption).SetDuration(duration).Show();
    }

    /// <summary>
    /// Shows a toast notification with custom theme.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <param name="theme">The color theme.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowToast(this Form form, string caption, ToastTheme theme)
    {
        Guard.NotNull(form, nameof(form));
        return Toast.Build(form, caption).SetTheme(theme).Show();
    }

    /// <summary>
    /// Shows a toast notification with caption, description, and theme.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <param name="description">The secondary text.</param>
    /// <param name="theme">The color theme.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowToast(this Form form, string caption, string description, ToastTheme theme)
    {
        Guard.NotNull(form, nameof(form));
        return Toast.Build(form, caption, description).SetTheme(theme).Show();
    }

    /// <summary>
    /// Shows an inputable toast that allows user text entry.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <param name="placeholder">Placeholder text for the input box.</param>
    /// <param name="onSubmit">Callback when user submits text.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowInputToast(
        this Form form,
        string caption,
        string? placeholder = null,
        Action<string>? onSubmit = null)
    {
        Guard.NotNull(form, nameof(form));
        var toast = Toast.Build(form, caption).EnableInput(placeholder);
        
        if (onSubmit != null)
            toast.OnSubmit += (_, e) => onSubmit(e.Text);
        
        return toast.Show();
    }

    /// <summary>
    /// Shows an inputable toast with description and submit callback.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="caption">The title text.</param>
    /// <param name="description">The secondary text.</param>
    /// <param name="placeholder">Placeholder text for the input box.</param>
    /// <param name="onSubmit">Callback when user submits text.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowInputToast(
        this Form form,
        string caption,
        string description,
        string? placeholder = null,
        Action<string>? onSubmit = null)
    {
        Guard.NotNull(form, nameof(form));
        var toast = Toast.Build(form, caption, description).EnableInput(placeholder);
        
        if (onSubmit != null)
            toast.OnSubmit += (_, e) => onSubmit(e.Text);
        
        return toast.Show();
    }

    /// <summary>
    /// Gets or creates the default ToastManager for this form.
    /// Use this for advanced scenarios requiring manager-level control.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <returns>The ToastManager instance for this form.</returns>
    public static ToastManager GetToastManager(this Form form)
    {
        Guard.NotNull(form, nameof(form));
        return ToastManagerRegistry.GetOrCreate(form);
    }

    /// <summary>
    /// Creates a new ToastManager with custom options for this form.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="options">Custom manager options.</param>
    /// <returns>A new ToastManager instance.</returns>
    public static ToastManager CreateToastManager(this Form form, ToastManagerOptions? options = null)
    {
        Guard.NotNull(form, nameof(form));
        return new ToastManager(form, options);
    }

    /// <summary>
    /// Shows a toast using the fluent builder pattern.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <param name="configure">Action to configure the toast builder.</param>
    /// <returns>The toast handle for further interaction.</returns>
    public static ToastHandle ShowToast(this Form form, Action<ToastBuilder> configure)
    {
        Guard.NotNull(form, nameof(form));
        var manager = ToastManagerRegistry.GetOrCreate(form);
        var builder = manager.Create();
        configure(builder);
        return builder.Show();
    }

    /// <summary>
    /// Dismisses all active toasts for this form.
    /// </summary>
    /// <param name="form">The owner form.</param>
    public static void DismissAllToasts(this Form form)
    {
        Guard.NotNull(form, nameof(form));
        var manager = ToastManagerRegistry.GetOrCreate(form);
        manager.DismissAll();
    }

    /// <summary>
    /// Gets the count of active toasts for this form.
    /// </summary>
    /// <param name="form">The owner form.</param>
    /// <returns>Number of visible toasts.</returns>
    public static int GetActiveToastCount(this Form form)
    {
        Guard.NotNull(form, nameof(form));
        var manager = ToastManagerRegistry.GetOrCreate(form);
        return manager.Count;
    }
}
