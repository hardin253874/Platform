// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Interface for an activity that executes immediately and returns
    /// </summary>
    public interface IRunNowActivity
    {
        void OnRunNow(IRunState context, ActivityInputs inputs);
    }
}