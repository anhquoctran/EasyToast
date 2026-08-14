using System.Drawing;
using System.Runtime.Versioning;
using FuzzyToast;
using FuzzyToast.Internal;
using FuzzyToast.Layout;

namespace EasyToastDemo;

/// <summary>
/// Interactive catalog of every public FuzzyToast 3.x API on Windows 10/11 (.NET 8+ WinForms).
/// Shared defaults (theme, close style, position, mute) apply unless a button documents an override.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class Form1 : Form
{
	private static readonly Size DesignClientSize = new(1080, 720);
	private static readonly Font UiFont = UiTheme.Ui;
	private static readonly Font HintFont = UiTheme.Hint;

	private ToastManager _toasts = null!;
	private Toast? _lastToast;
	private ToastHandle? _lastHandle;
	private CancellationTokenSource? _showCts;
	private Form? _secondOwner;
	private AutoDismissTimerState? _timerState;
	private int _timerArmedTick;
	private System.Windows.Forms.Timer? _timerUi;
	private Color _customBg = Color.FromArgb(103, 58, 183);
	private Color _customFg = Color.White;

	// Shared defaults
	private ComboBox _cbTheme = null!;
	private ComboBox _cbClose = null!;
	private ComboBox _cbPosition = null!;
	private CheckBox _chkMute = null!;
	private CheckBox _chkShowProgressBar = null!;
	private Label _lblStatus = null!;
	private RichTextBox _log = null!;

	// Basics
	private TextBox _txtCaption = null!;
	private TextBox _txtDescription = null!;
	private RadioButton _rShort = null!;
	private RadioButton _rLong = null!;
	private RadioButton _rInput = null!;
	private CheckBox _chkUseMs = null!;
	private NumericUpDown _numDurationMs = null!;
	private RadioButton _rFade = null!;
	private RadioButton _rSlide = null!;
	private PictureBox _picThumb = null!;
	private TextBox _txtThumbCaption = null!;

	// Appearance
	private Panel _pnlPreview = null!;
	private Label _lblPreview = null!;
	private Button _btnPickBg = null!;
	private Button _btnPickFg = null!;

	// Stack / manager
	private NumericUpDown _numStack = null!;
	private ComboBox _cbOverflow = null!;
	private NumericUpDown _numMaxToasts = null!;
	private NumericUpDown _numMaxPerPos = null!;
	private CheckBox _chkPauseHover = null!;
	private CheckBox _chkPlaySound = null!;
	private CheckBox _chkHideImage = null!;
	private NumericUpDown _numShortMs = null!;
	private NumericUpDown _numLongMs = null!;
	private NumericUpDown _numInputMs = null!;
	private NumericUpDown _numToastW = null!;
	private NumericUpDown _numToastH = null!;
	private NumericUpDown _numHMargin = null!;
	private NumericUpDown _numVMargin = null!;
	private NumericUpDown _numStackGap = null!;
	private NumericUpDown _numInputHeight = null!;
	private NumericUpDown _numInputExtra = null!;
	private Label _lblActive = null!;

	// Inputable
	private TextBox _txtInCaption = null!;
	private TextBox _txtInPlaceholder = null!;
	private TextBox _txtInDefault = null!;
	private TextBox _txtInSubmit = null!;
	private CheckBox _chkAllowEmpty = null!;
	private CheckBox _chkStayOpen = null!;
	private CheckBox _chkThenDisableInput = null!;
	private NumericUpDown _numInputTimeout = null!;

	// Utilities
	private Label _lblTimer = null!;

	public Form1()
	{
		AutoScaleMode = AutoScaleMode.None;
		InitializeComponent();
		AutoScaleMode = AutoScaleMode.None;
		MinimumSize = Size.Empty;
		MaximumSize = Size.Empty;
		ClientSize = DesignClientSize;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		MaximizeBox = false;
		StartPosition = FormStartPosition.CenterScreen;
		Text = "FuzzyToast Demo v3 — full public API";
		Font = UiFont;
		BackColor = UiTheme.Canvas;
		ForeColor = UiTheme.Text;
		BuildUi();
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		try { _showCts?.Cancel(); } catch { /* ignore */ }
		_showCts?.Dispose();
		_timerUi?.Stop();
		_timerUi?.Dispose();
		try { _secondOwner?.Close(); } catch { /* ignore */ }
		try { _toasts?.Dispose(); } catch { /* ignore dispose races */ }
		DisposeThumbnail();
		base.OnFormClosed(e);
	}

	private void Form1_Load(object? sender, EventArgs e)
	{
		try
		{
			var work = Screen.FromControl(this).WorkingArea;
			var w = Math.Min(DesignClientSize.Width, Math.Max(860, work.Width - 80));
			var h = Math.Min(DesignClientSize.Height, Math.Max(560, work.Height - 80));
			MinimumSize = Size.Empty;
			MaximumSize = Size.Empty;
			ClientSize = new Size(w, h);
			var x = Math.Max(work.Left, Math.Min(Left, work.Right - Width));
			var y = Math.Max(work.Top, Math.Min(Top, work.Bottom - Height));
			Location = new Point(x, y);
		}
		catch
		{
			ClientSize = DesignClientSize;
		}

		RecreateManager(announce: false);
		RefreshThemePreview();
		Log("Demo ready — FuzzyToast v3 catalog · every public API has a button on these tabs.");
		UpdateStatus();
	}

	// --- UI construction ---

	private void BuildUi()
	{
		var menu = new MenuStrip
		{
			BackColor = UiTheme.Card,
			ForeColor = UiTheme.Text,
			Font = UiFont,
			Renderer = new FlatToolStripRenderer(),
			Padding = new Padding(6, 2, 0, 2)
		};
		var help = new ToolStripMenuItem("Help");
		var about = new ToolStripMenuItem("About");
		about.Click += About_Click;
		help.DropDownItems.Add(about);
		menu.Items.Add(help);
		MainMenuStrip = menu;

		var defaults = BuildDefaultsBar();
		defaults.Dock = DockStyle.Top;

		var logHost = new Panel { Dock = DockStyle.Bottom, Height = 164, Padding = new Padding(12, 0, 12, 10), BackColor = UiTheme.Canvas };
		var logBox = new FlatCard("Live event log  ·  ToastAdded / Removed / Rejected / click / hover / submit")
		{
			Dock = DockStyle.Fill
		};
		_log = new RichTextBox
		{
			Dock = DockStyle.Fill,
			ReadOnly = true,
			BackColor = UiTheme.Card,
			ForeColor = UiTheme.Text,
			Font = new Font("Consolas", 8.25F),
			BorderStyle = BorderStyle.None
		};
		logBox.Controls.Add(_log);
		logBox.SetHeaderAction(Btn("Clear log", 0, 0, 124, 24, (_, _) => _log.Clear(), BtnKind.Ghost));
		logHost.Controls.Add(logBox);

		var tabHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 8), BackColor = UiTheme.Canvas };
		var tabs = new FlatTabStrip(tabHost);
		tabs.Add("Basics", BuildBasicsTab());
		tabs.Add("Appearance", BuildAppearanceTab());
		tabs.Add("Stack & manager", BuildStackTab());
		tabs.Add("Inputable", BuildInputTab());
		tabs.Add("Lifecycle", BuildLifecycleTab());
		tabs.Add("Utilities", BuildUtilitiesTab());

		// Dock z-order: Fill first, then Bottom, then Top (tab strip, defaults), MenuStrip last.
		Controls.Add(tabHost);
		Controls.Add(logHost);
		Controls.Add(tabs);
		Controls.Add(defaults);
		Controls.Add(menu);
	}

	private Panel BuildDefaultsBar()
	{
		var bar = new Panel
		{
			Height = 68,
			BackColor = UiTheme.Card,
			Padding = new Padding(12, 8, 12, 8)
		};
		bar.Paint += (_, e) =>
		{
			using var pen = new Pen(UiTheme.Border);
			e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
		};

		bar.Controls.Add(Lbl("Theme", 12, 10));
		_cbTheme = Combo(70, 6, 170);
		FillEnum(_cbTheme, ToastTheme.Dark);
		_cbTheme.SelectedIndexChanged += (_, _) => RefreshThemePreview();
		bar.Controls.Add(_cbTheme);

		bar.Controls.Add(Lbl("Close style", 256, 10));
		_cbClose = Combo(332, 6, 180);
		FillEnum(_cbClose, CloseStyle.ButtonAndClickEntire);
		bar.Controls.Add(_cbClose);

		bar.Controls.Add(Lbl("Position", 528, 10));
		_cbPosition = Combo(586, 6, 130);
		FillEnum(_cbPosition, ToastPosition.BottomRight);
		bar.Controls.Add(_cbPosition);

		_chkMute = Chk("Mute this toast", 736, 9, on: true);
		bar.Controls.Add(_chkMute);

		_chkShowProgressBar = Chk("Progress bar", 866, 9, on: true);
		bar.Controls.Add(_chkShowProgressBar);

		_lblStatus = new Label
		{
			AutoSize = false,
			Location = new Point(12, 38),
			Size = new Size(1050, 22),
			ForeColor = UiTheme.Muted,
			Font = HintFont,
			BackColor = UiTheme.Card,
			Text = "Shared defaults apply to most Show buttons. Recreate the manager on the Stack tab to change capacity / sound."
		};
		bar.Controls.Add(_lblStatus);
		return bar;
	}

	private Panel BuildBasicsTab()
	{
		var host = ScrollHost();

		var gBuild = Box("Toast.Build + Show / ShowAsync", 8, 8, 500, 210);
		_txtCaption = Txt(12, 42, 476, "Hello, I am Toast!");
		_txtDescription = Txt(12, 90, 476, "Click me — Tag + Metadata are returned in OnClick");
		gBuild.Controls.Add(Lbl("Caption  (Build overloads set this)", 12, 22));
		gBuild.Controls.Add(_txtCaption);
		gBuild.Controls.Add(Lbl("Description  (Build(owner, caption, description))", 12, 70));
		gBuild.Controls.Add(_txtDescription);
		gBuild.Controls.Add(Btn("Show()", 12, 126, 150, 28, BtnShow_Click, BtnKind.Primary));
		gBuild.Controls.Add(Btn("ShowAsync()", 170, 126, 150, 28, BtnShowAsync_Click, BtnKind.Primary));
		gBuild.Controls.Add(Btn("Build(caption, muting)", 328, 126, 160, 28, BtnShowMutingOverload_Click));
		gBuild.Controls.Add(Hint("Also wires OnClick, OnHover, OnClosed, SetTag/SetData, SetMetadata/SetExtData.", 12, 162, 476));
		host.Controls.Add(gBuild);

		var gDur = Box("Duration + Animation", 520, 8, 516, 210);
		var pnlDur = new Panel { Location = new Point(0, 16), Size = new Size(500, 30) };
		_rShort = Radio("Short  (~2s)", 12, 8, true);
		_rLong = Radio("Long  (~3s)", 130, 8, false);
		_rInput = Radio("Input  (InputDurationMs)", 250, 8, false);
		pnlDur.Controls.Add(_rShort);
		pnlDur.Controls.Add(_rLong);
		pnlDur.Controls.Add(_rInput);
		gDur.Controls.Add(pnlDur);

		var pnlAnim = new Panel { Location = new Point(0, 46), Size = new Size(190, 30) };
		_rFade = Radio("Fade", 12, 8, true);
		_rSlide = Radio("Slide", 90, 8, false);
		pnlAnim.Controls.Add(_rFade);
		pnlAnim.Controls.Add(_rSlide);
		gDur.Controls.Add(pnlAnim);

		_chkUseMs = Chk("SetDurationMs", 200, 54);
		_numDurationMs = Num(330, 50, 90, 0, ToastLimits.MaxDurationMs, 4000);
		gDur.Controls.Add(_chkUseMs);
		gDur.Controls.Add(_numDurationMs);
		gDur.Controls.Add(Btn("Build(caption, Duration)", 12, 92, 240, 28, BtnShowDuration_Click));
		gDur.Controls.Add(Btn("Build(caption, Animation)", 264, 92, 236, 28, BtnShowAnimation_Click));
		gDur.Controls.Add(Btn("Build(caption, Duration, Animation)", 12, 128, 488, 28, BtnShowDurationAndAnimation_Click, BtnKind.Primary));
		gDur.Controls.Add(Hint("SetDurationMs(0) stays open. Negative values throw before show.", 12, 164, 488));
		host.Controls.Add(gDur);

		// Keep this block compact: the Basics tab is the tallest, and a 168px box
		// clipped the second hint against the GroupBox border (looked faded / cut off).
		var gImg = Box("Thumbnail + ImageValidation", 8, 226, 1028, 154);
		_picThumb = new PictureBox
		{
			BorderStyle = BorderStyle.None,
			BackColor = UiTheme.Canvas,
			Location = new Point(12, 24),
			Size = new Size(80, 80),
			SizeMode = PictureBoxSizeMode.Zoom
		};
		_picThumb.Paint += (_, e) =>
		{
			using var pen = new Pen(UiTheme.Border);
			e.Graphics.DrawRectangle(pen, 0, 0, _picThumb.Width - 1, _picThumb.Height - 1);
		};
		_txtThumbCaption = Txt(104, 24, 908, "Hello! I'm Toast :)");
		gImg.Controls.Add(_picThumb);
		gImg.Controls.Add(_txtThumbCaption);
		gImg.Controls.Add(Btn("Choose image…", 104, 54, 140, 28, BtnChooseImage_Click));
		gImg.Controls.Add(Btn("Build(caption, Image)", 254, 54, 180, 28, BtnShowThumbnail_Click, BtnKind.Primary));
		gImg.Controls.Add(Btn("Build(caption, Image, Duration, Animation, mute)", 444, 54, 360, 28, BtnShowThumbnailFull_Click));
		gImg.Controls.Add(Hint(
			"ValidateImagePath (PNG/JPEG magic) · ValidateImageSize · SetThumbnail(ownsImage: true).",
			104, 90, 908, 18));
		gImg.Controls.Add(Hint(
			"Required minimum 64×64. Recommended 80×80 square. Max ToastLimits.MaxImageDimension. JPEG and PNG.",
			104, 110, 908, 18));
		host.Controls.Add(gImg);
		FinishScrollPage(host);
		return host;
	}

	private Panel BuildAppearanceTab()
	{
		var host = ScrollHost();

		var gTheme = Box("ThemeCatalog + ToastTheme.Custom", 8, 8, 1028, 220);
		_pnlPreview = new Panel
		{
			Location = new Point(12, 28),
			Size = new Size(280, 80),
			BorderStyle = BorderStyle.None,
			BackColor = Color.FromArgb(33, 33, 33)
		};
		_lblPreview = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter,
			ForeColor = Color.White,
			Font = new Font("Segoe UI", 11F, FontStyle.Bold),
			Text = "Theme preview"
		};
		_pnlPreview.Controls.Add(_lblPreview);
		gTheme.Controls.Add(_pnlPreview);
		gTheme.Controls.Add(Hint("Uses ThemeCatalog.Resolve(theme, custom). Custom requires a ColorScheme.", 12, 116, 280, 36));

		_btnPickBg = Btn("Background…", 310, 28, 140, 28, (_, _) => PickColor(ref _customBg, _btnPickBg));
		_btnPickFg = Btn("Foreground…", 310, 64, 140, 28, (_, _) => PickColor(ref _customFg, _btnPickFg));
		PaintColorButton(_btnPickBg, _customBg);
		PaintColorButton(_btnPickFg, _customFg);
		gTheme.Controls.Add(_btnPickBg);
		gTheme.Controls.Add(_btnPickFg);
		gTheme.Controls.Add(Btn("SetCustomColors(bg, fg)", 460, 28, 250, 28, BtnShowCustomRgb_Click));
		gTheme.Controls.Add(Btn("SetCustomColors(ColorScheme)", 460, 64, 250, 28, BtnShowCustomScheme_Click));
		gTheme.Controls.Add(Btn("ColorScheme(byte r,g,b…)", 720, 28, 220, 28, BtnShowRgbCtor_Click));
		gTheme.Controls.Add(Btn("Resolve every ToastTheme", 720, 64, 220, 28, BtnResolveAllThemes_Click));
		gTheme.Controls.Add(Btn("Build(caption, ToastTheme)", 460, 100, 250, 28, BtnBuildWithTheme_Click, BtnKind.Primary));
		gTheme.Controls.Add(Hint(
			"Toast.SetCustomColors(Color, Color) and ToastBuilder.SetCustomColors(ColorScheme) both set Theme = Custom. Pick Custom in the top Theme combo to apply these colors on other Show buttons.",
			310, 136, 690, 36));
		host.Controls.Add(gTheme);

		var gClose = Box("CloseStyle", 8, 236, 500, 130);
		gClose.Controls.Add(Btn("ClickEntire", 12, 28, 150, 28, (_, _) => ShowCloseStyle(CloseStyle.ClickEntire)));
		gClose.Controls.Add(Btn("Button", 174, 28, 150, 28, (_, _) => ShowCloseStyle(CloseStyle.Button)));
		gClose.Controls.Add(Btn("ButtonAndClickEntire", 336, 28, 150, 28, (_, _) => ShowCloseStyle(CloseStyle.ButtonAndClickEntire)));
		gClose.Controls.Add(Hint("ClickEntire hides ✕ (except inputable). Button keeps body clicks for OnClick.", 12, 68, 470));
		host.Controls.Add(gClose);

		var gSound = Box("Sound  (SetMuting + manager PlaySound)", 520, 236, 516, 130);
		gSound.Controls.Add(Btn("Muted toast", 12, 28, 150, 28, (_, _) => ShowSound(muted: true)));
		gSound.Controls.Add(Btn("Unmuted toast", 174, 28, 150, 28, (_, _) => ShowSound(muted: false)));
		gSound.Controls.Add(Hint("Sound plays only when manager PlaySound is true and the toast is not muted. Apply PlaySound on the Stack tab.", 12, 68, 488, 36));
		host.Controls.Add(gSound);
		FinishScrollPage(host);
		return host;
	}

	private Panel BuildStackTab()
	{
		var host = ScrollHost();

		var gPos = Box("ToastPosition — four independent stacks", 8, 8, 500, 150);
		gPos.Controls.Add(Btn("TopLeft", 12, 28, 110, 32, (_, _) => ShowPositioned(ToastPosition.TopLeft)));
		gPos.Controls.Add(Btn("TopRight", 132, 28, 110, 32, (_, _) => ShowPositioned(ToastPosition.TopRight)));
		gPos.Controls.Add(Btn("BottomLeft", 252, 28, 110, 32, (_, _) => ShowPositioned(ToastPosition.BottomLeft)));
		gPos.Controls.Add(Btn("BottomRight", 372, 28, 110, 32, (_, _) => ShowPositioned(ToastPosition.BottomRight)));
		gPos.Controls.Add(Lbl("Stack count", 12, 76));
		_numStack = Num(100, 72, 60, 1, 8, 3);
		gPos.Controls.Add(_numStack);
		gPos.Controls.Add(Btn("Show N stacked (BottomRight)", 176, 70, 306, 28, BtnShowStack_Click, BtnKind.Primary));
		gPos.Controls.Add(Hint("Each corner is its own stack. Per-corner cap is MaxToastsPerPosition.", 12, 110, 470));
		host.Controls.Add(gPos);

		var gOpt = Box("ToastManagerOptions  (recreate manager to apply)", 520, 8, 516, 360);
		gOpt.Controls.Add(Lbl("Overflow", 12, 28));
		_cbOverflow = Combo(90, 24, 160);
		FillEnum(_cbOverflow, ToastOverflowPolicy.DropNewest);
		gOpt.Controls.Add(_cbOverflow);
		gOpt.Controls.Add(Lbl("MaxToasts", 266, 28));
		_numMaxToasts = Num(350, 24, 60, 1, 20, 6);
		gOpt.Controls.Add(_numMaxToasts);
		gOpt.Controls.Add(Lbl("Per corner", 12, 62));
		_numMaxPerPos = Num(90, 58, 60, 1, 10, 3);
		gOpt.Controls.Add(_numMaxPerPos);
		_chkPauseHover = Chk("PauseOnHover", 170, 62, on: true);
		_chkPlaySound = Chk("PlaySound", 300, 62, on: true);
		_chkHideImage = Chk("HideImagePanelWhenEmpty", 12, 94, on: true);
		gOpt.Controls.Add(_chkPauseHover);
		gOpt.Controls.Add(_chkPlaySound);
		gOpt.Controls.Add(_chkHideImage);
		gOpt.Controls.Add(Lbl("ShortDurationMs", 12, 128));
		_numShortMs = Num(130, 124, 70, 1, 60_000, 2000);
		gOpt.Controls.Add(_numShortMs);
		gOpt.Controls.Add(Lbl("LongDurationMs", 220, 128));
		_numLongMs = Num(330, 124, 70, 1, 60_000, 3000);
		gOpt.Controls.Add(_numLongMs);
		gOpt.Controls.Add(Lbl("InputDurationMs", 12, 162));
		_numInputMs = Num(130, 158, 90, 0, ToastLimits.MaxDurationMs, 300_000);
		gOpt.Controls.Add(_numInputMs);
		gOpt.Controls.Add(Lbl("W", 12, 196));
		_numToastW = Num(32, 192, 60, 200, 800, 380);
		gOpt.Controls.Add(_numToastW);
		gOpt.Controls.Add(Lbl("H", 100, 196));
		_numToastH = Num(118, 192, 55, 60, 400, 100);
		gOpt.Controls.Add(_numToastH);
		gOpt.Controls.Add(Lbl("margin H/V", 180, 196));
		_numHMargin = Num(260, 192, 50, 0, 80, 12);
		gOpt.Controls.Add(_numHMargin);
		_numVMargin = Num(314, 192, 50, 0, 80, 10);
		gOpt.Controls.Add(_numVMargin);
		gOpt.Controls.Add(Lbl("gap", 370, 196));
		_numStackGap = Num(400, 192, 50, 0, 40, 10);
		gOpt.Controls.Add(_numStackGap);
		gOpt.Controls.Add(Lbl("InputToastHeight", 12, 230));
		_numInputHeight = Num(130, 226, 60, 80, 400, 132);
		gOpt.Controls.Add(_numInputHeight);
		gOpt.Controls.Add(Lbl("InputExtraHeight", 210, 230));
		_numInputExtra = Num(330, 226, 60, 0, 120, 36);
		gOpt.Controls.Add(_numInputExtra);
		gOpt.Controls.Add(Btn("Apply — recreate ToastManager", 12, 264, 250, 28, BtnApplyManager_Click, BtnKind.Primary));
		gOpt.Controls.Add(Btn("DismissAll()", 272, 264, 220, 28, BtnDismissAll_Click));
		_lblActive = new Label { AutoSize = false, Location = new Point(12, 300), Size = new Size(480, 48), Font = HintFont, ForeColor = UiTheme.Muted, BackColor = UiTheme.Card };
		gOpt.Controls.Add(_lblActive);
		host.Controls.Add(gOpt);

		var gFill = Box("Overflow drill", 8, 166, 500, 122);
		gFill.Controls.Add(Btn("Fill to cap (DropNewest → ToastRejected)", 12, 28, 470, 28, BtnFillNewest_Click));
		gFill.Controls.Add(Hint("DropOldest dismisses the victim then shows the new toast. Throw raises InvalidOperationException.", 12, 66, 470, 36));
		host.Controls.Add(gFill);
		FinishScrollPage(host);
		return host;
	}

	private Panel BuildInputTab()
	{
		var host = ScrollHost();

		var g = Box("EnableInput / SetInputable / Duration.Input", 8, 8, 1028, 320);
		g.Controls.Add(Lbl("Caption", 12, 24));
		_txtInCaption = Txt(12, 42, 320, "Quick reply");
		g.Controls.Add(_txtInCaption);
		g.Controls.Add(Lbl("Placeholder", 348, 24));
		_txtInPlaceholder = Txt(348, 42, 320, "Your message…");
		g.Controls.Add(_txtInPlaceholder);
		g.Controls.Add(Lbl("Default text", 684, 24));
		_txtInDefault = Txt(684, 42, 320, "");
		_txtInDefault.PlaceholderText = "InputDefaultText";
		g.Controls.Add(_txtInDefault);
		g.Controls.Add(Lbl("Submit button  (≤ ToastLimits.MaxSubmitButtonTextLength)", 12, 76));
		_txtInSubmit = Txt(12, 94, 160, "Send");
		_txtInSubmit.MaxLength = ToastLimits.MaxSubmitButtonTextLength;
		g.Controls.Add(_txtInSubmit);
		_chkAllowEmpty = Chk("AllowEmptySubmit", 190, 98);
		_chkStayOpen = Chk("DurationMs = 0  (stay open)", 360, 98, on: true);
		_chkThenDisableInput = Chk("then SetInputable(false)", 600, 98);
		g.Controls.Add(_chkAllowEmpty);
		g.Controls.Add(_chkStayOpen);
		g.Controls.Add(_chkThenDisableInput);
		g.Controls.Add(Lbl("Timeout ms  (used when stay-open is off)", 12, 134));
		_numInputTimeout = Num(12, 154, 100, 0, ToastLimits.MaxDurationMs, 15_000);
		g.Controls.Add(_numInputTimeout);
		g.Controls.Add(Btn("EnableInput(…).Show()", 130, 152, 240, 28, BtnShowInputable_Click, BtnKind.Primary));
		g.Controls.Add(Btn("Duration.Input only (no text box)", 382, 152, 280, 28, BtnShowDurationInput_Click));
		g.Controls.Add(Btn("SetInputable(true) without EnableInput", 674, 152, 330, 28, BtnSetInputableOnly_Click));
		g.Controls.Add(Hint(
			"EnableInput defaults DurationMs = 0 (stays until Send / Esc / ✕). OnSubmit fires before dismiss. Inputable toasts always keep a close button.",
			12, 198, 990));
		g.Controls.Add(Hint(
			"SetInputable toggles the text box without resetting placeholder / default / submit label. Duration.Input uses manager InputDurationMs unless DurationMs is set.",
			12, 230, 990, 48));
		host.Controls.Add(g);
		FinishScrollPage(host);
		return host;
	}

	private Panel BuildLifecycleTab()
	{
		var host = ScrollHost();

		var gHandle = Box("Toast + ToastHandle  (last shown toast)", 8, 8, 500, 220);
		gHandle.Controls.Add(Btn("Show sticky (8s) and keep handle", 12, 28, 470, 28, BtnShowSticky_Click, BtnKind.Primary));
		gHandle.Controls.Add(Btn("Toast.Dismiss()", 12, 64, 150, 28, (_, _) => DismissLastToast("Toast.Dismiss()")));
		gHandle.Controls.Add(Btn("Toast.Cancel()", 170, 64, 150, 28, (_, _) => CancelLastToast()));
		gHandle.Controls.Add(Btn("Handle.Dismiss()", 328, 64, 154, 28, (_, _) => DismissLastHandle()));
		gHandle.Controls.Add(Btn("Handle.Cancel()  [obsolete]", 12, 100, 200, 28, (_, _) => CancelLastHandle()));
		gHandle.Controls.Add(Btn("Handle.Dispose()", 220, 100, 118, 28, (_, _) => DisposeLastHandle()));
		gHandle.Controls.Add(Btn("await WhenDismissed", 346, 100, 136, 28, BtnAwaitDismissed_Click));
		gHandle.Controls.Add(Btn("Dump handle / ActiveToasts / Count", 12, 136, 470, 28, BtnDumpState_Click));
		gHandle.Controls.Add(Hint("ShowAsync completes when shown or rejected — not when dismissed. Use WhenDismissed for that.", 12, 172, 470));
		host.Controls.Add(gHandle);

		var gAsync = Box("ShowAsync + CancellationToken", 520, 8, 516, 140);
		gAsync.Controls.Add(Btn("ShowAsync(token)  15s toast", 12, 28, 250, 28, BtnShowAsyncCancellable_Click));
		gAsync.Controls.Add(Btn("Cancel token", 274, 28, 220, 28, BtnCancelToken_Click));
		gAsync.Controls.Add(Hint("Cancelling the token after show dismisses the toast. Cancelling before show rejects the attempt.", 12, 68, 488));
		host.Controls.Add(gAsync);

		var gMgr = Box("ToastManager.Create / Show(ToastOptions)", 520, 156, 516, 180);
		gMgr.Controls.Add(Btn("manager.Create().Show()", 12, 28, 240, 28, BtnManagerCreate_Click, BtnKind.Primary));
		gMgr.Controls.Add(Btn("Create().Build() → Show(options)", 260, 28, 236, 28, BtnManagerShowOptions_Click));
		gMgr.Controls.Add(Btn("manager.ShowAsync(options)", 12, 64, 240, 28, BtnManagerShowAsync_Click));
		gMgr.Controls.Add(Btn("Open second owner window", 260, 64, 236, 28, BtnOpenSecondOwner_Click));
		gMgr.Controls.Add(Hint("Create() is the fluent path on an existing manager. A second Form gets its own ToastManager via Toast.Build.", 12, 104, 488));
		host.Controls.Add(gMgr);

		var gHover = Box("OnHover / handle.Hovered / handle.Clicked", 8, 236, 500, 100);
		gHover.Controls.Add(Btn("Show toast that logs hover + click", 12, 28, 470, 28, BtnHoverToast_Click));
		gHover.Controls.Add(Hint("Pointer enter raises OnHover / Hovered. Body click raises OnClick / Clicked.", 12, 64, 470));
		host.Controls.Add(gHover);
		FinishScrollPage(host);
		return host;
	}

	private Panel BuildUtilitiesTab()
	{
		var host = ScrollHost();

		var gLim = Box("ToastLimits + ToastOptions.Validate + FreezeMetadata", 8, 8, 500, 180);
		gLim.Controls.Add(Btn("Dump ToastLimits", 12, 28, 230, 28, BtnDumpLimits_Click));
		gLim.Controls.Add(Btn("Validate() failures", 250, 28, 230, 28, BtnValidateOptions_Click));
		gLim.Controls.Add(Btn("FreezeMetadata (skip blank / oversize)", 12, 64, 468, 28, BtnFreezeMetadata_Click));
		gLim.Controls.Add(Hint("Validate is called automatically by ToastManager.Show. These buttons surface the same exceptions.", 12, 104, 470));
		host.Controls.Add(gLim);

		var gImg = Box("ImageValidation", 520, 8, 516, 120);
		gImg.Controls.Add(Btn("ValidateImagePath + IsPng / IsJpeg", 12, 28, 480, 28, BtnProbeImage_Click));
		gImg.Controls.Add(Hint("Reads 8 magic bytes only — does not decode pixels.", 12, 68, 480));
		host.Controls.Add(gImg);

		var gLay = Box("Layout  (ToastLayoutEngine, CapacityPolicy, ScreenWorkingArea)", 8, 196, 500, 160);
		gLay.Controls.Add(Btn("ComputeStack — 4 corners × 3", 12, 28, 468, 28, BtnComputeLayout_Click));
		gLay.Controls.Add(Btn("CapacityPolicy.Evaluate(current stack)", 12, 64, 468, 28, BtnEvaluateCapacity_Click));
		gLay.Controls.Add(Hint("Pure functions — no HWND required. Metrics from ToastLayoutMetrics.Default.", 12, 104, 470));
		host.Controls.Add(gLay);

		var gTimer = Box("AutoDismissTimerState  (FuzzyToast.Internal)", 520, 136, 516, 220);
		_lblTimer = new Label
		{
			AutoSize = false,
			Location = new Point(12, 24),
			Size = new Size(480, 40),
			Font = HintFont,
			ForeColor = UiTheme.Muted,
			BackColor = UiTheme.Card,
			Text = "No timer. Start a 5s countdown to see Pause / Resume (remaining is not reset)."
		};
		gTimer.Controls.Add(_lblTimer);
		gTimer.Controls.Add(Btn("Start 5s", 12, 72, 110, 28, BtnTimerStart_Click, BtnKind.Primary));
		gTimer.Controls.Add(Btn("Pause", 132, 72, 110, 28, BtnTimerPause_Click));
		gTimer.Controls.Add(Btn("Resume", 252, 72, 110, 28, BtnTimerResume_Click));
		gTimer.Controls.Add(Btn("OnTimerElapsed", 372, 72, 120, 28, BtnTimerElapsed_Click));
		gTimer.Controls.Add(Hint("This is the same countdown the toast UI uses. Hover pause subtracts elapsed; resume keeps the remainder.", 12, 112, 480, 36));
		host.Controls.Add(gTimer);
		FinishScrollPage(host);
		return host;
	}

	// --- Shared apply / manager ---

	private ToastTheme SelectedTheme =>
		_cbTheme.SelectedItem is ToastTheme t ? t : ToastTheme.Dark;

	private CloseStyle SelectedCloseStyle =>
		_cbClose.SelectedItem is CloseStyle s ? s : CloseStyle.ButtonAndClickEntire;

	private ToastPosition SelectedPosition =>
		_cbPosition.SelectedItem is ToastPosition p ? p : ToastPosition.BottomRight;

	private Duration SelectedDuration =>
		_rInput.Checked ? Duration.Input : _rLong.Checked ? Duration.Long : Duration.Short;

	private Animation SelectedAnimation =>
		_rSlide.Checked ? Animation.Slide : Animation.Fade;

	private void RecreateManager(bool announce)
	{
		if (_toasts is not null)
		{
			_toasts.ToastAdded -= OnToastAdded;
			_toasts.ToastRemoved -= OnToastRemoved;
			_toasts.CollectionCleared -= OnCollectionCleared;
			_toasts.ToastRejected -= OnToastRejected;
			try { _toasts.Dispose(); } catch { /* ignore */ }
		}

		_toasts = new ToastManager(this, new ToastManagerOptions
		{
			MaxToasts = (int)_numMaxToasts.Value,
			MaxToastsPerPosition = (int)_numMaxPerPos.Value,
			OverflowPolicy = _cbOverflow.SelectedItem is ToastOverflowPolicy p ? p : ToastOverflowPolicy.DropNewest,
			PauseOnHover = _chkPauseHover.Checked,
			PlaySound = _chkPlaySound.Checked,
			HideImagePanelWhenEmpty = _chkHideImage.Checked,
			ShortDurationMs = (int)_numShortMs.Value,
			LongDurationMs = (int)_numLongMs.Value,
			InputDurationMs = (int)_numInputMs.Value,
			ToastWidth = (int)_numToastW.Value,
			ToastHeight = (int)_numToastH.Value,
			HorizontalMargin = (int)_numHMargin.Value,
			VerticalMargin = (int)_numVMargin.Value,
			StackGap = (int)_numStackGap.Value,
			InputToastHeight = (int)_numInputHeight.Value,
			InputExtraHeight = (int)_numInputExtra.Value
		});
		_toasts.ToastAdded += OnToastAdded;
		_toasts.ToastRemoved += OnToastRemoved;
		_toasts.CollectionCleared += OnCollectionCleared;
		_toasts.ToastRejected += OnToastRejected;

		if (announce)
		{
			Log($"Manager recreated · Max={_toasts.Options.MaxToasts} " +
			    $"perCorner={_toasts.Options.MaxToastsPerPosition} " +
			    $"overflow={_toasts.Options.OverflowPolicy} " +
			    $"sound={_toasts.Options.PlaySound} pauseHover={_toasts.Options.PauseOnHover}");
		}

		UpdateStatus();
	}

	private Toast ApplyCommon(Toast toast)
	{
		if (SelectedTheme == ToastTheme.Custom)
			toast.SetCustomColors(_customBg, _customFg);
		else
			toast.SetTheme(SelectedTheme);

		toast.SetCloseStyle(SelectedCloseStyle)
			.SetPosition(SelectedPosition)
			.SetMuting(_chkMute.Checked)
			.SetShowProgressBar(_chkShowProgressBar.Checked);
		return toast;
	}

	private ToastBuilder ApplyCommon(ToastBuilder builder)
	{
		if (SelectedTheme == ToastTheme.Custom)
			builder.SetCustomColors(new ColorScheme(_customBg, _customFg));
		else
			builder.SetTheme(SelectedTheme);

		return builder
			.SetCloseStyle(SelectedCloseStyle)
			.SetPosition(SelectedPosition)
			.SetMuting(_chkMute.Checked)
			.SetShowProgressBar(_chkShowProgressBar.Checked);
	}

	private void Remember(Toast? toast, ToastHandle? handle)
	{
		_lastToast = toast;
		_lastHandle = handle ?? toast?.Handle;
		if (_lastHandle is not null)
			Log($"  handle {ShortId(_lastHandle.Id)} state={_lastHandle.State} visible={_lastHandle.IsVisible}");
		UpdateStatus();
	}

	private void WireInteractions(Toast toast)
	{
		toast.OnClick += OnToastClicked;
		toast.OnHover += OnToastHovered;
		toast.OnClosed += (_, _) => Log($"OnClosed  guid={ShortId(toast.Guid)}");
		toast.OnSubmit += OnToastSubmitted;
	}

	// --- Basics handlers ---

	private void BtnShow_Click(object? sender, EventArgs e)
	{
		Run("Show()", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, CaptionOrDefault(), DescriptionOrDefault()))
				.SetData(new DemoPayload(1001, "welcome"))
				.SetExtData("action", "open-home")
				.SetMetadata("feature", "simple-demo")
				.SetMetadata(new Dictionary<string, object?> { ["via"] = "SetMetadata(IEnumerable)" })
				.SetExtData(new Dictionary<string, object?> { ["batch"] = 1 });
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private async void BtnShowAsync_Click(object? sender, EventArgs e)
	{
		try
		{
			var toast = ApplyCommon(Toast.Build(this, CaptionOrDefault(), DescriptionOrDefault()))
				.SetMetadata("feature", "show-async");
			WireInteractions(toast);
			await toast.ShowAsync();
			Remember(toast, toast.Handle);
			Log($"ShowAsync completed · Guid={ShortId(toast.Guid)}");
		}
		catch (Exception ex)
		{
			Fail(ex);
		}
	}

	private void BtnShowMutingOverload_Click(object? sender, EventArgs e)
	{
		Run("Build(caption, muting)", () =>
		{
			var toast = Toast.Build(this, CaptionOrDefault(), _chkMute.Checked)
				.SetDescription("Build(window, caption, bool muting)")
				.SetCloseStyle(SelectedCloseStyle)
				.SetPosition(SelectedPosition);
			if (SelectedTheme == ToastTheme.Custom)
				toast.SetCustomColors(_customBg, _customFg);
			else
				toast.SetTheme(SelectedTheme);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnShowDuration_Click(object? sender, EventArgs e)
	{
		Run("Build(caption, Duration)", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, CaptionOrDefault(), SelectedDuration))
				.SetDescription($"Duration={SelectedDuration}" + (_chkUseMs.Checked ? $" · DurationMs={_numDurationMs.Value}" : ""));
			if (_chkUseMs.Checked)
				toast.SetDurationMs((int)_numDurationMs.Value);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnShowAnimation_Click(object? sender, EventArgs e)
	{
		Run("Build(caption, Animation)", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, CaptionOrDefault(), SelectedAnimation))
				.SetDescription($"Animation={SelectedAnimation}");
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnShowDurationAndAnimation_Click(object? sender, EventArgs e)
	{
		Run("Build(caption, Duration, Animation)", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, CaptionOrDefault(), SelectedDuration, SelectedAnimation))
				.SetDescription($"{SelectedDuration} + {SelectedAnimation}");
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnChooseImage_Click(object? sender, EventArgs e)
	{
		using var dlg = new OpenFileDialog
		{
			Title = "Choose toast thumbnail",
			Filter = "Image files|*.jpg;*.jpeg;*.png|JPEG|*.jpg;*.jpeg|PNG|*.png",
			CheckFileExists = true,
			Multiselect = false,
			RestoreDirectory = true,
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
		};
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		try
		{
			if (!ImageValidation.ValidateImagePath(dlg.FileName))
			{
				MessageBox.Show(this, "File is not a supported JPEG or PNG image.", "Invalid file",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using var fs = File.OpenRead(dlg.FileName);
			using var temp = Image.FromStream(fs);
			var loaded = new Bitmap(temp);
			if (!ImageValidation.ValidateImageSize(loaded, 64, 64))
			{
				loaded.Dispose();
				MessageBox.Show(this,
					"Image must be at least 64×64 pixels (recommended 80×80 square).",
					"Invalid size", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			DisposeThumbnail();
			_picThumb.Image = loaded;
			Log($"Thumbnail loaded: {Path.GetFileName(dlg.FileName)} ({loaded.Width}×{loaded.Height})");
		}
		catch (Exception ex)
		{
			Fail(ex);
		}
	}

	private void BtnShowThumbnail_Click(object? sender, EventArgs e)
	{
		if (!TryCloneThumbnail(out var thumb))
			return;
		Run("Build(caption, Image)", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, ThumbCaption(), thumb))
				.SetDescription("With thumbnail")
				.SetThumbnail(thumb, ownsImage: true)
				.SetTag(ThumbCaption())
				.SetMetadata("hasThumbnail", true);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnShowThumbnailFull_Click(object? sender, EventArgs e)
	{
		if (!TryCloneThumbnail(out var thumb))
			return;
		Run("Build(caption, Image, Duration, Animation, mute)", () =>
		{
			var toast = Toast.Build(this, ThumbCaption(), thumb, SelectedDuration, SelectedAnimation, _chkMute.Checked)
				.SetDescription("Full thumbnail overload")
				.SetThumbnail(thumb, ownsImage: true)
				.SetCloseStyle(SelectedCloseStyle)
				.SetPosition(SelectedPosition);
			if (SelectedTheme == ToastTheme.Custom)
				toast.SetCustomColors(_customBg, _customFg);
			else
				toast.SetTheme(SelectedTheme);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	// --- Appearance ---

	private void BtnBuildWithTheme_Click(object? sender, EventArgs e)
	{
		Run("Build(caption, ToastTheme)", () =>
		{
			if (SelectedTheme == ToastTheme.Custom)
			{
				var toast = Toast.Build(this, "Build + Custom", SelectedTheme)
					.SetCustomColors(_customBg, _customFg)
					.SetDescription("Build(window, caption, ToastTheme.Custom) + SetCustomColors")
					.SetCloseStyle(SelectedCloseStyle)
					.SetPosition(SelectedPosition)
					.SetMuting(_chkMute.Checked);
				WireInteractions(toast);
				toast.Show();
				Remember(toast, toast.Handle);
				return;
			}

			var themed = Toast.Build(this, $"Theme {SelectedTheme}", SelectedTheme)
				.SetDescription("Build(window, caption, ToastTheme)")
				.SetCloseStyle(SelectedCloseStyle)
				.SetPosition(SelectedPosition)
				.SetMuting(_chkMute.Checked);
			WireInteractions(themed);
			themed.Show();
			Remember(themed, themed.Handle);
		});
	}

	private void BtnShowCustomRgb_Click(object? sender, EventArgs e)
	{
		Run("SetCustomColors(bg, fg)", () =>
		{
			var toast = Toast.Build(this, "Custom RGB", "Toast.SetCustomColors(Color, Color)")
				.SetCustomColors(_customBg, _customFg)
				.SetCloseStyle(SelectedCloseStyle)
				.SetPosition(SelectedPosition)
				.SetMuting(_chkMute.Checked);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnShowCustomScheme_Click(object? sender, EventArgs e)
	{
		Run("SetCustomColors(ColorScheme)", () =>
		{
			var scheme = new ColorScheme(_customBg, _customFg);
			var handle = ApplyCommon(_toasts.Create()
					.SetCaption("Custom ColorScheme")
					.SetDescription("ToastBuilder.SetCustomColors(scheme)")
					.SetCustomColors(scheme))
				.Show();
			Remember(null, handle);
		});
	}

	private void BtnShowRgbCtor_Click(object? sender, EventArgs e)
	{
		Run("ColorScheme(byte…)", () =>
		{
			var scheme = new ColorScheme(
				_customBg.R, _customBg.G, _customBg.B,
				_customFg.R, _customFg.G, _customFg.B);
			Log($"ColorScheme RGB ctor  bg={scheme.Background} fg={scheme.Foreground} equals-pair={scheme.Equals(new ColorScheme(_customBg, _customFg))}");
			var handle = _toasts.Create()
				.SetCaption("RGB byte ctor")
				.SetDescription("new ColorScheme(rBg,gBg,bBg,rFg,gFg,bFg)")
				.SetCustomColors(scheme)
				.SetCloseStyle(SelectedCloseStyle)
				.SetPosition(SelectedPosition)
				.SetMuting(_chkMute.Checked)
				.Show();
			Remember(null, handle);
		});
	}

	private void BtnResolveAllThemes_Click(object? sender, EventArgs e)
	{
		var custom = new ColorScheme(_customBg, _customFg);
		foreach (var theme in Enum.GetValues<ToastTheme>())
		{
			try
			{
				var scheme = ThemeCatalog.Resolve(theme, theme == ToastTheme.Custom ? custom : null);
				Log($"ThemeCatalog.Resolve({theme})  bg={ToRgb(scheme.Background)} fg={ToRgb(scheme.Foreground)}");
			}
			catch (Exception ex)
			{
				Log($"ThemeCatalog.Resolve({theme})  {ex.GetType().Name}: {ex.Message}");
			}
		}
	}

	private void ShowCloseStyle(CloseStyle style)
	{
		Run($"CloseStyle.{style}", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, $"CloseStyle.{style}", "Try the body click vs the ✕ button"))
				.SetCloseStyle(style);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void ShowSound(bool muted)
	{
		Run(muted ? "muted" : "unmuted", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, muted ? "Muted" : "Unmuted", $"SetMuting({muted.ToString().ToLowerInvariant()}) · PlaySound={_toasts.Options.PlaySound}"))
				.SetMuting(muted);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	// --- Stack ---

	private void ShowPositioned(ToastPosition position)
	{
		Run(position.ToString(), () =>
		{
			var toast = ApplyCommon(Toast.Build(this, $"{position} toast", $"Position: {position}"))
				.SetPosition(position);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnShowStack_Click(object? sender, EventArgs e)
	{
		Run("stack", () =>
		{
			var count = (int)_numStack.Value;
			ToastHandle? last = null;
			for (var i = 1; i <= count; i++)
			{
				var toast = ApplyCommon(Toast.Build(this, $"Toast {i}", $"Stack item {i} of {count}"))
					.SetPosition(ToastPosition.BottomRight);
				WireInteractions(toast);
				toast.Show();
				last = toast.Handle;
			}
			Remember(null, last);
		});
	}

	private void BtnApplyManager_Click(object? sender, EventArgs e) => RecreateManager(announce: true);

	private void BtnDismissAll_Click(object? sender, EventArgs e)
	{
		_toasts.DismissAll();
		Log($"DismissAll() · remaining visible={_toasts.Count}");
		UpdateStatus();
	}

	private void BtnFillNewest_Click(object? sender, EventArgs e)
	{
		Run("fill to cap", () =>
		{
			var n = _toasts.Options.MaxToasts + 2;
			Log($"Showing {n} toasts (cap={_toasts.Options.MaxToasts}, policy={_toasts.Options.OverflowPolicy})");
			for (var i = 1; i <= n; i++)
			{
				try
				{
					var toast = ApplyCommon(Toast.Build(this, $"Cap probe {i}", $"#{i} of {n}"))
						.SetPosition(SelectedPosition);
					toast.Show();
					if (toast.Handle?.WasRejected == true)
						Log($"  #{i} rejected (WasRejected) reason via ToastRejected event");
				}
				catch (InvalidOperationException ex)
				{
					Log($"  #{i} Throw: {ex.Message}");
				}
			}
			UpdateStatus();
		});
	}

	// --- Inputable ---

	private void BtnShowInputable_Click(object? sender, EventArgs e)
	{
		Run("EnableInput", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, InputCaption(), "Type below, then Send or press Enter"))
				.EnableInput(
					placeholder: _txtInPlaceholder.Text?.Trim() ?? "Your message…",
					defaultText: _txtInDefault.Text ?? string.Empty,
					submitButtonText: string.IsNullOrWhiteSpace(_txtInSubmit.Text) ? "Send" : _txtInSubmit.Text.Trim(),
					allowEmptySubmit: _chkAllowEmpty.Checked)
				.SetCloseStyle(CloseStyle.Button)
				.SetExtData("action", "quick-input")
				.SetMetadata("demo", "inputable-v3")
				.SetTag(new DemoPayload(3001, "inputable"));

			if (_chkThenDisableInput.Checked)
				toast.SetInputable(false);

			if (!_chkStayOpen.Checked)
				toast.SetDurationMs((int)_numInputTimeout.Value);

			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
			Log(_chkThenDisableInput.Checked
				? "EnableInput then SetInputable(false) — shown as a normal toast."
				: $"Inputable shown · stayOpen={_chkStayOpen.Checked} · allowEmpty={_chkAllowEmpty.Checked}");
		});
	}

	private void BtnShowDurationInput_Click(object? sender, EventArgs e)
	{
		Run("Duration.Input", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, "Duration.Input", Duration.Input))
				.SetDescription($"No text box · waits InputDurationMs={_toasts.Options.InputDurationMs} ms (or SetDurationMs).");
			if (!_chkStayOpen.Checked)
				toast.SetDurationMs((int)_numInputTimeout.Value);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	private void BtnSetInputableOnly_Click(object? sender, EventArgs e)
	{
		Run("SetInputable(true)", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, InputCaption(), "SetInputable(true) without EnableInput — submit defaults to OK"))
				.SetInputable(true)
				.SetCloseStyle(CloseStyle.Button);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
		});
	}

	// --- Lifecycle ---

	private void BtnShowSticky_Click(object? sender, EventArgs e)
	{
		Run("sticky", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, "Sticky handle", "Lasts 8 seconds — use Dismiss / WhenDismissed"))
				.SetDurationMs(8000);
			WireInteractions(toast);
			toast.Show();
			Remember(toast, toast.Handle);
			if (toast.Handle is { } h)
			{
				_ = h.WhenDismissed.ContinueWith(
					_ => BeginInvoke(() => Log($"WhenDismissed (auto)  {ShortId(h.Id)} state={h.State}")),
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
			}
		});
	}

	private void DismissLastToast(string via)
	{
		if (_lastToast is null)
		{
			Log("No last Toast instance — show a toast from this form first.");
			return;
		}
		_lastToast.Dismiss();
		Log($"{via} called · handle state={_lastToast.Handle?.State}");
		UpdateStatus();
	}

	private void CancelLastToast()
	{
		if (_lastToast is null)
		{
			Log("No last Toast instance.");
			return;
		}
		_lastToast.Cancel();
		Log($"Toast.Cancel() called · handle state={_lastToast.Handle?.State}");
		UpdateStatus();
	}

	private void DismissLastHandle()
	{
		if (_lastHandle is null)
		{
			Log("No last ToastHandle.");
			return;
		}
		_lastHandle.Dismiss();
		Log($"Handle.Dismiss()  {ShortId(_lastHandle.Id)} state={_lastHandle.State} dismissed={_lastHandle.IsDismissed}");
		UpdateStatus();
	}

	private void CancelLastHandle()
	{
		if (_lastHandle is null)
		{
			Log("No last ToastHandle.");
			return;
		}
#pragma warning disable CS0618
		_lastHandle.Cancel();
#pragma warning restore CS0618
		Log($"Handle.Cancel() [obsolete alias]  {ShortId(_lastHandle.Id)} state={_lastHandle.State}");
		UpdateStatus();
	}

	private void DisposeLastHandle()
	{
		if (_lastHandle is null)
		{
			Log("No last ToastHandle.");
			return;
		}
		var id = _lastHandle.Id;
		_lastHandle.Dispose();
		Log($"Handle.Dispose()  {ShortId(id)} (dismiss + detach, idempotent)");
		UpdateStatus();
	}

	private async void BtnAwaitDismissed_Click(object? sender, EventArgs e)
	{
		if (_lastHandle is null)
		{
			Log("No last handle to await.");
			return;
		}

		var handle = _lastHandle;
		Log($"awaiting WhenDismissed for {ShortId(handle.Id)} (already completed={handle.WhenDismissed.IsCompleted})…");
		await handle.WhenDismissed;
		Log($"WhenDismissed completed  {ShortId(handle.Id)} state={handle.State} rejected={handle.WasRejected}");
		UpdateStatus();
	}

	private async void BtnShowAsyncCancellable_Click(object? sender, EventArgs e)
	{
		try
		{
			_showCts?.Cancel();
			_showCts?.Dispose();
			_showCts = new CancellationTokenSource();
			var toast = ApplyCommon(Toast.Build(this, "Cancellable ShowAsync", "Cancel the token to dismiss"))
				.SetDurationMs(15_000);
			WireInteractions(toast);
			await toast.ShowAsync(_showCts.Token);
			Remember(toast, toast.Handle);
			Log("ShowAsync(token) completed (shown or rejected).");
		}
		catch (OperationCanceledException)
		{
			Log("ShowAsync cancelled.");
		}
		catch (Exception ex)
		{
			Fail(ex);
		}
	}

	private void BtnCancelToken_Click(object? sender, EventArgs e)
	{
		if (_showCts is null)
		{
			Log("No active ShowAsync token. Click ShowAsync(token) first.");
			return;
		}
		_showCts.Cancel();
		Log("CancellationToken cancelled — visible toast is dismissed.");
	}

	private void BtnManagerCreate_Click(object? sender, EventArgs e)
	{
		Run("Create().Show()", () =>
		{
			var handle = ApplyCommon(_toasts.Create()
					.SetCaption("From ToastManager.Create()")
					.SetDescription("Fluent builder bound to this manager")
					.SetDuration(Duration.Long)
					.SetAnimation(SelectedAnimation)
					.SetTag(new DemoPayload(4001, "builder"))
					.SetMetadata("via", "Create"))
				.Show();
			handle.Clicked += (_, ev) => OnToastClicked(handle, ev);
			handle.Hovered += (_, ev) => OnToastHovered(handle, ev);
			handle.Dismissed += (_, _) => Log($"handle.Dismissed  {ShortId(handle.Id)}");
			Remember(null, handle);
		});
	}

	private void BtnManagerShowOptions_Click(object? sender, EventArgs e)
	{
		Run("Show(options)", () =>
		{
			var options = ApplyCommon(_toasts.Create()
					.SetCaption("Build() then Show(options)")
					.SetDescription("ToastOptions snapshot")
					.SetDuration(Duration.Short))
				.Build();
			options.Validate();
			Log($"ToastOptions.Validate() OK  caption=\"{options.Caption}\" theme={options.Theme} ms={options.DurationMs}");
			var handle = _toasts.Show(options);
			Remember(null, handle);
		});
	}

	private async void BtnManagerShowAsync_Click(object? sender, EventArgs e)
	{
		try
		{
			var options = ApplyCommon(_toasts.Create()
					.SetCaption("manager.ShowAsync")
					.SetDescription("Completes when shown or rejected")
					.SetDuration(Duration.Long))
				.Build();
			var handle = await _toasts.ShowAsync(options);
			Remember(null, handle);
			Log($"manager.ShowAsync completed  state={handle.State}");
		}
		catch (Exception ex)
		{
			Fail(ex);
		}
	}

	private void BtnOpenSecondOwner_Click(object? sender, EventArgs e)
	{
		if (_secondOwner is { IsDisposed: false })
		{
			_secondOwner.Activate();
			return;
		}

		_secondOwner = new Form
		{
			Text = "Second owner",
			ClientSize = new Size(380, 150),
			FormBorderStyle = FormBorderStyle.FixedSingle,
			MaximizeBox = false,
			StartPosition = FormStartPosition.CenterScreen,
			Font = UiFont,
			BackColor = UiTheme.Canvas,
			ForeColor = UiTheme.Text
		};
		var hint = new Label
		{
			Location = new Point(16, 16),
			Size = new Size(348, 48),
			Text = "Toast.Build(this) registers a separate ToastManager for this form — an independent stack.",
			ForeColor = UiTheme.Muted,
			BackColor = UiTheme.Canvas
		};
		var owner = _secondOwner;
		var btn = Btn("Toast.Build(this, \"Second owner\").Show()", 16, 76, 348, 32, (_, _) =>
		{
			Toast.Build(owner, "Second owner", "Independent per-owner stack")
				.SetTheme(ToastTheme.PrimaryLight)
				.SetPosition(ToastPosition.TopLeft)
				.SetMuting(_chkMute.Checked)
				.Show();
			Log("Toast shown on second owner (its own manager / stack).");
		}, BtnKind.Primary);
		_secondOwner.Controls.Add(hint);
		_secondOwner.Controls.Add(btn);
		_secondOwner.FormClosed += (_, _) => _secondOwner = null;
		_secondOwner.Show(this);
	}

	private void BtnHoverToast_Click(object? sender, EventArgs e)
	{
		Run("hover", () =>
		{
			var toast = ApplyCommon(Toast.Build(this, "Hover me", "OnHover / handle.Hovered fire on pointer enter"))
				.SetDurationMs(10_000)
				.SetMetadata("feature", "hover");
			WireInteractions(toast);
			toast.Show();
			if (toast.Handle is { } h)
			{
				h.Hovered += (_, ev) => Log($"handle.Hovered  {ShortId(ev.ToastId)}");
				h.Clicked += (_, ev) => Log($"handle.Clicked  {ShortId(ev.ToastId)}");
			}
			Remember(toast, toast.Handle);
		});
	}

	private void BtnDumpState_Click(object? sender, EventArgs e)
	{
		Log($"Manager  Count={_toasts.Count} IsDisposed={_toasts.IsDisposed} Owner={_toasts.Owner.Name} overflow={_toasts.Options.OverflowPolicy}");
		foreach (var h in _toasts.ActiveToasts)
			Log($"  Active  {ShortId(h.Id)} {h.State} {h.Options.Position} \"{h.Options.Caption}\" submitted={h.SubmittedText ?? "—"}");
		if (_lastToast is not null)
			Log($"  last Toast  Guid={ShortId(_lastToast.Guid)} Caption={_lastToast.Caption} Theme={_lastToast.Theme} Muted={_lastToast.IsMuted} Tag={_lastToast.Tag}");
		if (_lastHandle is not null)
			Log($"  last Handle {ShortId(_lastHandle.Id)} State={_lastHandle.State} Visible={_lastHandle.IsVisible} Dismissed={_lastHandle.IsDismissed} Rejected={_lastHandle.WasRejected}");
	}

	// --- Utilities ---

	private void BtnDumpLimits_Click(object? sender, EventArgs e)
	{
		Log($"ToastLimits  Caption={ToastLimits.MaxCaptionLength} Desc={ToastLimits.MaxDescriptionLength} " +
		    $"Input={ToastLimits.MaxInputTextLength} Submit={ToastLimits.MaxSubmitButtonTextLength} " +
		    $"MetaEntries={ToastLimits.MaxMetadataEntries} MetaKey={ToastLimits.MaxMetadataKeyLength}");
		Log($"  MaxDurationMs={ToastLimits.MaxDurationMs} ImageDim={ToastLimits.MinImageDimension}..{ToastLimits.MaxImageDimension} " +
		    $"FileBytes={ToastLimits.MaxImageFileBytes}");
	}

	private void BtnValidateOptions_Click(object? sender, EventArgs e)
	{
		TryValidate("empty caption", new ToastOptions());
		TryValidate("Custom without ColorScheme", new ToastOptions { Caption = "x", Theme = ToastTheme.Custom });
		TryValidate("DurationMs = -1", new ToastOptions { Caption = "x", DurationMs = -1 });
		TryValidate("input without submit", new ToastOptions { Caption = "x", EnableInput = true, SubmitButtonText = "  " });
		TryValidate("valid snapshot", new ToastOptions { Caption = "OK", Description = "fine" });
	}

	private void TryValidate(string label, ToastOptions options)
	{
		try
		{
			options.Validate();
			Log($"Validate({label})  OK");
		}
		catch (Exception ex)
		{
			Log($"Validate({label})  {ex.GetType().Name}: {ex.Message}");
		}
	}

	private void BtnFreezeMetadata_Click(object? sender, EventArgs e)
	{
		var tooLong = new string('k', ToastLimits.MaxMetadataKeyLength + 1);
		var frozen = ToastOptions.FreezeMetadata(new Dictionary<string, object?>
		{
			["keep"] = 1,
			[""] = "skip-blank",
			["  "] = "skip-ws",
			[tooLong] = "skip-oversize",
			["tag"] = "ok"
		});
		Log($"FreezeMetadata  count={frozen.Count} keys=[{string.Join(", ", frozen.Keys)}]");
	}

	private void BtnProbeImage_Click(object? sender, EventArgs e)
	{
		using var dlg = new OpenFileDialog
		{
			Title = "Probe image magic bytes",
			Filter = "All files|*.*",
			CheckFileExists = true
		};
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;

		var pathOk = ImageValidation.ValidateImagePath(dlg.FileName);
		var header = new byte[8];
		var read = 0;
		try
		{
			using var fs = File.OpenRead(dlg.FileName);
			read = fs.Read(header, 0, header.Length);
		}
		catch (Exception ex)
		{
			Log($"Image probe IO: {ex.Message}");
			return;
		}

		var span = header.AsSpan(0, read);
		Log($"ValidateImagePath(\"{Path.GetFileName(dlg.FileName)}\") = {pathOk}");
		Log($"  IsPng(byte[])={ImageValidation.IsPng(header)}  IsJpeg(byte[])={ImageValidation.IsJpeg(header)}");
		Log($"  IsPng(span)={ImageValidation.IsPng(span)}  IsJpeg(span)={ImageValidation.IsJpeg(span)}");
	}

	private void BtnComputeLayout_Click(object? sender, EventArgs e)
	{
		var wa = Screen.FromControl(this).WorkingArea;
		var area = new ScreenWorkingArea(wa.Left, wa.Top, wa.Right, wa.Bottom);
		var metrics = ToastLayoutMetrics.Default;
		Log($"ScreenWorkingArea  {area.Left},{area.Top}-{area.Right},{area.Bottom}  {area.Width}×{area.Height}");
		Log($"LayoutRect hint of this form  ({Left},{Top},{Width},{Height})");
		Log($"ToastLayoutMetrics.Default  {metrics.ToastWidth}×{metrics.ToastHeight} stride={metrics.EffectiveStackStride}");
		foreach (var pos in Enum.GetValues<ToastPosition>())
		{
			var stack = ToastLayoutEngine.ComputeStack(pos, 3, metrics, area);
			Log($"  {pos}  {string.Join(" | ", stack.Select(p => $"{p.X},{p.Y}"))}");
		}
	}

	private void BtnEvaluateCapacity_Click(object? sender, EventArgs e)
	{
		var active = _toasts.ActiveToasts
			.Select(h => (h.Id, h.Options.Position))
			.ToList();
		var decision = CapacityPolicy.Evaluate(
			_toasts.Options.OverflowPolicy,
			_toasts.Options.MaxToasts,
			_toasts.Options.MaxToastsPerPosition,
			SelectedPosition,
			active);
		Log($"CapacityPolicy.Evaluate  incoming={SelectedPosition} active={active.Count} " +
		    $"action={decision.Action} by={decision.TriggeredBy} victim={decision.VictimId ?? "—"} reason={decision.Reason}");
	}

	private void BtnTimerStart_Click(object? sender, EventArgs e)
	{
		_timerState = new AutoDismissTimerState(5000);
		_timerState.StartOrResume();
		_timerArmedTick = Environment.TickCount;
		EnsureTimerUi();
		Log("AutoDismissTimerState started (5000 ms).");
		RefreshTimerLabel();
	}

	private void BtnTimerPause_Click(object? sender, EventArgs e)
	{
		if (_timerState is null)
		{
			Log("Start the demo timer first.");
			return;
		}
		var elapsed = Environment.TickCount - _timerArmedTick;
		_timerState.Pause(elapsed);
		Log($"Pause({elapsed} ms)  remaining={_timerState.RemainingMs} paused={_timerState.IsPaused}");
		RefreshTimerLabel();
	}

	private void BtnTimerResume_Click(object? sender, EventArgs e)
	{
		if (_timerState is null)
			return;
		var interval = _timerState.Resume();
		_timerArmedTick = Environment.TickCount;
		Log($"Resume() interval={interval} remaining={_timerState.RemainingMs}");
		RefreshTimerLabel();
	}

	private void BtnTimerElapsed_Click(object? sender, EventArgs e)
	{
		if (_timerState is null)
			return;
		_timerState.OnTimerElapsed();
		Log($"OnTimerElapsed  expired={_timerState.IsExpired} remaining={_timerState.RemainingMs}");
		RefreshTimerLabel();
	}

	private void EnsureTimerUi()
	{
		if (_timerUi is not null)
			return;
		_timerUi = new System.Windows.Forms.Timer { Interval = 100 };
		_timerUi.Tick += (_, _) => RefreshTimerLabel();
		_timerUi.Start();
	}

	private void RefreshTimerLabel()
	{
		if (_timerState is null || _lblTimer.IsDisposed)
			return;
		var shown = _timerState.RemainingMs;
		if (!_timerState.IsPaused && !_timerState.IsExpired)
			shown = Math.Max(0, _timerState.RemainingMs - Math.Max(0, Environment.TickCount - _timerArmedTick));
		_lblTimer.Text =
			$"Total={_timerState.TotalDurationMs} ms · remaining≈{shown} · paused={_timerState.IsPaused} · expired={_timerState.IsExpired}";
	}

	// --- Events ---

	private void OnToastAdded(object? sender, ToastChangedEventArgs e) =>
		Log($"[+ ] {ShortId(e.Toast.Id)} shown · {e.Toast.Options.Position} · {e.Toast.Options.Theme} · \"{e.Toast.Options.Caption}\"");

	private void OnToastRemoved(object? sender, ToastChangedEventArgs e) =>
		Log($"[- ] {ShortId(e.Toast.Id)} dismissed  submitted={e.Toast.SubmittedText ?? "—"}");

	private void OnCollectionCleared(object? sender, EventArgs e) =>
		Log("[   ] collection empty");

	private void OnToastRejected(object? sender, ToastRejectedEventArgs e) =>
		Log($"[ ! ] rejected ({e.Reason}): {e.Options.Caption}  WasRejected={e.Toast.WasRejected}");

	private void OnToastClicked(object? sender, ToastInteractionEventArgs e)
	{
		var tagText = e.Tag switch
		{
			DemoPayload p => $"DemoPayload(Id={p.Id}, Kind={p.Kind})",
			null => "(null)",
			_ => e.Tag.ToString() ?? ""
		};
		var meta = string.Join(", ", e.Metadata.Select(kv => $"{kv.Key}={kv.Value}"));
		Log($"CLICK id={ShortId(e.ToastId)} tag={tagText} data={e.Data} meta=[{meta}] indexer.action={e["action"]}");
		if (e.TryGetMetadata<string>("action", out var action))
			Log($"  → TryGetMetadata<string>(\"action\") = {action}");
		Log($"  GetMetadata<string>(\"feature\") = {e.GetMetadata<string>("feature")}");
	}

	private void OnToastHovered(object? sender, ToastInteractionEventArgs e) =>
		Log($"HOVER id={ShortId(e.ToastId)} pos={e.Options.Position}");

	private void OnToastSubmitted(object? sender, ToastSubmittedEventArgs e)
	{
		Log($"SUBMIT id={ShortId(e.ToastId)} text=\"{e.InputText}\" empty={e.IsEmpty}");
		Log($"  tag={e.Tag} action={e.GetMetadata<string>("action")} demo={e.GetMetadata<string>("demo")}");
	}

	private void About_Click(object? sender, EventArgs e)
	{
		MessageBox.Show(this,
			"FuzzyToast Demo v3 — full public API catalog\n\n" +
			"Windows Forms toast library for Windows 10/11 · .NET 8+\n\n" +
			"Tabs:\n" +
			"  Basics — Toast.Build overloads, Show / ShowAsync, duration, animation, thumbnail\n" +
			"  Appearance — themes, ColorScheme, ThemeCatalog, CloseStyle, mute\n" +
			"  Stack & manager — 4 corners, overflow, ToastManagerOptions, DismissAll\n" +
			"  Inputable — EnableInput, SetInputable, Duration.Input\n" +
			"  Lifecycle — handle dismiss, WhenDismissed, cancel token, Create(), second owner\n" +
			"  Utilities — ToastLimits, Validate, ImageValidation, layout, CapacityPolicy, timer\n\n" +
			"MIT License",
			"About FuzzyToast Demo",
			MessageBoxButtons.OK,
			MessageBoxIcon.Information);
	}

	// --- helpers ---

	private string CaptionOrDefault() =>
		string.IsNullOrWhiteSpace(_txtCaption.Text) ? "Hello, I am Toast!" : _txtCaption.Text.Trim();

	private string DescriptionOrDefault() =>
		_txtDescription.Text?.Trim() ?? string.Empty;

	private string ThumbCaption() =>
		string.IsNullOrWhiteSpace(_txtThumbCaption.Text) ? "Hello! I'm Toast :)" : _txtThumbCaption.Text.Trim();

	private string InputCaption() =>
		string.IsNullOrWhiteSpace(_txtInCaption.Text) ? "Quick reply" : _txtInCaption.Text.Trim();

	private bool TryCloneThumbnail(out Image thumb)
	{
		if (_picThumb.Image is null)
		{
			MessageBox.Show(this, "Please choose an image first.", "Image required",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			thumb = null!;
			return false;
		}
		thumb = (Image)_picThumb.Image.Clone();
		return true;
	}

	private void DisposeThumbnail()
	{
		var img = _picThumb.Image;
		_picThumb.Image = null;
		img?.Dispose();
	}

	private void PickColor(ref Color current, Button button)
	{
		using var dlg = new ColorDialog { Color = current, FullOpen = true };
		if (dlg.ShowDialog(this) != DialogResult.OK)
			return;
		current = dlg.Color;
		PaintColorButton(button, current);
		RefreshThemePreview();
	}

	private static void PaintColorButton(Button button, Color color)
	{
		button.Tag = "swatch";
		button.BackColor = color;
		button.ForeColor = color.GetBrightness() < 0.5f ? Color.White : Color.Black;
		button.FlatAppearance.BorderColor = UiTheme.Border;
		button.FlatAppearance.MouseOverBackColor = color;
		button.FlatAppearance.MouseDownBackColor = color;
	}

	private void RefreshThemePreview()
	{
		if (_pnlPreview is null)
			return;
		try
		{
			var custom = new ColorScheme(_customBg, _customFg);
			var scheme = ThemeCatalog.Resolve(SelectedTheme, SelectedTheme == ToastTheme.Custom ? custom : null);
			_pnlPreview.BackColor = scheme.Background;
			_lblPreview.ForeColor = scheme.Foreground;
			_lblPreview.Text = SelectedTheme.ToString();
		}
		catch (Exception ex)
		{
			_lblPreview.Text = ex.GetType().Name;
		}
	}

	private void Run(string label, Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			Log($"{label} error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void Fail(Exception ex)
	{
		Log($"Error: {ex.Message}");
		MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	private void Log(string message)
	{
		if (IsDisposed || _log is null || _log.IsDisposed)
			return;

		void Append()
		{
			if (_log.IsDisposed)
				return;
			_log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
			_log.SelectionStart = _log.TextLength;
			_log.ScrollToCaret();
		}

		if (_log.InvokeRequired)
			_log.BeginInvoke(Append);
		else
			Append();
	}

	private void UpdateStatus()
	{
		if (_lblStatus is null || _lblStatus.IsDisposed || _toasts is null)
			return;
		var last = _lastHandle is null ? "none" : $"{ShortId(_lastHandle.Id)} {_lastHandle.State}";
		_lblStatus.Text =
			$"Active={_toasts.Count}  last={last}  overflow={_toasts.Options.OverflowPolicy}  " +
			$"PlaySound={_toasts.Options.PlaySound}  PauseOnHover={_toasts.Options.PauseOnHover}  " +
			$"theme={SelectedTheme}  close={SelectedCloseStyle}  pos={SelectedPosition}";
		if (_lblActive is { IsDisposed: false })
			_lblActive.Text = $"ActiveToasts={_toasts.Count}  IsDisposed={_toasts.IsDisposed}  Owner={_toasts.Owner.Name}";
	}

	private static string ShortId(string id) =>
		string.IsNullOrEmpty(id) ? "(none)" : id.Length >= 8 ? id[..8] : id;

	private static string ToRgb(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

	private sealed record DemoPayload(int Id, string Kind);

	// --- tiny control factories ---

	private static Panel ScrollHost() =>
		new()
		{
			Dock = DockStyle.Fill,
			AutoScroll = true,
			BackColor = UiTheme.Canvas
		};

	private static FlatCard Box(string title, int x, int y, int w, int h) =>
		new(title) { Location = new Point(x, y), Size = new Size(w, h) };

	private static Label Lbl(string text, int x, int y) =>
		new()
		{
			AutoSize = true,
			Text = text,
			Location = new Point(x, y),
			Font = HintFont,
			ForeColor = UiTheme.Muted,
			BackColor = Color.Transparent
		};

	private static Label Hint(string text, int x, int y, int w, int h = 32) =>
		new()
		{
			AutoSize = false,
			Text = text,
			Location = new Point(x, y),
			Size = new Size(w, h),
			ForeColor = UiTheme.Muted,
			BackColor = Color.Transparent,
			Font = HintFont,
			AutoEllipsis = true
		};

	/// <summary>
	/// WinForms AutoScroll only sees overflowing children if MinSize is set —
	/// otherwise the last few pixels of a card get clipped with no scrollbar.
	/// </summary>
	private static void FinishScrollPage(Panel host)
	{
		var bottom = 0;
		var right = 0;
		foreach (Control c in host.Controls)
		{
			bottom = Math.Max(bottom, c.Bottom);
			right = Math.Max(right, c.Right);
		}

		host.AutoScroll = true;
		host.AutoScrollMinSize = new Size(right + 8, bottom + 12);
	}

	private static TextBox Txt(int x, int y, int w, string text) =>
		new()
		{
			Location = new Point(x, y),
			Size = new Size(w, 23),
			Text = text,
			MaxLength = 512,
			Font = UiFont,
			BorderStyle = BorderStyle.FixedSingle,
			BackColor = UiTheme.Input,
			ForeColor = UiTheme.Text
		};

	private enum BtnKind { Secondary, Primary, Ghost }

	private static Button Btn(string text, int x, int y, int w, int h, EventHandler onClick, BtnKind kind = BtnKind.Secondary)
	{
		var primary = kind == BtnKind.Primary;
		var ghost = kind == BtnKind.Ghost;
		var b = new Button
		{
			Text = text,
			Location = new Point(x, y),
			Size = new Size(w, h),
			UseVisualStyleBackColor = false,
			Font = primary ? UiTheme.Title : UiFont,
			FlatStyle = FlatStyle.Flat,
			BackColor = primary ? UiTheme.Accent : ghost ? UiTheme.Card : UiTheme.Button,
			ForeColor = primary ? Color.White : UiTheme.Text,
			Cursor = Cursors.Hand,
			UseMnemonic = false
		};
		b.FlatAppearance.BorderSize = 1;
		b.FlatAppearance.BorderColor = primary ? UiTheme.Accent : ghost ? UiTheme.ButtonBorder : UiTheme.ButtonBorder;
		b.FlatAppearance.MouseOverBackColor = primary ? UiTheme.AccentHot : ghost ? UiTheme.AccentSoft : UiTheme.ButtonHot;
		b.FlatAppearance.MouseDownBackColor = primary ? UiTheme.AccentDown : UiTheme.ButtonDown;
		b.MouseEnter += (_, _) =>
		{
			if (b.Tag as string == "swatch")
				return;
			b.FlatAppearance.BorderColor = primary ? UiTheme.AccentHot : UiTheme.Accent;
			if (primary)
				b.BackColor = UiTheme.AccentHot;
		};
		b.MouseLeave += (_, _) =>
		{
			if (b.Tag as string == "swatch")
				return;
			b.FlatAppearance.BorderColor = primary ? UiTheme.Accent : UiTheme.ButtonBorder;
			if (primary)
				b.BackColor = UiTheme.Accent;
		};
		b.Click += onClick;
		return b;
	}

	private static CheckBox Chk(string text, int x, int y, bool on = false) =>
		new()
		{
			AutoSize = true,
			Text = text,
			Location = new Point(x, y),
			Checked = on,
			Font = UiFont,
			FlatStyle = FlatStyle.Flat,
			ForeColor = UiTheme.Text,
			BackColor = Color.Transparent,
			Cursor = Cursors.Hand
		};

	private static RadioButton Radio(string text, int x, int y, bool on) =>
		new()
		{
			AutoSize = true,
			Text = text,
			Location = new Point(x, y),
			Checked = on,
			Font = UiFont,
			FlatStyle = FlatStyle.Flat,
			ForeColor = UiTheme.Text,
			BackColor = Color.Transparent,
			Cursor = Cursors.Hand
		};

	private static ComboBox Combo(int x, int y, int w) =>
		new()
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			Location = new Point(x, y),
			Size = new Size(w, 23),
			Font = UiFont,
			FlatStyle = FlatStyle.Flat,
			BackColor = UiTheme.Input,
			ForeColor = UiTheme.Text
		};

	private static void FillEnum<T>(ComboBox combo, T selected) where T : struct, Enum
	{
		combo.BeginUpdate();
		combo.Items.Clear();
		foreach (var value in Enum.GetValues<T>())
			combo.Items.Add(value);
		combo.EndUpdate();
		combo.SelectedItem = selected;
	}

	private static NumericUpDown Num(int x, int y, int w, decimal min, decimal max, decimal value) =>
		new()
		{
			Location = new Point(x, y),
			Size = new Size(w, 23),
			Minimum = min,
			Maximum = max,
			Value = Math.Min(max, Math.Max(min, value)),
			ThousandsSeparator = true,
			Font = UiFont,
			BorderStyle = BorderStyle.FixedSingle,
			BackColor = UiTheme.Input,
			ForeColor = UiTheme.Text
		};
}
