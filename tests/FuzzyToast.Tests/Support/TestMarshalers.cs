using FuzzyToast.Internal;

namespace FuzzyToast.Tests.Support;

/// <summary>Always reports <see cref="InvokeRequired"/> so <c>ShowAsync</c> takes the marshal path.</summary>
internal sealed class ForcedInvokeMarshaler : IUiMarshaler
{
	public bool InvokeRequired { get; set; } = true;

	public void Invoke(Action action) => action();

	public Task InvokeAsync(Action action)
	{
		action();
		return Task.CompletedTask;
	}
}

/// <summary>Throws on every invoke — used to exercise <see cref="ToastManager.Dispose"/> catch.</summary>
internal sealed class ThrowingUiMarshaler : IUiMarshaler
{
	public bool InvokeRequired => false;

	public void Invoke(Action action) => throw new InvalidOperationException("marshaler gone");

	public Task InvokeAsync(Action action) => throw new InvalidOperationException("marshaler gone");
}

/// <summary>View that throws on dismiss/dispose so manager catch blocks run.</summary>
internal sealed class ThrowingToastView : IToastView
{
	public ThrowingToastView(ToastHandle handle) => ToastHandle = handle;

	public ToastHandle ToastHandle { get; }
	public bool IsDisposed => false;

	public event EventHandler? Closed
	{
		add { }
		remove { }
	}

	public event EventHandler? Clicked
	{
		add { }
		remove { }
	}

	public event EventHandler? Hovered
	{
		add { }
		remove { }
	}

	public event EventHandler<string>? Submitted
	{
		add { }
		remove { }
	}

	public void Apply(ToastOptions options, ColorScheme scheme, int durationMs, bool pauseOnHover, bool playSound)
	{
	}

	public void SetBounds(System.Drawing.Rectangle bounds)
	{
	}

	public void Show(System.Windows.Forms.IWin32Window? owner)
	{
	}

	public void BeginDismiss() => throw new InvalidOperationException("dismiss failed");

	public void Dispose() => throw new InvalidOperationException("dispose failed");
}
