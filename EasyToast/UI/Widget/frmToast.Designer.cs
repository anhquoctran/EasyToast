﻿namespace System.UI.Widget
{
	partial class FrmToast
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.picImage = new System.Windows.Forms.PictureBox();
            this.textContainer = new System.Windows.Forms.SplitContainer();
            this.btnClose = new System.Windows.Forms.Button();
            this.picAppOwnerIcon = new System.Windows.Forms.PictureBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.tmrClose = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textContainer)).BeginInit();
            this.textContainer.Panel1.SuspendLayout();
            this.textContainer.Panel2.SuspendLayout();
            this.textContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAppOwnerIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // mainContainer
            // 
            this.mainContainer.BackColor = System.Drawing.Color.Transparent;
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainContainer.ForeColor = System.Drawing.Color.White;
            this.mainContainer.IsSplitterFixed = true;
            this.mainContainer.Location = new System.Drawing.Point(0, 0);
            this.mainContainer.Name = "mainContainer";
            // 
            // mainContainer.Panel1
            // 
            this.mainContainer.Panel1.Controls.Add(this.picImage);
            this.mainContainer.Panel1.Click += new System.EventHandler(this.ToastContentClick);
            this.mainContainer.Panel1MinSize = 110;
            // 
            // mainContainer.Panel2
            // 
            this.mainContainer.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.mainContainer.Panel2.Controls.Add(this.textContainer);
            this.mainContainer.Panel2.Click += new System.EventHandler(this.ToastContentClick);
            this.mainContainer.Size = new System.Drawing.Size(406, 110);
            this.mainContainer.SplitterDistance = 110;
            this.mainContainer.SplitterWidth = 1;
            this.mainContainer.TabIndex = 0;
            // 
            // picImage
            // 
            this.picImage.BackColor = System.Drawing.Color.DimGray;
            this.picImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picImage.Location = new System.Drawing.Point(0, 0);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(110, 110);
            this.picImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picImage.TabIndex = 0;
            this.picImage.TabStop = false;
            this.picImage.Click += new System.EventHandler(this.ToastContentClick);
            // 
            // textContainer
            // 
            this.textContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.textContainer.IsSplitterFixed = true;
            this.textContainer.Location = new System.Drawing.Point(0, 0);
            this.textContainer.Name = "textContainer";
            this.textContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // textContainer.Panel1
            // 
            this.textContainer.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.textContainer.Panel1.Controls.Add(this.btnClose);
            this.textContainer.Panel1.Controls.Add(this.lblCaption);
            // 
            // textContainer.Panel2
            // 
            this.textContainer.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.textContainer.Panel2.Controls.Add(this.picAppOwnerIcon);
            this.textContainer.Panel2.Controls.Add(this.lblDescription);
            this.textContainer.Size = new System.Drawing.Size(295, 110);
            this.textContainer.SplitterDistance = 30;
            this.textContainer.SplitterWidth = 1;
            this.textContainer.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(266, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(28, 25);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "x";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // picAppOwnerIcon
            // 
            this.picAppOwnerIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picAppOwnerIcon.BackColor = System.Drawing.Color.DimGray;
            this.picAppOwnerIcon.Image = global::System.Properties.Resources.exe_icon;
            this.picAppOwnerIcon.Location = new System.Drawing.Point(267, 55);
            this.picAppOwnerIcon.Name = "picAppOwnerIcon";
            this.picAppOwnerIcon.Size = new System.Drawing.Size(20, 20);
            this.picAppOwnerIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picAppOwnerIcon.TabIndex = 2;
            this.picAppOwnerIcon.TabStop = false;
            // 
            // lblDescription
            // 
            this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDescription.AutoEllipsis = true;
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblDescription.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescription.Location = new System.Drawing.Point(0, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Padding = new System.Windows.Forms.Padding(2);
            this.lblDescription.Size = new System.Drawing.Size(292, 52);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec suscipit, diam qui" +
    "s sollicitudin pulvinar, diam tortor aliquam purus, eu vestibulum magna libero v" +
    "el diam.";
            // 
            // lblCaption
            // 
            this.lblCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCaption.AutoEllipsis = true;
            this.lblCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblCaption.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblCaption.Location = new System.Drawing.Point(3, 0);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(257, 27);
            this.lblCaption.TabIndex = 0;
            this.lblCaption.Text = "@caption";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCaption.Click += new System.EventHandler(this.ToastContentClick);
            // 
            // tmrClose
            // 
            this.tmrClose.Interval = 1000;
            this.tmrClose.Tick += new System.EventHandler(this.TmrClose_Tick);
            // 
            // FrmToast
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.ClientSize = new System.Drawing.Size(406, 110);
            this.ControlBox = false;
            this.Controls.Add(this.mainContainer);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmToast";
            this.Opacity = 0.98D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmToast";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmToast_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmToast_FormClosed);
            this.Load += new System.EventHandler(this.FrmToast_Load);
            this.Shown += new System.EventHandler(this.FrmToast_Shown);
            this.Click += new System.EventHandler(this.ToastContentClick);
            this.mainContainer.Panel1.ResumeLayout(false);
            this.mainContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
            this.mainContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.textContainer.Panel1.ResumeLayout(false);
            this.textContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.textContainer)).EndInit();
            this.textContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picAppOwnerIcon)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer mainContainer;
		private System.Windows.Forms.PictureBox picImage;
		private System.Windows.Forms.Label lblCaption;
		private System.Windows.Forms.Timer tmrClose;
		private System.Windows.Forms.SplitContainer textContainer;
		private System.Windows.Forms.Button btnClose;
		private Windows.Forms.Label lblDescription;
		private Windows.Forms.PictureBox picAppOwnerIcon;
	}
}