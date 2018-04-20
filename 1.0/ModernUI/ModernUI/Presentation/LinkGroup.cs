namespace ModernUI.Presentation
{
    /// <summary>
    ///     Represents a named group of links.
    /// </summary>
    public class LinkGroup
        : Displayable
    {
        string groupKey;
        Link selectedLink;

        /// <summary>
        ///     Gets or sets the key of the group.
        /// </summary>
        /// <value>The key of the group.</value>
        /// <remarks>
        ///     The group key is used to group link groups in a <see cref="ModernUI.Windows.Controls.ModernMenu" />.
        /// </remarks>
        public string GroupKey
        {
            get => groupKey;
            set
            {
                if (groupKey != value)
                {
                    groupKey = value;
                    OnPropertyChanged("GroupKey");
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected link in this group.
        /// </summary>
        /// <value>The selected link.</value>
        internal Link SelectedLink
        {
            get => selectedLink;
            set
            {
                if (selectedLink != value)
                {
                    selectedLink = value;
                    OnPropertyChanged("SelectedLink");
                }
            }
        }

        /// <summary>
        ///     Gets the links.
        /// </summary>
        /// <value>The links.</value>
        public LinkCollection Links { get; } = new LinkCollection();
    }
}