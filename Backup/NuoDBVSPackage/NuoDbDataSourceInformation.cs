﻿/****************************************************************************
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
    class NuoDbDataSourceInformation : AdoDotNetDataSourceInformation
    {
        public NuoDbDataSourceInformation(Microsoft.VisualStudio.Data.DataConnection dataConnection)
            : base(dataConnection)
        {
            base.AddProperty(AdoDotNetDataSourceInformation.CatalogSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.CatalogSupportedInDml, false);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultCatalog, null);
            NuoDbDataConnectionProperties helper = new NuoDbDataConnectionProperties();
            helper.ConnectionStringBuilder.ConnectionString = dataConnection.DisplayConnectionString;
            object defaultSchema = null;
            helper.ConnectionStringBuilder.TryGetValue("Schema", out defaultSchema);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultSchema, defaultSchema);
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierOpenQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierCloseQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierPartsCaseSensitive, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefix, "");
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefixInName, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ProcedureSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.QuotedIdentifierPartsCaseSensitive, true);
            base.AddProperty(AdoDotNetDataSourceInformation.QuotedIdentifierPartsStorageCase, 'M');
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupported, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupportedInDml, true);
            base.AddProperty(AdoDotNetDataSourceInformation.ServerSeparator, ".");
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsAnsi92Sql, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsQuotedIdentifierParts, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsCommandTimeout, false);
        }

    }
}
