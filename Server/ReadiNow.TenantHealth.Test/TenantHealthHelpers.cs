// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Database;
using NUnit.Framework;
using ReadiNow.Annotations;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;

namespace ReadiNow.TenantHealth.Test
{
    /// <summary>
    /// Helper methods for performing Tenant Health Tests.
    /// </summary>
    static class TenantHealthHelpers
    {
        static Lazy<IReadOnlyList<TenantInfo>> tenants = new Lazy<IReadOnlyList<TenantInfo>>(GetTenantsImpl);

		/// <summary>
		/// One kb
		/// </summary>
		public const long OneKb = 1024;

		/// <summary>
		/// One Mb
		/// </summary>
		public const long OneMb = OneKb * 1024;

		/// <summary>
		/// One Gb
		/// </summary>
		public const long OneGb = OneMb * 1024;

		/// <summary>
		/// One Tb
		/// </summary>
		public const long OneTb = OneGb * 1024;

		/// <summary>
		/// Get a list of tenants.
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyList<TenantInfo> GetTenants( )
        {
            return tenants.Value;
        }

        /// <summary>
        /// Get a list of tenants.
        /// </summary>
        /// <returns></returns>
        private static IReadOnlyList<TenantInfo> GetTenantsImpl()
        {
            List<TenantInfo> result = new List<TenantInfo>( );

            using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
            using ( IDbCommand command = ctx.CreateCommand( ) )
            {
                command.CommandText = "select Id, name from _vTenant order by name";

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    while ( reader.Read( ) )
                    {
                        long tenantId = reader.GetInt64( 0 );
                        string tenantName = reader.GetString( 1 );
                        TenantInfo tenantInfo = new TenantInfo( tenantId, tenantName );
                        result.Add( tenantInfo );
                    }
                }
            }

            return result;

            // Alas .. this doesn't seem to work when called from inside a TestCaseSource
            //using ( new GlobalAdministratorContext( ) )
            //{
            //    var tenants = TenantHelper.GetAll( );
            //    var result = tenants.Select( tenant => new TenantInfo( tenant.Id, tenant.Name ) ).ToList( );
            //    return result;
            //}
        }

