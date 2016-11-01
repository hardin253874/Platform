// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis based distributed queue
	/// </summary>
	public class RedisQueue<T> : IQueue<T>
	{
        const string RedisQueueName = "queue";
		/// <summary>
		///     The underlying database.
		/// </summary>
		private IDatabase Database { get; }

        /// <summary>
        /// The Id of the queue
        /// </summary>
        private RedisKey QueueId { get; }

        /// <summary>
        /// Compress the value as it is stored
        /// </summary>
        private bool CompressValue { get; }

        /// <summary>
        /// The Name of the queue
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryStore" /> class.
        /// </summary>
        /// <param name="name">The name of the queue</param>
        /// <param name="database">The database.</param>
        /// <param name="compressValue">Compress the values in the queue?</param>
        /// <exception cref="System.ArgumentNullException">database</exception>
        public RedisQueue( string name, IDatabase database, bool compressValue = false)
		{
			if ( database == null )
			{
				throw new ArgumentNullException( nameof(database) );
			}

            Name = name;
			Database = database;
            CompressValue = compressValue;

            QueueId = RedisHelper.GetKey(RedisQueueName, Name ?? Guid.NewGuid().ToString(), false);
        }


        public void Enqueue(T value, CommandFlags flags = CommandFlags.None)
        {
            var encodedValue = RedisHelper.GetRedisValue(value, CompressValue);
            Database.ListLeftPush(QueueId, encodedValue);
        }

        public void Enqueue(T[] values, CommandFlags flags = CommandFlags.None)
        {
            var encodedValues = values.Select(value => RedisHelper.GetRedisValue(value, CompressValue));

            Database.ListLeftPush(QueueId, encodedValues.ToArray());
        }


        public T Dequeue(CommandFlags flags = CommandFlags.None)
        {
            var encodedValue =  Database.ListRightPop(QueueId, flags);
            return RedisHelper.GetValue<T>(encodedValue, CompressValue);
        }

        public long Length
        {
            get
            {
                return Database.ListLength(QueueId);
            }
        }
    }
}