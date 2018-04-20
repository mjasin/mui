using System;
using System.Globalization;
using System.Windows;
using ModernUI.Presentation;
using ModernUI.Windows.Controls;

namespace ModernUI.App.Pages
{
    public class DpiAwarenessViewModel
        : NotifyPropertyChanged
    {
        readonly DpiAwareWindow wnd;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DpiAwarenessViewModel" /> class.
        /// </summary>
        public DpiAwarenessViewModel()
        {
            wnd = (DpiAwareWindow) Application.Current.MainWindow;
            wnd.DpiChanged += OnWndDpiChanged;
            wnd.SizeChanged += OnWndSizeChanged;
        }

        public string DpiAwareMessage => string.Format(CultureInfo.InvariantCulture,
            "The DPI awareness of this process is [b]{0}[/b]", ModernUIHelper.GetDpiAwareness());

        public string WpfDpi
        {
            get
            {
                DpiInformation info = wnd.DpiInformation;
                return string.Format(CultureInfo.InvariantCulture, "{0} x {1}", info.WpfDpiX, info.WpfDpiY);
            }
        }

        public string MonitorDpi
        {
            get
            {
                DpiInformation info = wnd.DpiInformation;
                if (info.MonitorDpiX.HasValue)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0} x {1}", info.MonitorDpiX, info.MonitorDpiY);
                }
                return "n/a";
            }
        }

        public string LayoutScale
        {
            get
            {
                DpiInformation info = wnd.DpiInformation;
                return string.Format(CultureInfo.InvariantCulture, "{0} x {1}", info.ScaleX, info.ScaleY);
            }
        }

        public string WindowSize
        {
            get
            {
                DpiInformation info = wnd.DpiInformation;
                double width = wnd.ActualWidth * info.WpfDpiX / 96D;
                double height = wnd.ActualHeight * info.WpfDpiY / 96D;

                return string.Format(CultureInfo.InvariantCulture, "{0} x {1}", width, height);
            }
        }

        void OnWndDpiChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(null); // refresh all properties
        }

        void OnWndSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPropertyChanged(null); // refresh all properties
        }
    }
}