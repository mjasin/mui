using System.Windows;
using System.Windows.Controls;
using ModernUI.Windows.Controls;

namespace ModernUI.App.Content
{
    /// <summary>
    ///     Interaction logic for ControlsModernDialog.xaml
    /// </summary>
    public partial class ControlsModernDialog : UserControl
    {
        public ControlsModernDialog()
        {
            InitializeComponent();
        }

        void CommonDialog_Click(object sender, RoutedEventArgs e)
        {
            ModernDialog dlg = new ModernDialog
            {
                Title = "Common dialog",
                Content = new LoremIpsum()
            };
            dlg.Buttons = new[] { dlg.OkButton, dlg.CancelButton };
            dlg.ShowDialog();

            dialogResult.Text = dlg.DialogResult.HasValue ? dlg.DialogResult.ToString() : "<null>";
            dialogMessageBoxResult.Text = dlg.MessageBoxResult.ToString();
        }

        void MessageDialog_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton btn = MessageBoxButton.OK;
            if (true == ok.IsChecked)
            {
                btn = MessageBoxButton.OK;
            }
            else if (true == okcancel.IsChecked)
            {
                btn = MessageBoxButton.OKCancel;
            }
            else if (true == yesno.IsChecked)
            {
                btn = MessageBoxButton.YesNo;
            }
            else if (true == yesnocancel.IsChecked) btn = MessageBoxButton.YesNoCancel;

            MessageBoxResult result =
                ModernDialog.ShowMessage("This is a simple Modern UI styled message dialog. Do you like it?",
                    "Message Dialog", btn);

            msgboxResult.Text = result.ToString();
        }
    }
}