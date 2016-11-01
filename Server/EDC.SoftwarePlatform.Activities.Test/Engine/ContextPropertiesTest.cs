// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
	[TestFixture]
	public class ContextPropertiesTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void EnsureNestedPropertiesWork( )
		{
			string key = "some property";

			var outerProperty = new ContextProperties( );

			outerProperty.Add( key, "original" );
			Assert.AreSame( "original", outerProperty.Find( key ), "Check set get" );

			ContextProperties innerProperty = outerProperty.CreateChildContext( );
			Assert.AreSame( "original", innerProperty.Find( key ), "Check set at parent get at child" );

			innerProperty.Add( key, "childValue" );
			Assert.AreSame( "childValue", innerProperty.Find( key ), "Check set at child overrides parent" );
			Assert.AreNotSame( "childValue", outerProperty.Find( key ), "Check setting at child does not override parent" );
		}
	}
}