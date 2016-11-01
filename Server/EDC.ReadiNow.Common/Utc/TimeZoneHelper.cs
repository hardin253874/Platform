// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using EDC.Database;
using EDC.ReadiNow.Database;
using Microsoft.SqlServer.Server;

namespace EDC.ReadiNow.Utc
{
	// Important: this file is shared to EDC.SoftwarePlatform.SQL
	// Code will be run within the SQL Server process.

	/// <summary>
	///     Helpers for converting between UTC and client-local time.
	/// </summary>
	public static class TimeZoneHelper
	{
	    /// <summary>
	    /// For tests, etc.
	    /// </summary>
        public static readonly string SydneyTimeZoneName = "AUS Eastern Standard Time";//"(UTC+10:00) Canberra, Melbourne, Sydney";


		/// <summary>
		///     Cache of time zones.
		/// </summary>
        private static volatile Dictionary<string, TimeZoneInfo> _timeZonesDisplayNamesDict;

        /// <summary>
        ///     Cache of time zones.
        /// </summary>
        private static volatile Dictionary<string, TimeZoneInfo> _timeZonesStandardNamesDict;

        /// <summary>
        /// Cache of Olson and Microsoft time zone names mapping.
        /// </summary>
        private static volatile Dictionary<string, string> _olsonMsTimezonesMap; 


        /// <summary>
        ///     Converts a UTC time to the specified local time.
        /// </summary>
        /// <param name="utcTime">The UTC time.</param>
        /// <param name="timeZone">Info about the timezone.</param>
        /// <returns></returns>
        public static DateTime ConvertToLocalTimeTZ(DateTime utcTime, TimeZoneInfo timeZone)
        {
            if (timeZone == null)
            {
                // could not convert
                return utcTime;
            }

            DateTime utcTime2 = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime2, timeZone);
            return localTime;
        }


		/// <summary>
		///     Converts a UTC time to the specified local time.
		/// </summary>
		/// <param name="utcTime">The UTC time.</param>
		/// <param name="timeZoneName">Name of the time zone returned from DisplayName.</param>
		/// <returns></returns>
		public static DateTime ConvertToLocalTime( DateTime utcTime, string timeZoneName )
		{
			TimeZoneInfo timeZone = GetTimeZoneInfo( timeZoneName );
            DateTime localTime = ConvertToLocalTimeTZ(utcTime, timeZone);
            return localTime;
		}


        /// <summary>
        ///     Converts a local time to UTC.
        /// </summary>
        /// <param name="localTime">The local time.</param>
        /// <param name="timeZone">Name of the time zone returned from DisplayName.</param>
        /// <returns></returns>
        public static DateTime ConvertToUtcTZ(DateTime localTime, TimeZoneInfo timeZone)
        {
            if (timeZone == null)
            {
                // could not convert
                return localTime;
            }

            DateTime unspecifiedTime = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
            // Must use unspecified for ConvertTimeToUtc to work
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(unspecifiedTime, timeZone);
            return utcTime;
        }


		/// <summary>
		///     Converts a local time to UTC.
		/// </summary>
		/// <param name="localTime">The local time.</param>
		/// <param name="timeZoneName">Name of the time zone returned from DisplayName.</param>
		/// <returns></returns>
		public static DateTime ConvertToUtc( DateTime localTime, string timeZoneName )
		{
			TimeZoneInfo timeZone = GetTimeZoneInfo( timeZoneName );
            DateTime utcTime = ConvertToUtcTZ(localTime, timeZone);
            return utcTime;
		}


		/// <summary>
		///     Gets the current time in the specified time zone.
		/// </summary>
		public static DateTime GetLocalTime( string timeZoneName )
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime localNow = ConvertToLocalTime( utcNow, timeZoneName );

			// Need to cast kind, otherwise some operations (such as .Date) can't be trusted.
			DateTime localNow2 = DateTime.SpecifyKind( localNow, DateTimeKind.Local );

