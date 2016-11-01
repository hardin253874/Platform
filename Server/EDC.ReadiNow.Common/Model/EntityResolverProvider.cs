// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model.FieldTypes;
using EDC.ReadiNow.Security;
using EDC.Database;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// An interface for creating a service that finds entities by field value.
    /// </summary>
    class EntityResolverProvider : IEntityResolverProvider
    {
        private readonly IEntityRepository _entityRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Entity repository, used to look up field type.</param>
        public EntityResolverProvider( IEntityRepository entityRepository )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( "entityRepository" );
            _entityRepository = entityRepository;
        }


        /// <summary>
        /// Create a resolver to find entities with a field of a particular value.
        /// </summary>
        /// <param name="typeId">The type of resource to search.</param>
        /// <param name="fieldId">The field to search on.</param>
        /// <param name="secured">True if the generated resolver should be secured to the current user.</param>
        /// <returns>An IEntityResolver that can efficiently locate instances based on the field provided.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// typeId
        /// or
        /// fieldId
        /// </exception>
        public IEntityResolver GetResolverForField( long typeId, long fieldId, bool secured )
        {
            if ( typeId == 0 )
                throw new ArgumentNullException( "typeId" );
            if ( fieldId == 0 )
                throw new ArgumentNullException( "fieldId" );

            DataType fieldType;

            // Get field type
            using ( new SecurityBypassContext( ) )
            {
                IEntity field = _entityRepository.Get<Field>( fieldId );
                if (field == null )
                    throw new ArgumentException( "Specified field ID does not refer to a field.", "fieldId" );
                fieldType = FieldHelper.ConvertToDataType( field );
            }

            if (fieldType != DataType.String && fieldType != DataType.Int32 )
                throw new ArgumentException( "Entity resolver does not support looking up instances by fields of type " + fieldType, "fieldId" );

            // Build query
            var query = new StructuredQuery
            {
                RootEntity = new ResourceEntity( new EntityRef( typeId ) )
            };

            query.Conditions.Add(
                new QueryCondition
                {
                    Expression = new ResourceDataColumn( query.RootEntity, new EntityRef( fieldId ) ),
                    Operator = ConditionType.AnyOf,
                    Parameter = "@valueList"
                } );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    Expression = new Metadata.Query.Structured.IdExpression
                    {
                        NodeId = query.RootEntity.NodeId
                    }
                } );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    Expression = new Metadata.Query.Structured.ResourceDataColumn
                    {
                        NodeId = query.RootEntity.NodeId,
                        FieldId = new EntityRef( fieldId )
                    }
                } );


            // Get user
            long userId = RequestContext.GetContext( ).Identity.Id;

            // Get the SQL string.
            var settings = new QuerySqlBuilderSettings
            {
                SecureQuery = secured,
                Hint = "Entity.GetMatches",
                EmergencyDecorationCallback = queryRunSettings =>
                {
                    queryRunSettings.ValueList = new string[0]; //  fieldValues
                    EDC.ReadiNow.Diagnostics.EventLog.Application.WriteWarning( "EmergencyDecorationCallback was executed and data was not provided. Some entities may get incorrectly hidden by security." );
                }
            };
            QueryBuild queryResult = Factory.QuerySqlBuilder.BuildSql( query, settings );

            return new EntityResolver( queryResult, fieldType );
        }
    }
}
