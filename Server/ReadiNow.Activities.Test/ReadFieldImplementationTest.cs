// Copyright 2011-2016 Global Software Innovation Pty Ltd
//using System.Collections.Generic;
//using EDC.ReadiNow.Model;
//using EDC.ReadiNow.Test;
//using NUnit.Framework;

//namespace EDC.SoftwarePlatform.Activities.Test
//{
//    [TestFixture]
//    public class ReadFieldImplementationTest : TestBase
//    {
//        [Test]
//        [RunAsDefaultTenant]
//        public void ReadDefaultValue( )
//        {
//            var nullBob = new Employee( );
//            nullBob.Save( );
//            ToDelete.Add( nullBob.Id );

//            var readAction = new ReadFieldActivity( );
//            readAction.Save( );
//            ToDelete.Add( readAction.Id );

//            var readActionAs = readAction.As<WfActivity>( );


//            ActivityImplementationBase nextActivity = readActionAs.CreateWindowsActivity( );

//            var args = new Dictionary<string, object>
//                {
//                    {
//                        "Resource to read", ( EntityRef ) nullBob
//                    },
//                    {
//                        "Field to read", ( EntityRef ) "shared:age"
//                    }
//                };

//            IDictionary<string, object> result = RunActivity( nextActivity, args );

//            object value = result[ "Value" ];
//            Assert.AreEqual( 0, value );

//            Assert.AreEqual( true, result[ "Is Empty" ] );
//        }

//        [Test]
//        [RunAsDefaultTenant]
//        public void ReadField( )
//        {
//            var bob = new Employee
//                {
//                    Age = 42
//                };
//            bob.Save( );
//            ToDelete.Add( bob.Id );

//            var readAction = new ReadFieldActivity( );
//            readAction.Save( );
//            ToDelete.Add( readAction.Id );

//            var readActionAs = readAction.As<WfActivity>( );


//            ActivityImplementationBase nextActivity = readActionAs.CreateWindowsActivity( );

//            var args = new Dictionary<string, object>
//                {
//                    {
//                        "Resource to read", ( EntityRef ) bob
//                    },
//                    {
//                        "Field to read", ( EntityRef ) "shared:age"
//                    }
//                };

//            IDictionary<string, object> result = RunActivity( nextActivity, args );

//            object value = result[ "Value" ];
//            Assert.AreEqual( 42, value );
//        }

//        [Test]
//        [RunAsDefaultTenant]
//        public void ReadNull( )
//        {
//            var nullBob = new Employee( );
//            nullBob.Save( );
//            ToDelete.Add( nullBob.Id );

//            var readAction = new ReadFieldActivity( );
//            readAction.Save( );
//            ToDelete.Add( readAction.Id );

//            var readActionAs = readAction.As<WfActivity>( );


//            ActivityImplementationBase nextActivity = readActionAs.CreateWindowsActivity( );

//            var args = new Dictionary<string, object>
//                {
//                    {
//                        "Resource to read", ( EntityRef ) nullBob
//                    },
//                    {
//                        "Field to read", ( EntityRef ) "core:description"
//                    }
//                };

//            IDictionary<string, object> result = RunActivity( nextActivity, args );

//            object value = result[ "Value" ];
//            Assert.AreEqual( "", value );

//            Assert.AreEqual( true, result[ "Is Empty" ] );
//        }

//        [Test]
//        [RunAsDefaultTenant]
//        public void ReadEmptyString()
//        {
//            var emptyBob = new Employee() { Description = string.Empty };
//            emptyBob.Save();
//            ToDelete.Add(emptyBob.Id);

//            var readAction = new ReadFieldActivity();
//            readAction.Save();
//            ToDelete.Add(readAction.Id);

//            var readActionAs = readAction.As<WfActivity>();


//            ActivityImplementationBase nextActivity = readActionAs.CreateWindowsActivity();

//            var args = new Dictionary<string, object>
//                {
//                    {
//                        "Resource to read", ( EntityRef ) emptyBob
//                    },
//                    {
//                        "Field to read", ( EntityRef ) "core:description"
//                    }
//                };

//            IDictionary<string, object> result = RunActivity(nextActivity, args);

//            object value = result["Value"];
//            Assert.AreEqual("", value);

//            Assert.AreEqual(true, result["Is Empty"]);
//        }

//        [Test]
//        [RunAsDefaultTenant]
//        public void ReadNonEmptyString()
//        {
//            var nonEmptyBob = new Employee() { Description = "my description" };
//            nonEmptyBob.Save();
//            ToDelete.Add(nonEmptyBob.Id);

//            var readAction = new ReadFieldActivity();
//            readAction.Save();
//            ToDelete.Add(readAction.Id);

//            var readActionAs = readAction.As<WfActivity>();


//            ActivityImplementationBase nextActivity = readActionAs.CreateWindowsActivity();

//            var args = new Dictionary<string, object>
//                {
//                    {
//                        "Resource to read", ( EntityRef ) nonEmptyBob
//                    },
//                    {
//                        "Field to read", ( EntityRef ) "core:description"
//                    }
//                };

//            IDictionary<string, object> result = RunActivity(nextActivity, args);

//            object value = result["Value"];
//            Assert.AreEqual("my description", value);

//            Assert.AreEqual(false, result["Is Empty"]);
//        }
//    }
//}