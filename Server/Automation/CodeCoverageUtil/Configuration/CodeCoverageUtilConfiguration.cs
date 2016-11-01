// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;
using CodeCoverageUtil.Commands;
using System.Collections.ObjectModel;

namespace CodeCoverageUtil.Configuration
{    
    /// <summary>
    /// 
    /// </summary>
    internal class CodeCoverageUtilConfiguration
    {
        #region Private Fields
        /// <summary>
        /// The name of the config file.
        /// </summary>
        private string configFileName;


        /// <summary>
        /// The directory where the config file resides.
        /// </summary>
        private string configDirectory;        
        #endregion        


        #region Properties
        /// <summary>
        /// Gets the details.
        /// </summary>
        public ReadOnlyCollection<String> Details
        {
            get
            {
                return details.AsReadOnly();
            }
        }
        private List<string> details = new List<string>();


        /// <summary>
        /// Gets a value indicating whether to keep history.
        /// </summary>
        /// <value>
        ///   <c>true</c> to keep history; otherwise, <c>false</c>.
        /// </value>
        public bool KeepHistory { get; private set; }


        /// <summary>
        /// Gets a value indicating whether results should be saved.
        /// </summary>
        /// <value>
        ///   <c>true</c> to not save results]; otherwise, <c>false</c>.
        /// </value>
        public bool NoResults { get; private set; }


        /// <summary>
        /// Gets the results path.
        /// </summary>
        public string ResultsPath { get; private set; }


        /// <summary>
        /// Gets the temp path.
        /// </summary>
        public string TempPath { get; private set; }


        /// <summary>
        /// The path to the test settings file.
        /// </summary>
        public string TestSettingsPath { get; private set; }


        /// <summary>
        /// The test assemblies configuration.
        /// </summary>
        public List<TestAssemblyConfig> TestAssembliesConfig
        {
            get
            {
                return testAssembliesConfig;
            }
        }
        private List<TestAssemblyConfig> testAssembliesConfig = new List<TestAssemblyConfig>();


        /// <summary>
        /// Referenced assemblies configuration.
        /// </summary>
        public List<ReferencedAssemblyConfig> ReferencedAssembliesConfig
        {
            get
            {
                return referencedAssembliesConfig;
            }
        }
        private List<ReferencedAssemblyConfig> referencedAssembliesConfig = new List<ReferencedAssemblyConfig>();


        /// <summary>
        /// The list of assemblies under test.
        /// </summary>
        public List<AssemblyUnderTestConfig> AssembliesUnderTestConfig
        {
            get
            {
                return assembliesUnderTestConfig;
            }
        }
        private List<AssemblyUnderTestConfig> assembliesUnderTestConfig = new List<AssemblyUnderTestConfig>();


        /// <summary>
        /// Shutdown commands
        /// </summary>
        public List<ICommand> ShutdownCommands
        {
            get
            {
                return shutdownCommands;
            }
        }
        private List<ICommand> shutdownCommands = new List<ICommand>();


        /// <summary>
        /// Startup commands
        /// </summary>
        public List<ICommand> StartUpCommands
        {
            get
            {
                return startUpCommands;
            }
        }
        private List<ICommand> startUpCommands = new List<ICommand>();        
        #endregion


        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public CodeCoverageUtilConfiguration(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            configFileName = Path.GetFullPath(fileName);
            configDirectory = Path.GetDirectoryName(configFileName);

            LoadConfigurationFromFile(configFileName);
        }
        #endregion


        #region Methods
        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private void LoadConfigurationFromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }            

