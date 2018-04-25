namespace EasyToastDemo
{
	partial class Form1
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
			this.btnShowToastDemo = new System.Windows.Forms.Button();
			this.btnSimpeWithCustomText = new System.Windows.Forms.Button();
			this.txtText = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnInsertImage = new System.Windows.Forms.Button();
			this.picThumbnail = new System.Windows.Forms.PictureBox();
			this.txttextImage = new System.Windows.Forms.TextBox();
			this.btnToastTextImage = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnDisplayMultiple = new System.Windows.Forms.Button();
			this.numofToasts = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.button4 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.btnToastWithAnimation = new System.Windows.Forms.Button();
			this.txtAnimation = new System.Windows.Forms.TextBox();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.timer2 = new System.Windows.Forms.Timer(this.components);
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numofToasts)).BeginInit();
			this.groupBox5.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnShowToastDemo
			// 
			this.btnShowToastDemo.Location = new System.Drawing.Point(6, 19);
			this.btnShowToastDemo.Name = "btnShowToastDemo";
			this.btnShowToastDemo.Size = new System.Drawing.Size(226, 30);
			this.btnShowToastDemo.TabIndex = 0;
			this.btnShowToastDemo.Text = "Show a simple toast";
			this.btnShowToastDemo.UseVisualStyleBackColor = true;
			this.btnShowToastDemo.Click += new System.EventHandler(this.BtnShowToastDemo_Click);
			// 
			// btnSimpeWithCustomText
			// 
			this.btnSimpeWithCustomText.Location = new System.Drawing.Point(6, 45);
			this.btnSimpeWithCustomText.Name = "btnSimpeWithCustomText";
			this.btnSimpeWithCustomText.Size = new System.Drawing.Size(226, 30);
			this.btnSimpeWithCustomText.TabIndex = 1;
			this.btnSimpeWithCustomText.Text = "Show a simple toast with custom text";
			this.btnSimpeWithCustomText.UseVisualStyleBackColor = true;
			this.btnSimpeWithCustomText.Click += new System.EventHandler(this.btnSimpeWithCustomText_Click);
			// 
			// txtText
			// 
			this.txtText.Location = new System.Drawing.Point(6, 19);
			this.txtText.MaxLength = 64;
			this.txtText.Name = "txtText";
			this.txtText.Size = new System.Drawing.Size(226, 20);
			this.txtText.TabIndex = 2;
			this.txtText.Text = "Hello, I am Toast!";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnShowToastDemo);
			this.groupBox1.Location = new System.Drawing.Point(12, 27);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(238, 59);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Simplest Toast";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtText);
			this.groupBox2.Controls.Add(this.btnSimpeWithCustomText);
			this.groupBox2.Location = new System.Drawing.Point(12, 92);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(238, 86);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Simple with custom text";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.btnInsertImage);
			this.groupBox3.Controls.Add(this.picThumbnail);
			this.groupBox3.Controls.Add(this.txttextImage);
			this.groupBox3.Controls.Add(this.btnToastTextImage);
			this.groupBox3.Location = new System.Drawing.Point(12, 184);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(238, 221);
			this.groupBox3.TabIndex = 5;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Text and thumbnail";
			// 
			// label1
			// 
			this.label1.AutoEllipsis = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
			this.label1.ForeColor = System.Drawing.Color.DimGray;
			this.label1.Location = new System.Drawing.Point(6, 112);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(226, 59);
			this.label1.TabIndex = 5;
			this.label1.Text = "Required minimum size of thumbnail is 64x64. Recommended size is 80x80. Square ra" +
    "tio for best display. JPEG and PNG format supported.";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnInsertImage
			// 
			this.btnInsertImage.Location = new System.Drawing.Point(76, 45);
			this.btnInsertImage.Name = "btnInsertImage";
			this.btnInsertImage.Size = new System.Drawing.Size(94, 30);
			this.btnInsertImage.TabIndex = 4;
			this.btnInsertImage.Text = "Choose image";
			this.btnInsertImage.UseVisualStyleBackColor = true;
			this.btnInsertImage.Click += new System.EventHandler(this.BtnInsertImage_Click);
			// 
			// picThumbnail
			// 
			this.picThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picThumbnail.Location = new System.Drawing.Point(6, 45);
			this.picThumbnail.Name = "picThumbnail";
			this.picThumbnail.Size = new System.Drawing.Size(64, 64);
			this.picThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picThumbnail.TabIndex = 3;
			this.picThumbnail.TabStop = false;
			// 
			// txttextImage
			// 
			this.txttextImage.Location = new System.Drawing.Point(6, 19);
			this.txttextImage.MaxLength = 64;
			this.txttextImage.Name = "txttextImage";
			this.txttextImage.Size = new System.Drawing.Size(226, 20);
			this.txttextImage.TabIndex = 2;
			this.txttextImage.Text = "Hello, I am Toast!";
			// 
			// btnToastTextImage
			// 
			this.btnToastTextImage.Location = new System.Drawing.Point(6, 185);
			this.btnToastTextImage.Name = "btnToastTextImage";
			this.btnToastTextImage.Size = new System.Drawing.Size(226, 30);
			this.btnToastTextImage.TabIndex = 1;
			this.btnToastTextImage.Text = "Show toast with text and thumbnail image";
			this.btnToastTextImage.UseVisualStyleBackColor = true;
			this.btnToastTextImage.Click += new System.EventHandler(this.BtnToastTextImage_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label3);
			this.groupBox4.Controls.Add(this.btnDisplayMultiple);
			this.groupBox4.Controls.Add(this.numofToasts);
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Location = new System.Drawing.Point(256, 27);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(225, 94);
			this.groupBox4.TabIndex = 6;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Multiple toasts";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 46);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(85, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Max allowed is 3";
			// 
			// btnDisplayMultiple
			// 
			this.btnDisplayMultiple.Location = new System.Drawing.Point(6, 62);
			this.btnDisplayMultiple.Name = "btnDisplayMultiple";
			this.btnDisplayMultiple.Size = new System.Drawing.Size(213, 27);
			this.btnDisplayMultiple.TabIndex = 2;
			this.btnDisplayMultiple.Text = "Display random multiple Toast";
			this.btnDisplayMultiple.UseVisualStyleBackColor = true;
			this.btnDisplayMultiple.Click += new System.EventHandler(this.btnDisplayMultiple_Click);
			// 
			// numofToasts
			// 
			this.numofToasts.Location = new System.Drawing.Point(148, 19);
			this.numofToasts.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numofToasts.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numofToasts.Name = "numofToasts";
			this.numofToasts.Size = new System.Drawing.Size(71, 20);
			this.numofToasts.TabIndex = 1;
			this.numofToasts.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.numofToasts.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 21);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(136, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Number of Toast to display:";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.button4);
			this.groupBox5.Controls.Add(this.button3);
			this.groupBox5.Location = new System.Drawing.Point(256, 127);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(225, 51);
			this.groupBox5.TabIndex = 7;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Position";
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(87, 19);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 3;
			this.button4.Text = "Bottom Right";
			this.button4.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(6, 19);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 2;
			this.button3.Text = "Top Right";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(901, 24);
			this.menuStrip1.TabIndex = 8;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1});
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.aboutToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem1
			// 
			this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
			this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
			this.aboutToolStripMenuItem1.Text = "About";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.btnToastWithAnimation);
			this.groupBox6.Controls.Add(this.txtAnimation);
			this.groupBox6.Controls.Add(this.radioButton2);
			this.groupBox6.Controls.Add(this.radioButton1);
			this.groupBox6.Location = new System.Drawing.Point(256, 184);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(226, 104);
			this.groupBox6.TabIndex = 9;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Animation";
			// 
			// btnToastWithAnimation
			// 
			this.btnToastWithAnimation.Location = new System.Drawing.Point(6, 68);
			this.btnToastWithAnimation.Name = "btnToastWithAnimation";
			this.btnToastWithAnimation.Size = new System.Drawing.Size(213, 27);
			this.btnToastWithAnimation.TabIndex = 4;
			this.btnToastWithAnimation.Text = "Display Toast with custom animation";
			this.btnToastWithAnimation.UseVisualStyleBackColor = true;
			// 
			// txtAnimation
			// 
			this.txtAnimation.Location = new System.Drawing.Point(6, 42);
			this.txtAnimation.MaxLength = 64;
			this.txtAnimation.Name = "txtAnimation";
			this.txtAnimation.Size = new System.Drawing.Size(213, 20);
			this.txtAnimation.TabIndex = 3;
			this.txtAnimation.Text = "Hello, I am Toast!";
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(107, 19);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(48, 17);
			this.radioButton2.TabIndex = 1;
			this.radioButton2.Text = "Slide";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Checked = true;
			this.radioButton1.Location = new System.Drawing.Point(9, 19);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(92, 17);
			this.radioButton1.TabIndex = 0;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Fade (Default)";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// groupBox7
			// 
			this.groupBox7.Location = new System.Drawing.Point(487, 27);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(402, 384);
			this.groupBox7.TabIndex = 10;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Toast Builder";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(901, 423);
			this.Controls.Add(this.groupBox7);
			this.Controls.Add(this.groupBox6);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Demo display Toast Notification";
			this.Click += new System.EventHandler(this.Form1_Click);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numofToasts)).EndInit();
			this.groupBox5.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnShowToastDemo;
		private System.Windows.Forms.Button btnSimpeWithCustomText;
		private System.Windows.Forms.TextBox txtText;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnInsertImage;
		private System.Windows.Forms.PictureBox picThumbnail;
		private System.Windows.Forms.TextBox txttextImage;
		private System.Windows.Forms.Button btnToastTextImage;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button btnDisplayMultiple;
		private System.Windows.Forms.NumericUpDown numofToasts;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button btnToastWithAnimation;
		private System.Windows.Forms.TextBox txtAnimation;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Timer timer2;
	}
}

