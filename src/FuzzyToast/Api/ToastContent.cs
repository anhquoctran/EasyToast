using System.Windows.Forms;

namespace FuzzyToast;

/// <summary>
/// Represents custom content that can be hosted inside a toast notification.
/// Use this to embed rich media, markdown-rendered controls, or any WinForms control.
/// </summary>
public class ToastContent : IDisposable
{
	private Control? _control;
	private bool _disposed;

	/// <summary>
	/// Creates custom toast content from an existing control.
	/// </summary>
	/// <param name="control">The control to display in the toast.</param>
	/// <param name="ownsControl">If true, the toast will dispose the control when closed.</param>
	public ToastContent(Control control, bool ownsControl = true)
	{
		_control = control ?? throw new ArgumentNullException(nameof(control));
		OwnsControl = ownsControl;
	}

	/// <summary>
	/// The control to display in the toast.
	/// </summary>
	public Control? Control
	{
		get => _control;
		set
		{
			if (_control != value)
			{
				if (_control != null && OwnsControl && !_disposed)
					_control.Dispose();
				_control = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets whether the toast owns and should dispose the control.
	/// </summary>
	public bool OwnsControl { get; set; }

	/// <summary>
	/// Gets or sets the minimum height required for this content.
	/// </summary>
	public int MinHeight { get; set; } = 60;

	/// <summary>
	/// Gets or sets the minimum width required for this content.
	/// </summary>
	public int MinWidth { get; set; } = 250;

	/// <summary>
	/// Called when the content is about to be displayed.
	/// Override to perform initialization or layout adjustments.
	/// </summary>
	public virtual void OnShow()
	{
		if (_control != null && !_control.IsDisposed)
		{
			_control.Visible = true;
		}
	}

	/// <summary>
	/// Called when the toast is being dismissed.
	/// Override to perform cleanup or save state.
	/// </summary>
	public virtual void OnDismiss()
	{
	}

	/// <summary>
	/// Disposes the content and optionally the underlying control.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		OnDismiss();

		if (_control != null && OwnsControl && !_control.IsDisposed)
			_control.Dispose();

		_control = null;
	}

	/// <summary>
	/// Creates toast content from a PictureBox displaying an image.
	/// </summary>
	/// <param name="image">The image to display.</param>
	/// <param name="sizeMode">How the image should be sized within the box.</param>
	/// <returns>A new ToastContent instance.</returns>
	public static ToastContent FromImage(Image image, PictureBoxSizeMode sizeMode = PictureBoxSizeMode.Zoom)
	{
		var pictureBox = new PictureBox
		{
			Image = image,
			SizeMode = sizeMode,
			Dock = DockStyle.Fill,
			BackColor = Color.Transparent
		};
		return new ToastContent(pictureBox, ownsControl: true);
	}

	/// <summary>
	/// Creates toast content from a Label with formatted text.
	/// Supports basic HTML-like formatting when UseCompatibleTextRendering is enabled.
	/// </summary>
	/// <param name="text">The text to display.</param>
	/// <param name="font">Optional custom font.</param>
	/// <param name="foreColor">Optional foreground color.</param>
	/// <returns>A new ToastContent instance.</returns>
	public static ToastContent FromText(string text, Font? font = null, Color? foreColor = null)
	{
		var label = new Label
		{
			Text = text,
			Dock = DockStyle.Fill,
			AutoSize = false,
			AutoEllipsis = true,
			ForeColor = foreColor ?? SystemColors.WindowText,
			Font = font ?? SystemFonts.DefaultFont,
			BackColor = Color.Transparent
		};
		label.UseCompatibleTextRendering = true;
		return new ToastContent(label, ownsControl: true);
	}

	/// <summary>
	/// Creates toast content from a Panel containing multiple controls.
	/// </summary>
	/// <param name="configure">Action to configure the panel and add controls.</param>
	/// <returns>A new ToastContent instance.</returns>
	public static ToastContent FromPanel(Action<Panel> configure)
	{
		var panel = new Panel
		{
			Dock = DockStyle.Fill,
			AutoSize = false,
			BackColor = Color.Transparent,
			Padding = new Padding(4)
		};
		configure(panel);
		return new ToastContent(panel, ownsControl: true);
	}
}
