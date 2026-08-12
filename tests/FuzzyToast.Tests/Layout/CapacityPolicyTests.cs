using FuzzyToast.Layout;
using Xunit;

namespace FuzzyToast.Tests;

public class CapacityPolicyTests
{
	[Fact]
	public void T10_PerPositionFull_DropNewest_Rejects()
	{
		var active = new List<(string Id, ToastPosition Position)>
		{
			("a", ToastPosition.TopRight),
			("b", ToastPosition.TopRight),
			("c", ToastPosition.TopRight)
		};

		var d = CapacityPolicy.Evaluate(
			ToastOverflowPolicy.DropNewest,
			maxToasts: 6,
			maxToastsPerPosition: 3,
			ToastPosition.TopRight,
			active);

		Assert.Equal(CapacityAction.RejectNewest, d.Action);
		Assert.Equal(CapacityConstraint.PerPosition, d.TriggeredBy);
		Assert.Null(d.VictimId);
		Assert.Equal("MaxToastsPerPosition", d.Reason);
	}

	[Fact]
	public void T11_GlobalFull_DropOldest_RemovesGlobalOldest()
	{
		// 3 TR + 3 BR = 6 global; incoming TopLeft has free per-pos slots
		var active = new List<(string Id, ToastPosition Position)>
		{
			("oldest", ToastPosition.BottomRight),
			("t1", ToastPosition.TopRight),
			("t2", ToastPosition.TopRight),
			("t3", ToastPosition.TopRight),
			("b2", ToastPosition.BottomRight),
			("b3", ToastPosition.BottomRight)
		};

		var d = CapacityPolicy.Evaluate(
			ToastOverflowPolicy.DropOldest,
			maxToasts: 6,
			maxToastsPerPosition: 3,
			ToastPosition.TopLeft,
			active);

		Assert.Equal(CapacityAction.RemoveVictimThenAllow, d.Action);
		Assert.Equal(CapacityConstraint.Global, d.TriggeredBy);
		Assert.Equal("oldest", d.VictimId);
		Assert.Equal("MaxToasts", d.Reason);
	}

	[Fact]
	public void T12_PerPosFull_DropOldest_VictimSamePosition()
	{
		var active = new List<(string Id, ToastPosition Position)>
		{
			("other-old", ToastPosition.BottomLeft),
			("same-old", ToastPosition.TopRight),
			("same-mid", ToastPosition.TopRight),
			("same-new", ToastPosition.TopRight)
		};

		var d = CapacityPolicy.Evaluate(
			ToastOverflowPolicy.DropOldest,
			maxToasts: 6,
			maxToastsPerPosition: 3,
			ToastPosition.TopRight,
			active);

		Assert.Equal(CapacityAction.RemoveVictimThenAllow, d.Action);
		Assert.Equal(CapacityConstraint.PerPosition, d.TriggeredBy);
		Assert.Equal("same-old", d.VictimId);
	}

	[Fact]
	public void ThrowPolicy_ReturnsThrowAction()
	{
		var active = new List<(string Id, ToastPosition Position)>
		{
			("a", ToastPosition.BottomRight),
			("b", ToastPosition.BottomRight),
			("c", ToastPosition.BottomRight)
		};

		var d = CapacityPolicy.Evaluate(
			ToastOverflowPolicy.Throw,
			maxToasts: 6,
			maxToastsPerPosition: 3,
			ToastPosition.BottomRight,
			active);

		Assert.Equal(CapacityAction.Throw, d.Action);
	}

	[Fact]
	public void Allow_WhenUnderLimits()
	{
		var active = new List<(string Id, ToastPosition Position)>
		{
			("a", ToastPosition.TopRight)
		};

		var d = CapacityPolicy.Evaluate(
			ToastOverflowPolicy.DropNewest,
			maxToasts: 6,
			maxToastsPerPosition: 3,
			ToastPosition.TopRight,
			active);

		Assert.Equal(CapacityAction.Allow, d.Action);
		Assert.Equal(CapacityConstraint.None, d.TriggeredBy);
	}
}
