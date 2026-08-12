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
			components = new System.ComponentModel.Container();
			btnShowToastDemo = new Button();
			btnSimpeWithCustomText = new Button();
			txtText = new TextBox();
			groupBox1 = new GroupBox();
			groupBox2 = new GroupBox();
			groupBox3 = new GroupBox();
			label1 = new Label();
			btnInsertImage = new Button();
			picThumbnail = new PictureBox();
			txttextImage = new TextBox();
			btnToastTextImage = new Button();
			groupBox4 = new GroupBox();
			label3 = new Label();
			btnDisplayMultiple = new Button();
			numofToasts = new NumericUpDown();
			label2 = new Label();
			groupBox5 = new GroupBox();
			btnBottom = new Button();
			btnTopRight = new Button();
			menuStrip1 = new MenuStrip();
			aboutToolStripMenuItem = new ToolStripMenuItem();
			aboutToolStripMenuItem1 = new ToolStripMenuItem();
			groupBox6 = new GroupBox();
			btnToastWithAnimation = new Button();
			txtAnimation = new TextBox();
			rSlide = new RadioButton();
			rFade = new RadioButton();
			groupBox7 = new GroupBox();
			rchTextWatch = new RichTextBox();
			timer1 = new System.Windows.Forms.Timer(components);
			timer2 = new System.Windows.Forms.Timer(components);
			groupBox8 = new GroupBox();
			btnCustomDuration = new Button();
			textBox1 = new TextBox();
			radioButton1 = new RadioButton();
			radioButton2 = new RadioButton();
			groupBox9 = new GroupBox();
			cbBuiltinThemes = new ComboBox();
			label4 = new Label();
			groupBox10 = new GroupBox();
			comboBox1 = new ComboBox();
			label5 = new Label();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
			groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picThumbnail).BeginInit();
			groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numofToasts).BeginInit();
			groupBox5.SuspendLayout();
			menuStrip1.SuspendLayout();
			groupBox6.SuspendLayout();
			groupBox7.SuspendLayout();
			groupBox8.SuspendLayout();
			groupBox9.SuspendLayout();
			groupBox10.SuspendLayout();
			SuspendLayout();
			// 
			// btnShowToastDemo
			// 
			btnShowToastDemo.Location = new Point(6, 19);
			btnShowToastDemo.Name = "btnShowToastDemo";
			btnShowToastDemo.Size = new Size(226, 30);
			btnShowToastDemo.TabIndex = 0;
			btnShowToastDemo.Text = "Show a simple toast";
			btnShowToastDemo.UseVisualStyleBackColor = true;
			btnShowToastDemo.Click += BtnShowToastDemo_Click;
			// 
			// btnSimpeWithCustomText
			// 
			btnSimpeWithCustomText.Location = new Point(6, 45);
			btnSimpeWithCustomText.Name = "btnSimpeWithCustomText";
			btnSimpeWithCustomText.Size = new Size(226, 30);
			btnSimpeWithCustomText.TabIndex = 1;
			btnSimpeWithCustomText.Text = "Show a simple toast with custom text";
			btnSimpeWithCustomText.UseVisualStyleBackColor = true;
			btnSimpeWithCustomText.Click += BtnSimpeWithCustomText_Click;
			// 
			// txtText
			// 
			txtText.Location = new Point(6, 19);
			txtText.MaxLength = 512;
			txtText.Name = "txtText";
			txtText.Size = new Size(226, 23);
			txtText.TabIndex = 2;
			txtText.Text = "Hello, I am Toast!";
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(btnShowToastDemo);
			groupBox1.Location = new Point(12, 27);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(238, 59);
			groupBox1.TabIndex = 3;
			groupBox1.TabStop = false;
			groupBox1.Text = "Simplest Toast";
			// 
			// groupBox2
			// 
			groupBox2.Controls.Add(txtText);
			groupBox2.Controls.Add(btnSimpeWithCustomText);
			groupBox2.Location = new Point(12, 92);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new Size(238, 86);
			groupBox2.TabIndex = 4;
			groupBox2.TabStop = false;
			groupBox2.Text = "Simple with caption";
			// 
			// groupBox3
			// 
			groupBox3.Controls.Add(label1);
			groupBox3.Controls.Add(btnInsertImage);
			groupBox3.Controls.Add(picThumbnail);
			groupBox3.Controls.Add(txttextImage);
			groupBox3.Controls.Add(btnToastTextImage);
			groupBox3.Location = new Point(12, 184);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new Size(238, 221);
			groupBox3.TabIndex = 5;
			groupBox3.TabStop = false;
			groupBox3.Text = "Text and thumbnail";
			// 
			// label1
			// 
			label1.AutoEllipsis = true;
			label1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 163);
			label1.ForeColor = Color.DimGray;
			label1.Location = new Point(6, 112);
			label1.Name = "label1";
			label1.Size = new Size(226, 59);
			label1.TabIndex = 5;
			label1.Text = "Required minimum size of thumbnail is 64x64. Recommended size is 80x80. Square ratio for best display. JPEG and PNG format supported.";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// btnInsertImage
			// 
			btnInsertImage.Location = new Point(76, 45);
			btnInsertImage.Name = "btnInsertImage";
			btnInsertImage.Size = new Size(94, 30);
			btnInsertImage.TabIndex = 4;
			btnInsertImage.Text = "Choose image";
			btnInsertImage.UseVisualStyleBackColor = true;
			btnInsertImage.Click += BtnInsertImage_Click;
			// 
			// picThumbnail
			// 
			picThumbnail.BorderStyle = BorderStyle.FixedSingle;
			picThumbnail.Location = new Point(6, 45);
			picThumbnail.Name = "picThumbnail";
			picThumbnail.Size = new Size(64, 64);
			picThumbnail.SizeMode = PictureBoxSizeMode.StretchImage;
			picThumbnail.TabIndex = 3;
			picThumbnail.TabStop = false;
			// 
			// txttextImage
			// 
			txttextImage.Location = new Point(6, 19);
			txttextImage.MaxLength = 512;
			txttextImage.Name = "txttextImage";
			txttextImage.Size = new Size(226, 23);
			txttextImage.TabIndex = 2;
			txttextImage.Text = "Hello, I am Toast!";
			// 
			// btnToastTextImage
			// 
			btnToastTextImage.Location = new Point(6, 185);
			btnToastTextImage.Name = "btnToastTextImage";
			btnToastTextImage.Size = new Size(226, 30);
			btnToastTextImage.TabIndex = 1;
			btnToastTextImage.Text = "Show toast with text and thumbnail image";
			btnToastTextImage.UseVisualStyleBackColor = true;
			btnToastTextImage.Click += BtnToastTextImage_Click;
			// 
			// groupBox4
			// 
			groupBox4.Controls.Add(label3);
			groupBox4.Controls.Add(btnDisplayMultiple);
			groupBox4.Controls.Add(numofToasts);
			groupBox4.Controls.Add(label2);
			groupBox4.Location = new Point(256, 27);
			groupBox4.Name = "groupBox4";
			groupBox4.Size = new Size(249, 94);
			groupBox4.TabIndex = 6;
			groupBox4.TabStop = false;
			groupBox4.Text = "Multiple toasts";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(6, 44);
			label3.Name = "label3";
			label3.Size = new Size(93, 15);
			label3.TabIndex = 3;
			label3.Text = "Max allowed is 3";
			// 
			// btnDisplayMultiple
			// 
			btnDisplayMultiple.Location = new Point(6, 62);
			btnDisplayMultiple.Name = "btnDisplayMultiple";
			btnDisplayMultiple.Size = new Size(237, 27);
			btnDisplayMultiple.TabIndex = 2;
			btnDisplayMultiple.Text = "Display random multiple Toast";
			btnDisplayMultiple.UseVisualStyleBackColor = true;
			btnDisplayMultiple.Click += BtnDisplayMultiple_Click;
			// 
			// numofToasts
			// 
			numofToasts.Location = new Point(172, 19);
			numofToasts.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
			numofToasts.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numofToasts.Name = "numofToasts";
			numofToasts.Size = new Size(71, 23);
			numofToasts.TabIndex = 1;
			numofToasts.TextAlign = HorizontalAlignment.Center;
			numofToasts.Value = new decimal(new int[] { 2, 0, 0, 0 });
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(6, 21);
			label2.Name = "label2";
			label2.Size = new Size(153, 15);
			label2.TabIndex = 0;
			label2.Text = "Number of Toast to display:";
			// 
			// groupBox5
			// 
			groupBox5.Controls.Add(btnBottom);
			groupBox5.Controls.Add(btnTopRight);
			groupBox5.Location = new Point(256, 127);
			groupBox5.Name = "groupBox5";
			groupBox5.Size = new Size(249, 51);
			groupBox5.TabIndex = 7;
			groupBox5.TabStop = false;
			groupBox5.Text = "Position";
			// 
			// btnBottom
			// 
			btnBottom.Location = new Point(87, 19);
			btnBottom.Name = "btnBottom";
			btnBottom.Size = new Size(75, 23);
			btnBottom.TabIndex = 3;
			btnBottom.Text = "Bottom Right";
			btnBottom.UseVisualStyleBackColor = true;
			btnBottom.Click += BtnBottom_Click;
			// 
			// btnTopRight
			// 
			btnTopRight.Location = new Point(6, 19);
			btnTopRight.Name = "btnTopRight";
			btnTopRight.Size = new Size(75, 23);
			btnTopRight.TabIndex = 2;
			btnTopRight.Text = "Top Right";
			btnTopRight.UseVisualStyleBackColor = true;
			btnTopRight.Click += BtnTopRight_Click;
			// 
			// menuStrip1
			// 
			menuStrip1.BackColor = SystemColors.Control;
			menuStrip1.Items.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
			menuStrip1.Location = new Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new Size(901, 24);
			menuStrip1.TabIndex = 8;
			menuStrip1.Text = "menuStrip1";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem1 });
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new Size(44, 20);
			aboutToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem1
			// 
			aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
			aboutToolStripMenuItem1.Size = new Size(107, 22);
			aboutToolStripMenuItem1.Text = "About";
			aboutToolStripMenuItem1.Click += About_Click;
			// 
			// groupBox6
			// 
			groupBox6.Controls.Add(btnToastWithAnimation);
			groupBox6.Controls.Add(txtAnimation);
			groupBox6.Controls.Add(rSlide);
			groupBox6.Controls.Add(rFade);
			groupBox6.Location = new Point(256, 184);
			groupBox6.Name = "groupBox6";
			groupBox6.Size = new Size(249, 104);
			groupBox6.TabIndex = 9;
			groupBox6.TabStop = false;
			groupBox6.Text = "Animation";
			// 
			// btnToastWithAnimation
			// 
			btnToastWithAnimation.Location = new Point(6, 68);
			btnToastWithAnimation.Name = "btnToastWithAnimation";
			btnToastWithAnimation.Size = new Size(237, 27);
			btnToastWithAnimation.TabIndex = 4;
			btnToastWithAnimation.Text = "Display Toast with custom animation";
			btnToastWithAnimation.UseVisualStyleBackColor = true;
			btnToastWithAnimation.Click += BtnToastWithAnimation_Click;
			// 
			// txtAnimation
			// 
			txtAnimation.Location = new Point(6, 42);
			txtAnimation.MaxLength = 512;
			txtAnimation.Name = "txtAnimation";
			txtAnimation.Size = new Size(237, 23);
			txtAnimation.TabIndex = 3;
			txtAnimation.Text = "Hello, I am Toast!";
			// 
			// rSlide
			// 
			rSlide.AutoSize = true;
			rSlide.Location = new Point(107, 19);
			rSlide.Name = "rSlide";
			rSlide.Size = new Size(50, 19);
			rSlide.TabIndex = 1;
			rSlide.Text = "Slide";
			rSlide.UseVisualStyleBackColor = true;
			// 
			// rFade
			// 
			rFade.AutoSize = true;
			rFade.Checked = true;
			rFade.Location = new Point(9, 19);
			rFade.Name = "rFade";
			rFade.Size = new Size(99, 19);
			rFade.TabIndex = 0;
			rFade.TabStop = true;
			rFade.Text = "Fade (Default)";
			rFade.UseVisualStyleBackColor = true;
			// 
			// groupBox7
			// 
			groupBox7.Controls.Add(rchTextWatch);
			groupBox7.Location = new Point(12, 411);
			groupBox7.Name = "groupBox7";
			groupBox7.Size = new Size(877, 148);
			groupBox7.TabIndex = 10;
			groupBox7.TabStop = false;
			groupBox7.Text = "Toast Collection Live Watch";
			// 
			// rchTextWatch
			// 
			rchTextWatch.BackColor = Color.White;
			rchTextWatch.Dock = DockStyle.Fill;
			rchTextWatch.Location = new Point(3, 19);
			rchTextWatch.Name = "rchTextWatch";
			rchTextWatch.ReadOnly = true;
			rchTextWatch.Size = new Size(871, 126);
			rchTextWatch.TabIndex = 0;
			rchTextWatch.Text = "";
			// 
			// groupBox8
			// 
			groupBox8.Controls.Add(btnCustomDuration);
			groupBox8.Controls.Add(textBox1);
			groupBox8.Controls.Add(radioButton1);
			groupBox8.Controls.Add(radioButton2);
			groupBox8.Location = new Point(256, 294);
			groupBox8.Name = "groupBox8";
			groupBox8.Size = new Size(249, 111);
			groupBox8.TabIndex = 11;
			groupBox8.TabStop = false;
			groupBox8.Text = "Duration";
			// 
			// btnCustomDuration
			// 
			btnCustomDuration.Location = new Point(6, 75);
			btnCustomDuration.Name = "btnCustomDuration";
			btnCustomDuration.Size = new Size(237, 30);
			btnCustomDuration.TabIndex = 4;
			btnCustomDuration.Text = "Display Toast with custom duration";
			btnCustomDuration.UseVisualStyleBackColor = true;
			btnCustomDuration.Click += BtnCustomDuration_Click;
			// 
			// textBox1
			// 
			textBox1.Location = new Point(6, 42);
			textBox1.MaxLength = 512;
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(237, 23);
			textBox1.TabIndex = 3;
			textBox1.Text = "Hello, I am Toast!";
			// 
			// radioButton1
			// 
			radioButton1.AutoSize = true;
			radioButton1.Location = new Point(64, 19);
			radioButton1.Name = "radioButton1";
			radioButton1.Size = new Size(53, 19);
			radioButton1.TabIndex = 1;
			radioButton1.Text = "Short";
			radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			radioButton2.AutoSize = true;
			radioButton2.Checked = true;
			radioButton2.Location = new Point(9, 19);
			radioButton2.Name = "radioButton2";
			radioButton2.Size = new Size(52, 19);
			radioButton2.TabIndex = 0;
			radioButton2.TabStop = true;
			radioButton2.Text = "Long";
			radioButton2.UseVisualStyleBackColor = true;
			// 
			// groupBox9
			// 
			groupBox9.Controls.Add(cbBuiltinThemes);
			groupBox9.Controls.Add(label4);
			groupBox9.Location = new Point(511, 27);
			groupBox9.Name = "groupBox9";
			groupBox9.Size = new Size(375, 151);
			groupBox9.TabIndex = 12;
			groupBox9.TabStop = false;
			groupBox9.Text = "Theme";
			// 
			// cbBuiltinThemes
			// 
			cbBuiltinThemes.DropDownStyle = ComboBoxStyle.DropDownList;
			cbBuiltinThemes.FormattingEnabled = true;
			cbBuiltinThemes.Location = new Point(98, 19);
			cbBuiltinThemes.Name = "cbBuiltinThemes";
			cbBuiltinThemes.Size = new Size(144, 23);
			cbBuiltinThemes.TabIndex = 1;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(6, 21);
			label4.Name = "label4";
			label4.Size = new Size(86, 15);
			label4.TabIndex = 0;
			label4.Text = "Built-in theme:";
			// 
			// groupBox10
			// 
			groupBox10.Controls.Add(comboBox1);
			groupBox10.Controls.Add(label5);
			groupBox10.Location = new Point(511, 184);
			groupBox10.Name = "groupBox10";
			groupBox10.Size = new Size(376, 109);
			groupBox10.TabIndex = 13;
			groupBox10.TabStop = false;
			groupBox10.Text = "Close style";
			// 
			// comboBox1
			// 
			comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox1.FormattingEnabled = true;
			comboBox1.Location = new Point(98, 18);
			comboBox1.Name = "comboBox1";
			comboBox1.Size = new Size(144, 23);
			comboBox1.TabIndex = 1;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(6, 21);
			label5.Name = "label5";
			label5.Size = new Size(66, 15);
			label5.TabIndex = 0;
			label5.Text = "Close style:";
			// 
			// Form1
			// 
			AutoScaleMode = AutoScaleMode.None;
			ClientSize = new Size(901, 571);
			Controls.Add(groupBox10);
			Controls.Add(groupBox9);
			Controls.Add(groupBox8);
			Controls.Add(groupBox7);
			Controls.Add(groupBox6);
			Controls.Add(groupBox5);
			Controls.Add(groupBox4);
			Controls.Add(groupBox3);
			Controls.Add(groupBox2);
			Controls.Add(groupBox1);
			Controls.Add(menuStrip1);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MainMenuStrip = menuStrip1;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "FuzzyToast Demo v3";
			Load += Form1_Load;
			Click += Form1_Click;
			groupBox1.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			groupBox3.ResumeLayout(false);
			groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picThumbnail).EndInit();
			groupBox4.ResumeLayout(false);
			groupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numofToasts).EndInit();
			groupBox5.ResumeLayout(false);
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			groupBox6.ResumeLayout(false);
			groupBox6.PerformLayout();
			groupBox7.ResumeLayout(false);
			groupBox8.ResumeLayout(false);
			groupBox8.PerformLayout();
			groupBox9.ResumeLayout(false);
			groupBox9.PerformLayout();
			groupBox10.ResumeLayout(false);
			groupBox10.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

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
		private System.Windows.Forms.Button btnBottom;
		private System.Windows.Forms.Button btnTopRight;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button btnToastWithAnimation;
		private System.Windows.Forms.TextBox txtAnimation;
		private System.Windows.Forms.RadioButton rSlide;
		private System.Windows.Forms.RadioButton rFade;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Timer timer2;
		private System.Windows.Forms.RichTextBox rchTextWatch;
		private System.Windows.Forms.GroupBox groupBox8;
		private System.Windows.Forms.Button btnCustomDuration;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.ComboBox cbBuiltinThemes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label5;
    }
}

