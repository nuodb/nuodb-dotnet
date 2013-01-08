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

// Guids.cs
// MUST match guids.h
using System;

namespace NuoDB.VisualStudio.DataTools
{
    static class GuidList
    {
        public const string guidNuoDBVSPackagePkgString = "407f565d-8772-4770-8243-5a1f396aae9d";
        public const string guidNuoDBVSPackageCmdSetString = "41f5f498-5d4f-4811-844e-6e6bd65125c8";
        public const string guidNuoDBObjectFactoryServiceString = "41f5f498-5d4f-4811-844e-6e6bd65125c9";
        public const string guidNuoDBDataSourceString = "0DFA90C2-CB54-4889-B3F4-77BA5E28FAAC";
        public const string guidNuoDBDataProvider = "80AF79F8-2574-48FB-9E78-3ABB020ABB3F";

        public static readonly Guid guidNuoDBVSPackageCmdSet = new Guid(guidNuoDBVSPackageCmdSetString);
        public static readonly Guid guidNuoDBObjectFactoryService = new Guid(guidNuoDBObjectFactoryServiceString);
        public static readonly Guid guidNuoDBDataSource = new Guid(guidNuoDBDataSourceString);
    };
}