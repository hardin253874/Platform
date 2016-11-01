// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow
{

    /// <summary>
    /// This class can be added as a static instance to a static class to allow finalization/Disposing behaviour to run.
    /// 
    /// </summary>
    public class StaticFinalizerAction: IDisposable
    {
        private bool _hasDisposed = false;
        private readonly Action _actionOnFinalize;

        public StaticFinalizerAction(Action actionOnFinalize)
        {
            _actionOnFinalize = actionOnFinalize;
        }

        public void Dispose()
        {
            if (!_hasDisposed)
            {
                _actionOnFinalize();
                _hasDisposed = true;
            }
        }

        ~StaticFinalizerAction()
        {
            Dispose();
        }

    }
}
