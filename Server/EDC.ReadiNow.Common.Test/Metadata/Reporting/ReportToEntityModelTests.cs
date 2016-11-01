// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata.Reporting;
using NUnit.Framework;
using FluentAssertions;

using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.Metadata.Reporting
{
    /// <summary>
    ///     Tests the conversion of reports to entity model.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class ReportToEntityModelTests
    {
        /// <summary>
        ///     Test that a valid report view can be created.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestTemplateReport()
        {
            string alias = "core:templateReport";
            var report = Entity.Get<Report>(alias);

            // Check report
            CheckResourceReport(report, "Resource", 3);

            // ID column
            var columns = report.ReportColumns.OrderBy(c => c.ColumnDisplayOrder).ToArray();
            CheckIdColumn(columns[0], report.RootNode.Id);
            CheckFieldColumn<StringArgument>(columns[1], "Name", "Name", report.RootNode);
            CheckFieldColumn<StringArgument>(columns[2], "Description", "Description", report.RootNode);
        }

        /// <summary>
        ///     Test that a valid report view can be created.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestANLSimpleChoice()
        {
            string alias = "test:rpt_AnalSimpleChoice";
            var report = Entity.Get<Report>(alias);

            // Check report
            CheckResourceReport(report, "RPT_AllTypesChoice", 13);

            var rootNode = report.RootNode.As<ResourceReportNode>();
            var choiceNode = rootNode.RelatedReportNodes.Single().As<RelationshipReportNode>();
            
            choiceNode.Should().NotBeNull();
            (choiceNode.FollowInReverse == true).Should().BeFalse();

            // Check columns
            var columns = report.ReportColumns.OrderBy(c => c.ColumnDisplayOrder).ToArray();
            CheckIdColumn(columns[0], report.RootNode.Id);
            CheckFieldColumn<StringArgument>(columns[1], "Text Field", "Text Field", report.RootNode);
            CheckFieldColumn<StringArgument>(columns[2], "Multiline Text Field", "Multiline Text Field", report.RootNode);
            CheckFieldColumn<IntegerArgument>(columns[3], "Number Field", "Number Field", report.RootNode);
            CheckFieldColumn<IntegerArgument>(columns[4], "AutoNumber Field", "AutoNumber Field", report.RootNode);
            CheckFieldColumn<DecimalArgument>(columns[5], "Decimal Field", "Decimal Field", report.RootNode);
            CheckFieldColumn<CurrencyArgument>(columns[6], "Currency Field", "Currency Field", report.RootNode);
            CheckFieldColumn<DateTimeArgument>(columns[7], "Date and Time Field", "Date and Time Field", report.RootNode);
            CheckFieldColumn<DateArgument>(columns[8], "Date only Field", "Date only Field", report.RootNode);
            CheckFieldColumn<TimeArgument>(columns[9], "Time only Field", "Time only Field", report.RootNode);
            CheckFieldColumn<BoolArgument>(columns[10], "Yes/No Field", "Yes/No Field", report.RootNode);
            CheckResourceExprColumn(columns[11], "Choice Field", choiceNode.As<ReportNode>(), "RPT_Choice");
            CheckScriptColumn<IntegerArgument>(columns[12], "Calculation", "[Number Field] + [AutoNumber Field]", report.RootNode);

            // Check conditions
            var conds = report.HasConditions.OrderBy(c => c.ConditionDisplayOrder).ToArray();
            CheckCondition<ResourceListArgument>(conds[0], "Choice Field", "RPT_Choice");
            CheckCondition<BoolArgument>(conds[1], "Yes/No Field");
            CheckCondition<TimeArgument>(conds[2], "Time only Field");
            CheckCondition<DateArgument>(conds[3], "Date only Field");
            CheckCondition<DateTimeArgument>(conds[4], "Date and Time Field");
            CheckCondition<CurrencyArgument>(conds[5], "Currency Field");
            CheckCondition<DecimalArgument>(conds[6], "Decimal Field");
            CheckCondition<IntegerArgument>(conds[7], "AutoNumber Field");
            CheckCondition<StringArgument>(conds[8], "Multiline Text Field");
            CheckCondition<IntegerArgument>(conds[9], "Number Field");
            CheckCondition<StringArgument>(conds[10], "Text Field");
            CheckCondition<IntegerArgument>(conds[11], "Calculation");
        }

        /// <summary>
        ///     Test that a valid report view can be created.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestANLSimpleChoiceCond()
        {
            var ireport = CodeNameResolver.GetInstance("ANL Simple Choice Cond", "Report");
            var report = ireport.As<Report>();

            // Check report
            CheckResourceReport(report, "RPT_AllTypesChoice", 12);

            var rootNode = report.RootNode.As<ResourceReportNode>();
            var choiceNode = rootNode.RelatedReportNodes.Single().As<RelationshipReportNode>();
            Assert.IsNotNull(choiceNode);
            Assert.IsFalse(choiceNode.FollowInReverse == true);

            // Check columns
            var columns = report.ReportColumns.OrderBy(c => c.ColumnDisplayOrder).ToArray();
            CheckIdColumn(columns[0], report.RootNode.Id);
            CheckFieldColumn<StringArgument>(columns[1], "Text Field", "Text Field", report.RootNode);
            CheckFieldColumn<StringArgument>(columns[2], "Multiline Text Field", "Multiline Text Field", report.RootNode);
            CheckFieldColumn<IntegerArgument>(columns[3], "Number Field", "Number Field", report.RootNode);
            CheckFieldColumn<IntegerArgument>(columns[4], "AutoNumber Field", "AutoNumber Field", report.RootNode);
            CheckFieldColumn<DecimalArgument>(columns[5], "Decimal Field", "Decimal Field", report.RootNode);
            CheckFieldColumn<CurrencyArgument>(columns[6], "Currency Field", "Currency Field", report.RootNode);
            CheckFieldColumn<DateTimeArgument>(columns[7], "Date and Time Field", "Date and Time Field", report.RootNode);
            CheckFieldColumn<DateArgument>(columns[8], "Date only Field", "Date only Field", report.RootNode);
            CheckFieldColumn<TimeArgument>(columns[9], "Time only Field", "Time only Field", report.RootNode);
            CheckFieldColumn<BoolArgument>(columns[10], "Yes/No Field", "Yes/No Field", report.RootNode);
            CheckResourceExprColumn(columns[11], "Choice Field", choiceNode.As<ReportNode>(), "RPT_Choice");

            // Check conditions
            var conds = report.HasConditions.OrderBy(c => c.ConditionDisplayOrder).ToArray();
            CheckCondition<ResourceListArgument>(conds[0], "Choice Field", "RPT_Choice");
            CheckCondition<BoolArgument>(conds[1], "Yes/No Field");
            CheckCondition<TimeArgument>(conds[2], "Time only Field");
            CheckCondition<DateArgument>(conds[3], "Date only Field");
            CheckCondition<DateTimeArgument>(conds[4], "Date and Time Field");
            CheckCondition<CurrencyArgument>(conds[5], "Currency Field");
            CheckCondition<DecimalArgument>(conds[6], "Decimal Field");
            CheckCondition<IntegerArgument>(conds[7], "AutoNumber Field");
            CheckCondition<StringArgument>(conds[8], "Multiline Text Field");
            CheckCondition<IntegerArgument>(conds[9], "Number Field");
            CheckCondition<StringArgument>(conds[10], "Text Field");
        }

        /// <summary>
        ///     Test that a valid report view can be created.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestNSWPopulation()
        {
            var ireport = CodeNameResolver.GetInstance("NSW Population", "Report");
            var report = ireport.As<Report>();

            // Check report
            CheckResourceReport(report, "Population", 5);
            Assert.IsNotNull(report.RootNode.As<ResourceReportNode>());

            // Check columns
            var columns = report.ReportColumns.OrderBy(c => c.ColumnDisplayOrder).ToArray();
            CheckIdColumn(columns[0], report.RootNode.Id);
            CheckFieldColumn<DateArgument>(columns[1], "Month", "Month", report.RootNode);
            CheckFieldColumn<IntegerArgument>(columns[2], "Male", "Male", report.RootNode);
            CheckFieldColumn<IntegerArgument>(columns[3], "Female", "Female", report.RootNode);
            CheckScriptColumn<IntegerArgument>(columns[4], "Combined", "Male+Female", report.RootNode);

            // Check conditions
            var conds = report.HasConditions.OrderBy(c => c.ConditionDisplayOrder).ToArray();
            CheckCondition<StringArgument>(conds[0], "State");   // currently returning string field??
            CheckCondition<DateArgument>(conds[1], "Month");
            CheckCondition<IntegerArgument>(conds[2], "Male");
            CheckCondition<IntegerArgument>(conds[3], "Female");
            CheckCondition<IntegerArgument>(conds[4], "Calculation");   // "Calculation" is what's in the analyzer XML. No idea why it's showing "Combined" in SL instead??
            Assert.AreEqual("NSW",
                            conds[0].ConditionParameter.ParamTypeAndDefault.As<StringArgument>().StringParameterValue);
        }

        /// <summary>
        ///     Test that a valid report view can be created.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestResultTypeOfGroupByColumn()
        {
            var ireport = CodeNameResolver.GetInstance("rpt_CHART_EmployeeManager", "Report");
            var report = ireport.As<Report>();

            Assert.IsTrue(report.ReportColumns[0].ColumnExpression.ReportExpressionResultType.Is<ResourceArgument>());
            Assert.AreEqual("AA_Manager", report.ReportColumns[0].ColumnExpression.ReportExpressionResultType.As<ResourceArgument>().ConformsToType.Name);
            Assert.IsTrue(report.ReportColumns[1].ColumnExpression.ReportExpressionResultType.Is<IntegerArgument>());
        }

        private static void CheckCondition<TArg>(ReportCondition condition, string name, string expectedTypeName = null) where TArg : class,IEntity
        {
            Assert.IsNotNull(condition);
            Assert.AreEqual(name, condition.Name);
            Assert.IsNotNull(condition.ConditionExpression);
            Assert.IsNotNull(condition.ConditionExpression.ReportExpressionResultType);
            Assert.IsTrue(condition.ConditionExpression.ReportExpressionResultType.Is<TArg>());
            if (expectedTypeName != null)
            {
                var rla = condition.ConditionExpression.ReportExpressionResultType.As<ResourceListArgument>();
                Assert.IsNotNull(rla, "ResourceListArgument");
                Assert.IsNotNull(rla.ConformsToType, "Conforms to type");
                Assert.AreEqual(expectedTypeName, rla.ConformsToType.Name, "Conforms to type");
            }
        }
        
        private static void CheckResourceReport(Report report, string typeName, int colCount)
        {
            Assert.IsTrue(report.RootNode.Is<ResourceReportNode>());
            var node = report.RootNode.As<ResourceReportNode>();
            Assert.IsTrue(node.ResourceReportNodeType.Name == typeName);
            Assert.AreEqual(colCount, report.ReportColumns.Count, "Columns");
        }

        private static void CheckIdColumn(ReportColumn column, long nodeId)
        {
            Assert.IsTrue(column.Name == null);
            Assert.IsTrue(column.ColumnIsHidden == true);
            Assert.IsTrue(column.ColumnExpression.Is<EDC.ReadiNow.Model.IdExpression>());
            Assert.IsTrue(column.ColumnExpression.ReportExpressionResultType.Is<ResourceArgument>());
        }

        private static void CheckFieldColumn<TArg>(ReportColumn column, string colName, string fieldName, ReportNode node) where TArg : class,IEntity
        {
            Assert.AreEqual(colName, column.Name, "Col name");
            Assert.IsTrue(column.ColumnIsHidden == false, "Is hidden");
            Assert.IsTrue(column.ColumnExpression.Is<FieldExpression>(), "Is field expr");
            Assert.IsTrue(column.ColumnExpression.ReportExpressionResultType.Is<TArg>(), "Correct column type");
            var nameExpr = column.ColumnExpression.As<FieldExpression>();
            Assert.AreEqual(fieldName, nameExpr.FieldExpressionField.Name, "Correct field");
            Assert.AreEqual(node.Id, nameExpr.SourceNode.Id);
        }

        private static void CheckResourceExprColumn(ReportColumn column, string colName, ReportNode node, string expectedTypeName)
        {
            Assert.AreEqual(colName, column.Name, "Col name");
            Assert.IsTrue(column.ColumnIsHidden == false, "Is hidden");
            Assert.IsTrue(column.ColumnExpression.Is<EDC.ReadiNow.Model.ResourceExpression>(), "Is res expr");
            Assert.IsTrue(column.ColumnExpression.ReportExpressionResultType.Is<ResourceArgument>());
            var nameExpr = column.ColumnExpression.As<EDC.ReadiNow.Model.ResourceExpression>();
            Assert.AreEqual(node.Id, nameExpr.SourceNode.Id);

            var resourceArg = column.ColumnExpression.ReportExpressionResultType.As<ResourceArgument>();
            Assert.IsNotNull(resourceArg.ConformsToType, "Conforms to type");
            Assert.AreEqual(expectedTypeName, resourceArg.ConformsToType.Name, "Conforms to type");
        }

        private static void CheckScriptColumn<TArg>(ReportColumn column, string colName, string script, ReportNode node) where TArg : class,IEntity
        {
            Assert.AreEqual(colName, column.Name, "Col name");
            Assert.IsTrue(column.ColumnIsHidden == false, "Is hidden");
            Assert.IsTrue(column.ColumnExpression.Is<EDC.ReadiNow.Model.ScriptExpression>(), "Is script expr");
            Assert.IsTrue(column.ColumnExpression.ReportExpressionResultType.Is<TArg>());
            var nameExpr = column.ColumnExpression.As<EDC.ReadiNow.Model.ScriptExpression>();
            Assert.IsTrue(nameExpr.ReportScript == script);
            Assert.AreEqual(node.Id, nameExpr.SourceNode.Id);
        }
    }
}