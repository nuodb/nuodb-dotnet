using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data.AdoDotNet;
using System.Data;

namespace NuoDB.VisualStudio.DataTools
{
    class NuoDBObjectConceptMapper : AdoDotNetObjectConceptMapper
    {
        protected override DbType GetDbTypeFromNativeType(string nativeType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBObjectConceptMapper::GetDbTypeFromNativeType({0})", nativeType));

            foreach(DataRow row in this.DataTypes.Rows)
                foreach(object o in row.ItemArray)
                    System.Diagnostics.Trace.WriteLine(o);

            DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

            if (rows != null && rows.Length > 0)
            {
                return (DbType)Convert.ToInt32(rows[0]["ProviderDbType"]);
            }

            return base.GetDbTypeFromNativeType(nativeType);
        }

        protected override int GetProviderTypeFromNativeType(string nativeType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBObjectConceptMapper::GetProviderTypeFromNativeType({0})", nativeType));
            DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

            if (rows != null && rows.Length > 0)
            {
                return Convert.ToInt32(rows[0]["ProviderDbType"]);
            }

            return base.GetProviderTypeFromNativeType(nativeType);
        }

        protected override Type GetFrameworkTypeFromNativeType(string nativeType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBObjectConceptMapper::GetFrameworkTypeFromNativeType({0})", nativeType));
            DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

            if (rows != null && rows.Length > 0)
            {
                return Type.GetType(rows[0]["DataType"].ToString());
            }

            return base.GetFrameworkTypeFromNativeType(nativeType);
        }
    }
}
