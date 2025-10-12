# AppoMobi.Maui.Gestures

Library for .NET MAUI to handle gestures. Can be consumed in XAML and code-behind. A NuGet package with the same name is available.

This library is used by [DrawnUI for .NET MAUI](https://github.com/taublast/DrawnUi).

.NET 8 and above compatible.

---

## What's New

### Version 1.10.1
* Added `Manual` touch mode for dynamic gesture control with parent controls, allowing true cooperative gesture handling inside ScrollViews and other containers.
* Pointer Data Support: Added rich pointer/mouse event data with `TouchActionEventArgs.Pointer` property
* Windows (Full Support): Complete mouse button detection (Left, Right, Middle, XButton1-9), pen pressure, hover tracking
* Android (Limited Support): Mouse button detection (Left, Right, Middle, XButton1-2), stylus pressure, hover tracking, scroll wheel
* Hover Tracking: Mouse movement without button press (Windows, Android)

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
* ✅ **Mouse button support** (Left, Right, Middle, XButton1-9) with pen pressure
* ✅ **Desktop-optimized** hover tracking and multi-button interactions

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
| **Panned** | Pan gesture completed | `Panned` event, `PannedCommand` |
| **Pinched** | Two-finger pinch/zoom | `Pinched` event, `PinchedCommand` |
| **Rotation** | Two-finger rotation | Multi-touch manipulation data |
| **Wheel** | Mouse wheel (Windows) | Wheel event with delta |
| **Pointer** | Mouse/pen hover without press | Pointer event with device info |
| **Mouse Buttons** | Left/Right/Middle/XButton1-9 clicks | Mouse event data with button info |

---

## Touch Handling Modes

Touch handling modes control how gestures interact with parent containers like ScrollView, ListView, etc.

```csharp
public enum TouchHandlingStyle
{
    Default,      // Normal behavior
    Lock,         // Lock input for this control
    Manual,       // Dynamic control via WIllLock property
    Disabled      // Same as InputTransparent=true
}
```

### Default Mode

**Use case**: Standard gesture handling with no special behavior.

```xml
<Label Text="Normal Touch"
       touch:TouchEffect.ShareTouch="Default"
       touch:TouchEffect.CommandTapped="{Binding TapCommand}" />
```

**Behavior**:
- Control receives gestures normally
- Parent containers (like ScrollView) can intercept gestures
- ⚠️ On iOS, parent ScrollViews may steal pan gestures

---

### Lock Mode

**Use case**: Controls that need exclusive gesture handling inside scrollable containers.

```xml
<Slider touch:TouchEffect.ShareTouch="Lock"
        touch:TouchEffect.ForceAttach="True" />
```

**Behavior**:
- **iOS**: Sets gesture recognizer delegates to block parent recognizers
- **Android**: Calls `RequestDisallowInterceptTouchEvent(true)` on parent
- Locks input for the control once a touch begins
- Parent ScrollView won't scroll at all (vertical or horizontal) while touching this control

**When to use**:
- Sliders inside ScrollViews (but see Cooperative mode below)
- Custom pan/drag controls
- Drawing canvases

**Limitation**: Blocks ALL parent gestures, including vertical scrolling for horizontal controls ❌

---

### Manual Mode (NEW!)

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
2. **Listening to the `WIllLock` state** you set dynamically
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

Access via: `TouchEffect.WIllLock` property

**Platform Implementation**:

- **iOS**:
  - Creates a child `UIPanGestureRecognizer` that can be dynamically failed/succeeded
  - Captures reference to parent ScrollView's recognizer
  - When `WIllLock = Locked`: Cancels parent recognizer to take control
  - When `WIllLock = Unlocked`: Fails child recognizer to release parent
  - Main recognizer always gets `TouchesEnded` for proper snapping

- **Android**:
  - Dynamically calls `RequestDisallowInterceptTouchEvent()` based on `WIllLock` state
  - When `WIllLock = Locked`: Blocks parent from intercepting touch events
  - When `WIllLock = Unlocked`: Allows parent to intercept touch events

- **Windows**:
  - Dynamically captures/releases pointer and changes `ManipulationMode` based on `WIllLock` state
  - When `WIllLock = Locked`: Captures pointer + sets `ManipulationMode = None` to block parent
  - When `WIllLock = Unlocked`: Releases pointer + sets `ManipulationMode = System` to allow parent
  - Mouse wheel events are blocked from parent when locked, but still delivered to your control
  - Control always receives all pointer and wheel events regardless of lock state

**Perfect for**:
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

**Key Benefits**:

1. **Always Receive Up Events**: Unlike `Lock` mode, your control always gets `TouchesEnded`/Up for proper snapping
2. **Dynamic Control**: Decide at runtime based on gesture direction, velocity, or any logic
3. **True Cooperation**: Can handle the "ScrollView started first" edge case by cancelling parent
4. **Platform-Optimized**: Each platform uses the most efficient native approach

**When to Use Each Mode**:

| Mode | Use Case | Parent Behavior |
|------|----------|-----------------|
| `Default` | Simple taps, no parent issues | Parent may intercept gestures |
| `Lock` | Always block parent (drawing canvas) | Parent never gets gestures |
| `Manual` | Dynamic cooperation (carousel, slider) | You control via `WIllLock` |
| `Disabled` | Temporarily disable gestures | All gestures pass through |

---

### Disabled Mode

**Use case**: Temporarily disable touch handling without removing the effect.

```xml
<Frame touch:TouchEffect.ShareTouch="Disabled">
    <!-- This won't receive any gestures -->
</Frame>
```

**Behavior**: Equivalent to `InputTransparent="True"` but managed through the effect.

---

## Mouse and Pen Support

The library provides comprehensive mouse and pen support on desktop platforms (Windows, macCatalyst) with full button tracking, hover detection, and pressure sensitivity.

### Mouse Button Events

Mouse button events are handled differently based on the button type to maintain backward compatibility:

- **Left Button (Primary)**: Uses standard touch events (`Down`, `Up`, `Tapped`) + mouse data
- **Other Buttons**: Uses `Pointer` events with button state information

```csharp
public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
{
    // Check if this is a mouse/pen event
    if (args.Pointer != null)
    {
        var mouse = args.Pointer;

        switch (action)
        {
            case TouchActionResult.Down:
            case TouchActionResult.Up:
            case TouchActionResult.Tapped:
                // LEFT button only - maintains backward compatibility
                HandleLeftButton(mouse, args.Location);
                break;

            case TouchActionResult.Pointer:
                // ALL other buttons + hover events
                if (mouse.State == MouseButtonState.Pressed)
                {
                    // Right, Middle, XButton1-9 pressed
                    HandleOtherButton(mouse.Button, args.Location);
                }
                else if (mouse.State == MouseButtonState.Released)
                {
                    // Button released
                    HandleButtonRelease(mouse.Button);
                }
                else
                {
                    // Pure hover (no buttons pressed)
                    HandleMouseHover(args.Location, mouse.DeviceType);
                }
                break;

            case TouchActionResult.Panning:
                // Drag with any button(s) pressed
                HandleMouseDrag(mouse.PressedButtons, args.Location, args.Distance);
                break;
        }
    }
    else
    {
        // Regular touch event
        HandleTouchEvent(type, args, action);
    }
}
```

### Mouse Event Data

When `args.Pointer != null`, you have access to rich mouse/pen information:

```csharp
public class PointerData
{
    // Button that triggered this event (for press/release)
    public MouseButton Button { get; set; }

    // Button number (1-based): 1=Left, 2=Right, 3=Middle, 4+=Extended
    public int ButtonNumber { get; set; }

    // Button state (Pressed/Released)
    public MouseButtonState State { get; set; }

    // All currently pressed buttons (flags)
    public MouseButtons PressedButtons { get; set; }

    // Device type (Mouse/Pen/Touch)
    public PointerDeviceType DeviceType { get; set; }

    // Pressure (0.0-1.0 for pen, 1.0 for mouse)
    public float Pressure { get; set; }
}
```

### Supported Mouse Buttons

```csharp
public enum MouseButton
{
    Left,       // Primary button
    Right,      // Secondary button
    Middle,     // Wheel button
    XButton1,   // Back button
    XButton2,   // Forward button
    XButton3,   // Gaming mouse button 6
    XButton4,   // Gaming mouse button 7
    XButton5,   // Gaming mouse button 8
    XButton6,   // Gaming mouse button 9
    XButton7,   // Gaming mouse button 10
    XButton8,   // Gaming mouse button 11
    XButton9,   // Gaming mouse button 12
    Extended    // For buttons beyond predefined ones
}

[Flags]
public enum MouseButtons
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 4,
    XButton1 = 8,
    XButton2 = 16,
    // ... up to XButton9 = 2048
}
```

### Common Usage Patterns

**Context Menu on Right Click:**
```csharp
if (action == TouchActionResult.Pointer &&
    args.Pointer?.State == MouseButtonState.Pressed &&
    args.Pointer?.Button == MouseButton.Right)
{
    ShowContextMenu(args.Location);
}
```

**Multi-Button Drag Operations:**
```csharp
if (action == TouchActionResult.Panning && args.Pointer != null)
{
    var buttons = args.Pointer.PressedButtons;

    if (buttons.HasFlag(MouseButtons.Left))
        HandleSelection(args.Location, args.Distance);
    else if (buttons.HasFlag(MouseButtons.Middle))
        HandlePanning(args.Distance);
    else if (buttons.HasFlag(MouseButtons.Left | MouseButtons.Right))
        HandleZoom(args.Distance);
}
```

**Pen Pressure Sensitivity:**
```csharp
if (args.Pointer?.DeviceType == PointerDeviceType.Pen)
{
    var pressure = args.Pointer.Pressure; // 0.0 to 1.0
    var strokeWidth = pressure * 10f;   // Variable stroke width
    DrawWithPressure(args.Location, strokeWidth);
}
```

**Gaming Mouse Support:**
```csharp
// Handle extended buttons for gaming mice
switch (args.Pointer?.Button)
{
    case MouseButton.XButton1:
        NavigateBack();
        break;
    case MouseButton.XButton2:
        NavigateForward();
        break;
    case MouseButton.XButton3:
        CustomAction1();
        break;
    // ... up to XButton9
}
```

### Platform Support

- **Windows**: Full support for all buttons, pen pressure, hover tracking
- **macCatalyst**: Basic mouse support (Left/Right click, hover tracking, Apple Pencil detection)
- **iOS/Android**: Touch events only (no mouse support)

#### macCatalyst Implementation Notes

The macCatalyst implementation provides basic mouse support with some limitations compared to Windows:

**✅ Supported Features:**
- ✅ Left and Right mouse button detection (basic)
- ✅ Apple Pencil detection (UITouchType.Stylus)
- ✅ Mouse vs touch differentiation
- ✅ Hover tracking (mouse movement without button press)
- ✅ Same smart button handling (Left = standard events, Right = Pointer events)

**⚠️ Limitations:**
- ❌ Extended buttons (XButton3-9) not available (UIKit limitation)
- ❌ Pressure tracking during hover not available (UIKit limitation)
- ⚠️ Right-click detection is heuristic-based, not as reliable as Windows
- ⚠️ Middle button detection not implemented (UIKit doesn't expose it)

**🔧 Technical Implementation:**
```csharp
// Mouse detection via touch characteristics
private bool IsMouseEvent(UITouch touch, UIEvent evt)
{
    return touch.Type == UITouchType.Indirect ||
           (touch.MaximumPossibleForce == 0 && touch.Force == 0);
}

// Hover tracking via UIHoverGestureRecognizer
var hoverGestureRecognizer = new UIHoverGestureRecognizer(HandleHover);
_view.AddGestureRecognizer(hoverGestureRecognizer);
```

**📝 Pressure Tracking Note:**
- Pressure is available during Apple Pencil touch events
- Pressure during hover is NOT available (UIKit limitation)
- Mouse always reports pressure = 1.0f

### Backward Compatibility

Existing controls continue to work unchanged:
- Touch events work exactly as before
- Left mouse button behaves like touch (generates `Down`/`Up`/`Tapped`)
- Only controls that check `args.Pointer != null` get enhanced mouse features

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

### 4. Prevent Memory Leaks

The effect auto-disposes when the element is detached, but you can manually dispose if needed:

```csharp
protected override void OnDisappearing()
{
    base.OnDisappearing();
    // Effect auto-disposes, but you can clean up custom resources
}
```

---

## Platform-Specific Notes

### iOS
- Uses custom `UIGestureRecognizer` implementations
- `Lock` mode uses `ShouldRecognizeSimultaneously = false`
- `Manual` mode creates dynamic child `UIPanGestureRecognizer` and captures parent recognizer references
- Supports iOS 13.4+ with `ShouldReceiveEvent` delegate

### Android
- Uses `View.IOnTouchListener` and `GestureDetector`
- `Lock` mode calls `RequestDisallowInterceptTouchEvent(true)`
- `Manual` mode dynamically calls `RequestDisallowInterceptTouchEvent()` based on `WIllLock` state
- Handles multi-pointer events correctly

### Windows
- Uses WinUI `FrameworkElement` pointer events, pointer capture, and `ManipulationMode`
- **Lock mode**:
  - Captures pointer on press
  - Sets `ManipulationMode = None` to block parent manipulations
  - Blocks mouse wheel events from reaching parent ScrollView
  - Your control still receives all pointer and wheel events
- **Manual mode**:
  - Dynamically captures/releases pointer in `PointerMoved` based on `WIllLock` state
  - When `WIllLock = Locked`: Captures pointer + `ManipulationMode = None`
  - When `WIllLock = Unlocked`: Releases pointer + `ManipulationMode = System`
  - Mouse wheel events blocked from parent when `WIllLock = Locked`, but your control still receives them
  - Tracks captured pointers internally to avoid null reference exceptions
- **Default mode**:
  - Captures pointer on press with `ManipulationMode = System`
  - Normal behavior, parent can receive manipulations
- **Mouse and Pen Support**:
  - Full mouse button detection (Left, Right, Middle, XButton1-2)
  - Gaming mice with extended buttons (XButton3-9) supported via `Extended` button type
  - Pen pressure sensitivity and device type detection
  - Hover tracking without button press (`TouchActionResult.Pointer`)
  - Left button uses standard touch events for backward compatibility
  - Other buttons use `Pointer` events to avoid breaking existing controls
  - Multi-button drag operations with `PressedButtons` flags
- Mouse wheel events can be used for zoom/scale in your control
- All touch gestures work with both touch and mouse input simultaneously
- **Testing Note**: Mouse wheel testing provides good coverage for Manual mode behavior even without touch device

---

## Troubleshooting

### Issue: ScrollView steals my control's gestures on iOS

**Solution**: Use `Manual` mode with dynamic locking:

```xml
<MyControl touch:TouchEffect.ShareTouch="Manual"
           touch:TouchEffect.ForceAttach="True" />
```

Then set `WIllLock = ShareLockState.Locked` when you want control, or `Unlocked` to release parent.

---

### Issue: Can't scroll vertically when touching my horizontal slider

**Problem**: Using `Lock` mode blocks all parent gestures.

**Solution**: Switch to `Manual` mode and only lock for horizontal gestures:

```csharp
TouchEffect.SetShareTouch(mySlider, TouchHandlingStyle.Manual);

// In your IGestureListener implementation:
// - Set WIllLock = Locked when horizontal pan detected
// - Set WIllLock = Unlocked when vertical pan detected
```

---

### Issue: Taps detected while swiping

**Solution**: Adjust the velocity threshold:

```csharp
TouchEffect.TappedVelocityThresholdPoints = 300f; // Increase for stricter tap detection
```

---

### Issue: Long press triggers too quickly/slowly

**Solution**: Adjust the long press timing:

```csharp
TouchEffect.LongPressTimeMsDefault = 1000; // 1 second instead of 1.5
```

---

### Issue: Gestures not working in custom control

**Checklist**:
1. ✅ Called `UseGestures()` in MauiProgram.cs?
2. ✅ Set `ForceAttach="True"` in XAML or code?
3. ✅ Implemented `IGestureListener` interface?
4. ✅ `InputTransparent` property returns `false`?
5. ✅ Control is visible and not behind other elements?

---

### Issue: Multi-touch gestures not working

**Checklist**:
1. ✅ Check `args.NumberOfTouches > 1`
2. ✅ Check `args.Manipulation != null` before accessing scale/rotation
3. ✅ Not using `Lock` mode which may interfere with multi-touch

---

## Additional Utilities

### Close Keyboard (Android)

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

## Philosophy

The library aims to provide **platform-agnostic gesture processing** by handling raw platform input in shared code. This approach ensures consistent behavior across iOS, Android, and Windows while maintaining native performance and feel.

All gesture data uses **pixels** instead of points for better precision across platforms. Convert to your preferred units as needed using the `TouchEffect.Density` property.

---

## Contributing

The library is actively developed. If you need additional features or find bugs:

1. 💬 Start a [Discussion](https://github.com/taublast/AppoMobi.Maui.Gestures/discussions)
2. 🐛 Open an [Issue](https://github.com/taublast/AppoMobi.Maui.Gestures/issues)
3. 🔧 Submit a [Pull Request](https://github.com/taublast/AppoMobi.Maui.Gestures/pulls)

---

## License

[Add your license information here]

## Credits

Developed by AppoMobi
Used in production by [DrawnUI for .NET MAUI](https://github.com/taublast/DrawnUi)
