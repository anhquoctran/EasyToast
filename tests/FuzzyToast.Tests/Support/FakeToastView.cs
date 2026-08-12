using System.Drawing;
using System.Windows.Forms;
using FuzzyToast.Internal;

namespace FuzzyToast.Tests.Support;

internal sealed class FakeToastView : IToastView
{
	private bool _disposed;

	public FakeToastView(ToastHandle handle) => ToastHandle = handle;

	public ToastHandle ToastHandle { get; }
	public bool IsDisposed => _disposed;
	public Rectangle Bounds { get; private set; }
	public bool WasShown { get; private set; }
	public ToastOptions? AppliedOptions { get; private set; }

	public event EventHandler? Closed;
	public event EventHandler? Clicked;
	public event EventHandler? Hovered;

	public void Apply(ToastOptions options, ColorScheme scheme, int durationMs, bool pauseOnHover, bool playSound)
	{
		AppliedOptions = options;
	}

	public void SetBounds(Rectangle bounds) => Bounds = bounds;

	public void Show(IWin32Window? owner) => WasShown = true;

	public void BeginDismiss()
	{
		Closed?.Invoke(this, EventArgs.Empty);
	}

	public void Dispose()
	{
		_disposed = true;
	}

	// silence unused event warnings in tests
	public void RaiseClicked() => Clicked?.Invoke(this, EventArgs.Empty);
	public void RaiseHovered() => Hovered?.Invoke(this, EventArgs.Empty);
}
