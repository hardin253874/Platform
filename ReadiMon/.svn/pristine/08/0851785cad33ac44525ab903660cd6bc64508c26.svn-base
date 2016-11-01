// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ReadiMon.Shared.Data;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     The historical transaction class.
	/// </summary>
	public class HistoricalTransaction
	{
		/// <summary>
		///     The tenant cache
		/// </summary>
		private static readonly IDictionary<long, string> _tenantCache = new Dictionary<long, string>( );

		/// <summary>
		///     The user cache
		/// </summary>
		private static readonly IDictionary<long, string> _userCache = new Dictionary<long, string>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="HistoricalTransaction" /> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public HistoricalTransaction( IDataReader reader )
		{
			TransactionId = reader.GetInt64( 0 );
			UserId = reader.GetInt64( 1, -1 );
			TenantId = reader.GetInt64( 2, -1 );
			Spid = reader.GetInt16( 3 );
			TimeStamp = new DateTime( reader.GetDateTime( 4 ).Ticks, DateTimeKind.Utc );
			HostName = reader.GetString( 5, string.Empty ).Trim( ).TrimEnd( '\0' );
			ProgramName = reader.GetString( 6 ).Trim( ).TrimEnd( '\0' );
			Domain = reader.GetString( 7, string.Empty ).Trim( ).TrimEnd( '\0' );
			UserName = reader.GetString( 8, string.Empty ).Trim( ).TrimEnd( '\0' );
			LoginName = reader.GetString( 9, string.Empty ).Trim( ).TrimEnd( '\0' );
			Context = reader.GetString( 10, string.Empty ).Trim( ).TrimEnd( '\0' );
			EntityAdded = reader.GetInt32( 11 );
			EntityDeleted = reader.GetInt32( 12 );
			RelationshipAdded = reader.GetInt32( 13 );
			RelationshipDeleted = reader.GetInt32( 14 );
			AliasAdded = reader.GetInt32( 15 );
			AliasDeleted = reader.GetInt32( 16 );
			BitAdded = reader.GetInt32( 17 );
			BitDeleted = reader.GetInt32( 18 );
			DateTimeAdded = reader.GetInt32( 19 );
			DateTimeDeleted = reader.GetInt32( 20 );
			DecimalAdded = reader.GetInt32( 21 );
			DecimalDeleted = reader.GetInt32( 22 );
			GuidAdded = reader.GetInt32( 23 );
			GuidDeleted = reader.GetInt32( 24 );
			IntAdded = reader.GetInt32( 25 );
			IntDeleted = reader.GetInt32( 26 );
			NVarCharAdded = reader.GetInt32( 27 );
			NVarCharDeleted = reader.GetInt32( 28 );
			XmlAdded = reader.GetInt32( 29 );
			XmlDeleted = reader.GetInt32( 30 );
		}

		/// <summary>
		///     Gets the tenant cache.
		/// </summary>
		/// <value>
		///     The tenant cache.
		/// </value>
		public static IDictionary<long, string> TenantCache
		{
			get
			{
				return _tenantCache;
			}
		}

		/// <summary>
		///     Gets the user cache.
		/// </summary>
		/// <value>
		///     The user cache.
		/// </value>
		public static IDictionary<long, string> UserCache
		{
			get
			{
				return _userCache;
			}
		}

		/// <summary>
		/// Gets the name of the tenant.
		/// </summary>
		/// <value>
		/// The name of the tenant.
		/// </value>
		public string TenantName
		{
			get
			{
				string tenantName;

				if ( !TenantCache.TryGetValue( TenantId, out tenantName ) )
				{
					return "Multiple";
				}

				return tenantName;
			}
		}

		/// <summary>
		/// Gets the name of the active user.
		/// </summary>
		/// <value>
		/// The name of the active user.
		/// </value>
		public string ActiveUserName
		{
			get
			{
				string userName;

				if ( !UserCache.TryGetValue( UserId, out userName ) )
				{
					return "Unspecified";
				}

				return userName;
			}
		}

		/// <summary>
		///     Gets the aliasentity added.
		/// </summary>
		/// <value>
		///     The aliasentity added.
		/// </value>
		public int AliasAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the alias deleted.
		/// </summary>
		/// <value>
		///     The alias deleted.
		/// </value>
		public int AliasDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the bit added.
		/// </summary>
		/// <value>
		///     The bit added.
		/// </value>
		public int BitAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the bit deleted.
		/// </summary>
		/// <value>
		///     The bit deleted.
		/// </value>
		public int BitDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		public string Context
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the date time added.
		/// </summary>
		/// <value>
		///     The date time added.
		/// </value>
		public int DateTimeAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the date time deleted.
		/// </summary>
		/// <value>
		///     The date time deleted.
		/// </value>
		public int DateTimeDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the decimal added.
		/// </summary>
		/// <value>
		///     The decimal added.
		/// </value>
		public int DecimalAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the decimal deleted.
		/// </summary>
		/// <value>
		///     The decimal deleted.
		/// </value>
		public int DecimalDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the domain.
		/// </summary>
		/// <value>
		///     The domain.
		/// </value>
		public string Domain
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entity added.
		/// </summary>
		/// <value>
		///     The entity added.
		/// </value>
		public int EntityAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entity deleted.
		/// </summary>
		/// <value>
		///     The entity deleted.
		/// </value>
		public int EntityDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the unique identifier added.
		/// </summary>
		/// <value>
		///     The unique identifier added.
		/// </value>
		public int GuidAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the unique identifier deleted.
		/// </summary>
		/// <value>
		///     The unique identifier deleted.
		/// </value>
		public int GuidDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the host.
		/// </summary>
		/// <value>
		///     The name of the host.
		/// </value>
		public string HostName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the int added.
		/// </summary>
		/// <value>
		///     The int added.
		/// </value>
		public int IntAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the int deleted.
		/// </summary>
		/// <value>
		///     The int deleted.
		/// </value>
		public int IntDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the login.
		/// </summary>
		/// <value>
		///     The name of the login.
		/// </value>
		public string LoginName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the next transaction.
		/// </summary>
		/// <value>
		///     The next transaction.
		/// </value>
		public HistoricalTransaction NextTransaction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the n variable character added.
		/// </summary>
		/// <value>
		///     The n variable character added.
		/// </value>
		public int NVarCharAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the n variable character deleted.
		/// </summary>
		/// <value>
		///     The n variable character deleted.
		/// </value>
		public int NVarCharDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the program.
		/// </summary>
		/// <value>
		///     The name of the program.
		/// </value>
		public string ProgramName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationship added.
		/// </summary>
		/// <value>
		///     The relationship added.
		/// </value>
		public int RelationshipAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationship deleted.
		/// </summary>
		/// <value>
		///     The relationship deleted.
		/// </value>
		public int RelationshipDeleted
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the spid.
		/// </summary>
		/// <value>
		///     The spid.
		/// </value>
		public short Spid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the time stamp.
		/// </summary>
		/// <value>
		///     The time stamp.
		/// </value>
		public DateTime TimeStamp
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tooltip.
		/// </summary>
		/// <value>
		///     The tooltip.
		/// </value>
		public string Tooltip
		{
			get
			{
				StringBuilder sb = new StringBuilder( );

				sb.AppendLine( $"Host name:		{HostName}" );
				sb.AppendLine( $"Domain:			{Domain}" );
				sb.AppendLine( $"Username:		{UserName}" );

				if ( EntityAdded > 0 )
					sb.AppendLine( $"Entity Added:		{EntityAdded}" );
				if ( EntityDeleted > 0 )
					sb.AppendLine( $"Entity Deleted:		{EntityDeleted}" );
				if ( RelationshipAdded > 0 )
					sb.AppendLine( $"Relationship Added:	{RelationshipAdded}" );
				if ( RelationshipDeleted > 0 )
					sb.AppendLine( $"Relationship Deleted:	{RelationshipDeleted}" );
				if ( AliasAdded > 0 )
					sb.AppendLine( $"Alias Added:		{AliasAdded}" );
				if ( AliasDeleted > 0 )
					sb.AppendLine( $"Alias Deleted:		{AliasDeleted}" );
				if ( BitAdded > 0 )
					sb.AppendLine( $"Bit Added:		{BitAdded}" );
				if ( BitDeleted > 0 )
					sb.AppendLine( $"Bit Deleted:		{BitDeleted}" );
				if ( DateTimeAdded > 0 )
					sb.AppendLine( $"DateTime Added:		{DateTimeAdded}" );
				if ( DateTimeDeleted > 0 )
					sb.AppendLine( $"DateTime Deleted:	{DateTimeDeleted}" );
				if ( DecimalAdded > 0 )
					sb.AppendLine( $"Decimal Added:		{DecimalAdded}" );
				if ( DecimalDeleted > 0 )
					sb.AppendLine( $"Decimal Deleted:		{DecimalDeleted}" );
				if ( GuidAdded > 0 )
					sb.AppendLine( $"Guid Added:		{GuidAdded}" );
				if ( GuidDeleted > 0 )
					sb.AppendLine( $"Guid Deleted:		{GuidDeleted}" );
				if ( IntAdded > 0 )
					sb.AppendLine( $"Int Added:		{IntAdded}" );
				if ( IntDeleted > 0 )
					sb.AppendLine( $"Int Deleted:		{IntDeleted}" );
				if ( NVarCharAdded > 0 )
					sb.AppendLine( $"NVarChar Added:		{NVarCharAdded}" );
				if ( NVarCharDeleted > 0 )
					sb.AppendLine( $"NVarChar Deleted:	{NVarCharDeleted}" );
				if ( XmlAdded > 0 )
					sb.AppendLine( $"Xml Added:		{XmlAdded}" );
				if ( XmlDeleted > 0 )
					sb.AppendLine( $"Xml Deleted:		{XmlDeleted}" );

				return sb.ToString( ).TrimEnd( '\n', '\r' );
			}
		}

		/// <summary>
		///     Gets the transaction identifier.
		/// </summary>
		/// <value>
		///     The transaction identifier.
		/// </value>
		public long TransactionId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the user identifier.
		/// </summary>
		/// <value>
		///     The user identifier.
		/// </value>
		public long UserId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the user.
		/// </summary>
		/// <value>
		///     The name of the user.
		/// </value>
		public string UserName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the XML added.
		/// </summary>
		/// <value>
		///     The XML added.
		/// </value>
		public int XmlAdded
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the XML deleted.
		/// </summary>
		/// <value>
		///     The XML deleted.
		/// </value>
		public int XmlDeleted
		{
			get;
			private set;
		}
	}
}