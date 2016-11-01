// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Configuration;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     System information.
	/// </summary>
	public static class SystemInfo
	{
		/// <summary>
		///     Lazy loaded version
		/// </summary>
		private static readonly Lazy<string> Version = new Lazy<string>( ( ) => ConfigurationSettings.GetServerConfigurationSection( ).SystemInfo.CurrentVersion, true );

		/// <summary>
		///     Lazy loaded branch
		/// </summary>
		private static readonly Lazy<string> Branch = new Lazy<string>( ( ) => ConfigurationSettings.GetServerConfigurationSection( ).SystemInfo.CurrentBranch, true );

        /// <summary>
		///     Lazy loaded install folder
		/// </summary>
		private static readonly Lazy<string> InstallFolderLazy = new Lazy<string>(() => ConfigurationSettings.GetServerConfigurationSection().SystemInfo.InstallFolder, true);

        /// <summary>
        ///     Gets the current branch name. I.e. the branch on which software been build.
        /// </summary>
        public static string BranchName => Branch.Value;

		/// <summary>
		///     Gets the current platform version. I.e. version of the running software.
		/// </summary>
		public static string PlatformVersion => Version.Value;

        /// <summary>
        /// Gets the install folder.
        /// </summary>
        /// <value>
        /// The install folder.
        /// </value>
        public static string InstallFolder => InstallFolderLazy.Value;
    }
}