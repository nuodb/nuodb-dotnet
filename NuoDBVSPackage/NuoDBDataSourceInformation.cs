using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data.AdoDotNet;
using System.Data;

namespace NuoDB.VisualStudio.DataTools
{
    class NuoDBDataSourceInformation : AdoDotNetDataSourceInformation
    {
        public NuoDBDataSourceInformation(Microsoft.VisualStudio.Data.DataConnection dataConnection)
            : base(dataConnection)
        {
            base.AddProperty(AdoDotNetDataSourceInformation.CatalogSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.CatalogSupportedInDml, false);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultCatalog, null);
            NuoDBDataConnectionProperties helper = new NuoDBDataConnectionProperties();
            helper.ConnectionStringBuilder.ConnectionString = dataConnection.DisplayConnectionString;
            object defaultSchema = null;
            helper.ConnectionStringBuilder.TryGetValue("Schema", out defaultSchema);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultSchema, defaultSchema);
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierOpenQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierCloseQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierPartsCaseSensitive, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefix, "");
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefixInName, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ProcedureSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.QuotedIdentifierPartsCaseSensitive, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupported, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupportedInDml, true);
            base.AddProperty(AdoDotNetDataSourceInformation.ServerSeparator, ".");
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsAnsi92Sql, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsQuotedIdentifierParts, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsCommandTimeout, false);
        }

        protected override object RetrieveValue(string propertyName)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBDataSourceInformation::RetrieveValue({0})", propertyName));
            return base.RetrieveValue(propertyName);
        }
    }
}
