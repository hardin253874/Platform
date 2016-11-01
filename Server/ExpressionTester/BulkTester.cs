// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using ExpressionTester.Properties;
using EDC.ReadiNow.Metadata.Tenants;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

namespace ExpressionTester
{
    public partial class BulkTester : Form
    {
        public BulkTester()
        {
            InitializeComponent();
            txtFile.Text = Settings.Default.BulkPath;
            LoadTenants();
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
                            let item = new ExpressionTestForm.TenantEntry() { Name = instance.Name, Id = instance.Id }
                            orderby item.ToString()
                            select item;

                cmbTenant.Items.Clear();
                cmbTenant.Items.Add("[Global]");
                cmbTenant.Items.AddRange(items.ToArray());
            }
            cmbTenant.SelectedIndex = 1;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void SaveSettings()
        {
            Settings.Default.BulkPath = txtFile.Text;
            Settings.Default.Save();
        }

        private void Run()
        {
            if (!File.Exists(txtFile.Text))
            {
                txtResult.Text = "Could not open " + txtFile.Text;
                return;
            }

            //SaveSettings();
            long tenantId = cmbTenant.SelectedIndex == 0
                                    ? 0
                                    : ((ExpressionTestForm.TenantEntry)cmbTenant.Items[cmbTenant.SelectedIndex]).Id;

            StringBuilder sbOK = new StringBuilder();
            StringBuilder sbFail = new StringBuilder();

            Regex re = new Regex(@".*\[TestCase\(\""(.*)\""\)].*");

            int pass = 0;
            int fail = 0;
            
            using (new TenantAdministratorContext(tenantId))
            {
                using (var reader = new StreamReader(txtFile.Text))
                {
                    int pos = 0;
                    while (reader.Peek() != -1)
                    {
                        pos++;
                        string line = reader.ReadLine();
                        if (line.Contains("//[TestCase"))
                            continue;
                        Match match = re.Match(line);
                        if (!match.Success)
                            continue;

                        string testData = match.Groups[1].Value;
                        testData = testData.Replace(@"\t", "\t");
                        testData = testData.Replace(@"\r", "\r");
                        testData = testData.Replace(@"\n", "\n");

                        try
                        {
                            TestHelper.Test(testData);
                            sbOK.AppendLine(string.Format("Line {0} PASSED: {1}", pos, testData));
                            pass++;
                        }
                        catch (Exception ex)
                        {
                            sbFail.AppendLine(string.Format("Line {0} FAILED: {1}", pos, testData));
                            sbFail.AppendLine(ex.Message);
                            fail++;
                        }                        
                    }
                }
            }

            txtResult.Text = sbFail.ToString() + "\r\n" + sbOK.ToString() + string.Format("\r\n\r\nPassed: {0}  Failed: {1}", pass, fail);
        }
    }
}
