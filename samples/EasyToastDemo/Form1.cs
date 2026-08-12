using System.Drawing;
using System.Runtime.Versioning;
using FuzzyToast;

namespace EasyToastDemo;

/// <summary>
/// Interactive demo for FuzzyToast on Windows 10/11 (.NET 8+ WinForms).
/// Uses Android-style <c>Toast.Build(...).Show()</c> with a shared per-form manager.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class Form1 : Form
{
	private ToastManager _toasts = null!;
	private bool _busy;

	// Inputable toast (v3) demo controls — created at runtime to avoid fragile designer churn.
	private GroupBox _grpInputable = null!;
	private TextBox _txtInputCaption = null!;
	private TextBox _txtInputPlaceholder = null!;
	private TextBox _txtSubmitLabel = null!;
	private Button _btnShowInputable = null!;
	private CheckBox _chkAllowEmpty = null!;
	private Label _lblInputHint = null!;

	/// <summary>Fixed client size for the absolute-positioned designer layout.</summary>
	private static readonly Size DesignClientSize = new(901, 571);

	public Form1()
	{
		// Disable scaling before controls are created.
		AutoScaleMode = AutoScaleMode.None;
		InitializeComponent();
		AutoScaleMode = AutoScaleMode.None;
		// Clear any min/max left from older builds that could force a huge window.
		MinimumSize = Size.Empty;
		MaximumSize = Size.Empty;
		ClientSize = DesignClientSize;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		MaximizeBox = false;
		StartPosition = FormStartPosition.CenterScreen;
		Text = "FuzzyToast Demo v3";
		BuildInputableDemoSection();
	}

	/// <summary>Adds the v3 Inputable toast panel — spaced rows, no overlap.</summary>
	private void BuildInputableDemoSection()
	{
		// Right column under Close style; tall enough for labeled fields + actions.
		_grpInputable = new GroupBox
		{
			Text = "Inputable toast (v3)",
			Location = new Point(511, 183),
			Size = new Size(376, 218),
			TabIndex = 14
		};

		_lblInputHint = new Label
		{
			AutoSize = false,
			Location = new Point(12, 22),
			Size = new Size(350, 18),
			ForeColor = Color.DimGray,
			Font = new Font("Segoe UI", 8.25F),
			Text = "Text box + Send · stays open until Submit / Esc"
		};

		var lblCap = new Label
		{
			AutoSize = true,
			Location = new Point(12, 48),
			Text = "Caption",
			Font = new Font("Segoe UI", 8.25F)
		};
		_txtInputCaption = new TextBox
		{
			Location = new Point(12, 66),
			Size = new Size(350, 23),
			Text = "Quick reply",
			MaxLength = 200,
			PlaceholderText = "Toast caption"
		};

		var lblPh = new Label
		{
			AutoSize = true,
			Location = new Point(12, 98),
			Text = "Placeholder",
			Font = new Font("Segoe UI", 8.25F)
		};
		var lblBtn = new Label
		{
			AutoSize = true,
			Location = new Point(250, 98),
			Text = "Button",
			Font = new Font("Segoe UI", 8.25F)
		};
		_txtInputPlaceholder = new TextBox
		{
			Location = new Point(12, 116),
			Size = new Size(228, 23),
			Text = "Your message…",
			MaxLength = 200,
			PlaceholderText = "Input placeholder"
		};
		_txtSubmitLabel = new TextBox
		{
			Location = new Point(250, 116),
			Size = new Size(112, 23),
			Text = "Send",
			MaxLength = 32,
			PlaceholderText = "Submit label"
		};

		_chkAllowEmpty = new CheckBox
		{
			AutoSize = true,
			Location = new Point(12, 152),
			Text = "Allow empty submit",
			Checked = false,
			Font = new Font("Segoe UI", 9F)
		};

		_btnShowInputable = new Button
		{
			Location = new Point(12, 178),
			Size = new Size(350, 28),
			Text = "Show inputable toast",
			UseVisualStyleBackColor = true,
			Font = new Font("Segoe UI", 9F)
		};
		_btnShowInputable.Click += BtnShowInputable_Click;

		_grpInputable.Controls.Add(_lblInputHint);
		_grpInputable.Controls.Add(lblCap);
		_grpInputable.Controls.Add(_txtInputCaption);
		_grpInputable.Controls.Add(lblPh);
		_grpInputable.Controls.Add(lblBtn);
		_grpInputable.Controls.Add(_txtInputPlaceholder);
		_grpInputable.Controls.Add(_txtSubmitLabel);
		_grpInputable.Controls.Add(_chkAllowEmpty);
		_grpInputable.Controls.Add(_btnShowInputable);
		Controls.Add(_grpInputable);
		_grpInputable.BringToFront();
	}

	private void Form1_Load(object? sender, EventArgs e)
	{
		// Final clamp once the window handle exists (working-area aware).
		try
		{
			var work = Screen.FromControl(this).WorkingArea;
			var w = Math.Min(DesignClientSize.Width, Math.Max(640, work.Width - 80));
			var h = Math.Min(DesignClientSize.Height, Math.Max(480, work.Height - 80));
			MinimumSize = Size.Empty;
			MaximumSize = Size.Empty;
			ClientSize = new Size(w, h);
			// Keep window fully on the current screen.
			var x = Math.Max(work.Left, Math.Min(Left, work.Right - Width));
			var y = Math.Max(work.Top, Math.Min(Top, work.Bottom - Height));
			Location = new Point(x, y);
		}
		catch
		{
			ClientSize = DesignClientSize;
		}

		// Shared manager: Toast.Build(this, …) and event log use the same stack.
		_toasts = new ToastManager(this, new ToastManagerOptions
		{
			MaxToasts = 6,
			MaxToastsPerPosition = 3,
			OverflowPolicy = ToastOverflowPolicy.DropNewest,
			PauseOnHover = true,
			PlaySound = false,
			HideImagePanelWhenEmpty = true
		});

		_toasts.ToastAdded += (_, e) =>
			Log($"[+ ] {ShortId(e.Toast.Id)} shown · {e.Toast.Options.Position} · {e.Toast.Options.Theme}");
		_toasts.ToastRemoved += (_, e) =>
			Log($"[- ] {ShortId(e.Toast.Id)} dismissed");
		_toasts.CollectionCleared += (_, _) =>
			Log("[   ] collection empty");
		_toasts.ToastRejected += (_, e) =>
			Log($"[ ! ] rejected ({e.Reason}): {e.Options.Caption}");

		// Capacity per corner is 3 — keep spinner in range.
		numofToasts.Minimum = 1;
		numofToasts.Maximum = 3;
		if (numofToasts.Value < 1) numofToasts.Value = 1;
		if (numofToasts.Value > 3) numofToasts.Value = 3;
		label3.Text = "Max allowed is 3 (per corner)";

		var themes = Enum.GetValues<ToastTheme>()
			.Where(t => t != ToastTheme.Custom)
			.ToArray();
		cbBuiltinThemes.DataSource = themes;
		if (cbBuiltinThemes.Items.Count > 0)
			cbBuiltinThemes.SelectedIndex = 0;

		// Close-style picker (was dead "Theme" duplicate group).
		groupBox10.Text = "Close style";
		label5.Text = "Close style:";
		comboBox1.DataSource = Enum.GetValues<CloseStyle>();
		comboBox1.SelectedItem = CloseStyle.ButtonAndClickEntire;

		// Duration radios: radioButton2 = Long (default), radioButton1 = Short
		radioButton2.Text = "Long";
		radioButton1.Text = "Short";
		radioButton2.Checked = true;

		// Inputable manager defaults (long wait while typing)
		// Already created above; InputDurationMs default is 30s — demos often override with SetDurationMs.

		Log("Demo ready — FuzzyToast v3 · inputable toast enabled · .NET 8+ WinForms");
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		try
		{
			_toasts?.Dispose();
		}
		catch
		{
			// ignore dispose races
		}

		DisposeThumbnail();
		base.OnFormClosed(e);
	}

	private ToastTheme SelectedTheme =>
		cbBuiltinThemes.SelectedItem is ToastTheme t ? t : ToastTheme.Dark;

	private CloseStyle SelectedCloseStyle =>
		comboBox1.SelectedItem is CloseStyle s ? s : CloseStyle.ButtonAndClickEntire;

	private Duration SelectedDuration =>
		radioButton1.Checked ? Duration.LENGTH_SHORT : Duration.LENGTH_LONG;

	private Animation SelectedAnimation =>
		rFade.Checked ? Animation.FADE : Animation.SLIDE;

	private void Log(string message)
	{
		if (IsDisposed || rchTextWatch.IsDisposed)
			return;

		void Append()
		{
			if (rchTextWatch.IsDisposed)
				return;
			rchTextWatch.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
			rchTextWatch.SelectionStart = rchTextWatch.TextLength;
			rchTextWatch.ScrollToCaret();
		}

		if (rchTextWatch.InvokeRequired)
			rchTextWatch.BeginInvoke(Append);
		else
			Append();
	}

	private static string ShortId(string id) =>
		id.Length >= 8 ? id[..8] : id;

	private bool TryEnterBusy()
	{
		if (_busy)
			return false;
		_busy = true;
		return true;
	}

	private void LeaveBusy() => _busy = false;

	// --- Handlers ---

	private async void BtnShowToastDemo_Click(object? sender, EventArgs e)
	{
		if (!TryEnterBusy())
			return;
		try
		{
			btnShowToastDemo.Enabled = false;
			var toast = Toast.Build(this, "Hello, I am Toast!", "Click me — metadata is returned in OnClick")
				.SetTheme(SelectedTheme)
				.SetDuration(Duration.LENGTH_SHORT)
				.SetCloseStyle(CloseStyle.ButtonAndClickEntire)
				.SetMuting(true)
				.SetData(new DemoPayload(Id: 1001, Kind: "welcome"))
				.SetExtData("action", "open-home")
				.SetMetadata("feature", "simple-demo");

			toast.OnClick += OnToastClicked;
			await toast.ShowAsync();
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			btnShowToastDemo.Enabled = true;
			LeaveBusy();
		}
	}

	private void OnToastClicked(object? sender, ToastInteractionEventArgs e)
	{
		var tagText = e.Tag switch
		{
			DemoPayload p => $"DemoPayload(Id={p.Id}, Kind={p.Kind})",
			null => "(null)",
			_ => e.Tag.ToString() ?? ""
		};

		var meta = string.Join(", ", e.Metadata.Select(kv => $"{kv.Key}={kv.Value}"));
		Log($"CLICK id={ShortId(e.ToastId)} tag={tagText} meta=[{meta}]");
		if (e.TryGetMetadata<string>("action", out var action))
			Log($"  → action: {action}");
	}

	private sealed record DemoPayload(int Id, string Kind);

	private void BtnSimpeWithCustomText_Click(object? sender, EventArgs e)
	{
		try
		{
			var text = string.IsNullOrWhiteSpace(txtText.Text) ? "Hello, I am Toast!" : txtText.Text.Trim();
			Toast.Build(this, text)
				.SetTheme(SelectedTheme)
				.SetCloseStyle(SelectedCloseStyle)
				.SetMuting(true)
				.Show();
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnShowInputable_Click(object? sender, EventArgs e) => ShowInputableToastDemo();

	/// <summary>v3 inputable toast — text box + Submit, long wait (default 60s in this demo).</summary>
	private void ShowInputableToastDemo()
	{
		try
		{
			var caption = string.IsNullOrWhiteSpace(_txtInputCaption.Text)
				? "Quick reply"
				: _txtInputCaption.Text.Trim();
			var placeholder = _txtInputPlaceholder.Text?.Trim() ?? "Your message…";
			var submit = string.IsNullOrWhiteSpace(_txtSubmitLabel.Text)
				? "Send"
				: _txtSubmitLabel.Text.Trim();

			// DurationMs = 0 (default from EnableInput): stays until Send / Esc / ✕ — no flash-dismiss.
			var toast = Toast.Build(this, caption, "Type below, then Send or press Enter")
				.SetTheme(SelectedTheme)
				.SetMuting(true)
				.EnableInput(
					placeholder: placeholder,
					defaultText: string.Empty,
					submitButtonText: submit,
					allowEmptySubmit: _chkAllowEmpty.Checked)
				.SetCloseStyle(CloseStyle.Button)
				.SetExtData("action", "quick-input")
				.SetMetadata("demo", "inputable-v3")
				.SetTag(new DemoPayload(Id: 3001, Kind: "inputable"));

			toast.OnSubmit += (_, e) =>
			{
				Log($"SUBMIT id={ShortId(e.ToastId)} text=\"{e.InputText}\" empty={e.IsEmpty}");
				Log($"  tag={e.Tag} action={e.GetMetadata<string>("action")} demo={e.GetMetadata<string>("demo")}");
			};
			toast.OnClosed += (_, _) => Log("Inputable toast closed (submit, Esc, or close button)");
			toast.Show();
			Log($"Inputable toast shown (stays open until Send/Esc/✕) · placeholder=\"{placeholder}\" · submit=\"{submit}\"");
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnInsertImage_Click(object? sender, EventArgs e)
	{
		using var opDlg = new OpenFileDialog
		{
			Title = "Choose toast thumbnail",
			Filter = "Image files|*.jpg;*.jpeg;*.png|JPEG|*.jpg;*.jpeg|PNG|*.png",
			CheckFileExists = true,
			Multiselect = false,
			RestoreDirectory = true,
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
		};

		if (opDlg.ShowDialog(this) != DialogResult.OK)
			return;

		try
		{
			if (!ImageValidation.ValidateImagePath(opDlg.FileName))
			{
				MessageBox.Show(this, "File is not a supported JPEG or PNG image.", "Invalid file",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// Load into independent bitmap so file handle is released.
			using var fs = File.OpenRead(opDlg.FileName);
			using var temp = Image.FromStream(fs);
			var loaded = new Bitmap(temp);

			if (!ImageValidation.ValidateImageSize(loaded, 64, 64))
			{
				loaded.Dispose();
				MessageBox.Show(this,
					"Image must be at least 64×64 pixels (recommended 80×80 square).",
					"Invalid size",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			DisposeThumbnail();
			picThumbnail.Image = loaded;
			Log($"Thumbnail loaded: {Path.GetFileName(opDlg.FileName)} ({loaded.Width}×{loaded.Height})");
		}
		catch (Exception ex)
		{
			Log($"Image error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Image error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnToastTextImage_Click(object? sender, EventArgs e)
	{
		if (picThumbnail.Image is null)
		{
			MessageBox.Show(this, "Please choose an image first.", "Image required",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		try
		{
			if (string.IsNullOrWhiteSpace(txttextImage.Text))
				txttextImage.Text = "Hello! I'm Toast! :)";

			// Clone so reloading the PictureBox does not break a visible toast; toast owns the clone.
			var thumb = (Image)picThumbnail.Image.Clone();
			var toast = Toast.Build(this, txttextImage.Text.Trim())
				.SetDescription("With thumbnail")
				.SetTheme(SelectedTheme)
				.SetCloseStyle(SelectedCloseStyle)
				.SetMuting(true)
				.SetThumbnail(thumb, ownsImage: true);

			toast
				.SetExtData("action", "open-image")
				.SetMetadata("hasThumbnail", true)
				.SetTag(txttextImage.Text.Trim());
			toast.OnClick += OnToastClicked;
			toast.Show();
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnDisplayMultiple_Click(object? sender, EventArgs e)
	{
		try
		{
			var count = (int)numofToasts.Value;
			for (var i = 1; i <= count; i++)
			{
				Toast.Build(this, $"This is Toast {i}", $"Stack item {i} of {count}")
					.SetTheme(SelectedTheme)
					.SetPosition(ToastPosition.BottomRight)
					.SetCloseStyle(SelectedCloseStyle)
					.SetMuting(true)
					.Show();
			}
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnToastWithAnimation_Click(object? sender, EventArgs e)
	{
		try
		{
			var caption = string.IsNullOrWhiteSpace(txtAnimation.Text)
				? "Hello, I am Toast!"
				: txtAnimation.Text.Trim();

			Toast.Build(this, caption, SelectedAnimation)
				.SetTheme(SelectedTheme)
				.SetCloseStyle(SelectedCloseStyle)
				.SetMuting(true)
				.Show();
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnCustomDuration_Click(object? sender, EventArgs e)
	{
		try
		{
			var caption = string.IsNullOrWhiteSpace(textBox1.Text)
				? "Hello, I am Toast!"
				: textBox1.Text.Trim();

			Toast.Build(this, caption, SelectedDuration)
				.SetTheme(SelectedTheme)
				.SetDescription(SelectedDuration == Duration.LENGTH_LONG ? "Duration: LONG (~3s)" : "Duration: SHORT (~2s)")
				.SetCloseStyle(SelectedCloseStyle)
				.SetMuting(true)
				.Show();
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void BtnTopRight_Click(object? sender, EventArgs e)
	{
		ShowPositioned(ToastPosition.TopRight, "Top-Right toast");
	}

	private void BtnBottom_Click(object? sender, EventArgs e)
	{
		ShowPositioned(ToastPosition.BottomRight, "Bottom-Right toast");
	}

	private void ShowPositioned(ToastPosition position, string caption)
	{
		try
		{
			Toast.Build(this, caption, $"Position: {position}")
				.SetPosition(position)
				.SetTheme(SelectedTheme)
				.SetCloseStyle(SelectedCloseStyle)
				.SetMuting(true)
				.Show();
		}
		catch (Exception ex)
		{
			Log($"Error: {ex.Message}");
			MessageBox.Show(this, ex.Message, "Toast error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void About_Click(object? sender, EventArgs e)
	{
		MessageBox.Show(this,
			"FuzzyToast Demo v3\n\n" +
			"Windows Forms toast library for Windows 10/11 · .NET 8+\n\n" +
			"API:\n" +
			"  Toast.Build(this, \"…\").Show()\n" +
			"  .EnableInput() — text box + Submit (v3)\n" +
			"  .SetTag / .SetExtData — data on click/submit\n\n" +
			"Use the panel \"Inputable toast (v3)\" on the form to try input.\n\n" +
			"MIT License",
			"About FuzzyToast Demo",
			MessageBoxButtons.OK,
			MessageBoxIcon.Information);
	}

	private void Form1_Click(object? sender, EventArgs e)
	{
		// no-op (designer wiring)
	}

	private void DisposeThumbnail()
	{
		var img = picThumbnail.Image;
		picThumbnail.Image = null;
		img?.Dispose();
	}
}
