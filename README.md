# FuzzyToast

A fuzzy Toast Nofification library for Windows Forms

**FuzzyToast** allows you to build, customize and display toast notifications like Windows 8/8.1/10 Toast Notifications in Windows Forms Applications. It's highly configurable with a set of built-in options like positions, image, duration and many others. It's extendable, it gives you the possibility to create custom and interactive notifications simply.

## Demo

### Some demo

## Installation

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 / 2026 or later (to build from source)
- Only Windows Forms Applications are supported

#### Via NuGet

You can install from `NuGet Package Manager Console`. In this case, all dependencies will be installed automatically.

```powershell
Install-Package FuzzyToast
```

#### Via direct download

You can download the latest stable release from [here](https://github.com/anhquoctran/EasyToast/releases).  
Then add the `FuzzyToast.dll` reference to your project.

#### Build from Source code

You need the .NET 8 SDK or Visual Studio 2022+ to build the source code to a `dll` file.  
First, clone this source code or download from Git:

```bash
git clone https://github.com/anhquoctran/EasyToast.git
```

Open the `EasyToast.slnx` solution file that you cloned in Visual Studio. Or, simply use the .NET CLI:

```bash
dotnet build EasyToast.slnx -c Release
```

After building, all you need is the `FuzzyToast.dll` file found in the `/bin/Release/net8.0-windows` folder.

## Usage

### Basic usage

First, you need to add our namespace `System.UI.Widget`

```csharp
using System.UI.Widget;
```

And then, get started to build a simplest toast popup, all in one line only:

```csharp
Toast.Build(this, "Hello, I am Toast!").Show();
```

Adding some description

```csharp
Toast.Build(this, "Hello, I am Toast!", "Description goes here...").Show();
```

If you have an image thumbnail, put it on Toast like this:  

```csharp
var image = Image.FromFile("D:\\thumbnail.jpg");
Toast.Build(this, "Hello, I am Toast!", image).Show();
```

***Warning:***

- If you want the thumbnail best fitted for display, I highly recommend a minimum size of 80x80, square rectangle.  
- Only JPEG and PNG formats are supported  

**Note:** `this` in this case is an instance of `System.Windows.Forms.Form`, where Toast will be created. Example: MainForm,...  

### ToastBuilder

We also provide `ToastBuilder` to create a Toast more powerfully, chaining is supported.

```csharp
private void CreateWithBuilder()
{
  var toast = new ToastBuilder(this) //<-- 'this' is your Form instance
    .SetCaption("Hello! I am Toast")
    .SetDescription("This is demo")
    .SetDuration(Duration.LENGTH_SHORT)
    .SetMuting(false)
    .Build();

  toast.Show();
}
```

#### More features

##### Duration

You can specify duration by using the `Duration` enum. There are two values for this enum.  
`Duration.LENGTH_SHORT` is 2 seconds and `Duration.LENGTH_LONG` is 3 seconds  
Default `Duration` value if you don't set it is `LENGTH_SHORT`  

**Example:**  

```csharp
Toast.Build(this, "Hello, I am Toast!", Duration.LENGTH_LONG).Show();
```

##### Animation

Like `Duration`, `Animation` also has two values: `Fading` and `Sliding`.  
Default is `Fading`

**Example:**

```csharp
Toast.Build(this, "Hello, I am Toast!", Animation.SLIDE).Show();
```

##### Async supports

Toast also supports asynchronous methods for displaying the toast without blocking your code

```csharp
private async void DisplayToastAsync() 
{
  await Toast.Build(this, "Hello! I am Toast!", Duration.LENGTH_SHORT).ShowAsync();
}
```

##### Theme

We provide 8 predefined themes. You can also add your custom theme.  
There are 8 built-in themes:  

- Dark
- Light
- PrimaryLight
- SuccessLight
- WarningLight
- ErrorLight
- PrimaryDark
- SuccessDark
- WarningDark
- ErrorDark

**Example:**

```csharp
Toast.Build(this, "Hello, I am Toast!", Theme.Light).Show();
```

#### More examples and documentation available in [wiki](https://github.com) and our [Official Documentation](/docs/html/index.html)

## License

FuzzyToast is licensed under the [MIT License](https://opensource.org/licenses/MIT).
