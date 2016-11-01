// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Diagnostics;
using System.IO;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EDC.ReadiNow.Model;
using ReadiNow.DocGen;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Core;

namespace DocGenTester
{
    public partial class MainForm : Form
    {
        internal class Settings
        {
            public string ContextType;
            public string ContextResource;
            public string TemplateFile;
            public string TargetFolder;
            public bool AutomaticallyOpen;
            public long TenantId;
        }

        internal class TenantEntry
        {
            public string Name { get; set; }
            public long Id { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            LoadTenants();

            lblMessage.Text = "";
            txtTemplateFile.Text = Properties.Settings.Default.TemplateFile;
            txtOutputFolder.Text = Properties.Settings.Default.OutputFolder;
            txtContextType.Text = Properties.Settings.Default.ContextType;
            txtContextResource.Text = Properties.Settings.Default.ContextResource;
            chkOpenAutomatically.Checked = Properties.Settings.Default.OpenAutomatically;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = txtTemplateFile.Text;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTemplateFile.Text = openFileDialog.FileName;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.ContextResource = txtContextResource.Text;
            Properties.Settings.Default.ContextType = txtContextType.Text;
            Properties.Settings.Default.TemplateFile = txtTemplateFile.Text;
            Properties.Settings.Default.OutputFolder = txtOutputFolder.Text;
            Properties.Settings.Default.OpenAutomatically = chkOpenAutomatically.Checked;
            Properties.Settings.Default.Save();
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            SaveSettings();

            // Validate settings
            if (string.IsNullOrEmpty(txtTemplateFile.Text))
            {
                MessageBox.Show("Please specify a template file.");
                return;
            }

            if (string.IsNullOrEmpty(txtOutputFolder.Text))
            {
                MessageBox.Show("Please specify a target folder.");
                return;
            }

            if (!File.Exists(txtTemplateFile.Text))
            {
                MessageBox.Show("The template file could not be found.");
                return;
            }

            try
            {
                // Start background worker
                long tenantId = cmbTenant.SelectedIndex == 0
                                    ? 0
                                    : ((TenantEntry) cmbTenant.Items[cmbTenant.SelectedIndex]).Id;

                lblMessage.Text = "Starting...";
                var settings = new Settings
                    {
                        TargetFolder = txtOutputFolder.Text,
                        TemplateFile = txtTemplateFile.Text,
                        AutomaticallyOpen = chkOpenAutomatically.Checked,
                        TenantId = tenantId,
                        ContextType = txtContextType.Text,
                        ContextResource = txtContextResource.Text
                    };
                backgroundWorker.RunWorkerAsync(settings);
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message);
                lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                lblMessage.Text = "";
            }
        }

        private void Run(string source, string targetFolder, bool open, long tenantId)
        {
            string file = Path.GetFileName(source);
            string tmp = Path.GetTempFileName();

            // Prepare files
            if (!File.Exists(source))
                throw new ApplicationException("Source file does not exist");

            string target;
            try
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);

                // Prepare target folder
                if (!Path.IsPathRooted(targetFolder))
                {
                    string sourceFolder = Path.GetDirectoryName(source);
                    targetFolder = Path.Combine(sourceFolder, targetFolder);
                }
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                target = Path.Combine(targetFolder, file);

                if (File.Exists(target))
                    File.Delete(target);

                File.Copy(source, tmp);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not write target file.\r\nThe folder may not exist or the file may be already open.", ex);
            }

            try
            {

                using (new TenantAdministratorContext(tenantId))
                {
                    // Get context
                    IEntity context = null;
                    if (!string.IsNullOrEmpty(txtContextType.Text))
                    {
                        long contextTypeId = Factory.ScriptNameResolver.GetTypeByName(txtContextType.Text);
                        if (contextTypeId == 0)
                        {
                            throw new ApplicationException("Cannot resolve context type: " + txtContextType.Text);
                        }
                        else if (!string.IsNullOrEmpty(txtContextResource.Text))
                        {
                            context = CodeNameResolver.GetInstance(txtContextResource.Text, txtContextType.Text);
                            if (context == null)
                            {
                                throw new ApplicationException("Cannot resolve context resource: " + txtContextType.Text);
                            }
                        }
                    }

                    // Generate document
                    using (Stream templateStream = File.Open(tmp, FileMode.Open, FileAccess.Read))
                    using (Stream targetStream = File.Create(target))
                    {
                        GeneratorSettings genSettings = new GeneratorSettings
                        {
                            WriteDebugFiles = true,
                            CurrentActivityCallback = message => backgroundWorker.ReportProgress(0, message),
                            TimeZoneName = TimeZoneHelper.SydneyTimeZoneName,
                            SelectedResourceId = (context == null ? 0 : context.Id)
                        };

                        Factory.DocumentGenerator.CreateDocument(templateStream, targetStream, genSettings);
                    }
                }
                backgroundWorker.ReportProgress(0, "Generation complete");

                // Open document
                if (open)
                {
                    Process.Start(target);
                }
                else
                {
                    MessageBox.Show("Done\n" + target);
                }
            }
            finally
            {
                // Tidy up
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblMessage.Text = "";
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var settings = (Settings)e.Argument;

            if (Debugger.IsAttached)
            {
                Run(settings.TemplateFile, settings.TargetFolder, settings.AutomaticallyOpen, settings.TenantId);
            }
            else
            {
                try
                {
                    Run(settings.TemplateFile, settings.TargetFolder, settings.AutomaticallyOpen, settings.TenantId);
                }
                catch (ApplicationException ex)
                {
                    MessageBox.Show(ex.Message, "Test-app Error");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Platform Error");
                }
            }            
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblMessage.Text = e.UserState as string;
        }



        /// <summary>
        /// Populate the tenants combo box.
        /// </summary>
        private void LoadTenants()
        {
            using (new AdministratorContext())
            {
                var tenants = TenantHelper.GetAll();

                var items = from instance in tenants
                            let item = new TenantEntry() { Name = instance.Name, Id = instance.Id }
                            orderby item.ToString()
                            select item;

                cmbTenant.Items.Clear();
                cmbTenant.Items.Add("[Global]");
                cmbTenant.Items.AddRange(items.ToArray());
            }
            cmbTenant.SelectedIndex = 1;
        }
    }
}
