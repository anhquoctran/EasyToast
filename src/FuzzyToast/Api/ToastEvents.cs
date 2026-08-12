namespace FuzzyToast;

public sealed class ToastChangedEventArgs : EventArgs
{
	public ToastChangedEventArgs(ToastHandle toast) => Toast = toast;
	public ToastHandle Toast { get; }
}

public sealed class ToastRejectedEventArgs : EventArgs
{
	public ToastRejectedEventArgs(ToastHandle toast, ToastOptions options, string reason)
	{
		Toast = toast;
		Options = options;
		Reason = reason;
	}

	public ToastHandle Toast { get; }
	public ToastOptions Options { get; }
	public string Reason { get; }
}

/// <summary>
/// Payload for toast user interaction (click / hover).
/// Carries <see cref="Tag"/> and key/value <see cref="Metadata"/> set when building the toast.
/// </summary>
public sealed class ToastInteractionEventArgs : EventArgs
{
	public ToastInteractionEventArgs(ToastHandle handle)
	{
		Handle = handle ?? throw new ArgumentNullException(nameof(handle));
	}

	/// <summary>Live handle for this toast (id, options, dismiss).</summary>
	public ToastHandle Handle { get; }

	/// <summary>Unique toast id.</summary>
	public string ToastId => Handle.Id;

	/// <summary>Full options snapshot used to show the toast.</summary>
	public ToastOptions Options => Handle.Options;

	/// <summary>Arbitrary object passed via <c>SetTag</c> / <c>SetData</c>.</summary>
	public object? Tag => Handle.Options.Tag;

	/// <summary>Alias of <see cref="Tag"/> (ext/user data bag).</summary>
	public object? Data => Handle.Options.Tag;

	/// <summary>Key/value metadata passed via <c>SetMetadata</c> / <c>SetExtData</c>.</summary>
	public IReadOnlyDictionary<string, object?> Metadata => Handle.Options.Metadata;

	/// <summary>Gets a metadata value by key, or <c>null</c> if missing.</summary>
	public object? this[string key] =>
		Handle.Options.Metadata.TryGetValue(key, out var value) ? value : null;

	/// <summary>Try get metadata value cast to <typeparamref name="T"/>.</summary>
	public bool TryGetMetadata<T>(string key, out T? value)
	{
		value = default;
		if (!Handle.Options.Metadata.TryGetValue(key, out var raw) || raw is null)
			return false;

		if (raw is T typed)
		{
			value = typed;
			return true;
		}

		try
		{
			value = (T)Convert.ChangeType(raw, typeof(T));
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>Get metadata as <typeparamref name="T"/>, or <paramref name="defaultValue"/> if missing/incompatible.</summary>
	public T? GetMetadata<T>(string key, T? defaultValue = default) =>
		TryGetMetadata<T>(key, out var value) ? value : defaultValue;
}
