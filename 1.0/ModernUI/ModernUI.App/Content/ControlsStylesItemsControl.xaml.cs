using System.Windows;
using System.Windows.Controls;

namespace ModernUI.App.Content
{
    /// <summary>
    ///     Interaction logic for ControlsStylesItemsControl.xaml
    /// </summary>
    public partial class ControlsStylesItemsControl : UserControl
    {
        public ControlsStylesItemsControl()
        {
            InitializeComponent();
        }

        MenuItem CreateSubMenu(string header)
        {
            MenuItem item = new MenuItem { Header = header };
            item.Items.Add(new MenuItem { Header = "Item 1" });
            item.Items.Add("Item 2");
            item.Items.Add(new Separator());
            item.Items.Add("Item 3");
            return item;
        }

        void ShowContextMenu_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();

            contextMenu.Items.Add(new MenuItem { Header = "Item" });
            contextMenu.Items.Add(new MenuItem { Header = "Item with gesture", InputGestureText = "Ctrl+C" });
            contextMenu.Items.Add(new MenuItem { Header = "Item, disabled", IsEnabled = false });
            contextMenu.Items.Add(new MenuItem { Header = "Item, checked", IsChecked = true });
            contextMenu.Items.Add(new MenuItem
            {
                Header = "Item, checked and disabled",
                IsChecked = true,
                IsEnabled = false
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(CreateSubMenu("Item with submenu"));

            MenuItem menu = CreateSubMenu("Item with submenu, disabled");
            menu.IsEnabled = false;
            contextMenu.Items.Add(menu);

            contextMenu.IsOpen = true;
        }
    }
}