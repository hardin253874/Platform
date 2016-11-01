// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net.Http;
using Jil;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Helpers for working with JSON.
	/// </summary>
	public static class JsonResponse
	{
		/// <summary>
		///     The Jil JSON options
		/// </summary>
		private static readonly Options JsonOptions = JilFormatter.GetDefaultOptions( );

		/// <summary>
		///     Convert object to JSON.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="value">Object value.</param>
		/// <returns>JSON</returns>
		public static string CreateJson<T>( T value )
		{
			return JSON.SerializeDynamic( value, JsonOptions );
		}

		/// <summary>
		///     Convert JSON to a HTTP response.
		/// </summary>
		/// <param name="json">JSON</param>
		/// <returns></returns>
		public static HttpResponseMessage CreateResponse( string json )
		{
			if ( json == null )
			{
				throw new ArgumentNullException( "json" );
			}

			var sc = new StringContent( json );
			sc.Headers.ContentType = JilFormatter.ApplicationJsonMediaType;

			var response = new HttpResponseMessage
			{
				Content = sc
			};

			return response;
		}
	}
}