// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;
using System.Xml.Schema;
using EDC.ReadiNow.Common.ConfigParser.Logging;

namespace EDC.ReadiNow.XsdGen
{
    /// <summary>
    /// Main program
    /// </summary>
    class Program
    {
		/// <summary>
		/// Event log source.
		/// </summary>
		public static readonly string eventLogSource = "EDC Xsd Generator";

        static int Main(string[] args)
        {
            try
            {
                // Read command line arguments
                if (args == null || args.Length < 4 || args[0] != "-o")
                {
                    Console.WriteLine(">>" + string.Join(" ", args) + "<<");
                    Console.WriteLine("Usage:");
                    Console.WriteLine("XsdGen.exe [-o result.xsd namepspace]+ [input.xml]+");
                    Console.WriteLine("Entire set of files must be self describing. Must minimally include the bootstrap.");
                    return 1;
                }

                var inputPaths = new List<string>();
                var outputSchemas = new List<SchemaFile>();

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg == "-o")
                    {
                        string path = args[i + 1];
                        string ns = args[i + 2];
                        if (!Regex.IsMatch(ns, "^[a-zA-Z]+$"))
                        {
                            throw new Exception(string.Format("The namespace specified for {0} was invalid or not set. Check that 'Custom Tool Namespace' is set.", Path.GetFileName(path)));
                        }
                        outputSchemas.Add(new SchemaFile() { Path = path, Namespace = ns });
                        i += 2;
                    }
                    else
                    {
                        inputPaths.Add(arg);
                    }
                }

                // Move to front. (Not necessary, but helpful when debugging)
                var n = inputPaths.FindIndex(x => x.Contains("Bootstrap_Types.xml"));
                if (n != -1)
                {
                    var path = inputPaths[n];
                    inputPaths.RemoveAt(n);
                    inputPaths.Insert(0, path);
                }

                // Process config files and get stream of entities
                IEnumerable<Entity> entities = XmlParser.ReadEntities(inputPaths);

                // Process the entities and build the schema
                XsdBuilder.BuildSchemas(entities, outputSchemas);

                // Write schemas to the output files
                try
                {
                    // First write all to temp files
                    foreach (var schemaFile in outputSchemas)
                    {
                        if (schemaFile.XmlSchema == null)
                            continue;
                        //throw new Exception("No schema is targeting " + schemaFile.Namespace);

                        schemaFile.TempPath = Path.GetTempFileName();
                        using (var writer = new StreamWriter(schemaFile.TempPath))
                        {
                            schemaFile.XmlSchema.Write(writer);
                        }
                    }
                    // Then on success, move over to live files
                    foreach (var schemaFile in outputSchemas)
                    {
                        if (schemaFile.TempPath == null)
                            continue;
                        File.Copy(schemaFile.TempPath, schemaFile.Path, true);
                    }
                }
                finally
                {
                    // Clean up temp files
                    foreach (var schemaFile in outputSchemas)
                    {
                        try
                        {
                            if (schemaFile.TempPath != null)
                                File.Delete(schemaFile.TempPath);
                        }
                        catch { }
                    }

                }

                return 0;
            }
            catch (BuildException ex)
            {
                WriteError(ex);
                Console.Error.WriteLine(ex.FormatMessage());
				EventLog.WriteError( ex.ToString( ), eventLogSource );
                return 1;
            }
            catch (Exception ex)
            {
                WriteError(ex);
                Console.Error.WriteLine("Config error: " + ex.Message);

                Console.Error.WriteLine("Arguments: " + string.Join(" ", args));
                Console.Error.WriteLine(ex.ToString());

				EventLog.WriteError( ex.ToString( ), eventLogSource );
                return 2;
            }
        }

        private static void WriteError(Exception ex)
        {
            try
            {
                using (TextWriter w = new StreamWriter(@"C:\XsdGenError.txt", false))
                {
                    w.WriteLine(System.DateTime.Now.ToString());
                    w.WriteLine(ex.ToString());
                }
            }
            catch { } 

        }
    }
}
