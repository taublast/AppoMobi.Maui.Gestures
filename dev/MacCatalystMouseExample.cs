using System;
using System.Drawing;

namespace AppoMobi.Maui.Gestures.Examples
{
    /// <summary>
    /// Example demonstrating macCatalyst pointer support in AppoMobi.Maui.Gestures
    ///
    /// This example shows how to handle mouse, trackpad, and pen events on macCatalyst platform,
    /// including left-click, right-click, trackpad scrolling, and Apple Pencil support.
    /// </summary>
    public class MacCatalystMouseExample
    {
        private TouchEffect touchEffect;

        public void SetupMouseHandling()
        {
            // Attach to TouchAction event
            touchEffect.TouchAction += OnTouchAction;
        }

        private void OnTouchAction(object sender, TouchActionEventArgs args)
        {
            // Check if this is a mouse/pen/trackpad event (Pointer property will be non-null)
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

            // Check for trackpad scrolling (two-finger pan)
            if (args.Type == TouchActionType.Moved && args.NumberOfTouches == 0)
            {
                HandleTrackpadScroll(args);
                return;
            }
            
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
            }

            // Handle specific device types
            if (pointer.DeviceType == PointerDeviceType.Pen)
            {
                Console.WriteLine($"Apple Pencil detected with pressure: {pointer.Pressure}");
                HandlePenInput(args.Location, pointer.Pressure);
            }
        }

        private void HandleTrackpadScroll(TouchActionEventArgs args)
        {
            var deltaX = args.Distance.Delta.X;
            var deltaY = args.Distance.Delta.Y;

            Console.WriteLine($"Trackpad Scroll: deltaX={deltaX:F1}, deltaY={deltaY:F1}");

            // Implement smooth panning based on trackpad deltas
            // This provides native macOS trackpad feel
            if (Math.Abs(deltaX) > 0.1f || Math.Abs(deltaY) > 0.1f)
            {
                HandleSmoothPan(deltaX, deltaY);
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
                        Console.WriteLine("Middle-click detected (limited support on macCatalyst)");
                        HandleMiddleClick(location);
                    }
                    break;

                case MouseButton.XButton1:
                    if (pointer.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Back button (XButton1) pressed (limited support on macCatalyst)");
                        HandleBackButton();
                    }
                    break;

                case MouseButton.XButton2:
                    if (pointer.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Forward button (XButton2) pressed (limited support on macCatalyst)");
                        HandleForwardButton();
                    }
                    break;

                case MouseButton.Extended:
                    Console.WriteLine($"Extended mouse button #{pointer.ButtonNumber} {pointer.State} (not supported on macCatalyst)");
                    HandleExtendedButton(pointer.ButtonNumber, pointer.State);
                    break;
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

        private void HandlePenInput(PointF location, float pressure)
        {
            Console.WriteLine($"Apple Pencil input at {location} with pressure {pressure:F2}");
            // Handle pressure-sensitive drawing or writing
        }

        private void HandleSmoothPan(float deltaX, float deltaY)
        {
            Console.WriteLine($"Smooth trackpad pan: deltaX={deltaX:F1}, deltaY={deltaY:F1}");

            // Implement smooth panning with trackpad deltas
            // This provides native macOS trackpad feel for content scrolling

            // Example: Scroll content based on deltas
            // ScrollContent(deltaX, deltaY);

            // Example: Pan a map or canvas
            // PanCanvas(deltaX, deltaY);

            // The deltas are already in the correct direction and magnitude
            // for natural trackpad scrolling behavior
        }
    }

    /// <summary>
    /// macCatalyst Pointer Support Summary:
    ///
    /// ARCHITECTURE:
    /// - Left button: Uses standard TouchActionType.Pressed/Released + Pointer data
    /// - Other buttons: Use TouchActionType.Pointer + Pointer data
    /// - Trackpad scrolling: Uses TouchActionType.Moved with NumberOfTouches=0 + Distance deltas
    /// - This maintains backward compatibility with existing touch-based controls
    ///
    /// DETECTION:
    /// - Mouse events detected via UITouch.Type == UITouchType.Indirect
    /// - Apple Pencil detected via UITouch.Type == UITouchType.Stylus
    /// - Hover tracking via UIHoverGestureRecognizer
    /// - Trackpad scrolling via UIScrollView detection
    /// - Pointer data only created for mouse/pen/trackpad events (null for finger touches)
    ///
    /// ✅ SUPPORTED FEATURES:
    /// - ✅ Left/Right click detection (basic)
    /// - ✅ Apple Pencil detection and pressure during touch events
    /// - ✅ Mouse vs touch differentiation
    /// - ✅ Hover tracking (mouse movement without button press)
    /// - ✅ Multi-button drag operations
    /// - ✅ Button state tracking during operations
    /// - ✅ Trackpad scrolling (two-finger pan detection)
    /// - ✅ Smooth trackpad deltas for native macOS feel
    ///
    /// ❌ LIMITATIONS (UIKit constraints):
    /// - ❌ Extended buttons (XButton3-9) not available
    /// - ❌ Pressure tracking during hover not available
    /// - ❌ Middle button detection limited
    /// - ❌ Trackpad momentum phases not available (UIScrollView limitation)
    /// - ⚠️ Right-click detection is heuristic-based, not as reliable as Windows
    /// - ⚠️ Trackpad detection uses UIScrollView workaround, not native NSEvent
    ///
    /// USAGE:
    /// 1. Check if args.Pointer != null to detect mouse/pen/trackpad events
    /// 2. Use args.Type to determine event type (Pressed/Released/Moved/Pointer)
    /// 3. Use args.Pointer.Button and args.Pointer.State for button-specific handling
    /// 4. Use args.Pointer.DeviceType to distinguish Mouse vs Pen
    /// 5. Use args.Pointer.Pressure for Apple Pencil pressure (during touch only)
    /// 6. For trackpad: Check args.Type == Moved && args.NumberOfTouches == 0
    /// 7. Use args.Distance.Delta for trackpad scroll deltas
    /// </summary>
    public class MacCatalystPointerSupportInfo
    {
        // This class serves as documentation for the macCatalyst pointer implementation
        // including mouse, trackpad, and Apple Pencil support
    }
}
