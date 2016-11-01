// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("EDC.ReadiNow.Common")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("Global Software Innovation Pty Ltd")]
[assembly: AssemblyProduct("SoftwarePlatform.com")]
[assembly: AssemblyCopyright("Copyright 2011-2016 Global Software Innovation Pty Ltd")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("0251c723-4f3a-4e1b-a455-a6d62315dcd2")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: NeutralResourcesLanguage("en")]
[assembly: InternalsVisibleTo("EDC.ReadiNow.Common.Test")]
[assembly: InternalsVisibleTo("Configure")]
[assembly: InternalsVisibleTo("Export")]
[assembly: InternalsVisibleTo("EDC.SoftwarePlatform.WebApp")]
[assembly: InternalsVisibleTo("EDC.SoftwarePlatform.Migration")]
[assembly: InternalsVisibleTo("EDC.SoftwarePlatform.Services.Test")]
[assembly: InternalsVisibleTo("EDC.ReadiNow.CAST")]
[assembly: InternalsVisibleTo("ReadiNow.QueryEngine.Test")]
[assembly: InternalsVisibleTo("EDC.SoftwarePlatform.Activities.Test")]

/////
// TODO: Remove this once the BulkRequest mechanism is moved into ReadiNow.EntityGraph
/////
[assembly: InternalsVisibleTo("ReadiNow.EntityGraph")]

/////
// TODO: Remove this once resource framework testing is complete.
/////

[assembly: InternalsVisibleTo("TestHarness")]