using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;
using System.Reflection;
using Microsoft.VisualStudio.Data.Core;

namespace NuoDB.VisualStudio.DataTools
{
    [Guid(GuidList.guidNuoDBObjectFactoryServiceString)]
    public class NuoDBDataProviderObjectFactory : AdoDotNetProviderObjectFactory
    {
        public NuoDBDataProviderObjectFactory() : base()
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataProviderObjectFactory()");
        }

        public override object CreateObject(Type objType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBDataProviderObjectFactory::CreateObject({0})", objType.FullName));

            if (objType == typeof(DataConnectionSupport))
            {
                return new NuoDBDataConnectionSupport();
            }
            else if (objType == typeof(DataConnectionUIControl))
            {
                return new NuoDBDataConnectionUIControl();
            }
            else if (objType == typeof(DataConnectionProperties))
            {
                return new NuoDBDataConnectionProperties();
            }

            return base.CreateObject(objType);
        }
    }
}
