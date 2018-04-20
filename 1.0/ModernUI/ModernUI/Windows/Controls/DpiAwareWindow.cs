using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using ModernUI.Win32;

namespace ModernUI.Windows.Controls
{
    /// <summary>
    ///     A window instance that is capable of per-monitor DPI awareness when supported.
    /// </summary>
    public abstract class DpiAwareWindow
        : Window
    {
        readonly bool isPerMonitorDpiAware;

        HwndSource source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DpiAwareWindow" /> class.
        /// </summary>
        protected DpiAwareWindow()
        {
            SourceInitialized += OnSourceInitialized;

            // WM_DPICHANGED is not send when window is minimized, do listen to global display setting changes
            SystemEvents.DisplaySettingsChanged += OnSystemEventsDisplaySettingsChanged;

            // try to set per-monitor dpi awareness, before the window is displayed
            isPerMonitorDpiAware = ModernUIHelper.TrySetPerMonitorDpiAware();
        }

        /// <summary>
        ///     Gets the DPI information for this window instance.
        /// </summary>
        /// <remarks>
        ///     DPI information is available after a window handle has been created.
        /// </remarks>
        public DpiInformation DpiInformation { get; private set; }

        /// <summary>
        ///     Occurs when the system or monitor DPI for this window has changed.
        /// </summary>
        public event EventHandler DpiChanged;

        /// <summary>
        ///     Raises the System.Windows.Window.Closed event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // detach global event handlers
            SystemEvents.DisplaySettingsChanged -= OnSystemEventsDisplaySettingsChanged;
        }

        void OnSystemEventsDisplaySettingsChanged(object sender, EventArgs e)
        {
            if (source != null && WindowState == WindowState.Minimized)
            {
                RefreshMonitorDpi();
            }
        }

        void OnSourceInitialized(object sender, EventArgs e)
        {
            source = (HwndSource)PresentationSource.FromVisual(this);

            // calculate the DPI used by WPF; this is the same as the system DPI
            Matrix matrix = source.CompositionTarget.TransformToDevice;

            DpiInformation = new DpiInformation(96D * matrix.M11, 96D * matrix.M22);

            if (isPerMonitorDpiAware)
            {
                source.AddHook(WndProc);

                RefreshMonitorDpi();
            }
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_DPICHANGED)
            {
                // Marshal the value in the lParam into a Rect.
                RECT newDisplayRect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

                // Set the Window's position & size.
                Matrix matrix = source.CompositionTarget.TransformFromDevice;
                Vector ul = matrix.Transform(new Vector(newDisplayRect.left, newDisplayRect.top));
                Vector hw = matrix.Transform(new Vector(newDisplayRect.right - newDisplayRect.left,
                    newDisplayRect.bottom - newDisplayRect.top));
                Left = ul.X;
                Top = ul.Y;
                UpdateWindowSize(hw.X, hw.Y);

                // Remember the current DPI settings.
                double? oldDpiX = DpiInformation.MonitorDpiX;
                double? oldDpiY = DpiInformation.MonitorDpiY;

                // Get the new DPI settings from wParam
                double dpiX = wParam.ToInt32() >> 16;
                double dpiY = wParam.ToInt32() & 0x0000FFFF;

                if (Math.Abs(oldDpiX.GetValueOrDefault(default(double)) - dpiX) > double.Epsilon || Math.Abs(oldDpiY.GetValueOrDefault(default(double)) - dpiY) > double.Epsilon)
                {
                    DpiInformation.UpdateMonitorDpi(dpiX, dpiY);

                    // update layout scale
                    UpdateLayoutTransform();

                    // raise DpiChanged event
                    OnDpiChanged(EventArgs.Empty);
                }

                handled = true;
            }
            return IntPtr.Zero;
        }

        void UpdateLayoutTransform()
        {
            if (isPerMonitorDpiAware)
            {
                FrameworkElement root = (FrameworkElement)GetVisualChild(0);
                if (root != null)
                {
                    if (Math.Abs(DpiInformation.ScaleX - 1) > double.Epsilon || Math.Abs(DpiInformation.ScaleY - 1) > double.Epsilon)
                    {
                        root.LayoutTransform = new ScaleTransform(DpiInformation.ScaleX, DpiInformation.ScaleY);
                    }
                    else
                    {
                        root.LayoutTransform = null;
                    }
                }
            }
        }

        void UpdateWindowSize(double width, double height)
        {
            // determine relative scalex and scaley
            double relScaleX = width / Width;
            double relScaleY = height / Height;

            if (Math.Abs(relScaleX - 1) > double.Epsilon || Math.Abs(relScaleY - 1) > double.Epsilon)
            {
                // adjust window size constraints as well
                MinWidth *= relScaleX;
                MaxWidth *= relScaleX;
                MinHeight *= relScaleY;
                MaxHeight *= relScaleY;

                Width = width;
                Height = height;
            }
        }

        /// <summary>
        ///     Refreshes the current monitor DPI settings and update the window size and layout scale accordingly.
        /// </summary>
        protected void RefreshMonitorDpi()
        {
            if (!isPerMonitorDpiAware)
            {
                return;
            }

            // get the current DPI of the monitor of the window
            IntPtr monitor = NativeMethods.MonitorFromWindow(source.Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);

            uint xDpi = 96;
            uint yDpi = 96;
            if (NativeMethods.GetDpiForMonitor(monitor, (int) MonitorDpiType.EffectiveDpi, ref xDpi, ref yDpi) !=
                NativeMethods.S_OK)
            {
                xDpi = 96;
                yDpi = 96;
            }
            // vector contains the change of the old to new DPI
            Vector dpiVector = DpiInformation.UpdateMonitorDpi(xDpi, yDpi);

            // update Width and Height based on the current DPI of the monitor
            UpdateWindowSize(Width * dpiVector.X, Height * dpiVector.Y);

            // update graphics and text based on the current DPI of the monitor
            UpdateLayoutTransform();
        }

        /// <summary>
        ///     Raises the <see cref="E:DpiChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnDpiChanged(EventArgs e)
        {
            DpiChanged?.Invoke(this, e);
        }
    }
}