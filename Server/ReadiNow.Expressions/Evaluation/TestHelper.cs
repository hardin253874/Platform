// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;
using ReadiNow.Expressions.Parser;
using ReadiNow.Expressions.Tree;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Evaluation
{
    public static class TestHelper
    {
        public static void Test(string test)
        {
            // Extract test data
            TestData testData = GetTestParameters(test);

            // (Queries do not support parameters)
			if ( testData.ScriptHost == ScriptHostType.Any && testData.Parameters.Count > 0 )
                testData.ScriptHost = ScriptHostType.Evaluate;

            // Attempt to compile
            IExpression expression = TestCompile(testData);
            
            if (testData.Error != null) // script is expected to be invalid.
                return;

            // Attempt to evaluate
            if (testData.ScriptHost != ScriptHostType.Report)
            {
                TestEvaluation(testData, expression);
            }

            // Attempt to build query
            if (testData.ScriptHost != ScriptHostType.Evaluate)
            {
                StructuredQuery query = TestBuildQuery(testData, expression);

                TestRunQuery(testData, query);

            }
        }

        private static TestData GetTestParameters(string test)
        {
            var testData = new TestData();
            try
            {
                string[] parts = test.Split(';');
                foreach (string part in parts)
                {
                    string[] subparts = part.TrimStart().Split(new[] { ':' }, 2);
                    string command = subparts[0];
                    string data = null;
                    string dataRaw = null;
                    if (subparts.Length > 1)
                    {
                        dataRaw = subparts[1].TrimStart();
                        data = dataRaw.TrimEnd();
                    }
                    subparts[subparts.Length - 1] = subparts[subparts.Length - 1].TrimEnd();

                    switch (command)
                    {
                        case "script":
                            testData.Script = data;
                            break;
                        case "context":
                            testData.Context = ReadValue(dataRaw);
                            break;
                        case "convertto":
                            testData.ConvertTo = ReadValue(dataRaw);
                            break;
                        case "expect":
                            testData.Expected = ReadValue(dataRaw);
                            break;
                        case "param":
                            if ( data == null )
                                throw new InvalidOperationException( $"Unexpected null for {command}" );
                            string[] paramParts = data.Split(new[] { '=' }, 2);
                            testData.Parameters[paramParts[0]] = ReadValue(paramParts[1]);
                            break;
                        case "error":
                            testData.Error = data;
                            break;
                        case "unsorted":
                            testData.Unsorted = true;
                            break;
                        case "host":
                            if ( data == null )
                                throw new InvalidOperationException( $"Unexpected null for {command}" );
                            testData.ScriptHost = (ScriptHostType)Enum.Parse(typeof(ScriptHostType), data, true);
                            break;
                        case "hostapi":
                            if ( data == null )
                                throw new InvalidOperationException( $"Unexpected null for {command}" );
                            testData.ScriptHostIsApi = bool.Parse(data);
                            break;
                        case "":
                            throw new InvalidOperationException("Double semicolon found in test");
                        default:
                            throw new InvalidOperationException("Unknown test command: " + command);
                    }
                }
                if (testData.Script == null)
                    throw new Exception("Script not specified.");
                if (testData.Expected == null && testData.Error == null)
                    throw new Exception("Expected result not specified.");
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid test data", ex);
            }
            return testData;
        }

        private static IExpression TestCompile(TestData testData)
        {
            // Prep settings
            BuilderSettings settings = new BuilderSettings();
            if (testData.ConvertTo != null)
                settings.ExpectedResultType = testData.ConvertTo.ExprType;
            if (testData.Context != null)
                settings.RootContextType = testData.Context.ExprType;
            settings.TestMode = true;
            settings.ParameterNames = testData.Parameters.Keys.ToList();
            settings.StaticParameterResolver = name => testData.Parameters.ContainsKey(name) ? testData.Parameters[name].ExprType : null;
            settings.ScriptHost = testData.ScriptHost;
            settings.ScriptHostIsApi = testData.ScriptHostIsApi;


            // Do compile
            IExpression expression;
            try
            {
                expression = Factory.ExpressionCompiler.Compile(testData.Script, settings);

                // Fail if we expected an error, but didn't get one
                if (testData.Error != null)
                    throw new Exception("Expected error message, but none was returned.");
            }
            catch (ParseException ex)
            {
                if (ex.Message == testData.Error)
                    return null;
                // Fail if we got an error, but didn't expect it
                throw;
            }

            // Check static result data
            ExprType expectedType = testData.Expected.ExprType;
            ExprType actualType = expression.ResultType;

            if (expectedType.Type != actualType.Type)
                throw new Exception(string.Format("Expected result type {0} but actual type was {1}.",
                    expectedType.Type, actualType.Type));

            if (expectedType.EntityType != null && actualType.EntityType.Id != expectedType.EntityType.Id)
                throw new Exception(string.Format("Expected result entity type {0} but actual type was {1}.",
                    expectedType.EntityType, actualType.EntityType));

            if (expectedType.IsList != actualType.IsList)
                throw new Exception(string.Format("Expected list result {0} but actual was {1}.",
                    expectedType.IsList, actualType.IsList));

            if (expectedType.DecimalPlaces != null && expectedType.DecimalPlaces != actualType.DecimalPlaces)
                throw new Exception(string.Format("Expected {0} decimal places but actual was {1}.",
                    expectedType.DecimalPlaces, actualType.DecimalPlaces));

            return expression;
        }

        private static void TestEvaluation(TestData testData, IExpression expression)
        {
            // Prep settings
            EvaluationSettings settings = new EvaluationSettings();
            if (testData.Context != null)
                settings.ContextEntity = testData.Context.Resource;
            settings.TimeZoneName = TimeZoneHelper.SydneyTimeZoneName;
            settings.ParameterResolver = name => testData.Parameters.ContainsKey(name) ? testData.Parameters[name].Value : null;

            // Do evaluate
            ExpressionRunResult result = Factory.ExpressionRunner.Run(expression, settings);
            object actual = result.Value;

            // Check result
            if (!testData.Expected.NoValue)
            {
                object expected = ReformatResult(testData.Expected.Value, testData);
                object actual2 = ReformatResult(actual, testData);

                if (!expected.Equals(actual2))
                {
                    throw new Exception(string.Format("Expected eval result {0} but actual result was {1}.", expected, actual2));
                }
            }
        }

        private static StructuredQuery TestBuildQuery(TestData testData, IExpression expression)
        {
            // Need to query against something (anything, whatever), in the case of test cases that don't specify a context
            EntityRef resourceType = testData.Context != null ?
                new EntityRef(testData.Context.ExprType.EntityType)
                : new EntityRef("core:type");

            // Create a query to graft results into
            StructuredQuery sq = new StructuredQuery();
            sq.RootEntity = new ResourceEntity
            {
                EntityTypeId = resourceType
            };

            // Select a single row (use the type resource as both the type and the instance)
            EntityRef resource = testData.Context != null && testData.Context.Resource != null ?
                new EntityRef(testData.Context.Resource.Id)
                : new EntityRef("core:type");
            sq.Conditions.Add(new QueryCondition
            {
                Expression = new IdExpression { NodeId = sq.RootEntity.NodeId },
                Operator = ConditionType.Equal,
                Argument = new TypedValue { Type = DatabaseType.IdentifierType, Value = resource.Id }
            });            

            // Prep settings
            var settings = new QueryBuilderSettings(); 
            settings.StructuredQuery = sq;
            settings.ContextEntity = (ResourceEntity)sq.RootEntity;

            // Build query
            ScalarExpression queryExpr = Factory.ExpressionCompiler.CreateQueryEngineExpression(expression, settings);
            sq.SelectColumns.Add(new SelectColumn { Expression = queryExpr });
            return sq;
        }

        private static void TestRunQuery(TestData testData, StructuredQuery query)
        {
            query.TimeZoneName = TimeZoneHelper.SydneyTimeZoneName;
            
            QuerySettings settings = new QuerySettings();
            QueryResult result;
            IQueryRunner queryRunner = Factory.Current.ResolveNamed<IQueryRunner>( "Test" );    // test instance does not cache runner or builder

            // Run query
            result = queryRunner.ExecuteQuery( query, settings );

            if (result.DataTable.Rows.Count == 0)
                throw new Exception("No rows returned.");

            object actual;
            if (result.DataTable.Rows.Count == 1)
            {
                actual = result.DataTable.Rows[0][0];
            }
            else
            {
                actual = result.DataTable.Rows.OfType<DataRow>().Select(row => row[0]).ToArray();
            }

            // Check result
            if (!testData.Expected.NoValue)
            {
                object expected = testData.Expected.Value;
                if (testData.Expected.ExprType.Type == DataType.Bool)
                {
                    if (expected == null)
                        expected = false;
                }
                if (actual is DBNull)
                {
                    actual = null;
                }
                if (actual is double)
                {
                    actual = (decimal)(double)actual;
                }
                if (actual is float)
                {
                    actual = (decimal)(float)actual;
                }
                if (actual is decimal && expected is int)
                {
                    expected = (decimal)(int)expected;
                }
                if (actual is int && expected is decimal)
                {
                    actual = (decimal)(int)actual;
                }
                if (expected is IEntity)
                {
                    // SQL returns names for now
                    expected = ((IEntity)expected).As<Resource>().Name;
                }

                object expected2 = ReformatResult(expected, testData);
                
                object actual2 = ReformatResult(actual, testData);

                // Note: reporting engine will cast int to bigint for sum aggregates - see #24609
                if ( actual2 is long )
                {
                    actual2 = ( int ) ( long ) actual2;
                }

                if (!expected2.Equals(actual2))
                {
                    throw new Exception(string.Format("Expected SQL result {0} but actual result was {1}.", expected2, actual2));
                }
            }
        }

        private static object ReformatResult(object result, TestData testData)
        {
            if (result == null)
                return "<null>";

            var list = result as IEnumerable<IEntity>;
            if (list != null)
            {
                var names = list.Select(e => e == null ? "<null>" : e.As<Resource>().Name ?? e.Alias);
                if (testData.Unsorted)
                    names = names.OrderBy(name => name);
                return string.Join(",", names);
            }
            if (result is string)
            {
                string sResult = (string)result;
                if (sResult.StartsWith("<e "))
                    sResult = DatabaseTypeHelper.GetEntityXmlName(sResult);
                return sResult;
            }
            var list2 = result as IEnumerable;
            if (list2 != null)
            {
                var names = list2.OfType<object>().Select(e => e == null ? "<null>" : e.ToString()).Select(f => f.StartsWith("<e ") ? DatabaseTypeHelper.GetEntityXmlName(f) : f);
                if (testData.Unsorted)
                    names = names.OrderBy(name => name);
                return string.Join(",", names);
            }
            decimal? dResult = result as decimal?;
            if (dResult != null && testData.Expected.ExprType.DecimalPlaces != null)
            {
                result = decimal.Round(dResult.Value, testData.Expected.ExprType.DecimalPlaces.Value);
            }
            return result;
        }
        
        private static TestValue ReadValue(string dataRaw)
        {
            string[] dataParts = dataRaw.Split(new[] { ':' }, 2);
            dataParts[0] = dataParts[0].Trim();

            var result = new TestValue();
            string typeName = dataParts[0];
            bool isList = typeName.EndsWith(" list");
            if (isList)
                typeName = typeName.Substring(0, typeName.Length - " list".Length);
            bool disallowList = typeName.EndsWith(" disallowlist");
            if (disallowList)
                typeName = typeName.Substring(0, typeName.Length - " disallowlist".Length);
            bool untrimmed = typeName.EndsWith(" untrimmed");
            if (untrimmed)
            {
                typeName = typeName.Substring(0, typeName.Length - " untrimmed".Length);
            }
            else
            {
                if (dataParts.Length > 1)
                    dataParts[dataParts.Length - 1] = dataParts[dataParts.Length - 1].TrimEnd();
            }

            if (typeName == "int")
                typeName = "Int32";

            string[] typeParts = typeName.Split(new [] { '(', ')' });
            string typeNameBase = typeParts[0];

            DataType dataType;
            if (Enum.TryParse(typeNameBase, true, out dataType))
            {
                // scalar
                result.ExprType = new ExprType(dataType);
                result.ExprType.IsList = isList;
                result.ExprType.DisallowList = disallowList;

                if (typeParts.Length > 1)
                {
                    result.ExprType.DecimalPlaces = int.Parse(typeParts[1]);
                }
                if (dataParts.Length > 1)
                {
                    var dbType = DataTypeHelper.ToDatabaseType(result.ExprType.Type);
                    if (isList)
                    {
                        var resultList = new List<object>();
                        string[] values = dataParts[1].Split(',');
                        foreach (string sValue in values)
                        {
                            object value = null;
                            if (sValue != "null")
                            {
                                if (typeNameBase == "date" || typeNameBase == "datetime")
                                {
                                    result.Value = DateTime.Parse(sValue, (new CultureInfo("en-AU")).DateTimeFormat);
                                    if (typeNameBase == "datetime")
                                        // treat date-time test data as local time
                                        result.Value = TimeZoneHelper.ConvertToUtc((DateTime)result.Value,
                                                                                   TimeZoneHelper.SydneyTimeZoneName);
                                }
                                else
                                {
                                    value = dbType.ConvertFromString(sValue); // this method is stuffed
                                    if (value is float)
                                        value = (decimal)(float)result.Value;
                                    if (value is TimeSpan)
                                        value = TimeType.NewTime((TimeSpan)result.Value);
                                }
                            }
                            resultList.Add(value);
                        }
                        result.Value = resultList;
                    }
                    else
                    {
                        if (dataParts[1] != "null")
                        {
                            if (typeNameBase == "date" || typeNameBase == "datetime")
                            {
                                result.Value = DateTime.Parse(dataParts[1], (new CultureInfo("en-AU")).DateTimeFormat);
                                if (typeNameBase == "datetime")
                                    // treat date-time test data as local time
                                    result.Value = TimeZoneHelper.ConvertToUtc((DateTime)result.Value,
                                                                               TimeZoneHelper.SydneyTimeZoneName);
                            }
                            else
                            {
                                result.Value = dbType.ConvertFromString(dataParts[1]); // this method is stuffed
                                if (result.Value is float)
                                    result.Value = (decimal) (float) result.Value;
                                if (result.Value is TimeSpan)
                                    result.Value = TimeType.NewTime((TimeSpan) result.Value);
                            }
                        }
                    }
                }
                else
                {
                    result.NoValue = true;
                }
            }
            else
            {
                // entity
                result.ResourceTypeName = typeName;
                long resourceTypeId = Factory.ScriptNameResolver.GetTypeByName(result.ResourceTypeName);
                EntityType resourceType = EDC.ReadiNow.Model.Entity.Get<EntityType>(resourceTypeId);
                if (resourceType == null)
                    throw new Exception("ResourceType not found");

                result.ExprType = ExprTypeHelper.EntityOfType(new EntityRef(resourceTypeId));
                result.ExprType.IsList = isList;
                if (dataParts.Length > 1)
                {
                    result.ResourceName = dataParts[1];
                    if (isList)
                    {
                        if (dataParts[1] == "")
                            result.Value = new EntityType[] { };
                        else
                        {
                            var resList = new List<IEntity>();
                            string names = dataParts[1].Trim();
                            if (names != "" && names != "empty")
                            {
                                foreach (string name in names.Split(','))
                                {
                                    var match = Factory.ScriptNameResolver.GetInstance(name.Trim(), resourceType.Id);
                                    if (match == null)
                                        throw new Exception(string.Format("Test error: could not find {0} called {1}",
                                                                          typeName, name));
                                    resList.Add(match);
                                }
                            }
                            result.Value = resList.ToArray();
                        }
                    }
                    else if (result.ResourceName != "null")
                    {
                        result.Resource = Factory.ScriptNameResolver.GetInstance(result.ResourceName, resourceType.Id);
                        result.Value = result.Resource;
                    }
                }
                else
                {
                    result.NoValue = true;
                }
            }
            result.Untrimmed = untrimmed;
            return result;
        }
    }

    public class TestValue
    {
        // Type data
        public ExprType ExprType { get; set; }
        public string ResourceTypeName { get; set; }
        
        // Value data
        public object Value { get; set; }
        public string ResourceName { get; set; }
        public IEntity Resource { get; set; }
        public bool NoValue { get; set; }
        public bool Untrimmed { get; set; }
    }

    public class TestData
    {
        public TestData()
        {
            Parameters = new Dictionary<string, TestValue>();
        }

        public string Script { get; set; }

        public TestValue Context { get; set; }

        public TestValue ConvertTo { get; set; }

        public TestValue Expected { get; set; }

        public Dictionary<string, TestValue> Parameters { get; set; }

        public bool Unsorted { get; set; }

        public string Error { get; set; }

        public ScriptHostType ScriptHost { get; set; }

        public bool ScriptHostIsApi { get; set; }
    }

}
