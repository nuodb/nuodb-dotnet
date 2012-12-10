using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data;

namespace NuoDB.VisualStudio.DataTools
{
    public class NuoDBDataObjectIdentifierResolver : DataObjectIdentifierResolver
    {
        private DataConnection dataConnection;

        public NuoDBDataObjectIdentifierResolver()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataObjectIdentifierResolver()");
        }

        public NuoDBDataObjectIdentifierResolver(DataConnection dataConnection)
        {
            this.dataConnection = dataConnection;
        }

        public override object[] ContractIdentifier(string typeName, object[] fullIdentifier, bool refresh)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBDataObjectIdentifierResolver::ContractIdentifier({0})", typeName));
            return base.ContractIdentifier(typeName, fullIdentifier, refresh);
        }
        public override object[] ExpandIdentifier(string typeName, object[] partialIdentifier, bool refresh)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBDataObjectIdentifierResolver::ExpandIdentifier({0})", typeName));
            return base.ExpandIdentifier(typeName, partialIdentifier, refresh);
        }

    }
}
