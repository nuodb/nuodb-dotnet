using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace NuoDB.VisualStudio.DataTools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). 
    // This attributes tells the shell that at the resource index 111 there is a Package Load Key (PLK)
    // corresponding to the company NuoDB for the version 1.0 of the product "DDEX Provider for NuoDB"
    // that can be loaded on any version (Standard, Professional) of Visual Studio 2008 and greater
    [ProvideLoadKey("Standard", "1.0", "DDEX Provider for NuoDB", "NuoDB", 111)]
    [Guid(GuidList.guidNuoDBVSPackagePkgString)]
    [ProvideService(typeof(NuoDBDataProviderObjectFactory), ServiceName = "NuoDB Provider Object Factory")]
    [ProvideDataSource(DataSourceGuid = GuidList.guidNuoDBDataSourceString, 
                       DataSourceName = "NuoDB Data Source", 
                       DataProviderGuid = GuidList.guidNuoDBDataProvider, 
                       DataProviderName = ".NET Framework Data Provider for NuoDB", 
                       FactoryGuid = GuidList.guidNuoDBObjectFactoryServiceString)]
    public sealed class NuoDBVSPackagePackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NuoDBVSPackagePackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            (this as IServiceContainer).AddService(typeof(NuoDBDataProviderObjectFactory), new ServiceCreatorCallback(this.CreateService), true);
            base.Initialize();

        }
        #endregion

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("DataPackage::CreateService({0})", serviceType.FullName));

            if (serviceType == typeof(NuoDBDataProviderObjectFactory))
            {
                return new NuoDBDataProviderObjectFactory();
            }

            return null;
        }

    }
}
