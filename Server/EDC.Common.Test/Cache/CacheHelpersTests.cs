// Copyright 2011-2016 Global Software Innovation Pty Ltd

using NUnit.Framework;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.ReadiNow.Test;

namespace EDC.Test.Cache
{
	[ReadiNowTestFixture]
    public class CacheHelpersTests
    {
        [Test]
        public void Test_GetOrAdd( )
        {
            ICache<long, string> cache = new DictionaryCache<long, string>( );
            string result = cache.GetOrAdd( 1, key1 =>
            {
                Assert.That( key1, Is.EqualTo( 1 ) );
                return "value";
            } );
            Assert.That( result, Is.EqualTo( "value" ) );

            // Second attempt
            result = cache.GetOrAdd( 1, key1 =>
            {
                Assert.Fail( "This should not be called a second time" );
                return "value";
            } );
            Assert.That( result, Is.EqualTo( "value" ) );
            
        }
    }
}
