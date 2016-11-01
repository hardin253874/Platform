// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     AutoNumber field.
	/// </summary>
    public class AutoNumberFieldHelper : IFieldHelper, IEntityFieldCreate, IEntityFieldSave
	{
        private readonly AutoNumberField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal AutoNumberFieldHelper(AutoNumberField fieldEntity)
        {
            _field = fieldEntity;
        }

		/// <summary>
		///     Called when an instance of a type containing an auto-number field is created.
		/// </summary>
		void IEntityFieldCreate.OnCreate( IEntity entity )
		{
			if ( entity == null )
			{
				return;
			}

			long userId;
			RequestContext.TryGetUserId( out userId );

			/////
			// This is already running in a transaction
			/////
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				/////
				// Command takes an Update Lock under SERIALIZABLE isolation to ensure concurrency.
				/////
				using ( DatabaseContextInfo.SetContextInfo( "Create autonumber instance" ) )
				using ( IDbCommand command = ctx.CreateCommand( "spCreateAutoNumberInstance", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@entityId", DbType.Int64, entity.Id );
					ctx.AddParameter( command, "@fieldId", DbType.Int64, _field.Id );
					ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );


					object newId = command.ExecuteScalar( );

					if ( newId != null && newId != DBNull.Value )
					{
                        /////
                        // Update the read-only cache with the new value.
                        /////
                        IEntityFieldValues readonlyFields = EntityFieldCache.Instance.GetOrCreate( entity.Id );

						/////
						// Set the value.
						/////
						readonlyFields[ _field.Id ] = ( int ) newId;
					}
				}
			}
		}

		/// <summary>
		///     Called when an entity field is saved.
		/// </summary>
		/// <param name="entity">The entity.</param>
		void IEntityFieldSave.OnSave( IEntity entity )
		{
			if ( entity == null )
			{
				return;
			}

			long userId;
			RequestContext.TryGetUserId( out userId );

			/////
			// This is already running in a transaction
			/////
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				/////
				// Command takes an Update Lock under SERIALIZABLE isolation to ensure concurrency.
				/////
				using ( DatabaseContextInfo.SetContextInfo( "Update autonumber instance" ) )
				using ( IDbCommand command = ctx.CreateCommand( "spUpdateAutoNumberInstance", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@entityId", DbType.Int64, entity.Id );
					ctx.AddParameter( command, "@fieldId", DbType.Int64, _field.Id );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					object newId = command.ExecuteScalar( );

					if ( newId != null && newId != DBNull.Value )
					{
                        IEntityFieldValues readonlyFields;

						/////
						// Update the read-only cache with the new value.
						/////
						if ( !EntityFieldCache.Instance.TryGetValue( entity.Id, out readonlyFields ) )
						{
							readonlyFields = new EntityFieldValues( );
							EntityFieldCache.Instance[ entity.Id ] = readonlyFields;
						}

						/////
						// Set the value.
						/////
						readonlyFields[ _field.Id ] = ( int ) newId;
					}
				}
			}
		}

		/// <summary>
		///     Converts a field into a database type object.
		/// </summary>
		/// <returns></returns>
		public DatabaseType ConvertToDatabaseType( )
		{
			return new Int32Type( );
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Int32;
        }

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public object ConvertXmlStringToObject( string data )
		{
			return int.Parse( data );
		}

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
		public string ValidateFieldValue( object value )
		{
			return null;
		}
	}
}