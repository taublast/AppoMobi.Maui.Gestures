# AppoMobi.Maui.Gestures

Library for .Net MAUI to handle gestures. Can be consumed in Xaml and code-behind. A nuget with the same name is available.

This library is used by [DrawnUi for .Net Maui](https://github.com/taublast/DrawnUi.Maui). 

## Features

* Attachable .Net Maui effect
* Multi-touch
* Customizable touch mode for cooperation with other views
* Report velocity, distance, time, etc	
* All data uses pixels on every platform for better precision

## Gestures

* Down/Up
* Tapping
* Longpressing
* Panning
* Rotation
* Zoom
* Mouse wheel on Windows

The philosophy is to have the more platform agnostic code as possible, by processing platform raw input in a shared code. The library is still in development, i am adding more features when i need them myself, so if you feel that something is missing please feel free to leave a message in Discussions or create a PR.

## Installation

Install the package __AppoMobi.Maui.Gestures__ from NuGet.

After that initialize the library in the MauiProgram.cs file:

```csharp
builder.UseGestures();
```

### Basic Usage

Getures are handled by a Maui Effect. You can just attach properties that would invoke your commands or handlers upon a specific gesture.

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
        public void OnGestureEvent(
            TouchActionType type,
            TouchActionEventArgs args,
            TouchActionResult action);

        public bool InputTransparent { get; }
    }
 ```

As you might guess __OnGestureEvent__ will be invoked on every touch detected if your __InputTransparent__ is not returning _True_.

The library passes 2 kinds of gesture type, the "raw" `TouchActionType` and the resulting logical `TouchActionResult` that you most probably would use.

__To note__: Since we detect multi-touch you could receive several Down/Up events, the first one would be recognizable with `NumberOfTouches` being at 1.

You will receive all gesture-related data inside `TouchActionEventArgs args`. 

When you get a `TouchActionResult.Panning` you could also have the property `ManipulationInfo Manipulation` filled with scale and rotation data. Otherwise expect it to be `null`.

The static property `TouchEffect.Density` is used internally to convert pixels/points, you can you it too as all the data you would get will be in pixels, convert it to points/whatever as you please.

An example of usage by a custom control by implementing the `IGestureListener` interface: https://github.com/taublast/DrawnUi.Maui/blob/main/src/Engine/Views/Canvas.cs

#### Hints

For a case of your custom control sitting inside a ScrollView there is a TouchMode property to be played with.
For example you might want to set it to `TouchHandlingStyle.Lock` so that when your control receives the Down event the parent ScrollView stops receiving gestures until we get an Up, so we can Pan our control at will.

#### Tweaks

`public static TouchEffect.TappedWhenMovedThresholdPoints`

How much finger can move between DOWN and UP for the gestured to be still considered as TAPPED. In points, not pixels.

```csharp
TouchEffect.TappedWhenMovedThresholdPoints = 10f;
```
