using FuzzyToast.Internal; // InternalsVisibleTo
using Xunit;

namespace FuzzyToast.Tests;

public class AutoDismissTimerStateTests
{
	[Fact]
	public void T23_PauseResume_UsesRemainingNotFullReset()
	{
		var state = new AutoDismissTimerState(2000);
		Assert.Equal(2000, state.StartOrResume());

		state.Pause(elapsedSinceArmMs: 700);
		Assert.True(state.IsPaused);
		Assert.Equal(1300, state.RemainingMs);

		var resumeInterval = state.Resume();
		Assert.False(state.IsPaused);
		Assert.Equal(1300, resumeInterval);
		Assert.Equal(1300, state.RemainingMs);
		Assert.NotEqual(2000, resumeInterval);
	}

	[Fact]
	public void OnTimerElapsed_Expires()
	{
		var state = new AutoDismissTimerState(500);
		state.StartOrResume();
		state.OnTimerElapsed();
		Assert.True(state.IsExpired);
		Assert.Equal(0, state.RemainingMs);
	}

	[Fact]
	public void Constructor_RejectsNonPositiveDuration()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new AutoDismissTimerState(0));
	}
}
