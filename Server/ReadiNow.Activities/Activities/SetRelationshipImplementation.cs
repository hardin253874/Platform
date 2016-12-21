// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities
{
    public sealed class SetRelationshipImplementation : ActivityImplementationBase, IRunNowActivity
    {
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var OriginKey = GetArgumentKey("setRelationshipActivityOriginArgument");
            var DestinationKey = GetArgumentKey("setRelationshipActivityDestinationArgument");
            var RelationshipKey = GetArgumentKey("setRelationshipActivityRelationshipArgument");
            var replaceExistingKey = GetArgumentKey("setRelationshipActivityReplaceExisting");
            var isReverseKey = GetArgumentKey("setRelationshipActivityIsReverse");

            var originRef = (IEntity)inputs[OriginKey];
            var relationshipRef = (IEntity)inputs[RelationshipKey];

            IEntity destinationRef = null;

            object destinationRefObj;
            if (inputs.TryGetValue(DestinationKey, out destinationRefObj))
            {
                destinationRef = (IEntity) destinationRefObj;
            }

            bool replaceExisting = false;

            object replaceExistingObj;
            if (inputs.TryGetValue(replaceExistingKey, out replaceExistingObj))
            {
                replaceExisting = (bool?) replaceExistingObj ?? false;
            }

            var direction = Direction.Forward;

            object isReverseObj;
            if (inputs.TryGetValue(isReverseKey, out isReverseObj))
            {
                direction = ((bool?)isReverseObj ?? false) ? Direction.Reverse : Direction.Forward;
            }


            var relationship = relationshipRef.As<Relationship>();

            SecurityBypassContext.RunAsUser(() =>
            {
                var origin = originRef.AsWritable<Entity>();

                var cardinality = relationship.Cardinality_Enum ?? CardinalityEnum_Enumeration.ManyToMany;

                replaceExisting =
                    replaceExisting
                    ||
                    cardinality == CardinalityEnum_Enumeration.OneToOne
                    ||
                    (direction == Direction.Forward && cardinality == CardinalityEnum_Enumeration.ManyToOne)
                    ||
                    (direction == Direction.Reverse && cardinality == CardinalityEnum_Enumeration.OneToMany)
                    ;

                var relCollection = origin.GetRelationships(relationshipRef, direction);

                if (replaceExisting)
                    relCollection.Clear();

                if (destinationRef != null)
                    relCollection.Add(destinationRef);

                origin.Save();
            });
        }
    }
}