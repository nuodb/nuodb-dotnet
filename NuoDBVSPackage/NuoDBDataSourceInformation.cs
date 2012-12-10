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
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultSchema);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultCatalog, null);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultSchema, null);
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierOpenQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierCloseQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierPartsCaseSensitive, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefix, "");
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefixInName, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ProcedureSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.QuotedIdentifierPartsCaseSensitive, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupportedInDml, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ServerSeparator, ".");
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsAnsi92Sql, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsQuotedIdentifierParts, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsCommandTimeout, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsQuotedIdentifierParts, true);
        }

        protected override object RetrieveValue(string propertyName)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBDataSourceInformation::RetrieveValue({0})", propertyName));
            return base.RetrieveValue(propertyName);
        }
    }
}
