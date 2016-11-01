// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;
using EDC.ReadiNow.Templates;
using Microsoft.VisualStudio.TextTemplating;

namespace EDC.ReadiNow.CodeGen
{
	/// <summary>
	///     Template processor class.
	/// </summary>
	public static class TemplateProcessor
	{
		/// <summary>
		///     Generates the template instances.
		/// </summary>
		public static int GenerateTemplateInstances( List<string> xmlFiles, List<string> templateFiles, string outputPath, string namespaceName )
		{
			IEnumerable<Entity> entities = XmlParser.ReadEntities( xmlFiles );

			/////
			// Construct the alias and schema resolver instances.
			/////
			IList<Entity> entityList = entities as IList<Entity> ?? entities.ToList( );
		    Decorator.DecorateEntities(entityList);

			IAliasResolver aliasResolver = new EntityStreamAliasResolver( entityList );
			var schemaResolver = new SchemaResolver( entityList, aliasResolver );
			schemaResolver.Initialize( );

			/////
			// Set the required values into the static model class.
			/////
			SetTemplateParameters( namespaceName, schemaResolver, aliasResolver );

			/////
			// Create the transform engine.
			/////
			var engine = new Engine( );

			var host = new TransformHost( );

			/////
			// Method return code.
			/////
			int returnCode = 0;

			/////
			// Cycle through the template files.
			/////
			foreach ( string templateFile in templateFiles )
			{
				try
				{
					var fi = new FileInfo( templateFile );

					host.TemplateFileValue = fi.Name;

                    if (templateFile.Contains("Combined"))
				    {
                        AcceptAnyTypePrefix();

                        GenerateResultFile(outputPath, engine, host, templateFile, "");
				    }
				    else
				    {
                        ResetTypePrefix();

                        while (MoveToNextTypePrefix())
				        {
				            string outputSuffix = "." + CurrentTypePrefix().ToString().ToUpperInvariant();
				            GenerateResultFile(outputPath, engine, host, templateFile, outputSuffix);
				        }
				    }
				}
				catch ( Exception exc )
				{
					Console.WriteLine( "Failed to generate instance for template '{0}'. Exception: {1}", templateFile, exc.Message );

					returnCode = 1;
				}
			}

			return returnCode;
		}

        /// <summary>
        /// Produces an individual output file.
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="host">The host.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="outputSuffix">The output suffix.</param>
        private static void GenerateResultFile(string outputPath, Engine engine, TransformHost host, string templateFile, string outputSuffix)
        {
            /////
            // Process the template.
            /////
            string templateText = engine.ProcessTemplate(File.ReadAllText(templateFile), host);

            if (host.Errors != null && host.Errors.Count > 0)
            {
                host.LogErrors(host.Errors);
            }

            if (!string.IsNullOrEmpty(templateText))
            {
                using (var writer = new StreamWriter(Path.Combine(outputPath, Path.GetFileNameWithoutExtension(templateFile) + outputSuffix + host.FileExtension)))
                {
                    /////
                    // Write the template out the file system.
                    /////
                    writer.Write(templateText);
                }
            }
        }

		/// <summary>
		///     Moves to next type prefix.
		/// </summary>
		private static bool MoveToNextTypePrefix( )
		{
			if ( Model.ProcessTypesStartingWith == 'z' )
			{
				return false;
			}

			Model.ProcessTypesStartingWith++;

			return true;
		}

		private static char CurrentTypePrefix( )
			{
				return Model.ProcessTypesStartingWith;
			}


        /// <summary>
        ///     Resets the type prefix.
        /// </summary>
        private static void AcceptAnyTypePrefix()
        {
            Model.ProcessTypesStartingWith = '*';
        }

        /// <summary>
		///     Resets the type prefix.
		/// </summary>
		private static void ResetTypePrefix( )
		{
			Model.ProcessTypesStartingWith = 'a';
			Model.ProcessTypesStartingWith--;
		}

		/// <summary>
		///     Sets the static model parameters.
		/// </summary>
		private static void SetTemplateParameters( string namespaceName, SchemaResolver schemaResolver, IAliasResolver aliasResolver )
		{
			Model.Namespace = namespaceName;
			Model.SchemaResolver = schemaResolver;
			Model.AliasResolver = aliasResolver;
		}
	}
}