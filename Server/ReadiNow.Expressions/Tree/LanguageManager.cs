// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using EDC.Core;
using EDC.Database;
using EDC.Xml;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Holds a database of elements that are available in the language.
    /// </summary>
    [XmlRoot("Language", Namespace = Constants.ExpressionsNamespace)]
    public class LanguageManager
    {
        public LanguageManager()
        {
            Operators = new List<Operator>();
        }


        /// <summary>
        /// Collection of registered operators.
        /// </summary>
        public List<Operator> Operators { get; set; }


        /// <summary>
        /// Collection of registered functions.
        /// </summary>
        [XmlArrayItem("Function")]
        public List<Function> Functions { get; set; }


        /// <summary>
        /// Collection of registered possible casts.
        /// </summary>
        [XmlArrayItem("Cast")]
        public List<CastInfo> Casts { get; set; }


        /// <summary>
        /// Collection of registered functions.
        /// </summary>
        [XmlIgnore]
        public ILookup<string, Function> FunctionsByToken { get; set; }


        #region Loader
        
        /// <summary>
        /// Thread-safe accessor to singleton of language database.
        /// </summary>
        public static LanguageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(LanguageManager))
                    {
                        if (_instance == null)
                        {
                            _instance = LoadDatabase();
                        }
                    }
                }
                return _instance;
            }
        }
        private static volatile LanguageManager _instance;


        /// <summary>
        /// Loads the language database from Language.xml.
        /// </summary>
        private static LanguageManager LoadDatabase()
        {
            LanguageManager result;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ReadiNow.Expressions.Tree.Language.xml"))
            {
                result = Serializer<LanguageManager>.FromXml(stream);
            }

            ExpandTypeGroups(result.Operators);
            ExpandTypeGroups(result.Functions);
            
            result.FunctionsByToken = result.Functions.Concat(result.Operators).ToLookup(f => f.Token);
            return result;
        }


        /// <summary>
        /// For convenience, many operators specify type groups instead of types. Expand them out to multiple operators at runtime.
        /// </summary>
        private static void ExpandTypeGroups<T>(List<T> functions) where T : Function
        {
            // ReSharper disable PossibleMultipleEnumeration

            var numeric = new[] { DataType.Int32, DataType.Decimal, DataType.Currency };
            var dateOrTime = new[] { DataType.Date, DataType.Time, DataType.DateTime };
            var comparable = numeric.Union(dateOrTime).Union(new[] { DataType.String, DataType.Entity  });
            var equatable = comparable.Union(new[] { DataType.Bool, DataType.Guid });
            var all = Enum.GetValues(typeof(DataType)).Cast<DataType>();
            var allExceptBool = all.Where(dt => dt != DataType.Bool).ToList();
            var allExceptEntity = all.Where(dt => dt != DataType.Entity).ToList();

            List<T> toAdd = new List<T>();
            HashSet<T> toRemove = new HashSet<T>();

            foreach (T fn in functions)
            {
                if (fn == null)
                    continue;
                TypeGroup typeGroup = fn.ResultType.TypeGroup;
                if (typeGroup == TypeGroup.None && fn.ParameterTypes != null)
                    typeGroup = fn.ParameterTypes.Select(pt => pt.TypeGroup).FirstOrDefault(tg => tg != TypeGroup.None && tg != TypeGroup.Custom);

                if (typeGroup != TypeGroup.None && typeGroup != TypeGroup.Custom)
                {
                    IEnumerable<DataType> types = null;
                    switch (typeGroup)
                    {
                        case TypeGroup.Numeric: types = numeric; break;
                        case TypeGroup.Comparable: types = comparable; break;
                        case TypeGroup.Equatable: types = equatable; break;
                        case TypeGroup.DateOrTime: types = dateOrTime; break;
                        case TypeGroup.AllExceptBool: types = allExceptBool; break;
                        case TypeGroup.AllExceptEntity: types = allExceptEntity; break;
                        case TypeGroup.All: types = all; break;
                        default:
                            throw new Exception( "Unknown typeGroup" );
                    }

                    foreach (DataType type in types)
                    {
                        T newOp = (T)fn.Clone();
                        if (newOp.ResultType.TypeGroup != TypeGroup.None)
                        {
                            newOp.ResultType.Type = type;
                        }
                        if (newOp.InputType != null && newOp.InputType.TypeGroup != TypeGroup.None)
                        {
                            newOp.InputType.Type = type;
                        }
                        foreach (LanguageExprType param in newOp.ParameterTypes)
                        {
                            if (param.TypeGroup != TypeGroup.None)
                            {
                                param.Type = type;
                            }
                        }
                        toAdd.Add(newOp);
                    }
                    toRemove.Add(fn);
                }
            }
            functions.RemoveAll(toRemove.Contains);
            functions.AddRange(toAdd);
        }
        #endregion

        #region Helpers
        internal DataType GetCastTypeByName(string typeTokenName)
        {
            var casts = Casts.Where(ci => ci.Token == typeTokenName);
            var castTo = casts.Select(ci => ci.ToType).FirstOrDefault();
            return castTo;
        }
        #endregion
    }
}
