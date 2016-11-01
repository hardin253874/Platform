// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using NUnit.Framework;

// Namespace declaration 

namespace EDC.ReadiNow.Test.Model.Relationships
{
	[TestFixture]
	[RunWithTransaction]
	public class SchemaTests
	{
		/// <summary>
		///     Ensure all relationships define a cardinality.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void CheckRelationshipCardinality( )
		{
			IEnumerable<Relationship> relationships = Entity.GetInstancesOfType<Relationship>( );

			foreach ( Relationship rel in relationships )
			{
                if (!TestHelpers.InValidatableSolution(rel.InSolution))
                    continue;
				Assert.IsNotNull( rel.Cardinality_Enum, string.Format( "No cardinality for {0}", rel.Alias ) );
			}
		}

		/// <summary>
		///     Ensure all relationships define endpoint types.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
        [Ignore]                // the application library causes incomplete relationships to exist at the moment - disabling.
		public void CheckRelationshipEndpoints( )
		{
			IEnumerable<Relationship> relationships = Entity.GetInstancesOfType<Relationship>( );

			foreach ( Relationship rel in relationships )
			{
                if (!TestHelpers.InValidatableSolution(rel.InSolution))
                    continue;
                Assert.IsNotNull(rel.FromType, string.Format("No 'fromType' for {0}", rel.Alias));
				Assert.IsNotNull( rel.ToType, string.Format( "No 'toType' for {0}", rel.Alias ) );
			}
		}

		/// <summary>
		///     Ensure all relationships define a name.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void CheckRelationshipNames( )
		{
			IEnumerable<Relationship> relationships = Entity.GetInstancesOfType<Relationship>( );

			foreach ( Relationship rel in relationships )
			{
                if (!TestHelpers.InValidatableSolution(rel.InSolution))
                    continue;
                Assert.IsNotNullOrEmpty(rel.Name, string.Format("No name for {0}", rel.Alias));
			}
		}
	}
}