// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Solutions;
using EDC.ReadiNow.Model;
using EDC.Xml;
using ESolution = EDC.ReadiNow.Model.Solution;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     Handles solution management.
	/// </summary>
	/// <remarks></remarks>
	public static class Solution
	{
		/// <summary>
		///     Look up a solution by name.
		/// </summary>
		/// <param name="solutionName">Name of the solution.</param>
		/// <returns>
		///     The solution ID.
		/// </returns>
		/// <exception cref="System.Exception">Multiple solutions found with name  + solutionName</exception>
		public static long GetSolutionIdByName( string solutionName )
		{
			IEnumerable<ESolution> matches = Entity.GetByName<ESolution>( solutionName );

			IList<ESolution> solutions = matches as IList<ESolution> ?? matches.ToList( );

			if ( solutions.Count <= 0 )
			{
				return -1;
			}

			if ( solutions.Count > 1 )
			{
				throw new Exception( "Multiple solutions found with name " + solutionName );
			}

			return solutions.First( ).Id;
		}

		/// <summary>
		///     Installs a Software Platform solution.
		/// </summary>
		/// <param name="solutionFilename">The solution filename.</param>
		/// <remarks>
		///     This function required the directory as well as the file name for the solution installation.
		///     The reason it has been done this way is because the installation of the solution must happen
		///     in the same directory as the files as there are typically no paths in the various solution
		///     bootstrap files for the other related entities.
		///     Whilst it is possible to extract the directory from the filename, by requiring the folder location as well
		///     it makes it more obvious what is going on.
		/// </remarks>
		public static void InstallSolution( string solutionFilename )
		{
			RunSolutionAction( solutionFilename, SolutionInstallAction.Install );
		}

		/// <summary>
		///     Upgrades the solution.
		/// </summary>
		/// <param name="solutionFilename">The solution filename.</param>
		public static void UpgradeSolution( string solutionFilename )
		{
			RunSolutionAction( solutionFilename, SolutionInstallAction.Upgrade );
		}

		/// <summary>
		///     Runs the solution action.
		/// </summary>
		/// <param name="solutionFilename">The solution filename.</param>
		/// <param name="action">The action.</param>
		private static void RunSolutionAction( string solutionFilename, SolutionInstallAction action )
		{
			RequestContext.SetTenantAdministratorContext( 0 );

			var document = new XmlDocument( );
			document.Load( solutionFilename );

			// Get the path to the configuration directory
			string configPath = Path.GetDirectoryName( solutionFilename );

			XmlNodeList solutionNodes = null;

			if ( XmlHelper.EvaluateNodes( document.DocumentElement, "/resources/resource" ) )
			{
				solutionNodes = XmlHelper.SelectNodes( document.DocumentElement, "/resources/resource" );
			}
			else if ( XmlHelper.EvaluateSingleNode( document.DocumentElement, "/resource" ) )
			{
				solutionNodes = XmlHelper.SelectNodes( document.DocumentElement, "/resource" );
			}

			if ( solutionNodes != null )
			{
				foreach ( XmlNode solutionNode in solutionNodes )
				{
					string solutionName = XmlHelper.ReadElementString( solutionNode, "name" );

					string startMessage;
					string endMessage;

					switch ( action )
					{
						case SolutionInstallAction.Install:
							startMessage = "Solution '{0}' is being installed.";
							endMessage = "Solution '{0}' has been installed.";
							break;
						case SolutionInstallAction.Upgrade:
							startMessage = "Solution '{0}' is being upgraded.";
							endMessage = "Solution '{0}' has been upgraded.";
							break;
						default:
							startMessage = "Solution '{0}' is being modified.";
							endMessage = "Solution '{0}' has been modified.";
							break;
					}

					EventLog.Application.WriteInformation( startMessage, solutionName );
					SolutionInstallerHelper.InstallSolution( solutionNode, configPath, action );
					EventLog.Application.WriteInformation( endMessage, solutionName );
				}
			}
		}
	}
}