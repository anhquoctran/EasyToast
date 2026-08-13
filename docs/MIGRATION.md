---
title: Migration
---

# Migrating to FuzzyToast 2.0

[Home](index.md) · [Getting started](getting-started.md) · [Design](design.md)

## Summary

| v1 | v2 |
|----|----|
| `using System.UI.Widget` / `System.Enums` | `using FuzzyToast` |
| `Toast.Build(this, "…").Show()` | **Unchanged** — still `Toast.Build(this, "…").Show()` |
| `Duration.LENGTH_SHORT` / `LENGTH_LONG` | **Kept** (also `Short` / `Long`) |
| `Animation.FADE` / `SLIDE` | **Kept** (also `Fade` / `Slide`) |
| Static global collection | Per-owner manager (automatic for `Build`) |
| `ShowAsync(): void` | `Task ShowAsync()` — completes when **shown** |
| `CloseStye` | `CloseStyle` |
| `Position.TopRight` | `ToastPosition.TopRight` (+ TopLeft, BottomLeft) |
| `Theme.*` | `ToastTheme.*` (+ `.SetTheme(...)` on Toast) |
| `Cancel()` throws if not shown | `Cancel()` / `Dismiss()` no-op if not visible |
| Silent max-toast drop | Capacity policy (reject / drop oldest / throw) |

## Typical code — no change to create style

```csharp
// Still valid in 2.0
Toast.Build(this, "Hello").Show();
Toast.Build(this, "Hello", "Description").Show();
Toast.Build(this, "Hello", Duration.LENGTH_LONG).Show();
Toast.Build(this, "Hello", Animation.SLIDE).Show();
```

Namespace only:

```csharp
// was: using System.UI.Widget; using System.Enums;
using FuzzyToast;
```

## Optional advanced API

```csharp
// Explicit manager (custom capacity, events) — optional
var manager = new ToastManager(this, new ToastManagerOptions { MaxToasts = 4 });
manager.Create().SetCaption("Hi").Show();
```

## Async

```csharp
await Toast.Build(this, "Hi").ShowAsync(); // completes when shown
// or:
var toast = Toast.Build(this, "Hi");
await toast.ShowAsync();
await toast.Handle!.WhenDismissed; // wait until dismissed
```

## Package version

- NuGet package id remains **`FuzzyToast`**
- Assembly version **2.0.0**
