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
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Tree;

namespace ReadiNow.Expressions.Test.Builder
{
    [TestFixture]
    [RunWithTransaction]
    public class StaticDependencyTests
    {
        [TestCase("Name", "core:name")]
        [TestCase("Name+[Job Title]", "core:name,test:jobTitle")]
        [TestCase("context()", "")]
        [TestCase("Manager.Name", "test:reportsTo,core:name")]
        [TestCase("Manager.Manager", "test:reportsTo")]
        [TestCase("Manager.Department", "test:reportsTo,test:empDepartment")]
        [TestCase("[Direct Reports].Name", "test:reportsTo,core:name")]
        [TestCase("all([AA_Manager]).Name", "test:manager,core:name")]
        [TestCase("resource([AA_Employee], [David Quint]).Name", "test:employee,test:aaDavidQuint,core:name")]
        [TestCase("resource([AA_Employee], [Peter Aylett])", "test:employee,test:aaPeterAylett")]
        [RunAsDefaultTenant]
        public void StaticDependency_ExplicitEntities(string script, string expectedReferences)
        {
            var settings = CreateBuilderSettings("AA_Manager");

            IExpressionCompiler compiler = Factory.ExpressionCompiler;
            IExpression expr = compiler.Compile(script, settings);
            CalculationDependencies dependencies = compiler.GetCalculationDependencies(expr);

            var expected = expectedReferences
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(alias => new EntityRef(alias).Id)
                .ToList();

            Assert.That(dependencies.IdentifiedEntities, Is.EquivalentTo(expected));
        }

        [TestCase("Name", "core:name")]
        [TestCase("Name+[Job Title]", "core:name,test:jobTitle")]
        [TestCase("context()", "")]
        [TestCase("Manager.Name", "core:name")]
        [TestCase("resource([AA_Employee], [David Quint]).Name", "core:name")]
        [TestCase("resource([AA_Employee], [Peter Aylett])", "")]
        [RunAsDefaultTenant]
        public void StaticDependency_Fields(string script, string expectedReferences)
        {
            var settings = CreateBuilderSettings("AA_Manager");

            IExpressionCompiler compiler = Factory.ExpressionCompiler;
            IExpression expr = compiler.Compile(script, settings);
            CalculationDependencies dependencies = compiler.GetCalculationDependencies(expr);

            var expected = expectedReferences
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(alias => new EntityRef(alias).Id)
                .ToList();

            Assert.That(dependencies.Fields, Is.EquivalentTo(expected));
        }

        [TestCase("Name", "")]
        [TestCase("Manager.Name", "test:reportsTo")]
        [TestCase("Manager.Manager", "test:reportsTo")]
        [TestCase("[Direct Reports]", "test:reportsTo")]
        [TestCase("[Direct Reports].Manager", "test:reportsTo")]
        [TestCase("Manager.Department", "test:reportsTo,test:empDepartment")]
        [RunAsDefaultTenant]
        public void StaticDependency_Relationships(string script, string expectedReferences)
        {
            var settings = CreateBuilderSettings("AA_Manager");

            IExpressionCompiler compiler = Factory.ExpressionCompiler;
            IExpression expr = compiler.Compile(script, settings);
            CalculationDependencies dependencies = compiler.GetCalculationDependencies(expr);

            var expected = expectedReferences
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(alias => new EntityRef(alias).Id)
                .ToList();

            Assert.That(dependencies.Relationships, Is.EquivalentTo(expected));
        }

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