			return localNow2;
		}


        /// <summary>
        /// Gets the name of the ms time zone.
        /// </summary>
        /// <param name="timeZoneName">Name of the time zone.</param>
        /// <returns></returns>
        public static string GetMsTimeZoneName(string timeZoneName)
        {
            var tz = GetTimeZoneInfo(timeZoneName);

            if(tz != null)
            {
                return tz.StandardName;
            }

            return null;
        }

        /// <summary>
        /// Fill the time-zone caches
        /// </summary>
        public static void Prewarm()
        {
            try
            {
                GetTimeZoneInfo("");
            }
            catch { }
        }

	    /// <summary>
		///     Gets a TimeZoneInfo by name.
		/// </summary>
		public static TimeZoneInfo GetTimeZoneInfo( string timeZoneName )
		{
			if ( timeZoneName == null )
			{
				return null;
			}

            if (_timeZonesDisplayNamesDict == null)
			{
				lock ( typeof ( TimeZoneHelper ) )
				{
                    if (_timeZonesDisplayNamesDict == null)
					{
						CultureInfo culture = CultureInfo.CurrentCulture;
						try
						{
							Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                            _timeZonesDisplayNamesDict = TimeZoneInfo.GetSystemTimeZones().ToDictionary(tz => tz.DisplayName);
						}
						finally
						{
							Thread.CurrentThread.CurrentCulture = culture;
						}
					}
				}
			}

            if (_timeZonesStandardNamesDict == null)
            {
                lock (typeof(TimeZoneHelper))
                {
                    if (_timeZonesStandardNamesDict == null)
                    {
                        CultureInfo culture = CultureInfo.CurrentCulture;
                        try
                        {
                            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                            _timeZonesStandardNamesDict = TimeZoneInfo.GetSystemTimeZones().ToDictionary(tz => tz.StandardName);
                        }
                        finally
                        {
                            Thread.CurrentThread.CurrentCulture = culture;
                        }
                    }
                }
            }

			TimeZoneInfo timeZone;
            if (!_timeZonesDisplayNamesDict.TryGetValue(timeZoneName, out timeZone))    // check in displayNamesDict
			{
                if (!_timeZonesStandardNamesDict.TryGetValue(timeZoneName, out timeZone))    // check in standardNamesDict
                {
                    // if an Olson timezone name is passed in then get the corresponding Windows timezone name
                    string msTimeZoneName = GetMsTimeZoneNameByOlsonName(timeZoneName);

                    if (!string.IsNullOrEmpty(msTimeZoneName))
                    {
                        try
                        {
                            return TimeZoneInfo.FindSystemTimeZoneById(msTimeZoneName);
                        }
                        catch (TimeZoneNotFoundException ex)
                        {
                            ReadiNow.Diagnostics.EventLog.Application.WriteError("Unable to find the windows timezone.\r\n{0}", ex);
                        }
                    }
                }
			}

			return timeZone;
		}

        /// <summary>
        /// Gets the name of the microsoft time zone.
        /// </summary>
        /// <param name="olsonTimeZoneName">Name of the olson time zone.</param>
        /// <returns></returns>
        private static string GetMsTimeZoneNameByOlsonName(string olsonTimeZoneName)
        {
            if (olsonTimeZoneName == null)
                return null;

            if (_olsonMsTimezonesMap == null)
            {
                lock (typeof(TimeZoneHelper))
                {
                    if (_olsonMsTimezonesMap == null)
                    {
                        _olsonMsTimezonesMap = new Dictionary<string, string>();
                    }
                }
            }

            string timeZonename;
            if (!(_olsonMsTimezonesMap.TryGetValue(olsonTimeZoneName, out timeZonename)))
            {
                // get from db and add to the dictionary
                using (DatabaseContext databaseContext = DatabaseContext.GetContext())
                {
                    // Create and initialize the command object
					using ( var command = databaseContext.CreateCommand( "spGetMsTimeZoneName", CommandType.StoredProcedure ) )
                    {
                        databaseContext.AddParameter(command, "@olsonTimeZoneName", DbType.String, olsonTimeZoneName);
                        timeZonename = (string)command.ExecuteScalar();
                    }
                }

                if (!string.IsNullOrEmpty(timeZonename))
                {
                    lock (typeof(TimeZoneHelper))
                    {
                        string tempTimeZoneName;
                        if (!(_olsonMsTimezonesMap.TryGetValue(olsonTimeZoneName, out tempTimeZoneName)))
                        {
                            _olsonMsTimezonesMap[olsonTimeZoneName] = timeZonename;
                        }
                    }
                }
            }

            return timeZonename;
        }
	}
}