// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Test.Complete
{
	/// <summary>
	///     Complete Solution Details.
	/// </summary>
	public class CompleteSolutionDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CompleteSolutionDetails" /> class.
		/// </summary>
		public CompleteSolutionDetails( )
		{
			Relationships = new List<RelationshipDetails>( );
		}

		/// <summary>
		///     Gets or sets the name of the solution.
		/// </summary>
		/// <value>
		///     The name of the solution.
		/// </value>
		public string SolutionName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the solution.
		/// </summary>
		/// <value>
		///     The solution.
		/// </value>
		public BasicEntityDetails Solution
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity basic.
		/// </summary>
		/// <value>
		///     The entity basic.
		/// </value>
		public BasicEntityDetails EntityBasic
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity complex.
		/// </summary>
		/// <value>
		///     The entity complex.
		/// </value>
		public ComplexEntityDetails EntityComplex
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		public List<RelationshipDetails> Relationships
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">The cascade delete.</param>
		/// <param name="cascadeDeleteTo">The cascade delete to.</param>
		/// <param name="implicitInSolution">The implicit in solution.</param>
		/// <param name="reverseImplicitInSolution">The reverse implicit in solution.</param>
		/// <returns></returns>
		public RelationshipDetails GetRelationship( CardinalityEnum_Enumeration? cardinality = null, bool? cascadeDelete = null, bool? cascadeDeleteTo = null, bool? implicitInSolution = null, bool? reverseImplicitInSolution = null )
		{
			return Relationships.FirstOrDefault( relationship =>
                Check(cardinality, relationship.Cardinality) &&
                Check(cascadeDelete, relationship.CascadeDelete) &&
                Check(cascadeDeleteTo, relationship.CascadeDeleteTo) &&
                Check(implicitInSolution, relationship.ImplicitInSolution) &&
                Check(reverseImplicitInSolution, relationship.ReverseImplicitInSolution) );
		}

        /// <summary>
        /// Optionally compare two values.
        /// </summary>
        private static bool Check<T>(Nullable<T> expected, T actual) where T : struct
        {
            if (expected == null)
                return true;
            return object.Equals(expected.Value, actual);
        }

		/// <summary>
		///     Entity Details.
		/// </summary>
		public class BasicEntityDetails : Details
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="BasicEntityDetails" /> class.
			/// </summary>
			/// <param name="entity">The entity.</param>
			public BasicEntityDetails( IEntity entity ) : base( entity )
			{
				Type = new TypeDetails( entity.EntityTypes.First( ) as IEntity );
			}

			/// <summary>
			///     Gets or sets the type.
			/// </summary>
			/// <value>
			///     The type.
			/// </value>
			public TypeDetails Type
			{
				get;
				set;
			}
		}

		/// <summary>
		///     Complex Entity Details.
		/// </summary>
		public class ComplexEntityDetails : BasicEntityDetails
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="ComplexEntityDetails" /> class.
			/// </summary>
			/// <param name="entity">The entity.</param>
			public ComplexEntityDetails( Entity entity ) : base( entity )
			{
				Fields = new Dictionary<FieldType, ComplexEntityFieldDetails>( );
			}

			/// <summary>
			///     Gets or sets the fields.
			/// </summary>
			/// <value>
			///     The fields.
			/// </value>
			public Dictionary<FieldType, ComplexEntityFieldDetails> Fields
			{
				get;
				set;
			}
		}

		/// <summary>
		///     Complex Entity Field Details.
		/// </summary>
		public class ComplexEntityFieldDetails : BasicEntityDetails
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="ComplexEntityFieldDetails" /> class.
			/// </summary>
			/// <param name="entity">The entity.</param>
			/// <param name="value">The value.</param>
			public ComplexEntityFieldDetails( Entity entity, object value ) : base( entity )
			{
				InitialValue = value;
			}

			/// <summary>
			///     Gets or sets the initial value.
			/// </summary>
			/// <value>
			///     The initial value.
			/// </value>
			public object InitialValue
			{
				get;
				set;
			}
		}

		/// <summary>
		///     Details.
		/// </summary>
		public class Details
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="Details" /> class.
			/// </summary>
			/// <param name="entity">The entity.</param>
			public Details( IEntity entity )
			{
				UpgradeId = entity.UpgradeId;
				Id = entity.Id;
			}

			/// <summary>
			///     Gets or sets the upgrade identifier.
			/// </summary>
			/// <value>
			///     The upgrade identifier.
			/// </value>
			public Guid UpgradeId
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the identifier.
			/// </summary>
			/// <value>
			///     The identifier.
			/// </value>
			public long Id
			{
				get;
				set;
			}
		}

		/// <summary>
		///     Relationship details.
		/// </summary>
		public class RelationshipDetails : Details
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="RelationshipDetails" /> class.
			/// </summary>
			/// <param name="relationship">The relationship.</param>
			/// <param name="fromInstance">From instance.</param>
			/// <param name="toInstance">To instance.</param>
			/// <param name="cardinality">The cardinality.</param>
			/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
			/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
			/// <param name="implicitInSolution">if set to <c>true</c> [implicit in solution].</param>
			/// <param name="reverseImplicitInSolution">if set to <c>true</c> [reverse implicit in solution].</param>
			public RelationshipDetails( Relationship relationship, Entity fromInstance, Entity toInstance, CardinalityEnum_Enumeration cardinality, bool cascadeDelete, bool cascadeDeleteTo, bool implicitInSolution, bool reverseImplicitInSolution ) : base( relationship )
			{
				FromType = new BasicEntityDetails( relationship.FromType );
				ToType = new BasicEntityDetails( relationship.ToType );

				FromInstance = new BasicEntityDetails( fromInstance );
				ToInstance = new BasicEntityDetails( toInstance );

				Cardinality = cardinality;
				CascadeDelete = cascadeDelete;
				CascadeDeleteTo = cascadeDeleteTo;
				ImplicitInSolution = implicitInSolution;
				ReverseImplicitInSolution = reverseImplicitInSolution;
			}

			/// <summary>
			///     Gets or sets from instance.
			/// </summary>
			/// <value>
			///     From instance.
			/// </value>
			public BasicEntityDetails FromInstance
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets to instance.
			/// </summary>
			/// <value>
			///     To instance.
			/// </value>
			public BasicEntityDetails ToInstance
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets from type.
			/// </summary>
			/// <value>
			///     From type.
			/// </value>
			public BasicEntityDetails FromType
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets to type.
			/// </summary>
			/// <value>
			///     To type.
			/// </value>
			public BasicEntityDetails ToType
			{
				get;
				set;
			}

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
			///     Gets or sets a value indicating whether this relationship cascade deletes.
			/// </summary>
			/// <value>
			///     <c>true</c> if this relationship cascade deletes; otherwise, <c>false</c>.
			/// </value>
			public bool CascadeDelete
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets a value indicating whether this relationship cascade deletes in the reverse direction.
			/// </summary>
			/// <value>
			///     <c>true</c> if this relationship cascade deletes in the reverse direction; otherwise, <c>false</c>.
			/// </value>
			public bool CascadeDeleteTo
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets a value indicating whether this relationship implicitly includes entities in the solution.
			/// </summary>
			/// <value>
			///     <c>true</c> if this relationship implicitly includes entities in the solution; otherwise, <c>false</c>.
			/// </value>
			public bool ImplicitInSolution
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets a value indicating whether this relationship implicitly includes entities in the reverse direction in
			///     the solution.
			/// </summary>
			/// <value>
			///     <c>true</c> if this relationship implicitly includes entities in the reverse direction in the solution; otherwise,
			///     <c>false</c>.
			/// </value>
			public bool ReverseImplicitInSolution
			{
				get;
				set;
			}
		}

		/// <summary>
		///     Type Details.
		/// </summary>
		public class TypeDetails : Details
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="TypeDetails" /> class.
			/// </summary>
			/// <param name="entity">The entity.</param>
			public TypeDetails( IEntity entity ) : base( entity )
			{
			}
		}
	}
}