#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

partial class ToastForm
{
	private System.ComponentModel.IContainer? components;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
			DisposeOwnedThumbnail();
		}
		base.Dispose(disposing);
		_disposed = true;
	}

	private void InitializeComponent()
	{
		components = new System.ComponentModel.Container();
		mainContainer = new SplitContainer();
		picImage = new PictureBox();
		textContainer = new SplitContainer();
		btnClose = new Button();
		lblDescription = new Label();
		lblCaption = new Label();
		tmrClose = new System.Windows.Forms.Timer(components);
		((System.ComponentModel.ISupportInitialize)mainContainer).BeginInit();
		mainContainer.Panel1.SuspendLayout();
		mainContainer.Panel2.SuspendLayout();
		mainContainer.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
		((System.ComponentModel.ISupportInitialize)textContainer).BeginInit();
		textContainer.Panel1.SuspendLayout();
		textContainer.Panel2.SuspendLayout();
		textContainer.SuspendLayout();
		SuspendLayout();
		// mainContainer
		mainContainer.BackColor = Color.Transparent;
		mainContainer.Dock = DockStyle.Fill;
		mainContainer.FixedPanel = FixedPanel.Panel1;
		mainContainer.IsSplitterFixed = true;
		mainContainer.Name = "mainContainer";
		mainContainer.Panel1.Controls.Add(picImage);
		mainContainer.Panel1.Click += ToastContentClick;
		mainContainer.Panel1MinSize = 96;
		mainContainer.Panel1Collapsed = true;
		mainContainer.Panel2.Controls.Add(textContainer);
		mainContainer.Panel2.Click += ToastContentClick;
		mainContainer.Panel2.Padding = new Padding(0, 0, 4, 0);
		mainContainer.Size = new Size(420, 140);
		mainContainer.SplitterDistance = 96;
		mainContainer.SplitterWidth = 1;
		// picImage
		picImage.BackColor = Color.FromArgb(48, 48, 48);
		picImage.Dock = DockStyle.Fill;
		picImage.Name = "picImage";
		picImage.Size = new Size(96, 140);
		picImage.SizeMode = PictureBoxSizeMode.Zoom;
		picImage.TabStop = false;
		picImage.Click += ToastContentClick;
		// textContainer
		textContainer.Dock = DockStyle.Fill;
		textContainer.FixedPanel = FixedPanel.Panel1;
		textContainer.IsSplitterFixed = true;
		textContainer.Name = "textContainer";
		textContainer.Orientation = Orientation.Horizontal;
		textContainer.Panel1.Controls.Add(btnClose);
		textContainer.Panel1.Controls.Add(lblCaption);
		textContainer.Panel1.Padding = new Padding(12, 12, 8, 0);
		textContainer.Panel2.Controls.Add(lblDescription);
		textContainer.Panel2.Padding = new Padding(12, 8, 12, 12);
		textContainer.Size = new Size(323, 140);
		textContainer.SplitterDistance = 52;
		textContainer.SplitterWidth = 1;
		// btnClose
		btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		btnClose.Cursor = Cursors.Hand;
		btnClose.FlatAppearance.BorderSize = 0;
		btnClose.FlatStyle = FlatStyle.Flat;
		btnClose.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
		btnClose.Location = new Point(267, 4);
		btnClose.Name = "btnClose";
		btnClose.Size = new Size(44, 44);
		btnClose.TabIndex = 1;
		btnClose.Text = "✕";
		btnClose.UseVisualStyleBackColor = false;
		btnClose.Click += BtnClose_Click;
		// lblDescription
		lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		lblDescription.AutoEllipsis = true;
		lblDescription.BackColor = Color.Transparent;
		lblDescription.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
		lblDescription.ForeColor = Color.Silver;
		lblDescription.Location = new Point(0, 0);
		lblDescription.Name = "lblDescription";
		lblDescription.Padding = new Padding(4, 0, 28, 0);
		lblDescription.Size = new Size(299, 64);
		lblDescription.Click += ToastContentClick;
		// lblCaption
		lblCaption.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		lblCaption.AutoEllipsis = true;
		lblCaption.BackColor = Color.Transparent;
		lblCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
		lblCaption.ForeColor = Color.White;
		lblCaption.Location = new Point(0, 4);
		lblCaption.Name = "lblCaption";
		lblCaption.Padding = new Padding(4, 0, 8, 0);
		lblCaption.Size = new Size(255, 36);
		lblCaption.TextAlign = ContentAlignment.MiddleLeft;
		lblCaption.Click += ToastContentClick;
		// tmrClose
		tmrClose.Tick += TmrClose_Tick;
		// ToastForm
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = Color.FromArgb(33, 33, 33);
		ClientSize = new Size(420, 140);
		ControlBox = false;
		Controls.Add(mainContainer);
		DoubleBuffered = true;
		Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
		FormBorderStyle = FormBorderStyle.None;
		MinimumSize = new Size(360, 120);
		Name = "ToastForm";
		Opacity = 0.98D;
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.Manual;
		Text = "Toast";
		TopMost = true;
		FormClosing += ToastForm_FormClosing;
		FormClosed += ToastForm_FormClosed;
		Load += ToastForm_Load;
		Shown += ToastForm_Shown;
		Click += ToastContentClick;
		MouseEnter += ToastForm_MouseEnter;
		MouseLeave += ToastForm_MouseLeave;
		mainContainer.Panel1.ResumeLayout(false);
		mainContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)mainContainer).EndInit();
		mainContainer.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)picImage).EndInit();
		textContainer.Panel1.ResumeLayout(false);
		textContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)textContainer).EndInit();
		textContainer.ResumeLayout(false);
		ResumeLayout(false);

		// Propagate hover pause to children (mouse leave on child would otherwise cancel hover).
		WireHover(mainContainer);
		WireHover(picImage);
		WireHover(textContainer);
		WireHover(lblCaption);
		WireHover(lblDescription);
		WireHover(btnClose);
	}

	private void WireHover(Control c)
	{
		c.MouseEnter += ToastForm_MouseEnter;
		c.MouseLeave += ToastForm_MouseLeave;
	}

	private SplitContainer mainContainer = null!;
	private PictureBox picImage = null!;
	private Label lblCaption = null!;
	private System.Windows.Forms.Timer tmrClose = null!;
	private SplitContainer textContainer = null!;
	private Button btnClose = null!;
	private Label lblDescription = null!;
}
