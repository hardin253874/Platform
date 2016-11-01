// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.DocumentLibrary
{
	[TestFixture]
	public class DocumentLibraryTests
	{
		private const string BpSemanticDbModelingDocx = @"BPSemanticDBModeling.docx";
		private const string BpSemanticDbModelingChanged = @"BPSemanticDBModelingChanged.docx";
		private const string BpSemanticDbModelingPdf = @"BPSemanticDBModeling.pdf";

		[TestFixtureSetUp]
		public static void TestClassInitialize( )
		{
			Assembly assembly = Assembly.GetExecutingAssembly( );
			using ( Stream originalDocumentStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.DocumentLibrary.BPSemanticDBModeling.docx" ) )
			{
				using ( FileStream fs = File.Create( BpSemanticDbModelingDocx ) )
				{
					if ( originalDocumentStream != null )
					{
						originalDocumentStream.CopyTo( fs );
					}

					fs.Flush( true );
				}
			}

			using ( Stream changedDocumentStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.DocumentLibrary.BPSemanticDBModelingChanged.docx" ) )
			{
				using ( FileStream fs = File.Create( BpSemanticDbModelingChanged ) )
				{
					if ( changedDocumentStream != null )
					{
						changedDocumentStream.CopyTo( fs );
					}

					fs.Flush( true );
				}
			}
			using ( Stream pdfDocumentStream = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.WebApi.Test.DocumentLibrary.BPSemanticDBModeling.pdf" ) )
			{
				using ( FileStream fs = File.Create( BpSemanticDbModelingPdf ) )
				{
					if ( pdfDocumentStream != null )
					{
						pdfDocumentStream.CopyTo( fs );
					}

					fs.Flush( true );
				}
			}
		}

		[TestFixtureTearDown]
		public static void TestClassCleanup( )
		{
			// Delete all files used for the test
			File.Delete( BpSemanticDbModelingDocx );
			File.Delete( BpSemanticDbModelingPdf );
			File.Delete( BpSemanticDbModelingChanged );
		}
	}
}