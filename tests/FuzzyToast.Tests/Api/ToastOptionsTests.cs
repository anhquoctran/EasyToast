using Xunit;

namespace FuzzyToast.Tests;

public class ToastOptionsTests
{
	[Fact]
	public void T14_EmptyCaption_Throws()
	{
		var o = new ToastOptions { Caption = "  " };
		Assert.Throws<ArgumentException>(() => o.Validate());
	}

	[Fact]
	public void T14b_EmptyDescription_Allowed()
	{
		var o = new ToastOptions { Caption = "Hi", Description = "" };
		o.Validate();
	}

	[Fact]
	public void CustomThemeWithoutColors_Throws()
	{
		var o = new ToastOptions { Caption = "Hi", Theme = ToastTheme.Custom };
		Assert.Throws<ArgumentException>(() => o.Validate());
	}
}
