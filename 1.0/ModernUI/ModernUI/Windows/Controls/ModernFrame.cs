﻿using ModernUI.Windows.Media;
using ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernUI.Windows.Controls
{
    /// <summary>
    ///     A simple content frame implementation with navigation support.
    /// </summary>
    public class ModernFrame
        : ContentControl
    {
        /// <summary>
        ///     Identifies the KeepAlive attached dependency property.
        /// </summary>
        public static readonly DependencyProperty KeepAliveProperty =
            DependencyProperty.RegisterAttached("KeepAlive", typeof(bool?), typeof(ModernFrame),
                new PropertyMetadata(null));

        /// <summary>
        ///     Identifies the KeepContentAlive dependency property.
        /// </summary>
        public static readonly DependencyProperty KeepContentAliveProperty =
            DependencyProperty.Register("KeepContentAlive", typeof(bool), typeof(ModernFrame),
                new PropertyMetadata(true, OnKeepContentAliveChanged));

        /// <summary>
        ///     Identifies the ContentLoader dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentLoaderProperty =
            DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(ModernFrame),
                new PropertyMetadata(new DefaultContentLoader(), OnContentLoaderChanged));

        /// <summary>
        /// Identifies the IsLoadingContent dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLoadingContentProperty =
            DependencyProperty.Register("IsLoadingContent", typeof(bool), typeof(ModernFrame),
                new PropertyMetadata(false));


        /// <summary>
        ///     Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(ModernFrame),
                new PropertyMetadata(OnSourceChanged));

        /// <summary>
        ///     Occurs when navigation to a content fragment begins.
        /// </summary>
        public event EventHandler<FragmentNavigationEventArgs> FragmentNavigation;

        /// <summary>
        ///     Occurs when a new navigation is requested.
        /// </summary>
        /// <remarks>
        ///     The navigating event is also raised when a parent frame is navigating. This allows for cancelling parent
        ///     navigation.
        /// </remarks>
        public event EventHandler<NavigatingCancelEventArgs> Navigating;

        /// <summary>
        ///     Occurs when navigation to new content has completed.
        /// </summary>
        public event EventHandler<NavigationEventArgs> Navigated;

        /// <summary>
        ///     Occurs when navigation has failed.
        /// </summary>
        public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

        readonly Stack<Uri> history = new Stack<Uri>();
        readonly Dictionary<Uri, object> contentCache = new Dictionary<Uri, object>();
#if NET4
        private readonly List<WeakReference> childFrames = new List<WeakReference>()
            ; // list of registered frames in sub tree
#else
        readonly List<WeakReference<ModernFrame>> childFrames =
new List<WeakReference<ModernFrame>>();        // list of registered frames in sub tree
#endif
        CancellationTokenSource tokenSource;
        bool isNavigatingHistory;
        bool isResetSource, _isLoadingContent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModernFrame" /> class.
        /// </summary>
        public ModernFrame()
        {
            DefaultStyleKey = typeof(ModernFrame);

            // associate application and navigation commands with this instance
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowseBack, OnCanBrowseBack));
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, OnGoToPage, OnCanGoToPage));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, OnRefresh, OnCanRefresh));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopy, OnCanCopy));

            Loaded += OnLoaded;
        }

        static void OnKeepContentAliveChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernFrame)o).OnKeepContentAliveChanged((bool)e.NewValue);
        }

        void OnKeepContentAliveChanged(bool keepAlive)
        {
            // clear content cache
            if (!keepAlive)
                contentCache.Clear();
        }

        static void OnContentLoaderChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                // null values for content loader not allowed
