// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Service;

namespace ReadiNow.Connector.Payload
{
    /// <summary>
    /// Factory for ReaderToEntityAdapter.
    /// </summary>
    /// <remarks>
    /// Note: the resulting ReaderToEntityAdapter(s) are intended to be cached locally. 
    /// </remarks>
    class ReaderToEntityAdapterProvider : IReaderToEntityAdapterProvider
    {
        private readonly IEntityRepository _entityRepository;
        private readonly IResourceResolverProvider _resourceResolverProvider;
        private readonly IEntityDefaultsDecoratorProvider _entityDefaultsDecoratorProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Loads the mapping configuration.</param>
        /// <param name="resourceResolverProvider">Looks up related entities.</param>
        /// <param name="entityDefaultsDecoratorProvider"></param>
        public ReaderToEntityAdapterProvider( [NotNull] IEntityRepository entityRepository, [NotNull] IResourceResolverProvider resourceResolverProvider, [CanBeNull] IEntityDefaultsDecoratorProvider entityDefaultsDecoratorProvider )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( "entityRepository" );
            if ( resourceResolverProvider == null )
                throw new ArgumentNullException( "resourceResolverProvider" );

            _entityRepository = entityRepository;
            _resourceResolverProvider = resourceResolverProvider;
            _entityDefaultsDecoratorProvider = entityDefaultsDecoratorProvider;
        }

        /// <summary>
        /// Creates an adapter that knows how to write objects into entities for a particular type of mapping.
        /// </summary>
        /// <param name="apiResourceMappingId">ID of the API mapping entity model that describes how to map object data to entities.</param>
        /// <param name="settings">Adapter settings.</param>
        public IReaderToEntityAdapter GetAdapter( long apiResourceMappingId, ReaderToEntityAdapterSettings settings )
        {
            // Note: we rely on the Api Key being bound to the endpoint.
            // No need to check specific access to the mapping and schema objects.
            // Access control will be applied when the adapter is being run (but not now while it is created).

            // Validate input
            if ( apiResourceMappingId <= 0 )
                throw new ArgumentOutOfRangeException( "apiResourceMappingId" );
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            // Get mapping descriptor
            ApiResourceMapping mapping = _entityRepository.Get<ApiResourceMapping>( apiResourceMappingId );
            if ( mapping == null )
                throw new ArgumentException( "apiResourceMappingId" );

            // Create member processors
            List<MemberProcessor> memberProcessors = CreateMemberProcessors( mapping, settings );

            // Factory function
            Func<IEntity> instanceFactory = CreateInstanceFactory( mapping, settings );

            // Set all defaults
            MemberProcessor defaultsProcessor = CreateDefaultValuesProcessor( mapping.MappedType.Id );

            return new ReaderToEntityAdapter( memberProcessors, instanceFactory, defaultsProcessor );
        }

        /// <summary>
        /// Create the member processor callbacks.
        /// </summary>
        private List<MemberProcessor> CreateMemberProcessors( ApiResourceMapping mapping, ReaderToEntityAdapterSettings settings )
        {
            var memberProcessors = new List<MemberProcessor>( );

            // Process mapped members
            // Note: we intentionally ignore unrecognised members.
            foreach ( ApiMemberMapping memberMappings in mapping.ResourceMemberMappings )
            {
                MemberProcessor memberProcessor;

                // Fill field
                ApiFieldMapping fieldMapping = memberMappings.As<ApiFieldMapping>( );
                if ( fieldMapping != null )
                {
                    memberProcessor = CreateFieldProcessor( fieldMapping, settings );
                    memberProcessors.Add( memberProcessor );
                    continue;
                }

                // Fill relationship
                ApiRelationshipMapping relMapping = memberMappings.As<ApiRelationshipMapping>( );
                if ( relMapping != null )
                {
                    memberProcessor = CreateRelationshipProcessor( relMapping, settings );
                    memberProcessors.Add( memberProcessor );
                    continue;
                }

                // Assert false
                throw new InvalidOperationException( "Unknown member mapping" );
            }

            return memberProcessors;
        }


