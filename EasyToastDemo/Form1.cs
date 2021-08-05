using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.UI.Widget;
using System.Enums;

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
			numofToasts.Maximum = (decimal)(ToastManager.MAX_TOASTS_ALLOWED/2.0);
			ToastManager.ToastCollection.ToastRemoved += ToastCollection_ToastRemoved;
			ToastManager.ToastCollection.ToastAdded += ToastCollection_ToastAdded;
			ToastManager.ToastCollection.CollectionTruncated += ToastCollection_CollectionTruncated;
			
		}

		private void ToastCollection_CollectionTruncated(object sender, EventArgs e)
		{
			rchTextWatch.AppendText("Toast collection is empty\r\n");
			rchTextWatch.ScrollToCaret();
		}

		private void ToastCollection_ToastAdded(object sender, ToastChangedEventArgs e)
		{
			rchTextWatch.AppendText($"Toast with ID {e.Toast.Guid} has been displayed\r\n");
			rchTextWatch.ScrollToCaret();
		}

		private void ToastCollection_ToastRemoved(object sender, ToastChangedEventArgs e)
		{
			rchTextWatch.AppendText($"Toast with ID {e.Toast.Guid} has been destroyed\r\n");
			rchTextWatch.ScrollToCaret();
		}

		private async void BtnShowToastDemo_Click(object sender, EventArgs e)
		{
			if (!_isDisplayedSimple1)
			{
				Toast.Build(this, "Hello, I am Toast!", "").Show();
				_isDisplayedSimple1 = true;
				await Task.Delay(1000);
				_isDisplayedSimple1 = false;
			}
			
		}

		private void BtnSimpeWithCustomText_Click(object sender, EventArgs e)
		{
			if (!_isDisplayedSimple2)
			{
				Toast.Build(this, string.IsNullOrEmpty(txtText.Text) ? "Hello, I am Toast!" : txtText.Text).Show();
				_isDisplayedSimple2 = !_isDisplayedSimple2;
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
						picThumbnail.Image = null;
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
				return image.Width >=80 || image.Height >= 80;
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

			var toast = Toast.Build(this, txttextImage.Text, picThumbnail.Image);
			toast.OnClick += Toast_OnClick;
			toast.Show();
		}

		private void Toast_OnClick(object sender, EventArgs e)
		{
			rchTextWatch.AppendText("Toast clicked\r\n");
		}

		private void Form1_Click(object sender, EventArgs e)
		{
			
		}

		private void BtnDisplayMultiple_Click(object sender, EventArgs e)
		{
			for (var i = 1; i <= (int)numofToasts.Value; i++)
			{
				Toast.Build(this, $"This is Toast {i}").Show();
			}
		}

		private void BtnToastWithAnimation_Click(object sender, EventArgs e)
		{
			Toast.Build(this, string.IsNullOrEmpty(txtAnimation.Text) ? "Hello, I am Toast!" : txtAnimation.Text, rFade.Checked ? Animation.FADE : Animation.SLIDE).Show();
		}

		private void BtnTopRight_Click(object sender, EventArgs e)
		{
			
		}

		private void BtnBottom_Click(object sender, EventArgs e)
		{

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			cbBuiltinThemes.DataSource = Enum.GetValues(typeof(Theme));
		}

		private void CreateWithBuilder()
		{
			var toast = new ToastBuilder(this)
				.SetCaption("Hello! I am Toast")
				.SetDescription("This is demo")
				.SetDuration(Duration.LENGTH_SHORT)
				.SetMuting(false)
				.Build();

			toast.Show();
		}
	}
}
