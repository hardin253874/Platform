// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Common.ConfigParser;
using System.Xml.Linq;
using EDC.ReadiNow.Common.ConfigParser.Logging;

namespace EDC.ReadiNow.XmlUtil
{
    /// <summary>
    /// Various tools for checking and updating the XML config files.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Event log source.
        /// </summary>
        public static readonly string eventLogSource = "EDC XML Util";


        static void Main(string[] args)
        {
            try
            {
                // Display command help
                if (args.Length == 0)
                {
                    Console.WriteLine(@"Usage:");
                    Console.WriteLine(@"XmlUtil.exe -check -config [file1] [file2] [etc] -map upgrademap.xml");
                    Console.WriteLine("Checks config and map for problems.");
                    Console.WriteLine(@"XmlUtil.exe -fixmap -config path\*.xml -map upgrademap.xml");
                    Console.WriteLine("Updates the map by adding/removing any missing aliases.");
                    Console.WriteLine(@"XmlUtil.exe -fixconfig -config path\*.xml");
                    Console.WriteLine("Updates the config by generating any missing aliases.");
                    Console.WriteLine(@"-f or -force to force simultaneous adds and removes.");
                    return;
                }

                // Parse command line arguments
                List<string> config = new List<string>();
                List<string> maps = new List<string>();
                bool checkConfig = false;
                bool fixMap = false;
                bool fixConfig = false;
                bool force = false;
                List<string> curList = null;

                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "-check":
                            checkConfig = true;
                            break;
                        case "-f":
                        case "-force":
                            force = true;
                            break;
                        case "-fixmap":
                            fixMap = true;
                            break;
                        case "-fixconfig":
                            fixConfig = true;
                            break;
                        case "-config":
                            curList = config;
                            break;
                        case "-map":
                            curList = maps;
                            break;
                        default:
                            curList.Add(arg);
                            break;
                    }
                }

                string mapFile = maps.FirstOrDefault();

                // Handle wildcard path in config list
                if (config.Count == 1 && config.First().Contains('*'))
                {
                    string search = config.First();
                    var files = Directory.GetFiles(Path.GetDirectoryName(search), Path.GetFileName(search), SearchOption.AllDirectories);
                    config = files.Where(x => XmlParser.IsConfigXml(x)).ToList();
                }
                
                // Run config check
                if (checkConfig)
                {
                    ConfigChecker check = new ConfigChecker();
                    check.CheckConfig(config, mapFile);
                    return;
                }

                if (fixConfig)
                {
                    AliasAdder adder = new AliasAdder();
                    ISet<string> takenAliases = FindAllAliases(config);
                    foreach (string file in config)
                    {
                        adder.AddMissingAliases(file, takenAliases);
                    }
                }

                if (fixMap)
                {
                    UpgradeMapTool.AddRemoveEntries(config, mapFile, force);
					UpgradeMapTool.CheckForDuplicates(mapFile, force);
				}
            }
            catch (BuildException ex)
            {
                WriteError(ex);
                Console.Error.WriteLine(ex.FormatMessage());
                EventLog.WriteError(ex.ToString(), eventLogSource);
                return;
            }
            catch (Exception ex)
            {
                WriteError(ex);
                Console.Error.WriteLine("Config error: " + ex.Message);

                Console.Error.WriteLine("Arguments: " + string.Join(" ", args));
                Console.Error.WriteLine(ex.ToString());

                EventLog.WriteError(ex.ToString(), eventLogSource);
                return;
            }
            return;

        }


        /// <summary>
        /// Scans a set of XML config files and finds the alias of every entity.
        /// </summary>
        public static ISet<string> FindAllAliases(IEnumerable<string> files)
        {
            return new HashSet<string>(
                XmlParser.ReadEntities(files)
                    .Where(e => e.Alias != null)
                    .Select(entity => string.Format("{0}:{1}", entity.Alias.Namespace, entity.Alias.Value)));
        }


        /// <summary>
        /// Write an error to a text file
        /// </summary>
        private static void WriteError(Exception ex)
        {
            try
            {
                using (TextWriter w = new StreamWriter(@"C:\XmlUtilError.txt", false))
                {
                    w.WriteLine(System.DateTime.Now.ToString());
                    w.WriteLine(ex.ToString());
                }
            }
            catch { }
        }

    }
}
