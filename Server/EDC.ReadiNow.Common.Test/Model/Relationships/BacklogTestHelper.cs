// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Model.Relationships
{
	internal static class BacklogTestHelper
	{
		public static EntityType CreateDefinition( string name )
		{
			var sourceDefn = new EntityType
				{
					Name = name
				};
			//add a string field to the definition
			var strField = new StringField
				{
					Name = "String field"
				};
			strField.Save( );
			//// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( strField.As<Field>( ) );
			sourceDefn.Save( );

			return sourceDefn;
		}

		public static Relationship CreateRelationship( EntityType sourceDefn, EntityType targetDefn, string name,
		                                               string cardinality, bool cascade, bool cascadeTo )
		{
			var sourceToTargetRelationship = new Relationship
				{
					FromType = sourceDefn,
					ToType = targetDefn,
					Name = name,
					Description = "Unit test relationship",
					Cardinality = Entity.Get<CardinalityEnum>( new EntityRef( "core", cardinality ) ),
					CascadeDelete = cascade,
					CascadeDeleteTo = cascadeTo
				};

			// get the cardinality 'relationship' - do not edit existing or create new
			sourceToTargetRelationship.Save( );

			return sourceToTargetRelationship;
		}

		/// <summary>
		///     load an array with definition id's from the entity list
		/// </summary>
		public static long[] LoadIdArray( List<Entity> entList )
		{
			int loop = 0;
			var idList = new long[entList.Count( )];
			//load id array
			foreach ( Entity eType in entList )
			{
				idList[ loop ] = eType.Id;
				loop++;
			}
			return idList;
		}

		// end loadIdArray

		/// <summary>
		///     load an array with definition id's from the relationship list
		/// </summary>
		public static long[] LoadIdArray( List<Relationship> relList )
		{
			int loop = 0;
			var idList = new long[relList.Count( )];
			//load id array
			foreach ( Relationship rType in relList )
			{
				idList[ loop ] = rType.Id;
				loop++;
			}
			return idList;
		}

		// end loadIdArray

		/// <summary>
		///     load an array with definition id's from the definition list
		/// </summary>
		public static long[] LoadIdArray( List<EntityType> defList )
		{
			int loop = 0;
			var idList = new long[defList.Count( )];
			//load id array
			foreach ( EntityType eType in defList )
			{
				idList[ loop ] = eType.Id;
				loop++;
			}
			return idList;
		}

		// end loadIdArray
	}
}