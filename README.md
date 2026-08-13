# FuzzyToast

A toast notification library for **Windows Forms** on **.NET Framework 4.6.1+** and **.NET 8+**.

[![CI](https://github.com/anhquoctran/FuzzyToast/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/anhquoctran/FuzzyToast/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/FuzzyToast.svg?style=flat-square&logo=nuget&label=NuGet)](https://www.nuget.org/packages/FuzzyToast)
[![Downloads](https://img.shields.io/nuget/dt/FuzzyToast.svg?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/FuzzyToast)
[![License: MIT](https://img.shields.io/badge/license-MIT-22c55e.svg?style=flat-square)](LICENSE)
[![Stars](https://img.shields.io/github/stars/anhquoctran/FuzzyToast?style=flat-square&logo=github&color=yellow)](https://github.com/anhquoctran/FuzzyToast/stargazers)
[![Last commit](https://img.shields.io/github/last-commit/anhquoctran/FuzzyToast?style=flat-square&logo=github)](https://github.com/anhquoctran/FuzzyToast/commits/master)

[![.NET Framework 4.6.1+](https://img.shields.io/badge/.NET%20Framework-4.6.1%2B-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Windows 10/11](https://img.shields.io/badge/OS-Windows%2010%20%2F%2011-0078D6?style=flat-square&logo=windows&logoColor=white)](#prerequisites)
[![WinForms](https://img.shields.io/badge/UI-Windows%20Forms-5C2D91?style=flat-square)](#prerequisites)
[![Coverage](https://img.shields.io/badge/coverage-%E2%89%A595%25-brightgreen?style=flat-square)](#tests--coverage)
[![Tests](https://img.shields.io/badge/tests-xUnit-2e8b57?style=flat-square)](#tests--coverage)
[![Changelog](https://img.shields.io/badge/changelog-3.0.3-0ea5e9?style=flat-square)](CHANGELOG.md)

**FuzzyToast 3.x** adds inputable toasts; 2.x brought an instance-based manager, fluent builder, four-corner stacking, touchable UI metrics, and capacity policies — without polluting the `System.*` namespace.

## Prerequisites

| Requirement | Detail |
|-------------|--------|
| **OS** | **Windows 10** (1809 / build 17763+) or **Windows 11** |
| **Runtime** | **.NET Framework 4.6.1+** or **.NET 8+** (`net461` / `net48` / `net8.0-windows` / `net9.0-windows`, …) |
| **UI stack** | **Windows Forms** only (`UseWindowsForms`) |
| **Threading** | Call `Toast.Build` / `Show` from the UI thread, or from a background thread (library marshals to UI) |

### Host app DPI (recommended)

For crisp toasts on high-DPI monitors, configure the **host** WinForms app:

```csharp
[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize(); // .NET 6+
    Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
    Application.Run(new MainForm());
}
```

Or in the project file:

```xml
<ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>
```

FuzzyToast scales toast size/margins from the owner control’s `DeviceDpi` (96 DPI baseline).

## Install

```bash
dotnet add package FuzzyToast
```

Package Manager Console:

```powershell
Install-Package FuzzyToast
```

Or build from source:

```bash
dotnet build EasyToast.slnx -c Release
```

Output: `src/FuzzyToast/bin/Release/net8.0-windows/FuzzyToast.dll`

### Repository layout

```text
src/FuzzyToast/          # Library (public API, enums, layout, internal UI)
samples/EasyToastDemo/   # WinForms demo app
tests/FuzzyToast.Tests/  # xUnit + Coverlet (≥95% line)
docs/                    # GitHub Pages site (https://anhquoctran.github.io/FuzzyToast/)
scripts/                 # Coverage helper script
```

## Quick start (Android-style API)

Same call style as classic toast libraries / the original FuzzyToast API:

```csharp
using FuzzyToast;

// Simplest
Toast.Build(this, "Hello, I am Toast!").Show();

// Caption + description
Toast.Build(this, "Hello, I am Toast!", "Details go here…").Show();

// Duration (LENGTH_SHORT / LENGTH_LONG — Android-style names)
Toast.Build(this, "Saved", Duration.LENGTH_SHORT).Show();

// Animation
Toast.Build(this, "Sliding in", Animation.SLIDE).Show();

// Thumbnail
Toast.Build(this, "With image", myImage).Show();

// Optional fluent extras (theme / position) then Show
Toast.Build(this, "Success")
    .SetTheme(ToastTheme.SuccessDark)
    .SetPosition(ToastPosition.TopRight)
    .Show();

// Events + metadata / ext data on click
var toast = Toast.Build(this, "Order #42 ready", "Tap to open")
    .SetTag(orderDto)                          // any object
    .SetData(orderDto)                         // alias of SetTag
    .SetExtData("orderId", 42)                 // key/value metadata
    .SetMetadata("source", "kitchen");

toast.OnClick += (_, e) =>
{
    var order = e.Tag as OrderDto;
    var id = e.GetMetadata<int>("orderId");    // or e["orderId"]
    var source = e.Metadata["source"];
    // …
};
toast.Show();
```

`Toast.Build` uses a shared per-form manager under the hood (layout, stacking, capacity).  
Advanced hosts can still use `ToastManager` + `Create()` explicitly if they need custom options/events.

### Inputable toast (v3)

Quick text input with Submit (or Enter). **Stays open until the user acts** (Submit / Esc / ✕) by default:

```csharp
var toast = Toast.Build(this, "Quick reply", "Type a short note")
    .EnableInput(placeholder: "Your message…", submitButtonText: "Send")
    // .SetDurationMs(120_000)  // optional safety timeout (ms); 0 = no auto-dismiss
    .SetExtData("action", "reply");

toast.OnSubmit += (_, e) =>
{
    var text = e.InputText;           // user text
    var action = e.GetMetadata<string>("action");
    // …
};
toast.Show();
```

- Default: **no auto-dismiss** (`DurationMs = 0`) so the toast does not vanish while typing
- Optional timeout: `.SetDurationMs(120000)` (countdown pauses while the input is focused)
- Escape / ✕ closes without submit; empty submit blocked unless `allowEmptySubmit: true`
- Input toasts take focus so you can type immediately

## Features

| Feature | Notes |
|---------|--------|
| **Android-style API** | `Toast.Build(this, "…").Show()` |
| **Windows 10/11** | Taskbar-aware `WorkingArea`, owner monitor, no focus steal |
| **DPI-aware layout** | Scales with host `DeviceDpi` (100%–200%+) |
| **Four corners** | `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |
| **Themes** | Dark/Light + semantic light/dark + custom RGB |
| **Capacity** | Max global / per-corner; `DropNewest` / `DropOldest` / `Throw` |
| **Hover pause** | Remaining time preserved (not full reset) |
| **Touchable UI** | 44×44 close target, 420×140 @ 96 DPI, content padding |
| **UI-thread safe** | Background `Show` marshaled to the owner form thread |

## Manager options

```csharp
var manager = new ToastManager(this, new ToastManagerOptions
{
    MaxToasts = 6,
    MaxToastsPerPosition = 3,
    OverflowPolicy = ToastOverflowPolicy.DropNewest,
    ShortDurationMs = 2000,
    LongDurationMs = 3000,
    PauseOnHover = true,
    PlaySound = true,
    HideImagePanelWhenEmpty = true
});
```

## Migration from 1.x

See [docs/migration.md](docs/migration.md) or the [GitHub Pages site](https://anhquoctran.github.io/FuzzyToast/).

**Breaking:** namespace is now `FuzzyToast` (not `System.UI.Widget`). Use `ToastManager` + `Create()` instead of static `Toast.Build(...)`.

## Tests & coverage

```bash
dotnet test EasyToast.slnx
dotnet run --project samples/EasyToastDemo
```

Line coverage is measured with **Coverlet** and must stay **≥ 95%** (designer/generated files excluded):

```powershell
# Windows PowerShell
./scripts/test-coverage.ps1
```

Or:

```bash
dotnet test tests/FuzzyToast.Tests/FuzzyToast.Tests.csproj -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=TestResults/coverage \
  /p:ExcludeByFile="**/Properties/**/*.cs%2c**/*Designer.cs" \
  /p:Include="[FuzzyToast]*" \
  /p:Threshold=95 \
  /p:ThresholdType=line
```

Report: `TestResults/coverage.cobertura.xml`

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for build/test/coverage steps and [SECURITY.md](SECURITY.md) to report vulnerabilities privately.

## License

MIT — see [LICENSE](LICENSE).
