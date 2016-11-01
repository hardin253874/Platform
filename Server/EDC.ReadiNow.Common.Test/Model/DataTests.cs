// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.Database;
using NUnit.Framework;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Data tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class DataTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void EnsureApprovedDefinitionsOnly( )
        {
            List<string> inCore = new List<string>( );
            List<string> inOthers = new List<string>( );

            IEnumerable<Definition> definitions = Entity.GetInstancesOfType<Definition>( false, "name, inSolution.name" );

            foreach (var defn in definitions)
            {
                string app = defn.InSolution.Name;

                if ( app == "ReadiNow Core" || app == "ReadiNow Core Data" )
                {
                    inCore.Add( defn.Name );
                }
                else if ( app == "ReadiNow Console" )
                {
                    inOthers.Add( defn.Name );
                }
            }

            Assert.That( inOthers, Is.Empty, "There should be no definitions in Console applications" );


            // If you want to add something to the UI, make it a managedType instead. It will appear in the picker reports if you turn on the 'Show advanced' analyser option.
            // If you want something to appear in the UI, just so you can add content to Core Data, then use managedType, then change it back to type if there is no business case to keep it.

            // IMPORTANT ::: Changes require approval of PMs and architects.   (And Sales also cares about this too)
            string approvedDefinitions = "Appointment, Document, Email Contact, Event, Organisation Assets, Organisation Structure, Person, Phone Contact, Task";
            
            // Note: Email Contact and Phone Contact have been approved for removal
            // Note: Document is pending discussion
            // READ THE ABOVE 


            inCore.Sort( );
            string foundDefinitions = string.Join( ", ", inCore );
            Assert.That( foundDefinitions, Is.EqualTo( approvedDefinitions ) );

            Assert.That( inOthers, Is.Empty, "There should be no definitions in Core Data or Console applications" );
        }

        [Test]
		[RunAsDefaultTenant]
        [TestCase("Type")]
        [TestCase("Report")]
        [TestCase("Role")]
        [TestCase("StringPattern")]
        [TestCase("Solution")]
        [TestCase("PasswordPolicy")]
        [TestCase("NavContainer")]
        [TestCase("DocumentType")]
        [TestCase("Field")]
        [TestCase("FieldGroup")]
		public void EnsureNameIsSet(string viewName)
		{
            string solutions = string.Join("','", TestHelpers.ValidatableSolutions);

            // Find null or blank names, but only look within the specified solutions.
            string sql = @"
                select t.*, sn.Data from _v" + viewName + @" t
                    join Entity ris on ris.UpgradeId = '7C77C3A0-75B5-4C59-99F6-3BA9229E6A55' -- resourceInSolution
                    join Entity name on name.UpgradeId = 'F8DEF406-90A1-4580-94F4-1B08BEAC87AF' -- name
                    join Relationship r on r.FromId = t.Id and r.TypeId = ris.Id
                    join Data_NVarChar sn on r.ToId = sn.EntityId and sn.FieldId = name.Id
                where sn.Data in ('" + solutions + @"')
                    and (t.name is null or t.name = '')";

		    using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
		    {
			    using ( IDbCommand cmd = ctx.CreateCommand( sql ) )
			    {
				    using ( IDataReader reader = cmd.ExecuteReader( ) )
				    {
					    List<string> failures = new List<string>();

					    if ( reader != null )
					    {
						    while ( reader.Read( ) )
						    {
                                failures.Add(string.Format("{0}", reader.IsDBNull(1) ? reader.GetInt64(0).ToString() : reader.GetString(1)));
						    }
					    }
                        Assert.IsEmpty(failures);
				    }
			    }
		    }
		}
	}
}