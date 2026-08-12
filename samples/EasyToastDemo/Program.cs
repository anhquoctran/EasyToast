using System;
using System.Windows.Forms;

namespace EasyToastDemo;

static class Program
{
	/// <summary>
	/// Entry point — Windows 10/11, .NET 8+ WinForms.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// Sets HighDpiMode, visual styles, and text rendering defaults safely.
		ApplicationConfiguration.Initialize();
		Application.Run(new Form1());
	}
}
