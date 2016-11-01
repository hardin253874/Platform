// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
	[TestFixture]
	[RunWithTransaction]
	public class PersonHasAccountEventTargetTests
    {

		[Test]
		[RunAsDefaultTenant]
		public void CreatedNoAccount( )
		{
            var person = new Person();
			person.Save( );

			person = Entity.Get<Person>( person.Id );

            Assert.That(person.CalcPersonHasAccount ?? false, Is.False);
		}


        [Test]
        [RunAsDefaultTenant]
        public void CreatedWithAccount()
        {
            var person = new Person();
            person.PersonHasUserAccount.Add(new UserAccount());
            person.Save();

            person = Entity.Get<Person>(person.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.True);
        }


        [Test]
        [RunAsDefaultTenant]
        public void UpdatedNoneToNone()
        {
            var person = new Person();
            person.Save();

            person = person.AsWritable<Person>();
            person.Name = "Bob";
            person.Save();
             
            person = Entity.Get<Person>(person.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.False);
        }

        [Test]
        [RunAsDefaultTenant]
        public void UpdatedNoneToHaving()
        {
            var person = new Person();
            person.Save();

            person = person.AsWritable<Person>();
            person.PersonHasUserAccount.Add(new UserAccount());
            person.Save();

            person = Entity.Get<Person>(person.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void UpdatedHavingToNone()
        {
            var person = new Person();
            person.PersonHasUserAccount.Add(new UserAccount());
            person.Save();

            person = person.AsWritable<Person>();
            person.PersonHasUserAccount.Clear();
            person.Save();

            person = Entity.Get<Person>(person.Id);

            Assert.That(person.CalcPersonHasAccount ?? false, Is.False);
        }
        

	}
}