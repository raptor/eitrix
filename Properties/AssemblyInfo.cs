using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Eitrix")]
[assembly: AssemblyProduct("Eitrix")]
[assembly: AssemblyDescription("")]

[assembly: AssemblyCopyright("Copyright © Microsoft 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("25072692-4720-431f-ae74-2ce008eeba63")]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("0.4.0.000")]

/// --------------------------------------------------------------------------
/// <summary>
/// Constants for the general assembly
/// </summary>
/// --------------------------------------------------------------------------
public static class AssemblyConstants
{
    public static DateTime expirationDate = DateTime.Parse("4/28/2011 8:28:37 PM",
        new CultureInfo("en-US"));
    public static string Version;

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Static Constructor
    /// </summary>
    /// --------------------------------------------------------------------------
    static AssemblyConstants()
    {
        //Default to the version of the currently executing Assembly
        Version version = Assembly.GetExecutingAssembly().GetName().Version;

        Version = version.ToString();
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Returns true if this assembly has expired
    /// </summary>
    /// --------------------------------------------------------------------------
    public static double DaysLeftToExpiration()
    {
#if DEBUG
        TimeSpan timeSpan = expirationDate - DateTime.Now;
        return timeSpan.TotalDays;
#else
        return 100000;
#endif
    }
}