        /// <summary>
        /// Create a delegate that will copy a single field member from a reader to an entity.
        /// </summary>
        private MemberProcessor CreateFieldProcessor( ApiFieldMapping mapping, ReaderToEntityAdapterSettings settings )
        {
            // Get field definition
            Field field = mapping.MappedField;
            if ( field == null )
            {
                throw new ConnectorConfigException( string.Format( Messages.FieldMappingHasNoField, mapping.Name ) );
            }
            bool memberRequired = mapping.ApiMemberIsRequired == true;
            bool mandatory = field.IsRequired == true || memberRequired;
            bool isBool = field.Is<BoolField>( );

            // Get mapping name
            string memberName = mapping.Name;
            string reportingName = GetReportingName( memberName, field.Name, settings );

            // Get value-getter
            Func<IObjectReader, object> getter = CreateValueGetter( memberName, field, settings );

            // Create processor that copies value.
            Action<IObjectReader, IEntity, IImportReporter> processor = ( reader, entity, reporter ) =>
            {
                // Handle missing fields
                bool hasKey = reader.HasKey( memberName );

                // Get value
                object value = null;
                try
                {
                    if ( hasKey )
                        value = getter( reader );
                    else if ( isBool && !memberRequired )
                        value = false;
                }
                catch ( FormatException )
                {
                    reporter.ReportError( reader, string.Format( Messages.PropertyWasFormattedIncorrectly, reportingName ) );
                    return;
                }
                catch ( InvalidCastException )
                {
                    reporter.ReportError( reader, string.Format( Messages.PropertyWasFormattedIncorrectly, reportingName ) );
                    return;
                }

                // Handle empty fields
                if ( mandatory && ( value == null || string.Empty.Equals( value ) ) )
                {
                    reporter.ReportError( reader, string.Format( Messages.MandatoryPropertyMissing, reportingName ) );
                    return;
                }

                // Set value
                entity.SetField( field, value );
            };
            return new MemberProcessor( processor );
        }


        /// <summary>
        /// Returns a delegate that will retrieve the desired value from the object reader.
        /// </summary>
        /// <param name="key">The member to read.</param>
        /// <param name="field">The expected field type.</param>
        /// <param name="settings">Additional behavior settings.</param>
        /// <returns>A callback that will retrive the relevant.</returns>
        private Func<IObjectReader, object> CreateValueGetter( string key, Field field, ReaderToEntityAdapterSettings settings )
        {
            IEntity nativeField = Entity.AsNative( field );
            Type fieldType = nativeField.GetType( );

            if ( fieldType == typeof( StringField ) || fieldType == typeof( AliasField ) || fieldType == typeof( XmlField ) )
            {
                StringField stringField = nativeField as StringField;
                bool multiLine = stringField != null && stringField.AllowMultiLines == true;
                if ( multiLine )
                {
                    return reader => StringExtensions.NormalizeForDatabase( reader.GetString( key ) );
                }
                else
                {
                    return reader => StringExtensions.NormalizeForSingleLine( reader.GetString( key ) );
                }

            }
            if ( fieldType == typeof( IntField ) || fieldType == typeof( AutoNumberField ) )
            {
                return reader => reader.GetInt( key );
            }
            if ( fieldType == typeof( BoolField ) )
            {
                return reader => reader.GetBoolean( key ) ?? false;
            }
            if ( fieldType == typeof( DecimalField ) || fieldType == typeof( CurrencyField ) )
            {
                return reader => reader.GetDecimal( key );
            }
            if ( fieldType == typeof( DateField ) )
            {
                return reader => reader.GetDate( key );
            }
            if ( fieldType == typeof( DateTimeField ) )
            {
                return reader => reader.GetDateTime( key );
            }
            if ( fieldType == typeof( TimeField ) )
            {
                return reader => reader.GetTime( key );
            }
            if ( fieldType == typeof( GuidField ) )
            {
                return reader => ParseGuid(reader.GetString(key));
            }

            throw new NotImplementedException( "Could not process mapping for field type " + fieldType.Name );
        }

        /// <summary>
        /// Parse a Guid.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>Nullable guid</returns>
        /// <exception cref="FormatException">Format exception.</exception>
        internal static Guid? ParseGuid( string value )
        {
            Guid? result;
            if ( string.IsNullOrEmpty(value) )
            {
                result = null;
            }
            else
            {
                if ( value.Length != 36 )
                    throw new FormatException( );

                Guid guidResult;
                if ( !Guid.TryParse( value, out guidResult ) )
                    throw new FormatException( );

                result = guidResult;
            }
            return result;
        }

