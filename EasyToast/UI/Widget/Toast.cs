using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace System.UI.Widget
{
	public class Toast
	{
		#region Private fields

		private static IWin32Window _window;
		internal readonly FrmToast FrmToast;

		#endregion

		#region properties

		internal static IWin32Window Window
		{
			get => _window;
			set => _window = value;
		}

		/// <summary>
		/// Gets unique ID of Toast
		/// </summary>
		public string Guid { get; }

		/// <summary>
		/// Gets or sets text of Toast
		/// </summary>
		[DefaultValue("")]
		internal string Text { get; set; } = string.Empty;

		[DefaultValue(Duration.LENGTH_SHORT)]
		internal Duration Duration { get; set; } = Duration.LENGTH_SHORT;

		[DefaultValue(false)]
		internal bool IsMuted { get; set; }

		[DefaultValue(Animation.FADE)]
		internal Animation Animation { get; set; } = Animation.FADE;

		internal Image Thumbnail { get; set; }

		[DefaultValue(Position.BottomRight)]
		internal Position Position { get; set; } = Position.BottomRight;

		[DefaultValue(Theme.Dark)]
		internal Theme ThemeStyle { get; set; } = Theme.Dark;

		[DefaultValue(CloseStye.ButtonAndClickEntire)]
		internal CloseStye CloseStyle { get; set; } = CloseStye.ButtonAndClickEntire;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct an empty Toast object. You must sets View before you can call Show().
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		internal Toast(IWin32Window window)
		{
			Guid = Utils.GetGuid();
			_window = window;
			FrmToast = new FrmToast();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Display the Toast for the specified configuration.
		/// <exception cref="ArgumentException">Thrown when Text property is null or empty</exception>
		/// </summary>
		public void Show()
		{
			InternalDisplayToast();
		}

		/// <summary>
		/// Display the Toast asynchronously for the specified configuration.
		/// <exception cref="ArgumentException">Thrown when Text property is null or empty</exception>
		/// </summary>
		public void ShowAsync()
		{
			InternalDisplayToast(true);
		}

		/// <summary>
		/// Close the Toast if it's showing, or don't show it if it isn't showing yet. You do not normally have to call this. Normally Toast will disappear on its own after the appropriate duration.
		/// </summary>
		public void Cancel()
		{
			if(FrmToast.IsShown)
				FrmToast.Close();
			else
				throw new InvalidOperationException("You cannot cancel toast displaying when it doesn't display");
		}

		/// <summary>
		/// Get current horizontal margin of toast
		/// </summary>
		/// <returns></returns>
		public int GetHorizontalMargin()
		{
			return FrmToast.HorizontalMargin;
		}

		/// <summary>
		/// Get current vertical margin of toast
		/// </summary>
		/// <returns></returns>
		public int GetVerticalMargin()
		{
			return FrmToast.VerticalMargin;
		}

		#endregion

		#region Private methods

		private void InternalDisplayToast(bool async = false)
		{
			FrmToast.IsAsync = async;
			ToastManager.Toast = this;
			FrmToast.Click += FrmToast_Click;
			FrmToast.MouseHover += FrmToast_MouseHover;
			FrmToast.FormClosed += FrmToast_FormClosed;
			ToastManager.AddToCollection();
		}

		private void FrmToast_FormClosed(object sender, FormClosedEventArgs e)
		{
			OnClosed?.Invoke(this, EventArgs.Empty);
		}

		private void FrmToast_MouseHover(object sender, EventArgs e)
		{
			OnHover?.Invoke(this, e);
		}

		private void FrmToast_Click(object sender, EventArgs e)
		{
			OnClick?.Invoke(this, e);
		}

		#endregion

		#region Public static methods

		/// <summary>
		/// Build a simplest Toast with Text only.
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <returns>Toast has been create but not yet display. Use Show() to display it.</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text)
		{
			var toast = new Toast(window)
			{
				Text = text,
			};
			return toast;
		}

		/// <summary>
		/// Build a Toast with custom duration and animation.
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="duration">How long to display. SHORT is 2 seconds and LONG is 3 seconds.</param>
		/// <param name="animation">Toast transition animation style. Use both fading and sliding animation style.</param>
		/// <returns>Toast has been create but not yet display. Use Show() or ShowAsync() to display it.</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Duration duration, Animation animation)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Duration = duration,
				Animation = animation
			};

			return toast;
		}

		/// <summary>
		/// Build Toast with custom duration length.
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="duration">How long to display. SHORT is 2 seconds and LONG is 3 seconds.</param>
		/// <returns>Toast has been create but not yet display. Use Show() or ShowAsync() to display it.</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Duration duration)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Duration = duration
			};

			return toast;
		}

		/// <summary>
		/// Build a Toast with custom animation, duration, sound
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="duration">How long to display. SHORT is 2 seconds and LONG is 3 seconds.</param>
		/// <param name="animation">Toast transition animation style. Use both fading and sliding animation style.</param>
		/// <param name="muting">Set sound state. Muting or not. Sound using Windows 10 default notification sound</param>
		/// <returns>Toast has been create but not yet display. Use Show() or ShowAsync() to display it.</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Animation animation, Duration duration, bool muting)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Animation = animation,
				Duration = duration,
				IsMuted = muting
			};

			return toast;
		}

		/// <summary>
		/// Build a Toast with custom animation
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="animation">Toast transition animation style. Use both fading and sliding animation style.</param>
		/// <returns>Toast has been create but not yet display. Use Show() or ShowAsync() to display it.</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Animation animation)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Animation = animation
			};

			return toast;
		}

		/// <summary>
		/// Build a simple Toast with specific sound status
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="muting">Set sound state. Muting or not. Sound using Windows 10 default notification sound</param>
		/// <returns>Toast has been create but not yet display. Use Show() or ShowAsync() to display it.</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, bool muting)
		{
			var toast = new Toast(window)
			{
				Text = text,
				IsMuted = muting
			};

			return toast;
		}

		/// <summary>
		/// Build a Toast with text and thumbnails and custom duration, animation
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="thumbnail">Thumbnail image to display in Toast. Required image have MINIMUM SIZE 64x64 pixels for best display</param>
		/// <param name="duration">How long to display. SHORT is 2 seconds and LONG is 3 seconds.</param>
		/// <param name="animation">Toast transition animation style. Use both fading and sliding animation style.</param>
		/// <returns>Toast has been create but not yet display. Use Show() to display it</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Image thumbnail, Duration duration, Animation animation)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Thumbnail = thumbnail,
				Duration = duration,
				Animation = animation
			};
			return toast;
		}

		/// <summary>
		/// Build a simple Toast with thumbnail and custom duration, animation and sound
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="thumbnail">Thumbnail image to display in Toast. Required image have MINIMUM SIZE 64x64 pixels for best display</param>
		/// <param name="duration">How long to display. SHORT is 2 seconds and LONG is 3 seconds.</param>
		/// <param name="animation">Toast transition animation style. Use both fading and sliding animation style.</param>
		/// <param name="muting"></param>
		/// <returns>Toast has been create but not yet display. Use Show() to display it</returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Image thumbnail, Duration duration,
			Animation animation, bool muting)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Thumbnail = thumbnail,
				Duration = duration,
				Animation = animation,
				IsMuted = muting
			};
			return toast;
		}

		/// <summary>
		/// Build a simple Toast with thumbnail
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="thumbnail">Thumbnail image to display in Toast. Required image have MINIMUM SIZE 64x64 pixels for best display</param>
		/// <returns></returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Image thumbnail)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Thumbnail = thumbnail
			};
			return toast;
		}

		/// <summary>
		/// Build a simple Toast with thumbnail and custom duration
		/// </summary>
		/// <param name="window">Containter form. Usually MainForm.</param>
		/// <param name="text">Text to display. Required not null or empty.</param>
		/// <param name="thumbnail">Thumbnail image to display in Toast. Required image have MINIMUM SIZE 64x64 pixels for best display</param>
		/// <param name="duration">How long to display. SHORT is 2 seconds and LONG is 3 seconds.</param>
		/// <returns></returns>
		[MustUseReturnValue("Toast has been create but not yet display. Use Show() to display it.")]
		public static Toast Build(IWin32Window window, string text, Image thumbnail, Duration duration)
		{
			var toast = new Toast(window)
			{
				Text = text,
				Thumbnail = thumbnail,
				Duration = duration
			};
			return toast;
		}

		#endregion

		#region Events

		public delegate void ClickEventHandler(object sender, EventArgs e);

		public event ClickEventHandler OnClick;

		public delegate void HoverEventHandler(object sender, EventArgs e);

		public event HoverEventHandler OnHover;

		public delegate void CloseEventHandler(object sender, EventArgs e);

		public event CloseEventHandler OnClosed;

		#endregion
	}

	#region Enums

	/// <summary>
	/// Duration definition. Short is 2 seconds, long is 3 seconds
	/// </summary>
	public enum Duration
	{
		LENGTH_SHORT = 0,
		LENGTH_LONG = 1
	}

	/// <summary>
	/// Animation to display Toast
	/// </summary>
	public enum Animation
	{
		FADE = 1,
        SLIDE = 0
	}

	/// <summary>
	/// Location of Toast. Only top right and bottom right supported
	/// </summary>
	public enum Position
	{
		TopRight,
		BottomRight
	}

	/// <summary>
	/// Way to closing Toast
	/// </summary>
	public enum CloseStye
	{
		ClickEntire,
		Button,
		ButtonAndClickEntire
	}

	/// <summary>
	/// Color scheme for Toast
	/// </summary>
	public enum Theme
	{
		Dark,
		Light,
		PrimaryLight,
		SuccessLight,
		WarningLight,
		ErrorLight,
		PrimaryDark,
		SuccessDark,
		WarningDark,
		ErrorDark,
		Custom
	}

	#endregion
}
