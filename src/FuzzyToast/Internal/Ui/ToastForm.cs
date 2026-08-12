using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

/// <summary>WinForms toast surface: touchable layout, optional input+submit, hover-pause.</summary>
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
	private bool _inputMode;
	private bool _allowEmptySubmit;
	private bool _activateForInput;

	private Panel? _inputPanel;
	private TextBox? _txtInput;
	private Button? _btnSubmit;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool AnimateWindow(IntPtr hwnd, int time, int flags);

	public ToastForm(ToastHandle handle)
	{
		_handle = handle;
		InitializeComponent();
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
	public event EventHandler<string>? Submitted;

	event EventHandler? IToastView.Closed
	{
		add => ToastClosed += value;
		remove => ToastClosed -= value;
	}

	/// <summary>Inputable toasts must activate so the user can type.</summary>
	protected override bool ShowWithoutActivation => !_activateForInput;

	protected override CreateParams CreateParams
	{
		get
		{
			var cp = base.CreateParams;
			cp.ExStyle |= WsExToolWindow | WsExTopMost;
			if (!_activateForInput)
				cp.ExStyle |= WsExNoActivate;
			return cp;
		}
	}

	public void Apply(ToastOptions options, ColorScheme scheme, int durationMs, bool pauseOnHover, bool playSound)
	{
		_animation = options.Animation;
		_closeStyle = options.CloseStyle;
		_pauseOnHover = pauseOnHover && !options.EnableInput; // keep timer running while typing; long duration instead
		_playSound = playSound;
		_ownsThumbnail = options.OwnsThumbnail;
		_thumbnail = options.Thumbnail;
		_inputMode = options.EnableInput;
		_allowEmptySubmit = options.AllowEmptySubmit;
		_activateForInput = options.EnableInput;

		lblCaption.Text = options.Caption?.Trim() ?? string.Empty;
		lblDescription.Text = options.Description?.Trim() ?? string.Empty;

		var hasImage = options.Thumbnail is not null;
		picImage.Image = options.Thumbnail;
		mainContainer.Panel1Collapsed = !hasImage;

		switch (options.CloseStyle)
		{
			case CloseStyle.ClickEntire:
				btnClose.Visible = !options.EnableInput; // keep close for input mode always useful
				if (options.EnableInput)
					btnClose.Visible = true;
				break;
			default:
				btnClose.Visible = true;
				break;
		}

		if (options.EnableInput)
		{
			// Body click should not dismiss input toasts (would lose typed text).
			_closeStyle = CloseStyle.Button;
			EnsureInputUi();
			if (_inputPanel is not null && _txtInput is not null && _btnSubmit is not null)
			{
				_inputPanel.Visible = true;
				_txtInput.PlaceholderText = options.InputPlaceholder ?? string.Empty;
				_txtInput.Text = options.InputDefaultText ?? string.Empty;
				_btnSubmit.Text = string.IsNullOrWhiteSpace(options.SubmitButtonText)
					? "OK"
					: options.SubmitButtonText;
			}
		}
		else if (_inputPanel is not null)
		{
			_inputPanel.Visible = false;
		}

		ApplyScheme(scheme);
		_timerState = new AutoDismissTimerState(Math.Max(1, durationMs));
		tmrClose.Interval = Math.Max(1, durationMs);
		tmrClose.Enabled = false;
	}

	public void SetBounds(Rectangle bounds)
	{
		StartPosition = FormStartPosition.Manual;
		Location = bounds.Location;
		ClientSize = bounds.Size;
		LayoutInputPanel();
	}

	public new void Show(IWin32Window? owner)
	{
		if (owner is Control control)
		{
			if (!control.IsDisposed && !control.IsHandleCreated)
				_ = control.Handle;
			if (!control.IsDisposed)
			{
				base.Show(control);
				if (_activateForInput && _txtInput is not null)
				{
					try
					{
						Activate();
						_txtInput.Focus();
						_txtInput.SelectAll();
					}
					catch { /* ignore focus failures */ }
				}
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

	private void EnsureInputUi()
	{
		if (_inputPanel is not null)
			return;

		_inputPanel = new Panel
		{
			Name = "inputPanel",
			Height = 40,
			Dock = DockStyle.Bottom,
			Padding = new Padding(8, 0, 8, 8),
			BackColor = Color.Transparent
		};

		_txtInput = new TextBox
		{
			Name = "txtInput",
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point),
			BorderStyle = BorderStyle.FixedSingle,
			// leave room for submit button
		};
		_txtInput.KeyDown += TxtInput_KeyDown;
		_txtInput.GotFocus += (_, _) => PauseTimerForInput();
		_txtInput.Click += (_, _) => { /* don't bubble as toast content click */ };

		_btnSubmit = new Button
		{
			Name = "btnSubmit",
			Text = "OK",
			Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point),
			FlatStyle = FlatStyle.Flat,
			Cursor = Cursors.Hand,
			Size = new Size(72, 28),
			TabIndex = 2
		};
		_btnSubmit.FlatAppearance.BorderSize = 0;
		_btnSubmit.Click += BtnSubmit_Click;

		_inputPanel.Controls.Add(_txtInput);
		_inputPanel.Controls.Add(_btnSubmit);
		// Add to form (above fill dock of mainContainer — dock bottom first works if added after)
		Controls.Add(_inputPanel);
		_inputPanel.BringToFront();
		LayoutInputPanel();
	}

	private void LayoutInputPanel()
	{
		if (_inputPanel is null || _txtInput is null || _btnSubmit is null || !_inputPanel.Visible)
			return;

		var pad = 8;
		var btnW = Math.Max(64, _btnSubmit.Width);
		var h = 28;
		_btnSubmit.Size = new Size(btnW, h);
		_btnSubmit.Location = new Point(_inputPanel.ClientSize.Width - pad - btnW, pad);
		_txtInput.Location = new Point(pad, pad);
		_txtInput.Size = new Size(Math.Max(40, _btnSubmit.Left - pad - 6), h);
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

		if (_btnSubmit is not null)
		{
			_btnSubmit.BackColor = Color.FromArgb(
				Math.Min(255, bg.R + 40),
				Math.Min(255, bg.G + 40),
				Math.Min(255, bg.B + 40));
			_btnSubmit.ForeColor = fg;
			_btnSubmit.FlatAppearance.BorderColor = bg;
		}

		if (_txtInput is not null)
		{
			_txtInput.BackColor = Color.FromArgb(
				Math.Min(255, bg.R + 20),
				Math.Min(255, bg.G + 20),
				Math.Min(255, bg.B + 20));
			_txtInput.ForeColor = fg;
		}
	}

	private void ToastForm_Load(object? sender, EventArgs e)
	{
		if (_playSound)
			TryPlaySound();

		try
		{
			if (_animation is Animation.Fade)
				_ = FadeInAsync();
			else
				AnimateWindow(base.Handle, 250, AwSlide | AwHorNegative);
		}
		catch
		{
			try { Opacity = 1; } catch { /* ignore */ }
		}

		LayoutInputPanel();
	}

	private void ToastForm_Shown(object? sender, EventArgs e)
	{
		if (_timerState is null)
			return;
		tmrClose.Interval = _timerState.StartOrResume();
		_armedAtTick = Environment.TickCount64;
		tmrClose.Start();

		if (_activateForInput && _txtInput is not null)
		{
			try
			{
				_txtInput.Focus();
				_txtInput.SelectAll();
			}
			catch { /* ignore */ }
		}
	}

	private void ToastForm_FormClosing(object? sender, FormClosingEventArgs e)
	{
		tmrClose.Stop();
		_closing = true;
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
			// ignore
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

	private void BtnSubmit_Click(object? sender, EventArgs e) => TrySubmit();

	private void TxtInput_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Enter)
		{
			e.SuppressKeyPress = true;
			e.Handled = true;
			TrySubmit();
		}
		else if (e.KeyCode == Keys.Escape)
		{
			e.SuppressKeyPress = true;
			e.Handled = true;
			BeginDismiss();
		}
	}

	private void TrySubmit()
	{
		if (!_inputMode || _txtInput is null)
			return;

		var text = _txtInput.Text ?? string.Empty;
		if (!_allowEmptySubmit && string.IsNullOrWhiteSpace(text))
		{
			_txtInput.Focus();
			return;
		}

		try
		{
			Submitted?.Invoke(this, text);
		}
		catch
		{
			/* ignore */
		}

		BeginDismiss();
	}

	private void PauseTimerForInput()
	{
		if (_timerState is null)
			return;
		var elapsed = (int)Math.Min(int.MaxValue, Environment.TickCount64 - _armedAtTick);
		tmrClose.Stop();
		_timerState.Pause(elapsed);
		// Resume with remaining when leave focus — keep long remaining for input
		_armedAtTick = Environment.TickCount64;
	}

	private void ToastContentClick(object? sender, EventArgs e)
	{
		if (_inputMode)
			return; // never dismiss by body click when typing

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

		// If input focused, keep paused
		if (_inputMode && _txtInput is { Focused: true })
			return;

		tmrClose.Interval = _timerState.Resume();
		_armedAtTick = Environment.TickCount64;
		tmrClose.Start();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		LayoutInputPanel();
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
		try
		{
			var stream = Properties.Resources.notificationSound;
			if (stream is null)
				return;

			using var copy = new MemoryStream();
			stream.Position = 0;
			stream.CopyTo(copy);
			copy.Position = 0;
			var player = new SoundPlayer(copy);
			player.Play();
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
