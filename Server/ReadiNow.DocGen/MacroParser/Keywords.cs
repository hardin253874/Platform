// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.DocGen.MacroParser
{
    public static class Keywords
    {
        /// <summary>
        /// Please list ALL KEYWORDS here !!!!
        /// </summary>
        public static readonly string[] AllKeywords = new[] {
            Load, TestData,
            With, If, Force, Rows, List,
            End, Show, Position };

        /// <summary>
        /// Source: load the specific instance of the specified type
        /// </summary>
        public const string Load = "load";

        /// <summary>
        /// Source: load some test data
        /// </summary>
        public const string TestData = "testdata";



        /// <summary>
        /// Behavior: visit each instance of data loaded
        /// </summary>
        public const string With = "with";

        /// <summary>
        /// Behavior: include the block if there is data, otherwise hide the block
        /// </summary>
        public const string If = "if";

        /// <summary>
        /// Behavior: show this block exactly once, even if there is no data, or multiple data
        /// </summary>
        public const string Force = "force";

        /// <summary>
        /// Behavior: show the data as table rows
        /// </summary>
        public const string Rows = "rows";

        /// <summary>
        /// Behavior: show the data as bullet point list
        /// </summary>
        public const string List = "list";



        /// <summary>
        /// End the current block
        /// </summary>
        public const string End = "end";

        /// <summary>
        /// Explicit declaration for showing expressions
        /// </summary>
        public const string Show = "show";

        /// <summary>
        /// Keyword to display the position of the current entity in the current set (shown as 1-based)
        /// </summary>
        public const string Position = "position";
    }
}
