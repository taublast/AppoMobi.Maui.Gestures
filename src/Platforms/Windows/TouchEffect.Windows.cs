using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

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
                frameworkElement.PointerWheelChanged += OnWheelChanged;
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
                frameworkElement.PointerWheelChanged -= OnWheelChanged;
            }
        }

        private bool _pressed = false;

        private volatile TouchEffect _touchEffect;

        private readonly HashSet<uint> activePointerIds = new HashSet<uint>();

        private readonly HashSet<uint> capturedPointerIds = new HashSet<uint>();

        public float ScaleLimitMin { get; set; } = 0.1f;

        public float ScaleLimitMax { get; set; } = 1000.0f;

        public float ScaleFactor { get; set; } = 1.0f;

        public float WheelDelta { get; set; } = 40 / 0.1f;

        private void OnWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            //_pressed = true;
            var id = args.Pointer.PointerId;

            var pointerPoint = args.GetCurrentPoint(frameworkElement);
            var windowsPoint = pointerPoint.Position;
            //var mouse = GetMouseButton(pointerPoint);
            //var device = GetTouchDevice(evt);
            var wheelDelta = pointerPoint?.Properties?.MouseWheelDelta ?? 0;

            float scaleFactorAdjustment = wheelDelta > 0 ? 1.05f : 0.95f;
            ScaleFactor = Math.Max(ScaleLimitMin, Math.Min(ScaleFactor * scaleFactorAdjustment, ScaleLimitMax));

            activePointerIds.Add(args.Pointer.PointerId);
            Wheel = new TouchEffect.WheelEventArgs()
            {
                Delta = wheelDelta / WheelDelta,
                Scale = (float)ScaleFactor,
                Center = new PointF((float)windowsPoint.X * TouchEffect.Density, (float)windowsPoint.Y * TouchEffect.Density)
            };

            // Always fire the event to YOUR TouchEffect first
            FireEvent(sender, TouchActionType.Wheel, args);

            // Then decide whether to block it from parent ScrollView
            // For Manual mode: block wheel events when WIllLock is Locked (consumer has control)
            if (_touchEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                if (_touchEffect.WIllLock == ShareLockState.Locked)
                {
                    // Consumer has control - block wheel from reaching parent ScrollView
                    args.Handled = true;
                    System.Diagnostics.Debug.WriteLine("Windows: Wheel event delivered to effect but BLOCKED from parent - Manual mode locked");
                }
            }
            // For Lock mode: always block wheel events from parent
            else if (_touchEffect.TouchMode == TouchHandlingStyle.Lock)
            {
                args.Handled = true;
                System.Diagnostics.Debug.WriteLine("Windows: Wheel event delivered to effect but BLOCKED from parent - Lock mode");
            }
        }

        void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            _pressed = true;
            activePointerIds.Add(args.Pointer.PointerId);

            // Handle pointer capture based on touch mode
            if (_touchEffect.TouchMode == TouchHandlingStyle.Lock)
            {
                // Lock mode: always capture pointer to block parent
                if (frameworkElement.CapturePointer(args.Pointer))
                {
                    capturedPointerIds.Add(args.Pointer.PointerId);
                }
                frameworkElement.ManipulationMode = ManipulationModes.None;
            }
            else if (_touchEffect.TouchMode == TouchHandlingStyle.Manual)
            {
                // Manual mode: start without capture, will be controlled dynamically in Move
                frameworkElement.ManipulationMode = ManipulationModes.System;
            }
            else
            {
                // Default mode: capture pointer with system manipulation
                if (frameworkElement.CapturePointer(args.Pointer))
                {
                    capturedPointerIds.Add(args.Pointer.PointerId);
                }
                frameworkElement.ManipulationMode = ManipulationModes.System;
            }

            FireEvent(sender, TouchActionType.Pressed, args);
        }

        void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (_pressed)
            {
                FireEvent(sender, TouchActionType.Moved, args);

                // For Manual mode: dynamically control pointer capture and manipulation based on WIllLock state
                if (_touchEffect.TouchMode == TouchHandlingStyle.Manual)
                {
                    if (_touchEffect.WIllLock == ShareLockState.Locked)
                    {
                        // Consumer wants control - capture pointer and disable manipulation to block parent
                        if (!capturedPointerIds.Contains(args.Pointer.PointerId))
                        {
                            if (frameworkElement.CapturePointer(args.Pointer))
                            {
                                capturedPointerIds.Add(args.Pointer.PointerId);
                                frameworkElement.ManipulationMode = ManipulationModes.None;
                                System.Diagnostics.Debug.WriteLine("Windows: Pointer CAPTURED + ManipulationMode.None - taking control");
                            }
                        }
                    }
                    else if (_touchEffect.WIllLock == ShareLockState.Unlocked)
                    {
                        // Consumer doesn't want control - release pointer and enable system manipulation to allow parent
                        if (capturedPointerIds.Contains(args.Pointer.PointerId))
                        {
                            frameworkElement.ReleasePointerCapture(args.Pointer);
                            capturedPointerIds.Remove(args.Pointer.PointerId);
                            frameworkElement.ManipulationMode = ManipulationModes.System;
                            System.Diagnostics.Debug.WriteLine("Windows: Pointer RELEASED + ManipulationMode.System - releasing to parent");
                        }
                    }
                    // For Initial state, do nothing (keep previous state)
                }
            }
        }

        void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            try
            {
                activePointerIds.Remove(args.Pointer.PointerId);
                FireEvent(sender, TouchActionType.Released, args);
                _pressed = activePointerIds.Count > 0;

                // Release pointer capture for Manual/Lock modes when all fingers are released
                if (!_pressed)
                {
                    if (capturedPointerIds.Contains(args.Pointer.PointerId))
                    {
                        frameworkElement.ReleasePointerCapture(args.Pointer);
                        capturedPointerIds.Remove(args.Pointer.PointerId);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void OnPointerExited(object sender, PointerRoutedEventArgs args)
        {
            try
            {
                if (_pressed)
                {
                    activePointerIds.Remove(args.Pointer.PointerId);
                    _pressed = activePointerIds.Count > 0;
                    FireEvent(sender, TouchActionType.Exited, args);

                    // Release pointer capture when all fingers are gone
                    if (!_pressed)
                    {
                        if (capturedPointerIds.Contains(args.Pointer.PointerId))
                        {
                            frameworkElement.ReleasePointerCapture(args.Pointer);
                            capturedPointerIds.Remove(args.Pointer.PointerId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

                args.Wheel = Wheel;

                if (pointer.Pointer.IsInContact)
                {
                    args.NumberOfTouches = activePointerIds.Count;
                }
                else
                {
                    args.NumberOfTouches = 1; //last finger released, and it was 1
                }

                //Trace.WriteLine($"TouchEffect: {touchActionType} {args.Location.X}x{args.Location.Y} {args.NumberOfTouches}");

                _touchEffect?.OnTouchAction(args);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
