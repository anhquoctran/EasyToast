using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Properties;

namespace System.UI.Widget
{
	internal partial class FrmToast : Form
	{
		private bool _hasImageSet;

		private readonly int _horizontalMargin;

		private readonly int _verticalMargin;

		private bool _shown;

		private bool _isMuted = false;

		private Animation _animation;

		private const int AW_SLIDE = 0X40000;

		private const int AW_HOR_POSITIVE = 0X1;

		private const int AW_HOR_NEGATIVE = 0X2;

		private const int AW_HIDE = 0x00010000;

		private const int AW_ACTIVATE = 0x00020000;

		private const int AW_BLEND = 0X80000;

		private const int AW_CENTER = 0x00000010;

		private byte _counter = 2;

		private Duration _duration;

		private Toast _toast;

		public bool IsAsync = false;

		public Theme Theme;

		public CloseStye CloseStyle;

		[DllImport("user32")]
		private static extern bool AnimateWindow(IntPtr hwnd, int time, int flags);

		internal FrmToast()
		{
			InitializeComponent();
			_horizontalMargin = 10;
			_verticalMargin = 10;
			
			var workingArea = Screen.GetWorkingArea(this);

            Location = new Point(workingArea.Right - Size.Width - _horizontalMargin,
				workingArea.Bottom - Size.Height - _verticalMargin);
        }

		internal Toast Toast
		{
			get => _toast;
			set => _toast = value;
		}

		internal bool IsShown => _shown;

		[DefaultValue(Duration.LENGTH_SHORT)]
		internal Duration Duration
		{
			get => _duration;
			set => _duration = value;
		}

		[DefaultValue(Animation.FADE)]
		internal Animation Animation
		{
			get => _animation;
			set => _animation = value;
		}

		[DefaultValue("")]
		internal string Caption
		{
			get => lblCaption.Text;
			set => lblCaption.Text = value;
		}

		internal Image Thumbnails
		{
			get => picImage.Image;
			set
			{
				picImage.Image = value;
				Invalidate();
				if (value != null)
				{
					_hasImageSet = true;
				}
				
			} 
		}

		[DefaultValue(false)]
		internal bool IsMuted
		{
			get => _isMuted;
			set => _isMuted = value;
		}

		internal int HorizontalMargin => _horizontalMargin;
		internal int VerticalMargin => _verticalMargin;

		private async void FrmToast_Load(object sender, EventArgs e)
		{
			if (IsAsync)
			{
				await TaskEx.Yield();
			}

			switch (CloseStyle)
			{
				case CloseStye.ClickEntire:
					textContainer.Panel1Collapsed = true;
					break;
				case CloseStye.Button:
					textContainer.Panel1Collapsed = false;
					break;
				case CloseStye.ButtonAndClickEntire:
					textContainer.Panel1Collapsed = false;
					break;
			}

			switch (_duration)
			{
				case Duration.LENGTH_SHORT:
					_counter = 2;
					break;
				case Duration.LENGTH_LONG:
					_counter = 3;
					break;
			}

			if (!_hasImageSet)
			{
				picImage.Image = Properties.Resources.info_icon;
			}
			
			if (!_isMuted)
			{
				//PlaySound();
			}
			SetTheme();
			switch (_animation)
			{
				case Animation.FADE:
					FadeIn();
					break;
				case Animation.SLIDE:
					AnimateWindow(Handle, 250, AW_SLIDE | AW_HOR_NEGATIVE | AW_ACTIVATE);
					break;
			}
			
		}

		private async void PlaySound()
		{
			await Task.Factory.StartNew(() =>
			{

				Stream sound = Properties.Resources.notificationSound;
				var player = new SoundPlayer(sound);
				player.Play();
			});
		}

