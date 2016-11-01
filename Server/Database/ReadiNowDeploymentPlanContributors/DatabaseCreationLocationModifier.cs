// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ReadiNowDeploymentPlanContributors
{
	/// <summary>
	///     Sets the location of the database files during creation.
	/// </summary>
	[ExportDeploymentPlanModifier( "ReadiNowDeploymentPlanContributors.DatabaseCreationLocationModifier", "1.0.0.0" )]
	public class DatabaseCreationLocationModifier : DeploymentPlanModifier
	{
		/// <summary>
		///     LDF file path.
		/// </summary>
		public const string LdfFilePath = "DatabaseCreationLocationModifier.LdfFilePath";

		/// <summary>
		///     MDF file path.
		/// </summary>
		public const string MdfFilePath = "DatabaseCreationLocationModifier.MdfFilePath";

		/// <summary>
		///     Called by the deployment engine to allow custom contributors to execute their unique tasks
		/// </summary>
		/// <param name="context">A <see cref="T:Microsoft.SqlServer.Dac.Deployment.DeploymentContributorContext" /> object</param>
		protected override void OnExecute( DeploymentPlanContributorContext context )
		{
			string mdfFilePath;
			string ldfFilePath;

			if ( context.Arguments.TryGetValue( MdfFilePath, out mdfFilePath ) && context.Arguments.TryGetValue( LdfFilePath, out ldfFilePath ) )
			{
				SetDatabaseLocation( context, mdfFilePath, ldfFilePath );
			}
		}

		/// <summary>
		///     Sets the database location.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="mdfFilePath">The MDF file path.</param>
		/// <param name="ldfFilePath">The LDF file path.</param>
		private void SetDatabaseLocation( DeploymentPlanContributorContext context, string mdfFilePath, string ldfFilePath )
		{
			DeploymentStep nextStep = context.PlanHandle.Head;

			/////
			// Loop through all steps in the deployment plan
			/////
			bool finished = false;

			while ( nextStep != null && !finished )
			{
				DeploymentStep currentStep = nextStep;

				/////
				// Only interrogate up to BeginPreDeploymentScriptStep
				/////
				if ( currentStep is BeginPreDeploymentScriptStep )
				{
					break;
				}

				var createDbStep = currentStep as SqlCreateDatabaseStep;

				if ( createDbStep != null )
				{
					TSqlFragment fragment = createDbStep.Script;

					var visitor = new CreateDatabaseStatementVisitor( mdfFilePath, ldfFilePath );

					fragment.Accept( visitor );

					finished = true;
				}

				nextStep = currentStep.Next;
			}
		}

		/// <summary>
		///     Create Database Statement visitor class.
		/// </summary>
		private class CreateDatabaseStatementVisitor : TSqlConcreteFragmentVisitor
		{
			/// <summary>
			///     The database log name variable
			/// </summary>
			private const string DatabaseLogNameVariable = "$(DatabaseName)_log";

			/// <summary>
			///     The database name variable
			/// </summary>
			private const string DatabaseNameVariable = "$(DatabaseName)";

			/// <summary>
			///     Initializes a new instance of the <see cref="CreateDatabaseStatementVisitor" /> class.
			/// </summary>
			/// <param name="mdfFileName">Name of the MDF file.</param>
			/// <param name="ldfFileName">Name of the LDF file.</param>
			public CreateDatabaseStatementVisitor( string mdfFileName, string ldfFileName )
			{
				MdfFileName = mdfFileName;
				LdfFileName = ldfFileName;
			}

			/// <summary>
			///     Gets or sets the name of the LDF file.
			/// </summary>
			/// <value>
			///     The name of the LDF file.
			/// </value>
			private string LdfFileName
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the name of the MDF file.
			/// </summary>
			/// <value>
			///     The name of the MDF file.
			/// </value>
			private string MdfFileName
			{
				get;
				set;
			}

			/// <summary>
			///     Explicitly visits the node.
			/// </summary>
			/// <param name="node">The node.</param>
			public override void ExplicitVisit( CreateDatabaseStatement node )
			{
				Visit( node );
			}

			/// <summary>
			///     Visits the specified node.
			/// </summary>
			/// <param name="node">The node.</param>
			public override void Visit( CreateDatabaseStatement node )
			{
				foreach ( FileGroupDefinition fileGroup in node.FileGroups )
				{
					foreach ( FileDeclaration file in fileGroup.FileDeclarations )
					{
						/////
						// The Primary file group is where the database's MDF file will be present 
						/////
						if ( file.IsPrimary )
						{
							var nameOption = file.Options.SingleOrDefault( opt => opt.OptionKind == FileDeclarationOptionKind.Name ) as NameFileDeclarationOption;

							if ( nameOption == null )
							{
								continue;
							}

							/////
							// Verify that the filename has the pattern "$(DatabaseName)", and overwrite
							/////
							if ( string.Compare( nameOption.LogicalFileName.Identifier.Value, DatabaseNameVariable, StringComparison.OrdinalIgnoreCase ) == 0 )
							{
								var fileNameOption = file.Options.SingleOrDefault( opt => opt.OptionKind == FileDeclarationOptionKind.FileName ) as FileNameFileDeclarationOption;

								if ( fileNameOption != null )
								{
									fileNameOption.OSFileName = new StringLiteral
									{
										Value = MdfFileName,
										IsNational = false
									};
								}
							}
						}
					}
				}

				foreach ( FileDeclaration logFile in node.LogOn )
				{
					var nameOption = logFile.Options.SingleOrDefault( opt => opt.OptionKind == FileDeclarationOptionKind.Name ) as NameFileDeclarationOption;

					if ( nameOption == null )
					{
						continue;
					}

					if ( string.Compare( nameOption.LogicalFileName.Identifier.Value, DatabaseLogNameVariable, StringComparison.OrdinalIgnoreCase ) == 0 )
					{
						var fileNameOption = logFile.Options.SingleOrDefault( opt => opt.OptionKind == FileDeclarationOptionKind.FileName ) as FileNameFileDeclarationOption;

						if ( fileNameOption != null )
						{
							fileNameOption.OSFileName = new StringLiteral
							{
								Value = LdfFileName,
								IsNational = false
							};
						}
					}
				}
			}
		}
	}
}