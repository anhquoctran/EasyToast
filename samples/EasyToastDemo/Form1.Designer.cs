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
		/// Required method for Designer support — chrome only.
		/// Catalog controls are created in <see cref="Form1"/> so every public API has a live button.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			SuspendLayout();
			//
			// Form1
			//
			AutoScaleMode = AutoScaleMode.None;
			ClientSize = new Size(1080, 720);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = true;
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "FuzzyToast Demo v3";
			Load += Form1_Load;
			ResumeLayout(false);
		}

		#endregion
	}
}
