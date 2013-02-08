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
using System.Security;
using System.Security.Permissions;
using System.Data.NuoDB.EntityFramework;

namespace System.Data.NuoDB
{
    public class NuoDBProviderFactory : DbProviderFactory, IServiceProvider
    {
        public static readonly NuoDBProviderFactory Instance = new NuoDBProviderFactory();

        public override bool CanCreateDataSourceEnumerator
        {
            get { return false; }
        }

        public override DbCommand CreateCommand()
        {
            return new NuoDBCommand();
        }
        
        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new NuoDBCommandBuilder();
        }
        
        public override DbConnection CreateConnection()
        {
            return new NuoDBConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new NuoDBConnectionStringBuilder();
        }
        
        public override DbDataAdapter CreateDataAdapter()
        {
            return new NuoDBDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return null;
        }

        public override DbParameter CreateParameter()
        {
            return new NuoDBParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return null;
        }


        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("NuoDBProviderFactory::GetService({0})", serviceType));
            if (serviceType == typeof(DbProviderServices))
            {
                return NuoDBProviderServices.Instance;
            }
            return null;
        }

        #endregion
    }
}
