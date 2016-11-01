// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Test.Sql
{
	/// <summary>
	///     The TestData class.
	/// </summary>
	public abstract class TestData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TestData" /> class.
		/// </summary>
		protected TestData( )
		{
			SourceApp = new AppClass( );
			TargetApp = new AppClass( );
			Map = new MapClass( );
			Relationship = new RelationshipClass( );
			TypeA = new TypeClass( );
			TypeB = new TypeClass( );
			TypeAInstance = new TypeClass( );
			TypeBInstance = new TypeClass( );
		}

		/// <summary>
		///     Gets the source application.
		/// </summary>
		/// <value>
		///     The source application.
		/// </value>
		public AppClass SourceApp
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationship.
		/// </summary>
		/// <value>
		///     The relationship.
		/// </value>
		public RelationshipClass Relationship
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type a.
		/// </summary>
		/// <value>
		///     The type a.
		/// </value>
		public TypeClass TypeA
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type b.
		/// </summary>
		/// <value>
		///     The type b.
		/// </value>
		public TypeClass TypeB
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type a instance.
		/// </summary>
		/// <value>
		///     The type a instance.
		/// </value>
		public TypeClass TypeAInstance
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type b instance.
		/// </summary>
		/// <value>
		///     The type b instance.
		/// </value>
		public TypeClass TypeBInstance
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the maps.
		/// </summary>
		/// <value>
		///     The maps.
		/// </value>
		public MapClass Map
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [target is dependent on source].
		/// </summary>
		/// <value>
		///     <c>true</c> if [target is dependent on source]; otherwise, <c>false</c>.
		/// </value>
		public bool TargetAppIsDependentOnSource
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the target application.
		/// </summary>
		/// <value>
		///     The target application.
		/// </value>
		public AppClass TargetApp
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [type a instance_ type b instance_ relationship].
		/// </summary>
		/// <value>
		///     <c>true</c> if [type a instance_ type b instance_ relationship]; otherwise, <c>false</c>.
		/// </value>
		public bool TypeAInstanceToTypeBInstanceRelationship
		{
			get;
			set;
		}

		/// <summary>
		///     The Maps class.
		/// </summary>
		public class MapClass
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="MapClass" /> class.
			/// </summary>
			public MapClass( )
			{
				Forward = new Dictionary<EntityTypes, long>( );
				Reverse = new Dictionary<long, EntityTypes>( );
			}

			/// <summary>
			///     Gets the forward.
			/// </summary>
			/// <value>
			///     The forward.
			/// </value>
			public Dictionary<EntityTypes, long> Forward
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets the reverse.
			/// </summary>
			/// <value>
			///     The reverse.
			/// </value>
			public Dictionary<long, EntityTypes> Reverse
			{
				get;
				private set;
			}
		}

		/// <summary>
		///     The TypeClass class.
		/// </summary>
		public class TypeClass : BaseClass
		{
			/// <summary>
			///     Gets or sets a value indicating whether [explicitly in source application].
			/// </summary>
			/// <value>
			///     <c>true</c> if [explicitly in source application]; otherwise, <c>false</c>.
			/// </value>
			public bool ExplicitlyInSourceApp
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets a value indicating whether [explicitly in target application].
			/// </summary>
			/// <value>
			///     <c>true</c> if [explicitly in target application]; otherwise, <c>false</c>.
			/// </value>
			public bool ExplicitlyInTargetApp
			{
				get;
				set;
			}
		}

		/// <summary>
		///     The AppClass class.
		/// </summary>
		/// <seealso cref="TestData.BaseClass" />
		public class AppClass : BaseClass
		{
		}

		/// <summary>
		///     The BaseClass class.
		/// </summary>
		public class BaseClass
		{
			/// <summary>
			///     Gets or sets the name.
			/// </summary>
			/// <value>
			///     The name.
			/// </value>
			public string Name
			{
				get;
				set;
			}
		}

		/// <summary>
		///     The RelationshipClass class.
		/// </summary>
		/// <seealso cref="TestData.TypeClass" />
		public class RelationshipClass : TypeClass
		{
			/// <summary>
			///     Gets or sets the cardinality.
			/// </summary>
			/// <value>
			///     The cardinality.
			/// </value>
			public CardinalityEnum_Enumeration Cardinality
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets a value indicating whether [implicit in solution].
			/// </summary>
			/// <value>
			///     <c>true</c> if [implicit in solution]; otherwise, <c>false</c>.
			/// </value>
			public bool ImplicitInSolution
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets a value indicating whether [reverse implicit in solution].
			/// </summary>
			/// <value>
			///     <c>true</c> if [reverse implicit in solution]; otherwise, <c>false</c>.
			/// </value>
			public bool ReverseImplicitInSolution
			{
				get;
				set;
			}
		}
	}

	/// <summary>
	/// 	The EntityTestData class.
	/// </summary>
	/// <seealso cref="TestData" />
	public class EntityTestData : TestData
	{
		/// <summary>
		///     Gets or sets the result contains.
		/// </summary>
		/// <value>
		///     The result contains.
		/// </value>
		public EntityTypes ResultContains
		{
			get;
			set;
		}
	}

	/// <summary>
	/// 	The RelationshipTestData class.
	/// </summary>
	/// <seealso cref="TestData" />
	public class RelationshipTestData : TestData
	{
		/// <summary>
		/// Gets or sets the result contains.
		/// </summary>
		/// <value>
		/// The result contains.
		/// </value>
		public List<ExpectedRelationship> ResultContains
		{
			get;
			set;
		}
	}
}