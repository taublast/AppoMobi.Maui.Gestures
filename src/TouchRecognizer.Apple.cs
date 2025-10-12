using System.Diagnostics;
using CoreGraphics;
using Foundation;
using UIKit;
#if MACCATALYST
using AppKit;
#endif

namespace AppoMobi.Maui.Gestures
{
    class TouchRecognizer : UIGestureRecognizer, IUIGestureRecognizerDelegate
    {

        volatile PlatformTouchEffect _parent;
        UIView _view;

        private bool _disposed;

#if MACCATALYST
        private MouseButtons _currentPressedButtons = MouseButtons.None;
#endif

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
            _view?.AddGestureRecognizer(this);

#if MACCATALYST
            // Enable mouse events for macCatalyst
            if (_view != null)
            {
                _view.UserInteractionEnabled = true;

                // Add hover tracking for mouse movement without button press
                var hoverGestureRecognizer = new UIHoverGestureRecognizer(HandleHover);
                _view.AddGestureRecognizer(hoverGestureRecognizer);

                // Setup trackpad detection for two-finger scrolling
                SetupTrackpadDetection();
            }
#endif

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

#if MACCATALYST
        #region Mouse Event Handling

        private (MouseButton button, int buttonNumber)? GetPressedButtonInfo(UIEvent evt)
        {
            // In macCatalyst, we need to inspect the UIEvent for mouse button information
            // This is a simplified approach - macCatalyst doesn't expose all mouse buttons like Windows

            // Try to detect button type based on event properties
            if (evt.AllTouches != null && evt.AllTouches.Count > 0)
            {
                var touch = evt.AllTouches.AnyObject as UITouch;
                if (touch != null)
                {
                    // Check for right-click characteristics
                    // In macCatalyst, right-click often has different tap count or force characteristics
                    if (touch.TapCount == 0 || (touch.MaximumPossibleForce > 0 && touch.Force > touch.MaximumPossibleForce * 0.8))
                    {
                        // Likely right-click or force touch
                        return (MouseButton.Right, 2);
                    }

                    // Check for middle button (this is very limited in macCatalyst)
                    // We can't reliably detect middle button, so we default to left

                    return (MouseButton.Left, 1);
                }
            }

            return (MouseButton.Left, 1); // Default fallback
        }

        private (MouseButton button, int buttonNumber)? GetReleasedButtonInfo(UIEvent evt)
        {
            // Similar logic for release
            return (MouseButton.Left, 1); // Default fallback
        }

        private MouseButtons GetMouseButtonFlag(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => MouseButtons.Left,
                MouseButton.Right => MouseButtons.Right,
                MouseButton.Middle => MouseButtons.Middle,
                MouseButton.XButton1 => MouseButtons.XButton1,
                MouseButton.XButton2 => MouseButtons.XButton2,
                MouseButton.XButton3 => MouseButtons.XButton3,
                MouseButton.XButton4 => MouseButtons.XButton4,
                MouseButton.XButton5 => MouseButtons.XButton5,
                MouseButton.XButton6 => MouseButtons.XButton6,
                MouseButton.XButton7 => MouseButtons.XButton7,
                MouseButton.XButton8 => MouseButtons.XButton8,
                MouseButton.XButton9 => MouseButtons.XButton9,
                _ => MouseButtons.None
            };
        }

        private PointerDeviceType GetPointerDeviceType(UIEvent evt)
        {
            // In macCatalyst, we can check for Apple Pencil
            if (evt.AllTouches != null && evt.AllTouches.Count > 0)
            {
                var touch = evt.AllTouches.AnyObject as UITouch;
                if (touch != null && touch.Type == UITouchType.Stylus)
                {
                    return PointerDeviceType.Pen;
                }
            }
            return PointerDeviceType.Mouse;
        }

        private PointF GetPointFromEvent(UIEvent evt, UITouch touch)
        {
            var locationInView = touch.LocationInView(_view);
            return new PointF((float)(locationInView.X * TouchEffect.Density), (float)(locationInView.Y * TouchEffect.Density));
        }

        private bool IsMouseEvent(UITouch touch, UIEvent evt)
        {
            // In macCatalyst, we can detect mouse events by checking the touch type
            // Mouse events typically have different characteristics than finger touches
            return touch.Type == UITouchType.Indirect ||
                   (touch.MaximumPossibleForce == 0 && touch.Force == 0); // Mouse doesn't have force
        }

