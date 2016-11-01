// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics.Response;
using EDC.ReadiNow.Resources;
using Microsoft.CSharp;
using Microsoft.Win32;
using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics.Request
{
    /// <summary>
    ///     Remote request.
    /// </summary>
    [ProtoContract]
    public class RemoteExecRequest : DiagnosticRequest
    {
        /// <summary>
        ///     Cache of scripts to exec methods.
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> MethodsCache = new ConcurrentDictionary<string, MethodInfo>();


        /// <summary>
        ///     The code to execute.
        /// </summary>
        [ProtoMember(1)]
        public string Code { get; set; }


        /// <summary>
        ///     The id of the request.
        /// </summary>
        [ProtoMember(2)]
        public string Id { get; set; }


        /// <summary>
        ///     The name of the target.
        /// </summary>
        [ProtoMember(3)]
        public string Target { get; set; }


        /// <summary>
        ///     Return true if we can execute the remote code, false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool CanExecute()
        {
            if (!string.IsNullOrWhiteSpace(Target) &&
                string.Compare(Target, Dns.GetHostEntry(Dns.GetHostName()).HostName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

	        return ConfigurationSettings.GetServerConfigurationSection( ).RemoteExec.Enabled;
        }


        /// <summary>
        ///     Compile the code and get the execute method.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private MethodInfo CompileAndGetExecMethod(string code)
        {
            using (var codeProvider = new CSharpCodeProvider())
            {
                var parameters = new CompilerParameters {GenerateInMemory = true};

                parameters.ReferencedAssemblies.AddRange(GetReferencedAssemblies(code).ToArray());

                CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, Code);

                if (results.Errors.HasErrors)
                {
                    var stringBuilder = new StringBuilder();

                    foreach (CompilerError error in results.Errors)
                    {
                        stringBuilder.AppendLine(String.Format("Error ({0}): Line: {1} Column: {2} {3}", error.ErrorNumber, error.Line, error.Column, error.ErrorText));
                    }

                    throw new InvalidOperationException(stringBuilder.ToString());
                }

                MethodInfo mi = results.CompiledAssembly.GetType("RemoteExecutor").GetMethod("Execute", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);
                return mi;
            }
        }


        /// <summary>
        ///     Get referenced assemblies
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private IEnumerable<string> GetReferencedAssemblies(string code)
        {
            var referencedAssemblies = new HashSet<string> {Assembly.GetExecutingAssembly().Location};

            // Get referenced assemblies 
            foreach (AssemblyName ra in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                try
                {
                    referencedAssemblies.Add(Assembly.Load(ra).Location);
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                    // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            var regex = new Regex("^//ref:\\s+(?<file>.*)", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(code);

            foreach (Match match in matches)
            {
                Match m = match;

                if (m.Groups["file"].Success &&
                    !string.IsNullOrWhiteSpace(m.Groups["file"].Value.Trim()))
                {
                    referencedAssemblies.Add(m.Groups["file"].Value.Trim());
                }
            }

            return referencedAssemblies;
        }


        /// <summary>
        ///     Gets the response.
        /// </summary>
        /// <returns></returns>
        public override DiagnosticResponse GetResponse()
        {
            if (!CanExecute())
            {
                return null;
            }

            try
            {
                MethodInfo execMethod = MethodsCache.GetOrAdd(Code, CompileAndGetExecMethod);
                var data = execMethod.Invoke(null, null) as List<Tuple<string, string>>;
                return new RemoteExecResponse {Data = data, Id = Id};
            }
            catch (Exception exc)
            {
                var errors = new List<Tuple<string, string>> {new Tuple<string, string>("Error", exc.Message)};
                return new RemoteExecResponse {Data = errors, Id = Id};
            }
        }
    }
}