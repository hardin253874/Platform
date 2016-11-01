// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command runs MsTest.exe.
    /// </summary>
    internal class MSTestCommand : StartProcessCommand
    {
        /// <summary>
        /// The name of the results file.
        /// </summary>
        /// <value>
        /// The name of the results file.
        /// </value>
        public string ResultsFileName
        {
            get
            {
                return resultsFileName;
            }
        }
        protected string resultsFileName;


        /// <summary>
        /// Initializes a new instance of the <see cref="MSTestCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="resultsFileName">Name of the results file.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public MSTestCommand(string arguments, string resultsFileName, bool mandatory)
            : base(Settings.Default.MsTestExePath, arguments, true, true, mandatory)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                throw new ArgumentNullException("arguments");
            }

            if (string.IsNullOrEmpty(resultsFileName))
            {
                throw new ArgumentNullException("resultsFileName");
            }

            this.resultsFileName = resultsFileName;            
        }


        /// <summary>
        /// Execute MSTest.exe.
        /// </summary>
        protected override void OnExecute()
        {
            if (!string.IsNullOrEmpty(resultsFileName) &&
                File.Exists(resultsFileName))
            {
                OnOutputDataReceived(string.Format("  * The test results file {0}, already exists. Deleting it.", resultsFileName));
                File.Delete(resultsFileName);
            }

            base.OnExecute();

            if (!string.IsNullOrEmpty(resultsFileName))
            {
                if (!File.Exists(resultsFileName))
                {
                    succeeded = false;
                    errorMessage = string.Format("The MsTest.exe results file {0} does not exist.", resultsFileName);
                }
                else
                {
                    XPathDocument document = new XPathDocument(resultsFileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
                    manager.AddNamespace("ms", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

                    XPathNavigator resultSummaryNode = navigator.SelectSingleNode("ms:TestRun/ms:ResultSummary", manager);
                    if (resultSummaryNode != null)
                    {
                        XPathNavigator countersNode = resultSummaryNode.SelectSingleNode("ms:Counters", manager);

                        if (countersNode != null)
                        {
                            int failed = 0;

                            string sFailed = countersNode.GetAttribute("failed", string.Empty);

                            if (int.TryParse(sFailed, out failed))
                            {
                                if (failed > 0)
                                {
                                    succeeded = false;
                                    errorMessage = string.Format("#{0} failed tests. See results file {1} for more information", failed.ToString(), resultsFileName);
                                }
                            }
                        }
                    }
                }
            }
        }        
    }
}
