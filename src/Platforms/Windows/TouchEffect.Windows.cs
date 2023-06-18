using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace AppoMobi.Maui.Gestures
{
	public partial class PlatformTouchEffect : PlatformEffect
	{
		FrameworkElement frameworkElement;


		protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(args);

			if (args.PropertyName == "Window" && Element is View view)
			{
				//if (view.Window == null)
				//    OnDetached();
			}
		}
		protected override void OnAttached()
		{
			// Get the Windows FrameworkElement corresponding to the Element that the effect is attached to
			frameworkElement = Control == null ? Container : Control;

			// Get access to the TouchEffect class in the .NET Standard library
			_touchEffect = Element.Effects.OfType<TouchEffect>().FirstOrDefault();

			if (_touchEffect != null && frameworkElement != null)
			{
				// Save the method to call on touch events

				// Set event handlers on FrameworkElement
				frameworkElement.PointerPressed += OnPointerPressed;
				frameworkElement.PointerMoved += OnPointerMoved;
				frameworkElement.PointerReleased += OnPointerReleased;
				frameworkElement.PointerExited += OnPointerExited;
			}
		}


		protected override void OnDetached()
		{
			if (frameworkElement != null)
			{
				frameworkElement.PointerPressed -= OnPointerPressed;
				frameworkElement.PointerMoved -= OnPointerMoved;
				frameworkElement.PointerReleased -= OnPointerReleased;
				frameworkElement.PointerExited -= OnPointerExited;
			}
		}

		private bool _pressed = false;

		private volatile TouchEffect _touchEffect;

		void OnPointerPressed(object sender, PointerRoutedEventArgs args)
		{
			_pressed = true;
			FireEvent(sender, TouchActionType.Pressed, args);
		}

		void OnPointerMoved(object sender, PointerRoutedEventArgs args)
		{
			if (_pressed)
				FireEvent(sender, TouchActionType.Moved, args);
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs args)
		{
			FireEvent(sender, TouchActionType.Released, args);
			_pressed = false;
		}

		private void OnPointerExited(object sender, PointerRoutedEventArgs args)
		{
			if (_pressed)
			{
				_pressed = false;
				FireEvent(sender, TouchActionType.Exited, args);
			}
		}


		void FireEvent(object sender, TouchActionType touchActionType, PointerRoutedEventArgs pointer)
		{
			try
			{

				var pointerPoint = pointer.GetCurrentPoint(sender as UIElement);

				var windowsPoint = pointerPoint.Position;

				var args = new TouchActionEventArgs(
					pointer.Pointer.PointerId,
					touchActionType,
					new Microsoft.Maui.Graphics.PointF((float)(windowsPoint.X * TouchEffect.Density), (float)(windowsPoint.Y * TouchEffect.Density)), null);

				args.IsInsideView = _pressed;

				if (pointer.Pointer.IsInContact)
				{
					var points = pointer.GetIntermediatePoints(null);
					if (points != null)
					{
						args.NumberOfTouches = points.Count;
					}
				}

				Trace.WriteLine($"TouchEffect: {touchActionType} {args.Location.X}x{args.Location.Y}");

				_touchEffect?.OnTouchAction(args);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}

		}
	}
}
