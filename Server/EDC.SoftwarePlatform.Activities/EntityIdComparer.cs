// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    // Custom comparer for the Entity classes using just the Id
    public class EntityIdComparer : IEqualityComparer<Entity>, IEqualityComparer<IEntity>, IEqualityComparer<ActivityArgument>, IEqualityComparer<WfExpression>
    {
        static readonly EntityIdComparer _singleton = new EntityIdComparer();

        public static EntityIdComparer Singleton
        {
            get { return _singleton; }
        }
    

        // Products are equal if their names and product numbers are equal. 
        bool EqualIds(IEntity x, IEntity y)
        {
            if ((x == null && y != null) || (x != null && y == null))
                return false;
            else
                return x.Id == y.Id;
        }


        public bool Equals(Entity x, Entity y)
        {
            return EqualIds(x, y);
        }


        public bool Equals(IEntity x, IEntity y)
        {
            if (x == null || y == null)
                return false;

            return x.Id == y.Id;
        }



        public bool Equals(ActivityArgument x, ActivityArgument y)
        {
            return EqualIds(x, y);
        }




        public bool Equals(WfExpression x, WfExpression y)
        {
            return EqualIds(x, y);
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public int GetHashCode(Entity entity)
        {
            return entity.Id.GetHashCode();
        }

        public int GetHashCode(ActivityArgument entity)
        {
            return entity.Id.GetHashCode();
        }

        public int GetHashCode(WfExpression entity)
        {
            return entity.Id.GetHashCode();
        }

        public int GetHashCode(IEntity obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}