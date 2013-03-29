/****************************************************************************
* Copyright (c) 2012-2013, NuoDB, Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*   * Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*   * Neither the name of NuoDB, Inc. nor the names of its contributors may
*     be used to endorse or promote products derived from this software
*     without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL NUODB, INC. BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
* OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
* ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Data.AdoDotNet;
using System.Data;

namespace NuoDb.VisualStudio.DataTools
{
    class NuoDbObjectConceptMapper : AdoDotNetObjectConceptMapper
    {
        internal DataRow[] FetchTypeInformations(string nativeType, out string typeName, out int len, out int scale)
        {
            typeName = nativeType;
            if (nativeType.Contains('('))
            {
                int parpos = nativeType.IndexOf('(');
                string rest = nativeType.Substring(parpos).Trim(new char[] { '(', ')' });
                typeName = nativeType.Substring(0, parpos);
                if (rest.Contains(','))
                {
                    int commapos = rest.IndexOf(',');
                    len = Int32.Parse(rest.Substring(0, commapos));
                    scale = Int32.Parse(rest.Substring(commapos + 1));
                }
                else
                {
                    len = Int32.Parse(rest);
                    scale = 0;
                }
            }
            else
            {
                len = 0;
                scale = 0;
                typeName = nativeType;
            }

            return this.DataTypes.Select(String.Format("TypeName = '{0}'", typeName));
        }

        protected override DbType GetDbTypeFromNativeType(string nativeType)
        {
            DbType dbType = (DbType)GetProviderTypeFromNativeType(nativeType);
#if DEBUG
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDbObjectConceptMapper::GetDbTypeFromNativeType({0}) = {1}", nativeType, dbType));
#endif
            return dbType;
        }

        protected override int GetProviderTypeFromNativeType(string nativeType)
        {
            string typeName;
            int len, scale;
            DataRow[] rows = FetchTypeInformations(nativeType, out typeName, out len, out scale);

            if (rows != null && rows.Length > 0)
            {
                int providerType = Convert.ToInt32(rows[0]["ProviderDbType"]);
                if ((providerType == (int)DbType.Int16 || providerType == (int)DbType.Int32 || providerType == (int)DbType.Int64) &&
                     scale > 0)
                    providerType = (int)DbType.Decimal;
                return providerType;
            }

            return base.GetProviderTypeFromNativeType(nativeType);
        }

        protected override Type GetFrameworkTypeFromNativeType(string nativeType)
        {
            string typeName;
            int len, scale;
            DataRow[] rows = FetchTypeInformations(nativeType, out typeName, out len, out scale);

            if (rows != null && rows.Length > 0)
            {
                string netType = rows[0]["DataType"].ToString();
#if DEBUG
                System.Diagnostics.Trace.WriteLine(String.Format("NuoDbObjectConceptMapper::GetFrameworkTypeFromNativeType({0}) = {1}", nativeType, netType));
#endif
                return Type.GetType(netType);
            }

            return base.GetFrameworkTypeFromNativeType(nativeType);
        }
    }
}
