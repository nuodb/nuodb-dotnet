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

using System.Data.Common;
using System.Xml;
using System.Data.Metadata.Edm;
using System;

namespace NuoDb.Data.Client.EntityFramework
{
    class NuoDbProviderManifest : DbXmlEnabledProviderManifest
    {
        internal const int BinaryMaxSize = Int32.MaxValue;
        internal const int VarcharMaxSize = 32765;
        internal const int NVarcharMaxSize = VarcharMaxSize;
        internal const char LikeEscapeCharacter = '\\';

        public NuoDbProviderManifest()
            : base(XmlReader.Create(typeof(NuoDbProviderManifest).Assembly.GetManifestResourceStream("NuoDb.Data.Client.EntityFramework.ProviderManifest.xml")))
        {
        }

        protected override XmlReader GetDbInformation(string informationType)
        {
            if (informationType == DbProviderManifest.StoreSchemaDefinition)
            {
                return XmlReader.Create(typeof(NuoDbProviderManifest).Assembly.GetManifestResourceStream("NuoDb.Data.Client.EntityFramework.StoreSchemaDefinition.ssdl"));
            }
            else if (informationType == DbProviderManifest.StoreSchemaMapping)
            {
                return XmlReader.Create(typeof(NuoDbProviderManifest).Assembly.GetManifestResourceStream("NuoDb.Data.Client.EntityFramework.StoreSchemaMapping.msl"));
            }
            throw new NotImplementedException();
        }

        public override string NamespaceName
        {
            get { return "NuoDB"; }
        }

        public override System.Data.Metadata.Edm.TypeUsage GetEdmType(System.Data.Metadata.Edm.TypeUsage storeType)
        {
            string storeTypeName = storeType.EdmType.Name.ToLower();
            PrimitiveType edmPrimitiveType = base.StoreTypeNameToEdmPrimitiveType[storeTypeName];
            if (storeTypeName == "string" || storeTypeName == "varchar")
            {
                Facet f;
                int maxLength = -1;
                if (storeType.Facets.TryGetValue("MaxLength", false, out f) && !f.IsUnbounded && f.Value != null)
                    maxLength = (int)f.Value;
                if (maxLength != -1)
                    return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, false, false, maxLength);
                else
                    return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, false, false);
            }
            else 
                return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
            throw new NotImplementedException();
        }

        public override System.Data.Metadata.Edm.TypeUsage GetStoreType(System.Data.Metadata.Edm.TypeUsage edmType)
        {
            throw new NotImplementedException();
        }
    }
}
