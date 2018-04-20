using System.Windows;
using System.Windows.Controls;

namespace ModernUI.Windows.Controls
{
    /// <summary>
    ///     Represents a control that indicates that an operation is ongoing.
    /// </summary>
    [TemplateVisualState(GroupName = GroupActiveStates, Name = StateInactive)]
    [TemplateVisualState(GroupName = GroupActiveStates, Name = StateActive)]
    public class ModernProgressRing
        : Control
    {
        const string GroupActiveStates = "ActiveStates";
        const string StateInactive = "Inactive";
        const string StateActive = "Active";

        /// <summary>
        ///     Identifies the IsActive property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ModernProgressRing),
                new PropertyMetadata(false, OnIsActiveChanged));

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModernProgressRing" /> class.
        /// </summary>
        public ModernProgressRing()
        {
            DefaultStyleKey = typeof(ModernProgressRing);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the <see cref="ModernProgressRing" /> is showing progress.
        /// </summary>
        public bool IsActive
        {
            get => (bool) GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        void GotoCurrentState(bool animate)
        {
            string state = IsActive ? StateActive : StateInactive;

            VisualStateManager.GoToState(this, state, animate);
        }

        /// <summary>
        ///     When overridden in a derived class, is invoked whenever application code or internal processes call
        ///     <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            GotoCurrentState(false);
        }

        static void OnIsActiveChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((ModernProgressRing)o).GotoCurrentState(true);
        }
    }
}