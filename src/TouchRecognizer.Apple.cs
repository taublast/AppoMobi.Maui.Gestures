using System.Diagnostics;
using CoreGraphics;
using Foundation;
using UIKit;

namespace AppoMobi.Maui.Gestures
{
    class TouchRecognizer : UIGestureRecognizer, IUIGestureRecognizerDelegate
    {

        volatile PlatformTouchEffect _parent;
        UIView _view;

        private bool _disposed;

        public TouchRecognizer(UIView view, PlatformTouchEffect parent)
        {
            _view = view;
            _parent = parent;
        }

        #region Lock Pan

        void CheckLockPan()
        {
            if (_disposed || _parent == null || _parent.FormsEffect == null)
                return;

            if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Lock || _parent.FormsEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                AttachPanning();
            }
            else
                if (_childPanGestureRecognizer != null)
            {
                DetachPanning();
            }
        }

        private UIPanGestureRecognizer _childPanGestureRecognizer; // Reference to child UIPanGestureRecognizer
        private UIGestureRecognizer _parentScrollViewRecognizer; // Reference to parent ScrollView's recognizer

        void AttachPanning()
        {
            if (_childPanGestureRecognizer != null)
                return;

            _childPanGestureRecognizer = new UIPanGestureRecognizer(HandlePan)
            {
                CancelsTouchesInView = true, // Allow touches to propagate
                Delegate = this
            };
            _view.AddGestureRecognizer(_childPanGestureRecognizer);
        }


        void DetachPanning()
        {
            if (_childPanGestureRecognizer != null)
            {
                _view?.RemoveGestureRecognizer(_childPanGestureRecognizer);
                _childPanGestureRecognizer.Delegate = null;
                _childPanGestureRecognizer = null;
            }
        }

        /// <summary>
        /// Handles the pan gesture action.
        /// For Lock mode: just blocks parent scrollviews by existing.
        /// For SoftLock mode: controlled dynamically via State in TouchesMoved.
        /// </summary>
        /// <param name="gesture">The UIPanGestureRecognizer triggering the action.</param>
        private void HandlePan(UIPanGestureRecognizer gesture)
        {
            // Do nothing - just being active blocks parent when needed
            // SoftLock mode dynamically fails this gesture in TouchesMoved when not handled
        }


        #endregion

        public void Detach()
        {
            try
            {
                _view?.RemoveGestureRecognizer(this);

                DetachPanning(); ;
            }
            catch (Exception e)
            {
                //might still crash when detaching from a destroyed view
                Console.WriteLine(e);
            }
        }

        public void Attach()
        {
            this.CancelsTouchesInView = false; // Allow touches to propagate
            _view?.AddGestureRecognizer(this);

            CheckLockPan();
        }

        PointF _lastPoint;

        public override bool ShouldRequireFailureOfGestureRecognizer(UIGestureRecognizer otherGestureRecognizer)
        {
            return false;
        }

        private bool ShouldRecLocked(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
        {
            if (othergesturerecognizer == _childPanGestureRecognizer || othergesturerecognizer == this)
            {
                return true; // Allow simultaneous recognition with child UIPanGestureRecognizer
            }

            // Capture reference to parent ScrollView's pan recognizer for SoftLock mode
            if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                if (othergesturerecognizer is UIPanGestureRecognizer &&
                    othergesturerecognizer != _childPanGestureRecognizer &&
                    _parentScrollViewRecognizer == null)
                {
                    _parentScrollViewRecognizer = othergesturerecognizer;
                    Debug.WriteLine($"Captured parent recognizer: {othergesturerecognizer.GetType().Name}");
                }
            }

            return false;
        }

        private bool SetTrueCapture(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
        {
            // Capture reference to parent ScrollView's pan recognizer for SoftLock mode
            if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                if (othergesturerecognizer is UIPanGestureRecognizer &&
                    othergesturerecognizer != _childPanGestureRecognizer &&
                    _parentScrollViewRecognizer == null)
                {
                    _parentScrollViewRecognizer = othergesturerecognizer;
                    Debug.WriteLine($"Captured parent recognizer: {othergesturerecognizer.GetType().Name}");
                }
            }

            return true;
        }

        private bool SetTrue(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
        {
            return true;
        }

        private bool SetFalse(UIGestureRecognizer gesturerecognizer, UIGestureRecognizer othergesturerecognizer)
        {
            return false;
        }

        void SoftLockTouch()
        {
            // For SoftLock: recognize simultaneously and require us to fail to pass gesture to parent
            ShouldBeRequiredToFailBy = SetFalse;
            ShouldRecognizeSimultaneously = SetTrueCapture;  
        }

        void LockTouch()
        {
            ShouldBeRequiredToFailBy =  SetTrue;
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

            CheckLockPan();

            this.CancelsTouchesInView = false;

            if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Lock)
            {
                LockTouch();
            }
            else if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                SoftLockTouch();
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

            // For SoftLock mode: dynamically control gesture blocking based on WIllLock state
            if (_parent.FormsEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                if (_parent.FormsEffect.WIllLock == ShareLockState.Unlocked)
                {
                    // Consumer didn't handle - fail child pan to release parent ScrollView
                    if (_childPanGestureRecognizer != null)
                    {
                        _childPanGestureRecognizer.State = UIGestureRecognizerState.Failed;
                        Debug.WriteLine("Child pan FAILED - releasing to parent");
                    }
                }
                else if (_parent.FormsEffect.WIllLock == ShareLockState.Locked)
                {
                    // Consumer wants control - fail parent ScrollView if it already started
                    if (_parentScrollViewRecognizer != null &&
                        _parentScrollViewRecognizer.State == UIGestureRecognizerState.Changed)
                    {
                        _parentScrollViewRecognizer.State = UIGestureRecognizerState.Cancelled;
                        Debug.WriteLine("Parent ScrollView CANCELLED - taking control");
                    }
                }
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

            // Reset parent recognizer reference for next gesture
            _parentScrollViewRecognizer = null;
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

            // Reset parent recognizer reference for next gesture
            _parentScrollViewRecognizer = null;
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
