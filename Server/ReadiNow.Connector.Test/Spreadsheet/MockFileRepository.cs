// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using EDC.IO;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// A mock IFileRepository that takes content on its constructor and returns it for any token.
    /// </summary>

    class MockFileRepository : IFileRepository
    {
        public Stream Stream { get; }

        public MockFileRepository( Stream stream )
        {
            Stream = stream;
        }

        public static IFileRepository FromText( string content )
        {
            MemoryStream stream = new MemoryStream( );
            using ( StreamWriter sw = new StreamWriter( stream, System.Text.Encoding.Default, 1024, true ) )
            {
                sw.Write( content );
            }
            stream.Position = 0;
            return new MockFileRepository( stream );
        }

        public Stream Get( string token )
        {
            Stream.Position = 0;
            MemoryStream copy = new MemoryStream( (int)Stream.Length );
            Stream.CopyTo( copy );
            Stream.Position = 0;
            copy.Position = 0;
            return copy;
        }

        public void Delete( string token )
        {
            throw new NotImplementedException( );
        }

        public string Put( Stream stream )
        {
            throw new NotImplementedException( );
        }

        public IEnumerable<string> GetTokens( )
        {
            throw new NotImplementedException( );
        }
    }
}
