using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace System.UI
{
	internal static class Utils
	{
		internal static string GetGuid()
		{
			var guid = Guid.NewGuid();
			return guid.ToString("N");
		}

		internal static bool ValidateImageSize(Image image)
		{
			if (image == null) return false;
			try
			{
				return image.Width >= 64 || image.Height >= 64;
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal static bool ValidateImage(string path)
		{
			if (string.IsNullOrEmpty(path)) return false;
			if (!File.Exists(path)) return false;
			var bytes = File.ReadAllBytes(path);
			var png = new byte[] { 137, 80, 78, 71 };
			var jpeg = new byte[] { 255, 216, 255, 224 };

			return png.SequenceEqual(bytes.Take(png.Length)) || jpeg.SequenceEqual(bytes.Take(jpeg.Length));
		}

		private delegate void SetPropertyThreadSafeDelegate<TTResult>(Control @this, Expression<Func<TTResult>> property, TTResult value);

		internal static void SetPropertyThreadSafe<TTResult>(
			this Control @this,
			Expression<Func<TTResult>> property,
			TTResult value)
		{
			var propertyInfo = ((MemberExpression) property.Body).Member
				as PropertyInfo;

			if (propertyInfo != null && propertyInfo.ReflectedType != null && (propertyInfo == null ||!@this.GetType().IsSubclassOf(propertyInfo.ReflectedType) || @this.GetType().GetProperty(propertyInfo.Name, propertyInfo.PropertyType) == null))
			{
				throw new ArgumentException("The lambda expression 'property' must reference a valid property on this Control.");
			}

			if (@this.InvokeRequired)
			{
				@this.Invoke(new SetPropertyThreadSafeDelegate<TTResult>(SetPropertyThreadSafe), @this, property, value);
			}
			else
			{
				@this.GetType().InvokeMember(propertyInfo?.Name, BindingFlags.SetProperty, null, @this, new object[] { value });
			}
		}
	}
}
