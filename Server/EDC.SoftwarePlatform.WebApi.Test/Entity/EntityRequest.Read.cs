// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using EDC.ReadiNow.Test;
using EDC.Security;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using EDC.ReadiNow.Expressions;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Test.EntityRequests
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class EntityRequest_Read
    {
        [Test]
        [TestCase( "core:type" )]
        [TestCase( "test:aaPeterAylett" )]
        [RunAsDefaultTenant]
        public void LoadIdOnly( string alias )
        {
            string query =
            @"{
                ""queries"": [
                    ""id""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""" + alias + @"""],
                        ""rq"":0,
                        ""get"":""basic""
                    }
                ]
            }";
            TestEntity.RunBatchTest( query );
        }

        [Test]
        [TestCase( "core:type" )]
        [TestCase( "test:aaPeterAylett" )]
        [TestCase( "test:aaPepsi" )]
        [TestCase( "test:aaShapes" )]
        [TestCase( "test:aaScottHopwood" )]
        [TestCase( "test:aaBasil" )]
        [TestCase( "test:aaGarlic" )]
        [RunAsDefaultTenant]
        public void LoadTypeOnly(string alias)
        {
            string query =
            @"{
                ""queries"": [
                    ""isOfType.id""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""" + alias + @"""],
                        ""rq"":0,
                        ""get"":""basic""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query);
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadNameOnly()
        {
            string query =
            @"{
                ""queries"": [
                    ""name""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""core:type""],
                        ""rq"":0,
                        ""get"":""basic""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query);
        }

        [Test]
        [RunAsDefaultTenant]
        public void InvalidAliasRequest()
        {
            string query =
            @"{
                ""queries"": [
                    ""name""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""invalid:alias""],
                        ""rq"":0,
                        ""get"":""basic""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query, HttpStatusCode.NotFound);
        }

        [Test]
        [RunAsDefaultTenant]
        public void InvalidQuery()
        {
            string query =
            @"{
                ""queries"": [
                    ""blah blah""
                ],
                ""requests"": [                    
                    {
                        ""aliases"":[""core:type""],
                        ""rq"":0,
                        ""get"":""basic""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query, HttpStatusCode.BadRequest);
        }

        [Test]
        [RunAsDefaultTenant]
        public void MissingQuery()
        {
            string query =
            @"{
                ""queries"": [
                    ""name""
                ],
                ""requests"": [                    
                    {
                        ""aliases"":[""core:type""],
                        ""rq"":999,
                        ""get"":""basic""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query, HttpStatusCode.BadRequest);
        }

        [Test]
        [RunAsDefaultTenant]
        public void BatchRequest()
        {
            string query =
            @"{
                ""queries"": [
                    ""name,description"",
                    ""core:firstName,core:lastName"",
                    ""some invalid query""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""test:person""],
                        ""rq"":0,
                        ""get"":""instances""
                    },
                    {
                        ""aliases"":[""test:peterAylett""],
                        ""rq"":1,
                        ""get"":""basic""
                    },
                    {
                        ""ids"":[1],
                        ""rq"":2,
                        ""get"":""basic""
                    },
                    {
                        ""aliases"":[""invalid:alias""],
                        ""rq"":0,
                        ""get"":""basic""
                    }
                ]
            }";

            using (var request = new PlatformHttpRequest(@"data/v2/entity", PlatformHttpMethod.Post))
            {
                request.PopulateBodyString(query);

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Check response body
                JsonQueryResult res = request.DeserialiseResponseBody<JsonQueryResult>();
                Assert.IsNotNull(res);

                Assert.AreEqual(4, res.Results.Count, "res.Results.Count");
                Assert.AreEqual(HttpStatusCode.OK, res.Results[0].Code, "res.Results[0].Code");
                Assert.AreEqual(HttpStatusCode.OK, res.Results[1].Code, "res.Results[1].Code");
                Assert.AreEqual(HttpStatusCode.BadRequest, res.Results[2].Code, "res.Results[2].Code");
                Assert.AreEqual(HttpStatusCode.NotFound, res.Results[3].Code, "res.Results[3].Code");
                Assert.Greater(res.Results[0].Ids.Count, 1, "res.Results[0].IDs.Count");
                Assert.AreEqual(1, res.Results[1].Ids.Count, "res.Results[1].IDs.Count");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntityUpgradeIdsByEntityIds()
        {
            // get three known entites
            var aliasEntity = Entity.Get(new EntityRef("alias"));
            var nameEntity = Entity.Get(new EntityRef("name"));
            var descEntity = Entity.Get(new EntityRef("description"));

            // store them locally to compare with later on
            IDictionary<long, Guid> refEntityDict = new Dictionary<long, Guid>();
            refEntityDict.Add(aliasEntity.Id, aliasEntity.UpgradeId);
            refEntityDict.Add(nameEntity.Id, nameEntity.UpgradeId);
            refEntityDict.Add(descEntity.Id, descEntity.UpgradeId);

            // Ids to send in request 
            var ids = new List<long>
            {
                aliasEntity.Id,
                nameEntity.Id,
                descEntity.Id
            };

            using (var request = new PlatformHttpRequest(@"data/v2/entity/getEntityUpgradeIdsByEntityIds", PlatformHttpMethod.Post))
            {
                request.PopulateBody(ids);

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var resultsDict = request.DeserialiseResponseBody<IDictionary<long, Guid>>();

                Assert.AreEqual(3, resultsDict.Count, "resultsDict.Count");
                Assert.AreEqual(aliasEntity.UpgradeId, resultsDict[aliasEntity.Id], "Upgrade id of aliasEntity does not match");
                Assert.AreEqual(nameEntity.UpgradeId, resultsDict[nameEntity.Id], "Upgrade id of nameEntity does not match");
                Assert.AreEqual(descEntity.UpgradeId, resultsDict[descEntity.Id], "Upgrade id of descEntity does not match");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetEntityIdsByUpgradeIds()
        {
            // get three known entites
            var aliasEntity = Entity.Get(new EntityRef("alias"));
            var nameEntity = Entity.Get(new EntityRef("name"));
            var descEntity = Entity.Get(new EntityRef("description"));

            // store them locally to compare with later on
            IDictionary<Guid, long> refEntityDict = new Dictionary<Guid, long>();
            refEntityDict.Add(aliasEntity.UpgradeId, aliasEntity.Id);
            refEntityDict.Add(nameEntity.UpgradeId, nameEntity.Id);
            refEntityDict.Add(descEntity.UpgradeId, descEntity.Id);

            // guids to send in request 
            var guids = new List<Guid>
            {
                aliasEntity.UpgradeId,
                nameEntity.UpgradeId,
                descEntity.UpgradeId
            };

            using (var request = new PlatformHttpRequest(@"data/v2/entity/getEntityIdsByUpgradeIds", PlatformHttpMethod.Post))
            {
                request.PopulateBody(guids);

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var resultsDict = request.DeserialiseResponseBody<IDictionary<string, long>>();

                Assert.AreEqual(3, resultsDict.Count, "resultsDict.Count");
                Assert.AreEqual(aliasEntity.Id, resultsDict[aliasEntity.UpgradeId.ToString( "B" )], "aliasEntity id does not match");
				Assert.AreEqual( nameEntity.Id, resultsDict [ nameEntity.UpgradeId.ToString( "B" ) ], "nameEntity id does not match" );
				Assert.AreEqual( descEntity.Id, resultsDict [ descEntity.UpgradeId.ToString( "B" ) ], "descEntity id does not match" );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadWildcardFieldsWhenFieldsHaveNoAlias()
        {
            using (var request = new PlatformHttpRequest(@"data/v2/entity?typename=Definition&name=Pizza&request=name,instancesOfType.*", PlatformHttpMethod.Get))
            {
                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Check response body
                if (request.HttpWebResponse == null)
                    throw new NullReferenceException();
                var reader = new StreamReader(request.HttpWebResponse.GetResponseStream());
                var body = reader.ReadToEnd();
                Assert.IsNotEmpty(body);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadFilterExactInstances()
        {
            string query =
            @"{
                ""queries"": [
                    ""name""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""core:report""],
                        ""rq"":0,
                        ""get"":""filterexactinstances"",
                        ""filter"":""[Name]='Mail Boxes'""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query);
        }

        [Test]
        [RunAsDefaultTenant]
        public void LoadFilterInstances()
        {
            string query =
            @"{
                ""queries"": [
                    ""name""
                ],
                ""requests"": [
                    {
                        ""aliases"":[""core:resourceViewer""],
                        ""rq"":0,
                        ""get"":""filterinstances"",
                        ""filter"":""[Name]='Mail Boxes'""
                    }
                ]
            }";
            TestEntity.RunBatchTest(query);
        }


        /// <summary>
        ///     Test creating a type and form, and verifying cache invalidation occurred on definition.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void CacheInvalidation_Bug21102_TypeAndForm()
        {
            // Request all definitions and enum types
            string query = @"{""queries"":[""instancesOfType.name""], ""requests"":[{""get"":""basic"", ""aliases"":[""core:definition""], ""hint"":""CacheInvalidation_Bug21102"", ""rq"":0}]}";
            JsonQueryResult res = TestEntity.RunBatchTest(query, HttpStatusCode.OK, 1);
            int count1 = res.Entities.Count;
            Assert.IsTrue(count1 > 1);


            // Create definition
            string defnName = "Test Defn " + Guid.NewGuid().ToString();
            string formName = "Test Form " + Guid.NewGuid().ToString();
            string createBody = @"{
    ""ids"": [9007199254740922],
    ""entities"": [
    	{
        	""id"": 3121, ""typeIds"": [],
        	""fields"": [], ""relationships"": [],
        	""dataState"": ""Update""
        }, {
			""id"": 9007199254740925,
			""typeIds"": [2436],
			""fields"": [{
				""fieldId"": 2397, ""typeName"": ""String"", ""svalue"": """ + defnName + @"""
			}],
			""relationships"": [{
				""relTypeId"": { ""id"": 2873, ""ns"": ""core"", ""alias"": ""inherits"" },
				""instances"": [{ ""entity"": 3121, ""relEntity"": 0, ""dataState"": ""Create"" }]
			}],
			""dataState"": ""Create""
        }, {
			""id"": 9007199254740922,
			""typeIds"": [4365],
			""fields"": [{
				""fieldId"": 9007199254740914, ""typeName"": ""String"", ""svalue"": """ + formName + @"""
			}],
			""relationships"": [{
				""relTypeId"": { ""id"": 9007199254740913, ""ns"": ""console"", ""alias"": ""typeToEditWithForm"" },
				""instances"": [{ ""entity"": 9007199254740925, ""relEntity"": 0, ""dataState"": ""Create"" }],
				""removeExisting"": true
			}],
			""dataState"": ""Create""
        }
    ],
    ""entityRefs"": [
        { ""id"": 9007199254740922 },
        { ""id"": 9007199254740925 },
        { ""id"": 3121, ""ns"": ""core"", ""alias"": ""userResource"" },
        { ""id"": 2397, ""ns"": ""core"", ""alias"": ""name"" },
        { ""id"": 2873, ""ns"": ""core"", ""alias"": ""inherits"" },
        { ""id"": 4365, ""ns"": ""console"", ""alias"": ""customEditForm"" },
        { ""id"": 2436, ""ns"": ""core"", ""alias"": ""definition"" },
        { ""id"": 9007199254740914, ""ns"": ""core"", ""alias"": ""name"" },
        { ""id"": 9007199254740913, ""ns"": ""console"", ""alias"": ""typeToEditWithForm"" }
    ]
}";

            createBody = createBody.Replace("3121", Entity.GetId("core:userResource").ToString());
            createBody = createBody.Replace("2397", Entity.GetId("core:name").ToString());
            createBody = createBody.Replace("2873", Entity.GetId("core:inherits").ToString());
            createBody = createBody.Replace("4365", Entity.GetId("console:customEditForm").ToString());
            createBody = createBody.Replace("2436", Entity.GetId("core:definition").ToString());

            using (var request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post))
            {
                request.PopulateBodyString(createBody);

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }

            // Request all definitions again
            res = TestEntity.RunBatchTest(query, HttpStatusCode.OK, 1);
            int count2 = res.Entities.Count;
            Assert.AreEqual(count1 + 1, count2, "Look for definition");
        }
        

        /// <summary>
        ///     Test creating a type, and verifying cache invalidation occurred on definition.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void CacheInvalidation_Bug21102_TypeOnly()
        {
            // Request all definitions and enum types
			string query = @"{""queries"":[""instancesOfType.name""], ""requests"":[{""get"":""basic"", ""aliases"":[""core:definition""], ""hint"":""CacheInvalidation_Bug21102"", ""rq"":0}]}";
            JsonQueryResult res = TestEntity.RunBatchTest(query, HttpStatusCode.OK, 1);
            int count1 = res.Entities.Count;
            Assert.IsTrue(count1 > 1);


            // Create definition
            string defnName = "Test Defn " + Guid.NewGuid().ToString();
            string createBody = @"{
    ""ids"": [9007199254740925],
    ""entities"": [
    	{
        	""id"": 3121, ""typeIds"": [],
        	""fields"": [], ""relationships"": [],
        	""dataState"": ""Update""
        }, {
			""id"": 9007199254740925,
			""typeIds"": [2436],
			""fields"": [{
				""fieldId"": 2397, ""typeName"": ""String"", ""svalue"": """ + defnName + @"""
			}],
			""relationships"": [{
				""relTypeId"": { ""id"": 2873, ""ns"": ""core"", ""alias"": ""inherits"" },
				""instances"": [{ ""entity"": 3121, ""relEntity"": 0, ""dataState"": ""Create"" }]
			}],
			""dataState"": ""Create""
        }
    ],
    ""entityRefs"": [
        { ""id"": 9007199254740925 },
        { ""id"": 3121, ""ns"": ""core"", ""alias"": ""userResource"" },
        { ""id"": 2397, ""ns"": ""core"", ""alias"": ""name"" },
        { ""id"": 2873, ""ns"": ""core"", ""alias"": ""inherits"" },
        { ""id"": 2436, ""ns"": ""core"", ""alias"": ""definition"" }
    ]
}";

            createBody = createBody.Replace("3121", Entity.GetId("core:userResource").ToString());
            createBody = createBody.Replace("2397", Entity.GetId("core:name").ToString());
            createBody = createBody.Replace("2873", Entity.GetId("core:inherits").ToString());
            createBody = createBody.Replace("2436", Entity.GetId("core:definition").ToString());

            using (var request = new PlatformHttpRequest(@"data/v1/entity", PlatformHttpMethod.Post))
            {
                request.PopulateBodyString(createBody);

                var response = request.GetResponse();

                // check that it worked (200)
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }

            // Request all definitions again
            res = TestEntity.RunBatchTest(query, HttpStatusCode.OK, 1);
            int count2 = res.Entities.Count;
            Assert.AreEqual(count1 + 1, count2, "Look for definition");
        }
    }
}