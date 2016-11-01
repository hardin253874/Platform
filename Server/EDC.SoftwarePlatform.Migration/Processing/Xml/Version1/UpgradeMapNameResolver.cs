// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version1
{
	/// <summary>
	///     Class representing the UpgradeMapNameResolver type.
	/// </summary>
	/// <seealso cref="EDC.SoftwarePlatform.Migration.Contract.INameResolver" />
	public class UpgradeMapNameResolver : INameResolver
	{
		/// <summary>
		///     The cache
		/// </summary>
		private readonly Dictionary<Guid, EntityAlias> _cache = new Dictionary<Guid, EntityAlias>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="UpgradeMapNameResolver" /> class.
		/// </summary>
		public UpgradeMapNameResolver( )
		{
			Assembly assembly = Assembly.GetExecutingAssembly( );

			const string resourceName = "EDC.SoftwarePlatform.Migration.Processing.Xml.Version1.UpgradeMap.xml";

			using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
			{
				if ( stream != null )
				{
					XElement element = XElement.Load( stream );

					_cache = element.Descendants( "entity" ).ToDictionary( e => new Guid( e.Attribute( "upgradeId" ).Value ), e => new EntityAlias( e.Attribute( "alias" ).Value ) );
				}
			}
		}

		/// <summary>
		///     Resolves the specified upgrade identifier.
		/// </summary>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <returns></returns>
		public EntityAlias Resolve( Guid upgradeId )
		{
			EntityAlias alias;

			_cache.TryGetValue( upgradeId, out alias );

			return alias;
		}
	}
}