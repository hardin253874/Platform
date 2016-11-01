// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Database;
using System;
using System.Collections.Generic;
using System.Data;
using EDC.Database;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    /// A SecuredData provider that stores the data using the databse SecuredData SPs
    /// </summary>
    public class DbSecuredData: ISecuredData
    {

        /// <summary>
        /// Set a value into secured data.
        /// </summary>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <param name="value">The value to be stored</param>
        public Guid Create(long tenantId, string context, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var secureId = Guid.NewGuid();

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand command = ctx.CreateCommand("spSecuredDataCreate", CommandType.StoredProcedure))
                { 
                    
                    ctx.AddParameter(command, "@tenantId", DbType.Int64, tenantId);
                    ctx.AddParameter(command, "@context", DbType.AnsiString, context);
                    ctx.AddParameter(command, "@secureId", DbType.Guid, secureId);
                    ctx.AddParameter(command, "@data", DbType.String, value);

                    command.ExecuteNonQuery();
                }
            }

            return secureId;
        }

        /// <summary>
        /// Get a value stored securely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <returns>The secured value, null if no value </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the securedId is not found</exception>
        public string Read(Guid secureId)
        {
            if (secureId == Guid.Empty)
                throw new ArgumentException("Can not be an empty Id", nameof(secureId));

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand command = ctx.CreateCommand("spSecuredDataRead", CommandType.StoredProcedure))
                {

                    ctx.AddParameter(command, "@secureId", DbType.Guid, secureId);

                    var result = command.ExecuteScalar();

                    if (result == DBNull.Value)
                        return null;
                    else if (result == null)
                        throw new SecureIdNotFoundException($"Key not present. secureId: '{secureId}'");
                    else
                        return (string)result;
                }
            }

        }



        /// <summary>
        /// Set a value into secured data.
        /// </summary>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <param name="value">The value to be stored</param>
        public void Update(Guid secureId, string value)
        {
            if (secureId == Guid.Empty)
                throw new ArgumentException(nameof(secureId));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContextInfo.SetContextInfo( "Update secure data" ) )
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "spSecuredDataUpdate", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@secureId", DbType.Guid, secureId );
					ctx.AddParameter( command, "@data", DbType.String, value );
					ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					var rows = command.ExecuteNonQuery( );

					if ( rows < 1 )
						throw new SecureIdNotFoundException( $"Key not present. secureId: '{secureId}'" );
				}
			}
        }

        /// <summary>
        /// Clear a value stored in secured data
        /// </summary>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <exception cref="SecureIdNotFoundException">Thrown if the securedId is not found</exception>
        public void Delete(Guid secureId)
        {
            if (secureId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(secureId));

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContextInfo.SetContextInfo( "Delete secure data" ) )
			using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand command = ctx.CreateCommand("spSecuredDataDelete", CommandType.StoredProcedure))
                {
                    ctx.AddParameter(command, "@secureId", DbType.Guid, secureId);
					ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					var rows = command.ExecuteNonQuery();

                    if (rows == 0)
                        throw new SecureIdNotFoundException($"Key not present. secureId: '{secureId}'");
                }
            }
        }
    }
}
