using System.Windows.Forms;
using Xunit;

namespace FuzzyToast.Tests;

/// <summary>Android-style Toast.MakeText(...).Show() surface.</summary>
public class ToastBuildApiTests
{
	[Fact]
	public void Build_Overloads_SetFields()
	{
		using var form = new Form();
		var t = Toast.MakeText(form, "Cap", "Desc", Duration.LENGTH_LONG);
		Assert.Equal("Cap", t.Caption);
		Assert.Equal("Desc", t.Description);
		Assert.Equal(Duration.LENGTH_LONG, t.Duration);
		Assert.Equal(Duration.Long, t.Duration);

		var a = Toast.MakeText(form, "X", Animation.FADE);
		Assert.Equal(Animation.FADE, a.Animation);
		Assert.Equal(Animation.Fade, a.Animation);

		var d = Toast.MakeText(form, "Y", Duration.LENGTH_SHORT);
		Assert.Equal(Duration.LENGTH_SHORT, d.Duration);
	}

	[Fact]
	public void Duration_And_Animation_Aliases_Match()
	{
		Assert.Equal(Duration.Short, Duration.LENGTH_SHORT);
		Assert.Equal(Duration.Long, Duration.LENGTH_LONG);
		Assert.Equal(Animation.Fade, Animation.FADE);
		Assert.Equal(Animation.Slide, Animation.SLIDE);
	}

	[Fact]
	public void Fluent_SetTheme_And_Position()
	{
		using var form = new Form();
		var t = Toast.MakeText(form, "Hi")
			.SetTheme(ToastTheme.PrimaryLight)
			.SetPosition(ToastPosition.TopLeft);
		Assert.Equal(ToastTheme.PrimaryLight, t.Theme);
		Assert.Equal(ToastPosition.TopLeft, t.Position);
	}
}
