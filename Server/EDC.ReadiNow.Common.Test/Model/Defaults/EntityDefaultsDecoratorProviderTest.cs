// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;
using ReadiNow.Common;

namespace EDC.ReadiNow.Test.Model.Defaults
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    class EntityDefaultsDecoratorProviderTest
    {
        public readonly DateTime MockToday = new DateTime( 2016, 05, 01, 0, 0, 0, DateTimeKind.Local );
        public readonly DateTime MockNow = new DateTime( 2016, 05, 01, 5, 0, 0, DateTimeKind.Local );
        public readonly DateTime MockUtcNow = new DateTime( 2016, 04, 30, 23, 0, 0, DateTimeKind.Utc );


        EntityDefaultsDecoratorProvider EntityDefaultsDecoratorProvider
        {
            get
            {
                IEntityRepository repos = Factory.Current.ResolveNamed<IEntityRepository>( "Graph" );
                IDateTime mockTime = new MockDateTime
                {
                    Now = MockNow,
                    UtcNow = MockUtcNow
                };
                return new EntityDefaultsDecoratorProvider( repos, mockTime );
            }
        }

        [ Test]
        [RunAsDefaultTenant]
        public void Test_DecoratorThrowsNull( )
        {
            EntityType type = new EntityType( );
            type.Save( );

            var decorator = EntityDefaultsDecoratorProvider.GetDefaultsDecorator( type.Id );
            Assert.Throws<ArgumentNullException>( ()=> decorator.SetDefaultValues( null ) );
        }

        [ Test]
        [RunAsDefaultTenant]
        public void Test_RunsWithEmpty( )
        {
            EntityType type = new EntityType( );
            type.Save( );

            IEntity entity = CreateInstance( type );
            var decorator = ApplyDefaults( type, entity );
            Assert.That( decorator, Is.InstanceOf<NoopEntityDefaultsDecorator>( ) );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_StringField_DefaultValue( bool hasExistingValue )
        {
            Test_Field<StringField, string>( hasExistingValue, "default", "default", "existing" );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_IntField_DefaultValue( bool hasExistingValue )
        {
            Test_Field<IntField, int>( hasExistingValue, "2", 2, 3 );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_DecimalField_DefaultValue( bool hasExistingValue )
        {
            Test_Field<DecimalField, decimal>( hasExistingValue, "2.2", 2.2M, 3.3M );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_CurrencyField_DefaultValue( bool hasExistingValue )
        {
            Test_Field<CurrencyField, decimal>( hasExistingValue, "2.2", 2.2M, 3.3M );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_DateTimeField_DefaultValue( bool hasExistingValue )
        {
            DateTime defDate = new DateTime( 2012, 03, 12, 6, 0, 0, DateTimeKind.Utc );
            DateTime altDate = new DateTime( 2012, 04, 14, 9, 0, 0, DateTimeKind.Utc );
            DateTime value = Test_Field<DateTimeField, DateTime>( hasExistingValue, "2012-03-12T06:00:00Z", defDate, altDate );
            Assert.That( value.Kind, Is.EqualTo( DateTimeKind.Utc ) );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_DateField_DefaultValue( bool hasExistingValue )
        {
            DateTime defDate = new DateTime( 2012, 03, 12, 0, 0, 0, DateTimeKind.Utc );
            DateTime altDate = new DateTime( 2012, 04, 14, 0, 0, 0, DateTimeKind.Utc );
            DateTime value = Test_Field<DateField, DateTime>( hasExistingValue, "2012-03-12T00:00:00.000Z", defDate, altDate );
            Assert.That( value.Kind, Is.EqualTo( DateTimeKind.Utc ) );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_TimeField_DefaultValue( bool hasExistingValue )
        {
            DateTime defDate = new DateTime( 1753, 01, 01, 5, 0, 0, DateTimeKind.Utc );
            DateTime altDate = new DateTime( 1753, 01, 01, 13, 0, 0, DateTimeKind.Utc );
            DateTime value = Test_Field<TimeField, DateTime>( hasExistingValue, "5:00 AM", defDate, altDate );  // WHY is the form builder storing default times like this??
            Assert.That( value.Kind, Is.EqualTo( DateTimeKind.Utc ) );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_BoolField_DefaultValue_True( bool hasExistingValue )
        {
            Test_Field<BoolField, bool>( hasExistingValue, "True", true, false );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_BoolField_DefaultValue_False( bool hasExistingValue )
        {
            Test_Field<BoolField, bool>( hasExistingValue, "False", false, true );
        }

        private TData Test_Field<TField, TData>( bool hasExistingValue, string defaultString, TData defaultValue, TData existingValue  ) where TField : IEntity, new()
        {
            // Setup
            Field field = new TField( ).As<Field>( );
            field.DefaultValue = defaultString;

            EntityType type = new EntityType
            {
                Fields = { field }
            };
            type.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            if ( hasExistingValue )
                entity.SetField( field.Id, existingValue );
            ApplyDefaults( type, entity );

            // Check
            TData fieldValue = entity.GetField<TData>( field.Id );

            TData expected = hasExistingValue ? existingValue : defaultValue;
            Assert.That( fieldValue, Is.EqualTo( expected ) );

            return fieldValue;
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_Relationship_ToTypeDefaultValue( bool hasExistingValue )
        {
            // Setup
            EntityType targetType = new EntityType( );
            targetType.Save( );

            IEntity defInstance = EntityRepository.Create( targetType.Id );
            defInstance.Save( );

            IEntity anotherInstance = EntityRepository.Create( targetType.Id );
            defInstance.Save( );

            Relationship rel = new Relationship
            {
                ToType = targetType,
                ToTypeDefaultValue = defInstance.As<Resource>( ),
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne
            };

            EntityType type = new EntityType
            {
                Relationships = { rel }
            };
            type.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            if ( hasExistingValue )
                entity.GetRelationships( rel.Id, Direction.Forward ).Add( anotherInstance );
            ApplyDefaults( type, entity );

            // Check
            var relValues = entity.GetRelationships( rel.Id, Direction.Forward );
            var relValue = relValues.SingleOrDefault( );

            long expected = hasExistingValue ? anotherInstance.Id : defInstance.Id;
            Assert.That( relValue, Is.Not.Null );
            Assert.That( relValue.Id, Is.EqualTo( expected ) );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void Test_Relationship_FromTypeDefaultValue( bool hasExistingValue )
        {
            // Setup
            EntityType targetType = new EntityType( );
            targetType.Save( );

            IEntity defInstance = EntityRepository.Create( targetType.Id );
            defInstance.Save( );

            IEntity anotherInstance = EntityRepository.Create( targetType.Id );
            defInstance.Save( );

            Relationship rel = new Relationship
            {
                FromType = targetType,
                FromTypeDefaultValue = defInstance.As<Resource>( ),
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne
            };

            EntityType type = new EntityType
            {
                ReverseRelationships = { rel }
            };
            type.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            if ( hasExistingValue )
                entity.GetRelationships( rel.Id, Direction.Reverse ).Add( anotherInstance );
            ApplyDefaults( type, entity );

            // Check
            var relValues = entity.GetRelationships( rel.Id, Direction.Reverse );
            var relValue = relValues.SingleOrDefault( );

            long expected = hasExistingValue ? anotherInstance.Id : defInstance.Id;
            Assert.That( relValue, Is.Not.Null );
            Assert.That( relValue.Id, Is.EqualTo( expected ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Relationship_DefaultToUseCurrentUser( )
        {
            // Setup
            Relationship rel = new Relationship
            {
                ToType = UserAccount.UserAccount_Type,
                DefaultToUseCurrent = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne
            };

            EntityType type = new EntityType
            {
                Relationships = { rel }
            };
            type.Save( );
            
            UserAccount userAccount = new UserAccount( );
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            using ( new SetUser( userAccount ) )
            {
                ApplyDefaults( type, entity );
            }

            // Check
            var relValues = entity.GetRelationships( rel.Id, Direction.Forward );
            var relValue = relValues.SingleOrDefault( );
            Assert.That( relValue, Is.Not.Null );
            long expectedId = userAccount.Id;
            Assert.That( relValue.Id, Is.EqualTo( expectedId ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Relationship_DefaultFromUseCurrentUser( )
        {
            // Setup
            Relationship rel = new Relationship
            {
                FromType = UserAccount.UserAccount_Type,
                DefaultFromUseCurrent = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne
            };

            EntityType type = new EntityType
            {
                ReverseRelationships = { rel }
            };
            type.Save( );

            UserAccount userAccount = new UserAccount( );
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            using ( new SetUser( userAccount ) )
            {
                ApplyDefaults( type, entity );
            }

            // Check
            var relValues = entity.GetRelationships( rel.Id, Direction.Reverse );
            var relValue = relValues.SingleOrDefault( );
            Assert.That( relValue, Is.Not.Null );
            long expectedId = userAccount.Id;
            Assert.That( relValue.Id, Is.EqualTo( expectedId ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Relationship_DefaultToUseCurrentPerson( )
        {
            // Setup
            Relationship rel = new Relationship
            {
                ToType = Person.Person_Type,
                DefaultToUseCurrent = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne
            };

            EntityType type = new EntityType
            {
                Relationships = { rel }
            };
            type.Save( );

            Person person = new Person( );
            UserAccount userAccount = new UserAccount( );
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.AccountHolder = person;
            userAccount.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            using ( new SetUser( userAccount ) )
            {
                ApplyDefaults( type, entity );
            }

            // Check
            var relValues = entity.GetRelationships( rel.Id, Direction.Forward );
            var relValue = relValues.SingleOrDefault( );
            Assert.That( relValue, Is.Not.Null );

            long expectedId = person.Id;
            Assert.That( relValue.Id, Is.EqualTo( expectedId ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Relationship_DefaultFromUseCurrentPerson_Derived( )
        {
            // Setup
            EntityType derivedType = new EntityType
            {
                Inherits = { Person.Person_Type }                
            };
            Relationship rel = new Relationship
            {
                FromType = derivedType,
                DefaultFromUseCurrent = true,
                Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne
            };

            EntityType type = new EntityType
            {
                ReverseRelationships = { rel }
            };
            type.Save( );

            Person person = new Person( );
            UserAccount userAccount = new UserAccount( );
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.AccountHolder = person;
            userAccount.Save( );

            // Run
            IEntity entity = CreateInstance( type );
            using ( new SetUser( userAccount ) )
            {
                ApplyDefaults( type, entity );
            }

            // Check
            var relValues = entity.GetRelationships( rel.Id, Direction.Reverse );
            var relValue = relValues.SingleOrDefault( );
            Assert.That( relValue, Is.Not.Null );
            long expectedId = person.Id;
            Assert.That( relValue.Id, Is.EqualTo( expectedId ) );
        }

        [TestCase( "Test" )]
        [TestCase( "NOW" )]
        [TestCase( "TODAY" )]
        [RunAsDefaultTenant]
        public void Test_GetDefaultValueGetter_String_Now( string defaultValue )
        {
            Field field = new StringField
            {
                DefaultValue = defaultValue
            }.As<Field>( );

            Func<object> callback = EntityDefaultsDecoratorProvider.GetDefaultValueGetter( field );
            object result = callback( );
            Assert.That( result, Is.EqualTo( defaultValue ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDefaultValueGetter_DateTime_Now( )
        {
            Field field = new DateTimeField
            {
                DefaultValue = "NOW"
            }.As<Field>( );

            Func<object> callback = EntityDefaultsDecoratorProvider.GetDefaultValueGetter( field );
            object result = callback( );
            Assert.That( result, Is.EqualTo( MockUtcNow ) ); // no time zone
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDefaultValueGetter_DateTime_Today( )
        {
            Field field = new DateTimeField
            {
                DefaultValue = "TODAY"
            }.As<Field>( );

            Func<object> callback = EntityDefaultsDecoratorProvider.GetDefaultValueGetter( field );
            object result = callback( );
            Assert.That( result, Is.EqualTo( MockUtcNow.Date ) ); // no time zone
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDefaultValueGetter_Date_Today( )
        {
            Field field = new DateTimeField
            {
                DefaultValue = "TODAY"
            }.As<Field>( );

            Func<object> callback = EntityDefaultsDecoratorProvider.GetDefaultValueGetter( field );
            object result = callback( );
            Assert.That( result, Is.EqualTo( MockUtcNow.Date ) ); // No time zone
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDefaultValueGetter_String( )
        {
            Field field = new StringField
            {
                DefaultValue = "Test"
            }.As<Field>( );

            Func<object> callback = EntityDefaultsDecoratorProvider.GetDefaultValueGetter( field );
            object result = callback( );
            Assert.That( result, Is.EqualTo( "Test" ) );
        }

        private IEntityRepository EntityRepository => Factory.EntityRepository;

        private IEntity CreateInstance( EntityType entityType )
        {
            return EntityRepository.Create( entityType.Id );
        }

        private IEntityDefaultsDecorator ApplyDefaults( EntityType entityType, IEntity entity )
        {
            var provider = Factory.Current.Resolve<IEntityDefaultsDecoratorProvider>( );
            var decorator = provider.GetDefaultsDecorator( entityType.Id );
            decorator.SetDefaultValues( entity.ToEnumerable( ) );
            return decorator;
        }
    }
}
