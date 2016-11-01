// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text;
using System.Xml;
using EDC.ReadiNow.Metadata.Media;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Media
{
	/// <summary>
	///     This class tests the ColorInfoHelper class
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ColorInfoHelperTests
	{
		/// <summary>
		///     Tests that the ReadColorInfoXml method throws the correct exception
		///     when a null element name is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadColorInfoXmlEmptyElemementNameTest( )
		{
			var doc = new XmlDocument( );
			doc.LoadXml( "<xml></xml>" );
			ColorInfoHelper.ReadColorInfoXml( doc.DocumentElement, string.Empty );
		}


		/// <summary>
		///     Tests that the ReadColorInfoXml method returns the correct color info
		///     object when invalid xml is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void ReadColorInfoXmlInvalidXmlTest( )
		{
			var doc = new XmlDocument( );
			doc.LoadXml( "<xml><color></color></xml>" );
			ColorInfo color = ColorInfoHelper.ReadColorInfoXml( doc.DocumentElement, "color" );
			Assert.AreEqual( 0, color.A, "The value of Color.A is invalid." );
			Assert.AreEqual( 0, color.R, "The value of Color.R is invalid." );
			Assert.AreEqual( 0, color.G, "The value of Color.G is invalid." );
			Assert.AreEqual( 0, color.B, "The value of Color.B is invalid." );
		}

		/// <summary>
		///     Tests that the ReadColorInfoXml method throws the correct exception
		///     when a null element name is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadColorInfoXmlNullElementNameTest( )
		{
			var doc = new XmlDocument( );
			doc.LoadXml( "<xml></xml>" );
			ColorInfoHelper.ReadColorInfoXml( doc.DocumentElement, null );
		}

		/// <summary>
		///     Tests that the ReadColorInfoXml method throws the correct exception
		///     when a null node is used.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadColorInfoXmlNullNodeTest( )
		{
			ColorInfoHelper.ReadColorInfoXml( null, "color" );
		}


		/// <summary>
		///     Tests that the WriteColorInfoXml throws the correct exception
		///     when an empty element name is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteColorInfoXmlEmptyElementNameTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} );

			ColorInfoHelper.WriteColorInfoXml( string.Empty, new ColorInfo( ), writer );
		}


		/// <summary>
		///     Tests that the WriteColorInfoXml throws the correct exception
		///     when a null color info value is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteColorInfoXmlNullColorInfoTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} );

			ColorInfoHelper.WriteColorInfoXml( "color", null, writer );
		}

		/// <summary>
		///     Tests that the WriteColorInfoXml throws the correct exception
		///     when a null element name is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteColorInfoXmlNullElementNameTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} );

			ColorInfoHelper.WriteColorInfoXml( null, new ColorInfo( ), writer );
		}


		/// <summary>
		///     Tests that the WriteColorInfoXml throws the correct exception
		///     when a null xml writer is passed.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteColorInfoXmlNullXmlWriterTest( )
		{
			ColorInfoHelper.WriteColorInfoXml( "color", new ColorInfo( ), null );
		}
	}
}