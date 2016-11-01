// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EDC.ReadiNow.Database;

namespace LogViewer.Common
{
    /// <summary>
    /// 
    /// </summary>
    internal static class IdResolver
    {
        #region Public Methods
        /// <summary>
        /// Resolves the id to an object.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static ObjectInfo ResolveId(Guid id)
        {
            ObjectInfo info = GetTypeInfo(id);
            if (info == null)
            {
                info = GetResourceInfo(id);
            }

            return info;
        }
        #endregion


        #region Non-Public Methods
        /// <summary>
        /// Gets the resource info.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static ResourceObjectInfo GetResourceInfo(Guid id)
        {
            ResourceObjectInfo resourceInfo = null;

            using (DatabaseContext context = DatabaseContext.GetContext())
            {
	            using ( IDbCommand cmd = context.CreateCommand( @"SELECT TOP 1 r.Name, r.TypeId, ISNULL(t.TypeName, N'') AS TypeName, r.SolutionId, 
                                                         ISNULL(rsol.Name, N'') AS [SolutionName], r.TenantId, ISNULL(rten.Name, N'') AS TenantName FROM Resource r
                                                         LEFT JOIN Type t ON t.Id = r.TypeId
                                                         LEFT JOIN Resource rsol ON rsol.Id = r.SolutionId
                                                         LEFT JOIN Resource rten ON rten.Id = r.TenantId
                                                         WHERE r.Id = @id" ) )
	            {
		            context.AddParameter( cmd, "@id", EDC.Database.DatabaseType.GuidType ).Value = id;

		            using ( IDataReader reader = cmd.ExecuteReader( ) )
		            {
			            if ( reader.Read( ) )
			            {
				            string name = reader.GetString( 0 );
				            Guid typeId = reader.GetGuid( 1 );
				            string typeName = reader.GetString( 2 );
				            Guid solutionId = reader.GetGuid( 3 );
				            string solutionName = reader.GetString( 4 );
				            Guid tenantId = reader.GetGuid( 5 );
				            string tenantName = reader.GetString( 6 );

				            if ( tenantId == Guid.Empty )
				            {
					            tenantName = "Global";
				            }

				            resourceInfo = new ResourceObjectInfo( )
					            {
						            Id = id,
						            Name = name,
						            TypeId = typeId,
						            TypeName = typeName,
						            SolutionId = solutionId,
						            SolutionName = solutionName,
						            TenantId = tenantId,
						            TenantName = tenantName
					            };
			            }
		            }
	            }
            }

            return resourceInfo;
        }


        /// <summary>
        /// Gets the type info.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static TypeObjectInfo GetTypeInfo(Guid id)
        {
            TypeObjectInfo typeInfo = null;

            using (DatabaseContext context = DatabaseContext.GetContext())
            {
	            using ( IDbCommand cmd = context.CreateCommand( @"SELECT TOP 1 t.SolutionId, ISNULL(r.Name, N'') AS [SolutionName], t.Name, t.TypeName, t.AssemblyName FROM Type t
                                                         LEFT JOIN Resource r ON r.Id = t.SolutionId
                                                         WHERE t.Id = @id" ) )
	            {
		            context.AddParameter( cmd, "@id", EDC.Database.DatabaseType.GuidType ).Value = id;

		            using ( IDataReader reader = cmd.ExecuteReader( ) )
		            {
			            if ( reader.Read( ) )
			            {
				            Guid solutionId = reader.GetGuid( 0 );
				            string solutionName = reader.GetString( 1 );
				            string name = reader.GetString( 2 );
				            string typeName = reader.GetString( 3 );
				            string assemblyName = reader.GetString( 4 );

				            typeInfo = new TypeObjectInfo( )
					            {
						            Id = id,
						            Name = name,
						            SolutionId = solutionId,
						            SolutionName = solutionName,
						            TypeName = typeName,
						            AssemblyName = assemblyName
					            };
			            }
		            }
	            }
            }

            return typeInfo;
        }
        #endregion
    }       
}
