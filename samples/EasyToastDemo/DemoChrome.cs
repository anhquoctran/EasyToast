using System.Drawing.Drawing2D;
using System.Runtime.Versioning;

namespace EasyToastDemo;

/// <summary>Flat light palette and chrome for the demo catalog.</summary>
[SupportedOSPlatform("windows")]
internal static class UiTheme
{
	public static readonly Color Canvas = Color.FromArgb(245, 247, 250);
	public static readonly Color Card = Color.FromArgb(255, 255, 255);
	public static readonly Color Border = Color.FromArgb(226, 232, 240);
	public static readonly Color BorderHot = Color.FromArgb(147, 197, 253);
	public static readonly Color Text = Color.FromArgb(30, 41, 59);
	public static readonly Color Muted = Color.FromArgb(100, 116, 139);
	public static readonly Color Accent = Color.FromArgb(37, 99, 235);
	public static readonly Color AccentHot = Color.FromArgb(29, 78, 216);
	public static readonly Color AccentDown = Color.FromArgb(30, 64, 175);
	public static readonly Color AccentSoft = Color.FromArgb(239, 246, 255);
	public static readonly Color Button = Color.FromArgb(226, 232, 240);
	public static readonly Color ButtonHot = Color.FromArgb(203, 213, 225);
	public static readonly Color ButtonDown = Color.FromArgb(148, 163, 184);
	public static readonly Color ButtonBorder = Color.FromArgb(100, 116, 139);
	public static readonly Color Input = Color.FromArgb(255, 255, 255);

	public static readonly Font Ui = new("Segoe UI", 9F);
	public static readonly Font Hint = new("Segoe UI", 8.25F);
	public static readonly Font Title = new("Segoe UI Semibold", 9F, FontStyle.Regular, GraphicsUnit.Point);
	public static readonly Font Tab = new("Segoe UI Semibold", 9F, FontStyle.Regular, GraphicsUnit.Point);
}

/// <summary>White card with a 1px border and a title — no GroupBox etching.</summary>
[SupportedOSPlatform("windows")]
internal sealed class FlatCard : Panel
{
	private readonly Label? _title;
	private Control? _headerAction;

	public FlatCard(string title)
	{
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
		BackColor = UiTheme.Card;
		ForeColor = UiTheme.Text;
		// Padding only affects docked children (log). Absolute children keep their GroupBox-era coords.
		Padding = new Padding(1, 28, 1, 1);
		_title = new Label
		{
			AutoSize = false,
			AutoEllipsis = true,
			Text = title,
			Location = new Point(12, 6),
			Size = new Size(200, 18),
			Font = UiTheme.Title,
			ForeColor = UiTheme.Text,
			BackColor = UiTheme.Card,
			UseMnemonic = false
		};
		Controls.Add(_title);
	}

	/// <summary>Places a compact control on the title row, right-aligned (e.g. Clear log).</summary>
	public void SetHeaderAction(Control action)
	{
		_headerAction = action;
		action.Anchor = AnchorStyles.None;
		Controls.Add(action);
		action.BringToFront();
		LayoutHeader();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		LayoutHeader();
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout(levent);
		LayoutHeader();
	}

	private void LayoutHeader()
	{
		if (_title is null || ClientSize.Width <= 0)
			return;

		var right = ClientSize.Width - 12;
		if (_headerAction is not null)
		{
			_headerAction.Location = new Point(Math.Max(12, right - _headerAction.Width), 2);
			right = _headerAction.Left - 12;
		}
		_title.Size = new Size(Math.Max(40, right - _title.Left), 18);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using var pen = new Pen(UiTheme.Border);
		e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
	}
}

/// <summary>Text-style tab row with an accent underline. Pages are swapped in <see cref="_body"/>.</summary>
[SupportedOSPlatform("windows")]
internal sealed class FlatTabStrip : Panel
{
	private readonly Panel _body;
	private readonly List<(Button Button, Control Page)> _tabs = [];
	private int _selected;

