// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace ReadiNow.Reporting.Helpers
{
    internal class DateTimeFormatHelper
    {
        internal static string DateTimeFormatFromEnumeration(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string[] enumValueParts = value.Split(':');
            return enumValueParts.Length == 2 ? enumValueParts[1] : enumValueParts[0];

        }

        public static Dictionary<string, string> DisplayNamesForType(string type)
        {
            switch (type)
            {
                case "Date":
                    return EnumerationToList(Entity.GetInstancesOfType<DateColFmtEnum>().OrderBy(e => e.EnumOrder).ToList());
                case "DateTime":
                    return EnumerationToList(Entity.GetInstancesOfType<DateTimeColFmtEnum>().OrderBy(e => e.EnumOrder).ToList());
                case "Time":
                    return EnumerationToList(Entity.GetInstancesOfType<TimeColFmtEnum>().OrderBy(e => e.EnumOrder).ToList());
            }
            return null;
        }

        private static Dictionary<string, string> EnumerationToList(List<DateColFmtEnum> toList)
        {
            Dictionary<string, string> typePairs = new Dictionary<string, string>(toList.Count());
            foreach (DateColFmtEnum dateColFmtEnum in toList)
            {
                typePairs[DateTimeFormatFromEnumeration(dateColFmtEnum.Alias)] = dateColFmtEnum.Name;
            }
            return typePairs;
        }

        private static Dictionary<string, string> EnumerationToList(List<DateTimeColFmtEnum> toList)
        {
            Dictionary<string, string> typePairs = new Dictionary<string, string>(toList.Count());
            foreach (DateTimeColFmtEnum dateTimeColFmtEnum in toList)
            {
                typePairs[DateTimeFormatFromEnumeration(dateTimeColFmtEnum.Alias)] = dateTimeColFmtEnum.Name;
            }
            return typePairs;
        }

        private static Dictionary<string, string> EnumerationToList(List<TimeColFmtEnum> toList)
        {
            Dictionary<string, string> typePairs = new Dictionary<string, string>(toList.Count());
            foreach (TimeColFmtEnum timeColFmtEnum in toList)
            {
                typePairs[DateTimeFormatFromEnumeration(timeColFmtEnum.Alias)] = timeColFmtEnum.Name;
            }
            return typePairs;
        }

    }
}
