using System;
using System.Drawing;

namespace AppoMobi.Maui.Gestures.Examples
{
    /// <summary>
    /// Example demonstrating macCatalyst mouse button support in AppoMobi.Maui.Gestures
    /// 
    /// This example shows how to handle mouse events on macCatalyst platform,
    /// including left-click, right-click, and pen support.
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
            // Check if this is a mouse/pen event (Mouse property will be non-null)
            if (args.Mouse != null)
            {
                HandleMouseEvent(args);
            }
            else
            {
                HandleTouchEvent(args);
            }
        }

        private void HandleMouseEvent(TouchActionEventArgs args)
        {
            var mouse = args.Mouse;
            
            Console.WriteLine($"Mouse Event: {args.Type} at {args.Location}");
            Console.WriteLine($"  Device: {mouse.DeviceType}");
            Console.WriteLine($"  Button: {mouse.Button} (#{mouse.ButtonNumber})");
            Console.WriteLine($"  State: {mouse.State}");
            Console.WriteLine($"  Pressed Buttons: {mouse.PressedButtons}");
            
            // Handle different event types
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Only left button uses Pressed - maintains backward compatibility
                    if (mouse.Button == MouseButton.Left)
                    {
                        Console.WriteLine("Left mouse button pressed - standard touch flow");
                        HandleLeftClick(args.Location);
                    }
                    break;

                case TouchActionType.Released:
                    // Only left button uses Released - maintains backward compatibility
                    if (mouse.Button == MouseButton.Left)
                    {
                        Console.WriteLine("Left mouse button released - standard touch flow");
                        HandleLeftRelease(args.Location);
                    }
                    break;

                case TouchActionType.Moved:
                    // Mouse drag with button tracking
                    Console.WriteLine($"Mouse drag with buttons: {mouse.PressedButtons}");
                    HandleMouseDrag(args.Location, mouse.PressedButtons);
                    break;

                case TouchActionType.Pointer:
                    // All non-left buttons use Pointer events to avoid breaking existing controls
                    HandlePointerEvent(args.Location, mouse);
                    break;
            }

            // Handle specific device types
            if (mouse.DeviceType == PointerDeviceType.Pen)
            {
                Console.WriteLine($"Apple Pencil detected with pressure: {mouse.Pressure}");
                HandlePenInput(args.Location, mouse.Pressure);
            }
        }

        private void HandlePointerEvent(PointF location, TouchEffect.MouseEventArgs mouse)
        {
            Console.WriteLine($"Pointer Event: {mouse.Button} {mouse.State} at {location}");

            // Handle different buttons
            switch (mouse.Button)
            {
                case MouseButton.Right:
                    if (mouse.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Right-click detected - showing context menu");
                        ShowContextMenu(location);
                    }
                    break;

                case MouseButton.Middle:
                    if (mouse.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Middle-click detected");
                        HandleMiddleClick(location);
                    }
                    break;

                case MouseButton.XButton1:
                    if (mouse.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Back button (XButton1) pressed");
                        HandleBackButton();
                    }
                    break;

                case MouseButton.XButton2:
                    if (mouse.State == MouseButtonState.Pressed)
                    {
                        Console.WriteLine("Forward button (XButton2) pressed");
                        HandleForwardButton();
                    }
                    break;

                case MouseButton.Extended:
                    Console.WriteLine($"Extended mouse button #{mouse.ButtonNumber} {mouse.State}");
                    HandleExtendedButton(mouse.ButtonNumber, mouse.State);
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
    }

    /// <summary>
    /// macCatalyst Mouse Support Summary:
    /// 
    /// ARCHITECTURE:
    /// - Left button: Uses standard TouchActionType.Pressed/Released + Mouse data
    /// - Other buttons: Use TouchActionType.Pointer + Mouse data
    /// - This maintains backward compatibility with existing touch-based controls
    /// 
    /// DETECTION:
    /// - Mouse events detected via UITouch.Type == UITouchType.Indirect
    /// - Apple Pencil detected via UITouch.Type == UITouchType.Stylus
    /// - Mouse data only created for mouse/pen events (null for finger touches)
    /// 
    /// SUPPORTED FEATURES:
    /// - Left/Right click detection
    /// - Apple Pencil with pressure sensitivity
    /// - Multi-button drag operations
    /// - Button state tracking during operations
    /// 
    /// LIMITATIONS:
    /// - macCatalyst doesn't expose all mouse buttons like Windows
    /// - Extended buttons (XButton3-9) may not be available
    /// - Right-click detection is basic compared to Windows implementation
    /// 
    /// USAGE:
    /// 1. Check if args.Mouse != null to detect mouse/pen events
    /// 2. Use args.Type to determine event type (Pressed/Released/Moved/Pointer)
    /// 3. Use args.Mouse.Button and args.Mouse.State for button-specific handling
    /// 4. Use args.Mouse.DeviceType to distinguish Mouse vs Pen
    /// 5. Use args.Mouse.Pressure for Apple Pencil pressure sensitivity
    /// </summary>
    public class MacCatalystMouseSupportInfo
    {
        // This class serves as documentation for the macCatalyst mouse implementation
    }
}
