using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace ModernUI.Behaviors
{
    /// <summary>
    /// </summary>
    public class RepositionPopupBehavior : Behavior<Popup>
    {
        /// <summary>
        /// 
        /// </summary>
        public bool EventsAttached { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Opened += AssociatedObject_Opened;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_Opened(object sender, EventArgs e)
        {
            if (EventsAttached)
                return;

            Window window = Window.GetWindow(AssociatedObject);

            if (window == null)
                return;

            window.LocationChanged += Window_LocationChanged;
            window.SizeChanged += Window_SizeChanged;

            EventsAttached = true;
        }


        void Window_LocationChanged(object sender, EventArgs e) => UpdatePosition();
        void Window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdatePosition();

        void UpdatePosition()
        {
            if (!AssociatedObject.IsOpen)
                return;

            AssociatedObject.HorizontalOffset++;
            AssociatedObject.HorizontalOffset--;
        }
    }
}