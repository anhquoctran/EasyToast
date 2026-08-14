using System.Drawing;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

internal interface IToastView : IDisposable
{
	ToastHandle ToastHandle { get; }
	void Apply(ToastOptions options, ColorScheme scheme, int durationMs, bool pauseOnHover, bool playSound, string? customSoundFilePath);
	void SetBounds(Rectangle bounds);
	void Show(IWin32Window? owner);
	void BeginDismiss();
	bool IsDisposed { get; }
	event EventHandler? Closed;
	event EventHandler? Clicked;
	event EventHandler? Hovered;

	/// <summary>Raised with the user's input text when Submit/Enter is used on an inputable toast.</summary>
	event EventHandler<string>? Submitted;
}
