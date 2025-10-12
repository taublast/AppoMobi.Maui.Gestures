// Example usage of the new mouse button functionality
// This demonstrates how to use the mouse button tracking in your controls

using AppoMobi.Maui.Gestures;

namespace ExampleApp
{
    public class MouseAwareControl : ContentView, IGestureListener
    {
        public MouseAwareControl()
        {
            // Enable gesture tracking
            TouchEffect.SetForceAttach(this, true);
            TouchEffect.SetShareTouch(this, TouchHandlingStyle.Manual);
            
            BackgroundColor = Colors.LightGray;
            Content = new Label 
            { 
                Text = "Mouse-aware control\nTry different mouse buttons!",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        public bool InputTransparent => false;

        public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
        {
            // Check if this is a mouse/pen event
            if (args.Mouse != null)
            {
                HandleMouseEvent(type, args, action);
            }
            else
            {
                HandleTouchEvent(type, args, action);
            }
        }

        private void HandleMouseEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
        {
            var mouse = args.Mouse;
            var deviceName = mouse.DeviceType == PointerDeviceType.Pen ? "Pen" : "Mouse";

            switch (action)
            {
                case TouchActionResult.Down:
                    // Only LEFT button uses Down/Up - maintains backward compatibility
                    System.Diagnostics.Debug.WriteLine(
                        $"{deviceName} LEFT Button pressed at {args.Location}");
                    HandleLeftClick(args.Location);
                    break;

                case TouchActionResult.Up:
                    // Only LEFT button uses Down/Up - maintains backward compatibility
                    System.Diagnostics.Debug.WriteLine(
                        $"{deviceName} LEFT Button released");
                    break;

                case TouchActionResult.Tapped:
                    // Only LEFT button generates Tapped events
                    System.Diagnostics.Debug.WriteLine(
                        $"{deviceName} LEFT Button clicked!");
                    break;

                case TouchActionResult.Pointer:
                    // ALL other buttons (Right, Middle, XButton1, etc.) use Pointer events
                    // Check the button state to determine press/release
                    if (mouse.State == MouseButtonState.Pressed)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"{deviceName} Button {mouse.Button} (#{mouse.ButtonNumber}) pressed at {args.Location}");

                        // Handle specific buttons
                        switch (mouse.Button)
                        {
                            case MouseButton.Right:
                                HandleRightClick(args.Location);
                                break;
                            case MouseButton.Middle:
                                HandleMiddleClick(args.Location);
                                break;
                            case MouseButton.XButton1:
                                HandleBackButton(args.Location);
                                break;
                            case MouseButton.XButton2:
                                HandleForwardButton(args.Location);
                                break;
                        }
                    }
                    else if (mouse.State == MouseButtonState.Released)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"{deviceName} Button {mouse.Button} (#{mouse.ButtonNumber}) released");
                    }
                    else
                    {
                        // Pure hover without any buttons pressed
                        HandleMouseHover(args.Location);

                        // For pen, you can access pressure
                        if (mouse.DeviceType == PointerDeviceType.Pen)
                        {
                            System.Diagnostics.Debug.WriteLine($"Pen hover with pressure: {mouse.Pressure:F2}");
                        }
                    }
                    break;

                case TouchActionResult.Panning:
                    // Check which buttons are pressed during drag
                    if (mouse.PressedButtons.HasFlag(MouseButtons.Left))
                    {
                        HandleLeftDrag(args.Location, args.Distance);
                    }
                    else if (mouse.PressedButtons.HasFlag(MouseButtons.Middle))
                    {
                        HandleMiddleDrag(args.Location, args.Distance);
                    }
                    else if (mouse.PressedButtons.HasFlag(MouseButtons.Right))
                    {
                        HandleRightDrag(args.Location, args.Distance);
                    }
                    
                    // Handle multiple buttons
                    if (mouse.PressedButtons.HasFlag(MouseButtons.Left | MouseButtons.Right))
                    {
                        HandleLeftRightDrag(args.Location, args.Distance);
                    }
                    break;

