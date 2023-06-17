using Android.Content;
using Android.Views;
using View = Android.Views.View;


namespace AppoMobi.Maui.Gestures;

public partial class PlatformTouchEffect
{

	/*

   #region SWIPES

   private static int SWIPE_THRESHOLD = 100;
   private static int SWIPE_VELOCITY_THRESHOLD = 100;

   public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
   {
       //Debug.WriteLine($"[TOUCH] !!! SCROLLING !!! {distanceY}");
       return base.OnScroll(e1, e2, distanceX, distanceY);
   }

   public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
   {
       //return base.OnFling(e1, e2, velocityX, velocityY);

       //System.Diagnostics.Debug.WriteLine($"[OnFling] Android Flee: vX {velocityX:0.000}");

       var result = false;
       try
       {
           float diffY = e2.GetY() - e1.GetY();
           float diffX = e2.GetX() - e1.GetX();
           if (Math.Abs(diffX) > Math.Abs(diffY))
           {
               if (Math.Abs(diffX) > SWIPE_THRESHOLD && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
               {
                   if (diffX > 0)
                   {
                       onSwipeRight();
                   }
                   else
                   {
                       onSwipeLeft();
                   }
                   result = true;
               }
           }
           else if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
           {
               if (diffY > 0)
               {
                   onSwipeBottom();
               }
               else
               {
                   onSwipeTop();
               }
               result = true;
           }
       }
       catch (Exception exception)
       {
           Console.WriteLine(exception);
       }
       return result;
   }

   private TouchActionType _swipe;

   public virtual void onSwipeRight()
   {
       _swipe = TouchActionType.SwipeRight;
   }

   public virtual void onSwipeLeft()
   {
       _swipe = TouchActionType.SwipeLeft;
   }

   public virtual void onSwipeTop()
   {
       _swipe = TouchActionType.SwipeTop;
   }

   public virtual void onSwipeBottom()
   {
       _swipe = TouchActionType.SwipeBottom;
   }

   #endregion

   */

	public class TouchListener : GestureDetector.SimpleOnGestureListener, View.IOnTouchListener
	{

		public static bool UseLowCpu = false;

		public bool PinchEnabled = false;

		#region ROTATION

		private static readonly float AngleThreshold = 15f; // Adjust the angle threshold as needed
		private float _startAngle;
		bool _isRotating;

		private static float GetAngle(MotionEvent e)
		{
			float dx = e.GetX(0) - e.GetX(1);
			float dy = e.GetY(0) - e.GetY(1);
			return (float)(Math.Atan2(dy, dx) * (180 / Math.PI));
		}

		#endregion

		#region PINCH

		ScaleListener _scaleListener;

		ScaleGestureDetector scaleGestureDetector;

		private float lastPich;
		public void OnScaleChanged(object sender, TouchEffect.EventArgsScale e)
		{
			lastPich = e.Scale;
		}

		#endregion

		public TouchListener(PlatformTouchEffect platformEffect, Context ctx)
		{
			_parent = platformEffect; //todo remove on dispose

			//!!! todo ADD CLEANUP !!!
			_scaleListener = new(ctx, this);
			scaleGestureDetector = new ScaleGestureDetector(ctx, _scaleListener);
		}

		private volatile PlatformTouchEffect _parent;


		void LockInput(View sender)
		{
			sender.Parent?.RequestDisallowInterceptTouchEvent(true);
			//    Debug.WriteLine($"[****MODE2*] LOCKED");
		}

		void UnlockInput(View sender)
		{
			sender.Parent?.RequestDisallowInterceptTouchEvent(false);
			//      Debug.WriteLine($"[****MODE2*] UN-LOCKED");
		}




		public bool IsPinching { get; set; }

		DateTime _lastEventTime = DateTime.Now;

		public bool OnTouch(View sender, MotionEvent motionEvent)
		{
			//System.Diagnostics.Debug.WriteLine($"[TOUCH] Android: {motionEvent.Action} {motionEvent.RawY:0}");

			//var _parent = GetParent(sender);

			if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Disabled)
				return false;

			_parent.CountFingers = motionEvent.PointerCount;

			// Get the pointer index
			int pointerIndex = motionEvent.ActionIndex;

			// Get the id that identifies a finger over the course of its progress
			int id = motionEvent.GetPointerId(pointerIndex);

			//Pixels relative to the view, not the screen 
			var coorsInsideView = new PointF(motionEvent.GetX(pointerIndex), motionEvent.GetY(pointerIndex));
			_parent.isInsideView = coorsInsideView.X >= 0 && coorsInsideView.X <= sender.Width && coorsInsideView.Y >= 0 && coorsInsideView.Y <= sender.Height;

			try
			{
				//_swipe = TouchActionType.Cancelled;

				//todo detect multitouch!!!

				var isPinching = false;
				if (scaleGestureDetector != null)
					scaleGestureDetector.OnTouchEvent(motionEvent);

				var swiped = false; //todo
				var canRemove = true && !swiped && !isPinching;

				if (IsPinching)
				{
					_parent.FireEvent(id,
						TouchActionType.Pinch, PointF.Zero, lastPich);
					return true;
				}

				switch (motionEvent.ActionMasked)
				{
					//detect additional pointers (i.e., fingers) going down on the screen after the initial touch.
					//typically used in multi-touch scenarios when multiple fingers are involved.
					case MotionEventActions.PointerDown:
						_isRotating = true;
						_startAngle = GetAngle(motionEvent);
						break;

					case MotionEventActions.Down:
						//case MotionEventActions.PointerDown:

						if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Lock)
							LockInput(sender);
						else
							UnlockInput(sender);

						_parent.FireEvent(id, TouchActionType.Pressed, coorsInsideView);

						break;

					case MotionEventActions.Move:

						if (UseLowCpu)
						{
							var now = DateTime.Now;
							if ((now - _lastEventTime).TotalMilliseconds < 16)
							{
								break;
							}
							_lastEventTime = now;
						}

						//if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Lock)
						//    LockInput(sender);
						//else
						//    UnlockInput(sender);

						// Multiple Move events are bundled, so handle them in a loop
						for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
						{
							id = motionEvent.GetPointerId(pointerIndex);

							coorsInsideView = new PointF(motionEvent.GetX(pointerIndex), motionEvent.GetY(pointerIndex));


							_parent.FireEvent(id, TouchActionType.Moved, coorsInsideView);

						}

						if (_isRotating)
						{
							float currentAngle = GetAngle(motionEvent);
							float deltaAngle = currentAngle - _startAngle;

							if (Math.Abs(deltaAngle) > AngleThreshold)
							{
								_parent.FireEvent(id, TouchActionType.Rotated, coorsInsideView);
								_startAngle = currentAngle;
							}
						}

						break;

					case MotionEventActions.Up:
						//case MotionEventActions.Pointer1Up:
						_isRotating = false;

						UnlockInput(sender);

						_parent.FireEvent(id, TouchActionType.Released,
							coorsInsideView);

						break;
					case MotionEventActions.Cancel:

						//Debug.WriteLine($"[TOUCH] Android native: {motionEvent.ActionMasked} - {_parent.capture}");

						UnlockInput(sender);

						_parent.FireEvent(id, TouchActionType.Cancelled, coorsInsideView);

						break;

					default:

						UnlockInput(sender);

						break;

				}



			}
			catch (Exception e)
			{
				Console.WriteLine($"[TOUCH] Android: {motionEvent} - {e}");
			}

			return true;
		}


	}
}