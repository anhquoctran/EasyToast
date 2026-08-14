using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

/// <summary>Win32 helpers used on .NET Framework 4.6 (and as fallback on modern TFMs).</summary>
internal static class NativeMethods
{
	private const int LogPixelsX = 88;
	private const uint EmSetCueBanner = 0x1501;
	
	// DPI Awareness constants
	private const int DpiAwarenessPerMonitorV2 = 2;

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

	[DllImport("shcore.dll")]
	private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("shcore.dll")]
	private static extern int GetProcessDpiAwareness(IntPtr hprocess, out int awareness);

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

	/// <summary>
	/// Gets DPI for a screen using Per-Monitor V2 awareness when available.
	/// Falls back to 96 DPI if unavailable.
	/// </summary>
	public static int GetDpiForScreen(Screen screen)
	{
		try
		{
			// Try GetDpiForMonitor (Windows 8.1+)
			var monitor = screen.Handle;
			var result = GetDpiForMonitor(monitor, 0, out var dpiX, out _); // 0 = MDT_Effective_DPI
			if (result == 0 && dpiX > 0)
				return (int)dpiX;
		}
		catch
		{
			// Fall through
		}

		// Fallback: use primary screen's DeviceDpi
		return screen.DeviceDpi > 0 ? screen.DeviceDpi : BaselineDpi;
	}

	/// <summary>
	/// Checks if the process is Per-Monitor V2 DPI aware.
	/// Returns false on older Windows versions or if not aware.
	/// </summary>
	public static bool IsPerMonitorV2Aware()
	{
		try
		{
			var processHandle = GetCurrentProcess();
			var result = GetProcessDpiAwareness(processHandle, out var awareness);
			return result == 0 && awareness == DpiAwarenessPerMonitorV2;
		}
		catch
		{
			return false;
		}
	}
}
