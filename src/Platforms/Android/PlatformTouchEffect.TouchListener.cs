using Android.Content;
using Android.Views;
using View = Android.Views.View;


namespace AppoMobi.Maui.Gestures;

public partial class PlatformTouchEffect
{
    public class TouchListener : GestureDetector.SimpleOnGestureListener, View.IOnTouchListener
    {
        public TouchListener(PlatformTouchEffect platformEffect, Context ctx)
        {
            _parent = platformEffect; //todo remove on dispose
        }

        private volatile PlatformTouchEffect _parent;


        void LockInput(View sender)
        {
            sender.Parent?.RequestDisallowInterceptTouchEvent(true);
        }

        void UnlockInput(View sender)
        {
            sender.Parent?.RequestDisallowInterceptTouchEvent(false);
        }

        public bool OnTouch(View sender, MotionEvent motionEvent)
        {
            //System.Diagnostics.Debug.WriteLine($"[TOUCH] Android: {motionEvent.Action} {motionEvent.RawY:0}");

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
                switch (motionEvent.ActionMasked)
                {
                    //detect additional pointers (i.e., fingers) going down on the screen after the initial touch.
                    //typically used in multi-touch scenarios when multiple fingers are involved.
                    case MotionEventActions.PointerDown:
                        _parent.FireEvent(id, TouchActionType.Pressed, coorsInsideView);
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
                        // Multiple Move events are bundled, so handle them in a loop
                        for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
                        {
                            id = motionEvent.GetPointerId(pointerIndex);
                            coorsInsideView = new PointF(motionEvent.GetX(pointerIndex), motionEvent.GetY(pointerIndex));
                            _parent.FireEvent(id, TouchActionType.Moved, coorsInsideView);
                        }
                        break;

                    case MotionEventActions.PointerUp:
                        _parent.FireEvent(id, TouchActionType.Released,
                            coorsInsideView);
                        break;

                    case MotionEventActions.Up:
                        UnlockInput(sender);
                        _parent.FireEvent(id, TouchActionType.Released,
                            coorsInsideView);
                        break;

                    case MotionEventActions.Cancel:
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
