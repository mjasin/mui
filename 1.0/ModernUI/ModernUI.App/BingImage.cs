using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ModernUI.App
{
    /// <summary>
    ///     Provides an attached property determining the current Bing image and assigning it to an image or imagebrush.
    /// </summary>
    public static class BingImage
    {
        public static readonly DependencyProperty UseBingImageProperty =
            DependencyProperty.RegisterAttached("UseBingImage", typeof(bool), typeof(BingImage),
                new PropertyMetadata(OnUseBingImageChanged));

        static BitmapImage cachedBingImage;

        static async void OnUseBingImageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;
            Image image = o as Image;
            ImageBrush imageBrush = o as ImageBrush;

            if (!newValue || image == null && imageBrush == null)
            {
                return;
            }

            if (cachedBingImage == null)
            {
                Uri url = await GetCurrentBingImageUrl();
                if (url != null)
                {
                    cachedBingImage = new BitmapImage(url);
                }
            }

            if (cachedBingImage != null)
            {
                if (image != null)
                {
                    image.Source = cachedBingImage;
                }
                else if (imageBrush != null)
                {
                    imageBrush.ImageSource = cachedBingImage;
                }
            }
        }

        static async Task<Uri> GetCurrentBingImageUrl()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage result =
                await client.GetAsync("http://www.bing.com/hpimagearchive.aspx?format=xml&idx=0&n=2&mbl=1&mkt=en-ww");
            if (result.IsSuccessStatusCode)
            {
                using (Stream stream = await result.Content.ReadAsStreamAsync())
                {
                    XDocument doc = XDocument.Load(stream);

                    string url = (string)doc.XPathSelectElement("/images/image/url");

                    return new Uri(string.Format(CultureInfo.InvariantCulture, "http://bing.com{0}", url),
                        UriKind.Absolute);
                }
            }

            return null;
        }


        public static bool GetUseBingImage(DependencyObject o)
        {
            return (bool) o.GetValue(UseBingImageProperty);
        }

        public static void SetUseBingImage(DependencyObject o, bool value)
        {
            o.SetValue(UseBingImageProperty, value);
        }
    }
}