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

using Microsoft.VisualStudio.Shell;

namespace NuoDB.VisualStudio.DataTools
{
    class ProvideDataSourceAttribute : RegistrationAttribute
    {
        private string dataSourceGuid;
        private string dataProviderGuid;
        private string dataSourceName;
        private string dataProviderName;
        private string factoryGuid;

        public string DataSourceGuid
        {
            get { return dataSourceGuid; }
            set { dataSourceGuid = value; }
        }

        public string DataProviderGuid
        {
            get { return dataProviderGuid; }
            set { dataProviderGuid = value; }
        }

        public string DataSourceName
        {
            get { return dataSourceName; }
            set { dataSourceName = value; }
        }

        public string DataProviderName
        {
            get { return dataProviderName; }
            set { dataProviderName = value; }
        }

        public string FactoryGuid
        {
            get { return factoryGuid; }
            set { factoryGuid = value; }
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            Key packageKey = null, packageProviderRef = null, packageProvider = null;
            try
            {
                packageKey = context.CreateKey(@"DataSources\{" + dataSourceGuid + @"}");
                packageKey.SetValue("", dataSourceName);
                packageKey.SetValue("DefaultProvider", @"{" + dataProviderGuid + @"}");

                packageProviderRef = packageKey.CreateSubkey(@"SupportingProviders\{"+ dataProviderGuid + @"}");
                packageProviderRef.SetValue("DisplayName", "Provider_Label, VSPackage, NuoDB.VisualStudio.DataTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b856dd8cd87216c3");
                packageProviderRef.SetValue("Description", "Provider_Help, VSPackage, NuoDB.VisualStudio.DataTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b856dd8cd87216c3");

                packageProvider = context.CreateKey(@"DataProviders\{" + dataProviderGuid + @"}");
                packageProvider.SetValue("", dataProviderName);

                packageProvider.SetValue("Description", "Provider_Description, VSPackage, NuoDB.VisualStudio.DataTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b856dd8cd87216c3");
                packageProvider.SetValue("DisplayName", "Provider_DisplayName, VSPackage, NuoDB.VisualStudio.DataTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b856dd8cd87216c3");
                packageProvider.SetValue("ShortDisplayName", "Provider_ShortDisplayName, VSPackage, NuoDB.VisualStudio.DataTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b856dd8cd87216c3");
                //packageProvider.SetValue("CodeBase", context.CodeBase);

                packageProvider.SetValue("InvariantName", "System.Data.NuoDB");
                //packageProvider.SetValue("RuntimeInvariantName", "System.Data.SqlServerCe.3.5");
                packageProvider.SetValue("Technology", "{77AB9A9D-78B9-4ba7-91AC-873F5338F1D2}");

                packageProvider.SetValue("FactoryService", "{" + factoryGuid + "}");
                //packageProvider.SetValue("AssociatedSource", "{" + dataSourceGuid + "}");
                //packageProvider.SetValue("PlatformVersion", "1.0");

                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionProperties").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDbDataConnectionProperties");
                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionSupport").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDbDataConnectionSupport");
                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionUIControl").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDbDataConnectionUIControl");
                packageProvider.CreateSubkey(@"SupportedObjects\DataViewSupport").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDbDataViewSupport");
                packageProvider.CreateSubkey(@"SupportedObjects\DataObjectSupport").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDbDataObjectSupport");
                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionPromptDialog");
                packageProvider.CreateSubkey(@"SupportedObjects\DataSourceSpecializer");
                packageProvider.CreateSubkey(@"SupportedObjects\Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataConnectionUIConnector");
                packageProvider.CreateSubkey(@"SupportedObjects\Microsoft.VisualStudio.Data.Core.IVsDataProviderDynamicSupport");
            }
            finally
            {
                if (packageKey != null)
                    packageKey.Close();
                if (packageProviderRef != null)
                    packageProviderRef.Close();
                if (packageProvider != null)
                    packageProvider.Close();
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(@"DataSources\{" + dataSourceGuid + @"}");
            context.RemoveKey(@"DataProviders\{" + dataProviderGuid + @"}");
        }

    }
}
