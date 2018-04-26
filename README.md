# EasyToast v1.0
A simple Toast Nofification library for Windows Forms

EasyToast allows you to build, custom and display push notification like Windows 8/8.1/10 Toast Notification in Windows Forms Application. It's highly configurable with set of built-in options like positions, image, duration and many others. It's extendable, it gives you possibility to create custom and interactive notifications in simply manner.

## Demo
### Some demo:

## Installation:
#### Prerequisites:
- .NET Framework 4.0 or later
- Visual Studio 2015 or later (to build from source)
- Windows Forms Application only

#### Via NuGet:

You can install from `NuGet Package Manager Console`. In this case, all dependencies will be installed automatically.
```powershell
Install-Package EasyToast
```
#### Via direct download:
You can download latest stable release from [here](https://github.com/anhquoctran/EasyToast/releases).
#### Build from Source code:
You need Visual Studio 2015 or later to build source code to `dll` file.  
First, clone this source code or download from Git:
```
git clone https://github.com/anhquoctran/EasyToast.git
```
Open Solution that you cloned in Visual Studio. After cloned or downloaded, open project in Visual Studio and restore NuGet packages if needed then build it.  
After build. all you need is `dll` file in `/bin/Release` or `bin/Debug` folders.

## Usage:
#### Basic usage
First, you need add our namespace `System.UI.Widget`
```csharp
using System.UI.Widget;
```
And then, get started to build a simplest toast popup, all in one line only:
```csharp
Toast.Build(this, "Hello, I am Toast!").Show();
```
If you have image thumbnail, puts it on Toast like this:  
```csharp
var image = Image.FromFile("D:\\thumbnail.jpg");
Toast.Build(this, "Hello, I am Toast!", image).Show();
```
***Warning:***
- If you want to thumbnail best fixed for display, I highly recommended minmum size of image is 80x80, square rectangle.  
- Only JPEG and PNG format are supported
