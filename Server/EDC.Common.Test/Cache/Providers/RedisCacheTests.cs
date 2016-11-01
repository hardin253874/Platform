// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ProtoBuf;

namespace EDC.Test.Cache.Providers
{
	[TestFixture]
	public class RedisCacheTests
	{
		/// <summary>
		///     The cache name
		/// </summary>
		private const string CacheName = "Test";

		/// <summary>
		///     Whether Redis compression is on or off.
		/// </summary>
		private const bool Compress = true;

		/// <summary>
		///     Gets the factory.
		/// </summary>
		/// <value>
		///     The factory.
		/// </value>
		private ICacheFactory Factory
		{
			get
			{
				return new CacheFactory
				{
					CacheName = CacheName,
					Redis = true,
					RedisValueCompression = Compress,
					Distributed = false,
					Logging = false,
					ThreadSafe = false
				};
			}
		}

		/// <summary>
		///     Test a typed redis cache.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarm">if set to <c>true</c>, pre-warms the redis cache.</param>
		private void TestRedisCache<TKey, TValue>( TKey key, TValue value, bool prewarm )
		{
			ICache<TKey, TValue> cache = Factory.Create<TKey, TValue>( CacheName );
			ICache<TKey, TValue> prewarmCache = GetPrewarmCache<TKey, TValue>( prewarm );

			RunTest( Add, cache, key, value, prewarmCache );
			RunTest( Remove, cache, key, value, prewarmCache );
			RunTest( Clear, cache, key, value, prewarmCache );
			RunTest( TryGetOrAdd, cache, key, value, prewarmCache );
			RunTest( TryGetValue, cache, key, value, prewarmCache );
		}

		/// <summary>
		///     Gets the pre-warm cache.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="prewarm">if set to <c>true</c>, pre-warms the redis cache.</param>
		/// <returns></returns>
		private ICache<TKey, TValue> GetPrewarmCache<TKey, TValue>( bool prewarm )
		{
			if ( ! prewarm )
			{
				return null;
			}

            // Note: using the same factory, because otherwise other factors, such as per-tenant isolation
            // can mutate the cache name so that it no longer matches.
            // We still receive a distinct cache object.
			return Factory.Create<TKey, TValue>(CacheName);
        }

		/// <summary>
		///     Runs the test.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="action">The action.</param>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarmCache">The pre-warm cache.</param>
		private void RunTest<TKey, TValue>( Action<ICache<TKey, TValue>, TKey, TValue, bool> action, ICache<TKey, TValue> cache, TKey key, TValue value, ICache<TKey, TValue> prewarmCache )
		{
			if ( prewarmCache != null )
			{
				prewarmCache.Add( key, value );
			}

			action( cache, key, value, prewarmCache != null );

			/////
			// Ensure the cache is cleaned up.
			/////
			cache.Remove( key );
		}

		/// <summary>
		///     Test the Add function.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		private void Add<TKey, TValue>( ICache<TKey, TValue> cache, TKey key, TValue value, bool prewarm )
		{
			cache.Add( key, value );

			TValue outValue;
			bool found = cache.TryGetValue( key, out outValue );

			Assert.AreEqual( true, found );
			Assert.AreEqual( value, outValue );

			cache.Add( key, value );

			found = cache.TryGetValue( key, out outValue );

			Assert.AreEqual( true, found );
			Assert.AreEqual( value, outValue );
		}

		/// <summary>
		///     Test the Add function.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		private void Clear<TKey, TValue>( ICache<TKey, TValue> cache, TKey key, TValue value, bool prewarm )
		{
			cache.Clear( );

			TValue outValue;
			bool found = cache.TryGetValue( key, out outValue );

			Assert.AreEqual( false, found );

			cache.Add( key, value );

			found = cache.TryGetValue( key, out outValue );

			Assert.AreEqual( true, found );
			Assert.AreEqual( value, outValue );

			cache.Clear( );

			found = cache.TryGetValue( key, out outValue );

			Assert.AreEqual( false, found );
		}

		/// <summary>
		///     Test the Remove function.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		private void Remove<TKey, TValue>( ICache<TKey, TValue> cache, TKey key, TValue value, bool prewarm )
		{
			bool found = cache.Remove( key );

			Assert.AreEqual( false, found );

			found = cache.Remove( key );

			/////
			// Redis always returns true when removing
			/////
			Assert.AreEqual( false, found );
		}

