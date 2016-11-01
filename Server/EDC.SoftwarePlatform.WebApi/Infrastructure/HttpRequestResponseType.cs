// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
    internal class HttpResponseMessageFormatter
    {
        /// <remarks>
        /// Defined here so that it can be overwritten in FormatterConfig, but without breaking the other webs that are also linking to this file.
        /// (And needs its own class because of the generics).
        /// </remarks>
        public static MediaTypeFormatter Formatter = new JsonMediaTypeFormatter( );
    }


	// Responses
	public class HttpResponseMessage<T> : HttpResponseMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="T:System.Net.Http.HttpResponseMessage" /> class with a specific
		///     <see cref="P:System.Net.Http.HttpResponseMessage.StatusCode" />.
		/// </summary>
		/// <param name="statusCode">The status code for the HTTP response.</param>
		public HttpResponseMessage( HttpStatusCode statusCode )
			: base( statusCode )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="HttpResponseMessage{T}" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <remarks>
		///     By default this uses the JSON media formatter to serialize the message body and returns a 200 status code.
		/// </remarks>
		public HttpResponseMessage( T value )
			: base( HttpStatusCode.OK )
		{
            Content = new ObjectContent<T>( value, HttpResponseMessageFormatter.Formatter );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="HttpResponseMessage{T}" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="statusCode">The status code for the HTTP response.</param>
		/// <remarks>
		///     By default this uses the JSON media formatter to serialize the message body
		/// </remarks>
		public HttpResponseMessage( T value, HttpStatusCode statusCode )
			: base( statusCode )
		{
            Content = new ObjectContent<T>( value, HttpResponseMessageFormatter.Formatter );
		}
	}
}