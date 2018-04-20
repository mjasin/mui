using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ModernUI.Presentation;
using ModernUI.Windows;

namespace ModernUI.App
{
    /// <summary>
    ///     Loads image content from Flickr.
    /// </summary>
    public class FlickrImageLoader
        : IContentLoader
    {
        const string apiKey = null; // your flickr API key here

        /// <summary>
        ///     Asynchronously loads content from specified uri.
        /// </summary>
        /// <param name="uri">The content uri.</param>
        /// <param name="cancellationToken">The token used to cancel the load content operation.</param>
        /// <returns>The loaded content.</returns>
        public async Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken)
        {
            // assuming uri is a valid image uri
            HttpClient client = new HttpClient();
            HttpResponseMessage result = await client.GetAsync(uri, cancellationToken);

            // raise exception is result is not ok
            result.EnsureSuccessStatusCode();

            using (Stream stream = await result.Content.ReadAsStreamAsync())
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();

                return new Image {Source = bitmap};
            }
            ;
        }

        /// <summary>
        ///     Gets a collection of image links from the Flickr interestingness list.
        /// </summary>
        /// <returns></returns>
        public async Task<LinkCollection> GetInterestingnessListAsync()
        {
            if (apiKey == null)
            {
                throw new InvalidOperationException(
                    "You need to specify a Flickr API key. Unfortunately the key cannot be distributed with the source code. Get your own from [url=http://www.flickr.com/services/api/misc.api_keys.html]http://www.flickr.com/services/api/misc.api_keys.html[/url].");
            }

            const int count = 50; // limit the number of images to 50
            string listUri = string.Format(CultureInfo.InvariantCulture,
                "https://api.flickr.com/services/rest/?method=flickr.interestingness.getList&api_key={0}&per_page={1}",
                apiKey, count);
            HttpClient client = new HttpClient();
            HttpResponseMessage result = await client.GetAsync(listUri);
            result.EnsureSuccessStatusCode();
            using (Stream stream = await result.Content.ReadAsStreamAsync())
            {
                XDocument doc = XDocument.Load(stream);

                return new LinkCollection(from p in doc.Descendants("photo")
                    let title = (string) p.Attribute("title")
                    orderby title
                    select new Link
                    {
                        DisplayName = string.IsNullOrWhiteSpace(title) ? "[untitled]" : title,
                        Source = new Uri(
                            string.Format(CultureInfo.InvariantCulture,
                                "http://farm{0}.static.flickr.com/{1}/{2}_{3}.jpg", (string) p.Attribute("farm"),
                                (string) p.Attribute("server"), (string) p.Attribute("id"),
                                (string) p.Attribute("secret")), UriKind.Absolute)
                    });
            }
        }
    }
}