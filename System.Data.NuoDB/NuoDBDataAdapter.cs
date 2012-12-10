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
    public class NuoDBDataAdapter : DbDataAdapter
    {
        private static readonly object EventRowUpdated = new object();
        private static readonly object EventRowUpdating = new object();

        public event NuoDBRowUpdatedEventHandler RowUpdated
        {
            add
            {
                base.Events.AddHandler(EventRowUpdated, value);
            }

            remove
            {
                base.Events.RemoveHandler(EventRowUpdated, value);
            }
        }

        public event NuoDBRowUpdatingEventHandler RowUpdating
        {
            add
            {
                base.Events.AddHandler(EventRowUpdating, value);
            }

            remove
            {
                base.Events.RemoveHandler(EventRowUpdating, value);
            }
        }

        public new NuoDBCommand SelectCommand
        {
            get { return (NuoDBCommand)base.SelectCommand; }
            set { base.SelectCommand = value; }
        }

        public new NuoDBCommand InsertCommand
        {
            get { return (NuoDBCommand)base.InsertCommand; }
            set { base.InsertCommand = value; }
        }

        public new NuoDBCommand UpdateCommand
        {
            get { return (NuoDBCommand)base.UpdateCommand; }
            set { base.UpdateCommand = value; }
        }

        public new NuoDBCommand DeleteCommand
        {
            get { return (NuoDBCommand)base.DeleteCommand; }
            set { base.DeleteCommand = value; }
        }

        public NuoDBDataAdapter()
			: base()
		{
		}

        public NuoDBDataAdapter(NuoDBCommand selectCommand)
			: base()
		{
			this.SelectCommand = selectCommand;
		}

        protected override int Fill(DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior)
        {
            return base.Fill(dataTables, startRecord, maxRecords, command, behavior);
        }

        protected override int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior)
        {
            return base.Fill(dataSet, startRecord, maxRecords, srcTable, command, behavior);
        }

        public override DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
        {
            return base.FillSchema(dataSet, schemaType);
        }

        public new DataTable FillSchema(DataTable dataTable, SchemaType schemaType)
        {
            return base.FillSchema(dataTable, schemaType);
        }

        public new DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, string srcTable)
        {
            return base.FillSchema(dataSet, schemaType, srcTable);
        }

        protected override DataTable FillSchema(DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior)
        {
            return base.FillSchema(dataTable, schemaType, command, behavior);
        }

        protected override DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior)
        {
            return base.FillSchema(dataSet, schemaType, command, srcTable, behavior);
        }

        protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
        {
            return base.GetBatchedParameter(commandIdentifier, parameterIndex);
        }

        protected override bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error)
        {
            recordsAffected = 0;
            error = null;
            return false;
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
            DataRow dataRow,
            IDbCommand command,
            StatementType statementType,
            DataTableMapping tableMapping)
        {
            return new RowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
            DataRow dataRow,
            IDbCommand command,
            StatementType statementType,
            DataTableMapping tableMapping)
        {
            return new RowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            NuoDBRowUpdatingEventHandler handler = (NuoDBRowUpdatingEventHandler)base.Events[EventRowUpdating];
            if (handler != null && value != null)
            {
                handler(this, value);
            }
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            NuoDBRowUpdatedEventHandler handler = (NuoDBRowUpdatedEventHandler)base.Events[EventRowUpdated];
            if (handler != null && value != null)
            {
                handler(this, value);
            }
        }

        protected override int Update(DataRow[] dataRows, DataTableMapping tableMapping)
        {
            return 0;
        }
    }
}
