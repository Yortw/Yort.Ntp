using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("A portable C# library for making requests to NTP servers for correct current time.")]
#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Yort")]
[assembly: AssemblyProduct("Yort.Ntp")]
[assembly: AssemblyCopyright("Copyright © 2018.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
#if SUPPORTS_COMVISIBLE
[assembly: ComVisible(false)]
#endif

[assembly: System.CLSCompliant(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.13.0")]
[assembly: AssemblyFileVersion("1.0.13.0")]
