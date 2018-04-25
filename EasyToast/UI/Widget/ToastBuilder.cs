using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.UI.Widget
{
	/// <summary>
	/// Make your own custom Toast Notification by the Builder
	/// </summary>
	public static class ToastBuilder
	{
		/// <summary>
		/// Create an empty Toast
		/// </summary>
		/// <param name="window"></param>
		/// <returns></returns>
		public static Toast Create(IWin32Window window)
		{
			return new Toast(window);
		}

		/// <summary>
		/// Set text for toast
		/// </summary>
		/// <param name="toast">toast</param>
		/// <param name="text">Text data to display</param>
		/// <returns></returns>
		public static Toast SetText(this Toast toast, string text)
		{
			toast.Text = text;
			return toast;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="toast"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public static Toast SetDuration(this Toast toast, Duration duration)
		{
			toast.Duration = duration;
			return toast;
		}

		public static Toast SetMuting(this Toast toast, bool muting)
		{
			toast.IsMuted = muting;
			return toast;
		}

		public static Toast SetThumbnail(this Toast toast, Image image)
		{
			toast.Thumbnail = image;
			return toast;
		}
	}
}
