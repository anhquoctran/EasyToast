# FuzzyToast

A toast notification library for **Windows Forms** on **.NET 8+**.

**FuzzyToast 2.0** provides an instance-based manager, fluent builder, four-corner stacking, touchable UI metrics, and capacity policies — without polluting the `System.*` namespace.

## Prerequisites

| Requirement | Detail |
|-------------|--------|
| **OS** | **Windows 10** (1809 / build 17763+) or **Windows 11** |
| **Runtime** | **.NET 8 or later** (e.g. net8.0-windows, net9.0-windows) |
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
tests/FuzzyToast.Tests/  # xUnit + Coverlet (≥85% line)
docs/                    # Design & migration notes
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

See [docs/MIGRATION.md](docs/MIGRATION.md).

**Breaking:** namespace is now `FuzzyToast` (not `System.UI.Widget`). Use `ToastManager` + `Create()` instead of static `Toast.Build(...)`.

## Tests & coverage

```bash
dotnet test EasyToast.slnx
dotnet run --project samples/EasyToastDemo
```

Line coverage is measured with **Coverlet** and must stay **≥ 85%** (designer/generated files excluded):

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
  /p:Threshold=85 \
  /p:ThresholdType=line
```

Report: `TestResults/coverage.cobertura.xml`

## License

MIT — see [LICENSE](LICENSE).
