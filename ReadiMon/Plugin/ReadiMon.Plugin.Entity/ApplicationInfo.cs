// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Application Info
	/// </summary>
	public class ApplicationInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="name">The name.</param>
		public ApplicationInfo( long id, string alias, string name )
		{
			Id = id;
			Alias = alias;
			Name = name;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="name">The name.</param>
		/// <param name="tooltip">The tool tip.</param>
		public ApplicationInfo( long id, string alias, string name, string tooltip )
			: this( id, alias, name )
		{
			Tooltip = tooltip;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="name">The name.</param>
		/// <param name="tooltip">The tool tip.</param>
		/// <param name="type">The type.</param>
		public ApplicationInfo( long id, string alias, string name, string tooltip, ApplicationType type )
			: this( id, alias, name, tooltip )
		{
			Type = type;
		}

		/// <summary>
		///     Gets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		public string Alias
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tool tip.
		/// </summary>
		/// <value>
		///     The tool tip.
		/// </value>
		public string Tooltip
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public ApplicationType Type
		{
			get;
			private set;
		}
	}
}