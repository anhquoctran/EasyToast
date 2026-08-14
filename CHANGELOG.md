# Changelog

## 3.0.5

### Added
- **Multi-Monitor DPI Awareness**: Enhanced layout rendering natively supports `PerMonitorV2`. Toasts now scale automatically and properly when dragged across monitors of different scaling factors.
- **Rich Text Format (RTF) Descriptions**: Added support for Emojis, colors, and font styling via natively generated RTF code directly in `Toast.Description`.
- **Automatic Thread Marshaling**: `Toast.MakeText(...).Show()` is now guaranteed to be 100% thread-safe. Developers can invoke Toasts from background threads (`Task.Run`) without manually using `Invoke` or `BeginInvoke`.

## 3.0.4

### Changed

- Package version **3.0.4**
- Release packaging update for the next NuGet publication

## 3.0.3

### Added

- Multi-target **`net461`** + **`net8.0-windows`** so existing .NET Framework 4.6.1+ WinForms apps can consume FuzzyToast
- `ToastLimits` and stricter `ToastOptions.Validate` / `ImageValidation` guards (caption/metadata size, image dimensions, 8 MiB file cap)

### Security

- Image path checks read **only the 8-byte magic header** (no full-file load / GDI+ decode)
- Reject reserved device names (`CON`, `NUL`, …), `\\.\` device paths, reparse points, and oversized bitmaps
- Input text box `MaxLength` capped; metadata key/entry counts bounded

### Notes

- Package version **3.0.3**

## 3.0.2

### Added

- Tests to keep **line coverage ≥ 95%** (Coverlet threshold raised from 85%)
- GitHub Actions **CI** (Windows, Release build, xUnit, coverage gate) and **Release** workflow (tag `v*` → pack + GitHub Release + NuGet)
- NuGet package icon, SourceLink, and symbol package (`snupkg`)
- Contributor / security docs, issue & PR templates, Dependabot, `.editorconfig`

## 3.0.1

### Changed

- Package project/repository URL → [github.com/anhquoctran/FuzzyToast](https://github.com/anhquoctran/FuzzyToast)
- Demo: Inputable panel layout (no overlap), compact Theme/Close style column
- Inputable toast: stay open until Submit/Esc by default; tighter content padding

### Notes

- Package version **3.0.1**

## 3.0.0

### Added

- **Inputable toast**: `EnableInput()` / `SetInputable()` with text box + Submit button
- `OnSubmit` / `ToastHandle.Submitted` with `ToastSubmittedEventArgs.InputText`
- `Duration.Input` / `LENGTH_INPUT` and `SetDurationMs(int)` for long wait while typing
- `ToastManagerOptions.InputDurationMs` (default 30s) and taller layout for input row
- Enter submits, Escape dismisses; optional `allowEmptySubmit`

### Notes

- Inputable toasts take keyboard focus (unlike normal toasts which avoid activation)
- Package version **3.0.0**

## 2.0.0

### Breaking

- Public namespace moved from `System.UI.Widget` / `System.Enums` to **`FuzzyToast`** (using line only)
- Enum renames: `CloseStyle`, `ToastPosition`, `ToastTheme` (Duration/Animation keep Android-style aliases)
- `ShowAsync` is now awaitable (`Task`, completes on show)

### Preserved (Android-style create API)

- **`Toast.MakeText(owner, …).Show()`** — same create style as 1.x
- `Duration.LENGTH_SHORT` / `LENGTH_LONG`
- `Animation.FADE` / `SLIDE`
- Optional fluent: `.SetTheme()`, `.SetPosition()`, …

### Added

- Four-corner stacking (TopLeft, TopRight, BottomLeft, BottomRight)
- Full fluent `ToastBuilder` (theme, position, animation, close style, custom colors, tag)
- Capacity policy: DropNewest / DropOldest / Throw with `ToastRejected` events
- Pure layout engine + reflow after dismiss
- Hover pause with remaining milliseconds
- Touchable UI defaults (44×44 close, 420×140, content spacing)
- xUnit test project (`FuzzyToast.Tests`)
- No keyboard focus steal (`ShowWithoutActivation`)
- **Windows 10/11 hardening:** DPI scaling from owner `DeviceDpi`, owner-monitor working area (taskbar-aware), UI-thread marshal with handle creation, tool-window / no-activate styles, best-effort animation & sound
- Demo: `PerMonitorV2` high DPI + app.manifest

### Fixed

- Description writing into caption label
- `Contains` self-compare bug
- Custom theme G/B channel swap
- BottomRight stacking used global count
- Image size validation used OR instead of AND
- JPEG EXIF signatures rejected

## 1.0.0

- Initial .NET Framework / later surface port to .NET 8
