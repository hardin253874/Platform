// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command restores an assembly from the
    /// backup directory to its original location.
    /// </summary>
    internal class RestoreAssemblyCommand : CommandBase
    {
        #region Fields
        /// <summary>
        /// The file name of the assembly to restore.
        /// </summary>
        /// <value>
        /// The name of the assembly file.
        /// </value>
        public string AssemblyFileName
        {
            get
            {
                return assemblyFileName;
            }
        }
        protected string assemblyFileName;


        /// <summary>
        /// The path to the assembly backup directory.
        /// </summary>
        public string AssemblyBackupDirectory
        {
            get
            {
                return assemblyBackupDirectory;
            }
        }
        protected string assemblyBackupDirectory;
        #endregion


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreAssemblyCommand"/> class.
        /// </summary>
        /// <param name="assemblyFileName">Name of the assembly file.</param>
        /// <param name="assemblyBackupDirectory">The assembly backup directory.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public RestoreAssemblyCommand(string assemblyFileName, string assemblyBackupDirectory, bool mandatory)
        {
            if (string.IsNullOrEmpty(assemblyFileName))
            {
                throw new ArgumentNullException("assemblyFileName");
            }

            if (string.IsNullOrEmpty(assemblyBackupDirectory))
            {
                throw new ArgumentNullException("assemblyBackupDirectory");
            }           

            this.assemblyFileName = assemblyFileName;
            this.assemblyBackupDirectory = assemblyBackupDirectory;
            this.mandatory = mandatory;

            name = string.Format("Restore assembly {0}", assemblyFileName);
        }
        #endregion


        #region Methods        
        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void OnExecute()
        {
            base.OnExecute();

            string backedUpAssemblyPath = Path.Combine(assemblyBackupDirectory, assemblyFileName);            

            if (File.Exists(backedUpAssemblyPath))
            {
                string originalPath = GetOriginalAssemblyPath(Path.Combine(assemblyBackupDirectory, assemblyFileName));

                if (string.Compare(originalPath, @"GAC", true) == 0)
                {
                    // Publish to GAC
                    Process process = new Process();
                    process.StartInfo.FileName = Settings.Default.GacUtilExePath;
                    process.StartInfo.Arguments = string.Format("/i \"{0}\" /f", backedUpAssemblyPath);
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);

                    OnOutputDataReceived(string.Format("  * Restoring file to GAC. Starting process {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments));
                    OnOutputDataReceived("");

                    process.Start();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                }
                else
                {
                    OnOutputDataReceived(string.Format("  * Restoring file. Copying file from {0} to {1}", backedUpAssemblyPath, originalPath));
                    File.Copy(backedUpAssemblyPath, originalPath, true);
                }
            }
        }


        /// <summary>
        /// Handles the OutputDataReceived event of the process control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance containing the event data.</param>
        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnOutputDataReceived(e.Data);
        }


        /// <summary>
        /// Gets the original assembly path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private static string GetOriginalAssemblyPath(string fileName)
        {
            string originalAssemblyPath = string.Empty;
            string directory = Path.GetDirectoryName(fileName);
            string fileNameWoExtension = Path.GetFileNameWithoutExtension(fileName);

            string infoFileName = Path.Combine(directory, fileNameWoExtension + ".xml");
            if (File.Exists(infoFileName))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(infoFileName);

                XmlNode sourcePathNode = doc.SelectSingleNode("assemblyDetails/originalPath");
                if (sourcePathNode != null)
                {
                    XmlAttribute pathAttribute = sourcePathNode.Attributes["path"];
                    if (pathAttribute != null)
                    {
                        originalAssemblyPath = pathAttribute.Value;
                    }
                }
            }

            return originalAssemblyPath;
        }
        #endregion
    }
}
