// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     The Instance class.
	/// </summary>
	public class Instance
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="Instance" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="typeName">Name of the type.</param>
		/// <param name="createdDate">The created date.</param>
		/// <param name="modifiedDate">The modified date.</param>
		/// <param name="applications">The applications.</param>
		public Instance( long id, string name, string description, string alias, string typeName, DateTime createdDate, DateTime modifiedDate, string applications )
		{
			Id = id;
			Name = name;
			Description = description;
			Alias = alias;
			TypeName = typeName;
			CreatedDate = createdDate;
			ModifiedDate = modifiedDate;
			Applications = applications;
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
		///     Gets the applications.
		/// </summary>
		/// <value>
		///     The applications.
		/// </value>
		public string Applications
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the created date.
		/// </summary>
		/// <value>
		///     The created date.
		/// </value>
		public DateTime CreatedDate
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
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
		///     Gets the modified date.
		/// </summary>
		/// <value>
		///     The modified date.
		/// </value>
		public DateTime ModifiedDate
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
		///     Gets or sets the tool-tip.
		/// </summary>
		/// <value>
		///     The tool-tip.
		/// </value>
		public string Tooltip
		{
			get
			{
				return string.Format( @"Id:			{0}
Name:			{1}
Description:		{2}
Alias:			{3}
Type Name:		{4}
Created Date:		{5}
Modified Date:		{6}
Applications:		{7}", Id, Name, Description, Alias, TypeName, CreatedDate, ModifiedDate, Applications );
			}
		}

		/// <summary>
		///     Gets the name of the type.
		/// </summary>
		/// <value>
		///     The name of the type.
		/// </value>
		public string TypeName
		{
			get;
			private set;
		}
	}
}