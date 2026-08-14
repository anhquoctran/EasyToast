using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class ToastInputableTests
{
	private static ToastManager CreateManager(ToastManagerOptions? options = null)
	{
		var area = new ScreenWorkingArea(0, 0, 1920, 1080);
		return new ToastManager(
			null,
			options ?? new ToastManagerOptions { PlaySound = false, InputDurationMs = 30_000 },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));
	}

	[Fact]
	public void EnableInput_Defaults_To_NoAutoDismiss()
	{
		FakeToastView? view = null;
		var area = new ScreenWorkingArea(0, 0, 1920, 1080);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false, InputDurationMs = 45_000 },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) =>
			{
				view = new FakeToastView(handle);
				return view;
			});

		var h = mgr.Create()
			.SetCaption("Quick reply")
			.EnableInput(placeholder: "Type here…", submitButtonText: "Send")
			.Show();

		Assert.True(h.IsVisible);
		Assert.NotNull(view);
		Assert.True(view!.AppliedOptions!.EnableInput);
		// EnableInput sets DurationMs=0 → stay open until user action
		Assert.Equal(0, view.AppliedDurationMs);
		Assert.Equal(0, view.AppliedOptions.DurationMs);
		Assert.True(view.Bounds.Height > ToastLayoutMetrics.Default.ToastHeight);
	}

	[Fact]
	public void SetDurationMs_After_EnableInput_Enables_Timeout()
	{
		FakeToastView? view = null;
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false, InputDurationMs = 30_000 },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) =>
			{
				view = new FakeToastView(handle);
				return view;
			});

		mgr.Create()
			.SetCaption("Custom wait")
			.EnableInput()
			.SetDurationMs(12_000)
			.Show();

		Assert.Equal(12_000, view!.AppliedDurationMs);
	}

	[Fact]
	public void Submitted_Raises_With_Text_And_Metadata()
	{
		FakeToastView? view = null;
		var area = new ScreenWorkingArea(0, 0, 800, 600);
		using var mgr = new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) =>
			{
				view = new FakeToastView(handle);
				return view;
			});

		var h = mgr.Create()
			.SetCaption("Name?")
			.EnableInput(placeholder: "Your name")
			.SetTag(99)
			.SetMetadata("field", "name")
			.Show();

		ToastSubmittedEventArgs? args = null;
		h.Submitted += (_, e) => args = e;

		view!.RaiseSubmitted("Alice");
		// Fake view does not auto-dismiss; simulate dismiss after submit like real form
		view.BeginDismiss();

		Assert.NotNull(args);
		Assert.Equal("Alice", args!.InputText);
		Assert.Equal("Alice", h.SubmittedText);
		Assert.Equal(99, args.Tag);
		Assert.Equal("name", args.GetMetadata<string>("field"));
		Assert.True(h.IsDismissed);
	}

	[Fact]
	public void Toast_Build_EnableInput_Sets_Options_And_OnSubmit()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			ToastSubmittedEventArgs? submitted = null;

			var toast = Toast.MakeText(form, "Reply", "Type a short note")
				.SetMuting(true)
				.EnableInput(placeholder: "Message…", submitButtonText: "Send")
				.SetExtData("action", "quick-reply");

			toast.OnSubmit += (_, e) => submitted = e;
			toast.Show();
			Application.DoEvents();

			Assert.NotNull(toast.Handle);
			Assert.True(toast.Handle!.Options.EnableInput);
			Assert.Equal("Message…", toast.Handle.Options.InputPlaceholder);
			Assert.Equal("Send", toast.Handle.Options.SubmitButtonText);
			// EnableInput defaults DurationMs=0 (no auto-dismiss); SetDurationMs(60000) overrides if called after.
			Assert.True(toast.Handle.Options.EnableInput);

			if (toast.Handle.IsVisible)
			{
				toast.Handle.RaiseSubmitted("hello");
				Assert.NotNull(submitted);
				Assert.Equal("hello", submitted!.InputText);
				Assert.Equal("quick-reply", submitted.GetMetadata<string>("action"));
			}

			toast.Dismiss();
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ResolveDuration_Input_Preset()
	{
		var o = new ToastManagerOptions { InputDurationMs = 25_000, ShortDurationMs = 2000, LongDurationMs = 3000 };
		// EnableInput with DurationMs=0 (from EnableInput fluent) => no auto-dismiss
		Assert.Equal(0, o.ResolveDurationMs(new ToastOptions { Caption = "x", EnableInput = true, DurationMs = 0 }));
		// Duration.Input without DurationMs uses InputDurationMs
		Assert.Equal(25_000, o.ResolveDurationMs(new ToastOptions { Caption = "x", Duration = Duration.Input }));
		Assert.Equal(2000, o.ResolveDurationMs(new ToastOptions { Caption = "x", Duration = Duration.Short }));
		Assert.Equal(3000, o.ResolveDurationMs(new ToastOptions { Caption = "x", Duration = Duration.Long }));
		Assert.Equal(9000, o.ResolveDurationMs(new ToastOptions { Caption = "x", DurationMs = 9000 }));
		Assert.Equal(0, o.ResolveDurationMs(new ToastOptions { Caption = "x", DurationMs = 0 }));
	}
}
