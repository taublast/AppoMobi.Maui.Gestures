using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Text;
using AppoMobi.Maui.Gestures;

namespace GesturesTester;

public class TouchBorder : Border, IGestureListener
{
    private int _eventCounter = 0;

    private void LogEvent(string source, TouchActionEventArgs args)
    {
        _eventCounter++;
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

        var eventInfo = new StringBuilder();
        eventInfo.Append($"[{_eventCounter:000}] {timestamp} {source,-20} {args.Type,-12}");

        // Add pointer information if available
        if (args.Pointer != null)
        {
            eventInfo.Append($" [POINTER {args.Pointer.DeviceType} Btn:{args.Pointer.Button}({args.Pointer.ButtonNumber}) {args.Pointer.State}");

            if (args.Pointer.DeviceType == PointerDeviceType.Pen)
            {
                eventInfo.Append($" Pressure:{args.Pointer.Pressure:F2}");
            }

            if (args.Pointer.PressedButtons != MouseButtons.None)
            {
                eventInfo.Append($" Pressed:{args.Pointer.PressedButtons}");
            }

            eventInfo.Append("]");
        }

        // Add touch count
        if (args.NumberOfTouches > 0)
        {
            eventInfo.Append($" Touches:{args.NumberOfTouches}");
        }

        // Add distance info for movement
        if (args.Type == TouchActionType.Moved && args.Distance != null)
        {
            eventInfo.Append($" Δ({args.Distance.Delta.X:F0},{args.Distance.Delta.Y:F0})");

            if (args.Distance.Velocity.X != 0 || args.Distance.Velocity.Y != 0)
            {
                eventInfo.Append($" V:{Math.Sqrt(args.Distance.Velocity.X * args.Distance.Velocity.X + args.Distance.Velocity.Y * args.Distance.Velocity.Y):F0}");
            }
        }

        // Add wheel info
        if (args.Wheel != null && args.Type == TouchActionType.Wheel)
        {
            eventInfo.Append($" Wheel Δ:{args.Wheel.Delta:F2} Scale:{args.Wheel.Scale:F2}");
        }

        var logLine = eventInfo.ToString();

        Debug.WriteLine(logLine);
    }

    // IGestureListener implementation (currently unused, but available for direct handling)
    public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
    {
        LogEvent($"{action}", args);
    }
}

public partial class MainPage : ContentPage
{
	private int _eventCounter = 0;
	private readonly StringBuilder _logBuilder = new();
	private const int MaxLogLines = 50;

	public MainPage()
	{
		InitializeComponent();

		// Slider value changed
		TestSlider.ValueChanged += (s, e) =>
		{
			SliderValueLabel.Text = $"Value: {e.NewValue:F0}";
		};

		// Attach gesture listeners after the page is loaded
		// This ensures the TouchEffect attached properties have been processed
		Loaded += OnPageLoaded;
	}

	private void OnPageLoaded(object sender, EventArgs e)
	{
		// Now that the page is loaded, the TouchEffect instances should be attached
		SetupGestureListeners();
	}

	private void SetupGestureListeners()
	{
		// Main test canvas - captures ALL events
		var canvasEffect = TouchEffect.GetFrom(TestCanvas);
		if (canvasEffect != null)
		{
			canvasEffect.TouchAction += OnCanvasTouchAction;
			canvasEffect.Down += OnCanvasDown;
			canvasEffect.Up += OnCanvasUp;
			canvasEffect.Tapped += OnCanvasTapped;
			canvasEffect.LongPressing += OnCanvasLongPress;
			canvasEffect.Panning += OnCanvasPanning;
			canvasEffect.Pinched += OnCanvasPinched;
			System.Diagnostics.Debug.WriteLine("✓ TestCanvas effect attached");
		}
		else
		{
			System.Diagnostics.Debug.WriteLine("✗ TestCanvas effect NOT FOUND");
		}
	}

	private void OnCanvasTouchAction(object sender, TouchActionEventArgs args)
	{
		// Update pointer indicator position for visual feedback
		if (args.Type == TouchActionType.Moved || args.Type == TouchActionType.Pressed || args.Type == TouchActionType.Pointer)
		{
			var density = TouchEffect.Density;
			var x = args.Location.X / density;
			var y = args.Location.Y / density;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				PointerIndicator.IsVisible = true;
				PointerIndicator.TranslationX = x - 12.5; // Center the circle (25/2)
				PointerIndicator.TranslationY = y - 12.5;
			});
		}
		else if (args.Type == TouchActionType.Released || args.Type == TouchActionType.Exited)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				PointerIndicator.IsVisible = false;
			});
		}

		// Log ALL TouchAction events
		LogEvent("TouchAction", args);
	}

	private void OnCanvasDown(object sender, TouchActionEventArgs args)
	{
		LogEvent("⬇ DOWN", args);
	}

	private void OnCanvasUp(object sender, TouchActionEventArgs args)
	{
		LogEvent("⬆ UP", args);
	}

	private void OnCanvasTapped(object sender, TouchActionEventArgs args)
	{
		LogEvent("👆 TAPPED", args);
	}

	private void OnCanvasLongPress(object sender, TouchActionEventArgs args)
	{
		LogEvent("⏱ LONG PRESS", args);
	}

	private void OnCanvasPanning(object sender, TouchActionEventArgs args)
	{
		LogEvent("↔ PANNING", args);
	}

	private void OnCanvasPinched(object sender, TouchActionEventArgs args)
	{
		LogEvent("🤏 PINCHED", args);
	}

	private void LogEvent(string source, TouchActionEventArgs args)
	{
        //todo later
	}



	public bool InputTransparent => false;
}
