// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses
{
    public class UserAccountHasPersonEventTarget : FilteredTarget
    {
        protected override HashSet<long> GetWatchedFields()
        {
            return new HashSet<long>();
        }

        protected override HashSet<long> GetWatchedForwardRels()
        {
            return new HashSet<long>();

        }

        protected override HashSet<long> GetWatchedReverseRels()
        {
            return new HashSet<long>
            {
                UserAccount.AccountHolder_Field.Id,
            };
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
                var saveList = new List<Person>();

                foreach (var e in entities)
                {
                    var newAccountHolder = e.GetRelationships<Person>(UserAccount.AccountHolder_Field, Direction.Reverse).FirstOrDefault();

                    if (e.IsTemporaryId && newAccountHolder != null )
                    {
                        SetPerson(newAccountHolder, true, saveList);
                    }
                    else
                    {
                        var originalAccountHolder = Entity.Get<UserAccount>(e.Id).AccountHolder;

                        if (originalAccountHolder == null && newAccountHolder != null)
                        {
                            SetPerson(newAccountHolder, true, saveList);
                        }

                        else if (originalAccountHolder != null && newAccountHolder == null)
                        {
                            // If the account was set from the other direction, we would not be firing this trigger. So we can assume that the only change was to remove this account.
                            // The reverse relationship will still contain our entry so we need to check is there are any other account holders.
                            SetPerson(originalAccountHolder, originalAccountHolder.PersonHasUserAccount.Count > 1, saveList);
                        }
                    }
                }

                if (saveList.Any())
                    Entity.Save(saveList);
            }

            return false;
        }


        void SetPerson(Person person, bool value, List<Person> saveList)
        {
            if (person.IsReadOnly)
            {
                person = person.AsWritable<Person>();
                saveList.Add(person);
            }

            person.CalcPersonHasAccount = value;
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
