namespace FuzzyToast.Layout;

/// <summary>
/// Working-area bounds in the same pixel space as WinForms <c>Screen.WorkingArea</c>.
/// </summary>
/// <param name="Left">Left edge (inclusive), screen coordinates.</param>
/// <param name="Top">Top edge (inclusive), screen coordinates.</param>
/// <param name="Right">Right edge (exclusive in width math), screen coordinates.</param>
/// <param name="Bottom">Bottom edge (exclusive in height math), screen coordinates.</param>
public readonly record struct ScreenWorkingArea(int Left, int Top, int Right, int Bottom)
{
	/// <summary>Width in pixels (<c>Right - Left</c>).</summary>
	public int Width => Right - Left;

	/// <summary>Height in pixels (<c>Bottom - Top</c>).</summary>
	public int Height => Bottom - Top;
}

/// <summary>Simple rectangle for layout hints (not a WinForms type dependency for pure tests).</summary>
/// <param name="X">Left of the hint rectangle.</param>
/// <param name="Y">Top of the hint rectangle.</param>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
public readonly record struct LayoutRect(int X, int Y, int Width, int Height);

/// <summary>
/// Abstracts multi-monitor working areas so layout can be unit-tested without HWND.
/// </summary>
public interface IScreenProvider
{
	/// <summary>Working area of the monitor that contains the center of <paramref name="hint"/>.</summary>
	ScreenWorkingArea GetWorkingAreaNear(LayoutRect hint);

	/// <summary>Working area of the rightmost screen (TopRight / BottomRight anchoring).</summary>
	ScreenWorkingArea GetRightmostWorkingArea();

	/// <summary>Working area of the leftmost screen (TopLeft / BottomLeft anchoring).</summary>
	ScreenWorkingArea GetLeftmostWorkingArea();
}