        private void HandleHover(UIHoverGestureRecognizer recognizer)
        {
            if (_parent == null || IsViewOrAncestorHidden(this.View))
                return;

            var location = recognizer.LocationInView(_view);
            var point = new PointF((float)(location.X * TouchEffect.Density), (float)(location.Y * TouchEffect.Density));

            // Determine device type and pressure for hover
            var deviceType = PointerDeviceType.Mouse;
            var pressure = 1.0f; // Default mouse pressure

            // Try to detect Apple Pencil during hover
            // Note: UIHoverGestureRecognizer doesn't provide direct access to pressure during hover
            // This is a limitation of UIKit - pressure is only available during actual touch events

            // Fire hover event with detected device type
            _parent.FireEventPointerWithMouse(0, point, deviceType, pressure);
        }

        #region UIScrollView-based Trackpad Detection

        // macCatalyst doesn't have NSEvent access, so we need to use UIScrollView
        // to detect trackpad scrolling. This is a different approach than pure NSEvent.

        private UIScrollView _trackpadDetectionScrollView;

        private void SetupTrackpadDetection()
        {
            // Create a transparent scroll view to capture trackpad gestures
            _trackpadDetectionScrollView = new UIScrollView
            {
                Frame = _view.Bounds,
                BackgroundColor = UIColor.Clear,
                UserInteractionEnabled = true,
                ScrollEnabled = true,
                ShowsHorizontalScrollIndicator = false,
                ShowsVerticalScrollIndicator = false,
                ContentSize = new CGSize(_view.Bounds.Width * 3, _view.Bounds.Height * 3) // Large content area
            };

            // Set content offset to center
            _trackpadDetectionScrollView.ContentOffset = new CGPoint(_view.Bounds.Width, _view.Bounds.Height);

            // Add scroll detection
            _trackpadDetectionScrollView.Scrolled += OnTrackpadScroll;

            // Insert behind other views so it doesn't interfere
            _view.InsertSubview(_trackpadDetectionScrollView, 0);
        }

        private void OnTrackpadScroll(object sender, EventArgs e)
        {
            if (_parent == null || IsViewOrAncestorHidden(this.View))
                return;

            var scrollView = sender as UIScrollView;
            if (scrollView == null) return;

            // Calculate scroll deltas
            var centerX = _view.Bounds.Width;
            var centerY = _view.Bounds.Height;
            var currentOffset = scrollView.ContentOffset;

            var deltaX = (float)(currentOffset.X - centerX);
            var deltaY = (float)(currentOffset.Y - centerY);

            // Reset to center to allow continuous scrolling
            scrollView.ContentOffset = new CGPoint(centerX, centerY);

            // Only process if there's actual movement
            if (Math.Abs(deltaX) > 0.1f || Math.Abs(deltaY) > 0.1f)
            {
                // Get current touch location (approximate)
                var point = new PointF((float)(_view.Bounds.Width / 2), (float)(_view.Bounds.Height / 2));

                // Fire trackpad pan event
                HandleTrackpadScroll(point, deltaX, deltaY);
            }
        }

        private void HandleTrackpadScroll(PointF point, float deltaX, float deltaY)
        {
            // Create trackpad pan event
            var distance = new PointF(deltaX, deltaY);

            // For UIScrollView-based detection, we don't have momentum phases
            // So we'll use a simpler approach - just fire move events
            var actionType = TouchActionType.Moved;

            // Fire trackpad pan event
            _parent.FireEventWithTrackpadPan(0, actionType, point, distance);
        }

        private void CleanupTrackpadDetection()
        {
            if (_trackpadDetectionScrollView != null)
            {
                _trackpadDetectionScrollView.Scrolled -= OnTrackpadScroll;
                _trackpadDetectionScrollView.RemoveFromSuperview();
                _trackpadDetectionScrollView.Dispose();
                _trackpadDetectionScrollView = null;
            }
        }

        #endregion