		private void SetTheme()
		{
			switch (Theme)
			{
				case Theme.Dark:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.DarkScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.DarkScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.DarkScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.DarkScheme.GetBackgroundColor();
					break;
				case Theme.Light:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.LightScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.LightScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.LightScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.LightScheme.GetBackgroundColor();
					break;
				case Theme.PrimaryLight:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.PrimaryLightScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.PrimaryLightScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.PrimaryLightScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.PrimaryLightScheme.GetBackgroundColor();
					break;
				case Theme.SuccessLight:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.SuccessLightScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.SuccessLightScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.SuccessLightScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.SuccessLightScheme.GetBackgroundColor();
					break;
				case Theme.WarningLight:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.WarningLightScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.WarningLightScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.WarningLightScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.WarningLightScheme.GetBackgroundColor();
					break;
				case Theme.ErrorLight:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.ErrorLightScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.ErrorLightScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.ErrorLightScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.ErrorLightScheme.GetBackgroundColor();
					break;
				case Theme.PrimaryDark:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.PrimaryDarkScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.PrimaryDarkScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.PrimaryDarkScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.PrimaryDarkScheme.GetBackgroundColor();
					break;
				case Theme.SuccessDark:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.SuccessDarkScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.SuccessDarkScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.SuccessDarkScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.SuccessDarkScheme.GetBackgroundColor();
					break;
				case Theme.WarningDark:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.WarningDarkScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.WarningDarkScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.WarningDarkScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.WarningDarkScheme.GetBackgroundColor();
					break;
				case Theme.ErrorDark:
					lblCaption.ForeColor = ThemeBuilder.BuiltinScheme.ErrorDarkScheme.GetForegroundColor();
					btnClose.ForeColor = ThemeBuilder.BuiltinScheme.ErrorDarkScheme.GetForegroundColor();
					BackColor = ThemeBuilder.BuiltinScheme.ErrorDarkScheme.GetBackgroundColor();
					btnClose.FlatAppearance.BorderColor = ThemeBuilder.BuiltinScheme.ErrorDarkScheme.GetBackgroundColor();
					break;
				case Theme.Custom:
					if (ThemeBuilder.CustomScheme == null)
					{
						throw new NullReferenceException($"You must create your scheme before set custom theme. Use ${nameof(ThemeBuilder.CreateCustomScheme)}() to create a custom scheme");
					}
					else
					{
						lblCaption.ForeColor = ThemeBuilder.CustomScheme.GetForegroundColor();
						btnClose.ForeColor = ThemeBuilder.CustomScheme.GetForegroundColor();
						BackColor = ThemeBuilder.CustomScheme.GetBackgroundColor();
						btnClose.FlatAppearance.BorderColor = ThemeBuilder.CustomScheme.GetBackgroundColor();
					}
					break;
			}
		}

		private void FrmToast_Shown(object sender, EventArgs e)
		{
			
			_shown = true;
            tmrClose.Start();
		}

		private void FrmToast_FormClosing(object sender, FormClosingEventArgs e)
		{
			switch (_animation)
			{
				case Animation.FADE:
					AnimateWindow(Handle, 250, AW_BLEND | AW_HIDE);
                    break;
				case Animation.SLIDE:
					AnimateWindow(Handle, 250, AW_SLIDE | AW_HOR_POSITIVE | AW_HIDE);
                    break;
			}
		}

		private void FrmToast_Click(object sender, EventArgs e)
		{
			switch (CloseStyle)
			{
				case CloseStye.ClickEntire:
				
				case CloseStye.ButtonAndClickEntire:
					Close();
					break;
				case CloseStye.Button:
					return;
			}
			
		}

		private void LblCaption_Click(object sender, EventArgs e)
		{
			switch (CloseStyle)
			{
				case CloseStye.ClickEntire:

				case CloseStye.ButtonAndClickEntire:
					Close();
					break;
				case CloseStye.Button:
					return;
			}
		}

		private void MainContainer_Panel2_Click(object sender, EventArgs e)
		{
			switch (CloseStyle)
			{
				case CloseStye.ClickEntire:

				case CloseStye.ButtonAndClickEntire:
					Close();
					break;
				case CloseStye.Button:
					return;
			}
		}

		private void MainContainer_Panel1_Click(object sender, EventArgs e)
		{
			switch (CloseStyle)
			{
				case CloseStye.ClickEntire:

				case CloseStye.ButtonAndClickEntire:
					Close();
					break;
				case CloseStye.Button:
					return;
			}
		}

		private void PicImage_Click(object sender, EventArgs e)
		{
			switch (CloseStyle)
			{
				case CloseStye.ClickEntire:

				case CloseStye.ButtonAndClickEntire:
					Close();
					break;
				case CloseStye.Button:
					return;
			}
		}

		private void TmrClose_Tick(object sender, EventArgs e)
		{
			_counter--;
			if (_counter != 0) return;
			tmrClose.Stop();
			Close();
		}

		private async void FadeIn()
		{
			Opacity = 0;
			while (Opacity < 1.0)
			{
				await TaskEx.Delay(3);
				Opacity += 0.05;
			}
			Opacity = 1;
		}

		private SizeF CalculateString()
		{
			if (string.IsNullOrEmpty(lblCaption.Text)) return SizeF.Empty;
			using (var g = CreateGraphics())
			{
				var size = g.MeasureString(lblCaption.Text, lblCaption.Font);
				return size;
			}
		}

		private void FrmToast_FormClosed(object sender, FormClosedEventArgs e)
		{
			ToastManager.ToastCollection.Remove(_toast);
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
