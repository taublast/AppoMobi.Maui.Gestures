using System.Text;
using AppoMobi.Maui.Gestures;

namespace GesturesTester;

public partial class MainPage : ContentPage, IGestureListener
{
	private int _eventCounter = 0;
	private readonly StringBuilder _logBuilder = new();
	private const int MaxLogLines = 50;

	public MainPage()
	{
		InitializeComponent();

		// Attach gesture listeners to test areas
		SetupGestureListeners();

		// Slider value changed
		TestSlider.ValueChanged += (s, e) =>
		{
			SliderValueLabel.Text = $"Value: {e.NewValue:F0}";
		};
	}

	private void SetupGestureListeners()
	{
		// Canvas touch events
		var canvasEffect = TouchEffect.GetFrom(Canvas);
		if (canvasEffect != null)
		{
			canvasEffect.TouchAction += OnCanvasTouchAction;
			canvasEffect.Tapped += OnCanvasTapped;
			canvasEffect.Down += OnCanvasDown;
			canvasEffect.Up += OnCanvasUp;
			canvasEffect.LongPressing += OnCanvasLongPress;
		}

		// Left button events
		var leftBtnEffect = TouchEffect.GetFrom(LeftClickBtn);
		if (leftBtnEffect != null)
		{
			leftBtnEffect.TouchAction += OnLeftButtonTouchAction;
		}

		// Right button events
		var rightBtnEffect = TouchEffect.GetFrom(RightClickBtn);
		if (rightBtnEffect != null)
		{
			rightBtnEffect.TouchAction += OnRightButtonTouchAction;
		}

		// Multi-touch area events
		var multiTouchEffect = TouchEffect.GetFrom(MultiTouchArea);
		if (multiTouchEffect != null)
		{
			multiTouchEffect.TouchAction += OnMultiTouchAction;
			multiTouchEffect.Pinched += OnPinched;
		}
	}

	private void OnCanvasTouchAction(object sender, TouchActionEventArgs args)
	{
		// Update pointer indicator position
		if (args.Type == TouchActionType.Moved || args.Type == TouchActionType.Pressed)
		{
			var density = TouchEffect.Density;
			var x = args.Location.X / density;
			var y = args.Location.Y / density;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				PointerIndicator.IsVisible = true;
				PointerIndicator.TranslationX = x - 10; // Center the circle
				PointerIndicator.TranslationY = y - 10;
			});
		}
		else if (args.Type == TouchActionType.Released || args.Type == TouchActionType.Exited)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				PointerIndicator.IsVisible = false;
			});
		}

		// Log event
		LogEvent("Canvas", args);
	}

	private void OnCanvasTapped(object sender, TouchActionEventArgs args)
	{
		LogEvent("Canvas TAPPED", args);
	}

	private void OnCanvasDown(object sender, TouchActionEventArgs args)
	{
		LogEvent("Canvas DOWN", args);
	}

	private void OnCanvasUp(object sender, TouchActionEventArgs args)
	{
		LogEvent("Canvas UP", args);
	}

	private void OnCanvasLongPress(object sender, TouchActionEventArgs args)
	{
		LogEvent("Canvas LONG PRESS", args);

		MainThread.BeginInvokeOnMainThread(() =>
		{
			CanvasLabel.Text = "Long Press Detected!";
		});
	}

	private void OnLeftButtonTouchAction(object sender, TouchActionEventArgs args)
	{
		if (args.Pointer != null)
		{
			// Mouse/pen event
			var pointerInfo = $"Device:{args.Pointer.DeviceType} Btn:{args.Pointer.Button} State:{args.Pointer.State}";

			if (args.Pointer.State == MouseButtonState.Pressed && args.Pointer.Button == MouseButton.Left)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ButtonFeedback.Text = $"LEFT button: {pointerInfo}";
					ButtonFeedback.TextColor = Colors.Blue;
				});
			}

			LogEvent($"LeftBtn {pointerInfo}", args);
		}
		else
		{
			// Touch event
			if (args.Type == TouchActionType.Pressed)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ButtonFeedback.Text = "LEFT button: Touch pressed";
					ButtonFeedback.TextColor = Colors.Green;
				});
			}

			LogEvent("LeftBtn TOUCH", args);
		}
	}

	private void OnRightButtonTouchAction(object sender, TouchActionEventArgs args)
	{
		if (args.Pointer != null)
		{
			// Mouse/pen event
			var pointerInfo = $"Device:{args.Pointer.DeviceType} Btn:{args.Pointer.Button} State:{args.Pointer.State}";

			if (args.Pointer.State == MouseButtonState.Pressed)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ButtonFeedback.Text = $"RIGHT button: {pointerInfo}";
					ButtonFeedback.TextColor = Colors.Red;
				});
			}

			LogEvent($"RightBtn {pointerInfo}", args);
		}
		else
		{
			// Touch event
			if (args.Type == TouchActionType.Pressed)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ButtonFeedback.Text = "RIGHT button: Touch pressed";
					ButtonFeedback.TextColor = Colors.Orange;
				});
			}

			LogEvent("RightBtn TOUCH", args);
		}
	}

	private void OnMultiTouchAction(object sender, TouchActionEventArgs args)
	{
		if (args.Manipulation != null)
		{
			var scale = args.Manipulation.Scale;
			var rotation = args.Manipulation.Rotation;
			var touches = args.Manipulation.TouchesCount;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				MultiTouchLabel.Text = $"Touches: {touches}\nScale: {scale:F2}\nRotation: {rotation:F1}°";
			});

			LogEvent($"MultiTouch Scale:{scale:F2} Rot:{rotation:F1}°", args);
		}
		else
		{
			LogEvent("MultiTouch", args);
		}
	}

	private void OnPinched(object sender, TouchActionEventArgs args)
	{
		LogEvent("PINCHED", args);
	}

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

		MainThread.BeginInvokeOnMainThread(() =>
		{
			_logBuilder.AppendLine(logLine);

			// Trim log if too long
			var lines = _logBuilder.ToString().Split('\n');
			if (lines.Length > MaxLogLines)
			{
				_logBuilder.Clear();
				for (int i = lines.Length - MaxLogLines; i < lines.Length; i++)
				{
					_logBuilder.AppendLine(lines[i]);
				}
			}

			EventLog.Text = _logBuilder.ToString();
		});
	}

	private void OnClearLogClicked(object sender, EventArgs e)
	{
		_logBuilder.Clear();
		_eventCounter = 0;
		EventLog.Text = "";
		ButtonFeedback.Text = "Click a button to test...";
		ButtonFeedback.TextColor = Colors.Gray;
		CanvasLabel.Text = "Move pointer here\nTry left click, right click, hover\nUse trackpad for scrolling";
		MultiTouchLabel.Text = "Pinch with two fingers\nor use trackpad gestures";
	}

	// IGestureListener implementation (currently unused, but available for direct handling)
	public void OnGestureEvent(TouchActionType type, TouchActionEventArgs args, TouchActionResult action)
	{
		// This would be called if we implement IGestureListener directly on controls
		// For now, we're using event handlers above
	}

	public bool InputTransparent => false;
}
