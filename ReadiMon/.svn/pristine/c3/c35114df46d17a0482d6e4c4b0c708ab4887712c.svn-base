// Copyright 2011-2015 Global Software Innovation Pty Ltd
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis
{
    /// <summary>
    ///     Interaction logic for RemoteExecControl.xaml
    /// </summary>
    public partial class RemoteExecControl
    {
        /// <summary>
        ///     The view model
        /// </summary>
        private readonly RemoteExecViewModel _viewModel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RemoteExecControl" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public RemoteExecControl(IPluginSettings settings)
        {
            InitializeComponent();

            _viewModel = new RemoteExecViewModel(settings);

            DataContext = _viewModel;            
        }
    }
}