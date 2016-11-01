// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using ExpressionTester.Properties;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security;

namespace ExpressionTester
{
    public partial class ExpressionTestForm : Form
    {
        public ExpressionTestForm()
        {
            InitializeComponent();
            LoadTenants();

            txtContextResource.Text = Settings.Default.ContextResource;
            txtContextType.Text = Settings.Default.ContextType;
            chkForceString.Checked = Settings.Default.AsString;
            txtFormula.Text = Settings.Default.Script;
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

        private void btnRun_Click(object sender, EventArgs e)
        {
            Run(false);
        }

        private void Run(bool runReport)
        {
            SaveSettings();
            long tenantId = cmbTenant.SelectedIndex == 0
                                ? 0
                                : ((TenantEntry) cmbTenant.Items[cmbTenant.SelectedIndex]).Id;

            StringBuilder sb = new StringBuilder();

            try
            {
                using (new TenantAdministratorContext(tenantId))
                using (new SecurityBypassContext())
                {
                    // Get context
                    long contextTypeId = 0;
                    IEntity context = null;
                    if (!string.IsNullOrEmpty(txtContextType.Text))
                    {
                        contextTypeId = Factory.ScriptNameResolver.GetTypeByName(txtContextType.Text);
                        if (contextTypeId == 0)
                        {
                            sb.AppendLine("Cannot resolve context type: " + txtContextType.Text);
                            return;
                        }
                        else if (!string.IsNullOrEmpty(txtContextResource.Text))
                        {
                            context = CodeNameResolver.GetInstance(txtContextResource.Text, txtContextType.Text);
                            if (context == null)
                            {
                                sb.AppendLine("Cannot resolve context resource: " + txtContextType.Text);
                                return;
                            }
                        }
                    }

                    // Compile
                    BuilderSettings bsettings = new BuilderSettings();
                    if (contextTypeId != 0)
                        bsettings.RootContextType = ExprTypeHelper.EntityOfType(new EntityRef(contextTypeId));
                    if (runReport)
                        bsettings.ScriptHost = ScriptHostType.Report;
                    IExpression expr = Factory.ExpressionCompiler.Compile(txtFormula.Text, bsettings);
                    sb.AppendLine("Type: " + expr.ResultType);


                    if (runReport)
                    {
                        RunReport(sb, contextTypeId, expr);
                    }
                    else
                    {
                        expr = RunEval(sb, context, bsettings, expr);
                    }
                }
            }
            catch (ParseException ex)
            {
                sb.AppendLine("Script error:");
                sb.AppendLine(ex.Message);
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }
            finally
            {
                txtResult.Text = sb.ToString();
            }
        }

        private static void RunReport(StringBuilder sb, long contextType, IExpression expr)
        {
            // Build structured query
            ResourceEntity re = new ResourceEntity();
            re.EntityTypeId = new EntityRef(contextType);
            StructuredQuery sq = new StructuredQuery { RootEntity = re };

            // Convert script to query
            QueryBuilderSettings qsettings = new QueryBuilderSettings();
            qsettings.StructuredQuery = sq;
            qsettings.ContextEntity = re;
            ScalarExpression resExpr = Factory.ExpressionCompiler.CreateQueryEngineExpression( expr, qsettings );
            sb.AppendLine("OK");

            // Render XML
            sb.AppendLine("\nXML:");
            sb.AppendLine(StructuredQueryHelper.ToXml(sq));
            
            // Render SQL
            sb.AppendLine("\nSQL:");
            sq.SelectColumns.Add(new SelectColumn { Expression = resExpr });
            sb.AppendLine("declare @tenant as bigint = (select min(Id) from _vTenant) -- test harness");
            string sql = EDC.ReadiNow.Metadata.Query.Structured.Builder.QueryBuilder.GetSql(sq);
            sb.AppendLine(sql);
        }

        private IExpression RunEval(StringBuilder sb, IEntity context, BuilderSettings bsettings, IExpression expr)
        {
            // Evaluate
            EvaluationSettings esettings = new EvaluationSettings();
            esettings.TimeZoneName = TimeZoneHelper.SydneyTimeZoneName;
            if (context != null)
                esettings.ContextEntity = context;
            object res = Factory.ExpressionRunner.Run( expr, esettings ).Value;

            // Cast result to string
            if (!expr.ResultType.IsList && chkForceString.Checked)
            {
                bsettings.ExpectedResultType = new ExprType(EDC.Database.DataType.String);
                expr = Factory.ExpressionCompiler.Compile( txtFormula.Text, bsettings );
            }

            // Display result
            if (res is IEnumerable<IEntity>)
            {
                // Resource list
                var arr = ((IEnumerable<IEntity>)res).ToArray();
                if (arr.Length == 0)
                    sb.AppendLine("<empty>");
                foreach (IEntity en in (IEnumerable<IEntity>)res)
                    sb.AppendLine(en == null ? "null" : en.As<Resource>().Name);
            }
            else if (res is IEnumerable && !(res is string))
            {
                // Scalar list
                var arr = ((IEnumerable)res).OfType<object>().ToArray();
                if (arr.Length == 0)
                    sb.AppendLine("<empty>");
                foreach (object o in arr)
                    sb.AppendLine(o == null ? "null" : o.ToString());
            }
            else
            {
                // Scalar
                sb.Append("Result: ");
                sb.AppendLine(res == null ? "null" : res.ToString());
            }
            return expr;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Settings.Default.ContextResource = txtContextResource.Text;
            Settings.Default.ContextType = txtContextType.Text;
            Settings.Default.AsString = chkForceString.Checked;
            Settings.Default.Script = txtFormula.Text;
            Settings.Default.Save();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BulkTester bt = new BulkTester();
            bt.Show();
        }

        private void btnClearResult_Click(object sender, EventArgs e)
        {
            txtResult.Text = "";
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            Run(true);
        }
    }
}
