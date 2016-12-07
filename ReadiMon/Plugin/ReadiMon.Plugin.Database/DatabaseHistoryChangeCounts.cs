// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Data;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Database History Change Counts class
	/// </summary>
	public class DatabaseHistoryChangeCounts
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHistoryChangeCounts" /> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public DatabaseHistoryChangeCounts( IDataReader reader )
		{
			EntityAdded = reader.GetInt32( 0 );
			EntityDeleted = reader.GetInt32( 1 );
			RelationshipAdded = reader.GetInt32( 2 );
			RelationshipDeleted = reader.GetInt32( 3 );
			AliasAdded = reader.GetInt32( 4 );
			AliasDeleted = reader.GetInt32( 5 );
			BitAdded = reader.GetInt32( 6 );
			BitDeleted = reader.GetInt32( 7 );
			DateTimeAdded = reader.GetInt32( 8 );
			DateTimeDeleted = reader.GetInt32( 9 );
			DecimalAdded = reader.GetInt32( 10 );
			DecimalDeleted = reader.GetInt32( 11 );
			GuidAdded = reader.GetInt32( 12 );
			GuidDeleted = reader.GetInt32( 13 );
			IntAdded = reader.GetInt32( 14 );
			IntDeleted = reader.GetInt32( 15 );
			NVarCharAdded = reader.GetInt32( 16 );
			NVarCharDeleted = reader.GetInt32( 17 );
			XmlAdded = reader.GetInt32( 18 );
			XmlDeleted = reader.GetInt32( 19 );
		}

		/// <summary>
		///     Gets or sets the alias added.
		/// </summary>
		/// <value>
		///     The alias added.
		/// </value>
		public int AliasAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the alias deleted.
		/// </summary>
		/// <value>
		///     The alias deleted.
		/// </value>
		public int AliasDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the bit added.
		/// </summary>
		/// <value>
		///     The bit added.
		/// </value>
		public int BitAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the bit deleted.
		/// </summary>
		/// <value>
		///     The bit deleted.
		/// </value>
		public int BitDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the date time added.
		/// </summary>
		/// <value>
		///     The date time added.
		/// </value>
		public int DateTimeAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the date time deleted.
		/// </summary>
		/// <value>
		///     The date time deleted.
		/// </value>
		public int DateTimeDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the decimal added.
		/// </summary>
		/// <value>
		///     The decimal added.
		/// </value>
		public int DecimalAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the decimal deleted.
		/// </summary>
		/// <value>
		///     The decimal deleted.
		/// </value>
		public int DecimalDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity added.
		/// </summary>
		/// <value>
		///     The entity added.
		/// </value>
		public int EntityAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity deleted.
		/// </summary>
		/// <value>
		///     The entity deleted.
		/// </value>
		public int EntityDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the unique identifier added.
		/// </summary>
		/// <value>
		///     The unique identifier added.
		/// </value>
		public int GuidAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the unique identifier deleted.
		/// </summary>
		/// <value>
		///     The unique identifier deleted.
		/// </value>
		public int GuidDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the int added.
		/// </summary>
		/// <value>
		///     The int added.
		/// </value>
		public int IntAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the int deleted.
		/// </summary>
		/// <value>
		///     The int deleted.
		/// </value>
		public int IntDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the n variable character added.
		/// </summary>
		/// <value>
		///     The n variable character added.
		/// </value>
		public int NVarCharAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the n variable character deleted.
		/// </summary>
		/// <value>
		///     The n variable character deleted.
		/// </value>
		public int NVarCharDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relationship added.
		/// </summary>
		/// <value>
		///     The relationship added.
		/// </value>
		public int RelationshipAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relationship deleted.
		/// </summary>
		/// <value>
		///     The relationship deleted.
		/// </value>
		public int RelationshipDeleted
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the XML added.
		/// </summary>
		/// <value>
		///     The XML added.
		/// </value>
		public int XmlAdded
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the XML deleted.
		/// </summary>
		/// <value>
		///     The XML deleted.
		/// </value>
		public int XmlDeleted
		{
			get;
			set;
		}
	}
}