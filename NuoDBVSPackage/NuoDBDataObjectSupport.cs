using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data;

namespace NuoDB.VisualStudio.DataTools
{
    public class NuoDBDataObjectSupport : DataObjectSupport
    {
        public NuoDBDataObjectSupport()
            : base("NuoDB.VisualStudio.DataTools.NuoDBDataObjectSupport", typeof(NuoDBDataObjectSupport).Assembly)
		{
            System.Diagnostics.Trace.WriteLine("NuoDBDataObjectSupport()");
		}
    }
}
