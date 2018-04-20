using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModernUI.Windows.Controls;

namespace ModernUI.App.Content
{
    /// <summary>
    ///     Interaction logic for ContentLoaderImages.xaml
    /// </summary>
    public partial class ContentLoaderImages : UserControl
    {
        public ContentLoaderImages()
        {
            InitializeComponent();

            LoadImageLinks();
        }

        async void LoadImageLinks()
        {
            FlickrImageLoader loader = (FlickrImageLoader)Tab.ContentLoader;

            try
            {
                // load image links and assign to tab list
                Tab.Links = await loader.GetInterestingnessListAsync();

                // select first link
                Tab.SelectedSource = Tab.Links.Select(l => l.Source).FirstOrDefault();
            }
            catch (Exception e)
            {
                ModernDialog.ShowMessage(e.Message, "Failure", MessageBoxButton.OK);
            }
        }
    }
}