#pragma warning disable RECS0143 // Cannot resolve symbol in text argument
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException("ContentLoader");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
#pragma warning restore RECS0143 // Cannot resolve symbol in text argument
            }
        }

        static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernFrame)o).OnSourceChanged((Uri)e.OldValue, (Uri)e.NewValue);
        }

        void OnSourceChanged(Uri oldValue, Uri newValue)
        {
            // if resetting source or old source equals new, don't do anything
            if (isResetSource || newValue != null && newValue.Equals(oldValue))
            {
                return;
            }

            // handle fragment navigation
            string newFragment = null;
            Uri oldValueNoFragment = NavigationHelper.RemoveFragment(oldValue);
            Uri newValueNoFragment = NavigationHelper.RemoveFragment(newValue, out newFragment);

            if (newValueNoFragment != null && newValueNoFragment.Equals(oldValueNoFragment))
            {
                // fragment navigation
                FragmentNavigationEventArgs args = new FragmentNavigationEventArgs
                {
                    Fragment = newFragment
                };

                OnFragmentNavigation(Content as IContent, args);
            }
            else
            {
                NavigationType navType = isNavigatingHistory ? NavigationType.Back : NavigationType.New;

                // only invoke CanNavigate for new navigation
                if (!isNavigatingHistory && !CanNavigate(oldValue, newValue, navType))
                {
                    return;
                }

                Navigate(oldValue, newValue, navType);
            }
        }

        bool CanNavigate(Uri oldValue, Uri newValue, NavigationType navigationType)
        {
            NavigatingCancelEventArgs cancelArgs = new NavigatingCancelEventArgs
            {
                Frame = this,
                Source = newValue,
                IsParentFrameNavigating = true,
                NavigationType = navigationType,
                Cancel = false
            };
            OnNavigating(Content as IContent, cancelArgs);

            // check if navigation cancelled
            if (cancelArgs.Cancel)
            {
                Debug.WriteLine("Cancelled navigation from '{0}' to '{1}'", oldValue, newValue);

                if (Source != oldValue)
                {
                    // enqueue the operation to reset the source back to the old value
                    Dispatcher.BeginInvoke((Action)(() =>
                   {
                       isResetSource = true;
                       SetCurrentValue(SourceProperty, oldValue);
                       isResetSource = false;
                   }));
                }
                return false;
            }

            return true;
        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        void Navigate(Uri oldValue, Uri newValue, NavigationType navigationType)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            Debug.WriteLine("Navigating from '{0}' to '{1}'", oldValue, newValue);

            // set IsLoadingContent state
            SetValue(IsLoadingContentProperty, true);

            // cancel previous load content task (if any)
            // note: no need for thread synchronization, this code always executes on the UI thread
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource = null;
            }

            // push previous source onto the history stack (only for new navigation types)
            if (oldValue != null && navigationType == NavigationType.New)
            {
                history.Push(oldValue);
            }

            object newContent = null;

            if (newValue != null)
            {
                // content is cached on uri without fragment
                Uri newValueNoFragment = NavigationHelper.RemoveFragment(newValue);

                if (navigationType == NavigationType.Refresh ||
                    !contentCache.TryGetValue(newValueNoFragment, out newContent))
                {
                    CancellationTokenSource localTokenSource = new CancellationTokenSource();
                    tokenSource = localTokenSource;
                    // load the content (asynchronous!)
                    TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    Task<object> task = ContentLoader.LoadContentAsync(newValue, tokenSource.Token);

                    task.ContinueWith(t =>
                    {
                        try
                        {
                            if (t.IsCanceled || localTokenSource.IsCancellationRequested)
                            {
                                Debug.WriteLine("Cancelled navigation to '{0}'", newValue);
                            }
                            else if (t.IsFaulted)
                            {
                                // raise failed event
                                NavigationFailedEventArgs failedArgs = new NavigationFailedEventArgs
                                {
                                    Frame = this,
                                    Source = newValue,
                                    Error = t.Exception.InnerException,
                                    Handled = false
                                };

                                OnNavigationFailed(failedArgs);

                                // if not handled, show error as content
                                newContent = failedArgs.Handled ? null : failedArgs.Error;

                                SetContent(newValue, navigationType, newContent, true);
                            }
                            else
                            {
                                newContent = t.Result;
                                if (ShouldKeepContentAlive(newContent))
                                {
                                    // keep the new content in memory
                                    contentCache[newValueNoFragment] = newContent;
                                }

                                SetContent(newValue, navigationType, newContent, false);
                            }
                        }
                        finally
                        {
                            // clear global tokenSource to avoid a Cancel on a disposed object
                            if (tokenSource == localTokenSource)
                            {
                                tokenSource = null;
                            }

                            // and dispose of the local tokensource
                            localTokenSource.Dispose();
                        }
                    }, scheduler);
                    return;
                }
            }

            // newValue is null or newContent was found in the cache
            SetContent(newValue, navigationType, newContent, false);
        }

        void SetContent(Uri newSource, NavigationType navigationType, object newContent, bool contentIsError)
        {
            IContent oldContent = Content as IContent;

            // assign content
            Content = newContent;

            // do not raise navigated event when error
            if (!contentIsError)
            {
                NavigationEventArgs args = new NavigationEventArgs
                {
                    Frame = this,
                    Source = newSource,
                    Content = newContent,
                    NavigationType = navigationType
                };

                OnNavigated(oldContent, newContent as IContent, args);
            }

            // set IsLoadingContent to false
            SetValue(IsLoadingContentProperty, _isLoadingContent);

            if (!contentIsError)
            {
                // and raise optional fragment navigation events
                string fragment;
                NavigationHelper.RemoveFragment(newSource, out fragment);
                if (fragment != null)
                {
                    // fragment navigation
                    FragmentNavigationEventArgs fragmentArgs = new FragmentNavigationEventArgs
                    {
                        Fragment = fragment
                    };

                    OnFragmentNavigation(newContent as IContent, fragmentArgs);
                }
            }
        }


        IEnumerable<ModernFrame> GetChildFrames()
        {
            var refs = childFrames.ToArray();
            foreach (var r in refs)
            {
                bool valid = false;
                ModernFrame frame;
                if (r.TryGetTarget(out frame) && NavigationHelper.FindFrame(null, frame) == this)
                {
                    valid = true;
                    yield return frame;
                }

                if (frame != null && !valid)
                {
                    //raise NavigatedFrom Event
                    if (frame.Content is IContent content)
                    {
                        content.OnNavigatingFrom(new NavigatingCancelEventArgs());

                        var args = new NavigationEventArgs
                        {
                            Frame = this,
                            Source = Source,
                            Content = Content,
                            NavigationType = NavigationType.Back
                        };
                        content.OnNavigatedFrom(args);
                    }
                    childFrames.Remove(r);
                }
            }
        }

        void OnFragmentNavigation(IContent content, FragmentNavigationEventArgs e)
        {
            // invoke optional IContent.OnFragmentNavigation
            if (content != null)
            {
                content.OnFragmentNavigation(e);
            }

            // raise the FragmentNavigation event
            if (FragmentNavigation != null)
            {
                FragmentNavigation(this, e);
            }
        }

        void OnNavigating(IContent content, NavigatingCancelEventArgs e)
        {
            // first invoke child frame navigation events
            foreach (ModernFrame f in GetChildFrames())
            {
                f.OnNavigating(f.Content as IContent, e);
            }

            e.IsParentFrameNavigating = e.Frame != this;

            // invoke IContent.OnNavigating (only if content implements IContent)
            if (content != null)
            {
                content.OnNavigatingFrom(e);
            }

            // raise the Navigating event
            if (Navigating != null)
            {
                Navigating(this, e);
            }
        }

        void OnNavigated(IContent oldContent, IContent newContent, NavigationEventArgs e)
        {
            // invoke IContent.OnNavigatedFrom and OnNavigatedTo
            if (oldContent != null)
            {
                oldContent.OnNavigatedFrom(e);
            }
            if (newContent != null)
            {
                newContent.OnNavigatedTo(e);
            }

            // raise the Navigated event
            if (Navigated != null)
            {
                Navigated(this, e);
            }
        }

        void OnNavigationFailed(NavigationFailedEventArgs e)
        {
            if (NavigationFailed != null)
            {
                NavigationFailed(this, e);
            }
        }

        /// <summary>
        ///     Determines whether the routed event args should be handled.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>This method prevents parent frames from handling routed commands.</remarks>
        bool HandleRoutedEvent(CanExecuteRoutedEventArgs args)
        {
            DependencyObject originalSource = args.OriginalSource as DependencyObject;

            if (originalSource == null)
            {
                return false;
            }
            return originalSource.AncestorsAndSelf().OfType<ModernFrame>().FirstOrDefault() == this;
        }

        void OnCanBrowseBack(object sender, CanExecuteRoutedEventArgs e)
        {
            // only enable browse back for source frame, do not bubble
            if (HandleRoutedEvent(e))
            {
                e.CanExecute = history.Count > 0;
            }
        }

        void OnCanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HandleRoutedEvent(e))
            {
                e.CanExecute = Content != null;
            }
        }

        void OnCanGoToPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HandleRoutedEvent(e))
            {
                e.CanExecute = e.Parameter is string || e.Parameter is Uri;
            }
        }

        void OnCanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HandleRoutedEvent(e))
            {
                e.CanExecute = Source != null;
            }
        }

        void OnBrowseBack(object target, ExecutedRoutedEventArgs e)
        {
            if (history.Count > 0)
            {
                Uri oldValue = Source;
                Uri newValue = history.Peek(); // do not remove just yet, navigation may be cancelled

                if (CanNavigate(oldValue, newValue, NavigationType.Back))
                {
                    isNavigatingHistory = true;
                    SetCurrentValue(SourceProperty, history.Pop());
                    isNavigatingHistory = false;
                }
            }
        }

        void OnGoToPage(object target, ExecutedRoutedEventArgs e)
        {
            Uri newValue = NavigationHelper.ToUri(e.Parameter);
            SetCurrentValue(SourceProperty, newValue);
        }

        void OnRefresh(object target, ExecutedRoutedEventArgs e)
        {
            if (CanNavigate(Source, Source, NavigationType.Refresh))
            {
                Navigate(Source, Source, NavigationType.Refresh);
            }
        }

        void OnCopy(object target, ExecutedRoutedEventArgs e)
        {
            // copies the string representation of the current content to the clipboard
            Clipboard.SetText(Content.ToString());
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            ModernFrame parent = NavigationHelper.FindFrame(NavigationHelper.FrameParent, this);
            if (parent != null)
            {
                parent.RegisterChildFrame(this);
            }
        }

        void RegisterChildFrame(ModernFrame frame)
        {
            // do not register existing frame
            if (!GetChildFrames().Contains(frame))
            {
#if NET4
                WeakReference r = new WeakReference(frame);
#else
                var r = new WeakReference<ModernFrame>(frame);
#endif
                childFrames.Add(r);
            }
        }

        /// <summary>
        ///     Determines whether the specified content should be kept alive.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool ShouldKeepContentAlive(object content)
        {
            DependencyObject o = content as DependencyObject;
            if (o != null)
            {
                bool? result = GetKeepAlive(o);

                // if a value exists for given content, use it
                if (result.HasValue)
                {
                    return result.Value;
                }
            }
            // otherwise let the ModernFrame decide
            return KeepContentAlive;
        }

        /// <summary>
        ///     Gets a value indicating whether to keep specified object alive in a ModernFrame instance.
        /// </summary>
        /// <param name="o">The target dependency object.</param>
        /// <returns>Whether to keep the object alive. Null to leave the decision to the ModernFrame.</returns>
        public static bool? GetKeepAlive(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }
            return (bool?)o.GetValue(KeepAliveProperty);
        }

        /// <summary>
        ///     Sets a value indicating whether to keep specified object alive in a ModernFrame instance.
        /// </summary>
        /// <param name="o">The target dependency object.</param>
        /// <param name="value">Whether to keep the object alive. Null to leave the decision to the ModernFrame.</param>
        public static void SetKeepAlive(DependencyObject o, bool? value)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }
            o.SetValue(KeepAliveProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value whether content should be kept in memory.
        /// </summary>
        public bool KeepContentAlive
        {
            get => (bool)GetValue(KeepContentAliveProperty);
            set => SetValue(KeepContentAliveProperty, value);
        }

        /// <summary>
        ///     Gets or sets the content loader.
        /// </summary>
        public IContentLoader ContentLoader
        {
            get => (IContentLoader)GetValue(ContentLoaderProperty);
            set => SetValue(ContentLoaderProperty, value);
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is currently loading content.
        /// </summary>
        public bool IsLoadingContent
        {
#pragma warning disable S4275 // Getters and setters should access the expected fields
            get => (bool)GetValue(IsLoadingContentProperty);
#pragma warning restore S4275 // Getters and setters should access the expected fields
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions
            set => SetValue(IsLoadingContentProperty, (_isLoadingContent = value));
#pragma warning restore S1121 // Assignments should not be made from within sub-expressions
        }
        /// <summary>
        ///     Gets or sets the source of the current content.
        /// </summary>
        public Uri Source
        {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        ///     Clears the history stack.
        /// </summary>
        public void ClearHistory()
        {
            history.Clear();
        }
    }
}