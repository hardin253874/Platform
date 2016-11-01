// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using EDC.Collections.Generic;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using System.Threading;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     Entity type cache.
    /// </summary>
    public class EntityTypeCache :
        MultikeyDictionary<long, EntityTypeCache.EntityTypeTenantKey, EntityTypeCache.EntityTypeContainer>
    {
        /// <summary>
        ///     Singleton instance.
        /// </summary>
        private static readonly Lazy<EntityTypeCache> CacheInstance = new Lazy<EntityTypeCache>(() => new EntityTypeCache(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     Static sync root.
        /// </summary>
        private static readonly object StaticSyncRoot = new object();

        /// <summary>
        ///     Prevents a default instance of the <see cref="EntityTypeCache" /> class from being created.
        /// </summary>
        private EntityTypeCache()
        {
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static EntityTypeCache Instance
        {
            get
            {
                return CacheInstance.Value;
            }
        }

        /// <summary>
        ///     Gets the assignable types for the specified type name.
        /// </summary>
        /// <param name="typeId">The type id.</param>
        /// <returns>
        ///     A set of type identifiers that represent the assignable types for the specified type.
        /// </returns>
        public static HashSet<long> GetAssignableTypes(long typeId)
        {
            EntityTypeContainer container;

            /////
            // Hit the cache.
            /////
            Instance.TryGetValue(typeId, out container);

            if (container == null || container.AssignableTypesMember == null)
            {
                var assignableTypes = new HashSet<long>
					{
						typeId
					};

                /////
                // Add the current type.
                /////

                IEntity entityType = Entity.Get(new EntityRef(typeId));

                if (entityType != null)
                {
                    long inheritsId = WellKnownAliases.CurrentTenant.Inherits;

                    var inheritsEntityRef = new EntityRef(inheritsId);

                    /////
                    // Add the 'inherits' types (base types).
                    /////
                    AggregateTypes(entityType, inheritsEntityRef, Direction.Forward, assignableTypes);
                }

                if (container == null)
                {
                    container = new EntityTypeContainer(typeId);
                    Instance[typeId] = container;
                }

                container.AssignableTypes = assignableTypes;
            }

            return container.AssignableTypes;
        }

        /// <summary>
        ///     Gets the assignable types for the specified type name.
        /// </summary>
        /// <param name="typeIds">The type ids.</param>
        /// <returns>
        ///     A set of type identifiers that represent the assignable types for the specified type.
        /// </returns>
        public static HashSet<long> GetAssignableTypes(IEnumerable<long> typeIds)
        {
            // If there is a single type, then just return it's set directly - for performance.
            // If we encounter a second type, then create a dedicated  there's more than one element, then create a temp set to hold the result in.

            HashSet<long> result = null;

            long index = 0;
            foreach (long typeId in typeIds)
            {
                var current = GetAssignableTypes(typeId);
                if (index == 0)
                {
                    result = current;
                }
                else if (index == 1)
                {
                    var aggregate = new HashSet<long>();
                    aggregate.UnionWith(result);
                    aggregate.UnionWith(current);
                    result = aggregate;
                }
                else
                {
                    result.UnionWith(current);
                }
                index++;
            }

            if (result == null)
            {
                result = new HashSet<long>();
            }

            return result;
        }

        /// <summary>
        ///     Gets the name of the data table used by this type.
        /// </summary>
        /// <param name="typeId">The id.</param>
        /// <returns>
        ///     The name of the data table used by this entity type.
        /// </returns>
        public static string GetDataTableName(long typeId)
        {
            EntityTypeContainer container;

            /////
            // Hit the cache.
            /////
            Instance.TryGetValue(typeId, out container);

            if (container == null || (container.DataTableNameMember == null && !container.DataTableNameResolvedMember))
            {
                /////
                // Initialize the dataTable to be a relationship.
                /////
                string dataTableName = "Relationship";
                long dbFieldTableId = WellKnownAliases.CurrentTenant.DbFieldTable;

                using (DatabaseContext ctx = DatabaseContext.GetContext())
                {
                    using (IDbCommand command = ctx.CreateCommand())
                    {
                        command.CommandText = "dbo.spData_NVarCharRead";
                        command.CommandType = CommandType.StoredProcedure;                                                

                        ctx.AddParameter(command, "@entityId", DbType.Int64, typeId);
                        ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);
                        ctx.AddParameter(command, "@fieldId", DbType.Int64, dbFieldTableId);

                        using (IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(3))
                            {                                
                                dataTableName = reader.GetString(3);                                
                            }
                        }                        
                    }
                }

                if (container == null)
                {
                    container = new EntityTypeContainer(typeId);
                    Instance[typeId] = container;
                }

                container.DataTableName = dataTableName;
            }

            return container.DataTableName;
        }

	    /// <summary>
	    ///     Gets the database type of the specified entity type.
	    /// </summary>
	    /// <param name="typeId">The type id.</param>
	    /// <returns>
	    ///     The database type of the specified entity if found; null otherwise.
	    /// </returns>
	    public static DbType? GetDbType( long typeId )
	    {
		    EntityTypeContainer container;

		    /////
		    // Hit the cache.
		    /////
		    Instance.TryGetValue( typeId, out container );

		    if ( container == null || ( container.DbTypeMember == null && !container.DbTypeResolvedMember ) )
		    {
			    /////
			    // Initialize the dataTable to be a relationship.
			    /////
			    DbType? dbType = null;

			    using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			    {
				    using ( IDbCommand command = ctx.CreateCommand( ) )
				    {
					    /////
					    // TODO: Replace this with a stored procedure call.
					    /////
					    command.CommandText =
							@"-- EntityTypeCache: GetDbType
SELECT
	Data
FROM
	dbo.Data_NVarChar
WHERE
	TenantId = @tenantId
	AND FieldId = @dbType
	AND EntityId = @id";

					    ctx.AddParameter( command, "@id", DbType.Int64, typeId );
                        ctx.AddParameter( command, "@dbType", DbType.Int64, WellKnownAliases.CurrentTenant.DbType );
					    ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					    object result = command.ExecuteScalar( );

					    if ( result != null && result != DBNull.Value )
					    {
						    var stringResult = result as string;

						    if ( !string.IsNullOrEmpty( stringResult ) )
						    {
							    dbType = ( DbType ) Enum.Parse( typeof ( DbType ), stringResult );
						    }
					    }
				    }
			    }

			    if ( container == null )
			    {
				    container = new EntityTypeContainer( typeId );
				    Instance[ typeId ] = container;
			    }

			    container.DbType = dbType;
		    }

		    return container.DbType;
	    }

	    /// <summary>
        ///     Gets the type specified by the type id.
        /// </summary>
        /// <param name="typeId">The id.</param>
        /// <returns>
        ///     The type instance of the specified type id.
        /// </returns>
        public static Type GetType(long typeId)
        {
            EntityTypeContainer container;

            /////
            // Hit the cache.
            /////
            Instance.TryGetValue(typeId, out container);

            if (container == null || container.TypeMember == null)
            {
                string typeName = GetTypeName(typeId);

                /////
                // Use a type resolver to determine the type.
                /////
                Type type = Type.GetType(typeName, a => null, (a, s, b) =>
                {
                    string namespaceName = typeof(Entity).Namespace;

                    return Type.GetType(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", namespaceName, s.ToPascalCase()), false,
                                         true);
                }, false, false) ?? typeof(Entity);

                /////
                // If no type can be gleamed, use Entity.
                /////

                if (!Instance.TryGetValue(typeId, out container))
                {
                    container = new EntityTypeContainer(typeId);
                    Instance[typeId] = container;
                    container.TypeName = typeName;
                }

                container.Type = type;
                return type;
            }

            return container.Type;
        }

        /// <summary>
        ///     Gets the name of the type.
        /// </summary>
        /// <param name="typeId">The id.</param>
        /// <returns>
        ///     The type name of the specified type id.
        /// </returns>
        public static string GetTypeName(long typeId)
        {
            EntityTypeContainer container;

            /////
            // Hit the cache.
            /////
            Instance.TryGetValue(typeId, out container);

            if (container == null || container.TypeNameMember == null)
            {
                string typeName;

                using (DatabaseContext ctx = DatabaseContext.GetContext())
                {
                    using (IDbCommand command = ctx.CreateCommand())
                    {
                        /////
                        // TODO: Replace this with a stored procedure call.
                        /////
                        command.CommandText = @"-- EntityTypeCache.GetTypeName
						DECLARE @className BIGINT = dbo.fnAliasId( 'className', @tenantId )
						DECLARE @alias BIGINT = dbo.fnAliasId( 'alias', @tenantId )
						DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
						DECLARE @type BIGINT = dbo.fnAliasNsId( 'type', 'core', @tenantId )

						IF EXISTS (
							SELECT
								Data
							FROM
								dbo.Data_NVarChar
							WHERE
								FieldId = @className AND
								EntityId = @id AND
								TenantId = @tenantId
						)
							SELECT
								Data
							FROM
								dbo.Data_NVarChar
							WHERE
								FieldId = @className AND
								EntityId = @id AND
								TenantId = @tenantId
						ELSE
							IF EXISTS (
								SELECT
									Data
								FROM
									dbo.Data_Alias
								WHERE
									FieldId = @alias AND
									EntityId = @id AND
									TenantId = @tenantId
							)
								SELECT
									Data
								FROM
									dbo.Data_Alias
								WHERE
									FieldId = @alias AND
									EntityId = @id AND
									TenantId = @tenantId
							ELSE
								WITH Recursion ( FromId, ToId, Depth, Levels )
								AS
								(
									SELECT
										r.FromId, r.ToId, Depth = 1, Levels = CAST( ToId AS NVARCHAR( MAX ) )
									FROM
										dbo.Relationship r
									WHERE
										r.FromId <> r.ToId AND
										r.TypeId = @isOfType AND
										TenantId = @tenantId AND
										FromId = @id
			
									UNION ALL 
		
									SELECT
										rr.FromId, r.ToId, rr.Depth + 1, rr.Levels + ',' + CAST( r.ToId AS NVARCHAR( MAX ) )
									FROM
										Recursion rr
									JOIN
										dbo.Relationship r ON
											rr.ToId = r.FromId AND
											r.TenantId = @tenantId AND
											r.TypeId = @isOfType AND
										( ',' + rr.Levels + ',' NOT LIKE '%,' + CAST( r.ToId AS NVARCHAR( MAX ) ) + ',%' )
									WHERE
										r.FromId <> r.ToId AND
										r.ToId <> rr.ToId
								)
								SELECT
									'ENTITY'
								FROM
									Entity e
								JOIN
									Recursion r ON
										e.Id = r.FromId AND
										e.TenantId = @tenantId
								WHERE
									e.Id = @id AND
									e.TenantId = @tenantId AND
									r.ToId = @type";

                        ctx.AddParameter(command, "@id", DbType.Int64, typeId);
                        ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);

                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            typeName = (string)result;
                        }
                        else
                        {
                            throw new InvalidTypeException("Unable to resolve type id.");
                        }
                    }
                }

                /////
                // Cache this type.
                /////
                container = new EntityTypeContainer(typeId);
                Instance[typeId] = container;

                if (typeName.ToUpperInvariant() != typeof(Entity).Name.ToUpperInvariant())
                {
                    container.TypeName = typeName;
                }
                else
                {
                    /////
                    // Set the member directly so there is no generated secondary key relation.
                    /////
                    container.TypeNameMember = typeName;
                }
            }

            return container.TypeName;
        }

        /// <summary>
        ///     Determines whether the specified type id is literal.
        /// </summary>
        /// <param name="typeId">The type id.</param>
        /// <returns>
        ///     <c>true</c> if the specified type id is literal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLiteral(long typeId)
        {
            EntityTypeContainer container;

            /////
            // Hit the cache.
            /////
            Instance.TryGetValue(typeId, out container);

            if (container == null)
            {
                container = new EntityTypeContainer(typeId);
                Instance[typeId] = container;
            }

            return container.IsLiteralInner;
        }

        /// <summary>
        ///     Aggregates the types.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="field">The field.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="accumulator">The accumulator.</param>
        private static void AggregateTypes(IEntity entity, IEntityRef field, Direction direction, HashSet<long> accumulator)
        {
            /////
            // Get the entities specified field.
            /////
            IChangeTracker<IMutableIdKey> results = Entity.GetRelationships(entity, field, direction);

            if (results != null)
            {
                foreach (var result in results)
                {
                    /////
                    // Stop infinite recursion.
                    /////
                    if (!accumulator.Contains(result.Key))
                    {
                        /////
                        // Add the entity id to the accumulator.
                        /////
                        accumulator.Add(result.Key);

                        IEntity destination;

                        var localCache = Entity.GetLocalCache();

                        if (!localCache.TryGetValue(result.Key, out destination))
                        {
                            EntityCache.Instance.TryGetValue(result.Key, out destination);
                        }

                        if (destination == null)
                        {
                            destination = Entity.Get(result.Key);
                        }

                        if (destination != null)
                        {
                            /////
                            // Aggregate the results from the result.
                            /////
                            AggregateTypes(destination, field, direction, accumulator);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Entity Type Container
        /// </summary>
        public class EntityTypeContainer
        {
            /// <summary>
            ///     Thread synchronization.
            /// </summary>
            private readonly object _syncRoot = new object();

            /// <summary>
            ///     Assignable types.
            /// </summary>
            internal volatile HashSet<long> AssignableTypesMember = null;

            /// <summary>
            ///     DataTable.
            /// </summary>
            internal string DataTableNameMember = null;

            /// <summary>
            ///     Whether the data table name has been resolved or not.
            /// </summary>
            internal bool DataTableNameResolvedMember = false;

            /// <summary>
            ///     DataTable.
            /// </summary>
            internal DbType? DbTypeMember;

            /// <summary>
            ///     Whether the data table name has been resolved or not.
            /// </summary>
            internal bool DbTypeResolvedMember = false;

            /// <summary>
            ///     IsLiteral.
            /// </summary>
            internal bool? IsLiteralMember = null;

            /// <summary>
            ///     Type.
            /// </summary>
            internal volatile Type TypeMember = null;

            /// <summary>
            ///     Type name.
            /// </summary>
            internal volatile string TypeNameMember = null;

            /// <summary>
            ///     Initializes a new instance of the <see cref="EntityTypeContainer" /> class.
            /// </summary>
            /// <param name="id">The id.</param>
            public EntityTypeContainer(long id)
            {
                Id = id;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="EntityTypeContainer" /> class.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <param name="typeName">Name of the type.</param>
            public EntityTypeContainer(long id, string typeName)
                : this(id)
            {
                TypeNameMember = typeName;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="EntityTypeContainer" /> class.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <param name="typeName">Name of the type.</param>
            /// <param name="type">The type.</param>
            public EntityTypeContainer(long id, string typeName, Type type)
                : this(id, typeName)
            {
                TypeMember = type;
            }

            /// <summary>
            ///     Gets the assignable types.
            /// </summary>
            public HashSet<long> AssignableTypes
            {
                get
                {
                    if (AssignableTypesMember == null)
                    {
                        lock (_syncRoot)
                        {
                            if (AssignableTypesMember == null)
                            {
                                return AssignableTypesMember = GetAssignableTypes(Id.ToEnumerable());
                            }
                        }
                    }

                    return AssignableTypesMember;
                }
                set
                {
                    if (AssignableTypesMember != value)
                    {
                        lock (_syncRoot)
                        {
                            AssignableTypesMember = value;
                        }
                    }
                }
            }

            /// <summary>
            ///     Gets or sets the name of the data table.
            /// </summary>
            /// <value>
            ///     The name of the data table.
            /// </value>
            public string DataTableName
            {
                get
                {
                    if (DataTableNameMember == null && !DataTableNameResolvedMember)
                    {
                        lock (_syncRoot)
                        {
                            if (DataTableNameMember == null)
                            {
                                DataTableNameMember = GetDataTableName(Id);
                                DataTableNameResolvedMember = true;
                            }
                        }
                    }

                    return DataTableNameMember;
                }
                set
                {
                    if (DataTableNameMember != value)
                    {
                        lock (_syncRoot)
                        {
                            DataTableNameMember = value;
                        }
                    }

                    DataTableNameResolvedMember = true;
                }
            }

            /// <summary>
            ///     Gets or sets the type of the db.
            /// </summary>
            /// <value>
            ///     The type of the db.
            /// </value>
            public DbType? DbType
            {
                get
                {
                    if (DbTypeMember == null && !DbTypeResolvedMember)
                    {
                        lock (_syncRoot)
                        {
                            if (DbTypeMember == null)
                            {
                                DbTypeMember = GetDbType(Id);
                                DbTypeResolvedMember = true;
                            }
                        }
                    }

                    return DbTypeMember;
                }
                set
                {
                    if (DbTypeMember != value)
                    {
                        lock (_syncRoot)
                        {
                            DbTypeMember = value;
                        }
                    }

                    DbTypeResolvedMember = true;
                }
            }

            /// <summary>
            ///     Gets or sets the id.
            /// </summary>
            /// <value>
            ///     The id.
            /// </value>
            public long Id
            {
                get;
                set;
            }

            /// <summary>
            ///     Gets a value indicating whether this instance is literal.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is literal; otherwise, <c>false</c>.
            /// </value>
            public bool IsLiteralInner
            {
                get
                {
                    if (IsLiteralMember == null)
                    {
                        IsLiteralMember = !AssignableTypes.Contains( WellKnownAliases.CurrentTenant.Relationship );
                    }

                    return IsLiteralMember.Value;
                }
            }

            /// <summary>
            ///     Gets or sets the type.
            /// </summary>
            /// <value>
            ///     The type.
            /// </value>
            public Type Type
            {
                get
                {
                    if (TypeMember == null)
                    {
                        lock (_syncRoot)
                        {
                            if (TypeMember == null)
                            {
                                return TypeMember = EntityTypeCache.GetType(Id);
                            }
                        }
                    }

                    return TypeMember;
                }
                set
                {
                    if (TypeMember != value)
                    {
                        lock (_syncRoot)
                        {
                            TypeMember = value;
                        }
                    }
                }
            }

            /// <summary>
            ///     Gets or sets the name of the type.
            /// </summary>
            /// <value>
            ///     The name of the type.
            /// </value>
            public string TypeName
            {
                get
                {
                    if (TypeNameMember == null)
                    {
                        lock (_syncRoot)
                        {
                            if (TypeNameMember == null)
                            {
                                return TypeNameMember = GetTypeName(Id);
                            }
                        }
                    }

                    return TypeNameMember;
                }
                set
                {
                    if (value != null)
                    {
                        value = value.ToUpperInvariant();
                    }

                    if (TypeNameMember != value)
                    {
                        lock (_syncRoot)
                        {
                            if (TypeNameMember != null)
                            {
                                var existingKey = new EntityTypeTenantKey(TypeNameMember, RequestContext.TenantId);

                                /////
                                // Remove the relation from the existing type name.
                                /////
                                Instance.Unrelate(existingKey);
                            }
                        }

                        TypeNameMember = value;

                        var newKey = new EntityTypeTenantKey(TypeNameMember, RequestContext.TenantId);

                        /////
                        // Add a relation to the new type name.
                        /////
                        Instance.Relate(newKey, Id);
                    }
                }
            }
        }

        /// <summary>
        ///     Entity TypeName TenantId Key
        /// </summary>
        [Immutable]
        public class EntityTypeTenantKey
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="EntityTypeTenantKey" /> class.
            /// </summary>
            /// <param name="typeName">Name of the type.</param>
            /// <param name="tenantId">The tenant id.</param>
            public EntityTypeTenantKey(string typeName, long tenantId)
            {
                TypeName = typeName;
                TenantId = tenantId;
            }

            /// <summary>
            ///     Gets the tenant id.
            /// </summary>
            public long TenantId
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
            ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">
            ///     The <see cref="System.Object" /> to compare with this instance.
            /// </param>
            /// <returns>
            ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                var key = obj as EntityTypeTenantKey;

                if (key == null)
                {
                    return false;
                }

                return TypeName == key.TypeName && TenantId == key.TenantId;
            }

            /// <summary>
            ///     Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode()
            {
				unchecked
				{
					int hash = 17;

					if ( TypeName != null )
					{
						hash = hash * 92821 + TypeName.GetHashCode( );
					}

					hash = hash * 92821 + TenantId.GetHashCode( );

					return hash;
				}
            }

            /// <summary>
            ///     Implements the operator !=.
            /// </summary>
            /// <param name="a">A.</param>
            /// <param name="b">The b.</param>
            /// <returns>
            ///     The result of the operator.
            /// </returns>
            public static bool operator !=(EntityTypeTenantKey a, EntityTypeTenantKey b)
            {
                return !(a == b);
            }

            /// <summary>
            ///     Implements the operator ==.
            /// </summary>
            /// <param name="a">A.</param>
            /// <param name="b">The b.</param>
            /// <returns>
            ///     The result of the operator.
            /// </returns>
            public static bool operator ==(EntityTypeTenantKey a, EntityTypeTenantKey b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }

                return a.TypeName == b.TypeName && a.TenantId == b.TenantId;
            }
        }
    }
}