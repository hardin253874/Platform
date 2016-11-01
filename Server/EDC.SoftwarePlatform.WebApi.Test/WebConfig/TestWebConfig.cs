// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.ReadiNow.IO;
using System.IO;

namespace EDC.SoftwarePlatform.WebApi.Test.WebConfig
{
    [TestFixture]
    public class TestWebConfig
    {
        [Test]
        public void CheckDebugFlagIsOff( )
        {
            string installPath = SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Install );
            string configFile = Path.Combine(installPath, @"SpApi\Web.config");

            Assert.That( File.Exists( configFile ), Is.True, "Web.config not found at " + configFile );

            XmlDocument doc = new XmlDocument( );
            doc.Load( configFile );

            XmlNode compilation;
            compilation = doc.SelectSingleNode( "/configuration/system.web/compilation" );
            Assert.That( compilation, Is.Not.Null, "compilation element should be present" );

            compilation = doc.SelectSingleNode( "/configuration/system.web/compilation[@debug='true']" );
            Assert.That( compilation, Is.Null, "compilation debug attribute should be off" );  
        }
    }
}