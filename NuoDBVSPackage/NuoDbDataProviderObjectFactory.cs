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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;
using System.Reflection;
using Microsoft.VisualStudio.Data.Core;

namespace NuoDB.VisualStudio.DataTools
{
    [Guid(GuidList.guidNuoDBObjectFactoryServiceString)]
    public class NuoDbDataProviderObjectFactory : AdoDotNetProviderObjectFactory
    {
        public NuoDbDataProviderObjectFactory() : base()
        {
            System.Diagnostics.Trace.WriteLine("NuoDbDataProviderObjectFactory()");
        }

        public override object CreateObject(Type objType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDbDataProviderObjectFactory::CreateObject({0})", objType.FullName));

            if (objType == typeof(DataConnectionSupport))
            {
                return new NuoDbDataConnectionSupport();
            }
            else if (objType == typeof(DataConnectionUIControl))
            {
                return new NuoDbDataConnectionUIControl();
            }
            else if (objType == typeof(DataConnectionProperties))
            {
                return new NuoDbDataConnectionProperties();
            }

            return base.CreateObject(objType);
        }
    }
}
