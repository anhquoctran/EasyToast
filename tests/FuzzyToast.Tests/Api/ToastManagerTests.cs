using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class ToastManagerTests
{
	private static ToastManager CreateManager(ToastManagerOptions? options = null)
	{
		var area = new ScreenWorkingArea(0, 0, 1920, 1080);
		return new ToastManager(
			owner: null,
			options ?? new ToastManagerOptions
			{
				MaxToasts = 6,
				MaxToastsPerPosition = 3,
				OverflowPolicy = ToastOverflowPolicy.DropNewest,
				PlaySound = false
			},
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));
	}

	[Fact]
	public void Show_AddsToActiveToasts()
	{
		using var mgr = CreateManager();
		var h = mgr.Show(new ToastOptions { Caption = "A" });
		Assert.True(h.IsVisible);
		Assert.Equal(1, mgr.Count);
		Assert.Contains(mgr.ActiveToasts, t => t.Id == h.Id);
	}

	[Fact]
	public void Show_EmptyCaption_Throws()
	{
		using var mgr = CreateManager();
		Assert.Throws<ArgumentException>(() => mgr.Show(new ToastOptions { Caption = "" }));
	}

	[Fact]
	public void T24_DropNewest_RejectsWithEvent()
	{
		using var mgr = CreateManager(new ToastManagerOptions
		{
			MaxToasts = 6,
			MaxToastsPerPosition = 2,
			OverflowPolicy = ToastOverflowPolicy.DropNewest
		});

		ToastRejectedEventArgs? rejected = null;
		mgr.ToastRejected += (_, e) => rejected = e;

		mgr.Show(new ToastOptions { Caption = "1", Position = ToastPosition.TopRight });
		mgr.Show(new ToastOptions { Caption = "2", Position = ToastPosition.TopRight });
		var h3 = mgr.Show(new ToastOptions { Caption = "3", Position = ToastPosition.TopRight });

		Assert.True(h3.WasRejected);
		Assert.Equal(2, mgr.Count);
		Assert.NotNull(rejected);
		Assert.Equal("MaxToastsPerPosition", rejected!.Reason);
		Assert.True(h3.WhenDismissed.IsCompletedSuccessfully);
	}

	[Fact]
	public void DropOldest_RemovesVictimAndShowsNew()
	{
		using var mgr = CreateManager(new ToastManagerOptions
		{
			MaxToasts = 6,
			MaxToastsPerPosition = 2,
			OverflowPolicy = ToastOverflowPolicy.DropOldest
		});

		var h1 = mgr.Show(new ToastOptions { Caption = "1", Position = ToastPosition.BottomRight });
		var h2 = mgr.Show(new ToastOptions { Caption = "2", Position = ToastPosition.BottomRight });
		var h3 = mgr.Show(new ToastOptions { Caption = "3", Position = ToastPosition.BottomRight });

		Assert.True(h3.IsVisible);
		Assert.True(h1.IsDismissed);
		Assert.Equal(2, mgr.Count);
		Assert.DoesNotContain(mgr.ActiveToasts, t => t.Id == h1.Id);
		Assert.Contains(mgr.ActiveToasts, t => t.Id == h2.Id);
	}

	[Fact]
	public void ThrowPolicy_Throws()
	{
		using var mgr = CreateManager(new ToastManagerOptions
		{
			MaxToastsPerPosition = 1,
			OverflowPolicy = ToastOverflowPolicy.Throw
		});
		mgr.Show(new ToastOptions { Caption = "1" });
		Assert.Throws<InvalidOperationException>(() => mgr.Show(new ToastOptions { Caption = "2" }));
	}

	[Fact]
	public async Task T15_ShowAsync_CompletesWhileVisible()
	{
		using var mgr = CreateManager();
		var h = await mgr.ShowAsync(new ToastOptions { Caption = "Async" });
		Assert.True(h.IsVisible);
		Assert.False(h.WhenDismissed.IsCompleted);
	}

	[Fact]
	public async Task T15b_WhenDismissed_CompletesOnClose()
	{
		using var mgr = CreateManager();
		var h = mgr.Show(new ToastOptions { Caption = "X" });
		h.Dismiss();
		await h.WhenDismissed.WaitAsync(TimeSpan.FromSeconds(2));
		Assert.True(h.IsDismissed);
		Assert.Equal(0, mgr.Count);
	}

	[Fact]
	public async Task T15c_Rejected_WhenDismissed_RanToCompletionImmediately()
	{
		using var mgr = CreateManager(new ToastManagerOptions
		{
			MaxToastsPerPosition = 1,
			OverflowPolicy = ToastOverflowPolicy.DropNewest
		});
		mgr.Show(new ToastOptions { Caption = "1" });
		var h = await mgr.ShowAsync(new ToastOptions { Caption = "2" });
		Assert.True(h.WasRejected);
		Assert.True(h.WhenDismissed.IsCompletedSuccessfully);
	}

	[Fact]
	public void T16_Reflow_AfterMiddleRemoved()
	{
		using var mgr = CreateManager();
		var h1 = mgr.Show(new ToastOptions { Caption = "1", Position = ToastPosition.TopRight });
		var h2 = mgr.Show(new ToastOptions { Caption = "2", Position = ToastPosition.TopRight });
		var h3 = mgr.Show(new ToastOptions { Caption = "3", Position = ToastPosition.TopRight });

		h2.Dismiss();

		Assert.Equal(2, mgr.Count);
		// remaining handles still visible
		Assert.True(h1.IsVisible);
		Assert.True(h3.IsVisible);
	}

	[Fact]
	public void T25_Dispose_Idempotent()
	{
		var mgr = CreateManager();
		mgr.Show(new ToastOptions { Caption = "1" });
		mgr.Dispose();
		mgr.Dispose();
		Assert.Throws<ObjectDisposedException>(() => mgr.Show(new ToastOptions { Caption = "2" }));
	}

	[Fact]
	public void Dismiss_OnRejected_IsNoOp()
	{
		using var mgr = CreateManager(new ToastManagerOptions
		{
			MaxToastsPerPosition = 1,
			OverflowPolicy = ToastOverflowPolicy.DropNewest
		});
		mgr.Show(new ToastOptions { Caption = "1" });
		var h = mgr.Show(new ToastOptions { Caption = "2" });
		h.Dismiss();
		Assert.True(h.WasRejected);
	}
}
