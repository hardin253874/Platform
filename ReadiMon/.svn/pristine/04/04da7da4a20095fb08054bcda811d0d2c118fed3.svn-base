// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Application
{
    /// <summary>
    /// Platform History Plugin.
    /// </summary>
    [AddIn("Platform History Plugin", Version = "1.0.0.0")]
    public class PlatformHistoryPlugin : PluginBase, IPlugin // IPlugin - explicit is necessary
    {
        /// <summary>
        ///     The user interface
        /// </summary>
        private PlatformHistory _userInterface;

        /// <summary>
        /// Plugin constructor.
        /// </summary>
        public PlatformHistoryPlugin()
        {
            SectionOrdinal = 10;
            SectionName = "Application";
            EntryName = "Platform History";
            EntryOrdinal = 12;
            HasOptionsUserInterface = false;
            HasUserInterface = true;
        }

        /// <summary>
        ///     Gets the user interface.
        /// </summary>
        /// <returns></returns>
        public override FrameworkElement GetUserInterface()
        {
            return _userInterface ?? (_userInterface = new PlatformHistory(Settings));
        }

        /// <summary>
        ///     Called when updating settings.
        /// </summary>
        protected override void OnUpdateSettings()
        {
            base.OnUpdateSettings();

            var viewModel = _userInterface?.DataContext as PlatformHistoryViewModel;

            if (viewModel != null)
            {
                viewModel.PluginSettings = Settings;
            }
        }
    }
}
