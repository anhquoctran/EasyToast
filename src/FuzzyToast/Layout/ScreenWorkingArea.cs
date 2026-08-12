namespace FuzzyToast.Layout;

/// <summary>
/// Working-area bounds in the same pixel space as WinForms <c>Screen.WorkingArea</c>.
/// </summary>
public readonly record struct ScreenWorkingArea(int Left, int Top, int Right, int Bottom)
{
	public int Width => Right - Left;
	public int Height => Bottom - Top;
}

/// <summary>Simple rectangle for layout hints (not a WinForms type dependency for pure tests).</summary>
public readonly record struct LayoutRect(int X, int Y, int Width, int Height);

/// <summary>
/// Abstracts multi-monitor working areas so layout can be unit-tested without HWND.
/// </summary>
public interface IScreenProvider
{
	ScreenWorkingArea GetWorkingAreaNear(LayoutRect hint);

	/// <summary>Working area of the rightmost screen (TopRight / BottomRight anchoring).</summary>
	ScreenWorkingArea GetRightmostWorkingArea();

	/// <summary>Working area of the leftmost screen (TopLeft / BottomLeft anchoring).</summary>
	ScreenWorkingArea GetLeftmostWorkingArea();
}
