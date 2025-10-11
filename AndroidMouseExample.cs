using System;
using System.Drawing;

namespace AppoMobi.Maui.Gestures.Examples
{
    /// <summary>
    /// Example demonstrating Android mouse support in AppoMobi.Maui.Gestures
    /// 
    /// This example shows how to handle mouse events on Android platform,
    /// including left-click, right-click, scroll wheel, and stylus support.
    /// Works with USB/Bluetooth mice, Chromebooks, Android TV, and DeX mode.
    /// </summary>
    public class AndroidMouseExample
    {
        private TouchEffect touchEffect;

        public void SetupMouseHandling()
        {
            // Attach to TouchAction event
            touchEffect.TouchAction += OnTouchAction;
        }

        private void OnTouchAction(object sender, TouchActionEventArgs args)
        {
            // Check if this is a mouse/stylus event (Pointer property will be non-null)
            if (args.Pointer != null)
            {
                HandlePointerEvent(args);
            }
            else
            {
                HandleTouchEvent(args);
            }
        }

        private void HandlePointerEvent(TouchActionEventArgs args)
        {
            var pointer = args.Pointer;
            
            Console.WriteLine($"Pointer Event: {args.Type} at {args.Location}");
            Console.WriteLine($"  Device: {pointer.DeviceType}");
            Console.WriteLine($"  Button: {pointer.Button} (#{pointer.ButtonNumber})");
            Console.WriteLine($"  State: {pointer.State}");
            Console.WriteLine($"  Pressed Buttons: {pointer.PressedButtons}");
            
            // Handle different event types
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Only left button uses Pressed - maintains backward compatibility
                    if (pointer.Button == MouseButton.Left)
                    {
                        Console.WriteLine("Left mouse button pressed - standard touch flow");
                        HandleLeftClick(args.Location);
                    }
                    break;

                case TouchActionType.Released:
                    // Only left button uses Released - maintains backward compatibility
                    if (pointer.Button == MouseButton.Left)
                    {
                        Console.WriteLine("Left mouse button released - standard touch flow");
                        HandleLeftRelease(args.Location);
                    }
                    break;

                case TouchActionType.Moved:
                    // Mouse drag with button tracking
                    Console.WriteLine($"Mouse drag with buttons: {pointer.PressedButtons}");
                    HandleMouseDrag(args.Location, pointer.PressedButtons);
                    break;

                case TouchActionType.Pointer:
                    // All non-left buttons use Pointer events to avoid breaking existing controls
                    HandleButtonEvent(args.Location, pointer);
                    break;

                case TouchActionType.Wheel:
                    // Mouse wheel scrolling
                    HandleWheelEvent(args.Location, args.Distance.Delta);
                    break;
            }

            // Handle specific device types
            if (pointer.DeviceType == PointerDeviceType.Pen)
            {
                Console.WriteLine($"Stylus detected with pressure: {pointer.Pressure}");
                HandleStylusInput(args.Location, pointer.Pressure);
            }
        }

        private void HandleButtonEvent(PointF location, TouchEffect.PointerData pointer)
        {
            Console.WriteLine($"Button Event: {pointer.Button} {pointer.State} at {location}");

            // Handle different buttons
            switch (pointer.Button)
            {
                case MouseButton.Right:
                    if (pointer.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Right-click detected - showing context menu");
                        ShowContextMenu(location);
                    }
                    break;

                case MouseButton.Middle:
                    if (pointer.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Middle-click detected");
                        HandleMiddleClick(location);
                    }
                    break;

                case MouseButton.XButton1:
                    if (pointer.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Back button (XButton1) pressed");
                        HandleBackButton();
                    }
                    break;

                case MouseButton.XButton2:
                    if (pointer.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Forward button (XButton2) pressed");
                        HandleForwardButton();
                    }
                    break;

                case MouseButton.Extended:
                    Console.WriteLine($"Extended mouse button #{pointer.ButtonNumber} {pointer.State}");
                    HandleExtendedButton(pointer.ButtonNumber, pointer.State);
                    break;
            }
        }

        private void HandleWheelEvent(PointF location, PointF wheelDelta)
        {
            Console.WriteLine($"Mouse wheel: deltaX={wheelDelta.X:F1}, deltaY={wheelDelta.Y:F1} at {location}");
            
            // Implement scrolling based on wheel deltas
            if (Math.Abs(wheelDelta.Y) > 0.1f)
            {
                HandleVerticalScroll(wheelDelta.Y);
            }
            
            if (Math.Abs(wheelDelta.X) > 0.1f)
            {
                HandleHorizontalScroll(wheelDelta.X);
            }
        }

