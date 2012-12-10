using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data;

namespace NuoDB.VisualStudio.DataTools
{
    public class NuoDBDataViewSupport : DataViewSupport
    {
        public NuoDBDataViewSupport()
            : base("NuoDB.VisualStudio.DataTools.NuoDBDataViewSupport", typeof(NuoDBDataViewSupport).Assembly)
		{
            System.Diagnostics.Trace.WriteLine("NuoDBDataViewSupport()");
		}
    }
}