        /// <summary>
        /// Get a list of tenants.
        /// </summary>
        /// <param name="typeAliasWithNamespace">Alias (with namespace) of type to load instances for.</param>
        /// <param name="csvIgnoreByName">CSV list of the names of instances that should be flagged as 'ignore'.</param>
        /// <returns>A list of TestCaseData with three parameters set: 1. <see cref="TenantInfo"/>, 2. instance ID, 3. instance name</returns>
        public static IEnumerable<TestCaseData> GetInstancesAsTestData( string typeAliasWithNamespace, string csvIgnoreByName = null, string customSql = null )
        {
            var tenantList = GetTenants( );
            var tenantDict = tenantList.ToDictionary( t => t.TenantId );

            if ( tenantDict.Count == 0 )
                throw new Exception( "No tenants found" );

            ISet<string> ignoreNames = new HashSet<string>( ( csvIgnoreByName ?? "" ).Split( ',' ).Where( n => n != "" ) );

            List<TestCaseData> result = new List<TestCaseData>( );

            using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
            using ( IDbCommand command = ctx.CreateCommand( ) )
            {
                // For all tenants, get instances of the (exact) type
                string sql = customSql ?? @"
                    select isOfTypeRel.TenantId, isOfTypeRel.FromId, name.Data from Relationship isOfTypeRel
                    join Data_Alias isOfTypeAlias on isOfTypeRel.TypeId = isOfTypeAlias.EntityId and isOfTypeRel.TenantId = isOfTypeAlias.TenantId and isOfTypeAlias.Data='isOfType'
                    join Data_Alias typeAlias on isOfTypeRel.ToId = typeAlias.EntityId and isOfTypeRel.TenantId = typeAlias.TenantId and typeAlias.Data=@typeAlias and typeAlias.Namespace=@typeNs
                    join Data_Alias nameAlias on isOfTypeRel.TenantId = nameAlias.TenantId and nameAlias.Data='name'
                    left join Data_NVarChar name on isOfTypeRel.FromId = name.EntityId and isOfTypeRel.TenantId = name.TenantId and name.FieldId = nameAlias.EntityId
                    where isOfTypeRel.TenantId <> 0
                    order by isOfTypeRel.TenantId, name.Data
                    ";

                command.CommandText = sql;

                // Apply type name
                if ( typeAliasWithNamespace  != null )
                {
                    string [ ] parts = typeAliasWithNamespace.Split( ':' );
                    if ( parts.Length != 2 )
                        throw new Exception( "Expected full namespace" );
                    ctx.AddParameter( command, "@typeAlias", DbType.String, parts [ 1 ] );
                    ctx.AddParameter( command, "@typeNs", DbType.String, parts [ 0 ] );
                }

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    while ( reader.Read( ) )
                    {
                        long tenantId = reader.GetInt64( 0 );
                        long entityId = reader.GetInt64( 1 );
                        string entityName = reader.IsDBNull(2) ? "Unnamed" : reader.GetString( 2 );

                        TenantInfo tenantInfo;
                        if ( !tenantDict.TryGetValue( tenantId, out tenantInfo ) )
                        {
                            tenantInfo = new TenantInfo( tenantId, "Tenant" + tenantId );
                            tenantDict.Add( tenantId, tenantInfo );
                        }

                        // Create test data
                        TestCaseData testCaseData = new TestCaseData( tenantInfo, entityId, entityName );
                        if ( ignoreNames.Contains( entityName ) )
                            testCaseData = testCaseData.Ignore( "Ignored" );
                        testCaseData = testCaseData.SetCategory( tenantInfo.TenantName );

						if ( entityName == "Self Serve Component" || entityName == "Control on Form Except Screens" )
						{
							testCaseData = testCaseData.SetCategory( "ExtendedTests" );
						}

						result.Add( testCaseData );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get a list of tenants.
        /// </summary>
        /// <remarks>
        /// Pass in a callback that generates tenant data for a particular tenant.
        /// This will get called for each tenant, with the RequestContext already set for that tenant.
        /// The test data will then have the tenant pre-appended as the first parameter.
        /// </remarks>
        /// <returns>Test data with TenantInfo pre-appended.</returns>
        public static IEnumerable<TestCaseData> GetTestDataPerTenant( [NotNull] Func<IEnumerable<TestCaseData>> testDataCallback )
        {
            if ( testDataCallback == null )
                throw new ArgumentNullException( nameof( testDataCallback ) );

            var tenants = GetTenants( );

            // Process each tenant
            foreach (TenantInfo tenant in tenants)
            {
                IEnumerable<TestCaseData> testDataForTenant;

                // Get the test cases, in the context of the current tenant
                using ( tenant.GetSystemAdminContext( ) )
                {
                    testDataForTenant = testDataCallback( ).ToList( );
                }

                // Mutate the test data to include the tenant
                foreach (TestCaseData testData in testDataForTenant )
                {
                    List<object> arguments = testData.Arguments.ToList( );
                    arguments.Insert( 0, tenant );
                    object [ ] argArray = arguments.ToArray( );

                    TestCaseData newData = new TestCaseData( argArray );
                    if ( testData.TestName != null )
                        newData = newData.SetName( testData.TestName );

                    yield return newData;
                }
            }
        }

		/// <summary>
		/// To the size of the pretty.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="decimalPlaces">The decimal places.</param>
		/// <returns></returns>
		public static string ToPrettySize( int value, int decimalPlaces = 0 )
		{
			return ToPrettySize( ( long ) value, decimalPlaces );
		}

		/// <summary>
		/// To the size of the pretty.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="decimalPlaces">The decimal places.</param>
		/// <returns></returns>
		public static string ToPrettySize( long value, int decimalPlaces = 0 )
		{
			bool isNeg = value < 0;

			if ( isNeg )
			{
				value = -value;
			}
			var asTb = Math.Round( ( double ) value / OneTb, decimalPlaces );
			var asGb = Math.Round( ( double ) value / OneGb, decimalPlaces );
			var asMb = Math.Round( ( double ) value / OneMb, decimalPlaces );
			var asKb = Math.Round( ( double ) value / OneKb, decimalPlaces );
			string chosenValue = asTb > 1 ? $"{asTb}Tb" : asGb > 1 ? $"{asGb}Gb" : asMb > 1 ? $"{asMb}Mb" : asKb > 1 ? $"{asKb}Kb" : $"{Math.Round( ( double ) value, decimalPlaces )}B";

			return isNeg ? $"-{chosenValue}" : chosenValue;
		}

		/// <summary>
		/// Clears all tenant caches.
		/// </summary>
		public static void ClearAllCaches( )
		{
			/////
			// Use reflection so that the EDC.ReadiNow.Common assembly does not have to change. Dropping this assembly in place.
			/////
			typeof( TenantHelper ).InvokeMember( "InvalidateLocalProcessImpl", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, null, new object [ ] { RequestContext.TenantId } );
		}
	}
}
