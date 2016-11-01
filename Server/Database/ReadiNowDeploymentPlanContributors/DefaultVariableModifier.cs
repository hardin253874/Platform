// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Dac.Deployment;

namespace ReadiNowDeploymentPlanContributors
{
	/// <summary>
	///     Sets the value of the default variables in the deployment plan.
	/// </summary>
	[ExportDeploymentPlanModifier( "ReadiNowDeploymentPlanContributors.DefaultVariableModifier", "1.0.0.0" )]
	public class DefaultVariableModifier : DeploymentPlanModifier
	{
		/// <summary>
		///     Argument defining the default file prefix
		/// </summary>
		public const string DefaultFilePrefix = "DefaultVariableModifier.DefaultFilePrefix";

		/// <summary>
		///     Argument specifying the default path to save files in the database.
		/// </summary>
		public const string DefaultPath = "DefaultVariableModifier.DefaultPath";

		/// <summary>
		///     Called by the deployment engine to allow custom contributors to execute their unique tasks
		/// </summary>
		/// <param name="context">A <see cref="T:Microsoft.SqlServer.Dac.Deployment.DeploymentContributorContext" /> object</param>
		protected override void OnExecute( DeploymentPlanContributorContext context )
		{
			string defaultPath;
			string defaultFilePrefix;

			if ( context.Arguments.TryGetValue( DefaultPath, out defaultPath ) && context.Arguments.TryGetValue( DefaultFilePrefix, out defaultFilePrefix ) )
			{
				defaultPath = new DirectoryInfo( defaultPath ).FullName + "\\";

				SetDefaultVariables( context, defaultPath, defaultFilePrefix );
			}
		}

		/// <summary>
		///     Sets the default variables.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="defaultPath">The default path.</param>
		/// <param name="defaultFilePrefix">The default file prefix.</param>
		private void SetDefaultVariables( DeploymentPlanContributorContext context, string defaultPath, string defaultFilePrefix )
		{
			DeploymentStep nextStep = context.PlanHandle.Head;

			/////
			// Loop through all steps in the deployment plan
			/////
			bool foundSetVars = false;

			while ( nextStep != null && !foundSetVars )
			{
				DeploymentStep currentStep = nextStep;

				/////
				// Only interrogate up to BeginPreDeploymentScriptStep - variables must be done before that
				// We know this based on debugging a new deployment and examining the output script
				/////
				if ( currentStep is BeginPreDeploymentScriptStep )
				{
					break;
				}

				var scriptStep = currentStep as DeploymentScriptStep;

				if ( scriptStep != null )
				{
					IList<string> scripts = scriptStep.GenerateTSQL( );

					foreach ( string script in scripts )
					{
						if ( script.Contains( "DefaultDataPath" ) )
						{
							/////
							// This is the step that sets the default data path and log path.
							/////
							foundSetVars = true;

							/////
							// Override variables before the deployment begins
							/////
							var sb = new StringBuilder( );

							sb.AppendFormat( ":setvar DefaultDataPath \"{0}\"", defaultPath );
							sb.AppendLine( );

							sb.AppendFormat( ":setvar DefaultLogPath \"{0}\"", defaultPath );
							sb.AppendLine( );

							sb.AppendFormat( ":setvar DefaultFilePrefix \"{0}\"", defaultFilePrefix );
							sb.AppendLine( );

							/////
							// Create a new step for the setvar statements, and add it after the existing step.
							// That ensures that the updated values are used instead of the defaults
							/////
							var setVarsStep = new DeploymentScriptStep( sb.ToString( ) );

							AddAfter( context.PlanHandle, scriptStep, setVarsStep );
						}
					}
				}

				nextStep = currentStep.Next;
			}
		}
	}
}