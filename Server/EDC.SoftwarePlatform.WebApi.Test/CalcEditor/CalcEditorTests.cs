// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.CalcEditor
{
    [DataContract]
    public class CompileResult
    {
        [DataMember(Name = "resultType", EmitDefaultValue = true, IsRequired = false)]
        public string ResultType { get; set; }

        [DataMember(Name = "error", EmitDefaultValue = true, IsRequired = false)]
        public string ErrorMessage { get; set; }
    }

    [TestFixture]
    public class CalcEditorTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test01_Compile_GetResultType()
        {
            CompileResult result = TestCompile("{'expr':'123'}");
            Assert.AreEqual("Int32", result.ResultType);
            Assert.IsNull(result.ErrorMessage);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test02_Compile_GetErrorMessage()
        {
            CompileResult result = TestCompile("{'expr':'123a'}");
            Assert.AreEqual("Number cannot be followed by a letter. (pos 1)", result.ErrorMessage);
            Assert.AreEqual("None", result.ResultType);           
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test03_Compile_PassContext()
        {
            CompileResult result = TestCompile("{'expr':'Name','context':'resource'}");
            Assert.AreEqual("String", result.ResultType);
            Assert.IsNull(result.ErrorMessage);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test04_Compile_PassParameter()
        {
            string json = @"{
                'expr':'convert(int, [param one]) * [param two]',
                'params':[
                    {'name':'param one', 'type':'String'},
                    {'name':'param two', 'type':'Int32'}
                ]}";

            CompileResult result = TestCompile(json);
            Assert.AreEqual("Int32", result.ResultType);
            Assert.IsNull(result.ErrorMessage);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test05_Compile_ExpectedType()
        {
            CompileResult result = TestCompile("{'expr':'123', 'expectedResultType':{'dataType':'String'} }");
            Assert.AreEqual("String", result.ResultType);
            Assert.IsNull(result.ErrorMessage);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test06_Compile_ExpectedTypeIncorrect()
        {
            CompileResult result = TestCompile("{'expr':'1.1', 'expectedResultType':{'dataType':'Int32'} }");
            Assert.AreEqual("Result was of type Decimal (const) but needed to be Int32.", result.ErrorMessage);
            Assert.AreEqual("None", result.ResultType);
        }


        private CompileResult TestCompile(string json)
        {
            var url = @"data/v1/calceditor/compile";

            using (var request = new PlatformHttpRequest(url, PlatformHttpMethod.Post))
            {
                request.PopulateBodyString(json.Replace("'","\""));

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Check response body
                return request.DeserialiseResponseBody<CompileResult>();
            }

        }
    }

}
