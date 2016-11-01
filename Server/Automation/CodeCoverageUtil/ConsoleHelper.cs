// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil
{
    internal static class ConsoleHelper
    {
        /// <summary>
        /// Write a line to console in the given color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void ConsoleWriteLine(ConsoleColor color, string format, params object[] args)
        {
            ConsoleColor backupColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ForegroundColor = backupColor;
        }


        /// <summary>
        /// Write a line to console in the given color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="value">The value.</param>
        public static void ConsoleWriteLine(ConsoleColor color, string value)
        {
            ConsoleColor backupColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = backupColor;
        }


        /// <summary>
        /// Write a message to console in the given color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void ConsoleWrite(ConsoleColor color, string format, params object[] args)
        {
            ConsoleColor backupColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(format, args);
            Console.ForegroundColor = backupColor;
        }


        /// <summary>
        /// Write a message to console in the given color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="value">The value.</param>
        public static void ConsoleWrite(ConsoleColor color, string value)
        {
            ConsoleColor backupColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = backupColor;
        }                        
    }
}
