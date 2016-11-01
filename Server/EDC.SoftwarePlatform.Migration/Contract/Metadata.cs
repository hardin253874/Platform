// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Sources;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Class representing the Metadata type.
	/// </summary>
	public class Metadata
	{
		/// <summary>
		///     The GUID that represents the overall application (without respect to version).
		/// </summary>
		/// <value>
		///     The app id.
		/// </value>
		public Guid AppId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the app.
		/// </summary>
		/// <value>
		///     The name of the app.
		/// </value>
		public string AppName
		{
			get;
			set;
		}

		/// <summary>
		///     The GUID that represents this version of this package.
		/// </summary>
		/// <value>
		///     The app version id.
		/// </value>
		public Guid AppVerId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the dependencies.
		/// </summary>
		/// <value>
		///     The dependencies.
		/// </value>
		public IList<SolutionDependency> Dependencies
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     The name of the application/package.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the publish date.
		/// </summary>
		/// <value>
		/// The publish date.
		/// </value>
		public DateTime PublishDate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publisher.
		/// </summary>
		/// <value>
		///     The publisher.
		/// </value>
		public string Publisher
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publisher URL.
		/// </summary>
		/// <value>
		///     The publisher URL.
		/// </value>
		public string PublisherUrl
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the release date.
		/// </summary>
		/// <value>
		///     The release date.
		/// </value>
		public DateTime ReleaseDate
		{
			get;
			set;
        }

        /// <summary>
        ///     Gets or sets the root entity being exported/imported.
        /// </summary>
        /// <value>
        ///     The Upgrade ID of the root entity.
        /// </value>
        public Guid RootEntityId
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public SourceType Type
		{
			get;
			set;
		}

		/// <summary>
		///     The dotted-numeric string that represents this version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public string Version
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the platform version.
		/// </summary>
		/// <value>
		///     The platform version.
		/// </value>
		public string PlatformVersion
		{
			get;
			set;
        }

        /// <summary>
        ///     Gets or sets the platform version.
        /// </summary>
        /// <value>
        ///     The platform version.
        /// </value>
        public Func<Guid, RelationshipTypeEntry> RelationshipTypeCallback
        {
            get;
            set;
        }
    }
}