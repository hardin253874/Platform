// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using NUnit.Framework;

[SetUpFixture]
// ReSharper disable once CheckNamespace
public class AssemblyTestFixture
{
    /// <summary>
    /// Setup for the assembly.
    /// </summary>
    [SetUp]

    public void AssemblySetup()
    {
        DeferredChannelMessageContext.SuppressNoContextWarning = true;
    }

    /// <summary>
    /// Teardown for the assembly.
    /// </summary>
    [TearDown]

    public void AssemblyTearDown()
    {
        DeferredChannelMessageContext.SuppressNoContextWarning = false;
    }
}