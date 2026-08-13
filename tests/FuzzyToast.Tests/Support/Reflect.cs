using System.Reflection;

namespace FuzzyToast.Tests.Support;

internal static class Reflect
{
	public static object? GetField(object obj, string name)
	{
		for (var t = obj.GetType(); t is not null; t = t.BaseType)
		{
			var field = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (field is not null)
				return field.GetValue(obj);
		}

		throw new MissingFieldException(obj.GetType().FullName, name);
	}

	public static void SetField(object obj, string name, object? value)
	{
		for (var t = obj.GetType(); t is not null; t = t.BaseType)
		{
			var field = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (field is not null)
			{
				field.SetValue(obj, value);
				return;
			}
		}

		throw new MissingFieldException(obj.GetType().FullName, name);
	}

	public static object? Invoke(object obj, string name, params object?[] args)
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		MethodInfo? method = null;
		for (var t = obj.GetType(); t is not null; t = t.BaseType)
		{
			var candidates = t.GetMethods(flags).Where(m => m.Name == name).ToArray();
			method = candidates.FirstOrDefault(m => m.GetParameters().Length == args.Length)
				?? candidates.FirstOrDefault();
			if (method is not null)
				break;
		}

		if (method is null)
			throw new MissingMethodException(obj.GetType().FullName, name);

		try
		{
			return method.Invoke(obj, args);
		}
		catch (TargetInvocationException ex) when (ex.InnerException is not null)
		{
			throw ex.InnerException;
		}
	}
}
