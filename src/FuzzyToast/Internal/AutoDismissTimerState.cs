namespace FuzzyToast.Internal;

/// <summary>
/// Pure auto-dismiss controller. UI layer supplies "now" and a one-shot timer.
/// Hover pause freezes remaining ms; resume does not reset to full duration.
/// </summary>
public sealed class AutoDismissTimerState
{
	public int TotalDurationMs { get; }
	public int RemainingMs { get; private set; }
	public bool IsPaused { get; private set; }
	public bool IsExpired => RemainingMs <= 0;

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
