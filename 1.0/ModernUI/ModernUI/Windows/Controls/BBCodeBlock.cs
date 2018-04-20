using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using ModernUI.Windows.Controls.BBCode;
using ModernUI.Windows.Navigation;

namespace ModernUI.Windows.Controls
{
    /// <summary>
    ///     A lighweight control for displaying small amounts of rich formatted BBCode content.
    /// </summary>
    [ContentProperty("BBCode")]
    public class BBCodeBlock
        : TextBlock
    {
        /// <summary>
        ///     Identifies the BBCode dependency property.
        /// </summary>
        public static readonly DependencyProperty BBCodeProperty = DependencyProperty.Register("BBCode", typeof(string),
            typeof(BBCodeBlock), new PropertyMetadata(OnBBCodeChanged));

        /// <summary>
        ///     Identifies the LinkNavigator dependency property.
        /// </summary>
        public static readonly DependencyProperty LinkNavigatorProperty = DependencyProperty.Register("LinkNavigator",
            typeof(ILinkNavigator), typeof(BBCodeBlock),
            new PropertyMetadata(new DefaultLinkNavigator(), OnLinkNavigatorChanged));

        /// <summary>
        ///     Identifies the BBCodeQuote dependency property.
        /// </summary>
        public static readonly DependencyProperty BBCodeQuoteBackgroundProperty =
            DependencyProperty.Register("BBCodeQuoteBackground", typeof(Brush), typeof(BBCodeBlock),
                new PropertyMetadata(OnBBCodeQuoteBackgroundChanged));

        bool dirty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BBCodeBlock" /> class.
        /// </summary>
        public BBCodeBlock()
        {
            // ensures the implicit BBCodeBlock style is used
            DefaultStyleKey = typeof(BBCodeBlock);

            AddHandler(FrameworkContentElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
            AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnRequestNavigate));
        }

        /// <summary>
        ///     Gets or sets the BB code.
        /// </summary>
        /// <value>The BB code.</value>
        public string BBCode
        {
            get => (string) GetValue(BBCodeProperty);
            set => SetValue(BBCodeProperty, value);
        }

        /// <summary>
        ///     Gets or sets the link navigator.
        /// </summary>
        /// <value>The link navigator.</value>
        public ILinkNavigator LinkNavigator
        {
            get => (ILinkNavigator) GetValue(LinkNavigatorProperty);
            set => SetValue(LinkNavigatorProperty, value);
        }

        /// <summary>
        ///     Gets or sets the BB quote background.
        /// </summary>
        /// <value>The BB code.</value>
        public Brush BBCodeQuoteBackground
        {
            get => (Brush) GetValue(BBCodeQuoteBackgroundProperty);
            set => SetValue(BBCodeQuoteBackgroundProperty, value);
        }

        static void OnBBCodeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((BBCodeBlock)o).UpdateDirty();
        }

        static void OnBBCodeQuoteBackgroundChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((BBCodeBlock)o).UpdateDirty();
        }

        static void OnLinkNavigatorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                // null values disallowed
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
#pragma warning disable RECS0143 // Cannot resolve symbol in text argument
                throw new ArgumentNullException("LinkNavigator");
#pragma warning restore RECS0143 // Cannot resolve symbol in text argument
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            ((BBCodeBlock)o).UpdateDirty();
        }

        void OnLoaded(object o, EventArgs e)
        {
            Update();
        }

        void UpdateDirty()
        {
            dirty = true;
            Update();
        }

        void Update()
        {
            if (!IsLoaded || !dirty)
            {
                return;
            }

            string bbcode = BBCode;

            Inlines.Clear();

            if (!string.IsNullOrWhiteSpace(bbcode))
            {
                Inline inline;
                try
                {
                    BBCodeParser parser = new BBCodeParser(bbcode, this, BBCodeQuoteBackground)
                    {
                        Commands = LinkNavigator.Commands
                    };
                    inline = parser.Parse();
                }
                catch (Exception)
                {
                    // parsing failed, display BBCode value as-is
                    inline = new Run { Text = bbcode };
                }
                Inlines.Add(inline);
            }
            dirty = false;
        }

        void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                // perform navigation using the link navigator
                LinkNavigator.Navigate(e.Uri, this, e.Target);
            }
            catch (Exception error)
            {
                // display navigation failures
                ModernDialog.ShowMessage(error.Message, ModernUI.Resources.NavigationFailed, MessageBoxButton.OK);
            }
        }
    }
}