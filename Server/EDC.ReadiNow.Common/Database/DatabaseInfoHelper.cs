// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Data;
using System.Globalization;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Helper class for loading information about the attached database.
	/// </summary>
	public static class DatabaseInfoHelper
	{
        /// <summary>
        /// The number of characters captured in the Data_NVarChar_StartsWith calculated column.
        /// </summary>
        public const int Data_NVarChar_StartsWith_Size = 100;

		private static volatile string _bookmarkScheme;

		/// <summary>
		///     Gets a string that is attached to each bookmark to determine if it is still recognizable.
		/// </summary>
		public static string GetBookmarkScheme( )
		{
			if ( _bookmarkScheme == null )
			{
				lock ( typeof ( DatabaseInfoHelper ) )
				{
					if ( _bookmarkScheme == null )
					{
						using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
						{
							const string sql = "select top 1 BookmarkScheme from PlatformInfo";

							using ( IDbCommand command = databaseContext.CreateCommand( sql ) )
							{
								object result = command.ExecuteScalar( );
								if ( result != null )
								{
									_bookmarkScheme = ( ( int ) result ).ToString( CultureInfo.InvariantCulture );
								}
							}
						}
					}
				}
			}

			return _bookmarkScheme;
		}
	}
}