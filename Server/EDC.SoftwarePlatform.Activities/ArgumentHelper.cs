// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
//using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public static class ArgumentHelper
    {
        /// <summary>
        /// Create a variable to be used within a windows activity based upon a model variable.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static Variable CreateVariable(ActivityArgument argument)
        {
            Variable result; 

            var argumentTypes = argument.IsOfType.Select( t => t.Id );

            // This is ugly, but it works
            if (argumentTypes.Contains(StringArgument.StringArgument_Type.Id))
                result =  new Variable<string>();
            else
                if (argumentTypes.Contains(IntegerArgument.IntegerArgument_Type.Id))
                    result = new Variable<int>();
                else
                    if (argumentTypes.Contains(BoolArgument.BoolArgument_Type.Id))
                        result = new Variable<bool>();
                    else
                        if (argumentTypes.Contains(DecimalArgument.DecimalArgument_Type.Id))
                            result = new Variable<decimal>();
                        else
                            if (argumentTypes.Contains(CurrencyArgument.CurrencyArgument_Type.Id))
                                result = new Variable<decimal>();
                            else
                                if (argumentTypes.Contains(ResourceArgument.ResourceArgument_Type.Id))
                                    result = new Variable<EntityRef>();
                                else
                                    if (argumentTypes.Contains(ObjectArgument.ObjectArgument_Type.Id))
                                        result = new Variable<object>();
                                    else
                                        throw new ArgumentException(string.Format("Unsupported result type "));

            result.Name = argument.Name;

            //TODO: Add in default values

            return result;
        }
    }
}
