// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using EDC.SoftwarePlatform.Install.Configure;

namespace SolutionHash
{
	internal class Program
	{
		/////
		// MD5 hashing.
		/////
		private static MD5 _md5;

		/// <summary>
		///     Gets the hash of the specified file.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		private static string GetFileHash( string filename )
		{
			return Convert.ToBase64String( _md5.ComputeHash( File.ReadAllBytes( filename ) ) );
		}

		/// <summary>
		///     Gets the hash of the specified string.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		private static string GetStringHash( string input )
		{
			return Convert.ToBase64String( _md5.ComputeHash( Encoding.Default.GetBytes( input ) ) );
		}

		/// <summary>
		///     Mains program entry point.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		/// <exception cref="System.ArgumentException">No -in argument specified</exception>
		private static void Main( string[ ] args )
		{
			var parser = new CommandLineParser( args );

			if ( parser.Count < 1 || parser.Count > 2 )
			{
				Console.WriteLine( @"Usage: SolutionHash.exe -in <solution.xml> -out <solution.hash>" );
				Console.WriteLine( );
				Console.WriteLine( @"  -in      The configuration file containing the solution elements to be hashed." );
				Console.WriteLine( @"  -out     The output file containing the solution hash." );
				Console.WriteLine( );
				return;
			}

			/////
			// Read the -in argument.
			/////
			if ( !parser.ContainsArgument( "-in" ) )
			{
				throw new ArgumentException( "No -in argument specified" );
			}

			var inputFilename = parser.ValueForArgument<string>( "-in" );

			if ( !File.Exists( inputFilename ) )
			{
				throw new ArgumentException( "Invalid xml solution file." );
			}

			/////
			// Read the -out argument (if specified).
			/////
			if ( !parser.ContainsArgument( "-out" ) )
			{
				throw new ArgumentException( "No -out argument specified" );
			}

			var outputFilename = parser.ValueForArgument<string>( "-out" );

			Console.Out.WriteLine( ProcessSolutionFile( inputFilename, outputFilename ).ToString( ).ToLowerInvariant( ) );
		}

		/// <summary>
		///     Processes the solution file.
		/// </summary>
		/// <param name="inputFilename">The input filename.</param>
		/// <param name="outputFilename">The output filename.</param>
		/// <returns></returns>
		/// <exception cref="System.Xml.XmlException">Invalid xml solution file.</exception>
		/// <exception cref="System.ArgumentException">Invalid xml solution file specified.</exception>
		private static bool ProcessSolutionFile( string inputFilename, string outputFilename )
		{
			/////
			// Use MD5 as the hashing algorithm since it is fast and
			// collisions have only been found using pre-crafted inputs.
			/////
			_md5 = MD5.Create( );

			/////
			// Load the solution file.
			/////
			var doc = new XmlDocument( );
			doc.Load( inputFilename );

			XmlNode element = doc.SelectSingleNode( "/resource" );

			if ( element == null )
			{
				throw new XmlException( "Invalid xml solution file." );
			}

			string configFolder = Path.GetDirectoryName( inputFilename );

			if ( configFolder == null )
			{
				throw new ArgumentException( "Invalid xml solution file specified." );
			}

			var rootElement = new
			{
				Node = element,
				Path = inputFilename,
				Root = true
			};

			var files = ( new[ ]
			{
				rootElement
			} ).ToList( );

			/////
			// Child node to process.
			/////
			string[ ] children =
			{
				"configuration/metadata/entities",
				"configuration/database/rawSql",
				"configuration/database/bulk",
				"configuration/database/scripts",
				"configuration/metadata/upgradeMap",
				"configuration/database/upgradeRawSql"
			};

			foreach ( string child in children )
			{
				/////
				// Process the metadata entities (if specified)
				/////
				XmlNodeList nodes = element.SelectNodes( child );

				if ( nodes != null && nodes.Count > 0 )
				{
					foreach ( XmlNode node in nodes )
					{
						string path = Path.Combine( Path.Combine( configFolder, node.ReadAttributeString( "@designTimeFolder", string.Empty ) ), node.ReadAttributeString( "@path", string.Empty ) );

						if ( File.Exists( path ) )
						{
							files.Insert( 0, new
							{
								Node = node,
								Path = path,
								Root = false
							} );
						}
					}
				}
			}

			bool modified = true;

			var completeHash = new StringBuilder( );

			foreach ( var file in files )
			{
				string newHash;

				/////
				// Root element has some special processing done.
				/////
				if ( file.Root )
				{
					XmlNode rootClone = element.Clone( );

					/////
					// Locate and remove the existing hash attribute (if one exists).
					/////
					if ( rootClone.Attributes != null )
					{
						XmlAttribute hashAttribute = rootClone.Attributes[ "hash" ];

						if ( hashAttribute != null )
						{
							rootClone.Attributes.Remove( hashAttribute );
						}
					}

					newHash = GetStringHash( rootClone.OuterXml );
				}
				else
				{
					/////
					// Hash the entire file.
					/////
					newHash = GetFileHash( file.Path );
				}

				/////
				// Build the complete hash.
				/////
				completeHash.Append( newHash );
			}

			string hash = GetStringHash( completeHash.ToString( ) );

			if ( File.Exists( outputFilename ) )
			{
				var existing = File.ReadAllText( outputFilename );

				modified = existing != hash;
			}

			File.WriteAllText( outputFilename, hash );

			return modified;
		}
	}
}