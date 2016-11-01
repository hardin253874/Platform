// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor;
using EDC.SoftwarePlatform.WebApi.Controllers.CalcEngine;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.CalcEngine
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class CalcEngineRequestHandlerTests
    {
        [Test]
        public void EvaluateExpressions_EmptyExpressions()
        {
            var handler = new CalcEngineRequestHandler();

            Assert.That(
                () => handler.EvaluateExpressions(new EntityType(), new Dictionary<string, CalcEngineExpression>()),
                Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("expressions"));
        }


        [Test]
        public void EvaluateExpressions_EvalMultipleExpressions()
        {
            var handler = new CalcEngineRequestHandler();

            var entity = new EntityType
            {
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString()
            };

            var expressions = new Dictionary<string, CalcEngineExpression>
            {
                {
                    "GetName",
                    new CalcEngineExpression
                    {
                        ExpectedResultType = new ExpressionType
                        {
                            DataType = DataType.String
                        },
                        Expression = "[Name]"
                    }
                },
                {
                    "GetDescription",
                    new CalcEngineExpression
                    {
                        ExpectedResultType = new ExpressionType
                        {
                            DataType = DataType.String
                        },
                        Expression = "[Description]"
                    }
                }
            };

            var results = handler.EvaluateExpressions(entity, expressions);

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Results.Count);
            Assert.IsTrue(results.Results.ContainsKey("GetName"));
            Assert.AreEqual(entity.Name, results.Results["GetName"].Value);
            Assert.AreEqual(DataType.String, results.Results["GetName"].ResultType);
            Assert.IsTrue(string.IsNullOrWhiteSpace(results.Results["GetName"].ErrorMessage));

            Assert.IsTrue(results.Results.ContainsKey("GetDescription"));
            Assert.AreEqual(entity.Description, results.Results["GetDescription"].Value);
            Assert.AreEqual(DataType.String, results.Results["GetDescription"].ResultType);
            Assert.IsTrue(string.IsNullOrWhiteSpace(results.Results["GetDescription"].ErrorMessage));
        }

        [Test]
        public void EvaluateExpressions_EvalMultipleExpressionsWithFailures()
        {
            var handler = new CalcEngineRequestHandler();

            var entity = new EntityType
            {
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString()
            };

            var expressions = new Dictionary<string, CalcEngineExpression>
            {
                {
                    "GetName",
                    new CalcEngineExpression
                    {
                        ExpectedResultType = new ExpressionType
                        {
                            DataType = DataType.String
                        },
                        Expression = "XXXXXX!!!"
                    }
                },
                {
                    "GetDescription",
                    new CalcEngineExpression
                    {
                        ExpectedResultType = new ExpressionType
                        {
                            DataType = DataType.String
                        },
                        Expression = "[Description]"
                    }
                }
            };

            var results = handler.EvaluateExpressions(entity, expressions);

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Results.Count);
            Assert.IsTrue(results.Results.ContainsKey("GetName"));
            Assert.IsFalse(string.IsNullOrWhiteSpace(results.Results["GetName"].ErrorMessage));
            Assert.IsTrue(string.IsNullOrWhiteSpace(results.Results["GetName"].Value));

            Assert.IsTrue(results.Results.ContainsKey("GetDescription"));
            Assert.IsTrue(string.IsNullOrWhiteSpace(results.Results["GetDescription"].ErrorMessage));
            Assert.AreEqual(entity.Description, results.Results["GetDescription"].Value);
            Assert.AreEqual(DataType.String, results.Results["GetDescription"].ResultType);
        }

        [Test]
        public void EvaluateExpressions_EvalSingleExpression()
        {
            var handler = new CalcEngineRequestHandler();

            var entity = new EntityType
            {
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString()
            };

            var expressions = new Dictionary<string, CalcEngineExpression>
            {
                {
                    "GetName",
                    new CalcEngineExpression
                    {
                        ExpectedResultType = new ExpressionType
                        {
                            DataType = DataType.String
                        },
                        Expression = "[Name]"
                    }
                }
            };

            var results = handler.EvaluateExpressions(entity, expressions);

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Results.Count);
            Assert.IsTrue(results.Results.ContainsKey("GetName"));
            Assert.AreEqual(entity.Name, results.Results["GetName"].Value);
            Assert.AreEqual(DataType.String, results.Results["GetName"].ResultType);
        }

        [Test]
        public void EvaluateExpressions_NullContextEntity()
        {
            var handler = new CalcEngineRequestHandler();

            Assert.That(() => handler.EvaluateExpressions(null, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("contextEntity"));
        }

        [Test]
        public void EvaluateExpressions_NullExpressions()
        {
            var handler = new CalcEngineRequestHandler();

            Assert.That(() => handler.EvaluateExpressions(new EntityType(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("expressions"));
        }
    }
}