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
using System.Data.Common;
using System.Data;

namespace NuoDb.Data.Client
{
    // this declaration prevents VS2010 from trying to edit this class with the designer.
    // it would try to do that because the parent class derives from Component, but it's abstract
    // and it cannot be instanciated
    [System.ComponentModel.DesignerCategory("Code")]
    public class NuoDbDataAdapter : DbDataAdapter, ICloneable
    {
        private static readonly object EventRowUpdated = new object();
        private static readonly object EventRowUpdating = new object();

        public event NuoDbRowUpdatedEventHandler RowUpdated
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

        public event NuoDbRowUpdatingEventHandler RowUpdating
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

        public new NuoDbCommand SelectCommand
        {
            get { return (NuoDbCommand)base.SelectCommand; }
            set { base.SelectCommand = value; }
        }

        public new NuoDbCommand InsertCommand
        {
            get { return (NuoDbCommand)base.InsertCommand; }
            set { base.InsertCommand = value; }
        }

        public new NuoDbCommand UpdateCommand
        {
            get { return (NuoDbCommand)base.UpdateCommand; }
            set { base.UpdateCommand = value; }
        }

        public new NuoDbCommand DeleteCommand
        {
            get { return (NuoDbCommand)base.DeleteCommand; }
            set { base.DeleteCommand = value; }
        }

        public NuoDbDataAdapter()
			: base()
		{
		}

        public NuoDbDataAdapter(string selectStatement, NuoDbConnection connection)
            : this(new NuoDbCommand(selectStatement, connection))
        {
        }

        public NuoDbDataAdapter(NuoDbCommand selectCommand)
			: base()
        {
            this.SelectCommand = selectCommand;
        }

        public NuoDbDataAdapter(NuoDbDataAdapter other)
            : base()
        {
            this.SelectCommand = other.SelectCommand is ICloneable ? (NuoDbCommand)other.SelectCommand.Clone() : null;
            this.InsertCommand = other.InsertCommand is ICloneable ? (NuoDbCommand)other.InsertCommand.Clone() : null;
            this.DeleteCommand = other.DeleteCommand is ICloneable ? (NuoDbCommand)other.DeleteCommand.Clone() : null;
            this.UpdateCommand = other.UpdateCommand is ICloneable ? (NuoDbCommand)other.UpdateCommand.Clone() : null;
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
            NuoDbRowUpdatingEventHandler handler = (NuoDbRowUpdatingEventHandler)base.Events[EventRowUpdating];
            if (handler != null && value != null)
            {
                handler(this, value);
            }
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            NuoDbRowUpdatedEventHandler handler = (NuoDbRowUpdatedEventHandler)base.Events[EventRowUpdated];
            if (handler != null && value != null)
            {
                handler(this, value);
            }
        }

        public object Clone()
        {
            return new NuoDbDataAdapter(this);
        }
    }
}
