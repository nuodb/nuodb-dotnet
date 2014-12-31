/****************************************************************************
* Copyright (c) 2012-2015, NuoDB, Inc.
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
    [System.ComponentModel.DesignerCategory("")]
    internal sealed class NuoDbMultipleCommands : DbCommand, ICloneable
    {
        public static CommandType MultipleTexts = (CommandType)4096;

        private NuoDbConnection connection;
        private string sqlText = "";
        private int timeout;
        private System.Data.CommandType commandType = CommandType.Text;
        private NuoDbDataParameterCollection parameters = new NuoDbDataParameterCollection();
        private bool isDesignTimeVisible = false;
        private UpdateRowSource updatedRowSource = UpdateRowSource.Both;
        internal Type[] ExpectedColumnTypes { get; private set; }

        internal NuoDbMultipleCommands(string query, DbConnection conn)
        {
            if (!(conn is NuoDbConnection))
                throw new ArgumentException("Connection is not a NuoDB connection", "conn");
            sqlText = query;
            connection = (NuoDbConnection)conn;
        }

        internal NuoDbMultipleCommands(DbConnection conn)
            : this("", conn)
        {
        }

        internal NuoDbMultipleCommands()
        {
        }

        internal NuoDbMultipleCommands(Type[] expectedColumnTypes)
            : this()
        {
            ExpectedColumnTypes = expectedColumnTypes;
        }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override string CommandText
        {
            get
            {
                return sqlText;
            }
            set
            {
                sqlText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        public override System.Data.CommandType CommandType
        {
            get
            {
                return commandType;
            }
            set
            {
                if (value != CommandType.Text && value != MultipleTexts)
                    throw new NotImplementedException("Only CommandType.Text and NuoDbMultipleCommands.MultipleTexts are supported");
                commandType = value;
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            return new NuoDbParameter();
        }

        protected override DbConnection DbConnection
        {
            get
            {
                return connection;
            }
            set
            {
                if (value != null && !(value is NuoDbConnection))
                    throw new ArgumentException("Connection is not a NuoDB connection", "conn");
                connection = (NuoDbConnection)value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                if (connection == null || connection.State != ConnectionState.Open)
                    return null;
                return connection.InternalConnection.transaction;
            }
            set
            {
                if (value != null)
                {
                    // setting this command to work inside a specific transaction means using its connection
                    if (!(value is NuoDbTransaction))
                        throw new ArgumentException("Transaction is not a NuoDB transaction object");
                    connection = (NuoDbConnection)value.Connection;
                }
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                return isDesignTimeVisible;
            }
            set
            {
                this.isDesignTimeVisible = value;
            }
        }

        private List<NuoDbCommand> splitStatements()
        {
            List<string> statements = new List<string>();

            StringBuilder curStatement = new StringBuilder();
            bool inSingleQuotes = false, inDoubleQuotes = false, inSmartQuotes = false;
            foreach (char c in CommandText)
            {
                if (c == '\'' && !(inDoubleQuotes || inSmartQuotes))
                {
                    inSingleQuotes = !inSingleQuotes;
                    curStatement.Append(c);
                    continue;
                }
                else if (c == '\"' && !(inSingleQuotes || inSmartQuotes))
                {
                    inDoubleQuotes = !inDoubleQuotes;
                    curStatement.Append(c);
                    continue;
                }
                else if (c == '`' && !(inSingleQuotes || inDoubleQuotes))
                {
                    inSmartQuotes = !inSmartQuotes;
                    curStatement.Append(c);
                    continue;
                }
                if (inSingleQuotes || inDoubleQuotes || inSmartQuotes)
                {
                    curStatement.Append(c);
                    continue;
                }

                if (c == ';')
                {
                    statements.Add(curStatement.ToString());
                    curStatement = new StringBuilder();
                }
                else
                {
                    curStatement.Append(c);
                }
            }
            if (curStatement.Length > 0)
            {
                statements.Add(curStatement.ToString());
            }

            List<NuoDbCommand> cmds = new List<NuoDbCommand>();
            if (statements.Count == 0)
                return cmds;

            for (int i = 0; i < statements.Count - 1; i++)
            {
                NuoDbCommand cmd = new NuoDbCommand(statements[i], Connection);
                cmd.CommandTimeout = CommandTimeout;
                cmd.UpdatedRowSource = UpdatedRowSource;
                cmds.Add(cmd);
            }
            NuoDbCommand lastCmd;
            if (ExpectedColumnTypes == null)
                lastCmd = new NuoDbCommand(statements[statements.Count - 1], Connection);
            else
            {
                lastCmd = new NuoDbCommand(ExpectedColumnTypes);
                lastCmd.CommandText = statements[statements.Count - 1];
                lastCmd.Connection = Connection;
            }
            lastCmd.CommandTimeout = CommandTimeout;
            lastCmd.UpdatedRowSource = UpdatedRowSource;
            foreach (NuoDbParameter p in this.Parameters)
            {
                lastCmd.Parameters.Add(((ICloneable)p).Clone());
            }
            cmds.Add(lastCmd);
            return cmds;
        }

        protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        {
            List<NuoDbCommand> statements = splitStatements();
            if (statements.Count == 0)
                throw new NuoDbSqlException("No statement specified");
            for (int i = 0; i < statements.Count - 1; i++)
                statements[i].ExecuteNonQuery();
            return statements[statements.Count-1].ExecuteReader();
        }

        public override int ExecuteNonQuery()
        {
            List<NuoDbCommand> statements = splitStatements();
            if (statements.Count == 0)
                throw new NuoDbSqlException("No statement specified");
            for (int i = 0; i < statements.Count - 1; i++)
                statements[i].ExecuteNonQuery();
            return statements[statements.Count - 1].ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            List<NuoDbCommand> statements = splitStatements();
            if (statements.Count == 0)
                throw new NuoDbSqlException("No statement specified");
            for (int i = 0; i < statements.Count - 1; i++)
                statements[i].ExecuteNonQuery();
            return statements[statements.Count - 1].ExecuteScalar();
        }

        public override void Prepare()
        {
            // NuoDbMultipleCommands doesn't support prepare
        }

        public override System.Data.UpdateRowSource UpdatedRowSource
        {
            get
            {
                return updatedRowSource;
            }
            set
            {
                this.updatedRowSource = value;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            NuoDbMultipleCommands command = new NuoDbMultipleCommands();

            command.CommandText = this.CommandText;
            command.Connection = this.Connection;
            command.Transaction = this.Transaction;
            command.CommandType = this.CommandType;
            command.CommandTimeout = this.CommandTimeout;
            command.UpdatedRowSource = this.UpdatedRowSource;

            if (this.ExpectedColumnTypes != null)
                command.ExpectedColumnTypes = (Type[])this.ExpectedColumnTypes.Clone();

            foreach (NuoDbParameter p in this.Parameters)
            {
                command.Parameters.Add(((ICloneable)p).Clone());
            }

            return command;
        }

        #endregion
    }
}
