// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Specialized;
using System.Xml;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.Xml.Test
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     This is a test class for the XmlHelpers type
	/// </summary>
	[TestFixture]
	public class XmlHelperTests
	{
		private const string SimpleXmlDocument = @"
				<resources>
					<resource type=""bool"" data=""true"">value1</resource>
					<resource type=""int"" data=""10"">value2</resource>
					<resource type=""string"" data=""Hello World"">value3</resource>
				</resources>";

		/// <summary>
		///     Tests that the EvaluateNode returns false if no XML nodes matches the xpath expression.
		/// </summary>
		[Test]
		public void EvaluateNodes_InvalidXPath_ReturnsFalse( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool result = XmlHelper.EvaluateNodes( document.DocumentElement, "/resources/bad" );

			Assert.IsFalse( result );
		}

		/// <summary>
		///     Tests that EvaluateNode returns true if at least one XML node matches the xpath expression.
		/// </summary>
		[Test]
		public void EvaluateNodes_ValidXPath_ReturnsTrue( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool result = XmlHelper.EvaluateNodes( document.DocumentElement, "/resources/resource" );

			Assert.IsTrue( result );
		}

		/// <summary>
		///     Tests that EvaluateSingleNode returns false if no XML nodes match the xpath expression.
		/// </summary>
		[Test]
		public void EvaluateSingleNode_InvalidXPath_ReturnFalse( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool result = XmlHelper.EvaluateSingleNode( document.DocumentElement, "/resources/bad" );

			Assert.IsFalse( result );
		}

		/// <summary>
		///     Tests that EvaluateSingleNode returns true if an XML node matches the xpath expression.
		/// </summary>
		[Test]
		public void EvaluateSingleNode_ValidXPath_ReturnsTrue( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool result = XmlHelper.EvaluateSingleNode( document.DocumentElement, "/resources/resource" );

			Assert.IsTrue( result );
		}

		/// <summary>
		///     Tests that GetChildNodeNames returns the correct names of the child nodes.
		/// </summary>
		[Test]
		public void GetChildNodeName_ValidXPath_ReturnsNodeNames( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			StringCollection childNames = XmlHelper.GetChildNodeNames( document.DocumentElement );

			Assert.IsTrue( childNames.Count > 0 );
			Assert.AreEqual( childNames[ 0 ], "resource" );
		}

		/// <summary>
		///     Tests that ReadAttributeBool returns an alternate value if no attribute (bool) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeBool_InvalidXPathWithAlternate_ReturnsAlternateBool( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='bool']" );
			bool result = XmlHelper.ReadAttributeBool( xmlNode, "@bad", true );

			Assert.AreEqual( result, true );
		}

		/// <summary>
		///     Tests that ReadAttributeBool throws an exception if no attribute (bool) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeBool_InvalidXPath_ThrowsException( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool exception = false;

			try
			{
				XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='bool']" );
				XmlHelper.ReadAttributeBool( xmlNode, "@bad" );
			}
			catch
			{
				exception = true;
			}

			Assert.IsTrue( exception );
		}

		/// <summary>
		///     Tests that ReadElementBool returns the correct attribute (bool) for a valid xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeBool_ValidXPath_ReturnsBool( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='bool']" );
			bool result = XmlHelper.ReadAttributeBool( xmlNode, "@data" );

			Assert.AreEqual( result, true );
		}

		/// <summary>
		///     Tests that ReadAttributeInt returns an alternate value if no attribute (int) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeInt_InvalidXPathWithAlternate_ReturnsAlternateInt( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='int']" );
			int result = XmlHelper.ReadAttributeInt( xmlNode, "@bad", 10 );

			Assert.AreEqual( result, 10 );
		}

		/// <summary>
		///     Tests that ReadAttributeInt throws an exception if no attribute (int) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeInt_InvalidXPath_ThrowsException( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool exception = false;

			try
			{
				XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='int']" );
				XmlHelper.ReadAttributeInt( xmlNode, "@bad" );
			}
			catch
			{
				exception = true;
			}

			Assert.IsTrue( exception );
		}

		/// <summary>
		///     Tests that ReadAttributeInt returns the correct attribute (int) for a valid xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeInt_ValidXPath_ReturnsInt( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='int']" );
			int result = XmlHelper.ReadAttributeInt( xmlNode, "@data" );

			Assert.AreEqual( result, 10 );
		}

		/// <summary>
		///     Tests that ReadAttributeString returns an alternate value if no attribute (string) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeString_InvalidXPathWithAlternate_ReturnsAlternateString( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='string']" );
			string result = XmlHelper.ReadAttributeString( xmlNode, "@bad", "Hello World" );

			Assert.AreEqual( result, "Hello World" );
		}

		/// <summary>
		///     Tests that ReadAttributeString throws an exception if no attribute (string) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeString_InvalidXPath_ThrowsException( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool exception = false;

			try
			{
				XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='string']" );
				XmlHelper.ReadAttributeString( xmlNode, "@bad" );
			}
			catch
			{
				exception = true;
			}

			Assert.IsTrue( exception );
		}

		/// <summary>
		///     Tests that ReadElementString returns the correct attribute (string) for a valid xpath expression.
		/// </summary>
		[Test]
		public void ReadAttributeString_ValidXPath_ReturnsString( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode xmlNode = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource[@type='string']" );
			string result = XmlHelper.ReadAttributeString( xmlNode, "@data" );

			Assert.AreEqual( result, "Hello World" );
		}

		/// <summary>
		///     Tests that ReadElementString returns an alternate value if no element (string) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadElementString_InvalidXPathWithAlternate_ReturnsAlternateString( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			string result = XmlHelper.ReadElementString( document.DocumentElement, "/resources/bad", "value1" );
			Assert.AreEqual( result, "value1" );
		}

		/// <summary>
		///     Tests that ReadElementString throws an exception if no element (string) matches the xpath expression.
		/// </summary>
		[Test]
		public void ReadElementString_InvalidXPath_ThrowsException( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool exception = false;

			try
			{
				XmlHelper.ReadElementString( document.DocumentElement, "/resources/bad" );
			}
			catch
			{
				exception = true;
			}

			Assert.IsTrue( exception );
		}

		/// <summary>
		///     Tests that ReadElementString returns the correct element (string) for a valid xpath expression.
		/// </summary>
		[Test]
		public void ReadElementString_ValidXPath_ReturnsString( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			string result = XmlHelper.ReadElementString( document.DocumentElement, "/resources/resource" );
			Assert.AreEqual( result, "value1" );
		}

		/// <summary>
		///     Tests that SelectNodes returns an empty list if no XML nodes match the xpath expression.
		/// </summary>
		[Test]
		public void SelectNodes_InvalidXPath_ReturnsEmptyXmlNodes( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNodeList nodeList = XmlHelper.SelectNodes( document.DocumentElement, "/resources/bad" );

			Assert.IsTrue( nodeList.Count == 0 );
		}

		/// <summary>
		///     Tests that SelectNodes returns the XML nodes that matches the xpath expression.
		/// </summary>
		[Test]
		public void SelectNodes_ValidXPath_ReturnsXmlNodes( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNodeList nodeList = XmlHelper.SelectNodes( document.DocumentElement, "/resources/resource" );

			Assert.IsTrue( nodeList.Count > 0 );
		}

		/// <summary>
		///     Tests that SelectSingleNode throws an exception if no XML nodes match the xpath expression.
		/// </summary>
		[Test]
		public void SelectSingleNode_InvalidXPath_ThrowsException( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			bool exception = false;

			try
			{
				XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/bad" );
			}
			catch
			{
				exception = true;
			}

			Assert.IsTrue( exception );
		}

		/// <summary>
		///     Tests that SelectSingleNode returns the first XML node that matches the xpath expression.
		/// </summary>
		[Test]
		public void SelectSingleNode_ValidXPath_ReturnsXmlNode( )
		{
			var document = new XmlDocument( );
			document.LoadXml( SimpleXmlDocument );

			XmlNode node = XmlHelper.SelectSingleNode( document.DocumentElement, "/resources/resource" );

			Assert.AreEqual( node.Name, "resource" );
		}
	}
}