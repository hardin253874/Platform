// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Serializer;
	/// </summary>
	public static class Serializer
	{
		/// <summary>
		///     Deserializes the object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stringToDeserialize">The string to deserialize.</param>
		/// <returns></returns>
		public static T DeserializeObject<T>( string stringToDeserialize )
		{
			var bytes = Encoding.UTF8.GetBytes( stringToDeserialize );

			using ( var memStm = new MemoryStream( bytes ) )
			{
				var serializer = new NetDataContractSerializer( );
				var obj = ( T ) serializer.ReadObject( memStm );

				return obj;
			}
		}

		/// <summary>
		///     Serializes the object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objectToSerialize">The object to serialize.</param>
		/// <returns></returns>
		public static string SerializeObject<T>( T objectToSerialize )
		{
			using ( var memStm = new MemoryStream( ) )
			{
				var serializer = new NetDataContractSerializer( );
				serializer.WriteObject( memStm, objectToSerialize );

				memStm.Seek( 0, SeekOrigin.Begin );

				using ( var streamReader = new StreamReader( memStm ) )
				{
					string result = streamReader.ReadToEnd( );
					return result;
				}
			}
		}
	}
}