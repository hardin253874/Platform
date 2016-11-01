// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Plugin.Application
{
    /// <summary>
    /// Interaction logic for PlatformHistory.xaml
    /// </summary>
    public partial class PlatformHistory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformHistory" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public PlatformHistory(IPluginSettings settings)
        {
            InitializeComponent();

            var viewModel = new PlatformHistoryViewModel(settings);
            DataContext = viewModel;
        }
    }
}
