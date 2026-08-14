using FuzzyToast.Internal;
using FuzzyToast.Layout;
using FuzzyToast.Tests.Support;
using Xunit;

namespace FuzzyToast.Tests;

public class ToastMetadataClickTests
{
	private static ToastManager CreateManager()
	{
		var area = new ScreenWorkingArea(0, 0, 1920, 1080);
		return new ToastManager(
			null,
			new ToastManagerOptions { PlaySound = false },
			new FixedScreenProvider(area),
			new ImmediateUiMarshaler(),
			(opts, handle) => new FakeToastView(handle));
	}

	[Fact]
	public void Clicked_Event_Delivers_Tag_And_Metadata()
	{
		using var mgr = CreateManager();
		var handle = mgr.Create()
			.SetCaption("Order ready")
			.SetTag(42)
			.SetMetadata("orderId", "ORD-100")
			.SetMetadata("source", "kitchen")
			.SetExtData("qty", 3)
			.Show();

		ToastInteractionEventArgs? args = null;
		handle.Clicked += (_, e) => args = e;

		handle.RaiseClicked();

		Assert.NotNull(args);
		Assert.Equal(handle.Id, args!.ToastId);
		Assert.Equal(42, args.Tag);
		Assert.Equal(42, args.Data);
		Assert.Equal("ORD-100", args.Metadata["orderId"]);
		Assert.Equal("kitchen", args["source"]);
		Assert.True(args.TryGetMetadata<int>("qty", out var qty));
		Assert.Equal(3, qty);
		Assert.Equal(3, args.GetMetadata<int>("qty"));
	}

	[Fact]
	public void Toast_Build_OnClick_Receives_ExtData()
	{
		StaHelper.Run(() =>
		{
			using var form = StaHelper.CreateVisibleOwner();
			ToastInteractionEventArgs? args = null;

			var toast = Toast.MakeText(form, "Notify", "tap me")
				.SetMuting(true)
				.SetData(new { UserId = 7 })
				.SetExtData("action", "open-profile")
				.SetMetadata("userId", 7);

			toast.OnClick += (_, e) => args = e;
			toast.Show();
			Application.DoEvents();

			Assert.NotNull(toast.Handle);
			if (toast.Handle!.IsVisible)
			{
				toast.Handle.RaiseClicked();
				Assert.NotNull(args);
				Assert.NotNull(args!.Tag);
				Assert.Equal("open-profile", args.GetMetadata<string>("action"));
				Assert.Equal(7, args.GetMetadata<int>("userId"));
				toast.Dismiss();
			}

			Application.DoEvents();
			form.Close();
		});
	}

	[Fact]
	public void SetMetadata_Rejects_EmptyKey()
	{
		using var form = new Form();
		var toast = Toast.MakeText(form, "x");
		Assert.Throws<ArgumentException>(() => toast.SetMetadata("  ", 1));
	}

	[Fact]
	public void Metadata_Is_Snapshot_On_Show()
	{
		using var mgr = CreateManager();
		var builder = mgr.Create()
			.SetCaption("Snap")
			.SetMetadata("k", "v1");
		var handle = builder.Show();
		// mutate builder after show must not change frozen options
		builder.SetMetadata("k", "v2");
		Assert.Equal("v1", handle.Options.Metadata["k"]);
	}
}
