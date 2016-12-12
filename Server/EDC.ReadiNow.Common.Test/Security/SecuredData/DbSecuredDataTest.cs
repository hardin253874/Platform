// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.SecuredData;
using EDC.Security;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Security.SecuredData
{
    [TestFixture]
    [RunWithTransaction]
    public class DbSecuredDataTest
    {
        DbSecuredData _dbSecuredData = new DbSecuredData();

        [TestCase(1, "context", "firstValue", "secondValue")]
        [TestCase(1, "context", "firstValue", "")]
        [TestCase(1, "context", "", "secondvalue")]
        [TestCase(1, "context", "", "")]
        public void CreateReadUpdateDelete(long tenantId, string context, string value, string updatedValue)
        {
            var secureId = _dbSecuredData.Create(tenantId, context, value);
            var getValue = _dbSecuredData.Read(secureId);

            Assert.That(getValue, Is.EqualTo(value));

            _dbSecuredData.Update(secureId, updatedValue);
            var getUpdate = _dbSecuredData.Read(secureId);


            Assert.That(getUpdate, Is.EqualTo(updatedValue));

            _dbSecuredData.Delete(secureId);
        }

        [Test]
        [ExpectedException(typeof(SecureIdNotFoundException))]
        public void DeleteClearsValue()
        {
            var secureId = _dbSecuredData.Create(1, "dummyContext", "dummyValue");
            _dbSecuredData.Delete(secureId);

            var oldvalue = _dbSecuredData.Read(secureId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateEmptySecureId()
        {
            var dummySecureId = Guid.Empty;
            _dbSecuredData.Update(dummySecureId, "Dummy value");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadEmptySecureId()
        {
            var dummySecureId = Guid.Empty;
            _dbSecuredData.Read(dummySecureId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteEmptySecureId()
        {
            var dummySecureId = Guid.Empty;
            _dbSecuredData.Delete(dummySecureId);
        }


        [Test]
        [ExpectedException(typeof(SecureIdNotFoundException))]
        public void UpdateMissingSecureId()
        {
            var dummySecureId = Guid.NewGuid();
            _dbSecuredData.Update(dummySecureId, "Dummy");
        }

        [Test]
        [ExpectedException(typeof(SecureIdNotFoundException))]
        public void ReadMissingSecureId()
        {
            var dummySecureId = Guid.NewGuid();
            var oldvalue = _dbSecuredData.Read(dummySecureId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNullValue()
        {
            _dbSecuredData.Create(1, "dummy", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateNullValue()
        {
            var dummySecureId = Guid.NewGuid();
            _dbSecuredData.Update(dummySecureId, null);
        }

		[Test]
		[RunAsDefaultTenant]
		public void SecuredValuesSurviveAfterDbMasterKeyChange( )
		{
			var dummyValue = "dummy value";
			var dummyContext = "dummy context";
			var newPassword = CryptoHelper.GetRandomPrintableString( 10 ) + "aA1!";

			var secureId = _dbSecuredData.Create( RequestContext.TenantId, dummyContext, dummyValue );

			// update master key

			using ( new GlobalAdministratorContext( ) )
			{
				var dbInfo = DatabaseContext.GetContext( ).DatabaseInfo;
				DatabaseHelper.CycleMasterKey( dbInfo, newPassword );
			}

			var getUpdate = _dbSecuredData.Read( secureId );
			Assert.That( getUpdate, Is.EqualTo( dummyValue ) );
		}
    }
}
