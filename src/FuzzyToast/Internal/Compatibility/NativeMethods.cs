using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

/// <summary>Win32 helpers used on .NET Framework 4.6 (and as fallback on modern TFMs).</summary>
internal static class NativeMethods
{
	private const int LogPixelsX = 88;
	private const uint EmSetCueBanner = 0x1501;

	[DllImport("user32.dll")]
	private static extern uint GetDpiForWindow(IntPtr hwnd);

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr hwnd);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

	[DllImport("gdi32.dll")]
	private static extern int GetDeviceCaps(IntPtr hdc, int index);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

	public static long TickCount64
	{
		get
		{
#if NET5_0_OR_GREATER
			return Environment.TickCount64;
#else
			return Environment.TickCount & 0xFFFFFFFFL;
#endif
		}
	}

	public static int GetDeviceDpi(Control? control)
	{
		if (control is null || control.IsDisposed)
			return DpiScaling.BaselineDpi;

#if NET47_OR_GREATER || NETCOREAPP
		try
		{
			var dpi = control.DeviceDpi;
			if (dpi > 0)
				return dpi;
		}
		catch
		{
			// fall through to Win32
		}
#endif

		try
		{
			if (control.IsHandleCreated)
			{
				try
				{
					var dpi = unchecked((int)GetDpiForWindow(control.Handle));
					if (dpi > 0)
						return dpi;
				}
				catch (EntryPointNotFoundException)
				{
					// Pre–Windows 10 1607
				}
			}

			var hwnd = control.IsHandleCreated ? control.Handle : IntPtr.Zero;
			var dc = GetDC(hwnd);
			if (dc == IntPtr.Zero)
				return DpiScaling.BaselineDpi;
			try
			{
				var dpi = GetDeviceCaps(dc, LogPixelsX);
				return dpi > 0 ? dpi : DpiScaling.BaselineDpi;
			}
			finally
			{
				ReleaseDC(hwnd, dc);
			}
		}
		catch
		{
			return DpiScaling.BaselineDpi;
		}
	}

	public static void SetCueBanner(TextBox box, string? text)
	{
#if NETCOREAPP
		box.PlaceholderText = text ?? string.Empty;
#else
		if (!box.IsHandleCreated)
			_ = box.Handle;
		try
		{
			SendMessage(box.Handle, EmSetCueBanner, (IntPtr)1, text ?? string.Empty);
		}
		catch
		{
			// cue banner is cosmetic
		}
#endif
	}

	public static void BeginInvokeOn(Control control, Action action)
	{
		control.BeginInvoke(new MethodInvoker(action.Invoke));
	}

	public static void InvokeOn(Control control, Action action)
	{
		control.Invoke(new MethodInvoker(action.Invoke));
	}
}
