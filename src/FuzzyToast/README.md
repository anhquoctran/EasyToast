# FuzzyToast (library)

Windows Forms toast notification library for **.NET 8+** on **Windows 10/11**.

## Folder map

| Path | Contents |
|------|----------|
| `Api/` | Public surface: `Toast`, `ToastManager`, options, builder, handle, themes, image validation |
| `Enums/` | `Animation`, `Duration`, `CloseStyle`, `ToastPosition`, `ToastTheme`, overflow / handle state |
| `Layout/` | Pure positioning & capacity (`ToastLayoutEngine`, `CapacityPolicy`, metrics) |
| `Internal/` | Non-public runtime helpers (DPI, marshal, registry, screen) |
| `Internal/Ui/` | `ToastForm` (WinForms view) |
| `Properties/` | Assembly info & embedded resources designer |
| `Resources/` | Icons, notification sound |

Namespaces remain under `FuzzyToast` / `FuzzyToast.Internal` / `FuzzyToast.Layout` (folders are for organization only).