        /// <summary>
        /// Create a delegate that will copy a single relationship member from a reader to an entity.
        /// </summary>
        private MemberProcessor CreateRelationshipProcessor( ApiRelationshipMapping mapping, ReaderToEntityAdapterSettings settings )
        {
            // Get relationship definition
            Relationship relationship = mapping.MappedRelationship;
            Direction dir = mapping.MapRelationshipInReverse == true ? Direction.Reverse : Direction.Forward;

            if ( relationship == null )
            {
                throw new ConnectorConfigException( string.Format( Messages.RelationshipMappingHasNoRelationship, mapping.Name ) );
            }

            // Determine if mandatory
            bool mandatory = mapping.ApiMemberIsRequired == true
                || (dir == Direction.Forward && relationship.RelationshipIsMandatory == true)
                || (dir == Direction.Reverse && relationship.RevRelationshipIsMandatory == true);

            // Get a resolver
            IResourceResolver resourceResolver = _resourceResolverProvider.GetResolverForRelationshipMapping( mapping );

            // Get mapping name
            string memberName = mapping.Name;
            string relName = ( dir == Direction.Forward ? relationship.ToName : relationship.FromName ) ?? relationship.Name;
            string reportingName = GetReportingName( memberName, relName, settings );

            // Support lookup on other fields
            Field lookupField = mapping.MappedRelationshipLookupField;

            // Create callback that can read the target identifier(s) out of an object reader.
            bool isLookup = relationship.IsLookup( dir );
            Func<IObjectReader, string, IReadOnlyCollection<object>> identityReader;

            if ( lookupField != null && lookupField.Is<IntField>( ) )
            {
                if ( isLookup )
                    identityReader = GetIntIdentityFromLookup;
                else
                    identityReader = GetIntIdentitiesFromRelationship;
            }
            else
            {
                if ( isLookup )
                    identityReader = GetStringIdentityFromLookup;
                else
                    identityReader = GetStringIdentitiesFromRelationship;
            }
            
            // Create processor that copies value.
            Action<IEnumerable<ReaderEntityPair>, IImportReporter> processor = (readerEntityPairs, reporter) => RelationshipProcessorImpl( readerEntityPairs, identityReader, resourceResolver, memberName, relationship.Id, dir, mandatory, reporter, reportingName );

            return new MemberProcessor( processor );
        }

        /// <summary>
        /// Run-time implementation for getting the identity string from a lookup property.
        /// </summary>
        private static IReadOnlyCollection<object> GetStringIdentityFromLookup( IObjectReader reader, string memberName )
        {
            string value = reader.GetString( memberName );
            if (string.IsNullOrEmpty(value))
                return new string[] { };
            return new [ ] { value };
        }

        /// <summary>
        /// Run-time implementation for getting the identity string from a lookup property.
        /// </summary>
        private static IReadOnlyCollection<object> GetIntIdentityFromLookup( IObjectReader reader, string memberName )
        {
            int? value = reader.GetInt( memberName );
            if ( value == null )
                return new string [ ] { };
            return new [ ] { (object)value.Value };
        }

        /// <summary>
        /// Run-time implementation for getting the identities string from a relationship property.
        /// </summary>
        private static IReadOnlyCollection<object> GetStringIdentitiesFromRelationship( IObjectReader reader, string memberName )
        {
            try
            {
                return reader.GetStringList( memberName );
            }
            catch ( InvalidCastException )
            {
                throw new ConnectorRequestException( Messages.ExpectedArrayOfIdentities );
            }
        }
        
        /// <summary>
        /// Run-time implementation for getting the identities string from a relationship property.
        /// </summary>
        private static IReadOnlyCollection<object> GetIntIdentitiesFromRelationship( IObjectReader reader, string memberName )
        {
            try
            {
                return reader.GetIntList( memberName )?.Select( i => ( object ) i ).ToList( );
            }
            catch ( InvalidCastException )
            {
                throw new ConnectorRequestException( Messages.ExpectedArrayOfIdentities );
            }
        }

