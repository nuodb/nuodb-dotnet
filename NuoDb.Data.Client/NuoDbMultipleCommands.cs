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
    /**
     * Helper class used to enforce a set of specific types for the columns of a DbDataReader
     * To be used when we cannot inject the specific types in the DbCommand because we cannot
     * create a NuoDbCommand because we have to work through the interfaces of ADO.NET only
     */
    internal class TypedDbReader : DbDataReader
    {
        private Type[] declaredTypes;
        private DbDataReader wrappedReader;

        internal TypedDbReader(DbDataReader reader, Type[] declaredTypes)
        {
            this.wrappedReader = reader;
            this.declaredTypes = declaredTypes;
        }

        public override void Close()
        {
            wrappedReader.Close();
        }

        public override int Depth
        {
            get { return wrappedReader.Depth; }
        }

        public override int FieldCount
        {
            get { return wrappedReader.FieldCount; }
        }

        public override bool GetBoolean(int ordinal)
        {
            return wrappedReader.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return wrappedReader.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return wrappedReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return wrappedReader.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return wrappedReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return wrappedReader.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return wrappedReader.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return wrappedReader.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return wrappedReader.GetDouble(ordinal);
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return wrappedReader.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            return declaredTypes.ElementAtOrDefault(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return wrappedReader.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return wrappedReader.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return wrappedReader.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return wrappedReader.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return wrappedReader.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return wrappedReader.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return wrappedReader.GetOrdinal(name);
        }

        public override DataTable GetSchemaTable()
        {
            return wrappedReader.GetSchemaTable();
        }

        public override string GetString(int ordinal)
        {
            return wrappedReader.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            object value = wrappedReader.GetValue(ordinal);
            Type declaredType = declaredTypes.ElementAtOrDefault(ordinal);
            if (declaredType == null)
                return value;
            if (declaredType == typeof(Guid))
                return GetGuid(ordinal);
            return Convert.ChangeType(value, declaredType);
        }

        public override int GetValues(object[] values)
        {
            int toRead = Math.Min(wrappedReader.FieldCount, values.Length);
            for (int i = 0; i < toRead; i++)
                values[i] = GetValue(i);
            for (int i = toRead; i < values.Length; i++)
                values[i] = null;
            return toRead;
        }

        public override bool HasRows
        {
            get { return wrappedReader.HasRows; }
        }

        public override bool IsClosed
        {
            get { return wrappedReader.IsClosed; }
        }

        public override bool IsDBNull(int ordinal)
        {
            return wrappedReader.IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            return wrappedReader.NextResult();
        }

        public override bool Read()
        {
            return wrappedReader.Read();
        }

        public override int RecordsAffected
        {
            get { return wrappedReader.RecordsAffected; }
        }

        public override object this[string name]
        {
            get { return GetValue(wrappedReader.GetOrdinal(name)); }
        }

        public override object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }
    }

    // this declaration prevents VS2010 from trying to edit this class with the designer.
    // it would try to do that because the parent class derives from Component, but it's abstract
    // and it cannot be instanciated
    [System.ComponentModel.DesignerCategory("")]
    internal sealed class NuoDbMultipleCommands : DbCommand, ICloneable
    {
        public static CommandType MultipleTexts = (CommandType)4096;

        private DbConnection connection;
        private string sqlText = "";
        private int timeout;
        private System.Data.CommandType commandType = CommandType.Text;
        private NuoDbDataParameterCollection parameters = new NuoDbDataParameterCollection();
        private bool isDesignTimeVisible = false;
        private UpdateRowSource updatedRowSource = UpdateRowSource.Both;
        internal Type[] ExpectedColumnTypes { get; private set; }

        internal NuoDbMultipleCommands(string query, DbConnection conn)
        {
            sqlText = query;
            connection = conn;
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
                connection = value;
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
                return ((NuoDbConnection)connection).InternalConnection.transaction;
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

        private List<DbCommand> splitStatements()
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

            List<DbCommand> cmds = new List<DbCommand>();
            if (statements.Count == 0)
                return cmds;

            for (int i = 0; i < statements.Count - 1; i++)
            {
                DbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = statements[i];
                cmd.CommandTimeout = CommandTimeout;
                cmd.UpdatedRowSource = UpdatedRowSource;
                cmds.Add(cmd);
            }
            DbCommand lastCmd;
            lastCmd = Connection.CreateCommand();
            lastCmd.CommandText = statements[statements.Count - 1];
            lastCmd.CommandTimeout = CommandTimeout;
            lastCmd.UpdatedRowSource = UpdatedRowSource;
            foreach (DbParameter p in this.Parameters)
            {
                DbParameter param = lastCmd.CreateParameter();
                param.ParameterName = p.ParameterName;
                param.Value = p.Value;
                param.Direction = p.Direction;
                param.Size = p.Size;
                param.DbType = p.DbType;
                param.SourceVersion = p.SourceVersion;
                param.SourceColumn = p.SourceColumn;
                param.SourceColumnNullMapping = p.SourceColumnNullMapping;
                lastCmd.Parameters.Add(param);
            }
            cmds.Add(lastCmd);
            return cmds;
        }

        protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        {
            List<DbCommand> statements = splitStatements();
            if (statements.Count == 0)
                throw new NuoDbSqlException("No statement specified");
            for (int i = 0; i < statements.Count - 1; i++)
                statements[i].ExecuteNonQuery();
            if (ExpectedColumnTypes != null)
                return new TypedDbReader(statements[statements.Count - 1].ExecuteReader(), ExpectedColumnTypes);
            return statements[statements.Count-1].ExecuteReader();
        }

        public override int ExecuteNonQuery()
        {
            List<DbCommand> statements = splitStatements();
            if (statements.Count == 0)
                throw new NuoDbSqlException("No statement specified");
            for (int i = 0; i < statements.Count - 1; i++)
                statements[i].ExecuteNonQuery();
            return statements[statements.Count - 1].ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            List<DbCommand> statements = splitStatements();
            if (statements.Count == 0)
                throw new NuoDbSqlException("No statement specified");
            for (int i = 0; i < statements.Count - 1; i++)
                statements[i].ExecuteNonQuery();
            object value = statements[statements.Count - 1].ExecuteScalar();
            if (ExpectedColumnTypes != null && ExpectedColumnTypes.Length > 0)
                return Convert.ChangeType(value, ExpectedColumnTypes[0]);
            return value;
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
            command.CommandType = this.CommandType;
            command.CommandTimeout = this.CommandTimeout;
            command.UpdatedRowSource = this.UpdatedRowSource;

            if (this.ExpectedColumnTypes != null)
                command.ExpectedColumnTypes = (Type[])this.ExpectedColumnTypes.Clone();

            foreach (DbParameter p in this.Parameters)
            {
                command.Parameters.Add(((ICloneable)p).Clone());
            }

            return command;
        }

        #endregion
    }
}
