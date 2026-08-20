using System;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Xunit;

namespace FuzzyToast.E2ETests;

public class ToastBasicE2ETests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public ToastBasicE2ETests()
    {
        // Calculate the path to the demo app executable.
        // During testing, the working directory is typically bin/Debug/net8.0-windows
        // We reference the demo app, so it should be built to its own bin folder,
        // or copied. Since we added ReferenceOutputAssembly=true, it might be in the same folder,
        // or we can find it relative to the solution root.
        
        // A robust way to find it relative to the test assembly path:
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        // Move up from tests/FuzzyToast.E2ETests/bin/Debug/net8.0-windows/
        // to solution root, then into samples/EasyToastDemo/bin/Debug/net8.0-windows/EasyToastDemo.exe
        // Actually, since we have a project reference, the EasyToastDemo.exe should be in the same output folder as the test dll.
        var exePath = Path.Combine(currentDir, "EasyToastDemo.exe");
        
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Could not find demo app at {exePath}");
        }

        _app = Application.Launch(exePath);
        _automation = new UIA3Automation();
        
        // Wait for the main window to appear (timeout 5s)
        _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(5));
        Assert.NotNull(_mainWindow);
    }

    [Fact]
    public void ClickShow_ShouldCreateToastWindow()
    {
        // Arrange
        // The demo app has a button with text "Show()"
        var showButton = _mainWindow.FindFirstDescendant(cf => cf.ByName("Show()"))?.AsButton();
        Assert.NotNull(showButton);

        // Act
        showButton.Invoke();
        
        // Assert
        // A new toast window should be spawned. The toast window might be a top-level window or child of the desktop.
        // FuzzyToast creates unowned top-level windows for toasts.
        var desktop = _automation.GetDesktop();
        
        // We poll for the toast window since animations might take a moment.
        Window toastWindow = null;
        var timeout = DateTime.Now.AddSeconds(3);
        while (DateTime.Now < timeout)
        {
            // FuzzyToast windows usually have a specific class or we can find it by text.
            // By default, the demo app shows "Hello, I am Toast!" as caption.
            var allWindows = desktop.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Window));
            foreach (var w in allWindows)
            {
                if (w.Name.Contains("Hello, I am Toast!") || w.FindFirstDescendant(cf => cf.ByName("Hello, I am Toast!")) != null)
                {
                    toastWindow = w.AsWindow();
                    break;
                }
            }
            
            if (toastWindow != null) break;
            System.Threading.Thread.Sleep(200);
        }

        Assert.NotNull(toastWindow);
        
        // Verify it contains the description text as well
        var descriptionElement = toastWindow.FindFirstDescendant(cf => cf.ByName("Click me — Tag + Metadata are returned in OnClick"));
        Assert.NotNull(descriptionElement);
    }

    public void Dispose()
    {
        _app?.Close();
        _app?.Dispose();
        _automation?.Dispose();
    }
}
