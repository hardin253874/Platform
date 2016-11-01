// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;
using EDC.Database;

namespace EDC.SoftwarePlatform.Services.Data
{
	/// <summary>
	///     Defines a data column.
	/// </summary>
	[DataContract]
	public class DbDataColumn
	{
		/// <summary>
		///     Initializes a new instance of the DbDataColumn class.
		/// </summary>
		public DbDataColumn( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the DbDataColumn class.
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="type">An enumeration describing the type of the column.</param>
		public DbDataColumn( string columnName, DatabaseType type )
		{
			ColumnName = columnName;
			Type = type;
		}

		/// <summary>
		///     Initializes a new instance of the DbDataColumn class.
		/// </summary>
		/// <param name="id">
		///     An ID identifying the column.
		/// </param>
		/// <param name="columnName">
		///     A string containing the internal name of the column.
		/// </param>
		/// <param name="name">
		///     A string containing the display name of the column.
		/// </param>
		/// <param name="type">
		///     An enumeration describing the type of the column.
		/// </param>
		public DbDataColumn( Guid id, string columnName, string name, DatabaseType type )
			: this( columnName, type )
		{
			Id = id;
			Name = name;
		}

		/// <summary>
		///     Gets or sets the internal name of the column.
		/// </summary>
		[DataMember]
		public string ColumnName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the ID associated with the column.
		/// </summary>
		[DataMember]
		public Guid Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is hidden.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is hidden; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool IsHidden
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the display name of the column.
		/// </summary>
		[DataMember]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type of the column.
		/// </summary>
		[DataMember]
		public DatabaseType Type
		{
			get;
			set;
		}
	}
}