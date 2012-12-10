using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data.AdoDotNet;
using Microsoft.VisualStudio.Data;

namespace NuoDB.VisualStudio.DataTools
{
    public class NuoDBDataConnectionSupport : AdoDotNetConnectionSupport
    {
        public NuoDBDataConnectionSupport()
            : base("System.Data.NuoDB")
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionSupport()");
        }
        
        protected override DataSourceInformation CreateDataSourceInformation()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataConnectionSupport::CreateDataSourceInformation()");

            return new NuoDBDataSourceInformation(base.Site as DataConnection);
        }

        protected override object GetServiceImpl(Type serviceType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBDataConnectionSupport::GetServiceImpl({0})", serviceType.FullName));

            if (serviceType == typeof(DataViewSupport))
            {
                return new NuoDBDataViewSupport();
            }
            else if (serviceType == typeof(DataObjectSupport))
            {
                return new NuoDBDataObjectSupport();
            }
            else if (serviceType == typeof(DataObjectIdentifierResolver))
            {
                return new NuoDBDataObjectIdentifierResolver(base.Site as DataConnection);
            }
            else if (serviceType == typeof(DataObjectIdentifierConverter))
            {
                return new NuoDBObjectIdentifierConverter(base.Site as DataConnection);
            }

            return base.GetServiceImpl(serviceType);
        }

    }
}
