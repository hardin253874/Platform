// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager
{
	/// <summary>
	///     The dependent application data class.
	/// </summary>
	[DataContract]
	public class DependentAppData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DependentAppData" /> class.
		/// </summary>
		public DependentAppData( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DependentAppData" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="version">The version.</param>
		/// <param name="upgrade">if set to <c>true</c> [upgrade].</param>
		/// <param name="currentVersion">The current version.</param>
		public DependentAppData( long id, string name, string version, bool upgrade = false, string currentVersion = null )
			: this( )
		{
			Id = id;
			Name = name;
			Version = version;
			Upgrade = upgrade;
			CurrentVersion = currentVersion;
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
		[DataMember( Name = "upgrade" )]
		public bool Upgrade
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