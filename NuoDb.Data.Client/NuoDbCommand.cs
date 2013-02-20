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
using NuoDb.Data.Client.Xml;
using System;
using System.Data;

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
        private int handle = -1;
        internal int updateCount;
        internal NuoDbDataReader generatedKeys;
        private NuoDbDataParameterCollection parameters = new NuoDbDataParameterCollection();
        private bool isPrepared = false;
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

        protected override void Dispose(bool disposing)
        {
            Close();
            base.Dispose(disposing);
        }

        public void Close()
        {
            if (handle == -1 || connection == null || (connection as IDbConnection).State == ConnectionState.Closed ||
                !connection.IsCommandRegistered(handle))
            {
                return;
            }
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbCommand::Close()");
#endif
            connection.CloseCommand(handle);
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
            if (connection.protocolVersion >= Protocol.PROTOCOL_VERSION2 && connection.protocolVersion < Protocol.PROTOCOL_VERSION7 && generatingKeys)
            {
                generatedKeys = createResultSet(dataStream, true);
            }

            // from v3 -v6, last commit info was not being sent if there was a gen key result set
            if ((connection.protocolVersion >= Protocol.PROTOCOL_VERSION3 && !generatingKeys) || connection.protocolVersion >= Protocol.PROTOCOL_VERSION7)
            {
                long transactionId = dataStream.getLong();
                int nodeId = dataStream.getInt();
                long commitSequence = dataStream.getLong();
                connection.setLastTransaction(transactionId, nodeId, commitSequence);
            }

            // from v7 gen key result set is sent after last commit info (if at all)
            if (connection.protocolVersion >= Protocol.PROTOCOL_VERSION7 && generatingKeys)
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
                if (param == null || System.DBNull.Value.Equals(param))
                {
                    dataStream.encodeNull();
                }
                else if (param is string)
                {
                    dataStream.encodeString((string)param);
                }
                else if (param is char)
                {
                    dataStream.encodeString(new string((char)param, 1));
                }
                else if (param is int)
                {
                    dataStream.encodeInt((int)param);
                }
                else if (param is long)
                {
                    dataStream.encodeLong((long)param);
                }
                else if (param is decimal)
                {
                    decimal d = (decimal)param;
                    int scale = 0;
                    while((d % 1) != 0)
                    {
                        scale++;
                        d *= 10;
                    }
                    dataStream.encodeLong((long)d, scale);
                }
                else if (param is bool)
                {
                    dataStream.encodeBoolean((bool)param);
                }
                else if (param is byte)
                {
                    dataStream.encodeInt((byte)param);
                }
                else if (param is short)
                {
                    dataStream.encodeInt((short)param);
                }
                else if (param is float)
                {
                    dataStream.encodeDouble((float)param);
                }
                else if (param is double)
                {
                    dataStream.encodeDouble((double)param);
                }
                else if (param is byte[])
                {
                    dataStream.encodeBytes((byte[])param);
                }
                else if (param is DateTime)
                {
                    dataStream.encodeDate((DateTime)param);
                }
                else if (param is TimeSpan)
                {
                    dataStream.encodeTime((TimeSpan)param);
                }
                else
                {
                    throw new ArgumentException(String.Format("Unsupported type of parameter: {0}", param.GetType().Name), "parameter");
                }
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
#if DEBUG
                System.Diagnostics.Trace.WriteLine("NuoDbCommand.CommandText = " + value);
#endif
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
                return connection.transaction;
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

        private void EnsureStatement()
        {
            // if the connection has been closed and reopened, the statement identified by this handle 
            // has been closed on the server, and we must re-create it
            if (handle == -1 || !connection.IsCommandRegistered(handle))
            {
                if (parameters.Count > 0)
                {
                    Prepare();
                }
                else
                {
                    EncodedDataStream dataStream = new RemEncodedStream(connection.protocolVersion);
                    dataStream.startMessage(Protocol.CreateStatement);
                    connection.sendAndReceive(dataStream);
                    handle = dataStream.getInt();
                    connection.RegisterCommand(handle);
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
                ExecuteUpdate(true);

                return generatedKeys;
            }
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbCommand.ExecuteDbDataReader(" + CommandText + ", " + behavior + ")");
#endif
            checkConnection();
            EnsureStatement();

            bool readColumnNames = true;
            EncodedDataStream dataStream = new RemEncodedStream(connection.protocolVersion);
            if (isPrepared)
            {
                dataStream.startMessage(Protocol.ExecutePreparedQuery);
                dataStream.encodeInt(handle);
                putParameters(dataStream);

                if (connection.protocolVersion >= Protocol.PROTOCOL_VERSION8)
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
            connection.sendAndReceive(dataStream);
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
            EnsureStatement();

            EncodedDataStream dataStream = new RemEncodedStream(connection.protocolVersion);
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
            connection.sendAndReceive(dataStream);
            updateRecordsUpdated(dataStream);

            // V2 txn ID obsolete as of V3 and no longer sending as of V7
            if (connection.protocolVersion >= Protocol.PROTOCOL_VERSION2 && connection.protocolVersion < Protocol.PROTOCOL_VERSION7)
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
            checkConnection();
            Close();

            EncodedDataStream dataStream = new RemEncodedStream(connection.protocolVersion);
            dataStream.startMessage(Protocol.PrepareStatement);
            dataStream.encodeString(sqlText);
            connection.sendAndReceive(dataStream);
            handle = dataStream.getInt();
            connection.RegisterCommand(handle);
            int numberParameters = dataStream.getInt();
            for (int i = parameters.Count; i < numberParameters; i++)
                parameters.Add(CreateParameter());
            for (int i = parameters.Count; i > numberParameters; i--)
                parameters.RemoveAt(i-1);
            isPrepared = true;
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

            foreach (NuoDbParameter p in this.Parameters)
            {
                command.Parameters.Add(((ICloneable)p).Clone());
            }

            return command;
        }

        #endregion
    }
}
