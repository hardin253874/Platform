// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Complete
{
	/// <summary>
	///     Tenant Merge Processor Tests Base.
	/// </summary>
	public abstract class TenantMergeProcessorTestsBase
	{
		/// <summary>
		///     The test tenant name
		/// </summary>
		protected const string TenantName = "Tenant Z";

		/// <summary>
		///     The cardinality upgrade identifier
		/// </summary>
		private Guid _cardinalityUpgradeId = Guid.Empty;

		/// <summary>
		///     The cascade delete to upgrade identifier
		/// </summary>
		private Guid _cascadeDeleteToUpgradeId = Guid.Empty;

		/// <summary>
		///     The cascade delete upgrade identifier
		/// </summary>
		private Guid _cascadeDeleteUpgradeId = Guid.Empty;

		/// <summary>
		///     The from type upgrade identifier
		/// </summary>
		private Guid _fromTypeUpgradeId = Guid.Empty;

		/// <summary>
		///     The implicit in solution upgrade identifier
		/// </summary>
		private Guid _implicitInSolutionUpgradeId = Guid.Empty;

		/// <summary>
		///     The in solution upgrade identifier
		/// </summary>
		private Guid _inSolutionUpgradeId = Guid.Empty;

		/// <summary>
		///     The many to many upgrade identifier
		/// </summary>
		private Guid _manyToManyUpgradeId = Guid.Empty;

		/// <summary>
		///     The many to one upgrade identifier
		/// </summary>
		private Guid _manyToOneUpgradeId = Guid.Empty;

		/// <summary>
		///     The name upgrade identifier
		/// </summary>
		private Guid _nameUpgradeId = Guid.Empty;

		/// <summary>
		///     The one to many upgrade identifier
		/// </summary>
		private Guid _oneToManyUpgradeId = Guid.Empty;

		/// <summary>
		///     The one to one upgrade identifier
		/// </summary>
		private Guid _oneToOneUpgradeId = Guid.Empty;

		/// <summary>
		///     The reverse implicit in solution upgrade identifier
		/// </summary>
		private Guid _reverseImplicitInSolutionUpgradeId = Guid.Empty;

		/// <summary>
		///     The to type upgrade identifier
		/// </summary>
		private Guid _toTypeUpgradeId = Guid.Empty;

		/// <summary>
		///     Gets the cardinality upgrade identifier.
		/// </summary>
		/// <value>
		///     The cardinality upgrade identifier.
		/// </value>
		protected Guid CardinalityUpgradeId
		{
			get
			{
				if ( _cardinalityUpgradeId == Guid.Empty )
				{
					_cardinalityUpgradeId = Entity.GetUpgradeId( Relationship.Cardinality_Field.Id );
				}

				return _cardinalityUpgradeId;
			}
		}

		/// <summary>
		///     Gets the cascade delete to upgrade identifier.
		/// </summary>
		/// <value>
		///     The cascade delete to upgrade identifier.
		/// </value>
		protected Guid CascadeDeleteToUpgradeId
		{
			get
			{
				if ( _cascadeDeleteToUpgradeId == Guid.Empty )
				{
					_cascadeDeleteToUpgradeId = Entity.GetUpgradeId( Relationship.CascadeDeleteTo_Field.Id );
				}

				return _cascadeDeleteToUpgradeId;
			}
		}

		/// <summary>
		///     Gets the cascade delete upgrade identifier.
		/// </summary>
		/// <value>
		///     The cascade delete upgrade identifier.
		/// </value>
		protected Guid CascadeDeleteUpgradeId
		{
			get
			{
				if ( _cascadeDeleteUpgradeId == Guid.Empty )
				{
					_cascadeDeleteUpgradeId = Entity.GetUpgradeId( Relationship.CascadeDelete_Field.Id );
				}

				return _cascadeDeleteUpgradeId;
			}
		}

		/// <summary>
		///     Gets the database context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		protected DatabaseContext Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets from type upgrade identifier.
		/// </summary>
		/// <value>
		///     From type upgrade identifier.
		/// </value>
		protected Guid FromTypeUpgradeId
		{
			get
			{
				if ( _fromTypeUpgradeId == Guid.Empty )
				{
					_fromTypeUpgradeId = Entity.GetUpgradeId( Relationship.FromType_Field.Id );
				}

				return _fromTypeUpgradeId;
			}
		}

		/// <summary>
		///     Gets the implicit in solution upgrade identifier.
		/// </summary>
		/// <value>
		///     The implicit in solution upgrade identifier.
		/// </value>
		protected Guid ImplicitInSolutionUpgradeId
		{
			get
			{
				if ( _implicitInSolutionUpgradeId == Guid.Empty )
				{
					_implicitInSolutionUpgradeId = Entity.GetUpgradeId( Relationship.ImplicitInSolution_Field.Id );
				}

				return _implicitInSolutionUpgradeId;
			}
		}

		/// <summary>
		///     Gets the in solution upgrade identifier.
		/// </summary>
		/// <value>
		///     The in solution upgrade identifier.
		/// </value>
		protected Guid InSolutionUpgradeId
		{
			get
			{
				if ( _inSolutionUpgradeId == Guid.Empty )
				{
					_inSolutionUpgradeId = Entity.GetUpgradeId( EntityType.InSolution_Field.Id );
				}

				return _inSolutionUpgradeId;
			}
		}

		/// <summary>
		///     Gets the many to many upgrade identifier.
		/// </summary>
		/// <value>
		///     The many to many upgrade identifier.
		/// </value>
		protected Guid ManyToManyUpgradeId
		{
			get
			{
				if ( _manyToManyUpgradeId == Guid.Empty )
				{
					_manyToManyUpgradeId = Entity.GetUpgradeId( Entity.GetId( "core", "manyToMany" ) );
				}

				return _manyToManyUpgradeId;
			}
		}

		/// <summary>
		///     Gets the many to one upgrade identifier.
		/// </summary>
		/// <value>
		///     The many to one upgrade identifier.
		/// </value>
		protected Guid ManyToOneUpgradeId
		{
			get
			{
				if ( _manyToOneUpgradeId == Guid.Empty )
				{
					_manyToOneUpgradeId = Entity.GetUpgradeId( Entity.GetId( "core", "manyToOne" ) );
				}

				return _manyToOneUpgradeId;
			}
		}

		/// <summary>
		///     Gets the name upgrade identifier.
		/// </summary>
		/// <value>
		///     The name upgrade identifier.
		/// </value>
		protected Guid NameUpgradeId
		{
			get
			{
				if ( _nameUpgradeId == Guid.Empty )
				{
					_nameUpgradeId = Entity.GetUpgradeId( Entity.GetId( "core", "name" ) );
				}

				return _nameUpgradeId;
			}
		}

		/// <summary>
		///     Gets the one to many upgrade identifier.
		/// </summary>
		/// <value>
		///     The one to many upgrade identifier.
		/// </value>
		protected Guid OneToManyUpgradeId
		{
			get
			{
				if ( _oneToManyUpgradeId == Guid.Empty )
				{
					_oneToManyUpgradeId = Entity.GetUpgradeId( Entity.GetId( "core", "oneToMany" ) );
				}

				return _oneToManyUpgradeId;
			}
		}

		/// <summary>
		///     Gets the one to one upgrade identifier.
		/// </summary>
		/// <value>
		///     The one to one upgrade identifier.
		/// </value>
		protected Guid OneToOneUpgradeId
		{
			get
			{
				if ( _oneToOneUpgradeId == Guid.Empty )
				{
					_oneToOneUpgradeId = Entity.GetUpgradeId( Entity.GetId( "core", "oneToOne" ) );
				}

				return _oneToOneUpgradeId;
			}
		}

		/// <summary>
		///     Gets the reverse implicit in solution upgrade identifier.
		/// </summary>
		/// <value>
		///     The reverse implicit in solution upgrade identifier.
		/// </value>
		protected Guid ReverseImplicitInSolutionUpgradeId
		{
			get
			{
				if ( _reverseImplicitInSolutionUpgradeId == Guid.Empty )
				{
					_reverseImplicitInSolutionUpgradeId = Entity.GetUpgradeId( Relationship.ReverseImplicitInSolution_Field.Id );
				}

				return _reverseImplicitInSolutionUpgradeId;
			}
		}

		/// <summary>
		///     Gets to type upgrade identifier.
		/// </summary>
		/// <value>
		///     To type upgrade identifier.
		/// </value>
		protected Guid ToTypeUpgradeId
		{
			get
			{
				if ( _toTypeUpgradeId == Guid.Empty )
				{
					_toTypeUpgradeId = Entity.GetUpgradeId( Relationship.ToType_Field.Id );
				}

				return _toTypeUpgradeId;
			}
		}

		/// <summary>
		///     Gets the cardinality enumeration upgrade identifier.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <returns></returns>
		protected Guid GetCardinalityEnumUpgradeId( CardinalityEnum_Enumeration cardinality )
		{
			switch ( cardinality )
			{
				case CardinalityEnum_Enumeration.OneToOne:
					return OneToOneUpgradeId;
				case CardinalityEnum_Enumeration.OneToMany:
					return OneToManyUpgradeId;
				case CardinalityEnum_Enumeration.ManyToOne:
					return ManyToOneUpgradeId;
				case CardinalityEnum_Enumeration.ManyToMany:
					return ManyToManyUpgradeId;
				default:
					throw new ArgumentException( @"Invalid cardinality.", "cardinality" );
			}
		}

		/// <summary>
		///     Tests the fixture setup.
		/// </summary>
		[TestFixtureSetUp]
		public virtual void TestFixtureSetup( )
		{
			Context = DatabaseContext.GetContext( true, commandTimeout: AppManager.DefaultTimeout, transactionTimeout: AppManager.DefaultTimeout );
		}

		/// <summary>
		///     Tests the fixture tear down.
		/// </summary>
		[TestFixtureTearDown]
		public virtual void TestFixtureTearDown( )
		{
			RequestContext.FreeContext( );

			if ( Context != null )
			{
				Context.Dispose( );
				Context = null;
			}
		}

		/// <summary>
		///     Tests the setup.
		/// </summary>
		[SetUp]
		public virtual void TestSetup( )
		{
			using ( IDbCommand command = Context.CreateCommand( "SAVE TRANSACTION Test" ) )
			{
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Tests the tear down.
		/// </summary>
		[TearDown]
		public virtual void TestTearDown( )
		{
			using ( IDbCommand command = Context.CreateCommand( "ROLLBACK TRANSACTION Test" ) )
			{
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}

			TenantHelper.Flush( );
		}
	}
}