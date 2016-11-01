// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text;
using System.Xml;
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Query.Structured
{
	/// <summary>
	///     This class tests the ConditionHelper class
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ConditionHelperTests
	{
		/// <summary>
		///     Tests that the ReadConditionXml method throws the correct exception
		///     when a null element name is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadConditionXmlEmptyElemementNameTest( )
		{
			var doc = new XmlDocument( );
			doc.LoadXml( "<xml></xml>" );
			ConditionHelper.ReadConditionXml( doc.DocumentElement, string.Empty );
		}


		/// <summary>
		///     Tests that the ReadConditionXml method returns the correct condition
		///     object when invalid xml is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void ReadConditionXmlInvalidXmlTest( )
		{
			var doc = new XmlDocument( );
			doc.LoadXml( "<xml><condition></condition></xml>" );
			Condition condition = ConditionHelper.ReadConditionXml( doc.DocumentElement, "condition" );
			Assert.AreEqual( ConditionType.Unspecified, condition.Operator, "The value of Condition.Operator is invalid." );
			Assert.AreEqual( string.Empty, condition.ColumnName, "The value of Condition.ColumnName is invalid." );

			Assert.AreEqual( DatabaseType.UnknownType.GetType( ), condition.ColumnType.GetType( ), "The value of Condition.ColumnType is invalid." );
			Assert.AreEqual( 0, condition.Arguments.Count, "The value of Arguments.Count is invalid." );
		}

		/// <summary>
		///     Tests that the ReadConditionXml method throws the correct exception
		///     when a null element name is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadConditionXmlNullElementNameTest( )
		{
			var doc = new XmlDocument( );
			doc.LoadXml( "<xml></xml>" );
			ConditionHelper.ReadConditionXml( doc.DocumentElement, null );
		}

		/// <summary>
		///     Tests that the ReadConditionXml method throws the correct exception
		///     when a null node is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadConditionXmlNullNodeTest( )
		{
			ConditionHelper.ReadConditionXml( null, "condition" );
		}


		/// <summary>
		///     Tests that the WriteConditionXml throws the correct exception
		///     when an empty element name is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteConditionXmlEmptyElementNameTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} );

			ConditionHelper.WriteConditionXml( string.Empty, new Condition( ), writer );
		}


		/// <summary>
		///     Tests that the WriteConditionXml throws the correct exception
		///     when a null condition value is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteConditionXmlNullConditionTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} );

			ConditionHelper.WriteConditionXml( "condition", null, writer );
		}

		/// <summary>
		///     Tests that the WriteConditionXml throws the correct exception
		///     when a null element name is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteConditionXmlNullElementNameTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} );

			ConditionHelper.WriteConditionXml( null, new Condition( ), writer );
		}


		/// <summary>
		///     Tests that the WriteConditionXml throws the correct exception
		///     when a null xml writer is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteConditionXmlNullXmlWriterTest( )
		{
			ConditionHelper.WriteConditionXml( "condition", new Condition( ), null );
		}
	}
}