// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities
{
    public sealed class SetChoiceImplementation : ActivityImplementationBase, IRunNowActivity
    {
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var ResourceToUpdateKey = GetArgumentKey("setChoiceActivityResourceArgument");
            var ValueKey = GetArgumentKey("setChoiceActivityValueArgument");
            var FieldKey = GetArgumentKey("setChoiceActivityFieldArgument");
            var replaceExistingKey = GetArgumentKey("setChoiceActivityReplaceExisting");

            var choiceToUpdateRef = (IEntity) inputs[FieldKey];
            var resId = (IEntity)inputs[ResourceToUpdateKey];
            var valueId = (IEntity)inputs[ValueKey];
            
            bool replaceExisting = false;

            object replaceExistingObj;
            if (inputs.TryGetValue(replaceExistingKey, out replaceExistingObj))
            {
                replaceExisting = (bool?) replaceExistingObj ?? false;
            }

            var relationship = choiceToUpdateRef.As<Relationship>();

            SecurityBypassContext.RunAsUser(() =>
            {
                var resource = resId.AsWritable();    // Does not have to be a writeable copy because we are only manipulating relationships.

                var cardinality = relationship.Cardinality_Enum ?? CardinalityEnum_Enumeration.ManyToMany;

                replaceExisting =
                    replaceExisting
                    ||
                    cardinality == CardinalityEnum_Enumeration.OneToOne
                    ||
                    cardinality == CardinalityEnum_Enumeration.ManyToOne;


                var relCollection = resource.GetRelationships(choiceToUpdateRef, Direction.Forward);        // you can't have a reverse choice field.

                if (replaceExisting)
                    relCollection.Clear();

                relCollection.Add(valueId);

                resource.Save();
            });
        }
    }
}