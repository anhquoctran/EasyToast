---
title: Getting started
---

# Getting started

[Home](index.md) · [Getting started](getting-started.md) · [API reference](api-reference.md) · [Migration](migration.md) · [Design](design.md)

## Install

```bash
dotnet add package FuzzyToast
```

Package Manager Console:

```powershell
Install-Package FuzzyToast
```

Target **.NET Framework 4.6.1+** or **.NET 8+** Windows Forms.

## First toast

```csharp
using FuzzyToast;

Toast.Build(this, "Hello, I am Toast!").Show();
Toast.Build(this, "Saved", "All changes written.").Show();
Toast.Build(this, "Sliding in", Animation.SLIDE).Show();
```

`this` must be a `Control` (typically your `Form`).

## Theme, position, events

```csharp
var toast = Toast.Build(this, "Order #42 ready", "Tap to open")
    .SetTheme(ToastTheme.SuccessDark)
    .SetPosition(ToastPosition.TopRight)
    .SetMuting(true)
    .SetTag(orderDto)
    .SetMetadata("orderId", 42);

toast.OnClick += (_, e) =>
{
    var id = e.GetMetadata<int>("orderId");
};
toast.Show();
```

## Inputable toast

Stays open until Submit, Escape, or close:

```csharp
var toast = Toast.Build(this, "Quick reply", "Type a short note")
    .EnableInput(placeholder: "Your message…", submitButtonText: "Send");

toast.OnSubmit += (_, e) =>
{
    var text = e.InputText;
};
toast.Show();
```

## Manager (optional)

```csharp
var manager = new ToastManager(this, new ToastManagerOptions
{
    MaxToasts = 6,
    MaxToastsPerPosition = 3,
    OverflowPolicy = ToastOverflowPolicy.DropNewest,
    PauseOnHover = true,
    PlaySound = true
});

manager.Create().SetCaption("Hi").SetMuting(true).Show();
```

## High DPI (host app)

```xml
<ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>
```

On .NET Framework 4.6.1, set DPI awareness in the application manifest instead.

## Limits

Content is bounded so a toast cannot become a resource bomb. See `ToastLimits` (caption, description, metadata, image size) in the [API reference](api-reference.md).
