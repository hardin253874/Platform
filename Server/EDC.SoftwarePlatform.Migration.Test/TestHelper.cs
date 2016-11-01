// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Test
{
	/// <summary>
	///     Test Helper class.
	/// </summary>
	public static class TestHelper
	{
		/// <summary>
		///     The default solution name
		/// </summary>
		public const string DefaultSolutionName = "Migration Testing Solution";

		/// <summary>
		///     The default solution description
		/// </summary>
		public const string DefaultSolutionDescription = "Solution for testing migration paths.";

		/// <summary>
		///     The default tenant name
		/// </summary>
		public const string DefaultTenantName = "Test Tenant";

		/// <summary>
		///     The default tenant description
		/// </summary>
		public const string DefaultTenantDescription = "Tenant used for testing.";

		/// <summary>
		///     The default instance name
		/// </summary>
		public const string DefaultInstanceName = "Test Instance";

		/// <summary>
		///     The default instance description
		/// </summary>
		public const string DefaultInstanceDescription = "Instance used for testing.";

		/// <summary>
		///     The default type name
		/// </summary>
		public const string DefaultTypeName = "Test Type";

		/// <summary>
		///     The default entity name
		/// </summary>
		public const string DefaultEntityName = "Test Entity";

		/// <summary>
		///     The default type description
		/// </summary>
		public const string DefaultTypeDescription = "Type used for testing.";

		/// <summary>
		///     The default entity description
		/// </summary>
		public const string DefaultEntityDescription = "Entity used for testing.";

		/// <summary>
		///     The field types
		/// </summary>
		private static readonly IEnumerable<Type> FieldTypesMember = new[ ]
		{
			typeof ( AliasField ), typeof ( AutoNumberField ), typeof ( BoolField ), typeof ( CurrencyField ), typeof ( DateField ), typeof ( DateTimeField ), typeof ( DecimalField ), typeof ( GuidField ), typeof ( IntField ), typeof ( StringField ), typeof ( TimeField ), typeof ( XmlField )
		};

		/// <summary>
		///     Gets the field types.
		/// </summary>
		/// <value>
		///     The field types.
		/// </value>
		public static IEnumerable<Type> FieldTypes
		{
			get
			{
				return FieldTypesMember;
			}
		}


		/// <summary>
		///     Creates the type of the entity.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="inherits">The inherits.</param>
		/// <param name="inSolution">The information solution.</param>
		/// <returns></returns>
		public static EntityType CreateEntityType( string name = DefaultTypeName, string description = DefaultTypeDescription, EntityType inherits = null, Solution inSolution = null )
		{
			var entityType = Entity.Create<EntityType>( );

			if ( name != null )
			{
				entityType.Name = name;
			}

			if ( description != null )
			{
				entityType.Description = description;
			}

			if ( inherits != null )
			{
				entityType.Inherits.Add( inherits );
			}

			if ( inSolution != null )
			{
				entityType.InSolution = inSolution;
			}

			return entityType;
		}

		/// <summary>
		///     Creates the entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="inSolution">The in solution.</param>
		/// <returns></returns>
		public static T CreateEntity<T>( string name = DefaultEntityName, string description = DefaultEntityDescription, Solution inSolution = null ) where T : class, IEntity
		{
			var entity = Entity.Create<T>( );

			if ( name != null )
			{
				entity.SetField( "core:name", name );
			}

			if ( description != null )
			{
				entity.SetField( "core:description", description );
			}

			if ( inSolution != null )
			{
				entity.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
				{
					inSolution
				} );
			}

			return entity;
		}

		public static IEntity CreateEntity( long typeId, string name = DefaultEntityName, string description = DefaultEntityDescription, Solution inSolution = null )
		{
			var entity = Entity.Create( new[ ]
			{
				typeId
			} );

			if ( name != null )
			{
				entity.SetField( "core:name", name );
			}

			if ( description != null )
			{
				entity.SetField( "core:description", description );
			}

			if ( inSolution != null )
			{
				entity.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
				{
					inSolution
				} );
			}

			return entity;
		}

		/// <summary>
		///     Creates the field.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="inSolution">The in solution.</param>
		/// <returns></returns>
		/// <exception cref="InvalidTypeException">Invalid generic type.</exception>
		public static T CreateField<T>( string name = DefaultEntityName, string description = DefaultEntityDescription, Solution inSolution = null ) where T : class, IEntity
		{
			Type genericType = typeof ( T );

			if ( FieldTypes.All( t => t != genericType ) )
			{
				throw new InvalidTypeException( "Invalid generic type." );
			}

			var entity = Entity.Create<T>( );

			if ( name != null )
			{
				entity.SetField( "core:name", name );
			}

			if ( description != null )
			{
				entity.SetField( "core:description", description );
			}

			if ( inSolution != null )
			{
				entity.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
				{
					inSolution
				} );
			}

			return entity;
		}

		/// <summary>
		///     Creates the instance.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="inSolution">The information solution.</param>
		/// <returns></returns>
		public static IEntity CreateInstance( EntityType type, string name = DefaultInstanceName, string description = DefaultInstanceDescription, Solution inSolution = null )
		{
			return CreateInstance( type.Id, name, description, inSolution );
		}

		/// <summary>
		///     Creates the instance.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="inSolution">The in solution.</param>
		/// <returns></returns>
		public static IEntity CreateInstance( long type, string name = DefaultInstanceName, string description = DefaultInstanceDescription, Solution inSolution = null )
		{
			IEntity instance = Entity.Create( type );

			if ( name != null )
			{
				instance.SetField( "core:name", name );
			}

			if ( description != null )
			{
				instance.SetField( "core:description", description );
			}

			if ( inSolution != null )
			{
				instance.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
				{
					inSolution
				} );
			}

			return instance;
		}

		/// <summary>
		///     Creates the relationship.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="fromType">From type.</param>
		/// <param name="toType">Type of the automatic.</param>
		/// <param name="inSolution">The information solution.</param>
		/// <param name="implicitInSolution">
		///     if set to <c>true</c> [implicit information solution].
		/// </param>
		/// <param name="reverseImplicitInSolution">
		///     if set to <c>true</c> [reverse implicit information solution].
		/// </param>
		/// <returns></returns>
		public static Relationship CreateRelationship( string name = null, string description = null, EntityType fromType = null, EntityType toType = null, Solution inSolution = null, bool implicitInSolution = false, bool reverseImplicitInSolution = false )
		{
			var relationship = Entity.Create<Relationship>( );

			if ( name != null )
			{
				relationship.Name = name;
			}

			if ( description != null )
			{
				relationship.Description = description;
			}

			if ( fromType != null )
			{
				relationship.FromType = fromType;
			}

			if ( toType != null )
			{
				relationship.ToType = toType;
			}

			if ( inSolution != null )
			{
				relationship.InSolution = inSolution;
			}

			if ( implicitInSolution )
			{
				relationship.ImplicitInSolution = true;
			}

			if ( reverseImplicitInSolution )
			{
				relationship.ReverseImplicitInSolution = true;
			}

			return relationship;
		}

		/// <summary>
		///     Creates the report.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <returns></returns>
		public static Report CreateReport( string name, string description )
		{
			var report = new Report
			{
				Name = name,
				Description = description
			};

			return report;
		}

		/// <summary>
		///     Creates the solution.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		public static Solution CreateSolution( string name = DefaultSolutionName, string description = DefaultSolutionDescription, string version = "1.0" )
		{
			var solution = new Solution
			{
				Name = name,
				Description = description,
				SolutionVersionString = version
			};

			return solution;
		}

		/// <summary>
		///     Creates the tenant.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <returns></returns>
		public static long CreateTenant( string name = DefaultTenantName, string description = DefaultTenantDescription )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				var tenant = Entity.Create<Tenant>( );
				tenant.Name = name;
				tenant.Description = description;
				tenant.Save( );

				return tenant.Id;
			}
		}

		/// <summary>
		///     Populates the basic solution.
		/// </summary>
		/// <param name="solution">The solution.</param>
		/// <exception cref="System.ArgumentNullException">solution</exception>
		public static List<IEntity> PopulateBasicSolution( Solution solution )
		{
			if ( solution == null )
			{
				throw new ArgumentNullException( "solution" );
			}

			var entities = new List<IEntity>( );

			EntityType entityType1 = CreateEntityType( "Dummy Type 1", "My Dummy Type 1", null, solution );
			entityType1.Save( );

			entities.Add( entityType1 );

			EntityType entityType2 = CreateEntityType( "Dummy Type 2", "My Dummy Type 2", null, solution );
			entityType2.Save( );

			entities.Add( entityType2 );

			/////
			// Create an instance of Dummy Type 1
			/////
			IEntity instance1 = Entity.Create( entityType1 );

			instance1.SetField( "core:name", "Dummy Instance 1" );
			instance1.SetField( "core:description", "My Dummy Instance 1" );

			instance1.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
			{
				solution
			} );

			instance1.Save( );

			entities.Add( instance1 );

			/////
			// Create an instance of Dummy Type 2
			/////
			IEntity instance2 = Entity.Create( entityType2 );

			instance2.SetField( "core:name", "Dummy Instance 2" );
			instance2.SetField( "core:description", "My Dummy Instance 2" );

			instance2.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
			{
				solution
			} );

			instance2.Save( );

			entities.Add( instance2 );

			return entities;
		}
	}
}