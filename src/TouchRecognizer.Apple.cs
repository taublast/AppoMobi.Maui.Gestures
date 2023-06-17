// ReSharper restore All

using CoreGraphics;
using Foundation;
using System.Diagnostics;
using UIKit;

namespace AppoMobi.Maui.Gestures
{
	class TouchRecognizer : UIGestureRecognizer
	{

		volatile PlatformTouchEffect _parent;
		UIView _view;

		UIPinchGestureRecognizer pinchGestureRecognizer;
		private bool _disposed;

		public TouchRecognizer(UIView view, PlatformTouchEffect parent)
		{
			_view = view;
			_parent = parent;
		}


		public void Detach()
		{

			_view.RemoveGestureRecognizer(pinchGestureRecognizer);

			pinchGestureRecognizer?.Dispose();

			pinchGestureRecognizer = null;

			_view.RemoveGestureRecognizer(this);
		}

		public void Attach()
		{
			_view.AddGestureRecognizer(this);

			pinchGestureRecognizer = new UIPinchGestureRecognizer(() =>
			{
				_parent.lastPich = (float)pinchGestureRecognizer.Scale;
				_parent.CountFingers = (int)pinchGestureRecognizer.NumberOfTouches;

				var cgPoint = pinchGestureRecognizer.LocationOfTouch(0, null);
				//CGPoint cgPoint = touch.LocationInView(recognizer.View);
				var point = new PointF((float)cgPoint.X, (float)cgPoint.Y);


				if (pinchGestureRecognizer.State == UIGestureRecognizerState.Ended
					|| pinchGestureRecognizer.State == UIGestureRecognizerState.Cancelled)
				{
					Debug.WriteLine("pinchGestureRecognizer.State == UIGestureRecognizerState.Ended");
					//FireEvent(0, TouchActionType.Cancelled, point);
				}
				else
				{
					_parent.FireEvent(0, TouchActionType.Pinch, point);
				}


			});

			_view.AddGestureRecognizer(pinchGestureRecognizer);
		}


		private bool ShouldRecLocked(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
		{
			return true;
		}
		private bool ShouldRecUnlocked(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
		{
			return false;
		}

		private bool ShouldFailLocked(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
		{
			return false;
		}

		void ShareTouch()
		{
			ShouldBeRequiredToFailBy = ShouldRecUnlocked;
			ShouldRecognizeSimultaneously = ShouldRecLocked;
		}
		void LockTouch()
		{
			ShouldBeRequiredToFailBy = ShouldFailLocked;
			ShouldRecognizeSimultaneously = ShouldRecLocked;

			//if (UIDevice.CurrentDevice.CheckSystemVersion(13, 4))
			//{
			//    ShouldReceiveEvent = ShouldEvent;
			//}

			//  Debug.WriteLine("[TOUCH] LOCKED!");
		}

		private bool ShouldEvent(UIGestureRecognizer gesturerecognizer, UIEvent @event)
		{
			return true;
		}

		void UnlockTouch()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(13, 4))
			{
				ShouldReceiveEvent = ShouldEvent;
			}

			ShouldBeRequiredToFailBy = null;
			ShouldRecognizeSimultaneously = null;

			//  Debug.WriteLine("[TOUCH] UNlocked!");
		}

		private bool IsViewOrAncestorHidden(UIView view)
		{
			if (view == null)
			{
				return false;
			}
			return view.Hidden || view.Alpha == 0 || IsViewOrAncestorHidden(view.Superview);
		}

		// touches = touches of interest; evt = all touches of type UITouch
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			if (IsViewOrAncestorHidden(this.View))
			{
				this.State = UIGestureRecognizerState.Failed;
				return;
			}

			//Console.WriteLine("TouchesBegan");

			_parent.CountFingers = (int)NumberOfTouches;


			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				long id = ((IntPtr)touch.Handle).ToInt64();
				_parent.FireEvent(id, TouchActionType.Pressed, touch);
			}

			// Save the setting of the Capture property
			//capture = TouchEffect.Capture;
			_parent.isInsideView = true;

			if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Lock)
			{
				LockTouch();
			}
			else
			if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Share)
			{
				ShareTouch();
			}
			else
			{
				UnlockTouch();
			}
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			//Console.WriteLine("TouchesMoved");

			base.TouchesMoved(touches, evt);

			_parent.CountFingers = (int)NumberOfTouches;

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				long id = ((IntPtr)touch.Handle).ToInt64();
				_parent.FireEvent(id, TouchActionType.Moved, touch);
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			_parent.CountFingers = (int)NumberOfTouches;

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				CGPoint cgPoint = touch.LocationInView(this.View);
				var xfPoint = new PointF((float)cgPoint.X, (float)cgPoint.Y);
				bool isInside = CheckPointIsInsideRecognizer(xfPoint, this);
				long id = ((IntPtr)touch.Handle).ToInt64();

				if (isInside)
					_parent.FireEvent(id, TouchActionType.Released, touch);
				else
					_parent.FireEvent(id, TouchActionType.Exited, touch);

				UnlockTouch();
			}

		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				long id = ((IntPtr)touch.Handle).ToInt64();
				_parent.FireEvent(id, TouchActionType.Cancelled, touch);
			}

			UnlockTouch();

		}

		public override bool CancelsTouchesInView
		{
			get
			{
				return false; //todo if TRUE allowes scrollView to cancel our touches
			}
		}

		//void CheckForBoundaryHop(UITouch touch)
		//{
		//	long id = ((IntPtr)touch.Handle).ToInt64();

		//	// TODO: Might require converting to a List for multiple hits
		//	TouchRecognizer touchEffectHit = null;

		//	foreach (UIView view in viewDictionary.Keys)
		//	{
		//		try
		//		{
		//			CGPoint location = touch.LocationInView(view);
		//			if (new CGRect(new CGPoint(), view.Frame.Size).Contains(location))
		//			{
		//				touchEffectHit = viewDictionary[view];
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			continue; //view might be disposed
		//		}
		//	}

		//	if (touchEffectHit != idToEffectDictionary[id])
		//	{
		//		if (touchEffectHit == null)
		//		{
		//			idToEffectDictionary[id] = null;
		//		}
		//	}
		//}

		bool CheckPointIsInsideRecognizer(PointF xfPoint, TouchRecognizer recognizer)
		{
			if (xfPoint.Y < 0 || xfPoint.Y > recognizer.View.Bounds.Height)
			{
				return false;
			}

			if (xfPoint.X < 0 || xfPoint.X > recognizer.View.Bounds.Width)
			{
				return false;
			}

			return true;
		}



		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			_disposed = true;
		}

	}
}