            string xml = File.ReadAllText(fileName);
            LoadConfigurationFromXml(xml);
        }


        /// <summary>
        /// Loads the configuration from XML.
        /// </summary>
        /// <param name="xmlConfiguration">The XML configuration.</param>
        private void LoadConfigurationFromXml(string xmlConfiguration)
        {
            if (string.IsNullOrEmpty(xmlConfiguration))
            {
                throw new ArgumentNullException("xmlConfiguration");
            }

            XPathDocument document = new XPathDocument(new StringReader(xmlConfiguration));
            XPathNavigator navigator = document.CreateNavigator();

            XPathNavigator codeCoverageConfigNode = navigator.SelectSingleNode("codeCoverageConfiguration");
            LoadConfiguration(codeCoverageConfigNode);
        }


        /// <summary>
        /// Load the configuration from the specified node
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadConfiguration(XPathNavigator codeCoverageConfigNode)
        {
            if (codeCoverageConfigNode == null)
            {
                throw new ArgumentNullException("codeCoverageConfigNode");
            }

            LoadResultsConfig(codeCoverageConfigNode);
            LoadTestConfig(codeCoverageConfigNode);
            LoadTestAssembliesConfig(codeCoverageConfigNode);
            LoadReferencedAssembliesConfig(codeCoverageConfigNode);
            LoadAssembliesUnderTestConfig(codeCoverageConfigNode);
            LoadStartUpCommands(codeCoverageConfigNode);
            LoadShutDownCommands(codeCoverageConfigNode);                        
        }


        /// <summary>
        /// Loads the results config.
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadResultsConfig(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator resultsNode = codeCoverageConfigNode.SelectSingleNode("results");
            if (resultsNode == null)
            {
                return;
            }

            string resultsPath = ResolveRelativePath(resultsNode.GetAttribute("resultsRootPath", string.Empty));
            if (string.IsNullOrEmpty(resultsPath))
            {
                throw new InvalidDataException("The codeCoverageConfigNode/@resultsRootPath attribute was not specified.");
            }

            KeepHistory = resultsNode.GetAttributeAsBool("keepHistory", false);

            TempPath = Path.Combine(resultsPath, "Tempfiles");

            ResultsPath = resultsPath;
        }


        /// <summary>
        /// Loads the test config.
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadTestConfig(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator testConfigurationNode = codeCoverageConfigNode.SelectSingleNode("testConfiguration");
            if (testConfigurationNode == null)
            {
                return;
            }

            NoResults = testConfigurationNode.GetAttributeAsBool("noResults", false);

            XPathNavigator testDetailsNode = testConfigurationNode.SelectSingleNode("testDetails");
            if (testDetailsNode != null)
            {
                XPathNodeIterator testDetailNodes = testDetailsNode.SelectChildren("testDetail", string.Empty);
                foreach (XPathNavigator testDetailNode in testDetailNodes)
                {
                    string detail = testDetailNode.GetAttribute("detail", string.Empty);
                    if (!string.IsNullOrEmpty(detail))
                    {
                        details.Add(detail);
                    }
                }
            }
        }


        /// <summary>
        /// Load the test assemblies configuration
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadTestAssembliesConfig(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator assembliesNode = codeCoverageConfigNode.SelectSingleNode("assemblies");
            if (assembliesNode == null)
            {
                return;
            }

            string defaultPublishPath = assembliesNode.GetAttribute("publishPath", string.Empty);

            XPathNavigator testAssembliesNode = assembliesNode.SelectSingleNode("testAssemblies");

            if (testAssembliesNode == null)
            {
                return;
            }

            TestSettingsPath = ResolveRelativePath(testAssembliesNode.GetAttribute("testSettingsPath", string.Empty));

            XPathNodeIterator testAssemblyNodes = testAssembliesNode.SelectChildren("testAssembly", string.Empty);
            if (testAssemblyNodes == null)
            {
                return;
            }

            foreach (XPathNavigator testAssemblyNode in testAssemblyNodes)
            {
                string path = testAssemblyNode.GetAttribute("path", string.Empty);
                string publishPath = testAssemblyNode.GetAttribute("publishPath", string.Empty);

                if (string.IsNullOrEmpty(publishPath))
                {
                    publishPath = defaultPublishPath;
                }

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                path = ResolveRelativePath(path);
                publishPath = ResolveRelativePath(publishPath);

                TestAssemblyConfig foundConfig = (from tac in testAssembliesConfig
                                                  where string.Compare(tac.AssemblyPath, path, true) == 0
                                                  select tac).FirstOrDefault();

                if (foundConfig == null)
                {                    
                    TestAssemblyConfig testAssemblyConfig = new TestAssemblyConfig(path, publishPath);
                    testAssembliesConfig.Add(testAssemblyConfig);
                }
            }
        }


        /// <summary>
        /// Loads the referenced assemblies config.
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadReferencedAssembliesConfig(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator assembliesNode = codeCoverageConfigNode.SelectSingleNode("assemblies");
            if (assembliesNode == null)
            {
                return;
            }

            string defaultPublishPath = assembliesNode.GetAttribute("publishPath", string.Empty);

            XPathNavigator referencedAssembliesNode = assembliesNode.SelectSingleNode("referencedAssemblies");

            if (referencedAssembliesNode == null)
            {
                return;
            }            

            XPathNodeIterator referencedAssemblyNodes = referencedAssembliesNode.SelectChildren("referencedAssembly", string.Empty);
            if (referencedAssemblyNodes == null)
            {
                return;
            }

            foreach (XPathNavigator referenedAssemblyNode in referencedAssemblyNodes)
            {
                string path = referenedAssemblyNode.GetAttribute("path", string.Empty);
                string publishPath = referenedAssemblyNode.GetAttribute("publishPath", string.Empty);

                if (string.IsNullOrEmpty(publishPath))
                {
                    publishPath = defaultPublishPath;
                }

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                path = ResolveRelativePath(path);
                publishPath = ResolveRelativePath(publishPath);

                ReferencedAssemblyConfig foundConfig = (from rac in referencedAssembliesConfig
                                                        where string.Compare(rac.AssemblyPath, path, true) == 0
                                                        select rac).FirstOrDefault();

                if (foundConfig == null)
                {
                    ReferencedAssemblyConfig referencedAssemblyConfig = new ReferencedAssemblyConfig(path, publishPath);
                    referencedAssembliesConfig.Add(referencedAssemblyConfig);
                }
            }
        }


        /// <summary>
        /// Load the code coverage assemblies configuration
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadAssembliesUnderTestConfig(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator assembliesNode = codeCoverageConfigNode.SelectSingleNode("assemblies");
            if (assembliesNode == null)
            {
                return;
            }

            string defaultPublishPath = assembliesNode.GetAttribute("publishPath", string.Empty);

            XPathNavigator assembliesUnderTestNode = assembliesNode.SelectSingleNode("assembliesUnderTest");

            XPathNodeIterator assemblyUnderTestNodes = assembliesUnderTestNode.SelectChildren("assemblyUnderTest", string.Empty);
            if (assemblyUnderTestNodes == null)
            {
                return;
            }

            using (AssemblyLoader loader = new AssemblyLoader())
            {                
                foreach (XPathNavigator assemblyUnderTestNode in assemblyUnderTestNodes)
                {
                    string path = assemblyUnderTestNode.GetAttribute("path", string.Empty);
                    string publishPath = assemblyUnderTestNode.GetAttribute("publishPath", string.Empty);                    

                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(publishPath))
                    {
                        publishPath = defaultPublishPath;
                    }

                    path = ResolveRelativePath(path);
                    publishPath = ResolveRelativePath(publishPath);

                    bool isStronglyNamed = loader.IsAssemblyStrongNamed(path);

                    AssemblyUnderTestConfig foundConfig = (from c in assembliesUnderTestConfig
                                                           where string.Compare(c.AssemblyPath, path, true) == 0
                                                           select c).FirstOrDefault();

                    if (foundConfig == null)
                    {
                        AssemblyUnderTestConfig assemblyUnderTestConfig = new AssemblyUnderTestConfig(path, publishPath, isStronglyNamed);
                        assembliesUnderTestConfig.Add(assemblyUnderTestConfig);
                    }
                }
            }
        }


        /// <summary>
        /// Loads the start up commands.
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadStartUpCommands(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator startUpCommandsNode = codeCoverageConfigNode.SelectSingleNode("startUpCommands");

            if (startUpCommandsNode == null)
            {
                return;
            }

            if (startUpCommandsNode.MoveToFirstChild())
            {
                do
                {
                    string nodeName = startUpCommandsNode.Name;
                    if (nodeName == "startProcess")
                    {
                        string fileName = startUpCommandsNode.GetAttribute("fileName", string.Empty);
                        string arguments = startUpCommandsNode.GetAttribute("arguments", string.Empty);
                        
                        StartProcessCommand command = new StartProcessCommand(fileName, arguments, true, false, true);
                        startUpCommands.Add(command);
                    }
                    else if (nodeName == "startService")
                    {
                        string name = startUpCommandsNode.GetAttribute("name", string.Empty);

                        StartServiceCommand command = new StartServiceCommand(name, true);
                        startUpCommands.Add(command);
                    }
                }
                while (startUpCommandsNode.MoveToNext());
            }            
        }


        /// <summary>
        /// Loads the shut down commands.
        /// </summary>
        /// <param name="codeCoverageConfigNode">The code coverage config node.</param>
        private void LoadShutDownCommands(XPathNavigator codeCoverageConfigNode)
        {
            XPathNavigator shutDownCommandsNode = codeCoverageConfigNode.SelectSingleNode("shutdownCommands");

            if (shutDownCommandsNode == null)
            {
                return;
            }

            if (shutDownCommandsNode.MoveToFirstChild())
            {
                do
                {
                    string nodeName = shutDownCommandsNode.Name;
                    if (nodeName == "stopProcess")
                    {
                        string name = shutDownCommandsNode.GetAttribute("name", string.Empty);

                        StopProcessCommand command = new StopProcessCommand(name, true);
                        shutdownCommands.Add(command);
                    }
                    else if (nodeName == "stopService")
                    {
                        string name = shutDownCommandsNode.GetAttribute("name", string.Empty);

                        StopServiceCommand command = new StopServiceCommand(name, true);
                        shutdownCommands.Add(command);
                    }
                }
                while (shutDownCommandsNode.MoveToNext());
            }
        }


        /// <summary>
        /// Resolves the relative path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private string ResolveRelativePath(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            if (!Path.IsPathRooted(path) && string.Compare(path, "GAC", true) != 0)
            {
                path = Path.GetFullPath(Path.Combine(configDirectory, path));
            }            

            return path;
        }
        #endregion        
    }
}