        private void HandleTouchEvent(TouchActionEventArgs args)
        {
            // Regular touch handling (finger touches)
            Console.WriteLine($"Touch Event: {args.Type} at {args.Location}");
            
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    Console.WriteLine("Finger touch started");
                    break;
                case TouchActionType.Moved:
                    Console.WriteLine("Finger touch moved");
                    break;
                case TouchActionType.Released:
                    Console.WriteLine("Finger touch ended");
                    break;
            }
        }

        // Event handlers for different mouse actions
        private void HandleLeftClick(PointF location)
        {
            Console.WriteLine($"Left click at {location}");
            // Standard click handling - works with existing controls
        }

        private void HandleLeftRelease(PointF location)
        {
            Console.WriteLine($"Left release at {location}");
            // Standard release handling
        }

        private void HandleMouseDrag(PointF location, MouseButtons pressedButtons)
        {
            Console.WriteLine($"Mouse drag at {location} with buttons: {pressedButtons}");
            
            // Handle multi-button drag operations
            if (pressedButtons.HasFlag(MouseButtons.Left | MouseButtons.Right))
            {
                Console.WriteLine("Special drag: Left + Right buttons");
            }
        }

        private void ShowContextMenu(PointF location)
        {
            Console.WriteLine($"Showing context menu at {location}");
            // Show context menu implementation
        }

        private void HandleMiddleClick(PointF location)
        {
            Console.WriteLine($"Middle click at {location}");
            // Middle click handling (e.g., open in new tab)
        }

        private void HandleBackButton()
        {
            Console.WriteLine("Navigate back");
            // Navigation back implementation
        }

        private void HandleForwardButton()
        {
            Console.WriteLine("Navigate forward");
            // Navigation forward implementation
        }

        private void HandleExtendedButton(int buttonNumber, MouseButtonState state)
        {
            Console.WriteLine($"Extended button {buttonNumber} {state}");
            // Handle gaming mouse buttons or other extended buttons
        }

        private void HandleStylusInput(PointF location, float pressure)
        {
            Console.WriteLine($"Stylus input at {location} with pressure {pressure:F2}");
            // Handle pressure-sensitive drawing or writing
        }

        private void HandleVerticalScroll(float deltaY)
        {
            Console.WriteLine($"Vertical scroll: {deltaY:F1}");
            // Implement vertical scrolling
        }

        private void HandleHorizontalScroll(float deltaX)
        {
            Console.WriteLine($"Horizontal scroll: {deltaX:F1}");
            // Implement horizontal scrolling
        }
    }

    /// <summary>
    /// Android Mouse Support Summary:
    ///
    /// ARCHITECTURE:
    /// - Left button: Uses standard TouchActionType.Pressed/Released + Pointer data
    /// - Other buttons: Use TouchActionType.Pointer + Pointer data
    /// - Wheel events: Use TouchActionType.Wheel + Distance deltas
    /// - This maintains backward compatibility with existing touch-based controls
    ///
    /// DETECTION:
    /// - Mouse events detected via MotionEvent.GetToolType() == MotionEventToolType.Mouse
    /// - Stylus detected via MotionEvent.GetToolType() == MotionEventToolType.Stylus
    /// - Hover tracking via OnHoverListener
    /// - Scroll wheel via OnGenericMotionListener
    /// - Pointer data only created for mouse/stylus events (null for finger touches)
    ///
    /// ✅ SUPPORTED FEATURES:
    /// - ✅ Full mouse button detection (Left, Right, Middle, XButton1-2)
    /// - ✅ Stylus detection and pressure sensitivity
    /// - ✅ Mouse vs touch differentiation
    /// - ✅ Hover tracking (mouse movement without button press)
    /// - ✅ Multi-button drag operations
    /// - ✅ Button state tracking during operations
    /// - ✅ Scroll wheel support (horizontal and vertical)
    /// - ✅ API level compatibility (ButtonPress/Release for API 23+)
    ///
    /// ⚠️ LIMITATIONS:
    /// - ⚠️ Extended buttons (XButton3+) depend on Android version and hardware
    /// - ⚠️ ButtonPress/ButtonRelease events only available on Android API 23+
    /// - ⚠️ Some Android devices may not support all mouse features
    ///
    /// USAGE:
    /// 1. Check if args.Pointer != null to detect mouse/stylus events
    /// 2. Use args.Type to determine event type (Pressed/Released/Moved/Pointer/Wheel)
    /// 3. Use args.Pointer.Button and args.Pointer.State for button-specific handling
    /// 4. Use args.Pointer.DeviceType to distinguish Mouse vs Stylus
    /// 5. Use args.Pointer.Pressure for stylus pressure sensitivity
    /// 6. Use args.Distance.Delta for wheel scroll deltas
    /// </summary>
    public class AndroidMouseSupportInfo
    {
        // This class serves as documentation for the Android mouse implementation
    }
}
