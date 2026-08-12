using System.Windows.Forms;

namespace FuzzyToast.Internal;

internal interface IUiMarshaler
{
	bool InvokeRequired { get; }
	void Invoke(Action action);
	Task InvokeAsync(Action action);
}

/// <summary>
/// Marshals work to the WinForms UI thread (Windows 10/11 + .NET 8 WinForms).
/// Ensures the control handle exists so Invoke/BeginInvoke are reliable.
/// </summary>
internal sealed class WinFormsUiMarshaler : IUiMarshaler
{
	private readonly Control _control;

	public WinFormsUiMarshaler(Control control) => _control = control;

	public bool InvokeRequired
	{
		get
		{
			EnsureHandle();
			return _control.InvokeRequired;
		}
	}

	public void Invoke(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		if (_control.IsDisposed)
			throw new ObjectDisposedException(_control.Name);

		EnsureHandle();

		if (!_control.InvokeRequired)
		{
			action();
			return;
		}

		_control.Invoke(action);
	}

	public Task InvokeAsync(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		if (_control.IsDisposed)
			return Task.FromException(new ObjectDisposedException(_control.Name));

		EnsureHandle();

		if (!_control.InvokeRequired)
		{
			try
			{
				action();
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}
		}

		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		try
		{
			_control.BeginInvoke(() =>
			{
				try
				{
					action();
					tcs.TrySetResult();
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});
		}
		catch (Exception ex)
		{
			tcs.TrySetException(ex);
		}

		return tcs.Task;
	}

	private void EnsureHandle()
	{
		if (_control.IsDisposed)
			return;

		if (_control.IsHandleCreated)
			return;

		// Creating the handle must happen on the thread that owns the control.
		// If we are already on that thread (typical for Form before first show), force handle creation.
		if (!_control.InvokeRequired)
		{
			_ = _control.Handle;
		}
	}
}

/// <summary>No-op marshaler for unit tests (always runs inline).</summary>
internal sealed class ImmediateUiMarshaler : IUiMarshaler
{
	public bool InvokeRequired => false;
	public void Invoke(Action action) => action();
	public Task InvokeAsync(Action action)
	{
		action();
		return Task.CompletedTask;
	}
}
