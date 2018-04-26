# EasyToast v1.0
A simple Toast Nofification library for Windows Forms

**EasyToast** allows you to build, custom and display push notification like Windows 8/8.1/10 Toast Notification in Windows Forms Application. It's highly configurable with set of built-in options like positions, image, duration and many others. It's extendable, it gives you possibility to create custom and interactive notifications in simply manner.

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
Then adding `EasyToast.dll` reference to your project.
#### Build from Source code:
You need Visual Studio 2015 or later to build source code to `dll` file.  
First, clone this source code or download from Git:
```
git clone https://github.com/anhquoctran/EasyToast.git
```
Open Solution that you cloned in Visual Studio. After cloned or downloaded, open project in Visual Studio and restore NuGet packages if needed then build it.  
After build. all you need is `EasyToast.dll` file in `/bin/Release` or `bin/Debug` folders.

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
Adding some description
```csharp
Toast.Build(this, "Hello, I am Toast!", "Description goes here...").Show();
```
If you have image thumbnail, puts it on Toast like this:  
```csharp
var image = Image.FromFile("D:\\thumbnail.jpg");
Toast.Build(this, "Hello, I am Toast!", image).Show();
```
***Warning:***
- If you want to thumbnail best fixed for display, I highly recommended minmum size of image is 80x80, square rectangle.  
- Only JPEG and PNG format are supported  

**Note:** `this` in this case is your Form, where Toast has been created. Example: MainForm,...  
#### More features
##### Duration
You can specific duration by using `Duration` enum. There are two values of this enum.  
`Duration.LENGTH_SHORT` is 2 seconds and `Duration.LENGTH_LONG` is 3 seconds  
Default `Duration` value if you don't set is `LENGTH_SHORT`  
**Example:**  
```csharp
Toast.Build(this, "Hello, I am Toast!", Duration.LENGTH_LONG).Show();
```
##### Animation
Like `Duration`, `Animation` also have two values `Animation.FADE` and `Animation.SLIDE`.  
Default `Animation` value if you don't set is `FADE`  
**Example:**
```csharp
Toast.Build(this, "Hello, I am Toast!", Animation.SLIDE).Show();
```
##### Theme
Our provided 8 predefined-themes. You can also adding your custom theme.  
There is 8 predefined-themes (built-in themes):  
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
#### More examples and documentation available in [wiki](https://github.com) and our [Officical Docummenation](https://google.com).
## License
EasyToast is licensed under the [GNU General Public License v3 (GPL-3)](http://www.gnu.org/copyleft/gpl.html).
