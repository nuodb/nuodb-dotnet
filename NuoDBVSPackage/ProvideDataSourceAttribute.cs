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

                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionProperties").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDBDataConnectionProperties");
                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionSupport").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDBDataConnectionSupport");
                packageProvider.CreateSubkey(@"SupportedObjects\DataConnectionUIControl").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDBDataConnectionUIControl");
                packageProvider.CreateSubkey(@"SupportedObjects\DataViewSupport").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDBDataViewSupport");
                packageProvider.CreateSubkey(@"SupportedObjects\DataObjectSupport").SetValue("", "NuoDB.VisualStudio.DataTools.NuoDBDataObjectSupport");
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
