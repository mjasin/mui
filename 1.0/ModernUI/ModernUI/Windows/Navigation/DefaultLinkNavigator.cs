using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ModernUI.Presentation;
using ModernUI.Windows.Controls;

namespace ModernUI.Windows.Navigation
{
    /// <summary>
    ///     The default link navigator with support for loading frame content, external link navigation using the default
    ///     browser and command execution.
    /// </summary>
    public class DefaultLinkNavigator
        : ILinkNavigator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultLinkNavigator" /> class.
        /// </summary>
        public DefaultLinkNavigator()
        {
            // register all ApperanceManager commands
#pragma warning disable S1075 // URIs should not be hardcoded
            Commands.Add(new Uri("cmd://accentcolor"), AppearanceManager.Current.AccentColorCommand);
            Commands.Add(new Uri("cmd://darktheme"), AppearanceManager.Current.DarkThemeCommand);
            Commands.Add(new Uri("cmd://largefontsize"), AppearanceManager.Current.LargeFontSizeCommand);
            Commands.Add(new Uri("cmd://lighttheme"), AppearanceManager.Current.LightThemeCommand);
            Commands.Add(new Uri("cmd://settheme"), AppearanceManager.Current.SetThemeCommand);
            Commands.Add(new Uri("cmd://smallfontsize"), AppearanceManager.Current.SmallFontSizeCommand);

            // register navigation commands
            Commands.Add(new Uri("cmd://browseback"), NavigationCommands.BrowseBack);
            Commands.Add(new Uri("cmd://refresh"), NavigationCommands.Refresh);

            // register application commands
            Commands.Add(new Uri("cmd://copy"), ApplicationCommands.Copy);
#pragma warning restore S1075 // URIs should not be hardcoded
        }

        /// <summary>
        ///     Gets or sets the schemes for external link navigation.
        /// </summary>
        /// <remarks>
        ///     Default schemes are http, https and mailto.
        /// </remarks>
        public string[] ExternalSchemes { get; set; } = {Uri.UriSchemeHttp, Uri.UriSchemeHttps, Uri.UriSchemeMailto};

        /// <summary>
        ///     Gets or sets the navigable commands.
        /// </summary>
        public CommandDictionary Commands { get; set; } = new CommandDictionary();

        /// <summary>
        ///     Performs navigation to specified link.
        /// </summary>
        /// <param name="uri">The uri to navigate to.</param>
        /// <param name="source">The source element that triggers the navigation. Required for frame navigation.</param>
        /// <param name="parameter">An optional command parameter or navigation target.</param>
        public virtual void Navigate(Uri uri, FrameworkElement source, string parameter = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            // first check if uri refers to a command
            ICommand command;
            if (Commands != null && Commands.TryGetValue(uri, out command))
            {
                // note: not executed within BBCodeBlock context, Hyperlink instance has Command and CommandParameter set
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
            else if (uri.IsAbsoluteUri && ExternalSchemes != null &&
                     ExternalSchemes.Any(s => uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase)))
            {
                // uri is external, load in default browser
                Process.Start(uri.AbsoluteUri);
            }
            else
            {
                // perform frame navigation
                if (source == null)
                {
                    // source required
                    throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
                        Resources.NavigationFailedSourceNotSpecified, uri));
                }

                // use optional parameter as navigation target to identify target frame (_self, _parent, _top or named target frame)
                ModernFrame frame = NavigationHelper.FindFrame(parameter, source);
                if (frame == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
                        Resources.NavigationFailedFrameNotFound, uri, parameter));
                }

                // delegate navigation to the frame
                frame.Source = uri;
            }
        }
    }
}