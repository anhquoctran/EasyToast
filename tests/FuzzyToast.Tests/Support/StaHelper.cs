using System.Windows.Forms;

namespace FuzzyToast.Tests.Support;

/// <summary>Runs work on a dedicated STA thread (required for WinForms Control/Form on Windows).</summary>
internal static class StaHelper
{
	public static void Run(Action action)
	{
		Exception? error = null;
		var thread = new Thread(() =>
		{
			try { action(); }
			catch (Exception ex) { error = ex; }
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.IsBackground = true;
		thread.Start();
		if (!thread.Join(TimeSpan.FromSeconds(60)))
			throw new TimeoutException("STA test did not finish within 60s.");
		if (error is not null)
			throw new Exception("STA test failed: " + error.Message, error);
	}

	public static T Run<T>(Func<T> func)
	{
		T? result = default;
		Run(() => { result = func(); });
		return result!;
	}

	/// <summary>Run async work on STA by blocking the STA thread on the task.</summary>
	public static void Run(Func<Task> asyncAction)
	{
		Run(() => asyncAction().GetAwaiter().GetResult());
	}

	public static Form CreateVisibleOwner()
	{
		var form = new Form
		{
			ShowInTaskbar = false,
			Opacity = 0.01,
			StartPosition = FormStartPosition.Manual,
			Location = new System.Drawing.Point(-32000, -32000),
			Size = new System.Drawing.Size(200, 100),
			Text = "ToastTestOwner"
		};
		form.Show();
		Application.DoEvents();
		return form;
	}
}
