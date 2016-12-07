// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using ReadiNow.DocGen;

namespace ReadiNow.DocGen.Test.MacroParser
{
    [TestFixture]
    public class DocumentTests
    {
        const bool WriteToExpected = false;
        const bool WriteToActual = false;

        // Ignore these:
        //[TestCase("Definitions")]
        //[TestCase("Workflow Activities")]

        // Fix these:
        //[TestCase("All Employee table with sum")]
        //[TestCase("All Employees with TOC")]
        //[TestCase("All Fields data")]
        //[TestCase("All Fields calculations")]
        //[TestCase("Parent")]
        [TestCase("Clipart")]
        [TestCase("Nested lists")]
        [TestCase("Repeated Picture")]
        [TestCase("Static calculations")]
        //[TestCase("Test error messages")] // broken by shared data changes
        [TestCase("Test errors2")]
        [TestCase("Test lists")]
        [TestCase("Test paragraphs")]
        [TestCase("Test Position")]
        [TestCase("Test table sum")]
        [TestCase("Unclosed")]
        [RunAsDefaultTenant]        
        public void TestTemplate(string file)
        {
            #pragma warning disable 429
            Stream actual =
                WriteToExpected ? GetOutputStream("Expected", file)
                : (WriteToActual ? GetOutputStream("Actual", file)
                : new MemoryStream());
            #pragma warning restore 429

            using (actual)
            using (Stream template = GetStream("Templates", file))
            using (Stream expected = GetStream("Expected", file))
            {
                Assert.Greater(template.Length, 0, "Template stream length");

                var settings = new GeneratorSettings {  TimeZoneName = TimeZoneHelper.SydneyTimeZoneName };
                Factory.DocumentGenerator.CreateDocument(template, actual, settings);
                actual.Flush();
                actual.Position = 0;

                if (!WriteToExpected)
                {
                    Assert.Greater(expected.Length, 0, "Expected stream length");
                    Assert.AreEqual(expected.Length, actual.Length, "Actual stream length");

                    //byte[] exphash = MD5.Create().ComputeHash(expected);
                    //byte[] acthash = MD5.Create().ComputeHash(actual);
                    //Assert.IsTrue(exphash.SequenceEqual(acthash), "Hash");
                }
            }

        }

        private Stream GetStream(string folder, string file)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("ReadiNow.DocGen.Test." + folder + "." + file + ".docx");
            return stream;
        }

        private Stream GetOutputStream(string folder, string file)
        {
            string curDir = Assembly.GetExecutingAssembly().CodeBase;
            // e.g. curDir = "file:///C:/Development/Untested/EDC.ReadiNow.Common.Test/bin/Debug/EDC.ReadiNow.Common.Test.DLL"
            string solutionRoot = curDir.Substring(8, curDir.IndexOf("ReadiNow.DocGen.Test") - 8).Replace("/", "\\");
            string path = Path.Combine(solutionRoot, "ReadiNow.DocGen.Test\\" + folder + "\\" + file + ".docx");
            // e.g: path= C:\Development\Untested\EDC.ReadiNow.Common.Test\Metadata\Query\Select single column.test
            FileStream result = File.Create(path);
            return result;
        }


    }
}
