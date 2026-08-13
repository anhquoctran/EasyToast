namespace FuzzyToast.Layout;

/// <summary>Which limit triggered a capacity decision.</summary>
public enum CapacityConstraint
{
	/// <summary>Under both global and per-corner limits.</summary>
	None,

	/// <summary>The incoming corner already has <c>MaxToastsPerPosition</c> toasts.</summary>
	PerPosition,

	/// <summary>The manager already has <c>MaxToasts</c> visible toasts.</summary>
	Global
}

/// <summary>Action the manager must take after <see cref="CapacityPolicy.Evaluate"/>.</summary>
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

/// <summary>Result of a pure capacity evaluation (no UI side effects).</summary>
/// <param name="Action">What the manager should do.</param>
/// <param name="TriggeredBy">Which limit fired, or <see cref="CapacityConstraint.None"/>.</param>
/// <param name="VictimId">Id to remove when <paramref name="Action"/> is <see cref="CapacityAction.RemoveVictimThenAllow"/>.</param>
/// <param name="Reason">Short machine-readable reason (<c>OK</c>, <c>MaxToasts</c>, <c>MaxToastsPerPosition</c>).</param>
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
	/// <summary>
	/// Decides whether an incoming toast may be shown.
	/// Per-corner limit is checked before the global limit.
	/// </summary>
	/// <param name="policy">Overflow policy from <see cref="ToastManagerOptions"/>.</param>
	/// <param name="maxToasts">Global maximum (must be ≥ 1).</param>
	/// <param name="maxToastsPerPosition">Per-corner maximum (must be ≥ 1).</param>
	/// <param name="incomingPosition">Corner of the new toast.</param>
	/// <param name="activeOldestFirstGlobal">Currently visible toasts, oldest first.</param>
	/// <exception cref="ArgumentNullException"><paramref name="activeOldestFirstGlobal"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">A max value is less than 1, or <paramref name="policy"/> is unknown.</exception>
	public static CapacityDecision Evaluate(
		ToastOverflowPolicy policy,
		int maxToasts,
		int maxToastsPerPosition,
		ToastPosition incomingPosition,
		IReadOnlyList<(string Id, ToastPosition Position)> activeOldestFirstGlobal)
	{
		FuzzyToast.Internal.Guard.NotNull(activeOldestFirstGlobal, nameof(activeOldestFirstGlobal));
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
