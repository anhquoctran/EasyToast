using FuzzyToast.Internal;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class NativeCompatTests
{
	[Fact]
	public void NativeMethods_TickCount_And_Dpi()
	{
		Assert.True(NativeMethods.TickCount64 >= 0);

		StaHelper.Run(() =>
		{
			Assert.Equal(DpiScaling.BaselineDpi, NativeMethods.GetDeviceDpi(null));

			using var disposed = new Form();
			disposed.Dispose();
			Assert.Equal(DpiScaling.BaselineDpi, NativeMethods.GetDeviceDpi(disposed));

			using var form = StaHelper.CreateVisibleOwner();
			var dpi = NativeMethods.GetDeviceDpi(form);
			Assert.True(dpi >= 96);
			NativeMethods.SetCueBanner(new TextBox { Parent = form }, "hint");
			var ran = false;
			NativeMethods.InvokeOn(form, () => ran = true);
			Assert.True(ran);
			form.Close();
		});
	}
}
