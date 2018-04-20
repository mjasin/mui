using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernUI.Presentation;
using ModernUI.Windows.Navigation;

namespace ModernUI.Windows.Controls
{
    /// <summary>
    ///     Represents a Modern UI styled window.
    /// </summary>
    public class ModernWindow
        : DpiAwareWindow
    {
        /// <summary>
        ///     Identifies the BackgroundContent dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundContentProperty =
            DependencyProperty.Register("BackgroundContent", typeof(object), typeof(ModernWindow));

        /// <summary>
        ///     Identifies the MenuLinkGroups dependency property.
        /// </summary>
        public static readonly DependencyProperty MenuLinkGroupsProperty =
            DependencyProperty.Register("MenuLinkGroups", typeof(LinkGroupCollection), typeof(ModernWindow));

        /// <summary>
        ///     Identifies the TitleLinks dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleLinksProperty =
            DependencyProperty.Register("TitleLinks", typeof(LinkCollection), typeof(ModernWindow));

        /// <summary>
        ///     Identifies the IsTitleVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTitleVisibleProperty =
            DependencyProperty.Register("IsTitleVisible", typeof(bool), typeof(ModernWindow),
                new PropertyMetadata(false));

        /// <summary>
        ///     Identifies the LogoData dependency property.
        /// </summary>
        public static readonly DependencyProperty LogoDataProperty =
            DependencyProperty.Register("LogoData", typeof(Geometry), typeof(ModernWindow));

        /// <summary>
        ///     Identifies the LogoCommand dependency property.
        /// </summary>
        public static readonly DependencyProperty LogoCommandProperty =
            DependencyProperty.Register("LogoCommand", typeof(ICommand), typeof(ModernWindow));

        /// <summary>
        ///     Identifies the LogoCommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty LogoCommandParameterProperty =
            DependencyProperty.Register("LogoCommandParameter", typeof(object), typeof(ModernWindow));

        /// <summary>
        ///     Defines the ContentSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentSourceProperty =
            DependencyProperty.Register("ContentSource", typeof(Uri), typeof(ModernWindow));

        /// <summary>
        ///     Identifies the ContentLoader dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentLoaderProperty =
            DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(ModernWindow),
                new PropertyMetadata(new DefaultContentLoader()));

        /// <summary>
        ///     Identifies the LinkNavigator dependency property.
        /// </summary>
        public static readonly DependencyProperty LinkNavigatorProperty = DependencyProperty.Register("LinkNavigator",
            typeof(ILinkNavigator), typeof(ModernWindow), new PropertyMetadata(new DefaultLinkNavigator()));

        /// <summary>
        ///     Identifies the BBCodeTitle dependency property.
        /// </summary>
        public static readonly DependencyProperty BBCodeTitleProperty =
            DependencyProperty.Register("BBCodeTitle", typeof(string), typeof(ModernWindow),
                new PropertyMetadata(null));
        /// <summary>
        /// Identifies the LogoDataToolTip dependency property
        /// </summary>
        public static readonly DependencyProperty LogoDataToolTipProperty = 
            DependencyProperty.Register("LogoDataToolTip", typeof(string), typeof(ModernWindow));

        Storyboard backgroundAnimation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModernWindow" /> class.
        /// </summary>
        public ModernWindow()
        {
            DefaultStyleKey = typeof(ModernWindow);

            // create empty collections
            SetCurrentValue(MenuLinkGroupsProperty, new LinkGroupCollection());
            SetCurrentValue(TitleLinksProperty, new LinkCollection());

            // associate window commands with this instance
#if NET4
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow,
                OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow,
                OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow,
                OnCanResizeWindow));
