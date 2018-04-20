using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using ModernUI.Presentation;

namespace ModernUI.App.Content
{
    /// <summary>
    ///     Interaction logic for ControlsModernMenu.xaml
    /// </summary>
    public partial class ControlsModernMenu : UserControl
    {
        int groupId = 2;
        int linkId = 5;

        public ControlsModernMenu()
        {
            InitializeComponent();

            // add group command
            AddGroup.Command = new RelayCommand(o =>
            {
                Menu.LinkGroups.Add(new LinkGroup
                {
                    DisplayName = string.Format(CultureInfo.InvariantCulture, "group {0}",
                        ++groupId)
                });
            });

            // add link to selected group command
            AddLink.Command = new RelayCommand(o =>
            {
                Menu.SelectedLinkGroup.Links.Add(new Link
                {
                    DisplayName = string.Format(CultureInfo.InvariantCulture, "link {0}", ++linkId),
                    Source = new Uri(string.Format(CultureInfo.InvariantCulture, "/link{0}", linkId), UriKind.Relative)
                });
            }, o => Menu.SelectedLinkGroup != null);

            // remove selected group command
            RemoveGroup.Command = new RelayCommand(o => { Menu.LinkGroups.Remove(Menu.SelectedLinkGroup); },
                o => Menu.SelectedLinkGroup != null);

            // remove selected linkcommand
            RemoveLink.Command = new RelayCommand(o => { Menu.SelectedLinkGroup.Links.Remove(Menu.SelectedLink); },
                o => Menu.SelectedLinkGroup != null && Menu.SelectedLink != null);

            // log SourceChanged events
            Menu.SelectedSourceChanged += (o, e) => { Debug.WriteLine("SelectedSourceChanged: {0}", e.Source); };
        }
    }
}