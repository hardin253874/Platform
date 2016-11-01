// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	public class DocumentDataEntry : IEntry<string>
	{
		/// <summary>
		///     The data hash.
		/// </summary>
		public string DataHash;

		/// <summary>
		///     The file extension.
		/// </summary>
		public string FileExtension;

		/// <summary>
		///     The load data call back
		/// </summary>
		public Func<string, byte[ ]> LoadDataCallback;

		/// <summary>
		///     The data
		/// </summary>
		private byte[ ] _data;

		/// <summary>
		///     True if the data has been loaded, false otherwise.
		/// </summary>
		private bool _loadedData;

		/// <summary>
		///     The data.
		/// </summary>
		public byte[ ] Data
		{
			get
			{
				if ( !_loadedData &&
				     _data == null &&
				     LoadDataCallback != null &&
				     !string.IsNullOrEmpty( DataHash ) )
				{
					_loadedData = true;
					_data = LoadDataCallback( DataHash );
				}

				return _data;
			}
			set
			{
				_data = value;
			}
		}

		/// <summary>
		///     Gets the primary key.
		/// </summary>
		public string GetKey( )
		{
			return DataHash;
		}

		/// <summary>
		///     Compares the non-key component of two entries.
		/// </summary>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		public bool IsSameData( object alt )
		{
			return false;
		}
	}
}