using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuoDb.Data.Client.Util
{
    static class Extensions
    {
        public static string ToNuoDbString(this Guid guid)
        {
            return guid.ToString("B");
        }
    }
}
