# AppoMobi.Maui.Gestures

Library for .Net MAUI to handle gestures. Can be consumed in Xaml and code-behind. A nuget with the same name is available.

This library is used by [DrawnUi for .Net Maui](https://github.com/taublast/AppoMobi.Maui.DrawnUi.Demo). 

## Features

* Down
* Up
* Tap
* LongPress
* Pan
* Pinch (_windows platform missing_)
* Rotate (_in progress_)

Here and there some properties are still missing, but the main functionality is there.

Some points of interest:

* Customizable touch mode
* Report velocity, distance, time, etc	
* All data uses pixels on every platform for better precision

The philosophy is to have the more platform agnostic code as possible, by processing platform raw input in a shared code.

## Installation

Install the package __AppoMobi.Maui.Gestures__ from NuGet.

After that initialize the library in the MauiProgram.cs file:

```csharp
builder.UseGestures();
```

## Usage

Getures are handled by a Maui Effect. 
It attaches itsself if you add one of its key attachable static properties to a control.

### Basic Usage

You can just attach properties that would invoke your commands or handlers upon a specific gesture.

#### Xaml

```xml
<Label Text="Hello World!" 
	    touch:TouchEffect.CommandLongPressing="{Binding Source={x:Reference ThisPage}, Path=BindingContext.CommandGoToAnotherPage}" 
	    touch:TouchEffect.CommandTapped="{Binding Source={x:Reference ThisPage}, Path=BindingContext.CommandGoToAnotherPage}" 
	    touch:TouchEffect.CommandTappedParameter="{Binding .}" />

```
#### Code behind

```csharp
TouchEffect.SetCommandTapped(tabItem, TabItemTappedCommand);
TouchEffect.SetCommandTappedParameter(tabItem, selectedIndex);
```

### Enhanced Usage

 You can opt for processing gestures on a lower level yourself, especially if you are creating a custom control. First we just attach the effect with a special property:

```xml
    <draw:Canvas
        touch:TouchEffect.ForceAttach="True">
```
 or
```csharp
 TouchEffect.SetForceAttach(myView, true);
```
Now you need to implement a buil-it interface __IGestureListener__ in your custom control:

```csharp
    public interface IGestureListener
    {
        public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action);
        public bool InputTransparent { get; }
    }
 ```

As you might guess __OnGestureEvent__ will be invoked on every touch detected if your __InputTransparent__ is not returning _True_.

#### Hints

For a case of your custom control sitting inside a ScrollView there is a TouchMode property to be played with.
For example you might want to set it to TouchHandlingStyle.Lock so that when your control receives the Down event the parent ScrollView stops receiving gestures until we get an Up, so we can Pan our control at will.

#### Tweaks

`public static TouchEffect.TappedWhenMovedThresholdPoints`

_How much finger can move between DOWN and UP for the gestured to be still considered as TAPPED. In points, not pixels._

```csharp
TouchEffect.TappedWhenMovedThresholdPoints = 10f;
```
