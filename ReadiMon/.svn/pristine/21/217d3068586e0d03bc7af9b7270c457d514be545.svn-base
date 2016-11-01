// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Data;
using ReadiMon.Shared.Data;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Tenant Info.
	/// </summary>
	public class TenantInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="createdDate">The created date.</param>
		/// <param name="modifiedDate">The modified date.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		public TenantInfo( long id, string name, string description, string createdDate = null, string modifiedDate = null, string enabled = null )
		{
			Id = id;
			Name = name;
			Description = description;
			CreatedDate = createdDate;
			ModifiedDate = modifiedDate;
			Enabled = enabled;
		}

		/// <summary>
		///     Gets the created date.
		/// </summary>
		/// <value>
		///     The created date.
		/// </value>
		public string CreatedDate
		{
			get;
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
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="TenantInfo" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public string Enabled
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
		}

		/// <summary>
		///     Gets the modified date.
		/// </summary>
		/// <value>
		///     The modified date.
		/// </value>
		public string ModifiedDate
		{
			get;
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
		}

		/// <summary>
		///     Gets the search strings.
		/// </summary>
		/// <value>
		///     The search strings.
		/// </value>
		public static List<TenantInfo> LoadTenants( DatabaseManager databaseManager )
		{
			var tenants = new List<TenantInfo>
			{
				new TenantInfo( 0, "Global", "The global tenant" )
			};

			const string commandText = @"--ReadiMon - LoadTenants
SELECT Id, name, description FROM _vTenant ORDER BY name";

			using ( var command = databaseManager.CreateCommand( commandText ) )
			{
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						long id = reader.GetInt64( 0 );
						string name = reader.GetString( 1, null );
						string description = reader.GetString( 2, null );

						if ( string.IsNullOrEmpty( name ) )
						{
							continue;
						}

						tenants.Add( new TenantInfo( id, name, description ) );
					}
				}
			}

			return tenants;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var tenantInfo = obj as TenantInfo;

			if ( tenantInfo == null )
			{
				return false;
			}

			return Id == tenantInfo.Id &&
			       Name == tenantInfo.Name &&
			       Description == tenantInfo.Description &&
			       CreatedDate == tenantInfo.CreatedDate &&
			       ModifiedDate == tenantInfo.ModifiedDate;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hash = 13;

			hash = hash * 7 + Id.GetHashCode( );

			if ( Name != null )
			{
				hash = hash * 7 + Name.GetHashCode( );
			}

			if ( Description != null )
			{
				hash = hash * 7 + Description.GetHashCode( );
			}

			if ( CreatedDate != null )
			{
				hash = hash * 7 + CreatedDate.GetHashCode( );
			}

			if ( ModifiedDate != null )
			{
				hash = hash * 7 + ModifiedDate.GetHashCode( );
			}

			return hash;
		}
	}
}