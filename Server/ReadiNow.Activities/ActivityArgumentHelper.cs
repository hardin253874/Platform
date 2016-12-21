// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database.Types;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public static class ActivityArgumentHelper
    {
        
        

        /// <summary>
        /// Get the type of an argument.
        /// </summary>
        public static EntityType GetArgumentType(this ActivityArgument argument)
        {
            var argumentType = new List<EntityType>
            {
                StringArgument.StringArgument_Type,
                IntegerArgument.IntegerArgument_Type,
                BoolArgument.BoolArgument_Type,
                DecimalArgument.DecimalArgument_Type,
                CurrencyArgument.CurrencyArgument_Type,
                ResourceArgument.ResourceArgument_Type,
                DateTimeArgument.DateTimeArgument_Type,
                DateArgument.DateArgument_Type,
                TimeArgument.TimeArgument_Type,
                ObjectArgument.ObjectArgument_Type,
                ResourceListArgument.ResourceListArgument_Type,
                GuidArgument.GuidArgument_Type
            };


            EntityType result = null;

            var resultTypes = argument.IsOfType.Intersect(argumentType, new EntityIdComparer()).ToList();

            if (resultTypes.Count == 0)
                throw new ArgumentException("Was asked to evaluate an expression with an argument that can not be evaluated. This should never occur.");

            if (resultTypes.Count > 1)
                throw new ArgumentException("Was asked to evaluate and expression with more than one argument type. This should never occur.");

            result = resultTypes[0];

            return result;
        }


        public static FieldType GetFieldType(this ActivityArgument arg)
        {
            var type = arg.IsOfType.FirstOrDefault().As<ArgumentType>();

            return type == null ? null : type.EquivFieldType;
        }


        public static WfArgumentInstance GetArgInstance(this ActivityArgument targetArg,  WfActivity activity)
        {
            WfArgumentInstance targetArgInst = null;
            var key = new Tuple<long, long>(activity.Id, targetArg.Id);

          
            targetArgInst =
                activity.ArgumentInstanceFromActivity.First(ai => ai.ArgumentInstanceArgument.Id == targetArg.Id);  

            //var targetArgInst =
            //    targetArg.ArgumentInstanceFromArgument.First(ai => ai.ArgumentInstanceActivity.Id == activity.Id);        // this is probably less efficient



            return targetArgInst;
        }

        /// <summary>Given an arg inst and a value create and activityArgument ready for storage.</summary>
        /// <param name="activity">The <see cref="WfActivity"/> being run. This cannot be null.</param>
        /// <param name="arg">The argument for the activity. This cannot be null.</param>
        /// <param name="value">The value of the argument. Its type is determined by <paramref name="arg"/>. This may be null for certain argument types.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="ArgumentNullException">Neither <paramref name="activity"/> nor <paramref name="arg"/> can be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="arg"/> must be of a supported type.</exception>
        public static ActivityArgument ConvertArgInstValue(WfActivity activity, ActivityArgument arg, object value)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            ActivityArgument result;

            string name = $"{activity.Name ?? "[Unnamed]"}.{arg.Name ?? "[Unnamed]"}";

            if (arg.Is<ResourceListArgument>())
            {
                var list = (IEnumerable<IEntity>)value;
                var created = new ResourceListArgument { Name = name };
				if (list != null)
                {
					var cleanList = list.Where(e => e != null).Select(e => e.As<Resource>());
					created.ResourceListParameterValues.AddRange(cleanList);
				}
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<ResourceArgument>())
            {
                var res = (IEntity)value;
                var created = new ResourceArgument { Name = name, ResourceParameterValue = res?.As<Resource>()};
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<StringArgument>())
            {
                var created = new StringArgument { Name = name, StringParameterValue = (string)value };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<BoolArgument>())
            {
                var created = new BoolArgument { Name = name, BoolParameterValue = (bool?) value ?? false };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<IntegerArgument>())
            {
                var created = new IntegerArgument { Name = name, IntParameterValue = (int?) value ?? 0 };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<DecimalArgument>())
            {
                var created = new DecimalArgument { Name = name, DecimalParameterValue = (decimal?) value ?? 0 };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<CurrencyArgument>())
            {
                var created = new CurrencyArgument { Name = name, DecimalParameterValue = (decimal?) value ?? 0 };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<DateTimeArgument>())
            {
                var created = new DateTimeArgument { Name = name, DateTimeParameterValue = (DateTime?) value };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<DateArgument>())
            {
                var created = new DateArgument { Name = name, DateParameterValue = (DateTime?) value };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<TimeArgument>())
            {
                // ensure that timeparametervalue only ever holds a datetime
                var dt = value is TimeSpan ? TimeType.NewTime((TimeSpan)value) : (DateTime?) value;
                
                var created = new TimeArgument { Name = name, TimeParameterValue = dt };
                result = created.As<ActivityArgument>();
            }
            else if (arg.Is<GuidArgument>())
            {
                var created = new GuidArgument { Name = name, GuidParameterValue = (Guid?) value ?? Guid.Empty };
                result = created.As<ActivityArgument>();
            }
            else
            {
                throw new ArgumentException($"Unsupported ActivityArgument '{arg.IsOfType.First().Name}' in '{name}'", "arg");
            }

            return result;
        }

        /// <summary>
        /// Given a argument holding a parameter, return the value
        /// </summary>
        /// <param name="argInst"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetArgParameterValue(ActivityArgument arg)
        {
            var rla = arg.As<ResourceListArgument>();
            if (rla != null)
            {
                return rla.ResourceListParameterValues;
            }

            var ra = arg.As<ResourceArgument>();
            if (ra != null)
            {
                return ra.ResourceParameterValue;
            }

            var sa = arg.As<StringArgument>();
            if (sa != null)
            {
                return sa.StringParameterValue;
            }

            var ba = arg.As<BoolArgument>();
            if (ba != null)
            {
                return ba.BoolParameterValue;
            }

            var ia = arg.As<IntegerArgument>();
            if (ia != null)
            {
                return ia.IntParameterValue;
            }

            var deca = arg.As<DecimalArgument>();
            if (deca != null)
            {
                return deca.DecimalParameterValue;
            }

            var ca = arg.As<CurrencyArgument>();
            if (ca != null)
            {
                return ca.DecimalParameterValue;
            }

            var dta = arg.As<DateTimeArgument>();
            if (dta != null)
            {
                return dta.DateTimeParameterValue;
            }

            var da = arg.As<DateArgument>();
            if (da != null)
            {
                return da.DateParameterValue;
            }

            var ta = arg.As<TimeArgument>();
            if (ta != null)
            {
                return ta.TimeParameterValue;
            }

            var ga = arg.As<GuidArgument>();
            if (ga != null)
            {
                return ga.GuidParameterValue;
            }
     
            throw new ArgumentException("Attempted to extract a value from an ActivityArgument for storage of a type that can not be stored: ", arg.IsOfType.First().Name);
        }
    }
}
