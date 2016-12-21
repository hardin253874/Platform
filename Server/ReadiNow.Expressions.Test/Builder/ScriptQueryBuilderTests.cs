// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.Xml;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Core;

namespace ReadiNow.Expressions.Test.Builder
{
    /// <summary>
    /// Remarks: these tests all assume that static building is working.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class ScriptQueryBuilderTests
    {
        readonly string[] Namespaces = {
            @" xmlns='http://enterprisedata.com.au/readinow/v2/query/2.0'",
            @" xmlns:xsd='http://www.w3.org/2001/XMLSchema'",
            @" xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'",
            @" xmlns:q1='http://enterprisedata.com.au/readinow/v2/query/2.0'" };

        //[RunAsDefaultTenant]

        #region Test Literals
        [Test]
        public void ConvertLiteral_Int()
        {
            string script = "123";
            string expected =
@"<ScalarExpression xsi:type='q1:LiteralExpression'>
  <q1:Value type='Int32'>123</q1:Value>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [Test]
        public void ConvertLiteral_String()
        {
            string script = "'hello'";
            string expected =
@"<ScalarExpression xsi:type='q1:LiteralExpression'>
  <q1:Value type='String'>hello</q1:Value>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [Test]
        public void ConvertLiteral_Bool()
        {
            string script = "true";
            string expected =
@"<ScalarExpression xsi:type='q1:LiteralExpression'>
  <q1:Value type='Bool'>True</q1:Value>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [Test]
        public void ConvertLiteral_Decimal()
        {
            string script = "1.2";
            string expected =
@"<ScalarExpression xsi:type='q1:LiteralExpression'>
  <q1:Value type='Decimal'>1.2</q1:Value>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }
        #endregion

        #region Test Operators

        [Test]
        public void TestIsNull()
        {
            string script = "'abc' is null";
            string expected =
@"<ScalarExpression xsi:type='q1:ComparisonExpression'>
  <q1:Operator>IsNull</q1:Operator>
  <q1:Expressions>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='String'>abc</q1:Value>
    </q1:Expression>
  </q1:Expressions>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [Test]
        public void TestIsNotNull()
        {
            string script = "'abc' is not null";
            string expected =
@"<ScalarExpression xsi:type='q1:ComparisonExpression'>
  <q1:Operator>IsNotNull</q1:Operator>
  <q1:Expressions>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='String'>abc</q1:Value>
    </q1:Expression>
  </q1:Expressions>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [Test]
        public void TestIif()
        {
            string script = "iif(1>2,'abc','def')";
            string expected =
@"<ScalarExpression xsi:type='q1:IfElseExpression'>
  <q1:BooleanExpression xsi:type='q1:ComparisonExpression'>
    <q1:Operator>GreaterThan</q1:Operator>
    <q1:Expressions>
      <q1:Expression xsi:type='q1:LiteralExpression'>
        <q1:Value type='Int32'>1</q1:Value>
      </q1:Expression>
      <q1:Expression xsi:type='q1:LiteralExpression'>
        <q1:Value type='Int32'>2</q1:Value>
      </q1:Expression>
    </q1:Expressions>
  </q1:BooleanExpression>
  <q1:IfBlockExpression xsi:type='q1:LiteralExpression'>
    <q1:Value type='String'>abc</q1:Value>
  </q1:IfBlockExpression>
  <q1:ElseBlockExpression xsi:type='q1:LiteralExpression'>
    <q1:Value type='String'>def</q1:Value>
  </q1:ElseBlockExpression>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [TestCase("getdate()", "TodayDate", "Date")]
        [TestCase("getdatetime()", "TodayDateTime", "DateTime")]
        [TestCase("gettime()", "Time", "Time")]
        public void TestZeroArgOperations(string script, string operatorName, string resultType)
        {
            string expected =
@"<ScalarExpression xsi:type='q1:CalculationExpression'>
  <q1:Operator>" + operatorName + @"</q1:Operator>
  <q1:DisplayType xsi:type='q1:" + resultType + @"Type' />
  <q1:Expressions />
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [TestCase("abs(123)", "Abs", "Int32", "Int32")]
        [TestCase("ceiling(123.0)", "Ceiling", "Decimal", "Int32")]
        [TestCase("exp(123.0)", "Exp", "Decimal", "Decimal")]
        [TestCase("floor(123.0)", "Floor", "Decimal", "Int32")]
        [TestCase("len('123')", "StringLength", "String", "Int32")]
        [TestCase("log(123.0)", "Log", "Decimal", "Decimal")]
        [TestCase("log10(123.0)", "Log10", "Decimal", "Decimal")]
        [TestCase("sign(123)", "Sign", "Int32", "Int32")]
        [TestCase("sqrt(123.0)", "Sqrt", "Decimal", "Decimal")]
        [TestCase("square(123.0)", "Square", "Decimal", "Decimal")]
        [TestCase("tolower('123')", "ToLower", "String", "String")]
        [TestCase("toupper('123')", "ToUpper", "String", "String")]
        public void TestUnaryOperations(string script, string operatorName, string inputType, string resultType)
        {
            string expected =
@"<ScalarExpression xsi:type='q1:CalculationExpression'>
  <q1:Operator>" + operatorName + @"</q1:Operator>
  <q1:DisplayType xsi:type='q1:" + resultType + @"Type' />
  <q1:Expressions>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='" + inputType + @"'>123</q1:Value>
    </q1:Expression>
  </q1:Expressions>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [TestCase("123 + 456", "Add", "Int32", "Int32")]
        [TestCase("123 - 456", "Subtract", "Int32", "Int32")]
        [TestCase("123 * 456", "Multiply", "Int32", "Int32")]
        [TestCase("123 % 456", "Modulo", "Int32", "Int32")]
        [TestCase("123.0 / 456.0", "Divide", "Decimal", "Decimal")]
        [TestCase("'123' + '456'", "Concatenate", "String", "String")]
        [TestCase("isnull('123', '456')", "IsNull", "String", "String")]
        [TestCase("power(123.0, 456.0)", "Power", "Decimal", "Decimal")]
        public void TestBinaryOperations(string script, string operatorName, string inputType, string resultType)
        {
            string expected =
@"<ScalarExpression xsi:type='q1:CalculationExpression'>
  <q1:Operator>" + operatorName + @"</q1:Operator>
  <q1:DisplayType xsi:type='q1:" + resultType + @"Type' />
  <q1:Expressions>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='" + inputType + @"'>123</q1:Value>
    </q1:Expression>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='" + inputType + @"'>456</q1:Value>
    </q1:Expression>
  </q1:Expressions>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [TestCase("'123' = '456'", "Equal")]
        [TestCase("'123' <> '456'", "NotEqual")]
        [TestCase("'123' > '456'", "GreaterThan")]
        [TestCase("'123' < '456'", "LessThan")]
        [TestCase("'123' >= '456'", "GreaterThanEqual")]
        [TestCase("'123' <= '456'", "LessThanEqual")]
        [TestCase("'123' <= '456'", "LessThanEqual")]
        [TestCase("'123' like '456'", "Like")]
        [TestCase("'123' not like '456'", "NotLike")]
        public void TestComparators(string script, string operatorName)
        {
            string expected =
@"<ScalarExpression xsi:type='q1:ComparisonExpression'>
  <q1:Operator>" + operatorName + @"</q1:Operator>
  <q1:Expressions>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='String'>123</q1:Value>
    </q1:Expression>
    <q1:Expression xsi:type='q1:LiteralExpression'>
      <q1:Value type='String'>456</q1:Value>
    </q1:Expression>
  </q1:Expressions>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [TestCase("year(getdate())", "Year", "TodayDate")]
        [TestCase("month(getdate())", "Month", "TodayDate")]
        [TestCase("day(getdate())", "Day", "TodayDate")]
        [TestCase("hour(gettime())", "Hour", "Time")]
        [TestCase("minute(gettime())", "Minute", "Time")]
        [TestCase("second(gettime())", "Second", "Time")]
        public void TestDateComponent(string script, string operatorName, string dateSourceOperator)
        {
            string expected =
@"<ScalarExpression xsi:type='q1:CalculationExpression'>
  <q1:Operator>" + operatorName + @"</q1:Operator>
  <q1:DisplayType xsi:type='q1:Int32Type' />
  <q1:Expressions>
    <q1:Expression xsi:type='q1:CalculationExpression'>
      <q1:Operator>" + dateSourceOperator + @"</q1:Operator>
      <q1:Expressions />
    </q1:Expression>
  </q1:Expressions>
</ScalarExpression>";

            string xml = CreateScalarExpressionXml(script);
            Assert.AreEqual(expected, xml);
        }

        [TestCase("left('abc',1)")]
        [TestCase("right('abc',1)")]
        [TestCase("replace('abc','b','c')")]
        [TestCase("substring('abc',2,2)")]
        [TestCase("charindex('abc','b')")]
        [TestCase("charindex('abc','b',2)")]
        [TestCase("datefromparts(2012,12,31)")]
        [TestCase("timefromparts(17,30,59)")]
        [TestCase("datetimefromparts(2012,12,31,17,30,59)")]
        [TestCase("true and true")]
        [TestCase("true or true")]
        [TestCase("not true")]
        public void TestMiscConvertsWithoutError(string script)
        {
            string xml = CreateScalarExpressionXml(script);
            Assert.IsNotNullOrEmpty(xml);
        }
        #endregion

        #region Test Field Access
        [Test]
        [RunAsDefaultTenant]
        public void TestFieldAccess()
        {
            var result = CreateQueryXml("Name", "oldshared:person");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <EntityTypeId entityRef='true'>oldshared:person</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid1'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid0</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }
        #endregion

        #region Test Relationship Access
        [Test]
        [RunAsDefaultTenant]
        public void TestLookup()
        {
            var result = CreateQueryXml("Manager.Name", "oldshared:employee");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:employee</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid1</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }


        [Test]
        [RunAsDefaultTenant]
        public void TestRelationshipField()
        {
            var result = CreateQueryXml("[Direct Reports].Name", "oldshared:manager");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
        <RelationshipDirection>Reverse</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid1</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }


        [Test]
        [RunAsDefaultTenant]
        public void TestRelationshipNoField()
        {
            var result = CreateQueryXml("[Direct Reports]", "oldshared:manager");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
        <RelationshipDirection>Reverse</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='ResourceExpression'>
        <NodeId>guid1</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestLookupOnLookup()
        {
            var result = CreateQueryXml("Manager.Department.Name", "oldshared:employee");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelatedEntities>
          <Entity xsi:type='RelatedResource' id='guid2'>
            <RelationshipTypeId entityRef='true'>oldshared:employeeInDepartment</RelationshipTypeId>
            <RelationshipDirection>Forward</RelationshipDirection>
          </Entity>
        </RelatedEntities>
        <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:employee</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid3'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid2</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }

        [TestCase("manager.Name + manager.Name")]
        [TestCase("let m = manager select m.Name + m.Name")]
        [TestCase("let m = manager let x=m select m.Name + x.Name")]
        [RunAsDefaultTenant]
        public void TestLookupReuse(string script)
        {
            var result = CreateQueryXml(script, "oldshared:employee");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:employee</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='CalculationExpression'>
        <Operator>Concatenate</Operator>
        <DisplayType xsi:type='StringType' />
        <Expressions>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
        </Expressions>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestCount()
        {
            var result = CreateQueryXml("count([Direct Reports])", "oldshared:manager");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='AggregateEntity' id='guid1'>
        <GroupedEntity xsi:type='RelatedResource' id='guid2'>
          <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
          <RelationshipDirection>Reverse</RelationshipDirection>
        </GroupedEntity>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid3'>
      <Expression xsi:type='AggregateExpression'>
        <NodeId>guid1</NodeId>
        <AggregateMethod>Count</AggregateMethod>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }

        [TestCase("max,Max")]
        [TestCase("min,Min")]
        [TestCase("sum,Sum")]
        [RunAsDefaultTenant]
        public void TestAggregateInt(string aggType)
        {
            string[] parts = aggType.Split(',');

            var result = CreateQueryXml(parts[0] + "([Direct Reports].Age)", "oldshared:manager");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='AggregateEntity' id='guid1'>
        <GroupedEntity xsi:type='RelatedResource' id='guid2'>
          <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
          <RelationshipDirection>Reverse</RelationshipDirection>
        </GroupedEntity>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid3'>
      <Expression xsi:type='AggregateExpression'>
        <NodeId>guid1</NodeId>
        <AggregateMethod>" + parts[1] + @"</AggregateMethod>
        <Expression xsi:type='ResourceDataColumn'>
          <NodeId>guid2</NodeId>
          <FieldId entityRef='true'>oldshared:age</FieldId>
        </Expression>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }

        [TestCase("max,Max")]
        [TestCase("min,Min")]
        [TestCase("sum,Sum")]
        [TestCase("avg,Average")]
        [TestCase("stdev,StandardDeviation")]
        [RunAsDefaultTenant]
        public void TestAggregateDecimal(string aggType)
        {
            string[] parts = aggType.Split(',');

            var result = CreateQueryXml(parts[0] + "(convert(decimal,[Direct Reports].Age))", "oldshared:manager");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='AggregateEntity' id='guid1'>
        <GroupedEntity xsi:type='RelatedResource' id='guid2'>
          <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
          <RelationshipDirection>Reverse</RelationshipDirection>
        </GroupedEntity>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid3'>
      <Expression xsi:type='AggregateExpression'>
        <NodeId>guid1</NodeId>
        <AggregateMethod>" + parts[1] + @"</AggregateMethod>
        <Expression xsi:type='CalculationExpression'>
          <Operator>Cast</Operator>
          <DisplayType xsi:type='DecimalType' />
          <InputType>Int32</InputType>
          <CastType xsi:type='DecimalType' />
          <Expressions>
            <Expression xsi:type='ResourceDataColumn'>
              <NodeId>guid2</NodeId>
              <FieldId entityRef='true'>oldshared:age</FieldId>
            </Expression>
          </Expressions>
        </Expression>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestLookupWhere()
        {
            var result = CreateQueryXml("(Manager where Name='Peter Aylett').Name", "oldshared:employee");

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <Conditions>
          <Condition xsi:type='ComparisonExpression'>
            <Operator>Equal</Operator>
            <Expressions>
              <Expression xsi:type='ResourceDataColumn'>
                <NodeId>guid1</NodeId>
                <FieldId entityRef='true'>core:name</FieldId>
              </Expression>
              <Expression xsi:type='LiteralExpression'>
                <Value type='String'>Peter Aylett</Value>
              </Expression>
            </Expressions>
          </Condition>
        </Conditions>
        <RelationshipTypeId entityRef='true'>oldshared:employeesManager</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>oldshared:employee</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid1</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual(expected, result);
        }


        #endregion

        #region Test Aggregates
        [Test]
        [RunAsDefaultTenant]
        public void TestMin( )
        {
            var result = CreateQueryXml( "min(Building.Rooms.[Room Type])", "name:Campuses" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='AggregateEntity' id='guid1'>
        <GroupedEntity xsi:type='RelatedResource' id='guid2'>
          <RelatedEntities>
            <Entity xsi:type='RelatedResource' id='guid3'>
              <RelatedEntities>
                <Entity xsi:type='RelatedResource' id='guid4'>
                  <RelationshipTypeId entityRef='true'>#[Room type]</RelationshipTypeId>
                  <RelationshipDirection>Forward</RelationshipDirection>
                </Entity>
              </RelatedEntities>
              <RelationshipTypeId entityRef='true'>#[Building - Rooms]</RelationshipTypeId>
              <RelationshipDirection>Forward</RelationshipDirection>
            </Entity>
          </RelatedEntities>
          <RelationshipTypeId entityRef='true'>#[Building - Campus]</RelationshipTypeId>
          <RelationshipDirection>Reverse</RelationshipDirection>
        </GroupedEntity>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>#[Campuses]</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid5'>
      <Expression xsi:type='AggregateExpression'>
        <NodeId>guid1</NodeId>
        <AggregateMethod>Min</AggregateMethod>
        <Expression xsi:type='ResourceExpression'>
          <NodeId>guid4</NodeId>
          <FieldId entityRef='true'>core:name</FieldId>
        </Expression>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestMinIff( )
        {
            var result = CreateQueryXml( "iif(min(Building.Rooms.[Room Type])='Lecture room',1,2)", "name:Campuses" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='AggregateEntity' id='guid1'>
        <GroupedEntity xsi:type='RelatedResource' id='guid2'>
          <RelatedEntities>
            <Entity xsi:type='RelatedResource' id='guid3'>
              <RelatedEntities>
                <Entity xsi:type='RelatedResource' id='guid4'>
                  <RelationshipTypeId entityRef='true'>#[Room type]</RelationshipTypeId>
                  <RelationshipDirection>Forward</RelationshipDirection>
                </Entity>
              </RelatedEntities>
              <RelationshipTypeId entityRef='true'>#[Building - Rooms]</RelationshipTypeId>
              <RelationshipDirection>Forward</RelationshipDirection>
            </Entity>
          </RelatedEntities>
          <RelationshipTypeId entityRef='true'>#[Building - Campus]</RelationshipTypeId>
          <RelationshipDirection>Reverse</RelationshipDirection>
        </GroupedEntity>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>#[Campuses]</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid5'>
      <Expression xsi:type='IfElseExpression'>
        <BooleanExpression xsi:type='ComparisonExpression'>
          <Operator>Equal</Operator>
          <Expressions>
            <Expression xsi:type='MutateExpression'>
              <Expression xsi:type='AggregateExpression'>
                <NodeId>guid1</NodeId>
                <AggregateMethod>Min</AggregateMethod>
                <Expression xsi:type='ResourceExpression'>
                  <NodeId>guid4</NodeId>
                  <FieldId entityRef='true'>core:name</FieldId>
                </Expression>
              </Expression>
              <MutateType>DisplaySql</MutateType>
            </Expression>
            <Expression xsi:type='LiteralExpression'>
              <Value type='String'>Lecture room</Value>
            </Expression>
          </Expressions>
        </BooleanExpression>
        <IfBlockExpression xsi:type='LiteralExpression'>
          <Value type='Int32'>1</Value>
        </IfBlockExpression>
        <ElseBlockExpression xsi:type='LiteralExpression'>
          <Value type='Int32'>2</Value>
        </ElseBlockExpression>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestSimpleUsesVariable( )
        {
            var result = CreateQueryXml( "let x = [Resource Type].Name select x", "name:AA_Manager" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>core:isOfType</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>test:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid1</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestVariableReused( )
        {
            var result = CreateQueryXml( "let x = [Resource Type] select x.Name + x.Description", "name:AA_Manager" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>core:isOfType</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>test:manager</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='CalculationExpression'>
        <Operator>Concatenate</Operator>
        <DisplayType xsi:type='StringType' />
        <Expressions>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:description</FieldId>
          </Expression>
        </Expressions>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Bug27975_1( )
        {
            //var result = CreateQueryXml( "let level = len([AA_Herb].[Name]) select iif([AA_Herb] is null, level, 'x' )", "name:AA_All Fields" );
            var result = CreateQueryXml( "let level = [AA_Herb].[Name] select [AA_Truck].[Name] + level", "name:AA_All Fields" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>test:trucks</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
      <Entity xsi:type='RelatedResource' id='guid2'>
        <RelationshipTypeId entityRef='true'>test:herbs</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>test:allFields</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid3'>
      <Expression xsi:type='CalculationExpression'>
        <Operator>Concatenate</Operator>
        <DisplayType xsi:type='StringType' />
        <Expressions>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid2</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
        </Expressions>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Bug28406( )
        {
            var result = CreateQueryXml( "let m = max([AA_All Fields].[DateTime]) select ([AA_All Fields] where [DateTime] = m).[Name]", "name:AA_Herb" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='AggregateEntity' id='guid1'>
        <GroupedEntity xsi:type='RelatedResource' id='guid2'>
          <RelationshipTypeId entityRef='true'>test:herbs</RelationshipTypeId>
          <RelationshipDirection>Reverse</RelationshipDirection>
        </GroupedEntity>
      </Entity>
      <Entity xsi:type='RelatedResource' id='guid3'>
        <Conditions>
          <Condition xsi:type='ComparisonExpression'>
            <Operator>Equal</Operator>
            <Expressions>
              <Expression xsi:type='ResourceDataColumn'>
                <NodeId>guid3</NodeId>
                <FieldId entityRef='true'>test:afDateTime</FieldId>
              </Expression>
              <Expression xsi:type='AggregateExpression'>
                <NodeId>guid1</NodeId>
                <AggregateMethod>Max</AggregateMethod>
                <Expression xsi:type='ResourceDataColumn'>
                  <NodeId>guid2</NodeId>
                  <FieldId entityRef='true'>test:afDateTime</FieldId>
                </Expression>
              </Expression>
            </Expressions>
          </Condition>
        </Conditions>
        <RelationshipTypeId entityRef='true'>test:herbs</RelationshipTypeId>
        <RelationshipDirection>Reverse</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>test:herb</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid4'>
      <Expression xsi:type='ResourceDataColumn'>
        <NodeId>guid3</NodeId>
        <FieldId entityRef='true'>core:name</FieldId>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Bug27975_2( )
        {
            //var result = CreateQueryXml( "let level = len([AA_Herb].[Name]) select iif([AA_Herb] is null, level, 'x' )", "name:AA_All Fields" );
            var result = CreateQueryXml( "let level = [AA_Herb].[Name] select [AA_Herb].[Name] + level", "name:AA_All Fields" );

            string expected =
@"<Query>
  <RootEntity xsi:type='ResourceEntity' id='guid0'>
    <RelatedEntities>
      <Entity xsi:type='RelatedResource' id='guid1'>
        <RelationshipTypeId entityRef='true'>test:herbs</RelationshipTypeId>
        <RelationshipDirection>Forward</RelationshipDirection>
      </Entity>
    </RelatedEntities>
    <EntityTypeId entityRef='true'>test:allFields</EntityTypeId>
  </RootEntity>
  <Columns>
    <Column id='guid2'>
      <Expression xsi:type='CalculationExpression'>
        <Operator>Concatenate</Operator>
        <DisplayType xsi:type='StringType' />
        <Expressions>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
          <Expression xsi:type='ResourceDataColumn'>
            <NodeId>guid1</NodeId>
            <FieldId entityRef='true'>core:name</FieldId>
          </Expression>
        </Expressions>
      </Expression>
    </Column>
  </Columns>
</Query>";
            Assert.AreEqual( expected, result );
        }
        #endregion

        public string CreateScalarExpressionXml(string script)
        {
            ScalarExpression queryExpr = CreateScalarExpression(script);
            string xml = Serializer<ScalarExpression>.ToXml(queryExpr);

            string xml2 = CleanXml(xml);
            return xml2;
        }

        public ScalarExpression CreateScalarExpression(string script)
        {
            BuilderSettings settings = new BuilderSettings();
            settings.RootContextType = ExprType.None;
            //settings.StaticParameterResolver
            //settings.ExpectedResultType

            IExpression expr = Factory.ExpressionCompiler.Compile(script, settings);

            QueryBuilderSettings qbSettings = new QueryBuilderSettings();
            //qbSettings.ContextEntity = 
            //qbSettings.StructuredQuery 

            ScalarExpression queryExpr = Factory.ExpressionCompiler.CreateQueryEngineExpression( expr, qbSettings );
            return queryExpr;
        }

        public string CreateQueryXml(string script, string rootType1)
        {
            EntityRef rootType;
            if ( rootType1.StartsWith( "name:" ) )
                rootType = new EntityRef( CodeNameResolver.GetTypeByName( rootType1.Substring( 5 ) ) );
            else
                rootType = (EntityRef)rootType1; // cast alias string

            StructuredQuery query = CreateQuery(script, rootType);
            string xml = Serializer<StructuredQuery>.ToXml(query);

            string xml2 = CleanXml(xml);
            return xml2;
        }

        public string CleanXml(string xml)
        {
            xml = xml.Replace("\"", "'");
            foreach (var n in Namespaces)
                xml = xml.Replace(n, "");

            int index = 0;
            Regex isGuid = new Regex(@"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}", RegexOptions.Compiled);

            while (true)
            {
                Match match = isGuid.Match(xml);
                if ( !match.Success )
                    break;
                    
                if (match.Value == "00000000-0000-0000-0000-000000000000")
                    xml = xml.Replace(match.Value, "empty-guid!!!");
                else
                    xml = xml.Replace(match.Value, "guid" + (index++));
            }

            Regex isId = new Regex( @"entityRef='true'\>[0-9][0-9]*", RegexOptions.Compiled );
            while ( true )
            {
                Match match = isId.Match( xml );
                if ( !match.Success )
                    break;
                long id = long.Parse( match.Value.Substring( "entityRef='true'>".Length ) );
                string name = EDC.ReadiNow.Model.Entity.GetName( id );

                xml = xml.Replace( match.Value, "entityRef='true'>#[" + name + "]" );
            }

            return xml.Replace( "empty-guid!!!", "00000000-0000-0000-0000-000000000000" );
        }

        public StructuredQuery CreateQuery(string script, EntityRef rootType)
        {
            StructuredQuery sq = new StructuredQuery();
            sq.RootEntity = new ResourceEntity
            {
                EntityTypeId = rootType
            };

            BuilderSettings settings = new BuilderSettings();
            settings.RootContextType = ExprTypeHelper.EntityOfType(rootType);
            settings.TestMode = true;
            IExpression expr = Factory.ExpressionCompiler.Compile(script, settings);
            string xml = expr.ToXml( );

            QueryBuilderSettings qbSettings = new QueryBuilderSettings();
            qbSettings.StructuredQuery = sq;
            qbSettings.ContextEntity = (ResourceEntity)sq.RootEntity;

            ScalarExpression queryExpr = Factory.ExpressionCompiler.CreateQueryEngineExpression( expr, qbSettings );
            sq.SelectColumns.Add(new SelectColumn { Expression = queryExpr });
            return sq;
        }
    }
}
