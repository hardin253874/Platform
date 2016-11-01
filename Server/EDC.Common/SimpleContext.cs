// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC
{
    /// <summary>
    /// Helper for creating simple context blocks.
    /// </summary>
    public static class ContextHelper
    {
        public static IDisposable Create(Action exitAction)
        {
            if (exitAction == null)
                throw new ArgumentNullException("exitAction");

            return new Context(exitAction);
        }

        private class Context : IDisposable
        {
            private readonly Action _exit;

            public Context(Action exit)
            {
                _exit = exit;
            }

            public void Dispose()
            {
                _exit();                
            }
        }        
    }
}
