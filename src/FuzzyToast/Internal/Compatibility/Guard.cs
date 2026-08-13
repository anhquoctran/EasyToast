namespace FuzzyToast.Internal;

internal static class Guard
{
	public static T NotNull<T>(T? value, string name) where T : class
	{
		if (value is null)
			throw new ArgumentNullException(name);
		return value;
	}

	public static void NotDisposed(bool disposed, object instance)
	{
		if (disposed)
			throw new ObjectDisposedException(instance.GetType().FullName);
	}
}
