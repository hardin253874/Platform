// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.EditForm
{
	/// <summary>
	/// </summary>
	[TestFixture]
	public class InstanceControllerTests
	{
		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetInstance_ExistingDefaultForm_ByAlias( )
		{
			using (
				var request = new PlatformHttpRequest( @"data/v1/instance/test/af01" ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
			}
		}

		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetInstance_ExistingDefualtForm_ById( )
		{
			var aaemployee = Entity.Get( new EntityRef( "test", "aaSteveGibbon" ) );

			using (
				var request = new PlatformHttpRequest( string.Format( @"data/v1/instance/{0}", aaemployee.Id ) ) )
			{
				HttpWebResponse response = request.GetResponse( );

				// check that it worked (200)
				Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
			}
		}

		/// <summary>
		///     Tests the get image.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetInstance_NoDefaultForm( )
		{
			EntityType myDef = null;

			try
			{
				var personType = CodeNameResolver.GetTypeByName( "AA_Person" ).As<EntityType>( );
				myDef = new EntityType( );
				myDef.Inherits.Add( personType );
				myDef.Save( );

				IEntity myInst = Entity.Create( myDef.Id );
				myInst.Save( );

				using (
					var request = new PlatformHttpRequest( string.Format( @"data/v1/instance/{0}", myInst.Id ) ) )
				{
					HttpWebResponse response = request.GetResponse( );

					// check that it worked (200)
					Assert.IsTrue( response.StatusCode == HttpStatusCode.OK );
				}
			}
			finally
			{
				try
				{
					if ( myDef != null )
					{
						myDef.Delete( );
					}
				}
				catch ( Exception exc )
				{
					EventLog.Application.WriteError( exc.ToString( ) );
				}
			}
		}
	}
}