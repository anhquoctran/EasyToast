using System.Drawing;
using Xunit;

namespace FuzzyToast.Tests;

public class ColorSchemeTests
{
	[Fact]
	public void T01_ByteConstructor_UsesStandardRgbOrder()
	{
		var scheme = new ColorScheme(10, 20, 30, 40, 50, 60);

		Assert.Equal(Color.FromArgb(10, 20, 30), scheme.Background);
		Assert.Equal(Color.FromArgb(40, 50, 60), scheme.Foreground);
	}

	[Fact]
	public void T02_ThemeCatalog_PrimaryLight_IsMaterialBlue()
	{
		var scheme = ThemeCatalog.Resolve(ToastTheme.PrimaryLight);

		Assert.Equal(Color.FromArgb(33, 150, 243), scheme.Background);
		Assert.Equal(Color.FromArgb(255, 255, 255), scheme.Foreground);
	}

	[Fact]
	public void T03_ThemeCatalog_CustomWithoutScheme_Throws()
	{
		Assert.Throws<InvalidOperationException>(() => ThemeCatalog.Resolve(ToastTheme.Custom));
	}

	[Fact]
	public void T03b_ThemeCatalog_CustomWithScheme_ReturnsScheme()
	{
		var custom = new ColorScheme(1, 2, 3, 4, 5, 6);
		var scheme = ThemeCatalog.Resolve(ToastTheme.Custom, custom);
		Assert.Same(custom, scheme);
	}
}
