using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DataDrain.ORM.Generator")]
[assembly: AssemblyDescription("DataDrain ORM é um mapeador objeto-relacional que permite que os desenvolvedores C# trabalhem com dados relacionais usando objetos específicos do domínio. Ele elimina a necessidade da maioria do código de acesso a dados que os desenvolvedores geralmente precisa escrever.")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("RodrigoDotNET")]
[assembly: AssemblyProduct("DataDrain.ORM.Generator")]
[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#pragma warning disable 1699
[assembly: AssemblyKeyFile(@"..\DataDrain.ORM.snk")]
#pragma warning restore 1699
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0833a6f4-0577-4ec2-95ca-02ebf171095c")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
//[assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyVersion("4.0.*")]
[assembly: AssemblyFileVersion("4.0.0.0")]
