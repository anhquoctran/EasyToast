using System.Drawing;
using System.Windows.Forms;
using FuzzyToast.Layout;

namespace FuzzyToast.Internal;

/// <summary>
/// Screen working areas for Windows 10/11 (multi-monitor, taskbar on any edge).
/// Prefer the owner's monitor so toasts appear where the user is working.
/// </summary>
internal sealed class WinFormsScreenProvider : IScreenProvider
{
	private readonly Control? _owner;

	public WinFormsScreenProvider(Control? owner = null) => _owner = owner;

	public ScreenWorkingArea GetWorkingAreaNear(LayoutRect hint)
	{
		var screen = Screen.FromPoint(new Point(hint.X + hint.Width / 2, hint.Y + hint.Height / 2));
		return ToArea(screen.WorkingArea);
	}

	public ScreenWorkingArea GetRightmostWorkingArea()
	{
		// Prefer owner screen for right-side anchors (typical multi-monitor setup).
		if (TryGetOwnerWorkingArea(out var ownerArea))
			return ownerArea;

		return GetExtremeWorkingArea(preferRight: true);
	}

	public ScreenWorkingArea GetLeftmostWorkingArea()
	{
		if (TryGetOwnerWorkingArea(out var ownerArea))
			return ownerArea;

		return GetExtremeWorkingArea(preferRight: false);
	}

	/// <summary>Working area of the monitor containing the owner control (or primary).</summary>
	public ScreenWorkingArea GetOwnerOrPrimaryWorkingArea()
	{
		if (TryGetOwnerWorkingArea(out var ownerArea))
			return ownerArea;

		var primary = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
		if (primary is null)
			return new ScreenWorkingArea(0, 0, 1920, 1080);

		return ToArea(primary.WorkingArea);
	}

	private bool TryGetOwnerWorkingArea(out ScreenWorkingArea area)
	{
		area = default;
		try
		{
			if (_owner is null || _owner.IsDisposed)
				return false;

			var screen = _owner.IsHandleCreated
				? Screen.FromControl(_owner)
				: Screen.FromPoint(_owner.Location);

			area = ToArea(screen.WorkingArea);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static ScreenWorkingArea GetExtremeWorkingArea(bool preferRight)
	{
		var screens = Screen.AllScreens;
		if (screens is null || screens.Length == 0)
		{
			var primary = Screen.PrimaryScreen;
			if (primary is not null)
				return ToArea(primary.WorkingArea);
			return new ScreenWorkingArea(0, 0, 1920, 1080);
		}

		var chosen = screens[0];
		foreach (var screen in screens)
		{
			if (preferRight)
			{
				if (screen.WorkingArea.Right > chosen.WorkingArea.Right)
					chosen = screen;
			}
			else if (screen.WorkingArea.Left < chosen.WorkingArea.Left)
			{
				chosen = screen;
			}
		}

		return ToArea(chosen.WorkingArea);
	}

	private static ScreenWorkingArea ToArea(Rectangle r) =>
		new(r.Left, r.Top, r.Right, r.Bottom);
}

/// <summary>Fixed working area for pure unit tests.</summary>
internal sealed class FixedScreenProvider : IScreenProvider
{
	private readonly ScreenWorkingArea _area;

	public FixedScreenProvider(ScreenWorkingArea area) => _area = area;

	public ScreenWorkingArea GetWorkingAreaNear(LayoutRect hint) => _area;
	public ScreenWorkingArea GetRightmostWorkingArea() => _area;
	public ScreenWorkingArea GetLeftmostWorkingArea() => _area;
}
