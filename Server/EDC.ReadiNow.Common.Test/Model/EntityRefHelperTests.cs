// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
    /// <summary>
    ///     EntityAlias tests.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class EntityRefHelperTests
    {
        /// <summary>
        ///     Tests the EntityRef XML visitor
        /// </summary>
        [Test]
        public void TestVisitEntityRefTextNodes()
        {
            string before = @"<?xml version=""1.0""?>
<Query xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://enterprisedata.com.au/readinow/v2/query/2.0"">
  <Conditions>
    <Condition>
      <Expression>
        <NodeId>b297bec5-45d4-4a58-a227-738ecd9f068a</NodeId>
        <Tag1 entityRef=""true"">core:name</Tag1>
        <Tag2 entityRef=""true""></Tag2>
        <Tag3 entityRef=""true""/>
        <Tag4 entityRef=""true"">12345</Tag4>
        <Tag5 entityRef=""true"">12345</Tag5>
        <TypedValue type=""ChoiceRelationship"" entityRef=""true"">25167<SourceEntityTypeId entityRef=""true"">24704</SourceEntityTypeId></TypedValue>
        <arguments>
          <argument type=""ChoiceRelationship"" entityRef=""true"">4416<SourceEntityTypeId entityRef=""true"">4533</SourceEntityTypeId></argument>
        </arguments>
      </Expression>
    </Condition>
  </Conditions>
</Query>";
            string expected = @"<?xml version=""1.0""?>
<Query xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://enterprisedata.com.au/readinow/v2/query/2.0"">
  <Conditions>
    <Condition>
      <Expression>
        <NodeId>b297bec5-45d4-4a58-a227-738ecd9f068a</NodeId>
        <Tag1 entityRef=""true"">*core:name*</Tag1>
        <Tag2 entityRef=""true""></Tag2>
        <Tag3 entityRef=""true""/>
        <Tag4 entityRef=""true"">*12345*</Tag4>
        <Tag5 entityRef=""true"">*12345*</Tag5>
        <TypedValue type=""ChoiceRelationship"" entityRef=""true"">*25167*<SourceEntityTypeId entityRef=""true"">*24704*</SourceEntityTypeId></TypedValue>
        <arguments>
          <argument type=""ChoiceRelationship"" entityRef=""true"">*4416*<SourceEntityTypeId entityRef=""true"">*4533*</SourceEntityTypeId></argument>
        </arguments>
      </Expression>
    </Condition>
  </Conditions>
</Query>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(before);

            EntityRefHelper.VisitEntityRefTextNodes(doc, node => { node.Value = "*" + node.Value + "*"; });
            string actual = doc.OuterXml;

            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(expected);
            string expected2 = doc2.OuterXml;

            Assert.AreEqual(expected2, actual);

            
        }

    }
}