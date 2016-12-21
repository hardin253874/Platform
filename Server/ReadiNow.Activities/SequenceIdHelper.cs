// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text.RegularExpressions;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities
{
    public class SequenceIdGenerator
    {
        private static readonly Random SequenceRandGen = new Random();

        private const string SequenceFormat = "{0}:<{1}.{2}>";

        /// <summary>
        /// The prfix used by the sequence generator
        /// </summary>
        public string SequencePrefix { get; private set;  }



        public Regex SequenceRegex { get; private set; } 


        public SequenceIdGenerator(string _sequencePrefix)
        {
            SequencePrefix = _sequencePrefix;

            SequenceRegex = new Regex(".*(" + SequencePrefix + ":<(\\d+).(\\d+)>).*");

        }

        /// <summary>
        /// Generate a default sequence Id for including in a paused activity
        /// </summary>
        /// <returns></returns>
        public string Next()
        {
            // TODO: replace with crypto helper
            return String.Format(SequenceFormat, SequencePrefix, RequestContext.TenantId, SequenceRandGen.Next());
        }


        /// <summary>
        /// Split a sequenceId of the format "tenantId.relationshipId"
        /// </summary>
        public void SplitSequenceId(string sequenceId, out string sequenceString, out long tenantId, out long sequenceNumber)
        {

            var split = SequenceRegex.Match(sequenceId);
            if (split.Groups.Count != 4)
                throw new InvalidSequenceId(sequenceId);

            sequenceString = split.Groups[1].Value;

            if (!Int64.TryParse(split.Groups[2].Value, out tenantId))
                throw new InvalidSequenceId(sequenceId);

            if (!Int64.TryParse(split.Groups[3].Value, out sequenceNumber))
                throw new InvalidSequenceId(sequenceId);
        }

        public class InvalidSequenceId : ApplicationException
        {
            public InvalidSequenceId(string badSequenceId)
                : base(String.Format("The provided Sequence Id was not in a recognised format: '{0}'", badSequenceId))
            {
            }

            public InvalidSequenceId(string message, string badSequenceId)
                : base(String.Format("{0}: '{0}'", message, badSequenceId))
            {
            }
        }

    }
}