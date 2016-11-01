// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EDC.SoftwarePlatform.Install.Configure
{
    internal class CommandLineParser
    {
        private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <remarks></remarks>
        public CommandLineParser(string[] arguments)
        {
            Parse(arguments);
        }

        /// <summary>
        /// Gets the number of arguments that are available for parsing.
        /// </summary>
        /// <remarks></remarks>
        public int Count
        {
            get { return _arguments.Count; }
        }

        /// <summary>
        /// Containers the argument.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool ContainsArgument(string argumentName)
        {
            if (string.IsNullOrEmpty(argumentName))
            {
                throw new ArgumentNullException();
            }

            return _arguments.ContainsKey(argumentName.ToLower());
        }

        /// <summary>
        /// Values for argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public T ValueForArgument<T>(string argumentName)
        {
            if (string.IsNullOrEmpty(argumentName))
            {
                throw new ArgumentNullException();
            }

            if (!_arguments.Any(kvp => kvp.Key == argumentName.ToLower()))
            {
                throw new ArgumentOutOfRangeException();
            }
            string result = _arguments[argumentName.ToLower()];

            // Convert the value to the return type
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(result);
        }

        /// <summary>
        /// Parses the specified arguments are turns them into a dictionary.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <remarks></remarks>
        private void Parse(string[] arguments)
        {
            for (int i = 0; i < arguments.Count(); i++)
            {
                if (!arguments[i].StartsWith("-"))
                    continue;
                if ((i + 1 == arguments.Count()) || arguments[i + 1].StartsWith("-"))
                {
                    _arguments.Add(arguments[i].ToLower(), string.Empty);
                }
                else
                {
                    _arguments.Add(arguments[i].ToLower(), arguments[i + 1]);
                    i++;
                }
            }
        }
    }
}
