// Copyright 2011-2016 Global Software Innovation Pty Ltd

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Messaging.Redis
{
    public static class RedisHelper
    {
        /// <summary>
        ///     Gets the redis key.
        /// </summary>
        /// <param name="key">The key to generate</param>
        /// <param name="cacheName">The name of the cache</param>
        /// <param name="compressKey">Compress the key?</param>
        /// <returns></returns>
        public static RedisKey GetKey<TKey>(string cacheName, TKey key, bool compressKey)
        {
            var stringKey = key as string;

            if (stringKey != null)
            {
                return string.Format("{0}:{1}", cacheName, stringKey);
            }

            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, key);

                byte[] bytes = stream.ToArray();

                if (compressKey)
                {
                    bytes = EDC.IO.CompressionHelper.Compress(bytes);
                }

                return string.Format("{0}:{1}", cacheName, Convert.ToBase64String(bytes));
            }
        }

        /// <summary>
        /// Gets the redis value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="compressValue">Compress the value?</param>
        /// <returns></returns>
        public static RedisValue GetRedisValue<TValue>(TValue value, bool compressValue)
        {
            
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, value);

                byte[] bytes = stream.ToArray();

                if (compressValue)
                {
                    bytes = EDC.IO.CompressionHelper.Compress(bytes);
                }

                return bytes;
            }
        }

        /// <summary>
		///     Gets the value.
		/// </summary>
		/// <param name="value">The value.</param>
        /// <param name="compressValue">Compress the value?</param>
		/// <returns></returns>
		public static TValue GetValue<TValue>(RedisValue value, bool valueCompressed)
        {
            if (value.IsNull)
            {
                return default(TValue);
            }

            byte[] bytes = value;

            if (valueCompressed)
            {
                bytes = EDC.IO.CompressionHelper.Decompress(bytes);
            }

            TValue invalidationValue;

            using (var stream = new MemoryStream(bytes))
            {
                invalidationValue = ProtoBuf.Serializer.Deserialize<TValue>(stream);
            }

            return invalidationValue;
        }
    }
}
