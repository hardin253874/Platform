// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Tree.Nodes;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Core;
using ReadiNow.Expressions.Tree;

namespace ReadiNow.Expressions.Test.Builder
{
    [TestFixture]
	[RunWithTransaction]
    public class StaticBuilderTests
    {

        [TestCase("Name", Result = "GetRootContextEntityNode")]
        [TestCase("context()", Result = "GetRootContextEntityNode(GetRootContextEntityNode)")]  // hmm
        [TestCase("Manager.Name", Result = "GetRootContextEntityNode(AccessRelationshipNode)")]
        [TestCase("Manager.Manager.Name", Result = "GetRootContextEntityNode(AccessRelationshipNode(AccessRelationshipNode))")]
        [TestCase("Manager.Name + Manager.Name", Result = "GetRootContextEntityNode(AccessRelationshipNode)")]
        [TestCase("let m = Manager select m.Name", Result = "GetRootContextEntityNode(AccessRelationshipNode)")]
        [TestCase("let m = Manager select m.Name + Name", Result = "GetRootContextEntityNode(AccessRelationshipNode)")]
        [TestCase("let m = Manager let x=m select x.Name", Result = "GetRootContextEntityNode(AccessRelationshipNode)")]
        [TestCase("Manager.Name + [Direct Reports].Name", Result = "GetRootContextEntityNode(AccessRelationshipNode,AccessRelationshipNode)")]
        [TestCase("[Direct Reports] where Age>1", Result = "GetRootContextEntityNode(WhereNode(AccessRelationshipNode))")]
        [TestCase("[Direct Reports] where context().Age", Result = "GetRootContextEntityNode(WhereNode(AccessRelationshipNode(GetRootContextEntityNode)))")] //hmm
        [TestCase("count([Direct Reports])", Result = "GetRootContextEntityNode(CountNode(AccessRelationshipNode))")]
        [TestCase("max([Direct Reports].Age)", Result = "GetRootContextEntityNode(MaxNode(AccessRelationshipNode))")]
        [TestCase("let m = max([AA_All Fields].[DateTime]) select ([AA_All Fields] where [DateTime] = m).[Name]", Result = "GetRootContextEntityNode(MaxNode(AccessRelationshipNode),WhereNode(AccessRelationshipNode))" )] // #28406
        [RunAsDefaultTenant]
        public string Calculations_NodeStructure(string script)
        {
            var settings = CreateBuilderSettings("AA_Manager");
            var expr = (Expression)Factory.ExpressionCompiler.Compile( script, settings );

            Func<EntityNode,string> build = null;
			build = node => node.GetType( ).Name + ( node.ChildContainer.ChildEntityNodes.Count > 0 ? ( "(" + string.Join( ",", node.ChildContainer.ChildEntityNodes.Select( build ) ) + ")" ) : "" );

            string structure = build(expr.ListRoot);
            return structure;
        }

        [TestCase(ScriptHostType.Any, false)]
        [TestCase(ScriptHostType.Report, false)]    // TODO
        [TestCase(ScriptHostType.Evaluate, true)]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void DisallowReferencesToCalculatedFields(ScriptHostType host, bool allow)
        {
            // Create scenario
            EntityType type = new EntityType();
            type.Name = "Test" + Guid.NewGuid().ToString();
            Field field = new StringField().As<Field>();
            field.Name = "MyCalcField";
            field.FieldCalculation = "Name + 'hello'";
            field.IsCalculatedField = true;
            type.Fields.Add(field);
            type.Save();

            var script = "MyCalcField";
            var settings = CreateBuilderSettings(type.Name);
            settings.ScriptHost = host;

            if (allow)
            {
                Factory.ExpressionCompiler.Compile(script, settings);
            }
            else
            {
                Assert.Throws<ParseException>(() => Factory.ExpressionCompiler.Compile(script, settings),
                    "'MyCalcField' is a calculated field. Calculated field cannot be used in other calculations.");
            }
        }

        //private ExpressionNode RunSingleTest(string script, BuilderSettings settings = null)
        //{
        //    var result = ExpressionGrammar.ParseMacro(script);
        //    var root = CheckTerm(result.Root, Terms.Expression);

        //    StaticBuilder sb = new StaticBuilder();
        //    sb.Settings = settings ?? (new BuilderSettings());
        //    Expression tree = sb.CompileTree(root);

        //    return tree.Root;
        //}

        ///// <summary>
        ///// Validate that the node represents the expected parse-term.
        ///// </summary>
        //private ParseTreeNode CheckTerm(ParseTreeNode node, string expectedTerm)
        //{
        //    Assert.AreEqual(expectedTerm, node.Term.Name);
        //    return node;
        //}

        private BuilderSettings CreateBuilderSettings(string contextDefinitionName)
        {
            var definition = CodeNameResolver.GetInstance(contextDefinitionName, EntityType.EntityType_Type).Single();

            BuilderSettings settings = new BuilderSettings
            {
                RootContextType = new ExprType { Type = DataType.Entity, EntityType = new EntityRef(definition.Id) }
            };
            return settings;
        }




    }
}
