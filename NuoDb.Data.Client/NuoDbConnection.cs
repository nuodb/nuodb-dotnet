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
* 
*	Author: Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System;
using System.Data;
using System.Data.Common;

namespace NuoDb.Data.Client
{
    // this declaration prevents VS2010 from trying to edit this class with the designer.
    // it would try to do that because the parent class derives from Component, but it's abstract
    // and it cannot be instanciated
    [System.ComponentModel.DesignerCategory("")]
    public sealed class NuoDbConnection : DbConnection, ICloneable
    {
        bool _disposed;
        ConnectionState _state;
        string _connectionString;
        NuoDbConnectionStringBuilder _parsedConnectionString;
        NuoDbConnectionInternal _internalConnection;
        bool _pooled;

        public NuoDbConnection()
        {
            _disposed = false;
            _state = ConnectionState.Closed;
            _pooled = false;
        }

        public NuoDbConnection(string connectionString)
            : this()
        {
            ConnectionString = connectionString;
        }

        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            CheckConnection();

            _internalConnection.EnlistTransaction(transaction);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            CheckConnection();

            return _internalConnection.BeginDbTransaction(isolationLevel);
        }

        public override void ChangeDatabase(string databaseName)
        {
            CheckConnection();

            _internalConnection.ChangeDatabase(databaseName);
        }

        public override void Open()
        {
            CheckDisposed();

            if (_state != ConnectionState.Closed)
                throw new InvalidOperationException();

            OnStateChange(_state, ConnectionState.Connecting);

            if (_parsedConnectionString.PoolingOrDefault)
            {
                _pooled = true;
                try
                {
                    _internalConnection = ConnectionPoolManager.Instance.Get(_connectionString);
                }
                catch
                {
                    CloseImpl();

                    throw;
                }
            }
            else
            {
                _internalConnection = new NuoDbConnectionInternal(_connectionString);
                try
                {
                    _internalConnection.Open();
                }
                catch
                {
                    CloseImpl();

                    throw;
                }
            }
            _internalConnection.Owner = this;

            OnStateChange(_state, ConnectionState.Open);
        }

        public override void Close()
        {
            CheckDisposed();

            CloseImpl();
        }

        void CloseImpl()
        {
            if (_internalConnection != null)
            {
                _internalConnection.Rollback();
                if (_pooled)
                {
                    _internalConnection.CloseOpenItems();
                    ConnectionPoolManager.Instance.Release(_internalConnection);
                }
                else
                    _internalConnection.Close();
                _pooled = false;
                _internalConnection = null;
            }

            OnStateChange(_state, ConnectionState.Closed);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbConnection::Dispose()");
#endif
            if (disposing)
            {
                CloseImpl();
            }

            _disposed = true;
        }

        protected override DbCommand CreateDbCommand()
        {
            return new NuoDbCommand(this);
        }

        public override int ConnectionTimeout
        {
            get { return 0; }
        }

        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                _parsedConnectionString = new NuoDbConnectionStringBuilder(ConnectionString);
                if (_internalConnection != null)
                    _internalConnection.ConnectionString = ConnectionString;
            }
        }

        public override DataTable GetSchema()
        {
            return NuoDbConnectionInternal.GetSchemaHelper(ConnectionString, "", null);
        }

        public override DataTable GetSchema(string collectionName)
        {
            return NuoDbConnectionInternal.GetSchemaHelper(ConnectionString, collectionName, null);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return NuoDbConnectionInternal.GetSchemaHelper(ConnectionString, collectionName, restrictionValues);
        }

        internal NuoDbConnectionInternal InternalConnection
        {
            get
            {
                CheckConnection();

                return _internalConnection;
            }
        }

        public override string DataSource
        {
            get { return _parsedConnectionString.Server; }
        }

        public override string Database
        {
            get { return _parsedConnectionString.Database; }
        }

        public override string ServerVersion
        {
            get
            {
                CheckConnection();

                return InternalConnection.ServerVersion;
            }
        }

        public override ConnectionState State
        {
            get { return _state; }
        }

        public override event StateChangeEventHandler StateChange;

        protected override DbProviderFactory DbProviderFactory
        {
            get { return NuoDbProviderFactory.Instance; }
        }

        public object Clone()
        {
            return new NuoDbConnection(ConnectionString);
        }

        void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(typeof(NuoDbConnection).Name);
        }

        void CheckConnection(bool openedNeeded = true)
        {
            CheckDisposed();
            if (openedNeeded && _state != ConnectionState.Open)
                throw new InvalidOperationException("Open connection is needed.");
            if (_internalConnection == null)
                throw new InvalidOperationException();
        }

        void OnStateChange(ConnectionState originalState, ConnectionState currentState)
        {
            _state = currentState;
            if (StateChange != null)
                StateChange(this, new StateChangeEventArgs(originalState, currentState));
        }

        public static int GetPooledConnectionCount(NuoDbConnection connection)
        {
            return GetPooledConnectionCount(connection.ConnectionString);
        }

        public static int GetPooledConnectionCount(String connectionString)
        {
            return ConnectionPoolManager.Instance.GetPooledConnectionCount(connectionString);
        }

        public static void ClearPool(NuoDbConnection connection)
        {
            ClearPool(connection.ConnectionString);
        }

        public static void ClearPool(String connectionString)
        {
            ConnectionPoolManager.Instance.ClearPool(connectionString);
        }

        public static void ClearAllPools()
        {
            ConnectionPoolManager.Instance.ClearAllPools();
        }
    }
}
