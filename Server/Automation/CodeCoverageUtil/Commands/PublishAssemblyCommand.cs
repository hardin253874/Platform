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
    /// This command publishes the specified assembly to the specified publish path.
    /// It will backup any existing assembly to the backup folder.
    /// </summary>
    internal class PublishAssemblyCommand : CommandBase
    {
        #region Fields
        /// <summary>
        /// The path to the assembly to publish.
        /// </summary>
        public string AssemblyFilePath
        {
            get
            {
                return assemblyFilePath;
            }
        }
        protected string assemblyFilePath;


        /// <summary>
        /// The directory to publish the assembly to.
        /// </summary>
        public string PublishDirectory
        {
            get
            {
                return publishDirectory;
            }
        }
        protected string publishDirectory;


        /// <summary>
        /// The directory to backup assemblies
        /// that already exist in the publish directory.
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


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishAssemblyCommand"/> class.
        /// </summary>
        /// <param name="assemblyFilePath">The assembly file path.</param>
        /// <param name="publishDirectory">The publish directory.</param>
        /// <param name="assemblyBackupDirectory">The assembly backup directory.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public PublishAssemblyCommand(string assemblyFilePath, string publishDirectory, string assemblyBackupDirectory, bool mandatory)
        {
            if (string.IsNullOrEmpty(assemblyFilePath))
            {
                throw new ArgumentNullException("assemblyFilePath");
            }

            if (string.IsNullOrEmpty(publishDirectory))
            {
                throw new ArgumentNullException("publishDirectory");
            }            

            this.assemblyFilePath = assemblyFilePath;
            this.publishDirectory = publishDirectory;
            this.assemblyBackupDirectory = assemblyBackupDirectory;
            this.mandatory = mandatory;

            name = string.Format("Publish assembly {0} to {1}", assemblyFilePath, publishDirectory);
        }
        #endregion        


        #region Methods        
        /// <summary>
        /// Publish the specified assembly.
        /// </summary>
        protected override void OnExecute()
        {
            base.OnExecute();            

            if (string.Compare(publishDirectory, @"GAC", true) == 0)
            {
                string assemblyLocation = string.Empty;
                using (AssemblyLoader loader = new AssemblyLoader())
                {
                    assemblyLocation = loader.GetAssemblyLocation(assemblyFilePath);
                }

                if (!string.IsNullOrEmpty(assemblyBackupDirectory) &&
                    !string.IsNullOrEmpty(assemblyLocation) &&
                    File.Exists(assemblyLocation))
                {
                    // Backup file
                    OnOutputDataReceived(string.Format("  * Backing up file. Copying file from {0} to {1}", assemblyLocation, Path.Combine(assemblyBackupDirectory, Path.GetFileName(assemblyFilePath))));
                    File.Copy(assemblyLocation, Path.Combine(assemblyBackupDirectory, Path.GetFileName(assemblyFilePath)), true);

                    // write source location
                    using (XmlWriter srcWriter = XmlWriter.Create(Path.Combine(assemblyBackupDirectory, Path.GetFileNameWithoutExtension(assemblyFilePath) + ".xml")))
                    {
                        srcWriter.WriteStartElement("assemblyDetails");
                        srcWriter.WriteStartElement("originalPath");
                        srcWriter.WriteAttributeString("path", @"GAC");
                        srcWriter.WriteEndElement();
                        srcWriter.WriteEndElement();
                    }
                }                
               
                // Publish to GAC
                Process process = new Process();
                process.StartInfo.FileName = Settings.Default.GacUtilExePath;
                process.StartInfo.Arguments = string.Format("/i \"{0}\" /f", assemblyFilePath);
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);

                OnOutputDataReceived(string.Format("  * Publishing file to GAC. Starting process {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments));
                OnOutputDataReceived("");

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
            else
            {
                if (!Directory.Exists(publishDirectory))
                {
                    Directory.CreateDirectory(publishDirectory);
                }

                string assemblyPublishPath = Path.Combine(publishDirectory, Path.GetFileName(assemblyFilePath));

                // Backup the orignal file
                if (!string.IsNullOrEmpty(assemblyBackupDirectory) &&
                    File.Exists(assemblyPublishPath))
                {
                    OnOutputDataReceived(string.Format("  * Backing up file. Copying file from {0} to {1}", assemblyPublishPath, Path.Combine(assemblyBackupDirectory, Path.GetFileName(assemblyFilePath))));
                    File.Copy(assemblyPublishPath, Path.Combine(assemblyBackupDirectory, Path.GetFileName(assemblyFilePath)), true);

                    // write source location
                    using (XmlWriter srcWriter = XmlWriter.Create(Path.Combine(assemblyBackupDirectory, Path.GetFileNameWithoutExtension(assemblyFilePath) + ".xml")))
                    {
                        srcWriter.WriteStartElement("assemblyDetails");
                        srcWriter.WriteStartElement("originalPath");
                        srcWriter.WriteAttributeString("path", assemblyPublishPath);
                        srcWriter.WriteEndElement();
                        srcWriter.WriteEndElement();
                    }
                }

                OnOutputDataReceived(string.Format("  * Publishing file. Copying file from {0} to {1}", assemblyFilePath, assemblyPublishPath));
                File.Copy(assemblyFilePath, assemblyPublishPath, true);
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
        #endregion
    }
}
