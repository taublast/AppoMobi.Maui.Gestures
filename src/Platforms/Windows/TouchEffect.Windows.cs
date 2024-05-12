using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.ComponentModel;

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
            FireEvent(sender, TouchActionType.Wheel, args);
        }

        void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            _pressed = true;
            activePointerIds.Add(args.Pointer.PointerId);
            FireEvent(sender, TouchActionType.Pressed, args);
        }

        void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (_pressed)
                FireEvent(sender, TouchActionType.Moved, args);
        }

        void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            activePointerIds.Remove(args.Pointer.PointerId);
            FireEvent(sender, TouchActionType.Released, args);
            _pressed = activePointerIds.Count > 0;
        }

        void OnPointerExited(object sender, PointerRoutedEventArgs args)
        {
            if (_pressed)
            {
                activePointerIds.Remove(args.Pointer.PointerId);
                _pressed = activePointerIds.Count > 0;
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
