using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ModernUI.Windows.Navigation;

namespace ModernUI.Presentation
{
    /// <summary>
    ///     Manages the theme, font size and accent colors for a Modern UI application.
    /// </summary>
    public class AppearanceManager
        : NotifyPropertyChanged
    {
        /// <summary>
        ///     The resource key for the accent color.
        /// </summary>
        public const string KeyAccentColor = "AccentColor";

        /// <summary>
        ///     The resource key for the accent brush.
        /// </summary>
        public const string KeyAccent = "Accent";

        /// <summary>
        ///     The resource key for the default font size.
        /// </summary>
        public const string KeyDefaultFontSize = "DefaultFontSize";

        /// <summary>
        ///     The resource key for the fixed font size.
        /// </summary>
        public const string KeyFixedFontSize = "FixedFontSize";

        /// <summary>
        ///     The location of the dark theme resource dictionary.
        /// </summary>
        public static readonly Uri DarkThemeSource =
#pragma warning disable S1075 // URIs should not be hardcoded
            new Uri("/ModernUI;component/Assets/ModernUI.Dark.xaml", UriKind.Relative);
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        ///     The location of the light theme resource dictionary.
        /// </summary>
        public static readonly Uri LightThemeSource =
#pragma warning disable S1075 // URIs should not be hardcoded
            new Uri("/ModernUI;component/Assets/ModernUI.Light.xaml", UriKind.Relative);
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppearanceManager" /> class.
        /// </summary>
        AppearanceManager()
        {
            DarkThemeCommand = new RelayCommand(o => ThemeSource = DarkThemeSource,
                o => !DarkThemeSource.Equals(ThemeSource));
            LightThemeCommand = new RelayCommand(o => ThemeSource = LightThemeSource,
                o => !LightThemeSource.Equals(ThemeSource));
            SetThemeCommand = new RelayCommand(o =>
            {
                Uri uri = NavigationHelper.ToUri(o);
                if (uri != null)
                {
                    ThemeSource = uri;
                }
            }, o => o is Uri || o is string);
            LargeFontSizeCommand = new RelayCommand(o => FontSize = FontSize.Large);
            SmallFontSizeCommand = new RelayCommand(o => FontSize = FontSize.Small);
            AccentColorCommand = new RelayCommand(o =>
            {
                if (o is Color)
                {
                    AccentColor = (Color)o;
                }
                else
                {
                    // parse color from string
                    string str = o as string;
                    if (str != null)
                    {
                        AccentColor = (Color)ColorConverter.ConvertFromString(str);
                    }
                }
            }, o => o is Color || o is string);
        }

        /// <summary>
        ///     Gets the current <see cref="AppearanceManager" /> instance.
        /// </summary>
        public static AppearanceManager Current { get; } = new AppearanceManager();

        /// <summary>
        ///     The command that sets the dark theme.
        /// </summary>
        public ICommand DarkThemeCommand { get; }

        /// <summary>
        ///     The command that sets the light color theme.
        /// </summary>
        public ICommand LightThemeCommand { get; }

        /// <summary>
        ///     The command that sets a custom theme.
        /// </summary>
        public ICommand SetThemeCommand { get; }

        /// <summary>
        ///     The command that sets the large font size.
        /// </summary>
        public ICommand LargeFontSizeCommand { get; }

        /// <summary>
        ///     The command that sets the small font size.
        /// </summary>
        public ICommand SmallFontSizeCommand { get; }

        /// <summary>
        ///     The command that sets the accent color.
        /// </summary>
        public ICommand AccentColorCommand { get; }

        /// <summary>
        ///     Gets or sets the current theme source.
        /// </summary>
        public Uri ThemeSource
        {
            get => GetThemeSource();
            set => SetThemeSource(value, true);
        }

        /// <summary>
        ///     Gets or sets the font size.
        /// </summary>
        public FontSize FontSize
        {
            get => GetFontSize();
            set => SetFontSize(value);
        }

        /// <summary>
        ///     Gets or sets the accent color.
        /// </summary>
        public Color AccentColor
        {
            get => GetAccentColor();
            set => SetAccentColor(value);
        }

        ResourceDictionary GetThemeDictionary()
        {
            // determine the current theme by looking at the app resources and return the first dictionary having the resource key 'WindowBackground' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("WindowBackground")
                    select dict).FirstOrDefault();
        }

        Uri GetThemeSource()
        {
            ResourceDictionary dict = GetThemeDictionary();
            if (dict != null)
            {
                return dict.Source;
            }

            // could not determine the theme dictionary
            return null;
        }

        void SetThemeSource(Uri source, bool useThemeAccentColor)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ResourceDictionary oldThemeDict = GetThemeDictionary();
            Collection<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries;
            ResourceDictionary themeDict = new ResourceDictionary { Source = source };

            // if theme defines an accent color, use it
            Color? accentColor = themeDict[KeyAccentColor] as Color?;
            if (accentColor.HasValue)
            {
                // remove from the theme dictionary and apply globally if useThemeAccentColor is true
                themeDict.Remove(KeyAccentColor);

                if (useThemeAccentColor)
                {
                    ApplyAccentColor(accentColor.Value);
                }
            }

            // add new before removing old theme to avoid dynamicresource not found warnings
            dictionaries.Add(themeDict);

            // remove old theme
            if (oldThemeDict != null)
            {
                dictionaries.Remove(oldThemeDict);
            }

            OnPropertyChanged("ThemeSource");
        }

        void ApplyAccentColor(Color accentColor)
        {
            // set accent color and brush resources
            Application.Current.Resources[KeyAccentColor] = accentColor;
            Application.Current.Resources[KeyAccent] = new SolidColorBrush(accentColor);
        }

        FontSize GetFontSize()
        {
            double? defaultFontSize = Application.Current.Resources[KeyDefaultFontSize] as double?;

            if (defaultFontSize.HasValue)
            {
                return Math.Abs(defaultFontSize.Value - 12D) < double.Epsilon ? FontSize.Small : FontSize.Large;
            }

            // default large
            return FontSize.Large;
        }

        void SetFontSize(FontSize fontSize)
        {
            if (GetFontSize() == fontSize)
            {
                return;
            }

            Application.Current.Resources[KeyDefaultFontSize] = fontSize == FontSize.Small ? 12D : 13D;
            Application.Current.Resources[KeyFixedFontSize] = fontSize == FontSize.Small ? 10.667D : 13.333D;

            OnPropertyChanged("FontSize");
        }

        Color GetAccentColor()
        {
            Color? accentColor = Application.Current.Resources[KeyAccentColor] as Color?;

            if (accentColor.HasValue)
            {
                return accentColor.Value;
            }

            // default color: teal
            return Color.FromArgb(0xff, 0x1b, 0xa1, 0xe2);
        }

        void SetAccentColor(Color value)
        {
            ApplyAccentColor(value);

            // re-apply theme to ensure brushes referencing AccentColor are updated
            Uri themeSource = GetThemeSource();
            if (themeSource != null)
            {
                SetThemeSource(themeSource, false);
            }

            OnPropertyChanged("AccentColor");
        }
    }
}