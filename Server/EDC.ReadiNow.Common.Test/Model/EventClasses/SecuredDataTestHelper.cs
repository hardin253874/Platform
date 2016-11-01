// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.Security;
using NUnit.Framework;
using Autofac;
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Security.SecuredData;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    public static class SecuredDataTestHelper
    {
        public static void TestField(EntityType eType, Field field, Field secureIdField)
        {
            var password = CryptoHelper.GetRandomPrintableString(10);

            // Set
            var e = Entity.Create(eType);

            e.SetField(field, password);
            e.Save();

            var secureId = e.GetField<Guid?>(secureIdField);

            Assert.That(secureId, Is.Not.Null);
            var savedPassword = Factory.SecuredData.Read((Guid)secureId);
            Assert.That(savedPassword, Is.EqualTo(password));

            // Clear
            if (!(field.IsRequired ?? false ))
            {
                e = e.AsWritable();
                e.SetField(field, string.Empty);
                e.Save();

                secureId = e.GetField<Guid?>(secureIdField);

                Assert.That(secureId, Is.Not.Null);

                var clearedPassword = Factory.SecuredData.Read((Guid)secureId);
                Assert.That(clearedPassword, Is.EqualTo(string.Empty));
            }
        }


      
        /// <summary>
        /// Test the upgrade process. Ensure that this is only called from within a transaction as it disabled the onSave events
        /// </summary>
        public static void TestUpgrade(EntityType eType, Field field, Field secureIdField, Func<string, string> encrypter)
        {
            // Set
            var password = CryptoHelper.GetRandomPrintableString(10);
            TestValue(eType, field, secureIdField, encrypter, password);

            if (!(field.IsRequired ?? false))
            {
                // Empty value
                TestValue(eType, field, secureIdField, encrypter, string.Empty);

                // Null
                TestNull(eType, field, secureIdField);
            }
        }

      class DisabledSaveHelper : ISecuredDataSaveHelper
        {
            public void UpgradeField(string securedDataContext, EntityType entityType, EntityRef dataField, EntityRef secureIdField, Func<string, string> decrytor)
            {
            }

            public void OnBeforeSave(string securedDataContext, EntityRef dataField, EntityRef secureIdField, IEntity entity)
            {
            }
        }


        public static void TestValue(EntityType eType, Field field, Field secureIdField, Func<string, string> encrypter, string password)
        {
            IEntity e = null;

            // Stop the normal save trigger running when we set up the fields.
            using (var scope = Factory.Current.BeginLifetimeScope(builder => builder.RegisterType<DisabledSaveHelper>().As<ISecuredDataSaveHelper>()))
            using (Factory.SetCurrentScope(scope))
            {
                // Set
                e = Entity.Create(eType);

                e.SetField(field, encrypter(password));
                e.Save();
            }

            SecureDataUpgradeHelper.Upgrade("EDC");

            e = Entity.Get(e.Id);
                
            Assert.That(e.GetField<string>(field), Is.Not.EqualTo(password));

            var secureId = e.GetField<Guid?>(secureIdField);

            Assert.That(secureId, Is.Not.Null);

            var savedPassword = Factory.SecuredData.Read((Guid)secureId);
            Assert.That(savedPassword, Is.EqualTo(password));
        }

        public static void TestNull(EntityType eType, Field field, Field secureIdField)
        {
            IEntity e = null;

            // Set
            e = Entity.Create(eType);
            e.Save();

            SecureDataUpgradeHelper.Upgrade("EDC");

            e = Entity.Get(e.Id);

            Assert.That(e.GetField<string>(field), Is.Null);

            var secureId = e.GetField<Guid?>(secureIdField);

            Assert.That(secureId, Is.Null);
        }

    }
}
