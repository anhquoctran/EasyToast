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
		contentShell = new Panel();
		mainContainer = new SplitContainer();
		picImage = new PictureBox();
		textContainer = new SplitContainer();
		btnClose = new Button();
		lblDescription = new Label();
		lblCaption = new Label();
		tmrClose = new System.Windows.Forms.Timer(components);

		contentShell.SuspendLayout();
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

		// contentShell — parent padding so all children sit inset from the toast edge
		contentShell.BackColor = Color.Transparent;
		contentShell.Dock = DockStyle.Fill;
		contentShell.Name = "contentShell";
		contentShell.Padding = new Padding(12, 10, 12, 10);
		contentShell.Controls.Add(mainContainer);

		// mainContainer
		mainContainer.BackColor = Color.Transparent;
		mainContainer.Dock = DockStyle.Fill;
		mainContainer.FixedPanel = FixedPanel.Panel1;
		mainContainer.IsSplitterFixed = true;
		mainContainer.Name = "mainContainer";
		mainContainer.Panel1.Controls.Add(picImage);
		mainContainer.Panel1.Click += ToastContentClick;
		mainContainer.Panel1.Padding = new Padding(0, 0, 8, 0); // gap between thumb and text
		mainContainer.Panel1MinSize = 64;
		mainContainer.Panel1Collapsed = true;
		mainContainer.Panel2.Controls.Add(textContainer);
		mainContainer.Panel2.Click += ToastContentClick;
		mainContainer.Panel2.Padding = new Padding(0);
		mainContainer.SplitterDistance = 72;
		mainContainer.SplitterWidth = 1;

		// picImage
		picImage.BackColor = Color.FromArgb(48, 48, 48);
		picImage.Dock = DockStyle.Fill;
		picImage.Name = "picImage";
		picImage.SizeMode = PictureBoxSizeMode.Zoom;
		picImage.TabStop = false;
		picImage.Click += ToastContentClick;
		picImage.Margin = new Padding(0);

		// textContainer
		textContainer.Dock = DockStyle.Fill;
		textContainer.FixedPanel = FixedPanel.Panel1;
		textContainer.IsSplitterFixed = true;
		textContainer.Name = "textContainer";
		textContainer.Orientation = Orientation.Horizontal;
		textContainer.Panel1.Controls.Add(btnClose);
		textContainer.Panel1.Controls.Add(lblCaption);
		// Inner spacing between caption / close (edge inset comes from contentShell)
		textContainer.Panel1.Padding = new Padding(2, 0, 0, 2);
		textContainer.Panel2.Controls.Add(lblDescription);
		textContainer.Panel2.Padding = new Padding(2, 2, 2, 0);
		textContainer.SplitterDistance = 28;
		textContainer.SplitterWidth = 1;

		// btnClose
		btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		btnClose.Cursor = Cursors.Hand;
		btnClose.FlatAppearance.BorderSize = 0;
		btnClose.FlatStyle = FlatStyle.Flat;
		btnClose.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
		btnClose.Location = new Point(250, 0);
		btnClose.Name = "btnClose";
		btnClose.Size = new Size(32, 26);
		btnClose.TabIndex = 1;
		btnClose.Text = "✕";
		btnClose.UseVisualStyleBackColor = false;
		btnClose.Click += BtnClose_Click;
		btnClose.Margin = new Padding(0);

		// lblDescription
		lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		lblDescription.AutoEllipsis = true;
		lblDescription.BackColor = Color.Transparent;
		lblDescription.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
		lblDescription.ForeColor = Color.Silver;
		lblDescription.Location = new Point(0, 0);
		lblDescription.Name = "lblDescription";
		lblDescription.Padding = new Padding(0, 0, 4, 0);
		lblDescription.Size = new Size(260, 32);
		lblDescription.Click += ToastContentClick;
		lblDescription.Margin = new Padding(0);

		// lblCaption
		lblCaption.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		lblCaption.AutoEllipsis = true;
		lblCaption.BackColor = Color.Transparent;
		lblCaption.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
		lblCaption.ForeColor = Color.White;
		lblCaption.Location = new Point(0, 0);
		lblCaption.Name = "lblCaption";
		lblCaption.Padding = new Padding(0, 0, 6, 0);
		lblCaption.Size = new Size(240, 24);
		lblCaption.TextAlign = ContentAlignment.MiddleLeft;
		lblCaption.Click += ToastContentClick;
		lblCaption.Margin = new Padding(0);

		// tmrClose
		tmrClose.Tick += TmrClose_Tick;

		// ToastForm
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = Color.FromArgb(33, 33, 33);
		ClientSize = new Size(380, 100);
		ControlBox = false;
		Controls.Add(contentShell);
		DoubleBuffered = true;
		Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
		FormBorderStyle = FormBorderStyle.None;
		MinimumSize = new Size(280, 72);
		Name = "ToastForm";
		Opacity = 0.98D;
		ShowIcon = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.Manual;
		Text = "Toast";
		TopMost = true;
		Padding = new Padding(0);
		FormClosing += ToastForm_FormClosing;
		FormClosed += ToastForm_FormClosed;
		Load += ToastForm_Load;
		Shown += ToastForm_Shown;
		Click += ToastContentClick;
		MouseEnter += ToastForm_MouseEnter;
		MouseLeave += ToastForm_MouseLeave;

		contentShell.ResumeLayout(false);
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

		WireHover(contentShell);
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

	/// <summary>Parent shell that applies outer padding around all toast content.</summary>
	private Panel contentShell = null!;
	private SplitContainer mainContainer = null!;
	private PictureBox picImage = null!;
	private Label lblCaption = null!;
	private System.Windows.Forms.Timer tmrClose = null!;
	private SplitContainer textContainer = null!;
	private Button btnClose = null!;
	private Label lblDescription = null!;
}
