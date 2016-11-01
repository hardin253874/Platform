// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Parses individual field values from a CSV document.
    /// Note: this is intentionally a separate implementation from JsonValueParser because we want to be more strict about types accepted by the latter.
    /// </summary>
    static class CsvValueParser
    {
        const NumberStyles DecimalStyle =
            NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol;

        const DateTimeStyles DateOrTimeStyle =
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal |
            DateTimeStyles.AllowWhiteSpaces;

        static readonly CultureInfo DateTimeCulture = CultureInfo.GetCultureInfo("en-AU");

        public static bool TryParseBool(string value, out bool result)
        {
            string lValue = value.ToLower();
            if (lValue == "y" || lValue == "yes" || lValue == "true" || lValue == "t" || lValue == "1")
            {
                result = true;
                return true;
            }
            if (lValue == "n" || lValue == "no" || lValue == "false" || lValue == "f" || lValue == "0")
            {
                result = false;
                return true;
            }
            result = false;
            return false;
        }

        public static bool TryParseInt(string value, out int result)
        {
            decimal dResult;
            if ( !TryParseDecimal( value, out dResult ) )
            {
                result = 0;
                return false;
            }
            decimal dRounded = Math.Round( dResult, MidpointRounding.AwayFromZero );
            result = ( int ) dRounded;
            return true;
        }

        public static bool TryParseDecimal(string value, out decimal result)
        {
            if (decimal.TryParse(value, DecimalStyle, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }
            if ( value.Contains( "$" ) )
            {
                string value2 = value.Replace( "$", "" );
                if ( decimal.TryParse( value2, DecimalStyle, CultureInfo.InvariantCulture, out result ) )
                {
                    return true;
                }
            }
            return false;
        }

        public static bool TryParseDate(string value, out DateTime result)
        {
            DateTime parsed;
            if (!DateTime.TryParse(value, DateTimeCulture, DateOrTimeStyle, out parsed))
            {
                result = parsed; // or whatever
                return false;
            }

            result = parsed.Date;
            return true;
        }

        public static bool TryParseTime(string value, out DateTime result)
        {
            
            DateTime parsed;
            if (!DateTime.TryParse(value, DateTimeCulture, DateOrTimeStyle, out parsed))
            {
                result = parsed; // or whatever
                return false;
            }

            result = TimeType.NewTime(parsed.TimeOfDay);
            return true;
        }

        public static bool TryParseDateTime(string value, TimeZoneInfo timeZone, out DateTime result)
        {
            // For parsing date times, we want the following behavior:
            // - if a timezone or 'z' is specified in the value, then use that timezone explicitly to adjust the value to Utc
            // - if no timezone is specified, then fall back to the timezone passed in as context.
            // - if no timezone is passed in the context either, then assume UTC

            const DateTimeStyles dateTimeStyle =
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces;

            DateTime parsed;

            if ( !DateTime.TryParse( value, DateTimeCulture, dateTimeStyle, out parsed ) )
            {
                result = DateTime.MinValue;
                return false;
            }

            switch ( parsed.Kind )
            {
                case DateTimeKind.Unspecified:
                    if ( timeZone != null )
                        result = TimeZoneInfo.ConvertTimeToUtc( parsed, timeZone );
                    else
                        result = DateTime.SpecifyKind( parsed, DateTimeKind.Utc );
                    break;
                case DateTimeKind.Utc:
                    result = parsed;
                    break;
                default:
                    throw new InvalidOperationException( "Assert false - Local" );
            }

            return true;
        }
    }
}
