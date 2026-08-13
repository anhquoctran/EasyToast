namespace FuzzyToast;

/// <summary>Raised when a toast is added to or removed from a <see cref="ToastManager"/>.</summary>
public sealed class ToastChangedEventArgs : EventArgs
{
	/// <param name="toast">Handle that was added or removed.</param>
	public ToastChangedEventArgs(ToastHandle toast) => Toast = toast;

	/// <summary>The toast that changed.</summary>
	public ToastHandle Toast { get; }
}

/// <summary>Raised when a show attempt is rejected by capacity policy.</summary>
public sealed class ToastRejectedEventArgs : EventArgs
{
	/// <param name="toast">Rejected handle (<see cref="ToastHandle.WasRejected"/> is <see langword="true"/>).</param>
	/// <param name="options">Options that were not shown.</param>
	/// <param name="reason">Machine-readable reason (for example <c>MaxToastsPerPosition</c>).</param>
	public ToastRejectedEventArgs(ToastHandle toast, ToastOptions options, string reason)
	{
		Toast = toast;
		Options = options;
		Reason = reason;
	}

	/// <summary>Handle in <see cref="ToastHandleState.RejectedCapacity"/>.</summary>
	public ToastHandle Toast { get; }

	/// <summary>Options that were not displayed.</summary>
	public ToastOptions Options { get; }

	/// <summary>Why the toast was rejected (<c>MaxToasts</c> or <c>MaxToastsPerPosition</c>).</summary>
	public string Reason { get; }
}

/// <summary>
/// Payload for toast user interaction (click / hover).
/// Carries <see cref="Tag"/> and key/value <see cref="Metadata"/> set when building the toast.
/// </summary>
public class ToastInteractionEventArgs : EventArgs
{
	/// <param name="handle">Live handle for the toast that was clicked or hovered.</param>
	/// <exception cref="ArgumentNullException"><paramref name="handle"/> is <see langword="null"/>.</exception>
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

	/// <summary>
	/// Tries to read metadata <paramref name="key"/> as <typeparamref name="T"/>.
	/// Accepts an exact type match or a successful <see cref="Convert.ChangeType(object, Type)"/>.
	/// </summary>
	/// <param name="key">Metadata key.</param>
	/// <param name="value">Converted value when the method returns <see langword="true"/>.</param>
	/// <returns><see langword="true"/> if the key exists and can be converted to <typeparamref name="T"/>.</returns>
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

/// <summary>
/// Raised when the user submits text from an inputable toast (Submit button or Enter).
/// </summary>
public sealed class ToastSubmittedEventArgs : ToastInteractionEventArgs
{
	/// <param name="handle">Handle of the inputable toast.</param>
	/// <param name="inputText">Text from the input box. <see langword="null"/> becomes empty.</param>
	public ToastSubmittedEventArgs(ToastHandle handle, string inputText)
		: base(handle)
	{
		InputText = inputText ?? string.Empty;
	}

	/// <summary>Text the user entered (trimmed by the form unless configured otherwise).</summary>
	public string InputText { get; }

	/// <summary>True when <see cref="InputText"/> is null/whitespace.</summary>
	public bool IsEmpty => string.IsNullOrWhiteSpace(InputText);
}
