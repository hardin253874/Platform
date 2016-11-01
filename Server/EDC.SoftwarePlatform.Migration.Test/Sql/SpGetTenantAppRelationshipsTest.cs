// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Sql
{
	/// <summary>
	///     The SpGetTenantAppRelationshipsTest class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	[RunAsDefaultTenant]
	[Category( "AppLibraryTests" )]
	public class SpGetTenantAppRelationshipsTest
	{
		/// <summary>
		///     Tests the permutations.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <exception cref="System.ArgumentNullException">data</exception>
		[TestCaseSource( "TestCases" )]
		public void TestPermutations( RelationshipTestData data )
		{
			if ( string.IsNullOrEmpty( data.SourceApp.Name ) )
			{
				throw new ArgumentNullException( "data" );
			}

			long sourceAppId = CreateApplication( data.SourceApp.Name );
			data.Map.Forward[ EntityTypes.SourceApp ] = sourceAppId;
			data.Map.Reverse[ sourceAppId ] = EntityTypes.SourceApp;

			long targetAppId = -1;

			if ( !string.IsNullOrEmpty( data.TargetApp.Name ) )
			{
				targetAppId = CreateApplication( data.TargetApp.Name );
				data.Map.Forward[ EntityTypes.TargetApp ] = targetAppId;
				data.Map.Reverse[ targetAppId ] = EntityTypes.TargetApp;

				if ( data.TargetAppIsDependentOnSource )
				{
					long appDependencyId = CreateDependency( targetAppId, sourceAppId );
					data.Map.Forward [ EntityTypes.AppDependency ] = appDependencyId;
					data.Map.Reverse [ appDependencyId ] = EntityTypes.AppDependency;
				}
			}

			long typeAId = -1;

			if ( !string.IsNullOrEmpty( data.TypeA.Name ) )
			{
				typeAId = CreateType( data.TypeA.Name );
				data.Map.Forward[ EntityTypes.TypeA ] = typeAId;
				data.Map.Reverse[ typeAId ] = EntityTypes.TypeA;

				if ( data.TypeA.ExplicitlyInSourceApp )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeAId, sourceAppId );
				}

				if ( data.TypeA.ExplicitlyInTargetApp && !string.IsNullOrEmpty( data.TargetApp.Name ) )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeAId, targetAppId );
				}
			}

			long typeBId = -1;

			if ( !string.IsNullOrEmpty( data.TypeB.Name ) )
			{
				typeBId = CreateType( data.TypeB.Name );
				data.Map.Forward[ EntityTypes.TypeB ] = typeBId;
				data.Map.Reverse[ typeBId ] = EntityTypes.TypeB;

				if ( data.TypeB.ExplicitlyInSourceApp )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeBId, sourceAppId );
				}

				if ( data.TypeB.ExplicitlyInTargetApp && !string.IsNullOrEmpty( data.TargetApp.Name ) )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeBId, targetAppId );
				}
			}

			long relationshipId = -1;

			if ( !string.IsNullOrEmpty( data.Relationship.Name ) )
			{
				relationshipId = CreateRelationship( data.Relationship.Name, typeAId, typeBId );
				data.Map.Forward[ EntityTypes.Relationship ] = relationshipId;
				data.Map.Reverse[ relationshipId ] = EntityTypes.Relationship;

				if ( data.Relationship.ExplicitlyInSourceApp )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), relationshipId, sourceAppId );
				}

				if ( data.Relationship.ExplicitlyInTargetApp && !string.IsNullOrEmpty( data.TargetApp.Name ) )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), relationshipId, targetAppId );
				}

				if ( data.Relationship.ImplicitInSolution )
				{
					SqlHelper.InsertBitData( relationshipId, GetId( "core:implicitInSolution" ), true );
				}

				if ( data.Relationship.ReverseImplicitInSolution )
				{
					SqlHelper.InsertBitData( relationshipId, GetId( "core:reverseImplicitInSolution" ), true );
				}

				switch ( data.Relationship.Cardinality )
				{
					case CardinalityEnum_Enumeration.ManyToMany:
						SqlHelper.InsertRelationship( GetId( "core:cardinality" ), relationshipId, GetId( "core:manyToMany" ) );
						break;
					case CardinalityEnum_Enumeration.ManyToOne:
						SqlHelper.InsertRelationship( GetId( "core:cardinality" ), relationshipId, GetId( "core:manyToOne" ) );
						break;
					case CardinalityEnum_Enumeration.OneToMany:
						SqlHelper.InsertRelationship( GetId( "core:cardinality" ), relationshipId, GetId( "core:oneToMany" ) );
						break;
					case CardinalityEnum_Enumeration.OneToOne:
						SqlHelper.InsertRelationship( GetId( "core:cardinality" ), relationshipId, GetId( "core:oneToOne" ) );
						break;
				}
			}

			long typeAInstanceId = -1;

			if ( !string.IsNullOrEmpty( data.TypeAInstance.Name ) && typeAId >= 0 )
			{
				typeAInstanceId = CreateInstance( data.TypeAInstance.Name, typeAId );
				data.Map.Forward[ EntityTypes.TypeAInstance ] = typeAInstanceId;
				data.Map.Reverse[ typeAInstanceId ] = EntityTypes.TypeAInstance;

				if ( data.TypeAInstance.ExplicitlyInSourceApp )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeAInstanceId, sourceAppId );
				}

				if ( data.TypeAInstance.ExplicitlyInTargetApp && !string.IsNullOrEmpty( data.TargetApp.Name ) )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeAInstanceId, targetAppId );
				}
			}

			long typeBInstanceId = -1;

			if ( !string.IsNullOrEmpty( data.TypeBInstance.Name ) && typeBId >= 0 )
			{
				typeBInstanceId = CreateInstance( data.TypeBInstance.Name, typeBId );
				data.Map.Forward[ EntityTypes.TypeBInstance ] = typeBInstanceId;
				data.Map.Reverse[ typeBInstanceId ] = EntityTypes.TypeBInstance;

				if ( data.TypeBInstance.ExplicitlyInSourceApp )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeBInstanceId, sourceAppId );
				}

				if ( data.TypeBInstance.ExplicitlyInTargetApp && !string.IsNullOrEmpty( data.TargetApp.Name ) )
				{
					SqlHelper.InsertRelationship( GetId( "core:inSolution" ), typeBInstanceId, targetAppId );
				}
			}

			if ( data.TypeAInstanceToTypeBInstanceRelationship && relationshipId > 0 && typeAInstanceId > 0 && typeBInstanceId > 0 )
			{
				SqlHelper.InsertRelationship( relationshipId, typeAInstanceId, typeBInstanceId );
			}

			HashSet<RelationshipResult> results = new HashSet<RelationshipResult>( );

			List<Tuple<Guid, Guid, Guid>> upgradeResults = new List<Tuple<Guid, Guid, Guid>>( );

			using ( var ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( "spGetTenantAppRelationships", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@solutionId", DbType.Int64, sourceAppId );
					ctx.AddParameter( command, "@tenant", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@selfContained", DbType.Boolean, true );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							Guid typeUid = reader.GetGuid( 0 );
							Guid fromUid = reader.GetGuid( 1 );
							Guid toUid = reader.GetGuid( 2 );

							upgradeResults.Add( new Tuple<Guid, Guid, Guid>( typeUid, fromUid, toUid ) );
						}
					}
				}
			}

			foreach ( Tuple<Guid, Guid, Guid> upgradeResult in upgradeResults )
			{
				results.Add( new RelationshipResult( Entity.GetIdFromUpgradeId( upgradeResult.Item1 ), Entity.GetIdFromUpgradeId( upgradeResult.Item2 ), Entity.GetIdFromUpgradeId( upgradeResult.Item3 ) ) );
			}

			string message = string.Empty;

			foreach ( ExpectedRelationship rel in data.ResultContains )
			{
				bool found = false;

				foreach ( RelationshipResult result in  results )
				{
					if ( !string.IsNullOrEmpty( rel.TypeAlias ) )
					{
						if ( result.TypeId != GetId( rel.TypeAlias ) )
						{
							continue;
						}
					}
					else
					{
						if ( result.TypeId != data.Map.Forward[ rel.Type ] )
						{
							continue;
						}
					}

					if ( !string.IsNullOrEmpty( rel.FromAlias ) )
					{
						if ( result.FromId != GetId( rel.FromAlias ) )
						{
							continue;
						}
					}
					else
					{
						if ( result.FromId != data.Map.Forward[ rel.From ] )
						{
							continue;
						}
					}

					if ( !string.IsNullOrEmpty( rel.ToAlias ) )
					{
						if ( result.ToId != GetId( rel.ToAlias ) )
						{
							continue;
						}
					}
					else
					{
						if ( result.ToId != data.Map.Forward[ rel.To ] )
						{
							continue;
						}
					}

					found = true;
					results.Remove( result );
					break;
				}

				if ( !found )
				{
					message += string.Format( "Expected relationship (Type: {0}, From: {1}, To: {2}) to be included but it was not.\n\n", string.IsNullOrEmpty( rel.TypeAlias ) ? rel.Type.ToString( ) : rel.TypeAlias, string.IsNullOrEmpty( rel.FromAlias ) ? rel.From.ToString( ) : rel.FromAlias, string.IsNullOrEmpty( rel.ToAlias ) ? rel.To.ToString( ) : rel.ToAlias );
				}
			}

			if ( results.Count > 0 )
			{
				message += results.Aggregate( "Found unexpected relationship:\n", ( current, id ) => current + string.Format( "Type: {0}, From: {1}, To: {2}\n\n", GetName( id.TypeId, data ), GetName( id.FromId, data ), GetName( id.ToId, data ) ) );
			}

			if ( !string.IsNullOrEmpty( message ) )
			{
				Assert.Fail( message );
			}
		}

		private string GetName( long id, RelationshipTestData data )
		{
			EntityTypes types;

			if ( data.Map.Reverse.TryGetValue( id, out types ) )
			{
				return types.ToString( );
			}

			IDictionary<IEntityRef, object> fields = Entity.GetField( new[ ]
			{
				new EntityRef( id )
			}, new EntityRef( "core:alias" ) );

			if ( fields != null )
			{
				if ( fields.Count > 0 )
				{
					return fields.FirstOrDefault( ).Value.ToString( );
				}
			}

			return id.ToString( );
		}

		/// <summary>
		///     Tests the cases.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<TestCaseData> TestCases( )
		{
			List<TestCaseData> testCases = new List<TestCaseData>
			{
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:indirectInSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.TypeB
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = true,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							Type = EntityTypes.Relationship,
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeBInstance
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.SourceApp
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = true,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							Type = EntityTypes.Relationship,
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeBInstance
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = true,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.SourceApp
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = null
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = true,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = true,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeB,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeB,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeB,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:indirectInSolution",
							From = EntityTypes.TypeBInstance,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							Type = EntityTypes.Relationship,
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeBInstance
						}
					}
				} ),

				/////
				// TargetApp tests
				/////
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					Relationship =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:indirectInSolution",
							From = EntityTypes.Relationship,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:toType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeB
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.Relationship,
							ToAlias = "core:resource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:cardinality",
							From = EntityTypes.Relationship,
							ToAlias = "core:manyToMany"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.Relationship,
							ToAlias = "core:relationship"
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = false,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:fromType",
							From = EntityTypes.Relationship,
							To = EntityTypes.TypeA
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = true,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true,
						ImplicitInSolution = false,
						ReverseImplicitInSolution = false,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = false,
					TypeBInstance =
					{
						Name = null,
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = false
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:dependencyApplication",
							From = EntityTypes.AppDependency,
							To = EntityTypes.SourceApp
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = true,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true,
						ImplicitInSolution = true,
						ReverseImplicitInSolution = true,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = true,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:dependencyApplication",
							From = EntityTypes.AppDependency,
							To = EntityTypes.SourceApp
						}
					}
				} ),
				new TestCaseData( new RelationshipTestData
				{
					SourceApp =
					{
						Name = "AppA"
					},
					TargetApp =
					{
						Name = "AppB"
					},
					TargetAppIsDependentOnSource = true,
					TypeA =
					{
						Name = "TypeA",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeB =
					{
						Name = "TypeB",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					Relationship =
					{
						Name = "RelA_B",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true,
						ImplicitInSolution = true,
						ReverseImplicitInSolution = true,
						Cardinality = CardinalityEnum_Enumeration.ManyToMany
					},
					TypeAInstance =
					{
						Name = "TypeAInstance",
						ExplicitlyInSourceApp = true,
						ExplicitlyInTargetApp = false
					},
					TypeAInstanceToTypeBInstanceRelationship = true,
					TypeBInstance =
					{
						Name = "TypeBInstance",
						ExplicitlyInSourceApp = false,
						ExplicitlyInTargetApp = true
					},
					ResultContains = new List<ExpectedRelationship>
					{
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.SourceApp,
							ToAlias = "core:solution"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeA,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inherits",
							From = EntityTypes.TypeA,
							ToAlias = "core:userResource"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeA,
							ToAlias = "core:definition"
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:inSolution",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.SourceApp
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:isOfType",
							From = EntityTypes.TypeAInstance,
							To = EntityTypes.TypeA
						},
						new ExpectedRelationship
						{
							TypeAlias = "core:dependencyApplication",
							From = EntityTypes.AppDependency,
							To = EntityTypes.SourceApp
						}
					}
				} )
			};

			return testCases;
		}

		/// <summary>
		///     Creates the application.
		/// </summary>
		/// <returns></returns>
		private static long CreateApplication( string name, string version = "1.0.0.0" )
		{
			/////
			// Solution entity
			/////
			long solutionId = SqlHelper.InsertEntity( );

			SqlHelper.InsertNVarCharData( solutionId, GetId( "core:name" ), name );
			SqlHelper.InsertGuidData( solutionId, GetId( "core:packageId" ), Guid.NewGuid( ) );
			SqlHelper.InsertNVarCharData( solutionId, GetId( "core:solutionVersionString" ), version );

			SqlHelper.InsertRelationship( GetId( "core:isOfType" ), solutionId, GetId( "core:solution" ) );

			return solutionId;
		}

		/// <summary>
		///     Creates the type.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private static long CreateType( string name )
		{
			long typeId = SqlHelper.InsertEntity( );

			SqlHelper.InsertNVarCharData( typeId, GetId( "core:name" ), name );

			SqlHelper.InsertRelationship( GetId( "core:inherits" ), typeId, GetId( "core:userResource" ) );
			SqlHelper.InsertRelationship( GetId( "core:isOfType" ), typeId, GetId( "core:definition" ) );

			return typeId;
		}

		/// <summary>
		///     Creates the relationship.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		/// <returns></returns>
		private static long CreateRelationship( string name, long fromId, long toId )
		{
			long relationshipId = SqlHelper.InsertEntity( );

			SqlHelper.InsertNVarCharData( relationshipId, GetId( "core:name" ), name );

			SqlHelper.InsertRelationship( GetId( "core:inherits" ), relationshipId, GetId( "core:resource" ) );
			SqlHelper.InsertRelationship( GetId( "core:isOfType" ), relationshipId, GetId( "core:relationship" ) );
			SqlHelper.InsertRelationship( GetId( "core:fromType" ), relationshipId, fromId );
			SqlHelper.InsertRelationship( GetId( "core:toType" ), relationshipId, toId );

			return relationshipId;
		}

		/// <summary>
		///     Creates the instance.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		private static long CreateInstance( string name, long typeId )
		{
			long instanceId = SqlHelper.InsertEntity( );

			SqlHelper.InsertNVarCharData( instanceId, GetId( "core:name" ), name );

			SqlHelper.InsertRelationship( GetId( "core:isOfType" ), instanceId, typeId );

			return instanceId;
		}

		/// <summary>
		/// Creates the dependency.
		/// </summary>
		/// <param name="dependentId">The dependent identifier.</param>
		/// <param name="dependencyId">The dependency identifier.</param>
		/// <returns></returns>
		private static long CreateDependency( long dependentId, long dependencyId )
		{
			long appDependencyId = SqlHelper.InsertEntity( );

			SqlHelper.InsertNVarCharData( appDependencyId, GetId( "core:name" ), $"App {dependentId} depends on {dependencyId}." );

			SqlHelper.InsertRelationship( GetId( "core:isOfType" ), appDependencyId, GetId( "core:applicationDependency" ) );

			SqlHelper.InsertRelationship( GetId( "core:dependentApplication" ), appDependencyId, dependentId );
			SqlHelper.InsertRelationship( GetId( "core:dependencyApplication" ), appDependencyId, dependencyId );

			return appDependencyId;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		private static long GetId( string alias )
		{
			return new EntityAlias( alias ).ToEntityId( );
		}
	}
}