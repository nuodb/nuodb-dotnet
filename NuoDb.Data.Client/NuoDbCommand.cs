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
* 
* Contributors:
*	Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System.Data.Common;
using System;
using System.Data;
using System.Text;

namespace NuoDb.Data.Client
{
    // this declaration prevents VS2010 from trying to edit this class with the designer.
    // it would try to do that because the parent class derives from Component, but it's abstract
    // and it cannot be instanciated
    [System.ComponentModel.DesignerCategory("")]
    public sealed class NuoDbCommand : DbCommand, ICloneable
    {
        private NuoDbConnection connection;
        private string sqlText = "";
        private int timeout;
        internal int handle = -1;
        internal int updateCount;
        internal NuoDbDataReader generatedKeys;
        private NuoDbDataParameterCollection parameters = new NuoDbDataParameterCollection();
        private bool isPrepared = false;
        private bool isPreparedWithKeys = false;
        private bool isDesignTimeVisible = false;
        private UpdateRowSource updatedRowSource = UpdateRowSource.Both;

        public NuoDbCommand(string query, DbConnection conn)
        {
            if (!(conn is NuoDbConnection))
                throw new ArgumentException("Connection is not a NuoDB connection", "conn");
            sqlText = query;
            connection = (NuoDbConnection)conn;
        }

        public NuoDbCommand(DbConnection conn)
            : this("", conn)
        {
        }

        public NuoDbCommand()
        {
        }

		internal NuoDbCommand(Type[] expectedColumnTypes)
			: this()
		{
			ExpectedColumnTypes = expectedColumnTypes;
		}

        protected override void Dispose(bool disposing)
        {
            try
            {
                Close();
            }
            catch (Exception)
            {
            }
            base.Dispose(disposing);
        }

        public void Close()
        {
            if (handle == -1 || connection == null || (connection as IDbConnection).State == ConnectionState.Closed ||
				!connection.InternalConnection.IsCommandRegistered(handle))
            {
                return;
            }
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbCommand::Close()");
#endif
            connection.InternalConnection.CloseCommand(handle);
            handle = -1;
        }

        private void checkConnection()
        {
            if (connection == null || (connection as IDbConnection).State == ConnectionState.Closed)
            {
                throw new NuoDbSqlException("connection is not open");
            }
        }
        private void updateRecordsUpdated(EncodedDataStream dataStream)
        {
            int count = dataStream.getInt();
            updateCount = (count >= -1) ? count : 0;
        }
        private void updateLastCommitInfo(EncodedDataStream dataStream, bool generatingKeys)
        {
            generatedKeys = null;

            // From v2 - v6, gen keys were sent before last commit info
			if (connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION2 && connection.InternalConnection.protocolVersion < Protocol.PROTOCOL_VERSION7 && generatingKeys)
            {
                generatedKeys = createResultSet(dataStream, true);
            }

            // from v3 -v6, last commit info was not being sent if there was a gen key result set
			if ((connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION3 && !generatingKeys) || connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION7)
            {
                long transactionId = dataStream.getLong();
                int nodeId = dataStream.getInt();
                long commitSequence = dataStream.getLong();
				connection.InternalConnection.setLastTransaction(transactionId, nodeId, commitSequence);
            }

            // from v7 gen key result set is sent after last commit info (if at all)
			if (connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION7 && generatingKeys)
            {
                generatedKeys = createResultSet(dataStream, true);
            }

        }

        private NuoDbDataReader createResultSet(EncodedDataStream dataStream, bool readColumnNames)
        {
            int handle = dataStream.getInt();

            if (handle == -1)
            {
                return null;
            }

            return new NuoDbDataReader(connection, handle, dataStream, this, readColumnNames);
        }

        private void putParameters(EncodedDataStream dataStream)
        {
            dataStream.encodeInt(parameters.Count);

            for (int n = 0; n < parameters.Count; ++n)
            {
                object param = ((NuoDbParameter)parameters[n]).Value;
#if DEBUG
                System.Diagnostics.Trace.WriteLine("param " + n + "="+param);
#endif
                dataStream.encodeDotNetObject(param);
            }
        }

        public override void Cancel()
        {
        }