		/// <summary>
		///     Test the TryGetOrAdd function.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		private void TryGetOrAdd<TKey, TValue>( ICache<TKey, TValue> cache, TKey key, TValue value, bool prewarm )
		{
			TValue outValue;
			bool found = cache.TryGetOrAdd( key, out outValue, k => value );

			Assert.IsFalse( found );
			Assert.AreEqual( value, outValue );

			found = cache.TryGetOrAdd( key, out outValue, k => value );

			Assert.IsTrue( found );
			Assert.AreEqual( value, outValue );
		}

		/// <summary>
		///     Test the TryGetValue function.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="cache">The cache.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		private void TryGetValue<TKey, TValue>( ICache<TKey, TValue> cache, TKey key, TValue value, bool prewarm )
		{
			TValue outValue;
			bool found = cache.TryGetValue( key, out outValue );

			Assert.AreEqual( prewarm, found );
			Assert.AreEqual( prewarm ? value : default( TValue ), outValue );

			cache.Add( key, value );

			found = cache.TryGetValue( key, out outValue );

			Assert.IsTrue( found );
			Assert.AreEqual( value, outValue );
		}

		/// <summary>
		///     Tests the redis caches. long to long.
		/// </summary>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		[TestCase( true )]
		[TestCase( false )]
		[RunAsDefaultTenant]
		public void TestRedisCache_Long_Long( bool prewarm )
		{
			TestRedisCache<long, long>( 1234, 5678, prewarm );
		}

		/// <summary>
		///     Tests the redis caches. string to string.
		/// </summary>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		[TestCase( true )]
		[TestCase( false )]
		[RunAsDefaultTenant]
		public void TestRedisCache_String_String( bool prewarm )
		{
			TestRedisCache( "Test Key", "Test Value", prewarm );
		}

		/// <summary>
		///     Tests the redis caches. tuple&lt;long, string&gt; to long.
		/// </summary>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		[TestCase( true )]
		[TestCase( false )]
		[RunAsDefaultTenant]
		public void TestRedisCache_TupleLongString_Long( bool prewarm )
		{
			TestRedisCache<Tuple<long, string>, long>( new Tuple<long, string>( 1234, "1234" ), 5678, prewarm );
		}

		/// <summary>
		///     Tests the redis caches. tuple&lt;long, string&gt; to Person.
		/// </summary>
		/// <param name="prewarm">Whether redis is pre-warmed.</param>
		[TestCase( true )]
		[TestCase( false )]
		[RunAsDefaultTenant]
		public void TestRedisCache_TupleLongString_Person( bool prewarm )
		{
			var mother = new Person( "Sue", 70 );
			var father = new Person( "Bob", 75 );
			var alice = new Person( "Alice", 35, mother, father );

			TestRedisCache( new Tuple<long, string>( 1, "Alice" ), alice, prewarm );
		}
	}

	[ProtoContract]
	public class Person
	{
		private Person( )
		{
			Children = new List<Person>( );
		}

		public Person( string name, int age, Person mother = null, Person father = null )
			: this( )
		{
			Name = name;
			Age = age;
			Mother = mother;
			Father = father;
		}

		[ProtoMember( 2 )]
		public readonly int Age;

		[ProtoMember( 3 )]
		public readonly List<Person> Children;

		[ProtoMember( 5 )]
		public readonly Person Father;

		[ProtoMember( 4 )]
		public readonly Person Mother;

		[ProtoMember( 1 )]
		public readonly string Name;

		public override bool Equals( object obj )
		{
			if ( ReferenceEquals( null, obj ) ) return false;
			if ( ReferenceEquals( this, obj ) ) return true;
			if ( obj.GetType( ) != GetType( ) ) return false;
			return Equals( ( Person ) obj );
		}

		protected bool Equals( Person other )
		{
			return string.Equals( Name, other.Name ) && Age == other.Age && Children.SequenceEqual( other.Children ) && Equals( Mother, other.Mother ) && Equals( Father, other.Father );
		}

		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				if ( Name != null )
				{
					hash = hash * 92821 + Name.GetHashCode( );
				}

				hash = hash * 92821 + Age.GetHashCode( );

				if ( Children != null )
				{
					hash = hash * 92821 + Children.GetHashCode( );
				}

				if ( Mother != null )
				{
					hash = hash * 92821 + Mother.GetHashCode( );
				}

				if ( Father != null )
				{
					hash = hash * 92821 + Father.GetHashCode( );
				}

				return hash;
			}
		}
	}
}