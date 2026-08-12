namespace FuzzyToast.Layout;

public enum CapacityConstraint
{
	None,
	PerPosition,
	Global
}

public enum CapacityAction
{
	/// <summary>Proceed to show without removing anyone.</summary>
	Allow,

	/// <summary>Do not show the new toast (DropNewest).</summary>
	RejectNewest,

	/// <summary>Remove victim then show the new toast (DropOldest).</summary>
	RemoveVictimThenAllow,

	/// <summary>Caller must throw <see cref="InvalidOperationException"/>.</summary>
	Throw
}

public sealed record CapacityDecision(
	CapacityAction Action,
	CapacityConstraint TriggeredBy,
	string? VictimId,
	string Reason);

/// <summary>
/// Pure capacity evaluation. No UI.
/// Active list is oldest-first global (index 0 = global oldest).
/// </summary>
public static class CapacityPolicy
{
	public static CapacityDecision Evaluate(
		ToastOverflowPolicy policy,
		int maxToasts,
		int maxToastsPerPosition,
		ToastPosition incomingPosition,
		IReadOnlyList<(string Id, ToastPosition Position)> activeOldestFirstGlobal)
	{
		ArgumentNullException.ThrowIfNull(activeOldestFirstGlobal);
		if (maxToasts < 1)
			throw new ArgumentOutOfRangeException(nameof(maxToasts));
		if (maxToastsPerPosition < 1)
			throw new ArgumentOutOfRangeException(nameof(maxToastsPerPosition));

		var global = activeOldestFirstGlobal.Count;
		var perPos = 0;
		string? perPosOldestId = null;
		foreach (var (id, position) in activeOldestFirstGlobal)
		{
			if (position != incomingPosition)
				continue;
			perPos++;
			perPosOldestId ??= id;
		}

		CapacityConstraint constraint;
		string? victimId;
		string reason;

		if (perPos >= maxToastsPerPosition)
		{
			constraint = CapacityConstraint.PerPosition;
			victimId = perPosOldestId;
			reason = "MaxToastsPerPosition";
		}
		else if (global >= maxToasts)
		{
			constraint = CapacityConstraint.Global;
			victimId = global > 0 ? activeOldestFirstGlobal[0].Id : null;
			reason = "MaxToasts";
		}
		else
		{
			return new CapacityDecision(CapacityAction.Allow, CapacityConstraint.None, null, "OK");
		}

		return policy switch
		{
			ToastOverflowPolicy.DropNewest => new CapacityDecision(
				CapacityAction.RejectNewest, constraint, null, reason),
			ToastOverflowPolicy.DropOldest => victimId is null
				? new CapacityDecision(CapacityAction.Throw, constraint, null, reason + ":NoVictim")
				: new CapacityDecision(CapacityAction.RemoveVictimThenAllow, constraint, victimId, reason),
			ToastOverflowPolicy.Throw => new CapacityDecision(
				CapacityAction.Throw, constraint, victimId, reason),
			_ => throw new ArgumentOutOfRangeException(nameof(policy))
		};
	}
}