	public FlatTabStrip(Panel body)
	{
		_body = body;
		Height = 38;
		Dock = DockStyle.Top;
		BackColor = UiTheme.Card;
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
	}

	public void Add(string title, Control page)
	{
		var width = Math.Max(88, TextRenderer.MeasureText(title, UiTheme.Tab).Width + 28);
		var left = _tabs.Count == 0 ? 8 : _tabs[^1].Button.Right;
		var btn = new Button
		{
			Text = title,
			Font = UiTheme.Tab,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(left, 0),
			Size = new Size(width, 37),
			BackColor = UiTheme.Card,
			ForeColor = UiTheme.Muted,
			Cursor = Cursors.Hand,
			TabStop = true,
			UseMnemonic = false,
			UseVisualStyleBackColor = false
		};
		btn.FlatAppearance.BorderSize = 0;
		btn.FlatAppearance.MouseOverBackColor = UiTheme.AccentSoft;
		btn.FlatAppearance.MouseDownBackColor = UiTheme.ButtonDown;
		var index = _tabs.Count;
		btn.Click += (_, _) => SelectTab(index);
		_tabs.Add((btn, page));
		Controls.Add(btn);

		page.Dock = DockStyle.Fill;
		page.Visible = false;
		page.BackColor = UiTheme.Canvas;
		_body.Controls.Add(page);

		if (_tabs.Count == 1)
			SelectTab(0);
	}

	public void SelectTab(int index)
	{
		if (index < 0 || index >= _tabs.Count)
			return;
		_selected = index;
		for (var i = 0; i < _tabs.Count; i++)
		{
			var on = i == index;
			_tabs[i].Page.Visible = on;
			_tabs[i].Button.ForeColor = on ? UiTheme.Accent : UiTheme.Muted;
		}
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.SmoothingMode = SmoothingMode.None;
		using (var pen = new Pen(UiTheme.Border))
			e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);

		if (_tabs.Count == 0)
			return;

		var sel = _tabs[_selected].Button;
		using var accent = new Pen(UiTheme.Accent, 2);
		var y = Height - 2;
		e.Graphics.DrawLine(accent, sel.Left + 10, y, sel.Right - 10, y);
	}
}

/// <summary>Removes the default 3D menu gradients.</summary>
[SupportedOSPlatform("windows")]
internal sealed class FlatToolStripRenderer : ToolStripProfessionalRenderer
{
	public FlatToolStripRenderer() : base(new FlatColorTable())
	{
		RoundedEdges = false;
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		using var pen = new Pen(UiTheme.Border);
		e.Graphics.DrawLine(pen, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
	}

	private sealed class FlatColorTable : ProfessionalColorTable
	{
		public override Color MenuStripGradientBegin => UiTheme.Card;
		public override Color MenuStripGradientEnd => UiTheme.Card;
		public override Color MenuItemSelected => UiTheme.AccentSoft;
		public override Color MenuItemSelectedGradientBegin => UiTheme.AccentSoft;
		public override Color MenuItemSelectedGradientEnd => UiTheme.AccentSoft;
		public override Color MenuItemBorder => UiTheme.Border;
		public override Color MenuBorder => UiTheme.Border;
		public override Color MenuItemPressedGradientBegin => UiTheme.ButtonDown;
		public override Color MenuItemPressedGradientEnd => UiTheme.ButtonDown;
		public override Color ImageMarginGradientBegin => UiTheme.Card;
		public override Color ImageMarginGradientMiddle => UiTheme.Card;
		public override Color ImageMarginGradientEnd => UiTheme.Card;
		public override Color ToolStripDropDownBackground => UiTheme.Card;
		public override Color SeparatorDark => UiTheme.Border;
		public override Color SeparatorLight => UiTheme.Card;
		public override Color OverflowButtonGradientBegin => UiTheme.Card;
		public override Color OverflowButtonGradientEnd => UiTheme.Card;
		public override Color RaftingContainerGradientBegin => UiTheme.Card;
		public override Color RaftingContainerGradientEnd => UiTheme.Card;
	}
}
