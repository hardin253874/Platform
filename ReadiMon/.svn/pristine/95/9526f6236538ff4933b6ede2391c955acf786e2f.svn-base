// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Type info
	/// </summary>
	public class TypeInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TypeInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="typeName">Name of the type.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="createdDate">The created date.</param>
		/// <param name="modifiedDate">The modified date.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="isAbstract">if set to <c>true</c> [is abstract].</param>
		/// <param name="isSealed">The is sealed.</param>
		public TypeInfo( long id, long typeId, string typeName, string name, string description, DateTime createdDate, DateTime modifiedDate, string alias, bool isAbstract, bool isSealed )
		{
			Id = id;
			TypeId = typeId;
			TypeName = typeName;
			Name = name;
			Description = description;
			CreatedDate = createdDate;
			ModifiedDate = modifiedDate;
			Alias = alias;
			IsAbstract = isAbstract;
			IsSealed = isSealed;

			Applications = new List<ApplicationInfo>( );
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
		public List<ApplicationInfo> Applications
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
		///     Gets a value indicating whether this instance is abstract.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is abstract; otherwise, <c>false</c>.
		/// </value>
		public bool IsAbstract
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the is sealed.
		/// </summary>
		/// <value>
		///     The is sealed.
		/// </value>
		public bool IsSealed
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
		///     Gets the tool tip.
		/// </summary>
		/// <value>
		///     The tool tip.
		/// </value>
		public string Tooltip
		{
			get
			{
				return string.Format( @"Name:			{0}
Description:		{1}
Id:			{2}
Alias:			{3}
Type Id:			{4}
Type Name:		{5}
Created Date:		{6}
Modified Date:		{7}
Is Abstract:		{8}
Is Sealed:		{9}
Application Id(s):		{10}
Application Alias(s):	{11}
Application Name(s):	{12}", Name, Description, Id, Alias, TypeId, TypeName, CreatedDate == DateTime.MinValue ? string.Empty : CreatedDate.ToString( ), ModifiedDate == DateTime.MinValue ? string.Empty : ModifiedDate.ToString( ), IsAbstract, IsSealed, GetApplicationId( ), GetApplicationAlias( ), GetApplicationName( ) );
			}
		}

		/// <summary>
		///     Gets the type identifier.
		/// </summary>
		/// <value>
		///     The type identifier.
		/// </value>
		public long TypeId
		{
			get;
			private set;
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

		/// <summary>
		///     Gets the application alias.
		/// </summary>
		/// <returns></returns>
		private string GetApplicationAlias( )
		{
			if ( Applications == null || Applications.Count <= 0 )
			{
				return string.Empty;
			}

			return string.Join( ", ", Applications.Select( a => a.Alias ) );
		}

		/// <summary>
		///     Gets the application identifier.
		/// </summary>
		/// <returns></returns>
		private string GetApplicationId( )
		{
			if ( Applications == null || Applications.Count <= 0 )
			{
				return string.Empty;
			}

			return string.Join( ", ", Applications.Select( a => a.Id ) );
		}

		/// <summary>
		///     Gets the name of the application.
		/// </summary>
		/// <returns></returns>
		private string GetApplicationName( )
		{
			if ( Applications == null || Applications.Count <= 0 )
			{
				return string.Empty;
			}

			return string.Join( ", ", Applications.Select( a => a.Name ) );
		}
	}
}