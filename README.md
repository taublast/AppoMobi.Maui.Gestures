# AppoMobi.Maui.Gestures

Library for .NET MAUI to handle gestures. Can be consumed in XAML and code-behind. A NuGet package with the same name is available.

This library is used by [DrawnUI for .NET MAUI](https://github.com/taublast/DrawnUi).

.NET 8 and above compatible.

---

## What's New

* Added `Manual` touch mode for dynamic gesture control with parent controls, allowing true cooperative gesture handling inside ScrollViews and other containers.
* Pointer Data Support: Added rich pointer/mouse event data with `TouchActionEventArgs.Pointer` property
* Windows: Hover mouse/pointer tracking, mouse button detection (up tu 9 buttons), pen pressure, hover tracking
* Android: Hover mouse/pointer tracking, limited mouse button detection
* iOS/MacCatalyst: Hover mouse/pointer tracking, two-fingers scrolling, limited mouse button detection

---

## Features

* ✅ Attachable .NET MAUI effect
* ✅ Multi-touch support (rotation, pinch/zoom)
* ✅ Customizable touch modes for cooperation with parent/child views
* ✅ Rich gesture data: velocity, distance, time, manipulation info
* ✅ Pixel-perfect precision across all platforms
* ✅ Platform-agnostic gesture processing
* ✅ Commands and event handlers support
* ✅ Mouse wheel support on Windows
* ✅ Mouse/touchpad support (limited upon platform)

---

## Installation

1. Install the **AppoMobi.Maui.Gestures** package from NuGet:

```bash
dotnet add package AppoMobi.Maui.Gestures
```

2. Initialize the library in `MauiProgram.cs`:

```csharp
builder.UseGestures();
```

---

## Basic Usage

Gestures are handled by a MAUI Effect. You can attach properties to invoke commands or handlers upon specific gestures.

### XAML

```xml
<ContentPage xmlns:touch="clr-namespace:AppoMobi.Maui.Gestures;assembly=AppoMobi.Maui.Gestures">

    <!-- Simple tap command -->
    <Label Text="Tap Me!"
           touch:TouchEffect.CommandTapped="{Binding TapCommand}" />

    <!-- Multiple gesture handlers with parameter -->
    <Frame touch:TouchEffect.CommandTapped="{Binding ItemTappedCommand}"
           touch:TouchEffect.CommandTappedParameter="{Binding .}"
           touch:TouchEffect.CommandLongPressing="{Binding LongPressCommand}">
        <Label Text="Long press or tap me!" />
    </Frame>

</ContentPage>
```

### Code-Behind

```csharp
// Attach tap command
TouchEffect.SetCommandTapped(myView, TapCommand);
TouchEffect.SetCommandTappedParameter(myView, itemData);

// Attach long press command
TouchEffect.SetCommandLongPressing(myView, LongPressCommand);

// Force attach effect (for manual processing)
TouchEffect.SetForceAttach(myView, true);

// Set touch mode
TouchEffect.SetShareTouch(myView, TouchHandlingStyle.Lock);
```

---

## Gestures

The library supports the following gesture types:

| Gesture | Description | Event/Command |
|---------|-------------|---------------|
| **Down** | Finger touches screen | `Down` event, `DownCommand` |
| **Up** | Finger lifts from screen | `Up` event, `UpCommand` |
| **Tapped** | Quick tap (with velocity threshold) | `Tapped` event, `CommandTapped` |
| **Long Pressing** | Finger held down for extended time | `LongPressing` event, `CommandLongPressing` |
| **Panning** | Finger moves across screen | `Panning` event, `PanningCommand` |
| **Pinched** | Two-finger pinch/zoom | `Pinched` event, `PinchedCommand` |
| **Wheel** | Mouse wheel (Windows) | Wheel event with delta |
| **Rotation** | Two-finger rotation | Multi-touch manipulation data |
| **Wheel** | Mouse wheel (Windows) | Wheel event with delta |
| **Pointer** | Mouse/touchpad related data |


---

## Touch Handling Modes

Touch handling modes control how gestures interact with parent containers like ScrollView, ListView, etc.

```csharp
public enum TouchHandlingStyle
{
    Default,      // Normal behavior
    Lock,         // Lock all input for this control only
    Manual,       // Dynamic control via WIllLock property, used by DrawnUI to work in cooperation with native ScrollView
    Disabled      // Same as InputTransparent = true
}
```

Not sure what mode to use? Start with `Default`.

### About Manual Mode

**Use case**: Controls that need **dynamic control** over parent gesture blocking - the ultimate flexibility! 🎯

Manual mode gives you full control over gesture cooperation by letting you decide at runtime whether to lock or release parent containers using the `WIllLock` property.

```xml
<!-- Horizontal carousel inside vertical ScrollView -->
<ScrollView Orientation="Vertical">
    <VerticalStackLayout>

        <!-- This carousel dynamically controls parent scrolling -->
        <!-- When panning horizontally: blocks vertical scroll -->
        <!-- When not handling: allows vertical scroll -->
        <controls:MyCarousel touch:TouchEffect.ShareTouch="Manual"
                             touch:TouchEffect.ForceAttach="True" />

        <Label Text="More content below..." />

    </VerticalStackLayout>
</ScrollView>
```

**How It Works**:

Manual mode works by:
1. **Always delivering touch events** to your control (so you receive `TouchesEnded` for snapping)
2. **Listening to the `WIllLock` state** YOU set dynamically
3. **Blocking or releasing the parent** based on your control's needs

**The WIllLock Property**:

```csharp
public enum ShareLockState
{
    Initial,   // Starting state, reset on Up/Down
    Locked,    // Block parent - you're consuming the gesture
    Unlocked   // Release parent - you're not consuming
}
```

**Use for custom controls**:
- ✅ Horizontal carousels in vertical ScrollViews (with snapping)
- ✅ Custom sliders that need directional control
- ✅ Pan-to-dismiss gestures alongside scrolling
- ✅ Any control that needs to decide "should I handle this?" at runtime

**Example - Horizontal Carousel with Dynamic Locking**:

```csharp
public class MyCarousel : ContentView, IGestureListener
{
    private PointF _startPoint;
    private bool _isHandling = false;

    public MyCarousel()
    {
        // Enable manual mode for dynamic control
        TouchEffect.SetShareTouch(this, TouchHandlingStyle.Manual);
        TouchEffect.SetForceAttach(this, true);
    }

    public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
    {
        var effect = TouchEffect.GetFrom(this);

        switch (action)
        {
            case TouchActionResult.Down:
                _startPoint = args.Location;
                _isHandling = false;
                // WIllLock is automatically reset to Initial
                break;

            case TouchActionResult.Panning:
                var deltaX = Math.Abs(args.Distance.Delta.X);
                var deltaY = Math.Abs(args.Distance.Delta.Y);

                // Decide: horizontal or vertical gesture?
                if (!_isHandling)
                {
                    if (deltaX > deltaY && deltaX > 5)
                    {
                        // Horizontal pan - WE want control
                        _isHandling = true;
                        effect.WIllLock = ShareLockState.Locked;
                    }
                    else if (deltaY > 5)
                    {
                        // Vertical pan - PARENT should handle
                        effect.WIllLock = ShareLockState.Unlocked;
                        return;
                    }
                }

                if (_isHandling)
                {
                    // Pan the carousel horizontally
                    ScrollBy(args.Distance.Delta.X);
                }
                break;

            case TouchActionResult.Up:
                if (_isHandling)
                {
                    // IMPORTANT: We ALWAYS get Up event in Manual mode!
                    // This is perfect for snapping behavior
                    SnapToNearestItem();
                }
                // WIllLock is automatically reset to Initial
                break;
        }
    }
}
```


---

## Mouse and Pen Support

The library provides mouse and pen support with button tracking, hover detection, and pressure sensitivity, where platform allows that.

### Platform Support

- **Windows**: Full pointer support, up to 8 buttons wheel, pen pressure, hover tracking
- **Android**:  Basic support (Left/Right click, hover tracking)
- **iOS/MacCatalyst**: Basic support (Left click, 2-fingers scrolling, hover tracking, Apple Pencil detection)

### Mouse Button Events

When `args.Pointer != null`, you have access to information:

```csharp
public class PointerData
{
    // Button that triggered this event (for press/release)
    public MouseButton Button { get; set; }

    // Button number (1-based): 1=Left, 2=Right, 3=Middle, 4+=Extended
    public int ButtonNumber { get; set; }

    // Button state (Pressed/Released)
    public MouseButtonState State { get; set; }

    // Detected pressed buttons (flags)
    public MouseButtons PressedButtons { get; set; }

    // Device type (Mouse/Pen/Touch)
    public PointerDeviceType DeviceType { get; set; }

    // Pressure (0.0-1.0 for pen, 1.0 for mouse)
    public float Pressure { get; set; }
}
```

 
 ---

## Advanced Usage

### Manual Gesture Processing

For custom controls that need low-level gesture processing:

```xml
<controls:MyCustomControl touch:TouchEffect.ForceAttach="True" />
```

### IGestureListener Interface

Implement this interface in your custom control:

```csharp
public class MyCustomControl : ContentView, IGestureListener
{
    public MyCustomControl()
    {
        // Attach the effect
        TouchEffect.SetForceAttach(this, true);
        TouchEffect.SetShareTouch(this, TouchHandlingStyle.Manual);
    }

    public void OnGestureEvent(
        TouchActionType type,           // Raw platform gesture type
        TouchActionEventArgs args,      // Gesture data
        TouchActionResult action)       // Processed gesture result
    {
        switch (action)
        {
            case TouchActionResult.Down:
                // First finger down
                if (args.NumberOfTouches == 1)
                {
                    _startPoint = args.Location;
                }
                break;

            case TouchActionResult.Panning:
                // Finger moving
                var deltaX = args.Distance.Delta.X;
                var deltaY = args.Distance.Delta.Y;
                var velocityX = args.Distance.Velocity.X;

                // Check for multi-touch manipulation
                if (args.Manipulation != null)
                {
                    var scale = args.Manipulation.Scale;
                    var rotation = args.Manipulation.Rotation;
                }
                break;

            case TouchActionResult.Tapped:
                // Quick tap detected
                ExecuteTapAction();
                break;

            case TouchActionResult.LongPressing:
                // Long press detected
                ShowContextMenu();
                break;
        }
    }

    public bool InputTransparent => false;
}
```

### Multi-Touch Support

When multiple fingers are detected:

```csharp
public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
{
    // Check number of touches
    if (args.NumberOfTouches > 1)
    {
        // Multi-touch gesture
        if (args.Manipulation != null)
        {
            // Scale (pinch/zoom)
            var scale = args.Manipulation.Scale;          // Current frame scale change
            var totalScale = args.Manipulation.ScaleTotal; // Total scale from start

            // Rotation
            var rotation = args.Manipulation.Rotation;          // Current frame rotation (degrees)
            var totalRotation = args.Manipulation.RotationTotal; // Total rotation from start

            // Center point of gesture
            var centerX = args.Manipulation.Center.X;
            var centerY = args.Manipulation.Center.Y;

            // Number of touches involved
            var touchCount = args.Manipulation.TouchesCount;
        }
    }
}
```

---

## Configuration

### Static Properties

Configure global behavior using static properties:

```csharp
// Tapped velocity threshold (in points, not pixels)
TouchEffect.TappedVelocityThresholdPoints = 200f; // Default: 200

// Long press duration (in milliseconds)
TouchEffect.LongPressTimeMsDefault = 1500; // Default: 1500ms

// Enable logging for debugging
TouchEffect.LogEnabled = true;

// Density for pixel/point conversion (auto-detected)
var density = TouchEffect.Density;
```

### Instance Properties

Configure individual effect instances:

```csharp
var effect = new TouchEffect
{
    TouchMode = TouchHandlingStyle.Manual,
    LongPressTimeMs = 2000,          // Custom long press time
    Draggable = true,                // Don't cancel when moved outside
    Capture = true                   // Capture pointer
};
```

---

## Best Practices

### 1. Choose the Right Touch Mode

```csharp
// ✅ Horizontal control in vertical ScrollView with dynamic control
TouchEffect.SetShareTouch(slider, TouchHandlingStyle.Manual);

// ✅ Drawing canvas that needs all gestures
TouchEffect.SetShareTouch(canvas, TouchHandlingStyle.Lock);

// ✅ Simple tap button
TouchEffect.SetShareTouch(button, TouchHandlingStyle.Default);
```

### 2. Use Pixel Values Consistently

All gesture data is in pixels for precision. Convert to points when needed:

```csharp
var density = TouchEffect.Density;
var pointsX = pixelsX / density;
var pointsY = pixelsY / density;
```

### 3. Handle Multi-Touch Correctly

Always check `NumberOfTouches` and `Manipulation` property:

```csharp
public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
{
    // Single touch
    if (args.NumberOfTouches == 1)
    {
        HandleSingleTouch(args);
    }
    // Multi-touch with manipulation data
    else if (args.Manipulation != null)
    {
        HandleMultiTouch(args.Manipulation);
    }
}
```


---

## Tune Up

### Long press duration

Adjust the long press timing:

```csharp
TouchEffect.LongPressTimeMsDefault = 1000; // 1 second instead of 1.5
```

### Close keyboard (Android)

```csharp
TouchEffect.CloseKeyboard();
```

### Check Tap Lock

Prevent accidental double-taps with built-in locking:

```csharp
// Check if locked
if (TouchEffect.CheckLocked("myButton"))
{
    return; // Skip this tap
}

// Lock and check in one call (locks for 500ms)
if (TouchEffect.CheckLockAndSet("myButton", ms: 500))
{
    return; // Was locked, skip
}
// Not locked, proceed with action
```

---

## Contributing

The library is actively developed. If you need additional features or find bugs:

1. 💬 Start a [Discussion](https://github.com/taublast/AppoMobi.Maui.Gestures/discussions)
2. 🐛 Open an [Issue](https://github.com/taublast/AppoMobi.Maui.Gestures/issues)
3. 🔧 Submit a [Pull Request](https://github.com/taublast/AppoMobi.Maui.Gestures/pulls)

---

Used by [DrawnUI for .NET MAUI](https://github.com/taublast/DrawnUi)