        #endregion
#endif

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

#if MACCATALYST
                // Check if this is a mouse event in macCatalyst
                if (IsMouseEvent(touch, evt))
                {
                    var deviceType = GetPointerDeviceType(evt);
                    var point = GetPointFromEvent(evt, touch);
                    var buttonInfo = GetPressedButtonInfo(evt);

                    if (buttonInfo.HasValue)
                    {
                        _currentPressedButtons |= GetMouseButtonFlag(buttonInfo.Value.button);

                        // IMPORTANT: Only use Pressed for primary button (Left/Button 1)
                        // All other buttons use Pointer to avoid breaking existing touch logic
                        if (buttonInfo.Value.button == MouseButton.Left)
                        {
                            // Primary button - use normal touch flow with mouse data
                            _parent.FireEventWithMouse(id, TouchActionType.Pressed, point, buttonInfo.Value.button,
                                buttonInfo.Value.buttonNumber, MouseButtonState.Pressed, deviceType);
                        }
                        else
                        {
                            // Secondary buttons (Right, Middle, XButton1, etc.) - use Pointer
                            _parent.FireEventWithMouse(id, TouchActionType.Pointer, point, buttonInfo.Value.button,
                                buttonInfo.Value.buttonNumber, MouseButtonState.Pressed, deviceType);
                        }
                    }
                    else
                    {
                        // Fallback to regular touch event
                        _parent.FireEvent(id, TouchActionType.Pressed, touch);
                    }
                }
                else
#endif
                {
                    // Regular touch event
                    _parent.FireEvent(id, TouchActionType.Pressed, touch);
                }
            }

            _parent.isInsideView = true;

            CheckLockPan();
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            _parent.CountFingers = (int)NumberOfTouches;

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = ((IntPtr)touch.Handle).ToInt64();

#if MACCATALYST
                // Check if this is a mouse event in macCatalyst
                if (IsMouseEvent(touch, evt))
                {
                    var deviceType = GetPointerDeviceType(evt);
                    var point = GetPointFromEvent(evt, touch);

                    // Handle mouse drag with button tracking
                    _parent.FireEventWithMouseMove(id, TouchActionType.Moved, point, deviceType);
                }
                else
#endif
                {
                    // Regular touch event
                    _parent.FireEvent(id, TouchActionType.Moved, touch);
                }
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
                //else if (_parent.FormsEffect.WIllLock == ShareLockState.Locked)
                //{
                //    // Consumer wants control - fail parent ScrollView if it already started
                //    if (_parentScrollViewRecognizer != null &&
                //        _parentScrollViewRecognizer.State == UIGestureRecognizerState.Changed)
                //    {
                //        _parentScrollViewRecognizer.State = UIGestureRecognizerState.Cancelled;
                //        Debug.WriteLine("Parent ScrollView CANCELLED - taking control");
                //    }
                //}
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

#if MACCATALYST
                    // Check if this is a mouse event in macCatalyst
                    if (IsMouseEvent(touch, evt))
                    {
                        var deviceType = GetPointerDeviceType(evt);
                        var point = GetPointFromEvent(evt, touch);
                        var buttonInfo = GetReleasedButtonInfo(evt);

                        if (buttonInfo.HasValue)
                        {
                            _currentPressedButtons &= ~GetMouseButtonFlag(buttonInfo.Value.button);

                            // IMPORTANT: Only use Released for primary button (Left/Button 1)
                            // All other buttons use Pointer to avoid breaking existing touch logic
                            if (buttonInfo.Value.button == MouseButton.Left)
                            {
                                // Primary button - use normal touch flow with mouse data
                                if (isInside)
                                    _parent.FireEventWithMouse(id, TouchActionType.Released, point, buttonInfo.Value.button,
                                        buttonInfo.Value.buttonNumber, MouseButtonState.Released, deviceType);
                                else
                                    _parent.FireEvent(id, TouchActionType.Exited, touch);
                            }
                            else
                            {
                                // Secondary buttons (Right, Middle, XButton1, etc.) - use Pointer
                                _parent.FireEventWithMouse(id, TouchActionType.Pointer, point, buttonInfo.Value.button,
                                    buttonInfo.Value.buttonNumber, MouseButtonState.Released, deviceType);
                            }
                        }
                        else
                        {
                            // Fallback to regular touch event
                            if (isInside)
                                _parent.FireEvent(id, TouchActionType.Released, touch);
                            else
                                _parent.FireEvent(id, TouchActionType.Exited, touch);
                        }
                    }
                    else
#endif
                    {
                        // Regular touch event
                        if (isInside)
                            _parent.FireEvent(id, TouchActionType.Released, touch);
                        else
                            _parent.FireEvent(id, TouchActionType.Exited, touch);
                    }
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

#if MACCATALYST
            // Clean up trackpad detection
            CleanupTrackpadDetection();
#endif

            _disposed = true;
        }

    }
}
