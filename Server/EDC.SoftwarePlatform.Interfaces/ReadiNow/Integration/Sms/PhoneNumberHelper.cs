// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReadiNow.Integration.Sms
{
    /// <summary>
    /// A helper class for dealing with SMS messages
    /// </summary>
    public static class PhoneNumberHelper
    {
        static Regex _whiteSpace = new Regex(@"\s*");
        static Regex _backetedZero = new Regex(@"\(0\)");
        static Regex _areaCodeZero = new Regex(@"\(0");
        static Regex _digitPlusOnly = new Regex(@"[^+\d]");

        // TODO: Use a tenant specific default country code
        public const string DefaultCountryCode = "+61";

        /// <summary>
        /// Clear a telephone number and turn it into E.123 format with no spaces
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string CleanNumber(string number)
        {
            return CleanNumber(DefaultCountryCode, number);
        }

        /// <summary>
        /// Clear a telephone number and turn it into E.123 format with no spaces
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string CleanNumber(string countryCode, string number)
        {
            // remove all non-numeric characters and deal with area code zeros in brackets
            string _number = StripNumber(number);

            // check for this in case they've entered 44 (0)xxxxxxxxx or similar
            if (!_number.StartsWith("+"))
            {
                _number = _number.TrimStart('0');
                _number = countryCode + _number;
            }

            return _number;
        }




        static string StripNumber(string number)
        {
            number = _whiteSpace.Replace(number, string.Empty);           
            number = _backetedZero.Replace(number, string.Empty);           // get rid of (0)
            number = _areaCodeZero.Replace(number, string.Empty);           // get rid of the leading zero from the area code
            number = _digitPlusOnly.Replace(number, string.Empty);          // get rid of everything not a plus or a number
            return number;
        }
    }
}
