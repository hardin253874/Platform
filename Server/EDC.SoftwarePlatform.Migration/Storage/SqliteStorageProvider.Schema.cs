// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Storage
{
	/// <summary>
	///     SqLite Storage provider.
	/// </summary>
	public partial class SqliteStorageProvider
	{
		private static readonly List< string > SqLiteSetupCommands = new List< string >
			{
				@"PRAGMA synchronous=OFF",
				@"CREATE TABLE IF NOT EXISTS [_Metadata]
(
	[Property] TEXT PRIMARY KEY NOT NULL,
    [Value] TEXT
)",
				@"CREATE TABLE IF NOT EXISTS [_Dependencies]
(
	[Id] TEXT PRIMARY KEY NOT NULL,
    [MinimumVersion] TEXT,
	[MaximumVersion] TEXT
)",
				@"CREATE TABLE IF NOT EXISTS [_Entity]
(
	[Id] INTEGER PRIMARY KEY NOT NULL,
    [Uid] TEXT
)",
				@"CREATE TABLE IF NOT EXISTS [_Relationship]
(
    [Id] INTEGER PRIMARY KEY NOT NULL,
	[FromUid] TEXT,
    [ToUid] TEXT,
	[TypeUid] TEXT,
    [EntityUid] TEXT
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_Alias]
(
	[EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] TEXT,
    [Namespace] TEXT,
    [AliasMarkerId] INTEGER
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_Bit]
(
	[EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] INTEGER
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_DateTime]
(
	[EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] NVARCHAR(30)
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_Decimal]
(
	[EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] NUMERIC
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_Guid]
(
    [EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] TEXT
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_Int]
(
    [EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] INTEGER
)",
				@"CREATE TABLE IF NOT EXISTS [_Data_NVarChar]
(
    [EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] TEXT
)",
                @"CREATE TABLE IF NOT EXISTS [_Data_XML]
(
    [EntityUid] TEXT,
    [FieldUid] TEXT,
    [Data] TEXT
)",
                @"CREATE TABLE IF NOT EXISTS [_SecureData]
(
    [SecureId] TEXT,
    [Context] TEXT,
    [Data] TEXT
)",
                @"CREATE TABLE IF NOT EXISTS [_Filestream_Binary]
(
    [FileExtension] TEXT,
    [DataHash] TEXT,
    [Data] BLOB    
)",
                @"CREATE TABLE IF NOT EXISTS [_Filestream_Document]
(
    [FileExtension] TEXT,
    [DataHash] TEXT,
    [Data] BLOB
)",
				@"CREATE INDEX IF NOT EXISTS [_IDX_Data_NVarChar] ON [_Data_NVarChar] ( [EntityUid], [FieldUid] )"
			};
	}
}