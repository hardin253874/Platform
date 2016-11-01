// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Net.Http.Formatting;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Configure the formatters.
	/// </summary>
	public class FormatterConfig
	{
		/// <summary>
		///     Configures the formatters.
		/// </summary>
		/// <param name="formatters">The formatters.</param>
		public static void ConfigureFormatters( MediaTypeFormatterCollection formatters )
		{
			MediaTypeFormatter formatter = new JilFormatter( );

			formatters.RemoveAt( 0 );
			formatters.Insert( 0, formatter );

			// Ensure same formatter is used by HttpResponseMessage<T>
			HttpResponseMessageFormatter.Formatter = formatter;

			// See also JsonResponse.cs
		}
	}
}