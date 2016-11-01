// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.SoftwarePlatform.Services.ApplicationManager;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager
{
	/// <summary>
	///     Extra data that helps the Application Manager page to determine which actions are available
	///     to the user for each Solution.
	/// </summary>
	[DataContract]
	public class AppManagerData : InstalledApplication
	{
		/// <summary>
		///     Gets or sets if the solution can be installed.
		/// </summary>
		[DataMember( Name = "candeploy" )]
		public bool CanDeploy
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets if the solution can be exported.
		/// </summary>
		[DataMember( Name = "canexport" )]
		public bool CanExport
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets if the solution can be published.
		/// </summary>
		[DataMember( Name = "canpublish" )]
		public bool CanPublish
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets if the solution can be repaired.
		/// </summary>
		[DataMember( Name = "canrepair" )]
		public bool CanRepair
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets if the solution can be upgraded.
		/// </summary>
		[DataMember( Name = "canupgrade" )]
		public bool CanUpgrade
		{
			get;
			set;
		}

        /// <summary>
        ///     Gets or sets if the solution can be uninstalled.
        /// </summary>
        [DataMember( Name = "canuninstall" )]
        public bool CanUninstall
        {
            get;
            set;
        }
	}
}