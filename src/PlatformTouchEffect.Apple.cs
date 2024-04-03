using CoreGraphics;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using UIKit;

namespace AppoMobi.Maui.Gestures
{
    public partial class PlatformTouchEffect : PlatformEffect
    {

        UIView _appleView;
        TouchRecognizer _touchRecognizer;

        public void FireEvent(long id, TouchActionType actionType, PointF point)
        {
            try
            {
                var args = new TouchActionEventArgs(id, actionType, point, null);
                args.Pinch = Pinch;
                args.NumberOfTouches = CountFingers;
                args.IsInsideView = isInsideView;

                FormsEffect?.OnTouchAction(args);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public void FireEvent(long id, TouchActionType actionType, UITouch touch, double pinch = 0.0)
        {

            try
            {
                // ios gives us points but the funny part we want pixels
                CGPoint cgPoint = touch.LocationInView(_appleView);
                var point = new PointF((float)(cgPoint.X * TouchEffect.Density), (float)(cgPoint.Y * TouchEffect.Density));

                FireEvent(id, actionType, point);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == "Window" && Element is View view)
            {
                //if (view.Window == null)
                //    OnDetached();
            }
        }

        private void OnParentVIewChanged(object sender, EventArgs e)
        {
            if (sender is Element element)
            {
                if (element.Parent == null)
                {
                    OnDetached();
                }
            }
        }

        protected override void OnAttached()
        {


            // Get the iOS UIView corresponding to the Element that the effect is attached to
            _appleView = Control == null ? Container : Control;

            // Get access to the TouchEffect class in the .NET Standard library
            FormsEffect = Element.Effects.FirstOrDefault(e => e is TouchEffect) as TouchEffect;


            if (FormsEffect != null && _appleView != null)
            {

                if (this.Element.Parent != null)
                {
                    //gonna watch parent changing, if parent becomes null we gonna dispose this effect thank you
                    this.Element.ParentChanged += OnParentVIewChanged;

                }


                // Create a TouchRecognizer for this UIView
                _touchRecognizer = new TouchRecognizer(_appleView, this);

                _touchRecognizer.Attach();

                //panRecognizer = new PanGestureRecognizer(Element, view, touchEffect);
                //view.AddGestureRecognizer(panRecognizer);

                FormsEffect.Disposing += OnFormsDisposing;
            }
        }



        private void OnFormsDisposing(object sender, EventArgs e)
        {
            OnDetached();
        }

        protected override void OnDetached()
        {
            if (FormsEffect != null)
            {

                if (this.Element != null)
                {
                    this.Element.ParentChanged -= OnParentVIewChanged;
                }

                FormsEffect.Disposing -= OnFormsDisposing;
                FormsEffect?.Dispose();
                FormsEffect = null;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _touchRecognizer?.Detach();
                _touchRecognizer?.Dispose();
                _touchRecognizer = null;
            });
        }
    }
}