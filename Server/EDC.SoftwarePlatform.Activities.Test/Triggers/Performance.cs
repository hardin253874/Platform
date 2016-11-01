// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Triggers
{
    [TestFixture]
    public class Performance
    {

        [Test]
        [RunAsDefaultTenant]
        [Explicit]
        public void CalcThroughput()
        {
            int peopleCount = 200;
            int delay = 30;
            int delayDrop = 2;
            int innerCount = 200;

            List<Person> people = new List<Person>();

            try
            {

                for (int i = 0; i < peopleCount; i++)
                {
                    people.Add(new Person { FirstName = "CalcThroughput" + i, LastName = "Lastx" });
                }

                Entity.Save(people);

                var phoneContacts = new List<PhoneContact> { new PhoneContact(), new PhoneContact(), null };

                var random = new Random(1);     // Same seed so it is repeatable

                int pickIndex = 0;
                long lastDuration = 1000000;
                double throughput = 0;
                do
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    var finishedCount = 0;

                    for (var inner = 0; inner < innerCount; inner ++)
                    {
                        var person = people[pickIndex++ % peopleCount];
                        ThreadPool.QueueUserWorkItem((o) =>
                        {
                            try
                            {
                                person = person.AsWritable<Person>();
                                person.LastName = DateTime.Now.Ticks.ToString();
                                person.PersonHasPhoneContact.Clear();
                                person.PersonHasPhoneContact.Add( phoneContacts[random.Next(3)]);
                                person.Save();
                            }
                            finally
                            {
                                lock(this) { finishedCount++; }
                            }
                        });
                        Thread.Sleep(delay);
                    }

                    while (finishedCount < innerCount)
                        Thread.Sleep(5);

                    watch.Stop();

                    var totalDuration = watch.ElapsedMilliseconds;
                    var averageDuration = (double) totalDuration / (double) innerCount;
                    var change = lastDuration - totalDuration;
                    throughput = 1000.0 / averageDuration;

                    Console.WriteLine("Throughput: " + throughput + "\tDelay: " + delay);

                    if ((change/totalDuration) < -0.1 || delay <= 0) // we are going backwards by more than 10% or don't have enough parallelism 
                        break;
                    else
                    {
                        delay -= delayDrop;
                    }

                    lastDuration = totalDuration;

                    Thread.Sleep(500);      // Let everything settle down.

                }
                while (true);
            }
            finally
            {
                Entity.Delete(people.Select(p=>p.Id));
            }

           
        }
    }
}
