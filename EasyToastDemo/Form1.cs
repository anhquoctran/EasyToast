using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.UI.Widget;

namespace EasyToastDemo
{
	public partial class Form1 : Form
	{
		private bool _isDisplayedSimple1;
		private bool _isDisplayedSimple2;

		public Form1()
		{
			InitializeComponent();
			BringToFront();
			
			numofToasts.Maximum = ToastManager.MAX_TOASTS_ALLOWED;
		}

		private async void BtnShowToastDemo_Click(object sender, EventArgs e)
		{
			if (!_isDisplayedSimple1)
			{
				Toast.Build(this, "Hello, I am Toast!", Duration.LENGTH_SHORT, Animation.FADE).Show();
				_isDisplayedSimple1 = true;
				await Task.Delay(1000);
				_isDisplayedSimple1 = false;
			}
			
		}

		private void btnSimpeWithCustomText_Click(object sender, EventArgs e)
		{
			if (!_isDisplayedSimple2)
			{
				Toast.Build(this, string.IsNullOrEmpty(txtText.Text) ? "Hello, I am Toast!" : txtText.Text).Show();
				_isDisplayedSimple2 = true;
			}
			
		}

		private void BtnInsertImage_Click(object sender, EventArgs e)
		{
			var opDlg = new OpenFileDialog
			{
				FileName = string.Empty,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
				CheckFileExists = true,
				RestoreDirectory = true,
				Multiselect = false,
				Filter = "JPEG Image or PNG image|*.jpg;*.jpeg;*.png"
			};

			if (opDlg.ShowDialog() == DialogResult.OK)
			{
				if (ValidateImage(opDlg.FileName))
				{
					if (ValidateImageSize(Image.FromFile(opDlg.FileName)))
					{
						picThumbnail.Image = Image.FromFile(opDlg.FileName);
					}
					else
					{
						MessageBox.Show(
							"An image that you provided is not valid required size for Toast popup component! Please using image with MINIMUM SIZE 64x64 pixels!", "Invalid Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else
				{
					MessageBox.Show("File is not an image file");
				}
				
			}
		}

		private static bool ValidateImageSize(Image image)
		{
			if (image == null) return false;
			try
			{
				if (image.Width >= 64 && image.Height >= 64)
					return true;
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static bool ValidateImage(string path)
		{
			if (string.IsNullOrEmpty(path)) return false;
			if (!File.Exists(path)) return false;
			var bytes = File.ReadAllBytes(path);
			var png = new byte[] { 137, 80, 78, 71 };
			var jpeg = new byte[] { 255, 216, 255, 224 };

			return png.SequenceEqual(bytes.Take(png.Length)) || jpeg.SequenceEqual(bytes.Take(jpeg.Length));
		}

		private void BtnToastTextImage_Click(object sender, EventArgs e)
		{
			if (picThumbnail.Image == null)
			{
				MessageBox.Show("Please choose an image", "Image required");
				return;
			}

			if (string.IsNullOrEmpty(txttextImage.Text))
			{
				txttextImage.Text = "Hello! I'm Toast! :)";
			}

			Toast.Build(this, txttextImage.Text, picThumbnail.Image).Show();
		}

		private void Form1_Click(object sender, EventArgs e)
		{
			
		}

		private void btnDisplayMultiple_Click(object sender, EventArgs e)
		{
			for (var i = 1; i <= (int)numofToasts.Value; i++)
			{
				Toast.Build(this, $"This is Toast {i}", Duration.LENGTH_LONG, Animation.SLIDE).Show();
			}
		}
	}
}
