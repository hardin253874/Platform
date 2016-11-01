using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace UpdateCopyright
{
    /// <summary>
    /// 
    /// </summary>
    public static class Program
    {
        //private static CopyrightConfig config = new CopyrightConfig
        //{
        //    Directives = new List<CopyrightDirective>
        //    {
        //        new CopyrightDirective
        //        {
        //            //Path = "..\\..\\..\\..\\", // Server
        //            Path = @"C:\Development\Trunk-Server",
        //            ExcludeSubPaths =
        //                {
        //                    "UpdateCopyright",
        //                    "bin",
        //                    "obj",
        //                    "packages",
        //                    "Dependencies"
        //                },
        //            ExcludeFiles = {"AssemblyInfo.cs", "DatabaseUpgrade.xml"}
        //        },
        //        new CopyrightDirective
        //        {
        //            Path = @"C:\Development\Trunk-Client",
        //            ExcludeSubPaths =
        //                {
        //                    "client\\build",
        //                    "client\\dist",
        //                    "client\\doc",
        //                    "client\\node_modules",
        //                    "client\\tests",
        //                    "client\\bin",
        //                    "client\\obj",
        //                    "client\\lib",
        //                    "client\\karma"
        //                },
        //            ExcludeFiles = {"manifest.tpl.appcache", "AssemblyInfo.cs"}
        //        }
        //    },
        //    OldCopyrightStartsWith = @"Copyright 2011-",
        //    OldCopyrightEndsWith = @"Enterprise Data Corporation.",
        //    CopyrightNoticeToApply = @"Copyright 2011-{0:yyyy} Global Software Innovation Pty Ltd"
        //};

        private static CopyrightConfig config = new CopyrightConfig();
        private static Dictionary<string, CopyrightInfo> info = new Dictionary<string, CopyrightInfo>
        {
            { "C# code", new CopyrightInfo { Extension = "*.cs", StartComment = @"//" }},
            { "Js code", new CopyrightInfo { Extension = "*.js", StartComment = @"//" }},
            { "Style sheet (less)", new CopyrightInfo { Extension = "*.less", StartComment = @"//" }},
            { "Style sheet", new CopyrightInfo { Extension = "*.css", StartComment = @"/*", EndComment = @"*/" }},
            { "Web page", new CopyrightInfo { Extension = "*.html", StartComment = @"<!--//", EndComment = @"//-->" }},
            { "XML", new CopyrightInfo { Extension = "*.xml", StartComment = @"<!--", EndComment = @"-->"}},
            { "XAML", new CopyrightInfo { Extension = "*.xaml", StartComment = @"<!--", EndComment = @"-->"}},
            { "SQL", new CopyrightInfo { Extension = "*.sql", StartComment = @"--", EndComment = @""}}
        };

        private static int countNew;
        private static int countUpdated;

        /// <summary>
        /// The main program.
        /// </summary>
        /// <param name="args">The command line args.</param>
        static void Main(string[] args)
        {
            try
            {
                //var serializer = new DataContractSerializer(typeof(CopyrightConfig));
                //using (var x = XmlWriter.Create("UpdateCopyright.cfg", new XmlWriterSettings { Indent = true }))
                //{
                //    serializer.WriteObject(x, config);
                //}
                //return;

                Console.Out.WriteLine("Loading config...");

                LoadFromConfig();

                Console.Out.WriteLine("Copyrighting...");

                countNew = 0;
                countUpdated = 0;
                foreach (var d in config.Directives)
                {
                    var dir = new DirectoryInfo(d.Path);
                    Console.Out.WriteLine("> {0}", dir.FullName);

                    ProcessDirective(d, dir);
                }

                Console.Out.WriteLine("Done!");
                Console.Out.WriteLine("");
                Console.Out.WriteLine("{0} new files copyrighted.", countNew);
                Console.Out.WriteLine("{0} copyrighted files updated.", countUpdated);
            }
            catch (Exception err)
            {
                Console.Out.WriteLine(err.ToString());
            }

            Console.Read();
        }

        /// <summary>
        /// Load in the config from file.
        /// </summary>
        static void LoadFromConfig()
        {
            if (File.Exists("UpdateCopyright.cfg"))
            {
                using (var reader = new FileStream("UpdateCopyright.cfg", FileMode.Open, FileAccess.Read))
                {
                    var ds = new DataContractSerializer(typeof(CopyrightConfig));
                    config = (CopyrightConfig)ds.ReadObject(reader);
                }
            }
        }

        /// <summary>
        /// Returns true if there is an existing copyright notice on this line that needs updating.
        /// </summary>
        /// <param name="line">The line from the file.</param>
        /// <param name="ci">The copyright info to apply.</param>
        /// <returns>True for existing copyright. False if not detected.</returns>
        static bool CheckForExistingCopyright(string line, CopyrightInfo ci)
        {
            var s = line.IndexOf(config.OldCopyrightStartsWith, StringComparison.InvariantCulture);
            var e = line.IndexOf(config.OldCopyrightEndsWith, StringComparison.InvariantCulture);
            if (s >= 0 && e >= 0 && e > s)
            {
                var trimmed = line.Trim();
                return !string.IsNullOrEmpty(trimmed) && trimmed.StartsWith(ci.StartComment) && (string.IsNullOrEmpty(ci.EndComment) || trimmed.EndsWith(ci.EndComment));
            }
            return false;
        }

        /// <summary>
        /// Updates the counts of how many notices have been inserted or updated.
        /// </summary>
        /// <param name="ours">A flag indicating if a notice detected was ours to update.</param>
        /// <param name="filename">The filename where the action occurred.</param>
        static void UpdateCounts(bool ours, string filename)
        {
            if (ours)
            {
                Console.Out.WriteLine(filename);
                countUpdated++;
            }
            else
            {
                countNew++;
            }
        }

        /// <summary>
        /// The main method used for applying copyright instructions to a directory and its subdirectories.
        /// </summary>
        /// <param name="directive">The copyright instructions and information.</param>
        /// <param name="directory">The directory.</param>
        static void ProcessDirective(CopyrightDirective directive, DirectoryInfo directory)
        {
            var notice = string.Format(config.CopyrightNoticeToApply, DateTime.Today);

            foreach (var i in info)
            {
                var c = i.Value;
                var files = directory.GetFiles(c.Extension);
                foreach (var file in files)
                {
                    //if (file.FullName.ToLowerInvariant().Contains("sptreeviewitem"))
                    //{
                    //    var p = "break";
                    //}

                    var skip = false;
                    foreach (var excludefile in directive.ExcludeFiles)
                    {
                        if (file.FullName.ToLowerInvariant().Contains(excludefile.ToLowerInvariant()))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    var fileContents = new StringBuilder("");
                    var copyrightExists = false;
                    var ourCopyrightExists = false;
                    var isGeneratedCode = false;

                    using (var fs = File.OpenRead(file.FullName))
                    {
                        using (var reader = new StreamReader(fs))
                        {
                            // Check the first ten lines for existing stuff
                            try
                            {
                                for (var l = 0; l < 10; l++)
                                {
                                    var text = reader.ReadLine();

                                    if (text != null)
                                    {
                                        if (text.ToLowerInvariant().Contains("copyright") ||
                                            text.ToLowerInvariant().Contains("(c)") ||
                                            text.ToLowerInvariant().Contains("license"))
                                        {
                                            copyrightExists = true;
                                        }

                                        if (text.ToLower().Contains(" generated "))
                                        {
                                            isGeneratedCode = true;
                                        }

                                        if (CheckForExistingCopyright(text, c))
                                        {
                                            ourCopyrightExists = true;
                                        }
                                        else
                                        {
                                            fileContents.AppendLine(text);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                fileContents.Append(reader.ReadToEnd());
                                reader.Close();
                            }
                        }
                    }

                    if ((!copyrightExists || ourCopyrightExists) && !isGeneratedCode)
                    {
                        UpdateCounts(ourCopyrightExists, file.FullName);

                        // Clear the file contents
                        File.WriteAllText(file.FullName, "");

                        using (var fs = File.OpenWrite(file.FullName))
                        {
                            // Special handling for XML files
                            if (c.Extension == "*.xml")
                            {
                                ProcessXml(fs, fileContents, notice);
                            }
                            else
                            {
                                using (var writer = new StreamWriter(fs))
                                {
                                    try
                                    {
                                        var copyrightline = string.Format(@"{0} {1}{2}", c.StartComment, notice, string.IsNullOrEmpty(c.EndComment) ? "" : " " + c.EndComment);

                                        // Special handling for directive templates
                                        if (file.FullName.EndsWith(".tpl.html"))
                                        {
                                            try
                                            {
                                                var contents = fileContents.ToString();
                                                var n = contents.Trim().Split('\n').Length;
                                                var t = contents.IndexOf('>');
                                                var b = contents.IndexOf('\n', t);
                                                var s = "";
                                                if (t > 0 && b > 0 && b > t)
                                                {
                                                    s = contents.Substring(t + 1, b - t).Trim();
                                                }

                                                if (n > 1 && b > 0 && t > 0 && b > t && string.IsNullOrEmpty(s))
                                                {
                                                    fileContents.Insert(b + 1, string.Format("\t{0}\n", copyrightline));
                                                }
                                                else
                                                {
                                                    var idx2 = contents.IndexOf("</", StringComparison.InvariantCulture);
                                                    if (idx2 > 0)
                                                    {
                                                        fileContents.Insert(idx2, copyrightline);
                                                    }
                                                    else
                                                    {
                                                        Console.Out.WriteLine(">> Skipping {0}. Please update manually.", file.FullName);
                                                    }
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                Console.Out.WriteLine(">> Skipping {0}. Please update manually.", file.FullName);
                                            }
                                        }
                                        else
                                        {
                                            writer.WriteLine(copyrightline);
                                        }
                                            
                                        writer.Write(fileContents.ToString());
                                    }
                                    finally
                                    {
                                        writer.Close();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var subdirs = directory.GetDirectories();
            foreach (var subdir in subdirs)
            {
                var exclude = false;
                foreach (var excludepath in directive.ExcludeSubPaths)
                {
                    if (subdir.FullName.Contains(excludepath))
                    {
                        exclude = true;
                        break;
                    }
                }

                if (exclude)
                {
                    continue;
                }

                ProcessDirective(directive, subdir);
            }
        }

        /// <summary>
        /// Processes an xml based file for copyright info, manipulating the DOM to achieve the result.
        /// </summary>
        /// <param name="fs">The filestream.</param>
        /// <param name="fileContents">The file contents in a string builder.</param>
        /// <param name="notice">The formatted copyright notice.</param>
        static void ProcessXml(FileStream fs, StringBuilder fileContents, string notice)
        {
            using (var xmlwriter = new XmlTextWriter(fs, Encoding.Default))
            {
                try
                {
                    xmlwriter.Formatting = Formatting.Indented;

                    var doc = new XmlDocument();

                    doc.LoadXml(fileContents.ToString());

                    var comment = doc.CreateComment(notice);
                    if (doc.DocumentElement != null && doc.DocumentElement.HasChildNodes)
                    {
                        doc.DocumentElement.InsertBefore(comment, doc.DocumentElement.FirstChild);
                    }
                    else
                    {
                        doc.AppendChild(comment);
                    }

                    doc.WriteTo(xmlwriter);
                }
                finally
                {
                    xmlwriter.Close();
                }
            }

        }
    }
}
