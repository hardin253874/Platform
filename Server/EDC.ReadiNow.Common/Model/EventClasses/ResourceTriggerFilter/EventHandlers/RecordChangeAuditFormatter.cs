// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Database;
using EDC.Database.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter.EventHandlers
{
    /// <summary>
    /// Class for formatting messages for resource audits
    /// </summary>
    internal class RecordChangeAuditFormatter
    {
        const string UnnamedText = "[Unnamed]";

        const int DefaultMaxRelationshipsNamesReported = 10;
        const int DefaultMaxRenderedFieldLength = 50;
        const int DefaultMaxDescriptionLength = 2000;

        List<ChangeRecord> _changes = new List<ChangeRecord>();
        string _entityName = null;
        string _userName = null;
        string _secondaryName = null;

        /// <summary>
        /// The number of changes that have been recorded
        /// </summary>
        public int Count {
            get
            {
                return _changes.Count;
            }
        }

        /// <summary>
        /// The maximum number of relationship names that are shown
        /// </summary>
        public int MaxRelationshipsNamesReported { get; set; }

        /// <summary>
        /// The maximum length of a rendered field value
        /// </summary>
        public int MaxRenderedFieldLength { get; set; }

        /// <summary>
        /// The total maximum length of a description field
        /// </summary>
        public int MaxDescriptionLength { get; set; }


        public RecordChangeAuditFormatter(string entityName, string userName, string secondaryName )
        {
            _entityName = entityName;
            _userName = userName;
            _secondaryName = secondaryName;

            MaxRelationshipsNamesReported = DefaultMaxRelationshipsNamesReported;
            MaxRenderedFieldLength = DefaultMaxRenderedFieldLength;
            MaxDescriptionLength = DefaultMaxDescriptionLength;
        }

        /// <summary>
        /// Add the name of the entity to the formatter
        /// </summary>
        /// <param name="name"></param>
        public void AddEntityName(string name)
        {
            _entityName = name;
        }

		/// <summary>
		/// Add the name of the entity to the formatter
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="secondaryName">The secondary name.</param>
		public void AddUserName(string name, string secondaryName)
        {
            _userName = name;
            _secondaryName = secondaryName;
        }

	    /// <summary>
	    /// Is the change a create.
	    /// </summary>
	    public bool IsCreate { get; set; }

        /// <summary>
        /// Is the change a delete.
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// Add a formatted message representing a change in a field.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void AddChangedField(Field field, object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
                throw new ArgumentException();

            var change = new ChangeRecord { Field = field.Name };
            _changes.Add(change);

            var fieldRnType = field.GetFieldType().ReadiNowType;
            var dbType = DatabaseTypeHelper.ConvertDatabaseTypeNameToDatabaseType(field.GetFieldType().ReadiNowType);

            Func<object, string> converter = (o) => FormatField(dbType.ConvertToString(o));

            if (oldValue == null)
                change.UpdateType = "set";
            else if (newValue == null)
                change.UpdateType = "cleared";
            else
                change.UpdateType = "changed";

            var sb = new StringBuilder();

            switch (field.GetFieldType().ReadiNowType)
            {
                case "BoolType":
                   change.UpdateType = "changed";                        // Booleans get special handling due to false and null being the same
                   AddTo(sb, converter, newValue ?? (object)false);      // special handling for nulls with bools.
                   break;

                case "CurrencyType":
                case "DecimalType":
                case "Int32Type":
                case "DateType":
                case "TimeType":
                case "GuidType":
                    AddFromTo(sb, converter, oldValue, newValue);
                    break;

                case "DateTimeType":
                    AddFromTo(sb, ZuluDateTimeConverter, oldValue, newValue);
                    break;


                case "StringType":
                    if (!SkipSummary(field))
                    {
                        AddFromTo(sb, converter, oldValue, newValue);
                    }
                    break;

                case "BinaryType":
                case "IdentifierType":
                case "DocumentType":
                case "XmlType":
                case "AutoIncrementType":
                // do nothing - values are not displayable.
                default:
                    break;
            }

            change.Summary = sb.ToString();
        }


        /// <summary>
        /// Add a formatted message representing a change in a lookup.
        /// </summary>
        public void AddChangedLookup(string relName, IEntity oldValue, IEntity newValue)
        {
            if (oldValue == null && newValue == null)
                throw new ArgumentException();

            var change = new ChangeRecord { Field = relName };
            _changes.Add(change);

            if (oldValue == null)
                change.UpdateType = "set";
            else if (newValue == null)
                change.UpdateType = "cleared";
            else
                change.UpdateType = "changed";

            var sb = new StringBuilder();

            AddEntityFromTo(sb, oldValue, newValue);

            change.Summary = sb.ToString();
        }


        /// <summary>
        /// Add a formatted message representing a change in a relationship.
        /// </summary>
        public void AddChangedRelationship(string relName, IEnumerable<long> removed, IEnumerable<long> added, int count)
        {
            var sb = new StringBuilder();

            var change = new ChangeRecord { Field = relName };
            _changes.Add(change);

            change.UpdateType = null;

            var overlapping = removed.Intersect(added);

            var reallyRemoved = removed.Except(overlapping);

            if (reallyRemoved.Any())
            {
                sb.Append("removed ");
                AddRelList(sb, reallyRemoved);
            }

            var reallyAdded = added.Except(overlapping);

            if (reallyAdded.Any())
            {
                if (reallyRemoved.Any())
                    sb.Append(' ');

                sb.Append("added ");
                AddRelList(sb, reallyAdded.Except(overlapping));
            }

            change.Summary = sb.ToString();
        }

        public void AddRelList(StringBuilder sb, IEnumerable<long> ids)
        {
            var names = Entity.Get<Resource>(ids.Take(MaxRelationshipsNamesReported), Resource.Name_Field).Select(r => FormatField(r.Name));

            sb.Append(String.Join(" | ", names));

            var notShown = ids.Count() - MaxRelationshipsNamesReported;

            if (notShown > 0)
                sb.AppendFormat(" plus {0} more", notShown);
        }

        /// <summary>
        /// Get the string that goes into the Name field on a log entry
        /// </summary>
        public string GetNameString()
        {

            var userName = (_userName ?? _secondaryName) ?? UnnamedText;
            var secondaryName = _secondaryName != null && _secondaryName != userName? "(" + _secondaryName + ")" : "";
            var entityName = _entityName ?? UnnamedText;

            return string.Format("{0}{1} {2} '{3}'", userName, secondaryName,  IsCreate ? "created" : IsDelete ? "deleted" : "updated", entityName);
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            if (IsDelete)
            {
                sb.Append("deleted");
            }
            else
            {
                bool isFirst = true;

                foreach (var change in _changes.OrderBy(c => c.Field))
                {
                    if (!isFirst)
                        sb.AppendLine();

                    sb.AppendFormat("[{0}]", change.Field);

                    if (change.UpdateType != null)
                        sb.AppendFormat(" {0}", change.UpdateType);

                    sb.AppendFormat(" {0}", change.Summary);

                    isFirst = false;
                }
            }
            return SquashString(sb.ToString(), MaxDescriptionLength);
        }

        void AddFromTo(StringBuilder sb, Func<object, string> converter, object oldValue, object newValue)
        {
            if (oldValue == null)
                sb.AppendFormat("to {0}", converter(newValue));
            else if (newValue == null)
                sb.AppendFormat("from {0}", converter(oldValue));
            else
                sb.AppendFormat("from {0} -> {1}", converter(oldValue), converter(newValue));
        }

        void AddTo(StringBuilder sb, Func<object, string> converter, object newValue)
        {
            sb.AppendFormat("to {0}", converter(newValue));
        }

        void AddEntityFromTo(StringBuilder sb, IEntity oldValue, IEntity newValue)
        {
            if (oldValue == null)
                sb.AppendFormat("to {0}", FormatField(ToName(newValue)));
            else if (newValue == null)
                sb.AppendFormat("from {0}", FormatField(ToName(oldValue)));
            else
                sb.AppendFormat("from {0} -> {1}", FormatField(ToName(oldValue)), FormatField(ToName(newValue)));
        }

        string ToName(IEntity e)
        {
            return e != null ? Entity.GetName(e.Id) : e.Id.ToString();
        }

        void AddChange(string field, string updateType, string summary)
        {
            _changes.Add(new ChangeRecord { Field = field, UpdateType = updateType, Summary = summary });
        }

        /// <summary>
        /// Skip the summary information for this field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        bool SkipSummary(Field field)
        {
            var stringField = field.As<StringField>();
            return stringField != null && stringField.FieldRepresents_Enum == FieldRepresentsEnum_Enumeration.FieldRepresentsPassword;
        }

        string FormatField(string s)
        {
            return "'" + SquashString(s, MaxRenderedFieldLength) + "'";
        }

        string SquashString(string s, int maxLength)
        {
            if (s == null)
                return null; 

            var trimmed = s.Length > maxLength ? s.Substring(0, maxLength - 3) + "..." : s;
            return trimmed;
        }

        string ZuluDateTimeConverter(object o)
        {
            var type = new DateTimeType();
            return FormatField(type.ConvertToString(o, DateTimeType.ModifiedISODateTimeFormat, true));    // treat dateTimes without a Kin specified as UTC
        }

        private class ChangeRecord
        {
            public string Field;
            public string UpdateType;
            public string Summary;
        } 

    }
}
