//// Copyright 2011-2016 Global Software Innovation Pty Ltd

//using System.Linq;
//using EDC.ReadiNow.EntityRequests;
//using EDC.ReadiNow.Model;
//using EDC.ReadiNow.Security;
//using FluentAssertions;
//using NUnit.Framework;

//namespace EDC.ReadiNow.Test.EntityRequests
//{
//    [TestFixture]
//    [RunWithTransaction]
//    public class ConfigXmlTests
//    {
//        [Test]
//        [RunAsDefaultTenant]
//        public void GenerateXmlForNewEntity()
//        {
//            using (new SetUser(Entity.Get<UserAccount>("core:administratorUserAccount")))
//            {
//                // Arrange
//                var chocBarsType = Entity.GetByName<EntityType>("AA_ChocBars").FirstOrDefault();
//                chocBarsType.Should().NotBeNull("Couldn't locate the type for choc bars.");

//                if (chocBarsType == null)
//                    return;

//                var e = Entity.Create(new EntityRef(chocBarsType.Id)).AsWritable<Resource>();
//                e.Alias = "test:kitkatWhite";
//                e.Name = "Kit-Kat White";
//                e.Description = "A kit-kat but with white chocolate.";
//                e.Save();

//                Entity.Exists(new EntityRef(e.Id)).Should().BeTrue();

//                // Act
//                var entResult = ConfigXmlGenerator.CreateXml(new EntityRef(e.Id));

//                // Assert
//                entResult.Should().NotBeNullOrEmpty();
//                entResult.Should().Contain(string.Format(@"<!-- 'Kit-Kat White' aa_chocbars -->
//  <aAChocBars{0}gen>
//    <alias>t:kitkatWhite</alias>
//    <name>Kit-Kat White</name>
//    <description>A kit-kat but with white chocolate.</description>
//    <createdBy>administratorUserAccount</createdBy>
//    <lastModifiedBy>administratorUserAccount</lastModifiedBy>
//    <securityOwner>administratorUserAccount</securityOwner>
//  </aAChocBars{0}gen>", chocBarsType.Id));
//            }
//        }

//        [Test]
//        [RunAsDefaultTenant]
//        public void GenerateXmlForNewDefinition()
//        {
//            using (new SetUser(Entity.Get<UserAccount>("core:administratorUserAccount")))
//            {
//                // Arrange
//                var d = Entity.Create<Definition>();
//                d.Name = "My New Definition";
//                d.Alias = "core:myNewDefinition";
//                d.Inherits.Add(Resource.Resource_Type);
//                d.Save();

//                Entity.Exists(new EntityRef(d.Id)).Should().BeTrue();

//                // Act
//                var defResult = ConfigXmlGenerator.CreateXml(new EntityRef(d.Id));

//                // Assert
//                defResult.Should().NotBeNullOrEmpty();
//                defResult.Should().Contain(@"<!-- 'My New Definition' definition -->
//  <definition>
//    <alias>myNewDefinition</alias>
//    <name>My New Definition</name>");
//                defResult.Should().EndWith(@"</definition>
//</resources>");
//            }
//        }
//    }
//}