#else
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
#endif
            // associate navigate link command with this instance
            CommandBindings.Add(new CommandBinding(LinkCommands.NavigateLink, OnNavigateLink, OnCanNavigateLink));

            // listen for theme changes
            AppearanceManager.Current.PropertyChanged += OnAppearanceManagerPropertyChanged;
        }

        /// <summary>
        ///     Gets or sets the background content of this window instance.
        /// </summary>
        public object BackgroundContent
        {
            get => GetValue(BackgroundContentProperty);
            set => SetValue(BackgroundContentProperty, value);
        }

        /// <summary>
        ///     Gets or sets the collection of link groups shown in the window's menu.
        /// </summary>
        public LinkGroupCollection MenuLinkGroups
        {
            get => (LinkGroupCollection) GetValue(MenuLinkGroupsProperty);
            set => SetValue(MenuLinkGroupsProperty, value);
        }

        /// <summary>
        ///     Gets or sets the collection of links that appear in the menu in the title area of the window.
        /// </summary>
        public LinkCollection TitleLinks
        {
            get => (LinkCollection) GetValue(TitleLinksProperty);
            set => SetValue(TitleLinksProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the window title is visible in the UI.
        /// </summary>
        public bool IsTitleVisible
        {
            get => (bool) GetValue(IsTitleVisibleProperty);
            set => SetValue(IsTitleVisibleProperty, value);
        }

        /// <summary>
        ///     Gets or sets the path data for the logo displayed in the title area of the window.
        /// </summary>
        public Geometry LogoData
        {
            get => (Geometry) GetValue(LogoDataProperty);
            set => SetValue(LogoDataProperty, value);
        }

        /// <summary>
        /// Gets or sets the ToolTip text of LogoData.
        /// </summary>
        public string LogoDataToolTip
        {
            get { return (string)GetValue(LogoDataToolTipProperty); }
            set { SetValue(LogoDataToolTipProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the command for the logo.
        /// </summary>
        public ICommand LogoCommand
        {
            get => (ICommand) GetValue(LogoCommandProperty);
            set => SetValue(LogoCommandProperty, value);
        }

        /// <summary>
        ///     Gets or sets the command parameter for the logo.
        /// </summary>
        public object LogoCommandParameter
        {
            get => GetValue(LogoCommandParameterProperty);
            set => SetValue(LogoCommandParameterProperty, value);
        }

        /// <summary>
        ///     Gets or sets the source uri of the current content.
        /// </summary>
        public Uri ContentSource
        {
            get => (Uri) GetValue(ContentSourceProperty);
            set => SetValue(ContentSourceProperty, value);
        }

        /// <summary>
        ///     Gets or sets the content loader.
        /// </summary>
        public IContentLoader ContentLoader
        {
            get => (IContentLoader) GetValue(ContentLoaderProperty);
            set => SetValue(ContentLoaderProperty, value);
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
        ///     Gets or sets the BBCode Title.
        /// </summary>
        /// <value>The link navigator.</value>
        public string BBCodeTitle
        {
            get => (string) GetValue(BBCodeTitleProperty);
            set => SetValue(BBCodeTitleProperty, value);
        }

        /// <summary>
        ///     Gets the content frame.
        /// </summary>
        /// <value>The link navigator.</value>
        public ModernFrame ContentFrame => GetTemplateChild("ContentFrame") as ModernFrame;

        /// <summary>
        ///     Raises the System.Windows.Window.Closed event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // detach event handler
            AppearanceManager.Current.PropertyChanged -= OnAppearanceManagerPropertyChanged;
        }

        /// <summary>
        ///     When overridden in a derived class, is invoked whenever application code or internal processes call
        ///     System.Windows.FrameworkElement.ApplyTemplate().
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // retrieve BackgroundAnimation storyboard
            Border border = GetTemplateChild("WindowBorder") as Border;
            if (border != null)
            {
                backgroundAnimation = border.Resources["BackgroundAnimation"] as Storyboard;

                if (backgroundAnimation != null)
                {
                    backgroundAnimation.Begin();
                }
            }
        }

        void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // start background animation if theme has changed
            if (e.PropertyName == "ThemeSource" && backgroundAnimation != null)
            {
                backgroundAnimation.Begin();
            }
        }

        void OnCanNavigateLink(object sender, CanExecuteRoutedEventArgs e)
        {
            // true by default
            e.CanExecute = true;
            if (LinkNavigator != null && 
                LinkNavigator.Commands != null && 
                NavigationHelper.TryParseUriWithParameters(e.Parameter, out Uri uri, out string parameter, out string targetName) && 
                LinkNavigator.Commands.TryGetValue(uri, out ICommand command))
            {
                e.CanExecute = command.CanExecute(parameter);
            }
        }

        void OnNavigateLink(object sender, ExecutedRoutedEventArgs e)
        {
            if (LinkNavigator != null && NavigationHelper.TryParseUriWithParameters(e.Parameter, out Uri uri, out string parameter, out string targetName))
            {
                LinkNavigator.Navigate(uri, e.Source as FrameworkElement, parameter);
            }
        }

        void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
#if NET4
            SystemCommands.CloseWindow(this);
#else
            SystemCommands.CloseWindow(this);
#endif
        }

        void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
#if NET4
            SystemCommands.MaximizeWindow(this);
#else
            SystemCommands.MaximizeWindow(this);
#endif
        }

        void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
#if NET4
            SystemCommands.MinimizeWindow(this);
#else
            SystemCommands.MinimizeWindow(this);
#endif
        }

        void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
#if NET4
            SystemCommands.RestoreWindow(this);
#else
            SystemCommands.RestoreWindow(this);
#endif
        }
    }
}