        public override string CommandText
        {
            get
            {
                return sqlText;
            }
            set
            {
                Close();
                sqlText = value;
                isPrepared = false;
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

        public override CommandType CommandType
        {
            get
            {
                return CommandType.Text;
            }
            set
            {
                if (value != CommandType.Text)
                    throw new NotImplementedException();
            }
        }

		internal Type[] ExpectedColumnTypes { get; private set; }

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
                if (connection == null)
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

        private void EnsureStatement(bool generatingKeys)
        {
            // if the statement is prepared, but with a different setting for generatingKeys, re-prepare it
            if (isPrepared && isPreparedWithKeys != generatingKeys)
            {
                Close();
                Prepare(generatingKeys);
            }
            // if the connection has been closed and reopened, the statement identified by this handle 
            // has been closed on the server, and we must re-create it
			if (handle == -1 || !connection.InternalConnection.IsCommandRegistered(handle))
            {
                if (parameters.Count > 0)
                {
                    Prepare(generatingKeys);
                }
                else
                {
					EncodedDataStream dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
                    dataStream.startMessage(Protocol.CreateStatement);
					connection.InternalConnection.sendAndReceive(dataStream);
                    handle = dataStream.getInt();
					connection.InternalConnection.RegisterCommand(handle);
                }
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            if (!CommandText.TrimStart(null).Substring(0, 6).ToUpper().Equals("SELECT"))
            {
#if DEBUG
                System.Diagnostics.Trace.WriteLine("The statement is not a SELECT: redirecting to ExecuteNonQuery");
#endif
                // If the command was already prepared, it will be prepared again, in order to enable the generatingKeys option
                ExecuteUpdate(true);

                return generatedKeys != null ? generatedKeys : new NuoDbDataReader(connection, -1, null, this, false);
            }
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbCommand.ExecuteDbDataReader(" + CommandText + ", " + behavior + ")");
#endif
            checkConnection();
            EnsureStatement(false);

            bool readColumnNames = true;
			EncodedDataStream dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
            if (isPrepared)
            {
                dataStream.startMessage(Protocol.ExecutePreparedQuery);
                dataStream.encodeInt(handle);
                putParameters(dataStream);

				if (connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION8)
                {

                    /*                    if (columnNames != null)
                                        {
                                            dataStream.encodeInt(Protocol.SkipColumnNames);
                                            readColumnNames = false;
                                        }
                                        else */
                    {
                        dataStream.encodeInt(Protocol.SendColumnNames);
                    }

                }
            }
            else
            {
                dataStream.startMessage(Protocol.ExecuteQuery);
                dataStream.encodeInt(handle);
                dataStream.encodeString(sqlText);
            }
			connection.InternalConnection.sendAndReceive(dataStream);
            return createResultSet(dataStream, readColumnNames);
        }

        public override int ExecuteNonQuery()
        {
            return ExecuteUpdate(false);
        }

        private int ExecuteUpdate(bool generatingKeys)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbCommand.ExecuteNonQuery(" + CommandText + ")");
#endif
            checkConnection();
            EnsureStatement(generatingKeys);

			EncodedDataStream dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
            if (isPrepared)
            {
                dataStream.startMessage(Protocol.ExecutePreparedUpdate);
                dataStream.encodeInt(handle);
                putParameters(dataStream);
            }
            else
            {
                if (generatingKeys)
                {
                    dataStream.startMessage(Protocol.ExecuteUpdateKeys);
                    dataStream.encodeInt(generatingKeys ? 1 : 0);
                }
                else
                    dataStream.startMessage(Protocol.ExecuteUpdate);
                dataStream.encodeInt(handle);
                dataStream.encodeString(sqlText);
            }
			connection.InternalConnection.sendAndReceive(dataStream);
            updateRecordsUpdated(dataStream);

            // V2 txn ID obsolete as of V3 and no longer sending as of V7
			if (connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION2 && connection.InternalConnection.protocolVersion < Protocol.PROTOCOL_VERSION7)
            {
                long txId = dataStream.getLong();
            }

            updateLastCommitInfo(dataStream, generatingKeys);

            return updateCount;
        }

        public override object ExecuteScalar()
        {
            IDataReader reader = ExecuteReader(CommandBehavior.SingleRow);
            if (!reader.Read() || reader.FieldCount < 1)
                return null;
            return reader.GetValue(0);
        }

        public override void Prepare()
        {
            Prepare(false);
        }

        private void Prepare(bool generatingKeys)
        {
            checkConnection();
            Close();

            StringBuilder sqlString = new StringBuilder(sqlText.Length);
            NuoDbDataParameterCollection newParams = new NuoDbDataParameterCollection();
            int state = 0;
            string curParamName = "";
            bool inSingleQuotes = false, inDoubleQuotes = false, inSmartQuotes = false;
            foreach (char c in sqlText)
            {
                if (c == '\'' && !(inDoubleQuotes || inSmartQuotes))
                {
                    inSingleQuotes = !inSingleQuotes;
                    state = 0;
                    sqlString.Append(c);
                    continue;
                }
                else if (c == '\"' && !(inSingleQuotes || inSmartQuotes))
                {
                    inDoubleQuotes = !inDoubleQuotes;
                    state = 0;
                    sqlString.Append(c);
                    continue;
                }
                else if (c == '`' && !(inSingleQuotes || inDoubleQuotes))
                {
                    inSmartQuotes = !inSmartQuotes;
                    state = 0;
                    sqlString.Append(c);
                    continue;
                }
                if (inSingleQuotes || inDoubleQuotes || inSmartQuotes)
                {
                    sqlString.Append(c);
                    continue;
                }

                if (c == '?')
                    state = 1;
                else if (c == '@')
                    state = 2;
                else if (state == 1)
                {
                    if (c == '.')
                        state = 2;
                    else
                    {
                        // either add a new parameter, or carry over the user-provided one
                        if (parameters.Count > newParams.Count)
                            newParams.Add(parameters[newParams.Count]);
                        else
                            newParams.Add(new NuoDbParameter());
                        state = 0;
                        sqlString.Append("?");
                        sqlString.Append(c);
                    }
                }
                else if (state == 2)
                {
                    if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' ||
                        (curParamName.Length > 0 && ((c >= '0' && c <= '9') || c == '-')))
                    {
                        curParamName += c;
                    }
                    else
                    {
                        // if the user-provided parameters have a value for this name, preserve it
                        if (parameters.Contains(curParamName))
                            newParams.Add(parameters[curParamName]);
                        else
                        {
                            NuoDbParameter p = new NuoDbParameter();
                            p.ParameterName = curParamName;
                            newParams.Add(p);
                        }
                        sqlString.Append("?.");
                        sqlString.Append(curParamName);
                        sqlString.Append(c);

                        curParamName = "";
                        state = 0;
                    }
                }
                else
                {
                    state = 0;
                    sqlString.Append(c);
                }
            }
            // handle the case where the SQL statement ended while parsing a parameter 
            if (state == 1)
            {
                // either add a new parameter, or carry over the user-provided one
                if (parameters.Count > newParams.Count)
                    newParams.Add(parameters[newParams.Count]);
                else
                    newParams.Add(new NuoDbParameter());
                sqlString.Append("?");
            }
            else if (state == 2)
            {
                // if the user-provided parameters have a value for this name, preserve it
                if (parameters.Contains(curParamName))
                    newParams.Add(parameters[curParamName]);
                else
                {
                    NuoDbParameter p = new NuoDbParameter();
                    p.ParameterName = curParamName;
                    newParams.Add(p);
                }
                sqlString.Append("?.");
                sqlString.Append(curParamName);
            }

			EncodedDataStream dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
            if (generatingKeys)
            {
                dataStream.startMessage(Protocol.PrepareStatementKeys);
                dataStream.encodeInt(generatingKeys ? 1 : 0);
            }
            else
                dataStream.startMessage(Protocol.PrepareStatement);
            dataStream.encodeString(sqlString.ToString());
			connection.InternalConnection.sendAndReceive(dataStream);
            handle = dataStream.getInt();
			connection.InternalConnection.RegisterCommand(handle);
            int numberParameters = dataStream.getInt();
            // a prepared DDL command fails to execute
            if (numberParameters != 0 || CommandText.TrimStart(null).Substring(0, 6).ToUpper().Equals("SELECT"))
            {
                parameters = newParams;
                isPrepared = true;
                isPreparedWithKeys = generatingKeys;
            }
            else
            {
                Close();
                isPrepared = false;
            }
        }

        public override UpdateRowSource UpdatedRowSource
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
            NuoDbCommand command = new NuoDbCommand();

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
