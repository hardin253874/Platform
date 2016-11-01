// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.EntityRequests;
using ReadiNow.EntityGraph;

namespace ReadiNow.EntityGraph.Test.Parser
{
    [TestFixture]
	[RunWithTransaction]
    public class RequestParserTests
    {
        [Test]
        public void Star()
        {
            var rq = EntityRequestHelper.BuildRequest("*", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.AllFields);
            Assert.IsTrue(rq.Relationships.Count == 0, "Relationships");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TwoFields()
        {
            var rq = EntityRequestHelper.BuildRequest("name, description");
            Assert.IsTrue(rq.Fields.Count == 2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void SimpleAliasRelation()
        {
            var rq = EntityRequestHelper.BuildRequest("isOfType.name");
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual("isOfType", rq.Relationships[0].RelationshipTypeId.Alias);
            Assert.IsFalse(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 1);
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReverseAliasRelation()
        {
            var rq = EntityRequestHelper.BuildRequest("-isOfType.name", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual("isOfType", rq.Relationships[0].RelationshipTypeId.Alias);
            Assert.IsTrue(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 1);
        }

        [Test]
        [RunAsDefaultTenant]
        public void SimpleIdRelation()
        {
            var rq = EntityRequestHelper.BuildRequest("#123.name", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual(123, rq.Relationships[0].RelationshipTypeId.Id);
            Assert.IsFalse(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 1);
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReverseIdRelation()
        {
            var rq = EntityRequestHelper.BuildRequest("-#123.name", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual(123, rq.Relationships[0].RelationshipTypeId.Id);
            Assert.IsTrue(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 1);
        }

        [Test]
        [RunAsDefaultTenant]
        public void SimpleAliasRelationMetadata()
        {
            var rq = EntityRequestHelper.BuildRequest("isOfType.?");
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual("isOfType", rq.Relationships[0].RelationshipTypeId.Alias);
            Assert.IsFalse(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 0);
            Assert.IsTrue(rq.Relationships[0].MetadataOnly);
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReverseAliasRelationMetadata()
        {
            var rq = EntityRequestHelper.BuildRequest("-isOfType.?", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual("isOfType", rq.Relationships[0].RelationshipTypeId.Alias);
            Assert.IsTrue(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 0);
            Assert.IsTrue(rq.Relationships[0].MetadataOnly);
        }

        [Test]
        [RunAsDefaultTenant]
        public void SimpleIdRelationMetadata()
        {
            var rq = EntityRequestHelper.BuildRequest("#123.?", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual(123, rq.Relationships[0].RelationshipTypeId.Id);
            Assert.IsFalse(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 0);
            Assert.IsTrue(rq.Relationships[0].MetadataOnly);
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReverseIdRelationMetadata()
        {
            var rq = EntityRequestHelper.BuildRequest("-#123.?", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.AreEqual(123, rq.Relationships[0].RelationshipTypeId.Id);
            Assert.IsTrue(rq.Relationships[0].IsReverse);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 0);
            Assert.IsTrue(rq.Relationships[0].MetadataOnly);
        }

        [Test]
        [RunAsDefaultTenant]
        public void IdRelationshipInBothDirections()
        {
            var rq = EntityRequestHelper.BuildRequest("#123.id,-#123.id", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 2);
            Assert.AreEqual(123, rq.Relationships[0].RelationshipTypeId.Id);
            Assert.IsTrue(!rq.Relationships[0].IsReverse);
            Assert.AreEqual(123, rq.Relationships[1].RelationshipTypeId.Id);
            Assert.IsTrue(rq.Relationships[1].IsReverse);
        }

        [Test]
        [RunAsDefaultTenant]
        public void RelationTwoFields()
        {
            var rq = EntityRequestHelper.BuildRequest("isOfType.{name,description}");
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 2);
        }

        [Test]
        public void TwoRelationTwoFields()
        {
            var rq = EntityRequestHelper.BuildRequest("{isOfType,whatever}.{name,description} /* with a comment */", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 2);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[1].RequestedMembers.Fields.Count == 2);
        }

        [Test]
        public void NestedChain()
        {
            var rq = EntityRequestHelper.BuildRequest("{isOfType,somethingElse.inherits*}.{name,description}", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 2);
            Assert.IsTrue(rq.Relationships[1].RequestedMembers.Relationships.Count == 1);
            Assert.IsTrue(rq.Relationships[1].RequestedMembers.Relationships[0].IsRecursive);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[1].RequestedMembers.Relationships[0].RequestedMembers.Fields.Count == 2);
        }

        [Test]
        [RunAsDefaultTenant]
        public void NestedChainWithMerge()
        {
            // isOfType, and isOfType.inherits get merged into the same instance of isOfType.
            // (this may change in the future)
            var rq = EntityRequestHelper.BuildRequest("{isOfType,isOfType.inherits*}.{name,description}");
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships.Count == 1);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships[0].IsRecursive);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships[0].RequestedMembers.Fields.Count == 2);
        }

        [Test]
        public void NestedLists()
        {
            var rq = EntityRequestHelper.BuildRequest("{a.{b,c,d},e}.{f,g}", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 2);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships.Count == 3);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships[0].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships[1].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships[2].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[1].RequestedMembers.Fields.Count == 2);
        }

        [Test]
        public void BasicMerge()
        {
            var rq = EntityRequestHelper.BuildRequest("a.b.c, a.b.d, a.e", RequestParserSettings.NoVerify);
            Assert.IsTrue(rq.Relationships.Count == 1);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships.Count == 1);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Relationships[0].RequestedMembers.Fields.Count == 2);
            Assert.IsTrue(rq.Relationships[0].RequestedMembers.Fields.Count == 1);
        }

        [Test]
        public void Variables_Simple()
        {
            var rq = EntityRequestHelper.BuildRequest("let @VAR = {name} @VAR", RequestParserSettings.NoVerify);
            Assert.That(rq, Is.Not.Null);
            Assert.That(rq, Has.Property("Relationships").Empty);
            Assert.That(rq, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(rq.Fields[0].Alias, Is.EqualTo("name"));
        }

        [Test]
        public void Variables_FollowRel()
        {
            var rq = EntityRequestHelper.BuildRequest("let @VAR = {name} followRel.@VAR", RequestParserSettings.NoVerify);
            
            Assert.That(rq, Is.Not.Null);
            Assert.That(rq, Has.Property("Relationships").Count.EqualTo(1));

            var relRq = rq.Relationships[0].RequestedMembers;
            Assert.That(relRq, Has.Property("Relationships").Empty);
            Assert.That(relRq, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(relRq.Fields[0].Alias, Is.EqualTo("name"));
        }

        [Test]
        public void Variables_VarHasRel()
        {
            var rq = EntityRequestHelper.BuildRequest("let @VAR = {followRel2.name} followRel.@VAR", RequestParserSettings.NoVerify);

            Assert.That(rq, Is.Not.Null);
            Assert.That(rq, Has.Property("Relationships").Count.EqualTo(1));

            var rel = rq.Relationships[0];
            var relRq = rel.RequestedMembers;
            Assert.That(rel.RelationshipTypeId.Alias, Is.EqualTo("followRel"));
            Assert.That(relRq, Has.Property("Relationships").Count.EqualTo(1));

            var rel2 = relRq.Relationships[0];
            Assert.That(rel2.RelationshipTypeId.Alias, Is.EqualTo("followRel2"));
            var relRq2 = rel2.RequestedMembers;
            Assert.That(relRq2, Has.Property("Relationships").Empty);
            Assert.That(relRq2, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(relRq2.Fields[0].Alias, Is.EqualTo("name"));
        }

        [Test]
        public void Variables_TwoVariables()
        {
            var rq = EntityRequestHelper.BuildRequest("let @VAR1 = { field1 } let @VAR2 = { field2 } rel1.@VAR1, rel2.@VAR2", RequestParserSettings.NoVerify);

            Assert.That(rq, Is.Not.Null);
            Assert.That(rq, Has.Property("Relationships").Count.EqualTo(2));

            var rel = rq.Relationships[0];
            var relRq = rel.RequestedMembers;
            Assert.That(rel.RelationshipTypeId.Alias, Is.EqualTo("rel1"));
            Assert.That(relRq, Has.Property("Relationships").Empty);
            Assert.That(relRq, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(relRq.Fields[0].Alias, Is.EqualTo("field1"));

            var rel2 = rq.Relationships[1];
            var relRq2 = rel2.RequestedMembers;
            Assert.That(rel2.RelationshipTypeId.Alias, Is.EqualTo("rel2"));
            Assert.That(relRq2, Has.Property("Relationships").Empty);
            Assert.That(relRq2, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(relRq2.Fields[0].Alias, Is.EqualTo("field2"));
        }

        [Test]
        public void Variables_TwoDependentVariables()
        {
            var rq = EntityRequestHelper.BuildRequest("let @VAR1 = { rel12.@VAR2, field1 } let @VAR2 = { rel21.@VAR1, field2 } @VAR1", RequestParserSettings.NoVerify);

            var var1rq = rq;
            Assert.That(var1rq, Is.Not.Null);
            Assert.That(var1rq, Has.Property("Relationships").Count.EqualTo(1));
            Assert.That(var1rq, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(var1rq.Fields[0].Alias, Is.EqualTo("field1"));            

            var rel12 = rq.Relationships[0];
            var var2rq = rel12.RequestedMembers;
            Assert.That(rel12.RelationshipTypeId.Alias, Is.EqualTo("rel12"));
            Assert.That(var2rq, Has.Property("Relationships").Count.EqualTo(1));
            Assert.That(var2rq, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(var2rq.Fields[0].Alias, Is.EqualTo("field2"));

            var rel21 = var2rq.Relationships[0];
            var var1rqB = rel21.RequestedMembers;
            Assert.That(rel21.RelationshipTypeId.Alias, Is.EqualTo("rel21"));
            Assert.That(var1rqB, Is.EqualTo(var1rq));            
        }

        [Test]
        public void Variables_SelfReference()
        {
            var rq = EntityRequestHelper.BuildRequest("let @TYPE = { name, inherits.@TYPE } @TYPE", RequestParserSettings.NoVerify);

            Assert.That(rq, Is.Not.Null);
            Assert.That(rq, Has.Property("Relationships").Count.EqualTo(1));
            Assert.That(rq, Has.Property("Fields").Count.EqualTo(1));
            Assert.That(rq.Fields[0].Alias, Is.EqualTo("name"));

            var rqRel = rq.Relationships[0];
            Assert.That(rqRel.RelationshipTypeId.Alias, Is.EqualTo("inherits"));
            Assert.That(rqRel.RequestedMembers, Is.EqualTo(rq));
        }

        [Test]
        [RunAsDefaultTenant]
        public void ConsoleTreeTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                    alias,
                    k:folderContents*.
                    {
	                    name,
	                    k:consoleOrder,
	                    k:shortcuts.{ name, k:consoleOrder },
	                    {
                            // Ways of getting to behaviors
		                    k:resourceConsoleBehavior,
		                    isOfType.k:typeConsoleBehavior,
		                    k:shortcuts.k:resourceConsoleBehavior,
		                    k:shortcuts.isOfType.k:typeConsoleBehavior
	                    }.{
                            // Properties to load for each behavior
		                    k:treeIconUrl
	                    }
                    }");
        }

        [Test]
        [RunAsDefaultTenant]
        public void DashboardCellConfigureQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                name,
                description,
                alias,
                isOfType.name,
                { relationships, reverseRelationships }.{ name, alias, { toType, fromType}.{ name, alias } }
                ");
        }

        [Test]
        [RunAsDefaultTenant]
        public void NameFieldConstraintQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"alias, isRequired, maxLength, minLength, pattern.{name, regex}");
        }

        [Test]
        [RunAsDefaultTenant]
        public void RelationshipControlsQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"instancesOfType.{ name, k:control, k:designControl }");
        }

        [Test]
        [RunAsDefaultTenant]
        public void FolderPickerNodeItemQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                    alias,
                    k:folderContents*.
	                    {
		                    // tree item info
		                    name,
		                    k:consoleOrder,
		                    { k:resourceConsoleBehavior, isOfType.k:typeConsoleBehavior }.
			                    {
				                    // console behavior info
				                    k:treeIconUrl
			                    }
	                    }");
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReportBuilderQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                  name,
                  alias,
                  {toType, fromType}.{alias, name},
                  fromName,
                  toName,
                  hideOnFromType
                ");
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReportBuilderQueryTest2()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                    name,
                    description,
                    alias,
                    {relationships, reverseRelationships}.
	                    {
		                    name,
		                    alias,
		                    {toType, fromType}.{alias, name},
		                    fromName,
		                    toName,
		                    hideOnFromType
	                    }
                    ");
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReportBuilderQueryTest3()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                name,
                alias,
                fieldInGroup.{name,alias},
                isOfType.{alias,readiNowType},
                {toType,fromType}.name");
        }

        [Test]
        [RunAsDefaultTenant]
        public void EditFormBuilderViewModelTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                name,
                    description,
                    alias,
                    inherits.alias,
                    { fields, inherits*.fields }.
	                    {
		                    name,
		                    alias,
		                    fieldInGroup.name,
		                    isOfType.alias,
		                    isOfType.readiNowType
	                    }");
        }

        [Test]
        [RunAsDefaultTenant]
        public void ScreenReportBuilderQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                name,
                description,
                alias,
                {relationships,reverseRelationships}.
	                {
		                name,
		                alias,
		                {toType,fromType}.{name,alias}
	                }");
        }

        [Test]
        [RunAsDefaultTenant]
        public void ReportColumnPickerQueryTest()
        {
            var rq = EntityRequestHelper.BuildRequest(@"
                name,
                    description,
                    alias,
                    inherits*.alias,
                    {fields, inherits*.fields}.
	                    {
		                    name,
		                    alias,
		                    fieldInGroup.{alias, name},
		                    isOfType.{alias, readiNowType}
	                    }");
        }
         
        
        
        


        
    }
}