                case TouchActionResult.Pointer:
                    // Mouse hover without any buttons pressed
                    HandleMouseHover(args.Location);
                    
                    // For pen, you can access pressure
                    if (mouse.DeviceType == PointerDeviceType.Pen)
                    {
                        System.Diagnostics.Debug.WriteLine($"Pen hover with pressure: {mouse.Pressure:F2}");
                    }
                    break;
            }
        }

        private void HandleTouchEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
        {
            // Regular touch handling
            System.Diagnostics.Debug.WriteLine($"Touch {action} at {args.Location}");
        }

        private void HandleLeftClick(PointF location)
        {
            System.Diagnostics.Debug.WriteLine("Left click - primary action");
            BackgroundColor = Colors.LightBlue;
        }

        private void HandleRightClick(PointF location)
        {
            System.Diagnostics.Debug.WriteLine("Right click - context menu");
            BackgroundColor = Colors.LightCoral;
            // Show context menu here
        }

        private void HandleMiddleClick(PointF location)
        {
            System.Diagnostics.Debug.WriteLine("Middle click - often used for panning");
            BackgroundColor = Colors.LightGreen;
        }

        private void HandleBackButton(PointF location)
        {
            System.Diagnostics.Debug.WriteLine("Back button (XButton1) - navigate back");
            BackgroundColor = Colors.LightYellow;
        }

        private void HandleForwardButton(PointF location)
        {
            System.Diagnostics.Debug.WriteLine("Forward button (XButton2) - navigate forward");
            BackgroundColor = Colors.LightPink;
        }

        private void HandleLeftDrag(PointF location, TouchActionEventArgs.DistanceInfo distance)
        {
            System.Diagnostics.Debug.WriteLine($"Left drag: {distance.Delta}");
            // Implement selection or drawing
        }

        private void HandleMiddleDrag(PointF location, TouchActionEventArgs.DistanceInfo distance)
        {
            System.Diagnostics.Debug.WriteLine($"Middle drag: {distance.Delta}");
            // Implement panning
        }

        private void HandleRightDrag(PointF location, TouchActionEventArgs.DistanceInfo distance)
        {
            System.Diagnostics.Debug.WriteLine($"Right drag: {distance.Delta}");
            // Implement custom action
        }

        private void HandleLeftRightDrag(PointF location, TouchActionEventArgs.DistanceInfo distance)
        {
            System.Diagnostics.Debug.WriteLine($"Left+Right drag: {distance.Delta}");
            // Implement zoom or special action
        }

        private void HandleMouseHover(PointF location)
        {
            // Update hover effects, tooltips, etc.
            // This is called frequently, so keep it lightweight
        }
    }

    // Example of checking mouse buttons in a command
    public class MouseButtonCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is TouchActionEventArgs args && args.Mouse != null)
            {
                var mouse = args.Mouse;
                System.Diagnostics.Debug.WriteLine(
                    $"Command executed with {mouse.DeviceType} button {mouse.Button}");
                
                // Different actions based on button
                switch (mouse.Button)
                {
                    case MouseButton.Left:
                        // Primary action
                        break;
                    case MouseButton.Right:
                        // Secondary action
                        break;
                    case MouseButton.Middle:
                        // Tertiary action
                        break;
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}

// XAML Usage Example:
/*
<ContentPage xmlns:touch="clr-namespace:AppoMobi.Maui.Gestures;assembly=AppoMobi.Maui.Gestures"
             xmlns:local="clr-namespace:ExampleApp">

    <!-- Mouse-aware control -->
    <local:MouseAwareControl />
    
    <!-- Or use with commands -->
    <Button Text="Click me with different mouse buttons!"
            touch:TouchEffect.CommandTapped="{Binding MouseButtonCommand}"
            touch:TouchEffect.CommandTappedParameter="{Binding .}" />

</ContentPage>
*/
