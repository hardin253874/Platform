// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;

namespace EDC.ReadiNow.Security.AccessControl
{
	/// <summary>
	///     Create the report used for a new access rule based on the default display report
	///     for the given type and its ancestor types.
	/// </summary>
	public class AccessRuleDisplayReportFactory : IAccessRuleReportFactory
	{
		/// <summary>
		///     The default report name when the <see cref="EntityType" /> lacks a name.
		/// </summary>
		public static readonly string DefaultReportName = "Access Rule Report";

		/// <summary>
		///     Get the default display report for the given <see cref="EntityType" />.
		///     If there is no display report, do a breadth-first recursion through
		///     the inherited types to find a suitable display report.
		/// </summary>
		/// <param name="securableEntity">
		///     The type (or other <see cref="SecurableEntity" /> the report will be for.
		///     Note that the current implementation requires this to be an <see cref="EntityType" />.
		/// </param>
        /// <param name="structuredQuery">
        ///     An optional <see cref="StructuredQuery" /> to use for the report..
        /// </param>
		/// <returns>
		///     A <see cref="ReadiNow.Model.Report" /> or null, if not report is found.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="securableEntity" /> cannot be null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="securableEntity" /> must be an <see cref="EntityType" />.
		/// </exception>
		public Report GetDisplayReportForSecurableEntity( SecurableEntity securableEntity, StructuredQuery structuredQuery = null )
		{
			if ( securableEntity == null )
			{
				throw new ArgumentNullException( "securableEntity" );
			}
            // TODO: Test the RootEntity of structuredQuery to ensure it is the correct ype.

			var entityType = securableEntity.As<EntityType>( );
			if ( entityType == null )
			{
				throw new ArgumentException( @"securableEntity is not an EntityType", "securableEntity" );
			}

			// Place in separate, uniquely named folder to ensure (1) security reports
			// do not appear inthe UI elsewhere and (2) the report name resource key 
			// does not block the saving of the report.
			Folder folder = Model.Entity.Create<Folder>( );
			folder.Name = string.Format( "{0}{1}", Guid.NewGuid( ), DateTime.Now );
			folder.Save( );

			// The report creation code is hardly the most efficient but
			// it has been working reliably in automated tests. This operation
			// also occurs rarely.
		    if (structuredQuery == null)
		    {
                string typeName = entityType.Name ?? "Name";
                structuredQuery = CreateEntitiesQuery(entityType, typeName );
		    }
            Report result = ToReport(structuredQuery);

			result.Name = entityType.Name ?? DefaultReportName;
			result.Description = string.Empty;
			result.ResourceInFolder.Add(folder.As<NavContainer>( )); // For cascading deletes
			result.Save( );

			return result;
		}

		/// <summary>
		///     Create a structured query showing all entities
		/// </summary>
		/// <param name="type">
		///     The type to create a <see cref="StructuredQuery" /> for.
		/// </param>
		/// <param name="typeName">
		///		Name of the type.
		/// </param>
		/// <returns>
		///     A <see cref="StructuredQuery" /> showing the entities of the specified type.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="type" /> cannot be null.
		/// </exception>
		internal static StructuredQuery CreateEntitiesQuery( EntityRef type , string typeName)
		{
			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			Guid resourceGuid = Guid.NewGuid( );
			var structuredQuery = new StructuredQuery
			{
				RootEntity = new ResourceEntity
				{
					EntityTypeId = type,
					ExactType = false,
					NodeId = resourceGuid
				},
				SelectColumns = new List<SelectColumn>( )
			};

			structuredQuery.SelectColumns.Add(
				new SelectColumn
				{
					ColumnName = "Id",
					DisplayName = "Id",
					IsHidden = true,
					Expression = new IdExpression
					{
						NodeId = resourceGuid
					}
				} );
			structuredQuery.SelectColumns.Add(
				new SelectColumn
				{
					ColumnName = "Name",
					DisplayName = typeName,
					Expression = new ResourceDataColumn( structuredQuery.RootEntity, new EntityRef( "core:name" ) )
				} );
			structuredQuery.SelectColumns.Add(
				new SelectColumn
				{
					ColumnName = "Description",
					DisplayName = "Description",
					Expression = new ResourceDataColumn( structuredQuery.RootEntity, new EntityRef( "core:description" ) )
				} );

			return structuredQuery;
		}

		/// <summary>
		///     Convert a <see cref="StructuredQuery" /> to a <see cref="Report" />.
		/// </summary>
		/// <param name="structuredQuery">
		///     The <see cref="StructuredQuery" /> to convert. This cannot be null.
		/// </param>
		/// <returns>
		///     The converted report.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="structuredQuery" /> cannot be null.
		/// </exception>
		internal static Report ToReport( StructuredQuery structuredQuery )
		{
			if ( structuredQuery == null )
			{
				throw new ArgumentNullException( "structuredQuery" );
			}

		    Report report = ReportToEntityModelHelper.ConvertToEntity( structuredQuery );

			return report;
		}
	}
}