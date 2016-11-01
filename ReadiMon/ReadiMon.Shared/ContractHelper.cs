// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ReadiMon.Shared
{
	/// <summary>
	///     Contract helper
	/// </summary>
	public static class ContractHelper
	{
		/// <summary>
		///     Converts to XML.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public static string ConvertToXml<T>( T instance )
		{
			if ( Equals( instance, default( T ) ) )
			{
				return string.Empty;
			}

			var ns = new XmlSerializerNamespaces( );
			ns.Add( "", "" );

			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true
			};

			var sb = new StringBuilder( );

			using ( XmlWriter writer = XmlWriter.Create( sb, settings ) )
			{
				var x = new XmlSerializer( typeof ( T ) );
				x.Serialize( writer, instance, ns );
			}

			return sb.ToString( );
		}
	}
}