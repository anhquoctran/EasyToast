using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

/// <summary>WinForms toast surface: touchable layout, hover-pause, no focus steal.</summary>
internal sealed partial class ToastForm : Form, IToastView
{
	private const int AwSlide = 0x40000;
	private const int AwHorPositive = 0x1;
	private const int AwHorNegative = 0x2;
	private const int AwHide = 0x00010000;
	private const int AwBlend = 0x80000;
	private const int WsExNoActivate = 0x08000000;
	private const int WsExToolWindow = 0x00000080;
	private const int WsExTopMost = 0x00000008;

	private readonly ToastHandle _handle;
	private Animation _animation = Animation.Fade;
	private CloseStyle _closeStyle = CloseStyle.ButtonAndClickEntire;
	private bool _pauseOnHover;
	private bool _playSound;
	private bool _ownsThumbnail;
	private Image? _thumbnail;
	private AutoDismissTimerState? _timerState;
	private long _armedAtTick;
	private bool _closedRaised;
	private bool _disposed;
	private bool _closing;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool AnimateWindow(IntPtr hwnd, int time, int flags);

	public ToastForm(ToastHandle handle)
	{
		_handle = handle;
		InitializeComponent();
		// Crisp rendering on Windows 10/11 high-DPI displays
		SetStyle(
			ControlStyles.AllPaintingInWmPaint |
			ControlStyles.OptimizedDoubleBuffer |
			ControlStyles.UserPaint |
			ControlStyles.ResizeRedraw,
			true);
		AutoScaleMode = AutoScaleMode.Dpi;
		Font = SystemFonts.MessageBoxFont ?? new Font("Segoe UI", 9F);
	}

	public ToastHandle ToastHandle => _handle;
	public new bool IsDisposed => _disposed || base.IsDisposed;

	public event EventHandler? ToastClosed;
	public event EventHandler? Clicked;
	public event EventHandler? Hovered;

	event EventHandler? IToastView.Closed
	{
		add => ToastClosed += value;
		remove => ToastClosed -= value;
	}

	protected override bool ShowWithoutActivation => true;

	protected override CreateParams CreateParams
	{
		get
		{
			var cp = base.CreateParams;
			// No activation (don't steal keyboard focus), tool window (no taskbar/Alt-Tab clutter), topmost.
			cp.ExStyle |= WsExNoActivate | WsExToolWindow | WsExTopMost;
			return cp;
		}
	}

	public void Apply(ToastOptions options, ColorScheme scheme, int durationMs, bool pauseOnHover, bool playSound)
	{
		_animation = options.Animation;
		_closeStyle = options.CloseStyle;
		_pauseOnHover = pauseOnHover;
		_playSound = playSound;
		_ownsThumbnail = options.OwnsThumbnail;
		_thumbnail = options.Thumbnail;

		lblCaption.Text = options.Caption?.Trim() ?? string.Empty;
		lblDescription.Text = options.Description?.Trim() ?? string.Empty;

		var hasImage = options.Thumbnail is not null;
		picImage.Image = options.Thumbnail;
		mainContainer.Panel1Collapsed = !hasImage;

		switch (options.CloseStyle)
		{
			case CloseStyle.ClickEntire:
				btnClose.Visible = false;
				break;
			default:
				btnClose.Visible = true;
				break;
		}

		ApplyScheme(scheme);
		_timerState = new AutoDismissTimerState(Math.Max(1, durationMs));
		tmrClose.Interval = Math.Max(1, durationMs);
		tmrClose.Enabled = false;
	}

	public void SetBounds(Rectangle bounds)
	{
		// Bounds are already DPI-scaled by ToastManager (device pixels / WinForms coordinates).
		StartPosition = FormStartPosition.Manual;
		Location = bounds.Location;
		ClientSize = bounds.Size;
	}

	public new void Show(IWin32Window? owner)
	{
		// Owner links Z-order without activating the toast (ShowWithoutActivation + WS_EX_NOACTIVATE).
		if (owner is Control control)
		{
			if (!control.IsDisposed && !control.IsHandleCreated)
				_ = control.Handle;
			if (!control.IsDisposed)
			{
				base.Show(control);
				return;
			}
		}
		else if (owner is not null)
		{
			base.Show(owner);
			return;
		}

		base.Show();
	}

	public void BeginDismiss()
	{
		if (IsDisposed || _closing)
			return;
		if (InvokeRequired)
		{
			try { BeginInvoke(BeginDismiss); }
			catch { /* handle destroyed */ }
			return;
		}

		_closing = true;
		try
		{
			if (!IsDisposed)
				Close();
		}
		catch
		{
			RaiseClosedOnce();
		}
	}

