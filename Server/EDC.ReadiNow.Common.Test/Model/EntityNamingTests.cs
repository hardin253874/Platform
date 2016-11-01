// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
    /// <summary>
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class EntityNamingTests
    {        
        /// <summary>
        ///     Capilitalisation Style Enumeration
        /// </summary>
        public enum CapitalisationStyle
        {
            TitleCase,
            SentenceCase
        }


        // The list of words that must be lower case, regardless of the casing style.
        // These words must be lower case.
        private readonly HashSet<string> _smallWords = new HashSet<string>
        {
            "a",
            "an",
            "and",
            "at",
            "but",
            "by",
            "in",
            "for",
            "nor",
            "of",
            "off",
            "on",
            "or",
            "so",
            "the",
            "to",
            "up",
            "yet",
            "from"
        };


        // The list of words that must be written as is.        
        private readonly List<string> _asIsWords = new List<string>
        {
            "iCal",
            "iPad",
            "UI",
            "XML",
            "URL",
            "SSL",
            "HTML",
            "GUID",
            "Id",
            "OpenId",
            "MIME",
            "MB",
            "ARGB",
            "HTML5",
            "SID"
        };


        // The list of names that must be written as is.
        // The names are per capitilisation style.
        private readonly Dictionary<CapitalisationStyle, List<string>> _asIsNames = new Dictionary<CapitalisationStyle, List<string>>
        {
            {CapitalisationStyle.TitleCase, new List<string> {"Schedule One Off", "One Off Schedule", "Triggered On"}}            
        };


        /// <summary>
        ///     The product installation date.
        /// </summary>
        private DateTime _productInstallationDate;


        /// <summary>
        ///     Returns a bool indicating whether the specified value has the correct capitilisation.
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="capitalisationStyle"></param>
        /// <returns></returns>
        private bool DoesFieldHaveHaveCorrectCapitalisation(string fieldValue, CapitalisationStyle capitalisationStyle)
        {
            List<string> asIsNames;

            if (string.IsNullOrEmpty(fieldValue))
            {
                return true;
            }

            if (_asIsNames.TryGetValue(capitalisationStyle, out asIsNames))
            {
                string asIsName = asIsNames.FirstOrDefault(s => String.Equals(s, fieldValue, StringComparison.InvariantCultureIgnoreCase));
                if (asIsName != null)
                {
                    return asIsName == fieldValue;
                }
            }

            string[] words = Regex.Split(fieldValue, @"\W+");
            bool isValid = true;

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (string.IsNullOrEmpty(word))
                {
                    continue;
                }

                string asIsWord = _asIsWords.FirstOrDefault(s => String.Equals(s, word, StringComparison.InvariantCultureIgnoreCase));

                if (asIsWord != null)
                {
                    // Ignore as is words that match
                    if (asIsWord == word)
                    {
                        continue;
                    }
                    isValid = false;
                    break;
                }

                string smallWord = _smallWords.FirstOrDefault(s => String.Equals(s, word, StringComparison.InvariantCultureIgnoreCase));

                if (i != 0 && smallWord != null)
                {
                    // The word is a small word
                    // It must be lowercase
                    if (word == word.ToLowerInvariant()) continue;
                    isValid = false;
                    break;
                }

                if ((i == 0 && capitalisationStyle == CapitalisationStyle.SentenceCase) || capitalisationStyle == CapitalisationStyle.TitleCase)
                {
                    // If first word of sentence case or title case then word must have first letter capitalised
                    if (word == word.Substring(0, 1).ToUpperInvariant() + word.Substring(1)) continue;
                    isValid = false;
                    break;
                }

                // If we are here the word is not the first word of a sentence case
                // which means it must be lower case.
                if (word == word.ToLowerInvariant()) continue;
                isValid = false;
                break;
            }

            return isValid;
        }

        

        /// <summary>
        ///     Setup the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public void Initialise()
        {
            // Get the product installation date
            _productInstallationDate = TestHelpers.GetReadiNowProductInstallationDate();
        }


        [Test]
        [RunAsDefaultTenant]
        [TestCase("core:definition", "core:name", CapitalisationStyle.TitleCase, false)]
        [TestCase("core:enumType", "core:name", CapitalisationStyle.TitleCase, true)]
        [TestCase("core:type", "core:name", CapitalisationStyle.TitleCase, false)]
        [TestCase("core:field", "core:name", CapitalisationStyle.SentenceCase, true)]
        [TestCase("core:relationship", "core:name", CapitalisationStyle.SentenceCase, true)]
        [TestCase("core:relationship", "core:toName,core:fromName", CapitalisationStyle.SentenceCase, true)]
        [TestCase("console:customEditForm", "core:name", CapitalisationStyle.TitleCase, false)]
        [TestCase("console:fieldControlOnForm", "core:name", CapitalisationStyle.SentenceCase, false)]
        [TestCase("console:relationshipControlOnForm", "core:name", CapitalisationStyle.SentenceCase, false)]
        [TestCase("console:verticalStackContainerControl", "core:name", CapitalisationStyle.TitleCase, false)]
        [TestCase("core:report", "core:name", CapitalisationStyle.TitleCase, true)]
        [TestCase("console:navSection", "core:name", CapitalisationStyle.TitleCase, true)]
        [TestCase("console:topMenu", "core:name", CapitalisationStyle.TitleCase, false)]
        [TestCase("console:actionMenuItem", "core:name", CapitalisationStyle.TitleCase, false)]
        // See discussion at: http://spwiki.sp.local/display/QA/2014/09/17/Capitalization+of+report+columns
        //[TestCase("core:reportColumn", "core:name", CapitalisationStyle.SentenceCase, true)]
        //[TestCase("core:reportCondition", "core:name", CapitalisationStyle.SentenceCase, true)]        
        public void EnsureEntityFieldNaming(string typeAlias, string fieldIds, CapitalisationStyle capitalisationStyle, bool includeDerivedTypes)
        {
            // Increate the fanout limit during the test.
            var originalMax = ReadiNow.EntityRequests.EntityDataBuilder<EDC.ReadiNow.Model.IEntity>.MaxRelatedEntities;
            ReadiNow.EntityRequests.EntityDataBuilder<long>.MaxRelatedEntities = 1000;

            var errors = new StringBuilder();

            try
            {
                string[] fields = fieldIds.Split(',');

                IEntityRef[] fieldEntityRefs = fields.Select(f => new EntityRef(f) as IEntityRef).Union(new[] { new EntityRef("core:createdDate") as IEntityRef }).ToArray();

                IEnumerable<Resource> entities = Entity.GetInstancesOfType(new EntityRef(typeAlias), includeDerivedTypes, fieldIds).Where(r => r != null).Select(r => r.As<Resource>());

                foreach (Resource entity in entities)
                {
                    if (!TestHelpers.InValidatableSolution(entity.InSolution))
                    {
                        // Only check the specified solutions
                        continue;
                    }

                    // Skip entities whose created date is after the installation date
                    DateTime? entityCreatedDate = entity.CreatedDate;
                    if (entityCreatedDate != null &&
                        entityCreatedDate.Value.ToLocalTime() > _productInstallationDate)
                    {
                        continue;
                    }

                    foreach (string field in fields)
                    {
                        var fieldValue = entity.GetField<string>(new EntityRef(field.Trim()));

                        if (fieldValue != null &&
                            fieldValue.Contains("&"))
                        {
                            errors.AppendFormat("Entity id:{0} alias:{1} field:{2} is invalid. Replace '&' with 'and'. Value:{3}.", entity.Id, entity.Alias, field, fieldValue);
                            errors.AppendLine();
                            continue;
                        }

                        if (DoesFieldHaveHaveCorrectCapitalisation(fieldValue, capitalisationStyle)) continue;

                        errors.AppendFormat("Entity id:{0} alias:{1} field:{2} is invalid. Expected to be:{3}. Value:{4}.", entity.Id, entity.Alias, field, capitalisationStyle, fieldValue);
                        errors.AppendLine();
                    }
                }
            }
            finally
            {
                ReadiNow.EntityRequests.EntityDataBuilder<long>.MaxRelatedEntities = originalMax;
            }

            Assert.IsNullOrEmpty(errors.ToString());
        }
    }
}