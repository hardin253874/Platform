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
    /// This command runs Nunit.exe.
    /// </summary>
    internal class OpenCoverConsoleCommand : StartProcessCommand
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
        /// Initializes a new instance of the <see cref="OpenCoverConsoleCommand"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="resultsFileName">Name of the results file.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public OpenCoverConsoleCommand(string arguments, string resultsFileName, bool mandatory)
            : base(Settings.Default.OpenCoverConsoleExePath, arguments, true, true, mandatory)
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
        /// Execute Nunit.exe.
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
                    errorMessage = string.Format("The Nunit results file {0} does not exist.", resultsFileName);
                }
                else
                {
                    XPathDocument document = new XPathDocument(resultsFileName);
                    XPathNavigator navigator = document.CreateNavigator();

                    XPathNavigator testResultsNode = navigator.SelectSingleNode("test-results");
                    if (testResultsNode != null)
                    {
                        int errors = testResultsNode.GetAttributeAsInt("errors", 0);
                        int failures = testResultsNode.GetAttributeAsInt("failures", 0);
                        int invalid = testResultsNode.GetAttributeAsInt("invalid", 0);
                        int inconclusive = testResultsNode.GetAttributeAsInt("inconclusive", 0);

                        if (errors > 0 ||
                            failures > 0 ||
                            invalid > 0 ||
                            inconclusive > 0)
                        {
                            succeeded = false;
                            errorMessage = string.Format("Errors:{0} Failures:{1} Invalid:{2} Inconclusive:{3}. See results file {1} for more information", errors, failures, invalid, inconclusive, resultsFileName);
                        }
                    }
                }
            }
        }        
    }
}