	private void ApplyScheme(ColorScheme scheme)
	{
		var fg = scheme.Foreground;
		var bg = scheme.Background;
		var desc = Color.FromArgb(fg.A, (fg.R * 3 + bg.R) / 4, (fg.G * 3 + bg.G) / 4, (fg.B * 3 + bg.B) / 4);
		lblCaption.ForeColor = fg;
		lblDescription.ForeColor = desc;
		btnClose.ForeColor = fg;
		BackColor = bg;
		btnClose.FlatAppearance.BorderColor = bg;
		btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, fg.R, fg.G, fg.B);
	}

	private void ToastForm_Load(object? sender, EventArgs e)
	{
		if (_playSound)
			TryPlaySound();

		try
		{
			// FADE/SLIDE are aliases of Fade/Slide (same enum values).
			if (_animation is Animation.Fade)
				_ = FadeInAsync();
			else
				AnimateWindow(base.Handle, 250, AwSlide | AwHorNegative);
		}
		catch
		{
			// Animation is best-effort (RDP, locked session, etc.)
			try { Opacity = 1; } catch { /* ignore */ }
		}
	}

	private void ToastForm_Shown(object? sender, EventArgs e)
	{
		if (_timerState is null)
			return;
		tmrClose.Interval = _timerState.StartOrResume();
		_armedAtTick = Environment.TickCount64;
		tmrClose.Start();
	}

	private void ToastForm_FormClosing(object? sender, FormClosingEventArgs e)
	{
		tmrClose.Stop();
		_closing = true;
		// AnimateWindow can fail on some RDP/session scenarios — never block close.
		try
		{
			if (!IsHandleCreated)
				return;
			if (_animation is Animation.Fade)
				AnimateWindow(base.Handle, 200, AwBlend | AwHide);
			else
				AnimateWindow(base.Handle, 200, AwSlide | AwHorPositive | AwHide);
		}
		catch
		{
			// ignore animation failures on Windows 10/11 edge cases
		}
	}

	private void ToastForm_FormClosed(object? sender, FormClosedEventArgs e)
	{
		RaiseClosedOnce();
		DisposeOwnedThumbnail();
	}

	private void TmrClose_Tick(object? sender, EventArgs e)
	{
		tmrClose.Stop();
		_timerState?.OnTimerElapsed();
		BeginDismiss();
	}

	private void BtnClose_Click(object? sender, EventArgs e) => BeginDismiss();

	private void ToastContentClick(object? sender, EventArgs e)
	{
		Clicked?.Invoke(this, EventArgs.Empty);
		if (_closeStyle is CloseStyle.ClickEntire or CloseStyle.ButtonAndClickEntire)
			BeginDismiss();
	}

	private void ToastForm_MouseEnter(object? sender, EventArgs e)
	{
		Hovered?.Invoke(this, EventArgs.Empty);
		if (!_pauseOnHover || _timerState is null)
			return;

		var elapsed = (int)Math.Min(int.MaxValue, Environment.TickCount64 - _armedAtTick);
		tmrClose.Stop();
		_timerState.Pause(elapsed);
	}

	private void ToastForm_MouseLeave(object? sender, EventArgs e)
	{
		if (!_pauseOnHover || _timerState is null || _timerState.IsExpired)
			return;
		if (ClientRectangle.Contains(PointToClient(MousePosition)))
			return;

		tmrClose.Interval = _timerState.Resume();
		_armedAtTick = Environment.TickCount64;
		tmrClose.Start();
	}

	private async Task FadeInAsync()
	{
		try
		{
			Opacity = 0;
			while (Opacity < 1.0 && !IsDisposed)
			{
				await Task.Delay(8).ConfigureAwait(true);
				if (IsDisposed)
					return;
				Opacity = Math.Min(1.0, Opacity + 0.08);
			}
			if (!IsDisposed)
				Opacity = 1;
		}
		catch
		{
			// form disposed mid-fade
		}
	}

	private void TryPlaySound()
	{
		// Sound is best-effort; failures must never break toast display (locked-down Win10/11, no audio device).
		try
		{
			var stream = Properties.Resources.notificationSound;
			if (stream is null)
				return;

			// UnmanagedMemoryStream from resources may not seek; copy for SoundPlayer.
			using var copy = new MemoryStream();
			stream.Position = 0;
			stream.CopyTo(copy);
			copy.Position = 0;
			var player = new SoundPlayer(copy);
			player.Play(); // async; do not Dispose player immediately (would cut off sound)
			// Keep stream alive until GC; SoundPlayer holds the stream reference.
			GC.KeepAlive(player);
		}
		catch
		{
			// optional sound
		}
	}

	private void RaiseClosedOnce()
	{
		if (_closedRaised)
			return;
		_closedRaised = true;
		ToastClosed?.Invoke(this, EventArgs.Empty);
	}

	private void DisposeOwnedThumbnail()
	{
		if (!_ownsThumbnail || _thumbnail is null)
			return;
		try { _thumbnail.Dispose(); } catch { /* ignore */ }
		_thumbnail = null;
		_ownsThumbnail = false;
	}
}
