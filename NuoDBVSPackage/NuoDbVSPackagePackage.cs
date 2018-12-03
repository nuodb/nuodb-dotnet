/****************************************************************************
* Copyright (c) 2012-2013, NuoDB, Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*   * Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*   * Neither the name of NuoDB, Inc. nor the names of its contributors may
*     be used to endorse or promote products derived from this software
*     without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL NUODB, INC. BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
* OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
* ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
****************************************************************************/

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
using NuoDb.VisualStudio.DataTools.Editors;

namespace NuoDb.VisualStudio.DataTools
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
    [InstalledProductRegistration("#110", "#112", "2.3.0.11", IconResourceID = 400)]
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). 
    // This attributes tells the shell that at the resource index 111 there is a Package Load Key (PLK)
    // corresponding to the company NuoDB for the version 1.0 of the product "DDEX Provider for NuoDB"
    // that can be loaded on any version (Standard, Professional) of Visual Studio 2008 and greater
    [ProvideLoadKey("Standard", "2.3.0.11", "DDEX Provider for NuoDB", "NuoDB", 111)]
    [ProvideService(typeof(NuoDbDataProviderObjectFactory), ServiceName = "NuoDB Provider Object Factory")]
    [ProvideDataSource(DataSourceGuid = GuidList.guidNuoDBDataSourceString, 
                       DataSourceName = "NuoDB Data Source", 
                       DataProviderGuid = GuidList.guidNuoDBDataProviderString, 
                       DataProviderName = ".NET Framework Data Provider for NuoDB", 
                       FactoryGuid = GuidList.guidNuoDBObjectFactoryServiceString)]
    [ProvideEditorFactory(typeof(SQLEditorFactory), 114, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(SQLEditorFactory), ".nuosql", 32,
        ProjectGuid = "{A2FE74E1-B743-11D0-AE1A-00A0C90FFFC3}",     // Miscellaneaous file projects
        TemplateDir = "Templates",
        NameResourceID = 113,
        DefaultName = "NuoDB SQL Editor")]
    [ProvideEditorLogicalView(typeof(SQLEditorFactory), GuidList.guidNuoDBSQLEditorLogicalViewString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidNuoDBVSPackagePkgString)]
    public sealed class NuoDbVSPackagePackage : Package
    {
        public static NuoDbVSPackagePackage Instance = null;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NuoDbVSPackagePackage()
        {
            Instance = this;
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
#if DEBUG
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
#endif
            (this as IServiceContainer).AddService(typeof(NuoDbDataProviderObjectFactory), new ServiceCreatorCallback(this.CreateService), true);
            base.Initialize();

            RegisterEditorFactory(new SQLEditorFactory());

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the 'New Query Connection' menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidNuoDBVSPackageCmdSet, (int)PkgCmdIDList.cmdNewConnection);
                MenuCommand menuItem = new MenuCommand(CreateNewQuery, menuCommandID);
                mcs.AddCommand(menuItem);
            }

        }
        #endregion

        private object CreateService(IServiceContainer container, Type serviceType)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine(String.Format("DataPackage::CreateService({0})", serviceType.FullName));
#endif

            if (serviceType == typeof(NuoDbDataProviderObjectFactory))
            {
                return new NuoDbDataProviderObjectFactory();
            }

            return null;
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void CreateNewQuery(object sender, EventArgs e)
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            EnvDTE.Window window = dte.ItemOperations.NewFile("NuoDB\\NuoDB SQL Editor");
            if (window != null && window.Object is SQLEditor)
            {
                ((SQLEditor)window.Object).Connect();
            }
        }

        private const int MaxVsVersion = 12;    // VS2013

        internal int GetMajorVStudioVersion()
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));
            string vsVersion = dte.Version;
            Version version;
            if (Version.TryParse(vsVersion, out version))
            {
                return version.Major;
            }
            return MaxVsVersion;
        }
    }
}
