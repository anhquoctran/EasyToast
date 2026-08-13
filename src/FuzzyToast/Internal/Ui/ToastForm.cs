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
	private bool _autoDismissEnabled;
	private int _remainingMs;
	private bool _countdownPaused;
	private const int CountdownTickMs = 250;

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

		// Outer inset always comes from contentShell.Padding (parent panel).
		ApplyContentShellPadding();

		if (options.EnableInput)
		{
			// Body click should not dismiss input toasts (would lose typed text).
			_closeStyle = CloseStyle.Button;
			EnsureInputUi();
			if (_inputPanel is not null && _txtInput is not null && _btnSubmit is not null)
			{
				_inputPanel.Visible = true;
				_inputPanel.Height = 34;
				// Horizontal inset already on contentShell — only small gap above input.
				_inputPanel.Padding = new Padding(0, 6, 0, 0);
				NativeMethods.SetCueBanner(_txtInput, options.InputPlaceholder ?? string.Empty);
				_txtInput.MaxLength = ToastLimits.MaxInputTextLength;
				_txtInput.Text = options.InputDefaultText ?? string.Empty;
				_btnSubmit.Text = string.IsNullOrWhiteSpace(options.SubmitButtonText)
					? "OK"
					: options.SubmitButtonText;
			}
			textContainer.SplitterDistance = 28;
			textContainer.Panel1.Padding = new Padding(2, 0, 0, 2);
			textContainer.Panel2.Padding = new Padding(2, 2, 2, 0);
			lblDescription.AutoEllipsis = true;
		}
		else
		{
			if (_inputPanel is not null)
				_inputPanel.Visible = false;
			textContainer.SplitterDistance = 28;
			textContainer.Panel1.Padding = new Padding(2, 0, 0, 2);
			textContainer.Panel2.Padding = new Padding(2, 2, 2, 0);
		}

		// Collapse description band when empty.
		var hasDescription = !string.IsNullOrWhiteSpace(options.Description);
		lblDescription.Visible = hasDescription;
		if (!hasDescription && !options.EnableInput)
		{
			textContainer.SplitterDistance = Math.Max(28, textContainer.Height - 4);
		}

		ApplyScheme(scheme);

		// durationMs == 0 → stay open until Submit / Esc / close (typical for inputable).
		_autoDismissEnabled = durationMs > 0;
		_remainingMs = _autoDismissEnabled ? durationMs : 0;
		_countdownPaused = false;
		_timerState = _autoDismissEnabled ? new AutoDismissTimerState(durationMs) : null;
		tmrClose.Stop();
		tmrClose.Interval = CountdownTickMs;
		tmrClose.Enabled = false;

		// Inputable toasts must be fully visible immediately (no fade-from-zero).
		if (_inputMode)
		{
			try { Opacity = 1; } catch { /* ignore */ }
		}
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
			try { NativeMethods.BeginInvokeOn(this, BeginDismiss); }
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

	/// <summary>Parent shell padding: keeps every child inset from the toast outer edge.</summary>
	private void ApplyContentShellPadding()
	{
		// LTRB — breathing room around the whole card content (normal + inputable).
		contentShell.Padding = new Padding(12, 10, 12, 10);
	}

	private void EnsureInputUi()
	{
		if (_inputPanel is not null)
			return;

		_inputPanel = new Panel
		{
			Name = "inputPanel",
			Height = 34,
			Dock = DockStyle.Bottom,
			// Edge inset is contentShell.Padding; only separate input from description above.
			Padding = new Padding(0, 6, 0, 0),
			BackColor = Color.Transparent
		};

		_txtInput = new TextBox
		{
			Name = "txtInput",
			Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
			BorderStyle = BorderStyle.FixedSingle,
			MaxLength = ToastLimits.MaxInputTextLength
		};
		_txtInput.KeyDown += TxtInput_KeyDown;
		_txtInput.GotFocus += (_, _) => PauseCountdown();
		_txtInput.LostFocus += (_, _) =>
		{
			// Only resume if focus left the toast entirely (not to Submit button).
			NativeMethods.BeginInvokeOn(this, () =>
			{
				if (IsDisposed || _btnSubmit is { Focused: true } || _txtInput is { Focused: true })
					return;
				ResumeCountdown();
			});
		};
		_txtInput.Click += (_, _) => { /* don't bubble as toast content click */ };

		_btnSubmit = new Button
		{
			Name = "btnSubmit",
			Text = "OK",
			Font = new Font("Segoe UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point),
			FlatStyle = FlatStyle.Flat,
			Cursor = Cursors.Hand,
			Size = new Size(64, 24),
			TabIndex = 2
		};
		_btnSubmit.FlatAppearance.BorderSize = 0;
		_btnSubmit.Click += BtnSubmit_Click;

		_inputPanel.Controls.Add(_txtInput);
		_inputPanel.Controls.Add(_btnSubmit);
		// Inside parent shell so outer padding applies to input row as well.
		contentShell.Controls.Add(_inputPanel);
		_inputPanel.BringToFront();
		mainContainer.BringToFront(); // Fill remaining space above bottom input
		// Dock order: bottom first, then fill — re-add main for correct dock
		contentShell.Controls.SetChildIndex(_inputPanel, 0);
		contentShell.Controls.SetChildIndex(mainContainer, 1);
		LayoutInputPanel();
		WireHover(_inputPanel);
		WireHover(_txtInput);
		WireHover(_btnSubmit);
	}

	private void LayoutInputPanel()
	{
		if (_inputPanel is null || _txtInput is null || _btnSubmit is null || !_inputPanel.Visible)
			return;

		// Client area already has panel Padding; layout children inside content box.
		var gap = 6;
		var btnW = 64;
		var h = 24;
		var contentW = _inputPanel.ClientSize.Width;
		var contentH = _inputPanel.ClientSize.Height;
		var y = Math.Max(0, (contentH - h) / 2);
		_btnSubmit.Size = new Size(btnW, h);
		_btnSubmit.Location = new Point(Math.Max(0, contentW - btnW), y);
		_txtInput.Location = new Point(0, y);
		_txtInput.Size = new Size(Math.Max(40, _btnSubmit.Left - gap), h);
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
			// Inputable: always fully opaque (fade-from-0 made toasts "vanish" before users could type).
			if (_inputMode || _animation is not Animation.Fade)
			{
				Opacity = 1;
				if (!_inputMode && _animation is not Animation.Fade)
					AnimateWindow(base.Handle, 250, AwSlide | AwHorNegative);
			}
			else
			{
				_ = FadeInAsync();
			}
		}
		catch
		{
			try { Opacity = 1; } catch { /* ignore */ }
		}

		LayoutInputPanel();
	}

	private void ToastForm_Shown(object? sender, EventArgs e)
	{
		if (_autoDismissEnabled && _remainingMs > 0)
		{
			_countdownPaused = false;
			_armedAtTick = NativeMethods.TickCount64;
			tmrClose.Interval = CountdownTickMs;
			tmrClose.Start();
		}

		if (_activateForInput && _txtInput is not null)
		{
			// Delay focus one tick so the form is fully shown first.
			NativeMethods.BeginInvokeOn(this, () =>
			{
				try
				{
					if (IsDisposed || _txtInput is null)
						return;
					Activate();
					_txtInput.Focus();
					_txtInput.SelectAll();
					// While typing, pause auto-dismiss countdown.
					PauseCountdown();
				}
				catch { /* ignore */ }
			});
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
		if (!_autoDismissEnabled || _countdownPaused)
			return;

		_remainingMs -= CountdownTickMs;
		if (_remainingMs > 0)
			return;

		tmrClose.Stop();
		_timerState?.OnTimerElapsed();
		BeginDismiss();
	}

	private void PauseCountdown()
	{
		if (!_autoDismissEnabled)
			return;
		_countdownPaused = true;
		// Keep timer running so resume is simple; Tick no-ops while paused.
	}

	private void ResumeCountdown()
	{
		if (!_autoDismissEnabled || _remainingMs <= 0)
			return;
		_countdownPaused = false;
		if (!tmrClose.Enabled)
		{
			tmrClose.Interval = CountdownTickMs;
			tmrClose.Start();
		}
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
		if (!_pauseOnHover || !_autoDismissEnabled)
			return;
		PauseCountdown();
	}

	private void ToastForm_MouseLeave(object? sender, EventArgs e)
	{
		if (!_pauseOnHover || !_autoDismissEnabled)
			return;
		if (ClientRectangle.Contains(PointToClient(MousePosition)))
			return;
		if (_inputMode && (_txtInput is { Focused: true } || _btnSubmit is { Focused: true }))
			return;
		ResumeCountdown();
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
