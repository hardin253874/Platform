// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses
{
    public class PersonHasAccountEventTarget : FilteredTarget
    {
        protected override HashSet<long> GetWatchedFields()
        {
            return new HashSet<long>();
        }

        protected override HashSet<long> GetWatchedForwardRels()
        {
            return new HashSet<long>
            {
                Person.PersonHasUserAccount_Field.Id,
            };
        }

        protected override HashSet<long> GetWatchedReverseRels()
        {
            return new HashSet<long>();
        }




        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        protected override bool FilteredOnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            using (new SecurityBypassContext())
            {
                foreach (var e in entities)
                {
                    var accounts = e.GetRelationships(Person.PersonHasUserAccount_Field);
                    e.SetField(Person.CalcPersonHasAccount_Field, accounts.Any());
                }
            }

            return false;
        }


        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void FilteredOnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // Do nothing
        }
    }
}
