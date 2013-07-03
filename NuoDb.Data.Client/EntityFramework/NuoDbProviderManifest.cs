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
* 
* Contributors:
*  Jiri Cincura (jiri@cincura.net)
****************************************************************************/

#if !__MonoCS__

using System.Data.Common;
using System.Xml;
using System.Data.Metadata.Edm;
using System;
using System.Text;

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

        public override TypeUsage GetEdmType(TypeUsage storeType)
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
            else if (storeTypeName == "decimal" || storeTypeName == "numeric")
            {
                Facet f;
                byte precision = 0;
                if (storeType.Facets.TryGetValue("Precision", false, out f) && !f.IsUnbounded && f.Value != null)
                    precision = (byte)f.Value;
                byte scale = 0;
                if (storeType.Facets.TryGetValue("Scale", false, out f) && !f.IsUnbounded && f.Value != null)
                    scale = (byte)f.Value;
                if (precision != 0 && scale != 0)
                    return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, precision, scale);
                else
                    return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType);
            }
            else
                return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
            throw new NotImplementedException();
        }

        public override TypeUsage GetStoreType(TypeUsage edmType)
        {
            if (edmType == null)
                throw new ArgumentNullException("edmType");

            System.Diagnostics.Debug.Assert(edmType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);

            PrimitiveType primitiveType = edmType.EdmType as PrimitiveType;
            if (primitiveType == null)
                throw new ArgumentException(String.Format("The underlying provider does not support the type '{0}'.", edmType));

            ReadOnlyMetadataCollection<Facet> facets = edmType.Facets;
            Facet f;

            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["boolean"]);

                case PrimitiveTypeKind.Byte:
                case PrimitiveTypeKind.Int16:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["smallint"]);

                case PrimitiveTypeKind.Int32:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["integer"]);

                case PrimitiveTypeKind.Int64:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bigint"]);

                case PrimitiveTypeKind.Double:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["double"]);

                case PrimitiveTypeKind.Single:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float"]);

                case PrimitiveTypeKind.Decimal: // decimal, numeric
                    {
                        byte precision = 9;
                        if (facets.TryGetValue("Precision", false, out f) && !f.IsUnbounded && f.Value != null)
                            precision = (byte)f.Value;
                        byte scale = 0;
                        if (facets.TryGetValue("Scale", false, out f) && !f.IsUnbounded && f.Value != null)
                            scale = (byte)f.Value;

                        return TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
                    }

                case PrimitiveTypeKind.String: // char, varchar, text blob
                    {
                        bool isUnicode = true;
                        if (facets.TryGetValue("Unicode", false, out f) && !f.IsUnbounded && f.Value != null)
                            isUnicode = (bool)f.Value;
                        bool isFixedLength = true;
                        if (facets.TryGetValue("FixedLength", false, out f) && !f.IsUnbounded && f.Value != null)
                            isFixedLength = (bool)f.Value;
                        int maxLength = Int32.MinValue;
                        if (facets.TryGetValue("MaxLength", false, out f) && !f.IsUnbounded && f.Value != null)
                            maxLength = (int)f.Value;

                        PrimitiveType storePrimitiveType = StoreTypeNameToStorePrimitiveType[isFixedLength ? "char" : "varchar"];
                        if (maxLength != Int32.MinValue)
                            return TypeUsage.CreateStringTypeUsage(storePrimitiveType, isUnicode, isFixedLength, maxLength);

                        return TypeUsage.CreateStringTypeUsage(storePrimitiveType, isUnicode, isFixedLength);
                    }

                case PrimitiveTypeKind.DateTime: // datetime, date
                    {
                        byte precision = 4;
                        if (facets.TryGetValue("Precision", false, out f) && !f.IsUnbounded && f.Value != null)
                            precision = (byte)f.Value;

                        bool useTimestamp = (precision != 0);

                        return TypeUsage.CreateDefaultTypeUsage(useTimestamp ? StoreTypeNameToStorePrimitiveType["datetime"] : StoreTypeNameToStorePrimitiveType["date"]);
                    }

                case PrimitiveTypeKind.Time:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["time"]);

                case PrimitiveTypeKind.Binary: // blob
                    {
                        bool isFixedLength = true;
                        if (facets.TryGetValue("FixedLength", false, out f) && !f.IsUnbounded && f.Value != null)
                            isFixedLength = (bool)f.Value;
                        int maxLength = Int32.MinValue;
                        if (facets.TryGetValue("MaxLength", false, out f) && !f.IsUnbounded && f.Value != null)
                            maxLength = (int)f.Value;

                        PrimitiveType storePrimitiveType = StoreTypeNameToStorePrimitiveType["binary"];
                        if (maxLength != Int32.MinValue)
                            return TypeUsage.CreateBinaryTypeUsage(storePrimitiveType, isFixedLength, maxLength);

                        return TypeUsage.CreateBinaryTypeUsage(storePrimitiveType, isFixedLength);
                    }

                case PrimitiveTypeKind.Guid:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["guid_char"]);

                default:
                    throw new NotSupportedException(string.Format("There is no store type corresponding to the EDM type '{0}' of primitive type '{1}'.", edmType, primitiveType.PrimitiveTypeKind));
            }
        }

#if NET_40
        public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
        {
            escapeCharacter = LikeEscapeCharacter;
            return true;
        }

        public override string EscapeLikeArgument(string argument)
        {
            StringBuilder sb = new StringBuilder(argument);
            sb.Replace(LikeEscapeCharacter.ToString(), LikeEscapeCharacter.ToString() + LikeEscapeCharacter.ToString());
            sb.Replace("%", LikeEscapeCharacter + "%");
            sb.Replace("_", LikeEscapeCharacter + "_");
            return sb.ToString();
        }
#endif

    }
}

#endif