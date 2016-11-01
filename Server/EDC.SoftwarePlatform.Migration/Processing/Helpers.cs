// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Helper class.
	/// </summary>
	/// <remarks>
	///     Member initialization order is important in this class.
	/// </remarks>
	public static class Helpers
	{
		/// <summary>
		///     The alias name
		/// </summary>
		public const string AliasName = "Alias";

		/// <summary>
		///     The bit name
		/// </summary>
		public const string BitName = "Bit";

		/// <summary>
		///     The date time name
		/// </summary>
		public const string DateTimeName = "DateTime";

		/// <summary>
		///     The decimal name
		/// </summary>
		public const string DecimalName = "Decimal";

		/// <summary>
		///     The unique identifier name
		/// </summary>
		public const string GuidName = "Guid";

		/// <summary>
		///     The int name
		/// </summary>
		public const string IntName = "Int";

		/// <summary>
		///     The NVarChar name
		/// </summary>
		public const string NVarCharName = "NVarChar";

		/// <summary>
		///     The XML name
		/// </summary>
		public const string XmlName = "Xml";

		/// <summary>
		///     The file GUID field upgrade id
		/// </summary>
		public static Guid FileDataHashFieldUpgradeId = new Guid( "256D76BC-B5E4-4AD7-A765-5DDCABE1121F" );

		/// <summary>
		///     The file extension field upgrade id
		/// </summary>
		public static Guid FileExtensionFieldUpgradeId = new Guid( "2EA3CC77-2BDC-479E-B55B-81F5A7B565BE" );

		/// <summary>
		///     IsOfType relationship upgrade id
		/// </summary>
		public static Guid IsOfTypeRelationshipUpgradeId = new Guid( "E1AFC9E2-A526-4DC6-B90F-E2271E130F24" );

		/// <summary>
		///     Inherits relationship upgrade id
		/// </summary>
		public static Guid InheritsRelationshipUpgradeId = new Guid( "50FE1B39-BB32-4825-8AB5-807A7DF9B5B8" );

		/// <summary>
		///     Image file type upgrade id
		/// </summary>
		public static Guid ImageFileTypeUpgradeId = new Guid( "4E061E43-38DD-412E-AD8C-580089A5F8FA" );

		/// <summary>
		///     The file type upgrade id
		/// </summary>
		public static Guid FileTypeUpgradeId = new Guid( "5A0C9FDB-4BB2-4E88-82DE-58840B84E12B" );

		/// <summary>
		///     List of data-type tables suffixes.
		/// </summary>
		public static readonly string[ ] FieldDataTables =
		{
			AliasName,
			BitName,
			DateTimeName,
			DecimalName,
			GuidName,
			IntName,
			NVarCharName,
			XmlName
		};

		/// <summary>
		///     Map of data-type table suffixes to type.
		/// </summary>
		public static readonly Dictionary<string, Type> FieldDataTableTypes = new Dictionary<string, Type>
		{
			{
				AliasName, typeof( string )
			},
			{
				BitName, typeof( bool )
			},
			{
				DateTimeName, typeof( DateTime )
			},
			{
				DecimalName, typeof( decimal )
			},
			{
				GuidName, typeof( Guid )
			},
			{
				IntName, typeof( long )
			},
			{
				NVarCharName, typeof( string )
			},
			{
				XmlName, typeof( string )
			}
		};
	}
}