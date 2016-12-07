// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	[RunWithTransaction]
	public class FieldTypeTests
	{        
		[Test]
		[RunAsDefaultTenant]
        public void RegexDoesNotCheckEmptyStrings()
		{
			// Ensure that regex is not applied for empty string, if IsRequired is not set.

		    StringField field = new StringField();
		    field.Name = "f1";
            field.IsRequired = true;
		    field.Pattern = Entity.Get<StringPattern>("emailPattern");
            field.IsRequired = false;

            EntityType type = new EntityType();
		    type.Name = "t1";
            type.Fields.Add(field.As<Field>());
		    type.Save();
            
		    var e = new Entity(type.Id);
		    e.SetField(field, "");
			e.Save( );

			e.Delete( );
		    field.Delete();
		    type.Delete();
		}

		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ValidationException ) )]
		public void RegexPatternChecksInvalid( )
		{
            StringField field = new StringField();
            field.Name = "f1";
            field.IsRequired = true;
            field.Pattern = Entity.Get<StringPattern>("emailPattern");
            field.IsRequired = false;

            EntityType type = new EntityType();
            type.Name = "t1";
            type.Fields.Add(field.As<Field>());
            type.Save();

            var e = new Entity(type.Id);
		    try
		    {
		        e.SetField(field, "blah!!!");
		        e.Save();
		    }
		    finally
		    {
		        e.Delete();
		        field.Delete();
		        type.Delete();
		    }
		}

		[Test]
		[RunAsDefaultTenant]
        public void RegexPatternChecksValid()
		{
            StringField field = new StringField();
            field.Name = "f1";
            field.IsRequired = true;
            field.Pattern = Entity.Get<StringPattern>("emailPattern");
            field.IsRequired = false;

            EntityType type = new EntityType();
            type.Name = "t1";
            type.Fields.Add(field.As<Field>());
            type.Save();

            var e = new Entity(type.Id);
            e.SetField(field, "valid@somewhere.com");
            e.Save();

            e.Delete();
            field.Delete();
            type.Delete();
		}



        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [ExpectedException(typeof(ValidationException))]
        public void StringFieldMaxLengthNotOver10k()
        {
            var tooLargeStringLength = ReadiNow.Model.FieldTypes.StringFieldHelper.RealMaximumStringFieldLength + 1;

            StringField field = new StringField();
            field.Name = "f1";
            field.IsRequired = true;
            field.Pattern = Entity.Get<StringPattern>("emailPattern");
            field.IsRequired = false;
            field.MaxLength = tooLargeStringLength;

            field.Save();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [ExpectedException(typeof(ValidationException))]
        public void StringFieldNotOver10k()
        {
            var tooLargeStringLength = ReadiNow.Model.FieldTypes.StringFieldHelper.RealMaximumStringFieldLength + 1;

            // Ensure that regex is not applied for empty string, if IsRequired is not set.

            StringField field = new StringField();
            field.Name = "f1";
            field.IsRequired = true;
            field.Pattern = Entity.Get<StringPattern>("emailPattern");
            field.IsRequired = false;

            EntityType type = new EntityType();
            type.Name = "t1";
            type.Fields.Add(field.As<Field>());
            type.Save();

            var e = new Entity(type.Id);
            e.SetField(field, new string('#', tooLargeStringLength));
            e.Save();

        }

        [Test]
		[RunAsDefaultTenant]
		public void EnsureTypeHasFields( )
		{
			// Ensure that regex is not applied for empty string, if IsRequired is not set.

			IEntity e = Entity.Get( new EntityRef( "test", "employee" ) );
			IEntityRelationshipCollection<IEntity> coll = e.GetRelationships( EntityType.Fields_Field, Direction.Reverse );
			Assert.IsTrue( coll.Count > 0 );
		}

		[Test]
		[RunAsDefaultTenant]
		public void FieldCastingTest_NameEqualToMaxLength( )
		{
			var sf = Entity.Get<StringField>( new EntityRef( "core", "name" ) );
			Assert.IsTrue( sf.MaxLength > 0, "Check that resource name has a max length" );

			if ( sf.MaxLength != null )
			{
				var p = new Person
					{
						Name = new string( 'z', sf.MaxLength.Value )
					};
				p.Save( );

				p.Delete( );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ValidationException ) )]
		public void FieldCastingTest_NameOverMaxLength( )
		{
			var sf = Entity.Get<StringField>( new EntityRef( "core", "name" ) );
			Assert.IsTrue( sf.MaxLength > 0, "Check that resource name has a max length" );

			var p = new Person( );

			try
			{
				if ( sf.MaxLength != null )
				{
					p.Name = new string( 'z', sf.MaxLength.Value + 1 );
				}
				p.Save( );
			}
			finally
			{
				p.Delete( );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void FieldCastingTest_StringMaxLength( )
		{
			var sf = new StringField
				{
					MaxLength = 5
				};

			var f = sf.As<Field>( );
			string res1 = f.ValidateFieldValue( "hello world" );
			Assert.IsNotNull( res1, "Test when too long" );

			string res2 = f.ValidateFieldValue( "hello" );
			Assert.IsNull( res2, "Test exact length" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetAllFieldTypes( )
		{
			IEnumerable<FieldType> fieldTypes = Entity.GetInstancesOfType<FieldType>( );

			string found = string.Join( ", ", fieldTypes.Select( field => field.Alias ).OrderBy( x => x ) );
            const string expected = "core:aliasField, core:autoNumberField, core:boolField, core:currencyField, core:dateField, core:dateTimeField, core:decimalField, core:field, core:guidField, core:intField, core:stringField, core:timeField, core:xmlField";

            Assert.AreEqual( expected, found );
		}

		[Test]
		[RunAsDefaultTenant]
		public void StringField_Regex( )
		{
			var sf = new StringField
				{
					Pattern = Entity.Get<StringPattern>( "core:emailPattern" )
				};

			var f = sf.As<Field>( );
			string res1 = f.ValidateFieldValue( "blah" );
			Assert.IsNotNull( res1, "Test invalid email" );

			string res2 = f.ValidateFieldValue( "valid@somewhere.com" );
			Assert.IsNull( res2, "Test valid email" );
		}

		/// <summary>
		/// Tests the AutoNumber field functionality.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void AutoNumberField( )
		{
			var field = new AutoNumberField( );
			field.Name = "TestAutoNumberField";

			var definition = new Definition( );
			definition.Name = "TestAutoNumberDefinition";
			definition.Fields.Add( field.As<Field>( ) );
			definition.Save( );

			Entity instance1 = new Entity( definition.Id );
			instance1.Save( );
            int? value = instance1.GetField<int?>(field);
			Assert.AreEqual( 1, value );

			Entity instance2 = new Entity( definition.Id );
			instance2.Save( );
            value = instance2.GetField<int?>(field);
			Assert.AreEqual( 2, value );

			Entity instance3 = new Entity( definition.Id );
			instance3.Save( );
            value = instance3.GetField<int?>(field);
			Assert.AreEqual( 3, value );
		}

		/// <summary>
		/// Tests the AutoNumber field seed functionality.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
        public void AutoNumberField_Seed()
		{
			var field = new AutoNumberField( );
			field.Name = "TestAutoNumberField";
			field.AutoNumberSeed = 123;

			var definition = new Definition( );
			definition.Name = "TestAutoNumberDefinition";
			definition.Fields.Add( field.As<Field>( ) );
			definition.Save( );

			Entity instance1 = new Entity( definition.Id );
			instance1.Save( );
			int value = instance1.GetField<int>( field );
			Assert.AreEqual( 123, value );

			Entity instance2 = new Entity( definition.Id );
			instance2.Save( );
			value = instance2.GetField<int>( field );
			Assert.AreEqual( 124, value );

			Entity instance3 = new Entity( definition.Id );
			instance3.Save( );
			value = instance3.GetField<int>( field );
			Assert.AreEqual( 125, value );
		}

		/// <summary>
		/// Tests the AutoNumber field concurrency.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
        [Ignore("Failing intermittently. Disabling test until we fix the bug.")]
		public void AutoNumberField_Concurrency( )
		{
			var field = new AutoNumberField( );
			var definition = new Definition( );
			var deletables = new List<Entity>( );

			try
			{
				field.Name = "TestAutoNumberField";

				definition.Name = "TestAutoNumberDefinition";
				definition.Fields.Add( field.As<Field>( ) );
				definition.Save( );

				var values = new List<int>( );
				object lockObject = new object( );

				var thread1 = new Thread( ( ) =>
					{
						for ( int i = 0; i < 10; i++ )
						{
							var instance = new Entity( definition.Id );
							instance.Save( );
							deletables.Add( instance );
							int value = instance.GetField<int>( field );

							lock ( lockObject )
							{
								values.Add( value );
							}
						}
					} );

				var thread2 = new Thread( ( ) =>
					{
						for ( int i = 0; i < 10; i++ )
						{
							var instance = new Entity( definition.Id );
							instance.Save( );
							deletables.Add( instance );
							int value = instance.GetField<int>( field );

							lock ( lockObject )
							{
								values.Add( value );
							}
						}
					} );

				thread1.IsBackground = true;
				thread1.Start( );

				thread2.IsBackground = true;
				thread2.Start( );

				thread1.Join( );
				thread2.Join( );

				Assert.AreEqual( 20, values.Count );
				Assert.AreEqual( 20, values.Distinct( ).Count( ) );

				for ( int i = 1; i <= 20; i++ )
				{
					Assert.IsTrue( values.Contains( i ) );
				}
			}
			finally
			{
				deletables.ForEach( e => e.Delete( ) );

				definition.Delete( );
				field.Delete( );
			}
		}

        
		/// <summary>
		/// Tests the AutoNumber field concurrency.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
        [Description("In a workflow an integer value being passed into a field update for a decimal fails due to casting issue.")]
        public void DecimalField_CastInteger_20677()
        {
            var decfield = new DecimalField();
            var field = decfield.As<Field>();

            int value = 10;
            var valueO = (object) value;
            var result = field.ValidateFieldValue(valueO);

            Assert.IsNull(result);
        }
	}
}