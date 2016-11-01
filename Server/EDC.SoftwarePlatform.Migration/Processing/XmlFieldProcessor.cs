// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Xml Conversion.
	/// </summary>
	public enum XmlConversionMode
	{
		/// <summary>
		///     Converts from tenant-specific IDs to UpgradeId (GUID).
		/// </summary>
		LocalIdToUpgradeGuid,

		/// <summary>
		///     Converts from UpgradeId (GUID) to tenant-specific IDs.
		/// </summary>
		UpgradeGuidToLocalId
	}

	/// <summary>
	///     Mechanism for handling XML fields during import.
	///     XML fields often contain references to entities.
	///     References may be ID, or by alias. Both need updating (although the latter only changes occasionally)
	/// </summary>
	public class XmlFieldProcessor
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="XmlFieldProcessor" /> class.
		/// </summary>
		public XmlFieldProcessor( )
		{
			/////
			// Create the caches.
			/////
			AliasToGuidCache = new Dictionary<string, Guid>( );
			AliasToLongCache = new Dictionary<string, long>( );
			GuidToLongCache = new Dictionary<Guid, long>( );
			LongToGuidCache = new Dictionary<long, Guid>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlFieldProcessor" /> class.
		/// </summary>
		/// <param name="aliasToGuidCache">The alias automatic unique identifier cache.</param>
		/// <param name="aliasToLongCache">The alias automatic long cache.</param>
		/// <param name="guidToLongCache">The unique identifier automatic long cache.</param>
		/// <param name="longToGuidCache">The long automatic unique identifier cache.</param>
		public XmlFieldProcessor( Dictionary<string, Guid> aliasToGuidCache = null, Dictionary<string, long> aliasToLongCache = null, Dictionary<Guid, long> guidToLongCache = null, Dictionary<long, Guid> longToGuidCache = null )
			: this( )
		{
			if ( aliasToGuidCache != null )
			{
				AliasToGuidCache = aliasToGuidCache;
			}

			if ( aliasToLongCache != null )
			{
				AliasToLongCache = aliasToLongCache;
			}

			if ( guidToLongCache != null )
			{
				GuidToLongCache = guidToLongCache;
			}

			if ( longToGuidCache != null )
			{
				LongToGuidCache = longToGuidCache;
			}
		}


		/// <summary>
		///     Type of conversion being performed.
		/// </summary>
		/// <value>
		///     The conversion mode.
		/// </value>
		public XmlConversionMode ConversionMode
		{
			get;
			set;
		}

		/// <summary>
		///     Database context to use for queries.
		/// </summary>
		/// <value>
		///     The database context.
		/// </value>
		public DatabaseContext DatabaseContext
		{
			get;
			set;
		}

		/// <summary>
		///     The ID of the batch used to hold data in the ImportMap table on the target database.
		/// </summary>
		/// <value>
		///     The import map batch id.
		/// </value>
		public long ImportMapBatchId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the processing context.
		/// </summary>
		/// <value>
		///     The processing context.
		/// </value>
		public IProcessingContext ProcessingContext
		{
			get;
			set;
		}

		/// <summary>
		///     The tenant on the target system that we're importing into.
		/// </summary>
		/// <value>
		///     The tenant id.
		/// </value>
		public long TenantId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the alias automatic unique identifier cache.
		/// </summary>
		/// <value>
		///     The alias automatic unique identifier cache.
		/// </value>
		private Dictionary<string, Guid> AliasToGuidCache
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the alias automatic long cache.
		/// </summary>
		/// <value>
		///     The alias automatic long cache.
		/// </value>
		private Dictionary<string, long> AliasToLongCache
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the unique identifier automatic long cache.
		/// </summary>
		/// <value>
		///     The unique identifier automatic long cache.
		/// </value>
		private Dictionary<Guid, long> GuidToLongCache
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the long automatic unique identifier cache.
		/// </summary>
		/// <value>
		///     The long automatic unique identifier cache.
		/// </value>
		private Dictionary<long, Guid> LongToGuidCache
		{
			get;
			set;
		}

		/// <summary>
		///     Remaps the XML entities.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public string RemapXmlEntities( Guid entityUpgradeId, Guid fieldUpgradeId, string xml )
		{
			if ( xml == null )
			{
				return null;
			}

			if ( xml.Trim( ) == string.Empty )
			{
				return xml;
			}

			try
			{
				using ( var stringReader = new StringReader( xml ) )
				{
					XDocument doc = XDocument.Load( stringReader, LoadOptions.SetLineInfo );

					IEnumerable<XElement> candidates = EntityRefHelper.GetEntityRefTextNodes( doc );

					IList<XElement> xmlTextNodes = candidates as IList<XElement> ?? candidates.ToList( );

					if ( candidates != null && xmlTextNodes.Count > 0 )
					{
						if ( ConversionMode == XmlConversionMode.LocalIdToUpgradeGuid )
						{
							ConvertFromLocalIdToUpgradeId( entityUpgradeId, fieldUpgradeId, xmlTextNodes );
						}
						else if ( ConversionMode == XmlConversionMode.UpgradeGuidToLocalId )
						{
							ConvertFromUpgradeIdToLocalId( entityUpgradeId, fieldUpgradeId, xmlTextNodes );
						}
					}

					return doc.ToString( );
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to remap xml entities. " + exc );
				return xml;
			}
		}

		/// <summary>
		///     Locates and remaps all EntityRefs in an XML document.
		///     Values are mapped from a data store ID space, to the new local database.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns>
		///     The remapped XML.
		/// </returns>
		public string RemapXmlEntities( string xml )
		{
			return RemapXmlEntities( Guid.Empty, Guid.Empty, xml );
		}

		/// <summary>
		///     Adds the element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="element">The element.</param>
		private void AddElement<T>( Dictionary<T, IList<XElement>> dictionary, T key, XElement element )
		{
			if ( dictionary == null || element == null )
			{
				return;
			}

			IList<XElement> nodes;

			if ( !dictionary.TryGetValue( key, out nodes ) )
			{
				nodes = new List<XElement>( );
				dictionary[ key ] = nodes;
			}

			nodes.Add( element );
		}

		/// <summary>
		///     Checks the GUIDs.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="checkGuids">The check GUIDs.</param>
		private void CheckGuids( Guid entityUpgradeId, Guid fieldUpgradeId, Dictionary<Guid, IList<XElement>> checkGuids )
		{
			if ( checkGuids == null )
			{
				return;
			}

			string sql = string.Format( @"
SELECT
	e.UpgradeId
FROM (
	VALUES {0}
) a( Guid )
LEFT JOIN
	Entity e ON e.TenantId = @tenant AND e.UpgradeId = a.Guid
WHERE
	e.Id IS NOT NULL", string.Join( ",", checkGuids.Keys.Select( k => string.Format( "('{0}')", k ) ).ToArray( ) ) );

			using ( IDbCommand cmd = DatabaseContext.CreateCommand( ) )
			{
				cmd.CommandText = sql;
				cmd.AddParameter( "@tenant", DbType.Int64, TenantId );

				using ( IDataReader reader = cmd.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							checkGuids.Remove( reader.GetGuid( 0 ) );
						}
					}
				}
			}

			if ( checkGuids.Count > 0 )
			{
				if ( ProcessingContext != null )
				{
					foreach ( var pair in checkGuids )
					{
						if ( pair.Value != null )
						{
							foreach ( XElement xmlText in pair.Value )
							{
								ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, xmlText, pair.Key, XmlConversionMode.LocalIdToUpgradeGuid, XmlFailureReason.UnknownUpgradeId, FailureLevel.Warning ) );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Checks the longs.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="checkLongs">The check longs.</param>
		private void CheckLongs( Guid entityUpgradeId, Guid fieldUpgradeId, Dictionary<long, IList<XElement>> checkLongs )
		{
			if ( checkLongs == null )
			{
				return;
			}

			string sql = string.Format( @"
SELECT
	e.Id
FROM (
	VALUES {0}
) a( Id )
LEFT JOIN
	Entity e ON e.TenantId = @tenant AND e.Id = a.Id
WHERE
	e.UpgradeId IS NOT NULL", string.Join( ",", checkLongs.Keys.Select( k => string.Format( "({0})", k ) ).ToArray( ) ) );

			using ( IDbCommand cmd = DatabaseContext.CreateCommand( ) )
			{
				cmd.CommandText = sql;
				cmd.AddParameter( "@tenant", DbType.Int64, TenantId );

				using ( IDataReader reader = cmd.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							checkLongs.Remove( reader.GetInt64( 0 ) );
						}
					}
				}
			}

			if ( checkLongs.Count > 0 )
			{
				if ( ProcessingContext != null )
				{
					foreach ( var pair in checkLongs )
					{
						if ( pair.Value != null )
						{
							foreach ( XElement xmlText in pair.Value )
							{
								ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, xmlText, pair.Key, XmlConversionMode.UpgradeGuidToLocalId, XmlFailureReason.UnknownLocalId, FailureLevel.Warning ) );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Converts from local tenant identifier to its upgrade identifier.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="candidates">The candidates.</param>
		private void ConvertFromLocalIdToUpgradeId( Guid entityUpgradeId, Guid fieldUpgradeId, IEnumerable<XElement> candidates )
		{
			if ( candidates == null )
			{
				return;
			}

			var checkGuids = new Dictionary<Guid, IList<XElement>>( );
			var missingLongs = new Dictionary<long, IList<XElement>>( );
			var missingAliases = new Dictionary<string, IList<XElement>>( );

			foreach ( XElement node in candidates )
			{
				string value = node.Value.Trim( );

				long? nullableLong;
				Guid? nullableGuid;
				string alias;

				if ( !TryParseIdGuidAlias( value, out nullableLong, out nullableGuid, out alias ) )
				{
					if ( ProcessingContext != null )
					{
						ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, node, value, XmlConversionMode.LocalIdToUpgradeGuid, XmlFailureReason.InvalidSource, FailureLevel.Warning ) );
					}

					/////
					// Unable to determine what this is.
					/////
					continue;
				}

				if ( nullableGuid != null )
				{
					/////
					// GUID -> GUID conversion.
					/////
					node.Value = nullableGuid.Value.ToString( );

					AddElement( checkGuids, nullableGuid.Value, node );
				}
				else if ( nullableLong != null )
				{
					Guid guid;

					/////
					// Check cache
					/////
					if ( !LongToGuidCache.TryGetValue( nullableLong.Value, out guid ) )
					{
						AddElement( missingLongs, nullableLong.Value, node );
					}
					else
					{
						node.Value = guid.ToString( );
					}
				}
				else if ( !string.IsNullOrEmpty( alias ) )
				{
					Guid guid;

					if ( alias.IndexOf( ':' ) < 0 )
					{
						/////
						// No namespace, append 'core'
						/////
						alias = string.Format( "core:{0}", alias );
					}

					/////
					// Check cache.
					/////
					if ( !AliasToGuidCache.TryGetValue( alias, out guid ) )
					{
						AddElement( missingAliases, alias, node );
					}
					else
					{
						node.Value = guid.ToString( );
					}
				}
			}

			/////
			// Resolve the missing longs.
			/////
			if ( missingLongs.Count > 0 )
			{
				LookupLongToGuid( entityUpgradeId, fieldUpgradeId, missingLongs, ( a, b ) => LongToGuidCache[ a ] = b );
			}

			/////
			// Resolve the missing aliases.
			/////
			if ( missingAliases.Count > 0 )
			{
				LookupAliasToGuid( entityUpgradeId, fieldUpgradeId, missingAliases, ( a, b ) => AliasToGuidCache[ a ] = b );
			}

			/////
			// Check the GUIDs.
			/////
			if ( checkGuids.Count > 0 )
			{
				CheckGuids( entityUpgradeId, fieldUpgradeId, checkGuids );
			}
		}

		/// <summary>
		///     Converts from upgrade unique identifier automatic local unique identifier.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="candidates">The candidates.</param>
		private void ConvertFromUpgradeIdToLocalId( Guid entityUpgradeId, Guid fieldUpgradeId, IEnumerable<XElement> candidates )
		{
			if ( candidates == null )
			{
				return;
			}

			var checkLongs = new Dictionary<long, IList<XElement>>( );
			var missingGuids = new Dictionary<Guid, IList<XElement>>( );
			var missingAliases = new Dictionary<string, IList<XElement>>( );

			foreach ( XElement node in candidates )
			{
				string value = node.Value.Trim( );

				long? nullableLong;
				Guid? nullableGuid;
				string alias;

				if ( !TryParseIdGuidAlias( value, out nullableLong, out nullableGuid, out alias ) )
				{
					if ( ProcessingContext != null )
					{
						ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, node, value, XmlConversionMode.UpgradeGuidToLocalId, XmlFailureReason.InvalidSource, FailureLevel.Warning ) );
					}

					/////
					// Unable to determine what this is.
					/////
					continue;
				}

				if ( nullableLong != null )
				{
					/////
					// long -> long conversion
					/////
					node.Value = nullableLong.Value.ToString( CultureInfo.InvariantCulture );

					AddElement( checkLongs, nullableLong.Value, node );
				}
				else if ( nullableGuid != null )
				{
					long l;

					/////
					// Check cache.
					/////
					if ( !GuidToLongCache.TryGetValue( nullableGuid.Value, out l ) )
					{
						AddElement( missingGuids, nullableGuid.Value, node );
					}
					else
					{
						node.Value = l.ToString( CultureInfo.InvariantCulture );
					}
				}
				else if ( !string.IsNullOrEmpty( alias ) )
				{
					long l;

					if ( alias.IndexOf( ':' ) < 0 )
					{
						/////
						// No namespace, append 'core'
						/////
						alias = string.Format( "core:{0}", alias );
					}

					/////
					// Check cache.
					/////
					if ( !AliasToLongCache.TryGetValue( alias, out l ) )
					{
						AddElement( missingAliases, alias, node );
					}
					else
					{
						node.Value = l.ToString( CultureInfo.InvariantCulture );
					}
				}
			}

			/////
			// Resolve the missing GUIDs.
			/////
			if ( missingGuids.Count > 0 )
			{
				LookupGuidToLong( entityUpgradeId, fieldUpgradeId, missingGuids, ( a, b ) => GuidToLongCache[ a ] = b );
			}

			/////
			// Resolve the missing aliases.
			/////
			if ( missingAliases.Count > 0 )
			{
				LookupAliasToLong( entityUpgradeId, fieldUpgradeId, missingAliases, ( a, b ) => AliasToLongCache[ a ] = b );
			}

			/////
			// Check the longs.
			/////
			if ( checkLongs.Count > 0 )
			{
				CheckLongs( entityUpgradeId, fieldUpgradeId, checkLongs );
			}
		}


		/// <summary>
		///     Lookups the alias automatic unique identifier.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="aliasNodes">The alias nodes.</param>
		/// <param name="aliasAction">The alias action.</param>
		private void LookupAliasToGuid( Guid entityUpgradeId, Guid fieldUpgradeId, Dictionary<string, IList<XElement>> aliasNodes, Action<string, Guid> aliasAction )
		{
			if ( aliasNodes == null )
			{
				return;
			}

			string sql = string.Format( @"
SELECT
	a.Data, e.UpgradeId
FROM (
	VALUES {0}
) a( Data )
JOIN
	Data_Alias d ON d.TenantId = @tenant AND a.Data = d.Namespace + ':' + d.Data
JOIN
	Entity e ON e.TenantId = @tenant AND e.Id = d.EntityId", string.Join( ",", aliasNodes.Keys.Select( k => string.Format( "('{0}')", k ) ).ToArray( ) ) );

			using ( IDbCommand cmd = DatabaseContext.CreateCommand( ) )
			{
				cmd.CommandText = sql;
				cmd.AddParameter( "@tenant", DbType.Int64, TenantId );

				using ( IDataReader reader = cmd.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							string alias = reader.GetString( 0 );
							Guid guid = reader.GetGuid( 1 );

							if ( aliasAction != null )
							{
								aliasAction( alias, guid );
							}

							IList<XElement> nodes;

							if ( aliasNodes.TryGetValue( alias, out nodes ) )
							{
								if ( nodes != null )
								{
									string guidString = guid.ToString( );

									foreach ( XElement node in nodes )
									{
										node.Value = guidString;
									}
								}

								aliasNodes.Remove( alias );
							}
						}
					}
				}
			}

			if ( aliasNodes.Count > 0 )
			{
				if ( ProcessingContext != null )
				{
					foreach ( var pair in aliasNodes )
					{
						if ( pair.Value != null )
						{
							foreach ( XElement xmlText in pair.Value )
							{
								ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, xmlText, pair.Key, XmlConversionMode.LocalIdToUpgradeGuid, XmlFailureReason.UnknownLocalAlias, FailureLevel.Warning ) );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Lookups the alias automatic long.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="aliasNodes">The alias nodes.</param>
		/// <param name="aliasAction">The alias action.</param>
		private void LookupAliasToLong( Guid entityUpgradeId, Guid fieldUpgradeId, Dictionary<string, IList<XElement>> aliasNodes, Action<string, long> aliasAction )
		{
			if ( aliasNodes == null )
			{
				return;
			}

			string sql = string.Format( @"
SELECT
	a.Data, d.EntityId
FROM (
	VALUES {0}
) a( Data )
JOIN
	Data_Alias d ON d.TenantId = @tenant AND a.Data = d.Namespace + ':' + d.Data", string.Join( ",", aliasNodes.Keys.Select( k => string.Format( "('{0}')", k ) ).ToArray( ) ) );

			using ( IDbCommand cmd = DatabaseContext.CreateCommand( ) )
			{
				cmd.CommandText = sql;
				cmd.AddParameter( "@tenant", DbType.Int64, TenantId );

				using ( IDataReader reader = cmd.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							string alias = reader.GetString( 0 );
							long lng = reader.GetInt64( 1 );

							if ( aliasAction != null )
							{
								aliasAction( alias, lng );
							}

							IList<XElement> nodes;

							if ( aliasNodes.TryGetValue( alias, out nodes ) )
							{
								if ( nodes != null )
								{
									string longString = lng.ToString( CultureInfo.InvariantCulture );

									foreach ( XElement node in nodes )
									{
										node.Value = longString;
									}
								}

								aliasNodes.Remove( alias );
							}
						}
					}
				}
			}

			if ( aliasNodes.Count > 0 )
			{
				if ( ProcessingContext != null )
				{
					foreach ( var pair in aliasNodes )
					{
						if ( pair.Value != null )
						{
							foreach ( XElement xmlText in pair.Value )
							{
								ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, xmlText, pair.Key, XmlConversionMode.UpgradeGuidToLocalId, XmlFailureReason.UnknownLocalAlias, FailureLevel.Warning ) );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Lookups the unique identifier automatic long.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="guidNodes">The unique identifier nodes.</param>
		/// <param name="guidAction">The unique identifier action.</param>
		private void LookupGuidToLong( Guid entityUpgradeId, Guid fieldUpgradeId, Dictionary<Guid, IList<XElement>> guidNodes, Action<Guid, long> guidAction )
		{
			if ( guidNodes == null )
			{
				return;
			}

			string sql = string.Format( @"
SELECT
	e.UpgradeId, e.Id
FROM (
	VALUES {0}
) a( Guid )
JOIN
	Entity e ON e.TenantId = @tenant AND e.UpgradeId = a.Guid", string.Join( ",", guidNodes.Keys.Select( k => string.Format( "('{0}')", k ) ).ToArray( ) ) );

			using ( IDbCommand cmd = DatabaseContext.CreateCommand( ) )
			{
				cmd.CommandText = sql;
				cmd.AddParameter( "@tenant", DbType.Int64, TenantId );

				using ( IDataReader reader = cmd.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							Guid guid = reader.GetGuid( 0 );
							long id = reader.GetInt64( 1 );

							if ( guidAction != null )
							{
								guidAction( guid, id );
							}

							IList<XElement> nodes;

							if ( guidNodes.TryGetValue( guid, out nodes ) )
							{
								if ( nodes != null )
								{
									string idString = id.ToString( CultureInfo.InvariantCulture );

									foreach ( XElement node in nodes )
									{
										node.Value = idString;
									}
								}

								guidNodes.Remove( guid );
							}
						}
					}
				}
			}

			if ( guidNodes.Count > 0 )
			{
				if ( ProcessingContext != null )
				{
					foreach ( var pair in guidNodes )
					{
						if ( pair.Value != null )
						{
							foreach ( XElement xmlText in pair.Value )
							{
								ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, xmlText, pair.Key, XmlConversionMode.UpgradeGuidToLocalId, XmlFailureReason.UnknownUpgradeId, FailureLevel.Warning ) );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Looks up the GUID identifiers for the specified long identifiers.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="longNodes">The long nodes.</param>
		/// <param name="longAction">The long action.</param>
		private void LookupLongToGuid( Guid entityUpgradeId, Guid fieldUpgradeId, Dictionary<long, IList<XElement>> longNodes, Action<long, Guid> longAction )
		{
			if ( longNodes == null )
			{
				return;
			}

			string sql = string.Format( @"
SELECT
	e.Id, e.UpgradeId
FROM (
	VALUES {0}
) a( Id )
JOIN
	Entity e ON e.TenantId = @tenant AND e.Id = a.Id", string.Join( ",", longNodes.Keys.Select( k => string.Format( "({0})", k ) ).ToArray( ) ) );

			using ( IDbCommand cmd = DatabaseContext.CreateCommand( ) )
			{
				cmd.CommandText = sql;
				cmd.AddParameter( "@tenant", DbType.Int64, TenantId );

				using ( IDataReader reader = cmd.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							Guid guid = reader.GetGuid( 1 );

							if ( longAction != null )
							{
								longAction( id, guid );
							}

							IList<XElement> nodes;

							if ( longNodes.TryGetValue( id, out nodes ) )
							{
								if ( nodes != null )
								{
									string guidString = guid.ToString( );

									foreach ( XElement node in nodes )
									{
										node.Value = guidString;
									}
								}

								longNodes.Remove( id );
							}
						}
					}
				}
			}

			if ( longNodes.Count > 0 )
			{
				if ( ProcessingContext != null )
				{
					foreach ( var pair in longNodes )
					{
						if ( pair.Value != null )
						{
							foreach ( XElement xmlText in pair.Value )
							{
								ProcessingContext.Report.FailedEntityData.Add( new XmlFailure( entityUpgradeId, fieldUpgradeId, xmlText, pair.Key, XmlConversionMode.LocalIdToUpgradeGuid, XmlFailureReason.UnknownLocalId, FailureLevel.Warning ) );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Tries the parse unique identifier unique identifier alias.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="id">The unique identifier.</param>
		/// <param name="guid">The unique identifier.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		private bool TryParseIdGuidAlias( string value, out long? id, out Guid? guid, out string alias )
		{
			id = null;
			guid = null;
			alias = null;

			if ( string.IsNullOrEmpty( value ) )
			{
				return false;
			}

			string[] split = value.Split( new[]
				{
					'|'
				}, StringSplitOptions.RemoveEmptyEntries );

			Guid g;
			long l;

			if ( long.TryParse( split[ 0 ], out l ) )
			{
				id = l;
			}
			else if ( Guid.TryParse( split[ 0 ], out g ) )
			{
				guid = g;

				if ( split.Length == 2 )
				{
					alias = split[ 1 ];
				}
			}
			else
			{
				alias = split[ 0 ];
			}

			return true;
		}
	}
}