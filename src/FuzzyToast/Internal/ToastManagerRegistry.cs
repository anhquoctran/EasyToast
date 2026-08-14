using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace FuzzyToast.Internal;

/// <summary>
/// One shared <see cref="ToastManager"/> per owner control for Android-style <c>Toast.MakeText</c> API.
/// Uses a weak table so managers do not pin forms forever.
/// </summary>
internal static class ToastManagerRegistry
{
	private static readonly ConditionalWeakTable<Control, ToastManager> Managers = new();

	public static ToastManager GetOrCreate(Control owner)
	{
		Guard.NotNull(owner, nameof(owner));
		if (owner.IsDisposed)
			throw new ObjectDisposedException(nameof(owner));

		lock (Managers)
		{
			if (Managers.TryGetValue(owner, out var existing))
				return existing;
				
			// Create inline to ensure exactly one manager per owner.
			// owner.Disposed subscription happens safely within this lock.
			var manager = new ToastManager(owner);
			return manager;
		}
	}

	/// <summary>
	/// Registers <paramref name="manager"/> as the shared instance for <paramref name="owner"/>.
	/// Called automatically from the public <see cref="ToastManager"/> constructor.
	/// </summary>
	public static void Register(Control owner, ToastManager manager)
	{
		Guard.NotNull(owner, nameof(owner));
		Guard.NotNull(manager, nameof(manager));
		lock (Managers)
		{
			Managers.Remove(owner);
			Managers.Add(owner, manager);
		}
	}
}
