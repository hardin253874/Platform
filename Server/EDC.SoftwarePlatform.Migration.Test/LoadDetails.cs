// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Test
{
	/// <summary>
	///     Load details.
	/// </summary>
	public class LoadDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="LoadDetails" /> class.
		/// </summary>
		/// <param name="appId">The application identifier.</param>
		/// <param name="appPackageId">The application package identifier.</param>
		public LoadDetails( Guid appId, Guid appPackageId )
		{
			AppId = appId;
			AppPackageId = appPackageId;
		}

		/// <summary>
		///     Gets the application identifier.
		/// </summary>
		/// <value>
		///     The application identifier.
		/// </value>
		public Guid AppId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the application package identifier.
		/// </summary>
		/// <value>
		///     The application package identifier.
		/// </value>
		public Guid AppPackageId
		{
			get;
			private set;
		}
	}
}