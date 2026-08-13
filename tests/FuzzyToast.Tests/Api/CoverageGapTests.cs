using System.Drawing;
using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

/// <summary>Closes remaining line-coverage gaps in public/pure API types.</summary>
public class CoverageGapTests
{
	[Fact]
	public void ThemeCatalog_UnknownTheme_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => ThemeCatalog.Resolve((ToastTheme)999));
	}

	[Fact]
	public void ImageValidation_DisposedImage_ReturnsFalse()
	{
		var bmp = new Bitmap(80, 80);
		bmp.Dispose();
		Assert.False(ImageValidation.ValidateImageSize(bmp));
	}

	[Fact]
	public void ImageValidation_LockedFile_ReturnsFalse()
	{
		var path = Path.Combine(Path.GetTempPath(), "FuzzyToast_lock_" + Guid.NewGuid().ToString("N") + ".png");
		File.WriteAllBytes(path, [137, 80, 78, 71, 13, 10, 26, 10]);
		try
		{
			using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			Assert.False(ImageValidation.ValidateImagePath(path));
		}
		finally
		{
			try { File.Delete(path); } catch { /* ignore */ }
		}
	}

	[Fact]
	public void Toast_Fluent_Metadata_Input_And_Properties()
	{
		using var form = new Form();
		var toast = Toast.Build(form, "cap");
		toast.Tag = 7;
		Assert.Equal(7, toast.Tag);
		Assert.Empty(toast.Metadata);

		toast.SetMetadata(new Dictionary<string, object?>
		{
			["a"] = 1,
			["  "] = "skip",
			[""] = "skip2",
			["b"] = "ok"
		});
		toast.SetExtData(new[] { new KeyValuePair<string, object?>("c", 3) });
		toast.SetInputable();
		toast.SetInputable(false);
		toast.SetInputable(true);
		toast.SetDurationMs(1500);
		Assert.Equal(1, toast.Metadata["a"]);
		Assert.Equal("ok", toast.Metadata["b"]);
		Assert.Equal(3, toast.Metadata["c"]);
		Assert.False(toast.Metadata.ContainsKey("  "));

		Assert.Throws<ArgumentOutOfRangeException>(() => toast.SetDurationMs(-1));
	}

	[Fact]
	public void Toast_Build_DisposedOwner_Throws()
	{
		var form = new Form();
		form.Dispose();
		Assert.Throws<ObjectDisposedException>(() => Toast.Build(form, "x"));
	}

	[Fact]
	public void ToastBuilder_Metadata_Input_And_Data()
	{
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));

		var builder = mgr.Create()
			.SetCaption("x")
			.SetData("payload")
			.SetExtData(new[] { new KeyValuePair<string, object?>("k", "v") })
			.SetMetadata(new Dictionary<string, object?>
			{
				["keep"] = 1,
				[" "] = "skip"
			})
			.SetInputable()
			.SetInputable(false)
			.SetInputable(true)
			.SetDurationMs(0);

		var options = builder.Build();
		Assert.Equal("payload", options.Tag);
		Assert.Equal("v", options.Metadata["k"]);
		Assert.Equal(1, options.Metadata["keep"]);
		Assert.True(options.EnableInput);
		Assert.Equal(0, options.DurationMs);

		Assert.Throws<ArgumentException>(() => mgr.Create().SetMetadata("  ", 1));
		Assert.Throws<ArgumentNullException>(() => mgr.Create().SetMetadata(null!));
		Assert.Throws<ArgumentOutOfRangeException>(() => mgr.Create().SetDurationMs(-5));
	}

	[Fact]
	public void ToastEvents_Metadata_Convert_And_EmptySubmit()
	{
		var options = new ToastOptions
		{
			Caption = "c",
			Tag = "tag",
			Metadata = ToastOptions.FreezeMetadata(new Dictionary<string, object?>
			{
				["n"] = "42",
				["bad"] = new object()
			})
		};
		var handle = new ToastHandle("id", options, ToastHandleState.Visible, null!);
		var args = new ToastInteractionEventArgs(handle);
		Assert.Same(options, args.Options);
		Assert.True(args.TryGetMetadata<int>("n", out var n));
		Assert.Equal(42, n);
		Assert.False(args.TryGetMetadata<int>("missing", out _));
		Assert.False(args.TryGetMetadata<int>("bad", out _));
		Assert.Equal(-1, args.GetMetadata("missing", -1));

		var submitted = new ToastSubmittedEventArgs(handle, "   ");
		Assert.True(submitted.IsEmpty);
		var rejected = new ToastRejectedEventArgs(handle, options, "why");
		Assert.Same(handle, rejected.Toast);
	}

	[Fact]
	public void ToastHandle_Swallows_HandlerExceptions_And_NullSubmit()
	{
		var handle = new ToastHandle("h", new ToastOptions { Caption = "c" }, ToastHandleState.Visible, null!);
		handle.Clicked += (_, _) => throw new InvalidOperationException("click");
		handle.Hovered += (_, _) => throw new InvalidOperationException("hover");
		handle.Submitted += (_, _) => throw new InvalidOperationException("submit");
		handle.Dismissed += (_, _) => throw new InvalidOperationException("dismiss");

		handle.RaiseClicked();
		handle.RaiseHovered();
		handle.RaiseSubmitted(null!);
		Assert.Equal(string.Empty, handle.SubmittedText);
		handle.MarkDismissed();
		handle.MarkDismissed(); // already dismissed
		Assert.True(handle.IsDismissed);
	}

	[Fact]
	public void ToastOptions_Validate_And_FreezeMetadata()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			new ToastOptions { Caption = "c", DurationMs = -1 }.Validate());
		Assert.Throws<ArgumentException>(() =>
			new ToastOptions { Caption = "c", EnableInput = true, SubmitButtonText = "  " }.Validate());
		Assert.Empty(ToastOptions.FreezeMetadata(null));
		Assert.Empty(ToastOptions.FreezeMetadata(Array.Empty<KeyValuePair<string, object?>>()));
	}

	[Fact]
	public void CapacityPolicy_UnknownPolicy_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			CapacityPolicy.Evaluate(
				(ToastOverflowPolicy)99,
				maxToasts: 3,
				maxToastsPerPosition: 2,
				ToastPosition.TopRight,
				[("a", ToastPosition.TopRight), ("b", ToastPosition.TopRight)]));
	}

	[Fact]
	public void LayoutEngine_UnknownPosition_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			ToastLayoutEngine.ComputeLocation(
				(ToastPosition)99,
				0,
				ToastLayoutMetrics.Default,
				new ScreenWorkingArea(0, 0, 100, 100)));
	}

	[Fact]
	public void AutoDismiss_Exposes_TotalDuration()
	{
		var state = new AutoDismissTimerState(250);
		Assert.Equal(250, state.TotalDurationMs);
	}

	[Fact]
	public void LayoutMetrics_ZeroStackGap_UsesVerticalMargin()
	{
		var m = new ToastLayoutMetrics
		{
			ToastWidth = 100,
			ToastHeight = 40,
			HorizontalMargin = 4,
			VerticalMargin = 7,
			StackGap = 0
		};
		Assert.Equal(47, m.EffectiveStackStride);
	}

	[Fact]
	public void ImmediateUiMarshaler_InvokeAsync_RunsInline()
	{
		var ran = false;
		var m = new ImmediateUiMarshaler();
		var task = m.InvokeAsync(() => ran = true);
		Assert.True(task.IsCompletedSuccessfully);
		Assert.True(ran);
		m.Invoke(() => { });
		Assert.False(m.InvokeRequired);
	}

	[Fact]
	public async Task ToastManager_ShowAsync_InvokeRequired_And_AlreadyCancelled()
	{
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ForcedInvokeMarshaler(),
			(opts, handle) => new FakeToastView(handle));

		using var cts = new CancellationTokenSource();
		cts.Cancel();
		var handle = await mgr.ShowAsync(new ToastOptions { Caption = "x" }, cts.Token);
		Assert.True(handle.IsDismissed || handle.IsVisible);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			mgr.ShowAsync(new ToastOptions { Caption = "" }));
	}

	[Fact]
	public void ToastManager_DisposedOwner_And_PostDispose_NoOps()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			form.Dispose();
			Assert.Throws<ObjectDisposedException>(() => new ToastManager(form));
			Assert.Throws<ObjectDisposedException>(() => ToastManagerRegistry.GetOrCreate(form));
		});

		var area = new ScreenWorkingArea(0, 0, 800, 600);
		var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));
		var handle = mgr.Show(new ToastOptions { Caption = "x" });
		mgr.Dispose();
		mgr.DismissAll();
		mgr.DismissInternal(handle);
		handle.Dismiss();
		Assert.True(mgr.IsDisposed);
	}

	[Fact]
	public void ToastManager_Dispose_Swallows_MarshalerErrors()
	{
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ThrowingUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));
		mgr.Dispose();
		Assert.True(mgr.IsDisposed);
	}

	[Fact]
	public void ToastManager_DropOldest_WithThrowingView_Continues()
	{
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions
			{
				PlaySound = false,
				MaxToastsPerPosition = 1,
				OverflowPolicy = ToastOverflowPolicy.DropOldest
			},
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new ThrowingToastView(handle));

		mgr.Show(new ToastOptions { Caption = "1" });
		var second = mgr.Show(new ToastOptions { Caption = "2" });
		Assert.True(second.IsVisible);
	}
}
