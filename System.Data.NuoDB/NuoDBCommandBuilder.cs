/****************************************************************************
* Copyright (c) 2012, NuoDB, Inc.
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
using System.Data.Common;

namespace System.Data.NuoDB
{
    // this declaration prevents VS2010 from trying to edit this class with the designer.
    // it would try to do that because the parent class derives from Component, but it's abstract
    // and it cannot be instanciated
    [System.ComponentModel.DesignerCategory("")]
    class NuoDBCommandBuilder : DbCommandBuilder
    {
        new public DbCommand GetDeleteCommand()
        {
            return base.GetDeleteCommand();
        }

        new public DbCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return base.GetDeleteCommand(useColumnsForParameterNames);
        }

        new public DbCommand GetInsertCommand()
        {
            return base.GetInsertCommand();
        }

        new public DbCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return base.GetInsertCommand(useColumnsForParameterNames);
        }

        protected override DataTable GetSchemaTable(DbCommand sourceCommand)
        {
            return base.GetSchemaTable(sourceCommand);
        }

        new public DbCommand GetUpdateCommand()
        {
            return base.GetUpdateCommand();
        }

        new public DbCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return base.GetUpdateCommand(useColumnsForParameterNames);
        }

        protected override DbCommand InitializeCommand(DbCommand command)
        {
            return base.InitializeCommand(command);
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            return unquotedIdentifier;
        }

        public override void RefreshSchema()
        {
            base.RefreshSchema();
        }

        new protected void RowUpdatingHandler(RowUpdatingEventArgs rowUpdatingEvent)
        {
            base.RowUpdatingHandler(rowUpdatingEvent);
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            return quotedIdentifier;
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause)
        {
            throw new NotImplementedException();
        }

        protected override string GetParameterName(string parameterName)
        {
            throw new NotImplementedException();
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            throw new NotImplementedException();
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            throw new NotImplementedException();
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (!(adapter is NuoDBDataAdapter))
                throw new InvalidOperationException("adapter needs to be a NuoDBDataAdapter");

            ((NuoDBDataAdapter)adapter).RowUpdating += new NuoDBRowUpdatingEventHandler(this.RowUpdatingHandlerHelper);
        }

        private void RowUpdatingHandlerHelper(object sender, RowUpdatingEventArgs e)
        {
            base.RowUpdatingHandler(e);
        }


    }
}
