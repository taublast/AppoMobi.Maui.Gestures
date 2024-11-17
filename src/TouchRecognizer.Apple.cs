﻿using CoreGraphics;
using Foundation;
using UIKit;

namespace AppoMobi.Maui.Gestures
{
    class TouchRecognizer : UIGestureRecognizer
    {

        volatile PlatformTouchEffect _parent;
        UIView _view;

        private bool _disposed;

        public TouchRecognizer(UIView view, PlatformTouchEffect parent)
        {
            _view = view;
            _parent = parent;
        }

        public void Detach()
        {
            try
            {
                _view?.RemoveGestureRecognizer(this);
            }
            catch (Exception e)
            {
                //might still crash when detaching from a destroyed view
                Console.WriteLine(e);
            }
        }

        public void Attach()
        {
            _view?.AddGestureRecognizer(this);
        }

        PointF _lastPoint;

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

            _parent.CountFingers = (int)NumberOfTouches;


            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = ((IntPtr)touch.Handle).ToInt64();
                _parent.FireEvent(id, TouchActionType.Pressed, touch);
            }

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

            var uiTouches = touches.Cast<UITouch>();
            if (uiTouches.Count() > 0)
            {
                foreach (UITouch touch in uiTouches)
                {
                    CGPoint cgPoint = touch.LocationInView(this.View);
                    var xfPoint = new PointF((float)cgPoint.X, (float)cgPoint.Y);
                    bool isInside = CheckPointIsInsideRecognizer(xfPoint, this);
                    long id = ((IntPtr)touch.Handle).ToInt64();

                    if (isInside)
                        _parent.FireEvent(id, TouchActionType.Released, touch);
                    else
                        _parent.FireEvent(id, TouchActionType.Exited, touch);
                }
            }
            else
            {
                _parent.FireEvent(0, TouchActionType.Released, PointF.Zero);
            }

            UnlockTouch();
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
