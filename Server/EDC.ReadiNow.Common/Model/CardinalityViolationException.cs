// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Security;
using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     Exception that is raised when a relationship instance constraint fails.
    /// </summary>
    [Serializable]
    public class CardinalityViolationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CardinalityViolationException" /> class.
		/// </summary>
		public CardinalityViolationException( )
            : base("Cardinality violation detected.")
		{
		}

		/// <summary>
        ///     Initializes a new instance of the <see cref="CardinalityViolationException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public CardinalityViolationException( string message )
			: base( message )
		{
		}

		/// <summary>
        ///     Initializes a new instance of the <see cref="CardinalityViolationException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public CardinalityViolationException( string message, Exception inner )
			: base( message, inner )
		{
		}


        /// <summary>
        ///     Initializes a new instance of the <see cref="CardinalityViolationException" /> class.
        /// </summary>       
        /// <param name="inner">The inner.</param>
        public CardinalityViolationException(Exception inner)
            : base(ConvertMessage(inner), inner)
        {

        }

        /// <summary>
        /// convert the original exception message from "Cardinality violation detected. TypeId: xxxxx, FromId: xxxxx,ToId xxxxx." to more meaningful format
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static string ConvertMessage(Exception ex)
        {
            string fromName = string.Empty;
            string toName = string.Empty;

			bool genericFromName = false;
			bool genericToName = false;

            //get fromName by fromId from error message
            Match match = Regex.Match(ex.Message, @"FromId:(.*?),");
            if (match.Success)
            {
                long fromId = 0;
                if (long.TryParse(match.Groups[1].Value.Trim(), out fromId))
                {
                    fromName = GetName( fromId );

                    if ( string.IsNullOrEmpty( fromName ) )
                    {
                        genericFromName = true;
                    }
                }
            }
            //get toName by toId from error message
            match = Regex.Match(ex.Message, @"ToId:(.*?)\.");
            if (match.Success)
            {
                long toId = 0;
                if (long.TryParse(match.Groups[1].Value.Trim(), out toId))
                {
                    toName = GetName( toId );

                    if ( string.IsNullOrEmpty( toName ) )
					{
						genericToName = true;
					}
                }
            }

			return string.Format( "{0} cannot be associated with {1} because {2} is already associated with another record", genericFromName ? "The source item" : fromName, genericToName ? "the destination item" : toName, genericFromName ? "it" : fromName );
        }

        private static string GetName( long id )
        {
            string result;
            try
            {
                result = Entity.GetName( id );
            }
            catch ( PlatformSecurityException )
            {
                result = "Restricted";
            }
            catch
            {
                result = "?";
            }

            return result;
        }
    }
}
