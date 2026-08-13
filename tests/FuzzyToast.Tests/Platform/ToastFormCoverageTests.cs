using System.Drawing;
using FuzzyToast.Internal;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

/// <summary>STA coverage of ToastForm private handlers, input mode, and platform edges.</summary>
public class ToastFormCoverageTests
{
	[Fact]
	public void ToastForm_Inputable_Submit_Enter_Escape_And_EmptyReject()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "Reply",
				Description = "type",
				EnableInput = true,
				InputPlaceholder = "msg",
				InputDefaultText = "hello",
				SubmitButtonText = "  ",
				AllowEmptySubmit = false,
				CloseStyle = CloseStyle.ClickEntire,
				IsMuted = true,
				DurationMs = 5000
			};
			var handle = new ToastHandle("in1", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			var submitted = 0;
			view.Submitted += (_, _) => submitted++;
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 5000, pauseOnHover: true, playSound: false);
			view.SetBounds(new Rectangle(80, 80, 420, 140));
			view.Show(form);
			Application.DoEvents();

			Assert.False(view.IsDisposed);
			EventHandler closed = (_, _) => { };
			((IToastView)view).Closed += closed;
			((IToastView)view).Closed -= closed;

			var txt = Find<TextBox>(view, "txtInput");
			var btn = Find<Button>(view, "btnSubmit");
			Assert.NotNull(txt);
			Assert.NotNull(btn);
			Assert.Equal("OK", btn!.Text);

			txt!.Text = "";
			btn.PerformClick();
			Application.DoEvents();
			Assert.Equal(0, submitted);

			Reflect.Invoke(view, "TrySubmit");
			Assert.Equal(0, submitted);

			txt.Text = "world";
			Reflect.Invoke(view, "TxtInput_KeyDown", txt, new KeyEventArgs(Keys.Enter));
			Application.DoEvents();
			Assert.Equal(1, submitted);

			form.Close();
		});
	}

	[Fact]
	public void ToastForm_Inputable_Escape_And_SecondApplyWithoutInput()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var input = new ToastOptions
			{
				Caption = "In",
				EnableInput = true,
				AllowEmptySubmit = true,
				SubmitButtonText = "Go",
				IsMuted = true
			};
			var handle = new ToastHandle("in2", input, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(input, ThemeCatalog.Resolve(ToastTheme.Light), 0, pauseOnHover: false, playSound: false);
			view.SetBounds(new Rectangle(40, 40, 400, 140));
			view.Show(form);
			Application.DoEvents();

			var txt = Find<TextBox>(view, "txtInput");
			Assert.NotNull(txt);
			txt!.Focus();
			Application.DoEvents();
			form.Activate();
			Application.DoEvents();

			Reflect.Invoke(view, "TxtInput_KeyDown", txt, new KeyEventArgs(Keys.A));
			Reflect.Invoke(view, "TxtInput_KeyDown", txt, new KeyEventArgs(Keys.Escape));
			Application.DoEvents();
			PumpUntilDisposed(view, TimeSpan.FromSeconds(2));

			// Recreate to apply input then non-input (hide existing panel).
			var handle2 = new ToastHandle("in3", input, ToastHandleState.Visible, null!);
			using var view2 = new ToastForm(handle2);
			view2.Apply(input, ThemeCatalog.Resolve(ToastTheme.Dark), 0, false, false);
			view2.Apply(
				new ToastOptions { Caption = "plain", Description = "", IsMuted = true },
				ThemeCatalog.Resolve(ToastTheme.Dark),
				2000,
				false,
				false);
			view2.SetBounds(new Rectangle(10, 10, 380, 100));
			view2.Show(form);
			Application.DoEvents();
			view2.BeginDismiss();
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_ClickEntire_HoverPause_And_TimerTick()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "Click",
				Description = "body",
				CloseStyle = CloseStyle.ButtonAndClickEntire,
				Animation = Animation.Fade,
				IsMuted = true
			};
			var handle = new ToastHandle("clk", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			var clicked = 0;
			var hovered = 0;
			view.Clicked += (_, _) => clicked++;
			view.Hovered += (_, _) => hovered++;
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.PrimaryLight), 4000, pauseOnHover: true, playSound: false);
			view.SetBounds(new Rectangle(30, 30, 400, 120));
			view.Show(form);
			Application.DoEvents();

			Reflect.Invoke(view, "ToastForm_MouseEnter", view, EventArgs.Empty);
			Reflect.Invoke(view, "PauseCountdown");
			Reflect.Invoke(view, "ToastForm_MouseLeave", view, EventArgs.Empty);
			Reflect.Invoke(view, "ResumeCountdown");
			Reflect.Invoke(view, "TmrClose_Tick", view, EventArgs.Empty);

			Reflect.SetField(view, "_autoDismissEnabled", true);
			Reflect.SetField(view, "_countdownPaused", true);
			Reflect.SetField(view, "_remainingMs", 1000);
			Reflect.Invoke(view, "TmrClose_Tick", view, EventArgs.Empty);

			Reflect.SetField(view, "_countdownPaused", false);
			Reflect.SetField(view, "_remainingMs", 500);
			Reflect.Invoke(view, "TmrClose_Tick", view, EventArgs.Empty);

			Reflect.SetField(view, "_autoDismissEnabled", true);
			Reflect.SetField(view, "_remainingMs", 10);
			Reflect.SetField(view, "_countdownPaused", false);
			Reflect.Invoke(view, "TmrClose_Tick", view, EventArgs.Empty);

			Reflect.Invoke(view, "ToastContentClick", view, EventArgs.Empty);
			Assert.True(clicked >= 1);
			Assert.True(hovered >= 1);

			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_Show_OwnerVariants_And_BackgroundDismiss()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions { Caption = "own", IsMuted = true, Animation = Animation.Slide };
			var handle = new ToastHandle("own", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 3000, false, false);
			view.SetBounds(new Rectangle(20, 20, 380, 100));

			using var unshown = new Form { ShowInTaskbar = false, Size = new Size(80, 80) };
			view.Show(unshown);
			Application.DoEvents();
			view.BeginDismiss();
			Application.DoEvents();

			var handle2 = new ToastHandle("own2", options, ToastHandleState.Visible, null!);
			using var view2 = new ToastForm(handle2);
			view2.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 3000, false, false);
			view2.SetBounds(new Rectangle(20, 20, 380, 100));
			view2.Show(new DummyWindow(form.Handle));
			Application.DoEvents();

			var worker = new Thread(() => view2.BeginDismiss());
			worker.IsBackground = true;
			worker.Start();
			var sw = System.Diagnostics.Stopwatch.StartNew();
			while (worker.IsAlive && sw.Elapsed < TimeSpan.FromSeconds(3))
			{
				Application.DoEvents();
				Thread.Sleep(15);
			}
			worker.Join(500);
			Application.DoEvents();

			var handle3 = new ToastHandle("own3", options, ToastHandleState.Visible, null!);
			using var view3 = new ToastForm(handle3);
			view3.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 2000, false, false);
			view3.SetBounds(new Rectangle(20, 20, 380, 100));
			using var disposed = new Form();
			disposed.Show();
			Application.DoEvents();
			disposed.Dispose();
			view3.Show(disposed);
			Application.DoEvents();
			view3.Show();
			Application.DoEvents();
			view3.BeginDismiss();
			Application.DoEvents();

			form.Close();
		});
	}

	[Fact]
	public void ToastForm_Show_NullOwner_And_PrivateHandlers()
	{
		StaHelper.Run(() =>
		{
			var options = new ToastOptions { Caption = "solo", Description = "", IsMuted = true, Animation = Animation.Fade };
			var handle = new ToastHandle("solo", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 2000, true, playSound: true);
			view.SetBounds(new Rectangle(10, 10, 380, 100));
			view.Show(null);
			Application.DoEvents();

			Reflect.Invoke(view, "ToastForm_Load", view, EventArgs.Empty);
			Reflect.Invoke(view, "ToastForm_Shown", view, EventArgs.Empty);
			Reflect.Invoke(view, "ToastForm_FormClosing", view, new FormClosingEventArgs(CloseReason.UserClosing, false));
			Reflect.Invoke(view, "ApplyContentShellPadding");
			Reflect.Invoke(view, "TryPlaySound");
			Reflect.Invoke(view, "RaiseClosedOnce");
			Reflect.Invoke(view, "RaiseClosedOnce");
			Reflect.Invoke(view, "BtnClose_Click", view, EventArgs.Empty);
			Application.DoEvents();

			// FormClosing before handle exists
			var handle2 = new ToastHandle("pre", options, ToastHandleState.Visible, null!);
			using var view2 = new ToastForm(handle2);
			view2.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 1000, false, false);
			Reflect.Invoke(view2, "ToastForm_FormClosing", view2, new FormClosingEventArgs(CloseReason.None, false));
			Reflect.Invoke(view2, "ToastForm_FormClosed", view2, new FormClosedEventArgs(CloseReason.None));
		});
	}

	[Fact]
	public void ToastForm_Input_AutoDismiss_And_Resize()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "tick",
				EnableInput = true,
				AllowEmptySubmit = true,
				IsMuted = true,
				DurationMs = 200
			};
			var handle = new ToastHandle("tick", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.SuccessDark), 200, pauseOnHover: false, playSound: false);
			view.SetBounds(new Rectangle(15, 15, 400, 140));
			view.Show(form);
			Application.DoEvents();

			view.ClientSize = new Size(360, 150);
			Application.DoEvents();
			Reflect.Invoke(view, "LayoutInputPanel");
			Reflect.Invoke(view, "EnsureInputUi");

			PumpUntilDisposed(view, TimeSpan.FromSeconds(3));
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_Shown_DisposesDuringInputFocus()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "focus",
				EnableInput = true,
				IsMuted = true
			};
			var handle = new ToastHandle("focus", options, ToastHandleState.Visible, null!);
			var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 0, false, false);
			view.SetBounds(new Rectangle(10, 10, 400, 140));
			view.Show(form);
			view.Dispose();
			Application.DoEvents();
			Thread.Sleep(50);
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_ContentClick_IgnoredInInputMode()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions { Caption = "in", EnableInput = true, IsMuted = true };
			var handle = new ToastHandle("ign", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			var clicked = 0;
			view.Clicked += (_, _) => clicked++;
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 0, false, false);
			view.SetBounds(new Rectangle(10, 10, 400, 140));
			view.Show(form);
			Application.DoEvents();
			Reflect.Invoke(view, "ToastContentClick", view, EventArgs.Empty);
			Assert.Equal(0, clicked);
			view.BeginDismiss();
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_HoverLeave_WhenPointerInside_And_InputFocused()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions
			{
				Caption = "hov",
				EnableInput = true,
				IsMuted = true,
				DurationMs = 8000
			};
			var handle = new ToastHandle("hov", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.Dark), 8000, pauseOnHover: true, playSound: false);
			view.SetBounds(new Rectangle(50, 50, 400, 140));
			view.Show(form);
			Application.DoEvents();

			Reflect.SetField(view, "_pauseOnHover", true);
			Reflect.SetField(view, "_autoDismissEnabled", true);
			Reflect.SetField(view, "_inputMode", true);
			Reflect.Invoke(view, "ToastForm_MouseEnter", view, EventArgs.Empty);
			Reflect.Invoke(view, "ToastForm_MouseLeave", view, EventArgs.Empty);

			Reflect.SetField(view, "_pauseOnHover", false);
			Reflect.Invoke(view, "ToastForm_MouseEnter", view, EventArgs.Empty);
			Reflect.Invoke(view, "ToastForm_MouseLeave", view, EventArgs.Empty);

			Reflect.SetField(view, "_autoDismissEnabled", false);
			Reflect.Invoke(view, "PauseCountdown");
			Reflect.Invoke(view, "ResumeCountdown");

			view.BeginDismiss();
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void ToastForm_Submit_WhenNotInput_IsNoOp()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var options = new ToastOptions { Caption = "plain", IsMuted = true, CloseStyle = CloseStyle.ClickEntire };
			var handle = new ToastHandle("plain", options, ToastHandleState.Visible, null!);
			using var view = new ToastForm(handle);
			view.Apply(options, ThemeCatalog.Resolve(ToastTheme.WarningDark), 1500, false, false);
			view.SetBounds(new Rectangle(10, 10, 380, 100));
			view.Show(form);
			Application.DoEvents();
			Reflect.Invoke(view, "TrySubmit");
			Reflect.Invoke(view, "BtnSubmit_Click", view, EventArgs.Empty);
			Reflect.Invoke(view, "ToastContentClick", view, EventArgs.Empty);
			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void WinFormsUiMarshaler_Disposed_And_InvokeAsyncFaults()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var m = new WinFormsUiMarshaler(form);

			var faulted = m.InvokeAsync(() => throw new InvalidOperationException("inline"));
			Assert.True(faulted.IsFaulted);

			form.Close();
			form.Dispose();
			_ = m.InvokeRequired;
			Assert.Throws<ObjectDisposedException>(() => m.Invoke(() => { }));
			var disposedTask = m.InvokeAsync(() => { });
			Assert.True(disposedTask.IsFaulted);
		});
	}

	[Fact]
	public void WinFormsUiMarshaler_BeginInvoke_ActionThrows()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			var m = new WinFormsUiMarshaler(form);
			Task? task = null;
			var worker = new Thread(() =>
			{
				task = m.InvokeAsync(() => throw new InvalidOperationException("bg"));
			});
			worker.IsBackground = true;
			worker.Start();
			var sw = System.Diagnostics.Stopwatch.StartNew();
			while ((task is null || !task.IsCompleted) && sw.Elapsed < TimeSpan.FromSeconds(5))
			{
				Application.DoEvents();
				Thread.Sleep(10);
			}
			worker.Join(1000);
			Assert.NotNull(task);
			Assert.True(task!.IsFaulted);
			form.Close();
		});
	}

	private static T? Find<T>(Control root, string name) where T : Control
	{
		var found = root.Controls.Find(name, searchAllChildren: true);
		return found.OfType<T>().FirstOrDefault();
	}

	private static void PumpUntilDisposed(ToastForm view, TimeSpan timeout)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		while (!view.IsDisposed && sw.Elapsed < timeout)
		{
			Application.DoEvents();
			Thread.Sleep(25);
		}
	}

	private sealed class DummyWindow : IWin32Window
	{
		public DummyWindow(IntPtr handle) => Handle = handle;
		public IntPtr Handle { get; }
	}
}
