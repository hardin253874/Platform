// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.Database.Types;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.FieldTypes;
using EDC.ReadiNow.Utc;
using ReadiNow.Common;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Creates a EntityDefaultsDecorator, which is able to set default values for entities of a particular type.
    /// </summary>
    public class EntityDefaultsDecoratorProvider : IEntityDefaultsDecoratorProvider
    {
        const string TypeRequest =
            @"
            let @FIELD = {
                defaultValue
            }
            let @FWDRELATIONSHIP = { 
                toTypeDefaultValue.id,
                defaultToUseCurrent
            }
            let @REVRELATIONSHIP = { 
                fromTypeDefaultValue.id,
                defaultFromUseCurrent
            }
            let @TYPE = {
                isOfType.id,
                inherits.@TYPE,
                fields.@FIELD,
                relationships.@FWDRELATIONSHIP,
                reverseRelationships.@REVRELATIONSHIP
            }
            @TYPE";

        private IEntityRepository _entityRepository;

        private IDateTime _dateTimeProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Repository for loading type data.</param>
        public EntityDefaultsDecoratorProvider( IEntityRepository entityRepository, IDateTime dateTimeProvider )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( "entityRepository" );
            if ( dateTimeProvider == null )
                throw new ArgumentNullException( "dateTimeProvider" );

            _entityRepository = entityRepository;
            _dateTimeProvider = dateTimeProvider;
        }


        /// <summary>
        /// Creates a IEntityDefaultsDecorator for a particular type.
        /// </summary>
        /// <param name="typeId">The type.</param>
        /// <returns>A default-value decorator.</returns>
        public IEntityDefaultsDecorator GetDefaultsDecorator( long typeId )
        {
            EntityType entityType = _entityRepository.Get<EntityType>( typeId, TypeRequest );
            if ( entityType == null )
                throw new Exception( "Entity type could not be loaded." );

            IReadOnlyCollection<EntityType> allTypes = EntityTypeHelper.GetAncestorsAndSelf( entityType ).ToList( );

            var forwardRelActions = allTypes.SelectMany( type => type.Relationships ).Select( GetActionForForwardRelationship );
            var reverseRelActions = allTypes.SelectMany( type => type.ReverseRelationships ).Select( GetActionForReverseRelationship );
            var fieldActions = allTypes.SelectMany( type => type.Fields ).Select( GetActionForField );

            IReadOnlyCollection<Action<IEnumerable<IEntity>>> allActions;
            allActions = forwardRelActions.Concat( reverseRelActions ).Concat( fieldActions ).WhereNotNull( ).ToList( );

            if ( allActions.Count == 0 )
                return NoopEntityDefaultsDecorator.Instance;

            return new PrebuiltEntityDefaultsDecorator( allActions );
        }


        /// <summary>
        /// Create an action that applies a default value to a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The action, or null if there is no default.</returns>
        private Action<IEnumerable<IEntity>> GetActionForField( Field field )
        {
            if ( field == null )
                return null;

            Func<object> defaultGetter = GetDefaultValueGetter( field );
            if ( defaultGetter == null )
                return null;

            long fieldId = field.Id;
            Action<IEnumerable<IEntity>> action = entities =>
            {
                object defaultValue = defaultGetter( );

                foreach ( IEntity entity in entities )
                {
                    object value = entity.GetField( fieldId );
                    if ( value == null )
                    {
                        entity.SetField( fieldId, defaultValue );
                    }
                }
            };

            return action;
        }

        internal Func<object> GetDefaultValueGetter( Field field )
        {
            string defaultValue = field.DefaultValue;
            if ( defaultValue == null )
                return null;

            // Handle default "NOW"
            if ( defaultValue == DateTimeType.DefaultValueNow )
            {
                if ( field.Is<DateTimeField>( ) )
                    return ( ) => _dateTimeProvider.UtcNow;
            }

            // Handle default "TODAY"
            if ( defaultValue == DateType.DefaultValueToday )
            {
                bool isDateTimeField = field.Is<DateTimeField>( );
                bool isDateField = !isDateTimeField && field.Is<DateField>( );

                if ( isDateField || isDateTimeField )
                {
                    // If no timezone in context, use UTC today.
                    TimeZoneInfo tzi = RequestContext.GetContext( ).TimeZoneInfo;
                    if ( tzi == null )
                        return ( ) => _dateTimeProvider.UtcNow.Date;

                    if ( isDateTimeField )
                    {
                        // Return a UTC value that refers to midnight of today in hte curre Give the UTC datetime for midnight in the current timezone
                        return ( ) =>
                        {
                            DateTime localNow = TimeZoneHelper.ConvertToLocalTimeTZ( _dateTimeProvider.UtcNow, tzi );
                            DateTime localToday = localNow.Date;
                            DateTime utcDefault = TimeZoneHelper.ConvertToUtcTZ( localToday, tzi );
                            return utcDefault;
                        };
                    }
                    if ( isDateField )
                    {
                        // Return a value with a date component equal to today in the current timezone, and time of 00:00:00
                        return ( ) =>
                        {
                            DateTime localNow = TimeZoneHelper.ConvertToLocalTimeTZ( _dateTimeProvider.UtcNow, tzi );
                            DateTime localToday = localNow.Date;
                            return localToday;
                        };
                    }
                }
            }

            // Get default value
            Func<string, object> converter = FieldHelper.GetConverterFunction( field );
            object nativeDefaultValue = converter( defaultValue );

            if ( nativeDefaultValue is DateTime )
            {
                nativeDefaultValue = DateTime.SpecifyKind( ( DateTime ) nativeDefaultValue, DateTimeKind.Utc );
            }

            return ( ) => nativeDefaultValue;
        }


        /// <summary>
        /// Create an action that applies a default value to a forward relationship.
        /// </summary>
        /// <param name="rel">The field.</param>
        /// <returns>The action, or null if there is no default.</returns>
        private Action<IEnumerable<IEntity>> GetActionForForwardRelationship( Relationship rel )
        {
            if ( rel == null )
                return null;

            bool defaultUseCurrentUser = rel.DefaultToUseCurrent == true;
            bool defaultUseCurrentPerson = defaultUseCurrentUser && UsePersonInsteadOfUser( rel.ToType );

            IEntity defaultValue = rel.ToTypeDefaultValue;
            if ( defaultValue == null && !defaultUseCurrentUser )
                return null;

            long relId = rel.Id;
            Action<IEnumerable<IEntity>> action = entities =>
            {
                SetDefaultForRelationship( entities, relId, Direction.Forward, defaultValue, defaultUseCurrentUser, defaultUseCurrentPerson );
            };
            return action;
        }


        /// <summary>
        /// Create an action that applies a default value to a reverse relationship.
        /// </summary>
        /// <param name="rel">The field.</param>
        /// <returns>The action, or null if there is no default.</returns>
        private Action<IEnumerable<IEntity>> GetActionForReverseRelationship( Relationship rel )
        {
            if ( rel == null )
                return null;

            bool defaultUseCurrentUser = rel.DefaultFromUseCurrent == true;
            bool defaultUseCurrentPerson = defaultUseCurrentUser && UsePersonInsteadOfUser( rel.FromType );

            IEntity defaultValue = rel.FromTypeDefaultValue;
            if ( defaultValue == null && !defaultUseCurrentUser )
                return null;

            long relId = rel.Id;
            Action<IEnumerable<IEntity>> action = entities =>
            {
                SetDefaultForRelationship( entities, relId, Direction.Reverse, defaultValue, defaultUseCurrentUser, defaultUseCurrentPerson );
            };
            return action;
        }

        /// <summary>
        /// Determine if 'use current user by default' refers to user account or person.
        /// </summary>
        /// <param name="entityType">The type of entity that the result is to be stored into.</param>
        private bool UsePersonInsteadOfUser( EntityType entityType )
        {
            if ( entityType.Id == UserAccount.UserAccount_Type.Id )
                return false;
            return true; // assume person, unless relationship explicitly targets user account.
        }


        /// <summary>
        /// Implementation for relationship actions.
        /// </summary>
        /// <param name="entities">Entities to set.</param>
        /// <param name="relId">ID of relationship to update.</param>
        /// <param name="direction">Direction of relationship.</param>
        /// <param name="defaultValue">Proposed default value.</param>
        /// <param name="defaultToCurrentUser">True if the current user should be the default value.</param>
        /// <param name="defaultUseCurrentPerson">True if the account holder of the user should be the default value.</param>
        private void SetDefaultForRelationship( IEnumerable<IEntity> entities, long relId, Direction direction, IEntity defaultValue, bool defaultToCurrentUser, bool defaultUseCurrentPerson )
        {
            if ( defaultToCurrentUser || defaultUseCurrentPerson )
                defaultValue = GetCurrentUser( defaultUseCurrentPerson );

            foreach ( IEntity entity in entities )
            {
                IEntityRelationshipCollection<IEntity> relValues = entity.GetRelationships( relId, direction );
                if ( relValues.Count == 0 )
                {
                    relValues.Add( defaultValue );
                }
            }
        }


        /// <summary>
        /// Get the current user account.
        /// </summary>
        /// <returns></returns>
        private IEntity GetCurrentUser( bool useCurrentPerson )
        {
            long accountId = RequestContext.GetContext( ).Identity.Id;
            UserAccount userAccount = Entity.Get<UserAccount>( accountId );

            if ( userAccount == null )
                return null;

            IEntity result = useCurrentPerson ? (IEntity)userAccount.AccountHolder : userAccount;
            return result;
        }

    }
}
