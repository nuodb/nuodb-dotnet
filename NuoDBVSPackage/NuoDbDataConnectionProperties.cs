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

using Microsoft.VisualStudio.Data.AdoDotNet;
using Microsoft.VisualStudio.Data;

namespace NuoDb.VisualStudio.DataTools
{
    public class NuoDbDataConnectionProperties : AdoDotNetConnectionProperties
    {
        public NuoDbDataConnectionProperties() 
            : base("NuoDb.Data.Client")
        {
        }

        public override string[] GetBasicProperties()
        {
            return new string[] { "Server", "User", "Password", "Database", "Pooling", "ConnectionLifetime", "MaxLifetime", "MaxConnections" };
        }

        protected override void InitializeProperties()
        {
            base.InitializeProperties();

            this.AddProperty("Server", typeof(System.String));
            this.AddProperty("User", typeof(System.String));
            this.AddProperty("Password", typeof(System.String));
            this.AddProperty("Database", typeof(System.String));
            this.AddProperty("Schema", typeof(System.String));
            this.AddProperty("Pooling", typeof(System.Boolean));
            this.AddProperty("ConnectionLifetime", typeof(System.Int32));
            this.AddProperty("MaxLifetime", typeof(System.Int32));
            this.AddProperty("MaxConnections", typeof(System.Int32));
        }

    }
}
