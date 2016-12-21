// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    public class ExpressionHelperTests : TestBase
    {
        /// <summary>
        /// </summary>
        [TestCase("abc", Result="'abc'")]
        [TestCase("a'b", Result = "'a''b'")]
        [TestCase("a\nb", Result = "'a\\nb'")]
        [TestCase("{{hello}}", Result = "<<hello>>")]
        [TestCase("{{'hello'}}", Result = "<<'hello'>>")]
        [TestCase("abc{{'hello'}}def", Result = "'abc' + <<'hello'>> + 'def'")]
        public string TemplateExpressionTest(string test)
        {
            string result = ExpressionHelper.ConvertTemplateToExpressionString(test);

            string result2 = result.Replace("join(", "<<").Replace(")", ">>");
            
            return result2;
        }


        [Test]
        [RunAsDefaultTenant]
        public void ExpressionWithSingleKnownEntity()
        {
            var entity = new Resource();

            var expression = new WfExpression { ExpressionString = null };
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "Bob", ReferencedEntity = entity });
            expression.ArgumentToPopulate = new ResourceArgument().As<ActivityArgument>();
            
            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableTo(typeof(IEntity)));
            Assert.That(((IEntity)result).Id, Is.EqualTo(entity.Id));
        }


        [Test]
        [RunAsDefaultTenant]
        public void ListExpressionWithMultipleKnownEntities()
        {
            var entity1 = new Resource();
            var entity2 = new Resource();

            var expression = new WfExpression { ExpressionString = null };
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "1", ReferencedEntity = entity1 });
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "2", ReferencedEntity = entity2});
            expression.ArgumentToPopulate = new ResourceListArgument().As<ActivityArgument>();

            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableTo(typeof(IEnumerable<IEntity>)));
            var list = (IEnumerable<IEntity>)result;
            Assert.That(list.Count(), Is.EqualTo(2));
        }

        [Test]
        [RunAsDefaultTenant]
        public void ExpressionWithMultipleKnownEntities()
        {
            var entity1 = new Resource();
            var entity2 = new Resource();

            var expression = new WfExpression { ExpressionString = null };
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "1", ReferencedEntity = entity1 });
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "2", ReferencedEntity = entity2 });
            expression.ArgumentToPopulate = new ResourceArgument().As<ActivityArgument>();

            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableTo(typeof(IEntity)));
            Assert.That(((IEntity) result).Id, Is.EqualTo(entity1.Id));
        }

        [Test]
        [RunAsDefaultTenant]
        public void ExpressionWithNoKnownEntities()
        {
            var expression = new WfExpression { ExpressionString = null };
            expression.ArgumentToPopulate = new ResourceArgument().As<ActivityArgument>();

            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        public void ListExpressionWithNoKnownEntities()
        {
            var expression = new WfExpression { ExpressionString = null };
            expression.ArgumentToPopulate = new ResourceListArgument().As<ActivityArgument>();

            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Not.Null);
            Assert.That(((IEnumerable<IEntity>) result).Any(), Is.False);
        }


        [Test]
        [RunAsDefaultTenant]
        public void KnownAsInExpression()
        {
            var entity1 = new Resource();

            var expression = new WfExpression { ExpressionString = "E1" };
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "E1", ReferencedEntity = entity1 });
            expression.ArgumentToPopulate = new ResourceArgument().As<ActivityArgument>();

            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableTo(typeof(IEntity)));
            Assert.That(((IEntity)result).Id, Is.EqualTo(entity1.Id));
        }

        [Test]
        [RunAsDefaultTenant]
        public void KnownAsInExpressionToList()
        {
            var entity1 = new Resource();

            var expression = new WfExpression { ExpressionString = "E1" };
            expression.WfExpressionKnownEntities.Add(new NamedReference { Name = "E1", ReferencedEntity = entity1 });
            expression.ArgumentToPopulate = new ResourceListArgument().As<ActivityArgument>();

            Workflow wf = new Workflow();
            wf.ExpressionMap.Add(expression);

            var result = ExpressionHelper.EvaluateExpression(expression, TestRunState.CreateDummyState(wf));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.AssignableTo(typeof(IEnumerable<IEntity>)));
            Assert.That(((IEnumerable<IEntity>)result).Count, Is.EqualTo(1));
        }

    }
}