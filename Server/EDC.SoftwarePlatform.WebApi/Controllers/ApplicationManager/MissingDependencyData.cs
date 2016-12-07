// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager
{
	/// <summary>
	///     Missing Dependency Data class.
	/// </summary>
	[DataContract]
	public class MissingDependencyData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MissingDependencyData" /> class.
		/// </summary>
		public MissingDependencyData( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MissingDependencyData" /> class.
		/// </summary>
		/// <param name="failure">The failure.</param>
		/// <param name="dependencyPackageId">The dependency package identifier.</param>
		/// <param name="dependencyAppPackageVersion">The dependency application package version.</param>
		public MissingDependencyData( DependencyFailure failure, long dependencyPackageId, string dependencyAppPackageVersion ) : this( )
		{
			if ( failure == null )
			{
				throw new ArgumentNullException( nameof( failure ) );
			}

			Id = dependencyPackageId;
			Name = failure.DependencyName;

			if ( failure.CurrentVersion != null )
			{
				CurrentVersion = failure.CurrentVersion.ToString( 4 );
			}

			Version = dependencyAppPackageVersion;
			Reason = failure.Reason;
		}

		/// <summary>
		///     Gets or sets the current version.
		/// </summary>
		/// <value>
		///     The current version.
		/// </value>
		[DataMember( Name = "currentVersion" )]
		public string CurrentVersion
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[DataMember( Name = "id" )]
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name" )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="DependentAppData" /> is an upgrade.
		/// </summary>
		/// <value>
		///     <c>true</c> if an upgrade; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "reason" )]
		public DependencyFailureReason Reason
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		[DataMember( Name = "version" )]
		public string Version
		{
			get;
			set;
		}
	}
}