        /// <summary>
        /// Run-time implementation for copying the relationship data from object readers to corresponding entities.
        /// </summary>
        /// <param name="readerEntityPairs">The readers and their corresponding entities.</param>
        /// <param name="identityReader">Callback for reading the identities from the readers.</param>
        /// <param name="resourceResolver">Service for resolving identity values.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="relationshipId">ID of relationship being updated.</param>
        /// <param name="direction">Direction that the relationship is being updated.</param>
        /// <param name="mandatory">Indicates that the member is mandatory.</param>
        /// <param name="reporter">Target for any errors.</param>
        /// <param name="reportingName">Member name used for reporting.</param>
        /// <exception cref="ConnectorRequestException">
        /// </exception>
        private void RelationshipProcessorImpl( IEnumerable<ReaderEntityPair> readerEntityPairs, Func<IObjectReader, string, IReadOnlyCollection<object>> identityReader, IResourceResolver resourceResolver, string memberName, long relationshipId, Direction direction, bool mandatory, IImportReporter reporter, string reportingName )
		{
		    if ( reporter == null )
		        throw new ArgumentNullException( nameof( reporter ) );

            // Get identifiers
            var identities = new HashSet<object>( );
            var entitiesToIdentities = new Dictionary<IEntity, IReadOnlyCollection<object>>( );
            foreach ( ReaderEntityPair pair in readerEntityPairs )
            {
                // Resolve targeted entity
                IReadOnlyCollection<object> targetResourceIdentities;
                IObjectReader reader = pair.ObjectReader;
                try
                {
                    targetResourceIdentities = identityReader( reader, memberName);

                    // Handle missing fields
                    if (mandatory && (!reader.HasKey(memberName) || targetResourceIdentities.Count == 0))
                    {
                        string message = string.Format( Messages.MandatoryPropertyMissing, reportingName );
                        reporter.ReportError( reader, message );
                        continue;
                    }
                }
                catch ( FormatException ) // hmm .. this is a bit specific to the JSON reader
                {
                    reporter.ReportError( reader, Messages.IdentifierListContainedNulls );
                    continue;
                }

                if ( targetResourceIdentities == null )
                    continue;

                foreach ( object identity in targetResourceIdentities )
                {
                    if ( identity == null || identity as string == string.Empty )
                        continue;
                    identities.Add( identity );
                }
                entitiesToIdentities.Add( pair.Entity, targetResourceIdentities );
            }

            // Resolve resources
            IDictionary<object, ResourceResolverEntry> resourceLookup = null;
            if ( identities.Count > 0 )
            {
                resourceLookup = resourceResolver.ResolveResources( identities.ToList() );
            }

            // Update relationships
            foreach ( ReaderEntityPair pair in readerEntityPairs )
            {
                IObjectReader reader = pair.ObjectReader;
                IEntity updateEntity = pair.Entity;
                IReadOnlyCollection<object> targetIdentities;
                if ( !entitiesToIdentities.TryGetValue( updateEntity, out targetIdentities ) )
                    continue;

                // Update relationship
                var col = updateEntity.GetRelationships( relationshipId, direction );

                // Clearing is mandatory for lookups;
                // mandatory for relationships on new instances with default values;
                // and a good idea for other relationships.
                // Clearing will reset any default instances, but we've decided this is sensible, otherwise a subsequent update using the
                // same data would just clear them anyway. Default values do not get applied for mapped columns.
                col.Clear( );

                // Find each target identity in the lookup
                foreach ( object targetIdentity in targetIdentities )
                {
                    if ( targetIdentity == null )
                        continue;

                    ResourceResolverEntry entry;
                    if ( resourceLookup == null || !resourceLookup.TryGetValue( targetIdentity, out entry ) )
                    {
                        reporter.ReportError( reader, Messages.ResourceNotFoundByField );
                        continue;
                    }
                    if ( entry.Error != ResourceResolverError.None )
                    {
                        string message = ResourceResolver.FormatResolverError( entry.Error, targetIdentity.ToString( ) );
                        reporter.ReportError( reader, message );
                        continue;
                    }

                    IEntity targetEntity = entry.Entity;

                    // And set into the relationship
                    if ( targetEntity != null )
                    {
                        col.Add( targetEntity );
                    }
                }
            }
        }

        /// <summary>
        /// Create a processor that sets default values.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        private MemberProcessor CreateDefaultValuesProcessor( long typeId )
        {
            if ( _entityDefaultsDecoratorProvider == null )
                return null;

            IEntityDefaultsDecorator defaultsDecorator = _entityDefaultsDecoratorProvider.GetDefaultsDecorator( typeId );

            Action<IEnumerable<ReaderEntityPair>, IImportReporter> processor = ( readerEntityPairs, reporter ) =>
            {
                defaultsDecorator.SetDefaultValues( readerEntityPairs.Select( pair => pair.Entity ) );
            };
            return new MemberProcessor( processor );
        }

        /// <summary>
        /// Create a factory that can create instances of the mapped type.
        /// </summary>
        /// <remarks>
        /// Note: at some point this will probably need to be extended to support derived types by inspecting field data.
        /// </remarks>
        private Func<IEntity> CreateInstanceFactory( ApiResourceMapping mapping, ReaderToEntityAdapterSettings settings )
        {
            // Get type
            EntityType type = mapping.MappedType;
            if ( type == null )
            {
                throw new ConnectorConfigException( Messages.ResourceMappingHasNoType );
            }

            // Create callback
            long typeId = type.Id;
            return ( ) =>
            {
                return _entityRepository.Create( typeId );
            };
        }

        /// <summary>
        /// Create a label suitable for describing a member.
        /// </summary>
        /// <param name="memberName">The source member name.</param>
        /// <param name="targetName">The target member name.</param>
        /// <param name="settings">Settings</param>
        /// <returns>The reporting label.</returns>
        private string GetReportingName( string memberName, string targetName, ReaderToEntityAdapterSettings settings )
        {
            if ( string.IsNullOrEmpty( memberName ) )
                return memberName;

            return settings.UseTargetMemberNameForReporting ? targetName : memberName;
        }
    }
}
