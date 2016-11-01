// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System.Collections.Generic;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
	[TestFixture]
	[RunWithTransaction]
	public class UserAccountHasPersonEventTargetTests
    {

		[Test]
		[RunAsDefaultTenant]
		public void CreatedNoAccount( )
		{
            var account = new UserAccount();
            account.Save( );
            // no exceptions thrown
        }


        [Test]
        [RunAsDefaultTenant]
        public void CreatedWithAccount()
        {
            var account = new UserAccount();
            account.AccountHolder = new Person();
            account.Save();

            Assert.That(account.AccountHolder.CalcPersonHasAccount, Is.True);
            
            var person = Entity.Get<Person>(account.AccountHolder.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.True);
        }


        [Test]
        [RunAsDefaultTenant]
        public void UpdatedNoneToHaving()
        {
            var account = new UserAccount();
            account.Save();

            account = account.AsWritable<UserAccount>();
            account.AccountHolder = new Person();
            account.Save();

            var person = Entity.Get<Person>(account.AccountHolder.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.True);
        }


        [Test]
        [RunAsDefaultTenant]
        public void UpdatedNoneToHaving_ExitingPerson()
        {
            var account = new UserAccount();
            account.Save();

            var person = new Person();
            person.Save();

            account = account.AsWritable<UserAccount>();
            account.AccountHolder = person;
            account.Save();

            person = Entity.Get<Person>(account.AccountHolder.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void UpdatedHavingToNone()
        {
            var account = new UserAccount();
            account.AccountHolder = new Person();
            account.Save();

            var personId = account.AccountHolder.Id;

            account = account.AsWritable<UserAccount>();
            account.AccountHolder = null;
            account.Save();

            var person = Entity.Get<Person>(personId);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.False);
        }
           

	}
}