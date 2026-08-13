namespace FuzzyToast.Internal;

/// <summary>
/// Pure auto-dismiss controller. UI layer supplies "now" and a one-shot timer.
/// Hover pause freezes remaining ms; resume does not reset to full duration.
/// </summary>
public sealed class AutoDismissTimerState
{
	/// <summary>Original duration in milliseconds.</summary>
	public int TotalDurationMs { get; }

	/// <summary>Milliseconds still remaining (not reset on resume after pause).</summary>
	public int RemainingMs { get; private set; }

	/// <summary>Whether <see cref="Pause"/> is active.</summary>
	public bool IsPaused { get; private set; }

	/// <summary><see langword="true"/> when <see cref="RemainingMs"/> is 0 or less.</summary>
	public bool IsExpired => RemainingMs <= 0;

	/// <param name="totalDurationMs">Must be at least 1.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="totalDurationMs"/> is less than 1.</exception>
	public AutoDismissTimerState(int totalDurationMs)
	{
		if (totalDurationMs < 1)
			throw new ArgumentOutOfRangeException(nameof(totalDurationMs));

		TotalDurationMs = totalDurationMs;
		RemainingMs = totalDurationMs;
	}

	/// <summary>Call when toast becomes visible and timer should start. Returns interval ms.</summary>
	public int StartOrResume()
	{
		IsPaused = false;
		return Math.Max(RemainingMs, 1);
	}

	/// <summary>On mouse enter when PauseOnHover. Subtract elapsed since last arm.</summary>
	public void Pause(int elapsedSinceArmMs)
	{
		if (IsPaused)
			return;

		RemainingMs = Math.Max(0, RemainingMs - Math.Max(0, elapsedSinceArmMs));
		IsPaused = true;
	}

	/// <summary>On mouse leave. Returns interval for one-shot UI timer.</summary>
	public int Resume()
	{
		IsPaused = false;
		return Math.Max(RemainingMs, 1);
	}

	/// <summary>Timer fired: mark expired.</summary>
	public void OnTimerElapsed()
	{
		RemainingMs = 0;
		IsPaused = false;
	}
}
