---
title: API reference
---

# API reference

[Home](index.md) · [Getting started](getting-started.md) · [API reference](api-reference.md) · [Migration](migration.md) · [Design](design.md)

Public surface of **FuzzyToast 3.0.3** (`net461` + `net8.0-windows`). Internal types in `FuzzyToast.Internal` (except `AutoDismissTimerState`) are not listed.

**Namespaces:** `FuzzyToast` · `FuzzyToast.Layout` · `FuzzyToast.Internal` (`AutoDismissTimerState` only)

- [Toast](#toast)
- [ToastBuilder](#toastbuilder)
- [ToastManager](#toastmanager)
- [ToastHandle](#toasthandle)
- [ToastOptions](#toastoptions)
- [ToastManagerOptions](#toastmanageroptions)
- [Events](#events)
- [ColorScheme & ThemeCatalog](#colorscheme--themecatalog)
- [Enums](#enums)
- [ToastLimits](#toastlimits)
- [ImageValidation](#imagevalidation)
- [Layout](#layout)
- [AutoDismissTimerState](#autodismisstimerstate)

---

## Toast

`public sealed class Toast`

Android-style entry point: `Toast.Build(owner, "Hello").Show()`. Uses the per-owner `ToastManager` (created on first use).

**Owner** must be a `System.Windows.Forms.Control` (typically your `Form`). A bare `IWin32Window` that is not a `Control` throws `ArgumentException`. A disposed control throws `ObjectDisposedException`.

### Factory — `Build`

All overloads return a `Toast` that is **not shown** until `Show()` / `ShowAsync()`.

| Signature | Sets |
|-----------|------|
| `Build(IWin32Window window, string caption)` | Caption |
| `Build(window, caption, string description)` | + Description |
| `Build(window, caption, Duration duration)` | + Duration |
| `Build(window, caption, Duration duration, Animation animation)` | + Duration, Animation |
| `Build(window, caption, string description, Duration duration)` | + Description, Duration |
| `Build(window, caption, Animation animation)` | + Animation |
| `Build(window, caption, Animation animation, Duration duration, bool muting)` | + Animation, Duration, mute |
| `Build(window, caption, bool muting)` | + mute |
| `Build(window, caption, Image thumbnail)` | + Thumbnail |
| `Build(window, caption, Image thumbnail, Duration duration)` | + Thumbnail, Duration |
| `Build(window, caption, Image thumbnail, Duration duration, Animation animation)` | + Thumbnail, Duration, Animation |
| `Build(window, caption, Image thumbnail, Duration duration, Animation animation, bool muting)` | + all of the above |
| `Build(window, caption, ToastTheme theme)` | + Theme |

### Properties

| Member | Type | Description |
|--------|------|-------------|
| `Guid` | `string` | Id after show; empty before `Show`. |
| `Handle` | `ToastHandle?` | Handle from the last show; `null` if not shown. |
| `Caption` | `string` | Title (required when shown). |
| `Description` | `string` | Secondary line (trimmed). |
| `Duration` | `Duration` | Preset length. Overridden by `SetDurationMs`. |
| `Animation` | `Animation` | Fade (default) or Slide. |
| `Position` | `ToastPosition` | Corner stack (default `BottomRight`). |
| `Theme` | `ToastTheme` | Built-in palette. |
| `IsMuted` | `bool` | Skip notification sound. |
| `Thumbnail` | `Image?` | Left image. |
| `Tag` | `object?` | Arbitrary payload (`SetTag` / `SetData`). |
| `Metadata` | `IReadOnlyDictionary<string, object?>` | Key/value bag. |

### Events

| Event | Args | When |
|-------|------|------|
| `OnClick` | `ToastInteractionEventArgs` | Body click. |
| `OnHover` | `ToastInteractionEventArgs` | Pointer enter. |
| `OnSubmit` | `ToastSubmittedEventArgs` | Input submit (before dismiss). |
| `OnClosed` | `EventArgs` | After dismiss. |

### Fluent setters

Each setter returns `this`.

| Method | Description |
|--------|-------------|
| `SetCaption(string)` | Title. `null` → empty. |
| `SetDescription(string?)` | Secondary text; trimmed. `null` clears. |
| `SetDuration(Duration)` | Preset duration. |
| `SetAnimation(Animation)` | Fade / Slide. |
| `SetPosition(ToastPosition)` | Corner. |
| `SetTheme(ToastTheme)` | Built-in theme. |
| `SetCustomColors(Color background, Color foreground)` | Sets `Theme = Custom`. |
| `SetCloseStyle(CloseStyle)` | How the user can close. |
| `SetMuting(bool muted = true)` | Mute sound. |
| `SetThumbnail(Image? image, bool ownsImage = false)` | Left image. `ownsImage: true` disposes it on close. |
| `SetTag(object?)` | Payload → `e.Tag`. |
| `SetData(object?)` | Alias of `SetTag`. |
| `SetMetadata(string key, object? value)` | One key. Empty key throws; key longer than `ToastLimits.MaxMetadataKeyLength` throws. |
| `SetMetadata(IEnumerable<KeyValuePair<string, object?>>)` | Merge; blank keys skipped. |
| `SetExtData(...)` | Aliases of `SetMetadata`. |
| `EnableInput(placeholder, defaultText, submitButtonText, allowEmptySubmit)` | Text box + submit. Default `DurationMs = 0` (stay open). |
| `SetInputable(bool enabled = true)` | Toggle input without resetting other input fields. |
| `SetDurationMs(int milliseconds)` | Explicit ms. `0` = no auto-dismiss. Negative throws. |

### Show / dismiss

| Method | Description |
|--------|-------------|
| `Show()` | Show on the UI thread. Validates options. |
| `ShowAsync(CancellationToken = default)` | Completes when **shown or rejected**, not when dismissed. Cancel after show dismisses the toast. |
| `Cancel()` | Dismiss if visible. No-op otherwise (does not throw). |
| `Dismiss()` | Same as `Cancel()`. |

---

## ToastBuilder

`public sealed class ToastBuilder`

From `ToastManager.Create()`. Same fluent surface as `Toast` (except `Build` overloads). Constructor is internal.

| Method | Returns | Description |
|--------|---------|-------------|
| `SetCaption` / `SetDescription` / `SetDuration` / `SetAnimation` / `SetPosition` / `SetTheme` | `ToastBuilder` | Same meaning as on `Toast`. |
| `SetCustomColors(Color, Color)` | `ToastBuilder` | Custom RGB. |
| `SetCustomColors(ColorScheme)` | `ToastBuilder` | Custom scheme. `null` throws. |
| `SetCloseStyle` / `SetMuting` / `SetThumbnail` / `SetTag` / `SetData` | `ToastBuilder` | Same as `Toast`. |
| `SetMetadata` / `SetExtData` | `ToastBuilder` | Same as `Toast`. |
| `EnableInput(...)` / `SetInputable` / `SetDurationMs` | `ToastBuilder` | Input + timeout. |
| `Build()` | `ToastOptions` | Snapshot; does not show. |
| `Show()` | `ToastHandle` | Validate + show. |
| `ShowAsync(CancellationToken)` | `Task<ToastHandle>` | Completes when shown or rejected. |

```csharp
manager.Create()
    .SetCaption("Saved")
    .SetTheme(ToastTheme.SuccessDark)
    .Show();
```

---

## ToastManager

`public sealed class ToastManager : IDisposable`

One manager per owner form. `Toast.Build` reuses the instance registered for that owner.

### Constructor

```csharp
public ToastManager(Control owner, ToastManagerOptions? options = null)
```

- `owner` must not be `null` or disposed.
- Disposing `owner` disposes the manager.
- Registers itself so `Toast.Build(owner, …)` shares the same stack.

### Properties

| Member | Type | Description |
|--------|------|-------------|
| `Owner` | `Control` | Host control. Throws if constructed in test mode without an owner. |
| `Options` | `ToastManagerOptions` | Capacity, duration, layout defaults. |
| `IsDisposed` | `bool` | After `Dispose()`. |
| `ActiveToasts` | `IReadOnlyList<ToastHandle>` | Visible handles (snapshot). |
| `Count` | `int` | Visible toast count. |

### Events

| Event | Args | When |
|-------|------|------|
| `ToastAdded` | `ToastChangedEventArgs` | After a toast is shown. |
| `ToastRemoved` | `ToastChangedEventArgs` | After dismiss / capacity victim / dispose. |
| `CollectionCleared` | `EventArgs` | Last toast gone, or manager disposed. |
| `ToastRejected` | `ToastRejectedEventArgs` | `DropNewest` rejected the incoming toast. |

### Methods

| Method | Description |
|--------|-------------|
| `Create()` | New `ToastBuilder`. |
| `Show(ToastOptions)` | Validate + show. Returns visible or rejected handle. Throws if disposed, options invalid, or policy is `Throw`. |
| `ShowAsync(ToastOptions, CancellationToken)` | Same, async. Token cancel after show dismisses. |
| `DismissAll()` | Begin dismiss on every visible toast. No-op if disposed. |
| `Dispose()` | Dismiss remaining toasts, unregister owner. Idempotent. |

---

## ToastHandle

`public sealed class ToastHandle : IDisposable`

Live handle for one show attempt (or a rejected show).

| Member | Type | Description |
|--------|------|-------------|
| `Id` | `string` | Hex GUID without dashes. |
| `Options` | `ToastOptions` | Snapshot used for this toast. |
| `State` | `ToastHandleState` | `Visible` / `Dismissed` / `RejectedCapacity`. |
| `IsVisible` | `bool` | `State == Visible`. |
| `IsDismissed` | `bool` | Closed. |
| `WasRejected` | `bool` | Never shown (capacity). |
| `WhenDismissed` | `Task` | Completes on dismiss. For rejected handles, already completed (`RanToCompletion`). |
| `SubmittedText` | `string?` | Last submitted input, if any. |
| `Clicked` | event | Body click. |
| `Hovered` | event | Pointer hover. |
| `Submitted` | event | Input submit (before dismiss). |
| `Dismissed` | event | Transition to `Dismissed` (not raised for rejected handles). |
| `Dismiss()` | void | Close if visible; no-op otherwise. |
| `Cancel()` | void | Obsolete alias of `Dismiss()`. |
| `Dispose()` | void | Dismiss + detach. Idempotent. |

---

## ToastOptions

`public sealed class ToastOptions`

Immutable configuration (`init` properties). Validated by `ToastManager.Show`.

| Property | Default | Notes |
|----------|---------|--------|
| `Caption` | `""` | Required, ≤ `ToastLimits.MaxCaptionLength`. |
| `Description` | `""` | Optional, ≤ `MaxDescriptionLength`. |
| `Duration` | `Short` | Ignored if `DurationMs` is set. |
| `Animation` | `Fade` | |
| `Position` | `BottomRight` | |
| `Theme` | `Dark` | `Custom` requires `CustomColors`. |
| `CustomColors` | `null` | |
| `CloseStyle` | `ButtonAndClickEntire` | |
| `IsMuted` | `false` | |
| `Thumbnail` | `null` | Size must be within `ToastLimits`. |
| `OwnsThumbnail` | `false` | Dispose image on close. |
| `Tag` | `null` | Arbitrary payload. |
| `Metadata` | empty | ≤ `MaxMetadataEntries` keys. |
| `EnableInput` | `false` | Text box + submit. |
| `InputPlaceholder` | `""` | |
| `InputDefaultText` | `""` | |
| `SubmitButtonText` | `"OK"` | Required when `EnableInput`. |
| `AllowEmptySubmit` | `false` | |
| `DurationMs` | `null` | `null` = preset; `0` = stay open; `> 0` = ms timeout. |

### Methods

| Method | Description |
|--------|-------------|
| `Validate()` | Throws `ArgumentException` / `ArgumentOutOfRangeException` if limits or invariants fail. |
| `FreezeMetadata(IEnumerable<KeyValuePair<string, object?>>?)` | Immutable copy. Skips blank/oversize keys; caps entry count. `null` → empty. |

---

## ToastManagerOptions

`public sealed class ToastManagerOptions`

Manager-wide defaults (`init`).

| Property | Default | Description |
|----------|---------|-------------|
| `MaxToasts` | `6` | Global visible cap. |
| `MaxToastsPerPosition` | `3` | Per-corner cap. |
| `OverflowPolicy` | `DropNewest` | When a cap is hit. |
| `ShortDurationMs` | `2000` | `Duration.Short`. |
| `LongDurationMs` | `3000` | `Duration.Long`. |
| `InputDurationMs` | `300000` | Input / `Duration.Input` when `DurationMs` is unset (5 minutes). |
| `InputExtraHeight` | `36` | Extra row height (used if `InputToastHeight` is not driving height). |
| `InputToastHeight` | `132` | Total height for inputable toasts (96 DPI). |
| `HorizontalMargin` | `12` | From working-area left/right. |
| `VerticalMargin` | `10` | From working-area top/bottom. |
| `ToastWidth` | `380` | 96 DPI width. |
| `ToastHeight` | `100` | 96 DPI height (non-input). |
| `StackGap` | `10` | Gap between stacked toasts. |
| `PauseOnHover` | `true` | Pause countdown on hover (non-input). |
| `PlaySound` | `true` | Play sound unless the toast is muted. |
| `HideImagePanelWhenEmpty` | `true` | Collapse thumbnail column. |

---

## Events

### ToastChangedEventArgs

| Member | Type | Description |
|--------|------|-------------|
| ctor `(ToastHandle toast)` | | |
| `Toast` | `ToastHandle` | Added or removed handle. |

### ToastRejectedEventArgs

| Member | Type | Description |
|--------|------|-------------|
| ctor `(ToastHandle toast, ToastOptions options, string reason)` | | |
| `Toast` | `ToastHandle` | Rejected handle (`WasRejected`). |
| `Options` | `ToastOptions` | What was not shown. |
| `Reason` | `string` | `MaxToasts` or `MaxToastsPerPosition`. |

### ToastInteractionEventArgs

Click / hover payload.

| Member | Type | Description |
|--------|------|-------------|
| ctor `(ToastHandle handle)` | | `handle` required. |
| `Handle` | `ToastHandle` | Live handle. |
| `ToastId` | `string` | `Handle.Id`. |
| `Options` | `ToastOptions` | Snapshot. |
| `Tag` / `Data` | `object?` | From `SetTag` / `SetData`. |
| `Metadata` | `IReadOnlyDictionary<string, object?>` | |
| `this[string key]` | `object?` | Metadata or `null`. |
| `TryGetMetadata<T>(string key, out T? value)` | `bool` | Exact type or `Convert.ChangeType`. |
| `GetMetadata<T>(string key, T? defaultValue = default)` | `T?` | Value or default. |

### ToastSubmittedEventArgs : ToastInteractionEventArgs

| Member | Type | Description |
|--------|------|-------------|
| ctor `(ToastHandle handle, string inputText)` | | `null` text → empty. |
| `InputText` | `string` | What the user typed. |
| `IsEmpty` | `bool` | Whitespace / empty. |

---

## ColorScheme & ThemeCatalog

### ColorScheme

`public sealed class ColorScheme : IEquatable<ColorScheme>`

| Member | Description |
|--------|-------------|
| `Background` / `Foreground` | `Color` |
| `ColorScheme(Color background, Color foreground)` | |
| `ColorScheme(byte rBg, byte gBg, byte bBg, byte rFg, byte gFg, byte bFg)` | RGB 0–255, background then foreground. |
| `Equals` / `GetHashCode` | Compare ARGB of both colors. |

### ThemeCatalog

```csharp
public static ColorScheme Resolve(ToastTheme theme, ColorScheme? custom = null)
```

`ToastTheme.Custom` requires `custom` or throws `InvalidOperationException`. Unknown enum throws `ArgumentOutOfRangeException`.

---

## Enums

### Duration

| Value | Meaning |
|-------|---------|
| `Short` / `LENGTH_SHORT` | ~2 s (`ShortDurationMs`) |
| `Long` / `LENGTH_LONG` | ~3 s (`LongDurationMs`) |
| `Input` / `LENGTH_INPUT` | Input wait (`InputDurationMs`) unless `DurationMs` is set |

### Animation

| Value | Meaning |
|-------|---------|
| `Fade` / `FADE` | Opacity (default) |
| `Slide` / `SLIDE` | Slide from the screen edge |

### ToastPosition

`TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` (default). Each corner is an independent stack.

### CloseStyle

| Value | Meaning |
|-------|---------|
| `ClickEntire` | Body click dismisses (close button hidden except in input mode) |
| `Button` | Only ✕ dismisses; body click raises `OnClick` |
| `ButtonAndClickEntire` | Either (default) |

Inputable toasts always keep a close button.

### ToastTheme

`Dark`, `Light`, `PrimaryLight`, `SuccessLight`, `WarningLight`, `ErrorLight`, `PrimaryDark`, `SuccessDark`, `WarningDark`, `ErrorDark`, `Custom`.

### ToastHandleState

`Visible`, `Dismissed`, `RejectedCapacity`.

### ToastOverflowPolicy

| Value | Meaning |
|-------|---------|
| `DropNewest` | Reject incoming toast; raise `ToastRejected` (default) |
| `DropOldest` | Dismiss victim, then show incoming |
| `Throw` | `InvalidOperationException` |

---

## ToastLimits

`public static class ToastLimits`

| Constant | Value | Applies to |
|----------|-------|------------|
| `MaxCaptionLength` | 1024 | Caption |
| `MaxDescriptionLength` | 4096 | Description |
| `MaxInputTextLength` | 2000 | Placeholder, default text, typed input |
| `MaxSubmitButtonTextLength` | 32 | Submit label |
| `MaxMetadataEntries` | 64 | Metadata map |
| `MaxMetadataKeyLength` | 128 | Metadata keys |
| `MaxDurationMs` | 86_400_000 | Explicit `DurationMs` (24 h) |
| `MaxImageDimension` | 4096 | Thumbnail width/height |
| `MinImageDimension` | 1 | Thumbnail when present |
| `MaxImageFileBytes` | 8 MiB | `ValidateImagePath` |

---

## ImageValidation

`public static class ImageValidation`

Does **not** decode pixels from disk (avoids GDI+ parser bugs).

| Method | Description |
|--------|-------------|
| `ValidateImageSize(Image? image, int minWidth = 64, int minHeight = 64, int maxWidth = MaxImageDimension, int maxHeight = MaxImageDimension)` | Both axes must be in range. `null` / disposed → `false`. |
| `ValidateImagePath(string? path)` | Regular file, PNG/JPEG magic (8 bytes only). Rejects device names, `\\.\`, reparse points, oversize. |
| `IsPng(byte[]?)` / `IsPng(ReadOnlySpan<byte>)` | Signature `89 50 4E 47`. |
| `IsJpeg(byte[]?)` / `IsJpeg(ReadOnlySpan<byte>)` | Signature `FF D8 FF`. |

---

## Layout

Namespace `FuzzyToast.Layout`. Used by the manager; safe to call from tests without a HWND.

### ToastLayoutEngine

| Method | Description |
|--------|-------------|
| `ComputeLocation(ToastPosition position, int stackIndex, ToastLayoutMetrics metrics, ScreenWorkingArea area)` | Top-left of one toast. `stackIndex` 0 = oldest at the corner. |
| `ComputeStack(position, int count, metrics, area)` | Locations for `count` toasts. |

### ToastLayoutMetrics

96-DPI metrics (`init`). Required: `ToastWidth`, `ToastHeight`, `HorizontalMargin`, `VerticalMargin`.

Also: `MinTouchTargetPx`, `CloseButtonSize`, `ThumbnailSize`, content paddings, `CaptionDescriptionGap`, `CaptionMinHeight`, `DescriptionMinHeight`, `StackGap`.

- `Default` — compact 380×100 card.
- `EffectiveStackStride` — `ToastHeight + (StackGap > 0 ? StackGap : VerticalMargin)`.

### CapacityPolicy

```csharp
CapacityDecision Evaluate(
    ToastOverflowPolicy policy,
    int maxToasts,
    int maxToastsPerPosition,
    ToastPosition incomingPosition,
    IReadOnlyList<(string Id, ToastPosition Position)> activeOldestFirstGlobal)
```

Per-corner limit is checked before the global limit. `max*` must be ≥ 1.

**CapacityDecision:** `Action`, `TriggeredBy` (`None` / `PerPosition` / `Global`), `VictimId`, `Reason`.

**CapacityAction:** `Allow`, `RejectNewest`, `RemoveVictimThenAllow`, `Throw`.

### ScreenWorkingArea / LayoutRect / IScreenProvider

| Type | Role |
|------|------|
| `ScreenWorkingArea(Left, Top, Right, Bottom)` | Working area. `Width` / `Height`. |
| `LayoutRect(X, Y, Width, Height)` | Hint rectangle (no WinForms type). |
| `IScreenProvider` | `GetWorkingAreaNear`, `GetRightmostWorkingArea`, `GetLeftmostWorkingArea`. |

---

## AutoDismissTimerState

`public sealed class FuzzyToast.Internal.AutoDismissTimerState`

Pure countdown used by the UI. Pause does **not** reset remaining time.

| Member | Description |
|--------|-------------|
| ctor `(int totalDurationMs)` | Must be ≥ 1. |
| `TotalDurationMs` / `RemainingMs` / `IsPaused` / `IsExpired` | State. |
| `StartOrResume()` | Unpause; returns interval ms (≥ 1). |
| `Pause(int elapsedSinceArmMs)` | Subtract elapsed; second pause is a no-op. |
| `Resume()` | Unpause; returns remaining interval. |
| `OnTimerElapsed()` | Sets remaining to 0. |
