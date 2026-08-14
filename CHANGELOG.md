# Changelog

All notable changes to FuzzyToast will be documented in this file.

## [3.0.4] - 2026

### Added
- **Extension Methods for Form**: Simplified API with `form.ShowToast()` extension methods for common scenarios
- **Enhanced DPI Awareness**: Per-Monitor V2 support for multi-monitor setups with different DPI settings
- **Custom Content Hosting**: `ToastContent` class for embedding rich media, markdown-rendered controls, or any WinForms control
- **Interactive Actions**: `ToastAction` class for adding action buttons to toasts (Undo, Open, Reply, etc.)
- **Toast Grouping & Queue**: `GroupId` property on ToastOptions for grouping related notifications
- **Static ToastService**: `ToastService.Default` static entry point for quick scenarios without explicit manager instances
- **Dark Mode Support**: `EnableDarkModeDetection` option for automatic theme switching on Windows 10/11

### Improved
- **Performance**: Zero-allocation optimizations in rendering loops
- **Security Hardening**: Enhanced image validation and metadata sanitization
- **DPI Scaling**: Better handling of Per-Monitor V2 awareness mode detection

### Changed
- Version bumped from 3.0.3 to 3.0.4

### API Additions
- `ToastExtensions` - Extension methods for Form
- `ToastAction` & `ToastActionStyle` - Interactive action buttons
- `ToastContent` - Custom content hosting
- `ToastService` - Static default manager
- `ToastOptions.GroupId` - Group identifier for queue management
- `ToastOptions.Actions` - List of interactive actions
- `ToastOptions.CustomContent` - Custom embedded content
- `ToastManagerOptions.EnableDarkModeDetection` - Auto dark mode
- `ToastManagerOptions.EnableGrouping` - Enable toast grouping
- `ToastManagerOptions.MaxToastsPerGroup` - Max toasts per group

---

## [3.0.3] - Previous Release
- Security hardening
- .NET Framework 4.6.1 support improvements

---

## [3.0.0] - Inputable Toasts
- Breaking change: Inputable toast support
- Submit button with text validation

---

## [2.0.0] - Four-Corner Stacking
- Namespace changed to FuzzyToast
- Independent stacks per corner

---

## [1.0.0] - Initial Release
- Basic toast notifications for WinForms
