using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.UI.Widget
{
	/// <summary>
	/// Use the Manager to managing multiple Toast widgets
	/// </summary>
	public static class ToastManager
	{
		public const byte MAX_TOASTS_ALLOWED = 3;
		private static ToastCollection _privateCollection = new ToastCollection();
		internal static Toast Toast;

		/// <summary>
		/// Get all toasts displaying
		/// </summary>
		public static ToastCollection ToastCollection
		{
			get => _privateCollection;
			private set => _privateCollection = value;
		}

		/// <summary>
		/// Add toast to collection
		/// </summary>
		internal static void AddToCollection()
		{
			if (ToastCollection.Count >= MAX_TOASTS_ALLOWED) return;
			if (string.IsNullOrEmpty(Toast.Text))
			{
				throw new ArgumentException("Text property is required to display Toast");
			}

			Toast.FrmToast.CloseStyle = Toast.CloseStyle;
			Toast.FrmToast.IsMuted = Toast.IsMuted;
			Toast.FrmToast.Toast = Toast;
			Toast.FrmToast.Duration = Toast.Duration;
			Toast.FrmToast.Animation = Toast.Animation;
			Toast.FrmToast.Caption = Toast.Text;
			Toast.FrmToast.Thumbnails = Toast.Thumbnail;
			Toast.FrmToast.Theme = Toast.ThemeStyle;

			SetLocation(Toast.Position);

			_privateCollection.Add(Toast);
			Toast.FrmToast.Show(Toast.Window);

		}

		private static void SetLocation(Position position)
		{
			switch (position)
			{
				case Position.TopRight:
					var rightmost = Screen.AllScreens[0];
					foreach (var screen in Screen.AllScreens)
					{
						if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
							rightmost = screen;
					}

					if (ToastCollection.Count == 0)
					{
						Toast.FrmToast.Left = rightmost.WorkingArea.Right - Toast.FrmToast.Width - Toast.GetHorizontalMargin();
						Toast.FrmToast.Top = rightmost.WorkingArea.Top + Toast.GetVerticalMargin();
					}
					else
					{
						var collection = ToastCollection.GetTopRightToasts();
						var enumerable = collection as List<Toast> ?? collection.ToList();
						if (enumerable.Count == 0)
						{
							Toast.FrmToast.Left = rightmost.WorkingArea.Right - Toast.FrmToast.Width - Toast.GetHorizontalMargin();
							Toast.FrmToast.Top = rightmost.WorkingArea.Top + Toast.GetVerticalMargin();
						}
						else
						{
							Toast.FrmToast.Left = rightmost.WorkingArea.Right - Toast.FrmToast.Width - Toast.GetHorizontalMargin();
							Toast.FrmToast.Top = rightmost.WorkingArea.Top + enumerable.Count * Toast.FrmToast.Height + enumerable.Count* Toast.GetVerticalMargin() + Toast.GetVerticalMargin();
						}
					}
					
					break;
				case Position.BottomRight:
				{
					var workingArea = Screen.GetWorkingArea(Toast.FrmToast);
					if (ToastCollection.Count == 0)
					{
						Toast.FrmToast.Location = new Point(workingArea.Right - Toast.FrmToast.Size.Width - Toast.GetHorizontalMargin(),
							workingArea.Bottom - Toast.FrmToast.Size.Height - Toast.GetVerticalMargin());
					}
					else
					{
						var collection = ToastCollection.GetBottomRightToasts();
						var enumerable = collection as List<Toast> ?? collection.ToList();

						if (enumerable.Count == 0)
						{
							Toast.FrmToast.Location = new Point(workingArea.Right - Toast.FrmToast.Size.Width - Toast.GetHorizontalMargin(),
								workingArea.Bottom - Toast.FrmToast.Size.Height - Toast.GetVerticalMargin());
							}
						else
						{
							Toast.FrmToast.Location = new Point(workingArea.Right - Toast.FrmToast.Size.Width - Toast.GetHorizontalMargin(),
								workingArea.Bottom - Toast.FrmToast.Size.Height - Toast.FrmToast.Size.Height * ToastCollection.Count - Toast.GetVerticalMargin() * ToastCollection.Count - Toast.GetVerticalMargin());
						}	
					}
				}
					break;
			}
		}

	}

	public class ToastCollection : ICollection<Toast>
	{
		private readonly List<Toast> _privateList;

		/// <summary>
		/// Initialize empty Toast collection
		/// </summary>
		public ToastCollection()
		{
			_privateList = new List<Toast>(ToastManager.MAX_TOASTS_ALLOWED);
		}

		/// <summary>
		/// Initialize Toast collection from other collection
		/// </summary>
		/// <param name="other"></param>
		public ToastCollection(ToastCollection other)
		{
			if (other != null)
			{
				_privateList = other._privateList;
			}
		}

		public Toast this[int index0]
		{
			get => _privateList[index0];
			set => _privateList[index0] = value;
		}

		public int Locate(Toast toast)
		{
			return _privateList.IndexOf(toast);
		}

		public IEnumerator<Toast> GetEnumerator()
		{
			return _privateList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Toast item)
		{
			_privateList.Add(item);
		}

		public void Clear()
		{
			_privateList.Clear();
		}

		public bool Contains(Toast item)
		{
			return false;
		}

		public void CopyTo(Toast[] array, int arrayIndex)
		{
			_privateList.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Remove Toast item from current collection
		/// </summary>
		/// <param name="toast"></param>
		/// <returns></returns>
		public bool Remove(Toast toast)
		{
			return _privateList.Remove(toast);
		}

		/// <summary>
		/// Remove all Toasts matched condition in predicate
		/// </summary>
		/// <param name="match"></param>
		/// <returns></returns>
		public int RemoveAll(Predicate<Toast> match)
		{
			return _privateList.RemoveAll(match);
		}

		/// <summary>
		/// Get all Top-Right Toasts in collection
		/// </summary>
		/// <returns>Top-Right Toast list</returns>
		public IEnumerable<Toast> GetTopRightToasts()
		{
			if (_privateList.Count == 0) yield break;
			foreach (var toast in _privateList)
			{
				if (toast.Position == Position.TopRight)
				{
					yield return toast;
				}
			}
		}

		/// <summary>
		/// Get all Bottom-Right Toasts in collection
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Toast> GetBottomRightToasts()
		{
			if(_privateList.Count == 0) yield break;
			foreach (var toast in _privateList)
			{
				if (toast.Position == Position.BottomRight)
				{
					yield return toast;
				}
			}
		}

		public int Count => _privateList.Count;
		public bool IsReadOnly => false;
	}
}
