// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using EDC.SoftwarePlatform.Migration.Processing;

namespace TenantDiffTool
{
	/// <summary>
	///     Class representing the UpgradeIdCache type.
	/// </summary>
	public static class UpgradeIdCache
	{
		/// <summary>
		///     Initializes the <see cref="UpgradeIdCache" /> class.
		/// </summary>
		static UpgradeIdCache( )
		{
			Assembly assembly = Assembly.GetExecutingAssembly( );

			const string resourceName = "TenantDiffTool.SupportClasses.UpgradeMap.xml";

			using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
			{
				if ( stream != null )
				{
					XElement element = XElement.Load( stream );

					Instance = element.Descendants( XmlConstants.EntityConstants.Entity ).ToDictionary( e => new Guid( e.Attribute( XmlConstants.UpgradeId ).Value ), e => e.Attribute( "alias" ).Value );
				}
			}
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		/// <value>
		///     The instance.
		/// </value>
		public static Dictionary<Guid, string> Instance
		{
			get;
			private set;
		}
	}
}