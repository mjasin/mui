using System.Globalization;
using System.Windows.Controls;
using ModernUI.Windows.Navigation;

namespace ModernUI.App.Content
{
    /// <summary>
    ///     Interaction logic for ControlsModernFrame.xaml
    /// </summary>
    public partial class ControlsModernFrame : UserControl
    {
        string eventLogMessage;

        public ControlsModernFrame()
        {
            InitializeComponent();

            TextEvents.Text = eventLogMessage;
        }

        void LogMessage(string message, params object[] o)
        {
            message = string.Format(CultureInfo.CurrentUICulture, message, o);

            if (TextEvents == null)
            {
                eventLogMessage += message;
            }
            else
            {
                TextEvents.AppendText(message);
            }
        }

        void Frame_FragmentNavigation(object sender, FragmentNavigationEventArgs e)
        {
            LogMessage("FragmentNavigation: {0}\r\n", e.Fragment);
        }

        void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            LogMessage("Navigated: [{0}] {1}\r\n", e.NavigationType, e.Source);
        }

        void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            LogMessage("Navigating: [{0}] {1}\r\n", e.NavigationType, e.Source);
        }

        void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            LogMessage("NavigationFailed: {0}\r\n", e.Error.Message);
        }
    }
}