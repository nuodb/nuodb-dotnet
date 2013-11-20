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
#if NET_40
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NuoDb.Data.Client
{
    internal sealed class ConnectionPoolManager : IDisposable
    {
        static ConnectionPoolManager _instance = new ConnectionPoolManager();
        public static ConnectionPoolManager Instance { get { return _instance; } }

        sealed class ConnectionPool : IDisposable
        {
            sealed class Item : IDisposable
            {
                bool _disposed;

                public DateTimeOffset Created { get; private set; }
                public NuoDbConnectionInternal Connection { get; private set; }

                public Item()
                {
                    _disposed = false;
                }

                public Item(DateTimeOffset created, NuoDbConnectionInternal connection)
                    : this()
                {
                    Created = created;
                    Connection = connection;
                }

                #region IDisposable Members

                public void Dispose()
                {
                    if (_disposed)
                        return;
                    _disposed = true;
                    Created = default(DateTimeOffset);
                    Connection.Dispose();
                    Connection = null;
                }

                #endregion
            }

            bool _disposed;
            object _syncRoot;
            string _connectionString;
            TimeSpan _lifeTime, _maxLifeTime;
            Queue<Item> _available;
            List<NuoDbConnectionInternal> _busy;
            Semaphore _maxConnections;

            public ConnectionPool(string connectionString)
            {
                _disposed = false;
                _syncRoot = new object();
                _connectionString = connectionString;
                NuoDbConnectionStringBuilder connBuilder = new NuoDbConnectionStringBuilder(_connectionString);
                _lifeTime = TimeSpan.FromSeconds(connBuilder.ConnectionLifetimeOrDefault);
                _maxLifeTime = TimeSpan.FromSeconds(connBuilder.MaxLifetimeOrDefault);
                _available = new Queue<Item>();
                _busy = new List<NuoDbConnectionInternal>();
                int maxConn = connBuilder.MaxConnectionsOrDefault;
                _maxConnections = new Semaphore(maxConn, maxConn);
            }

            public NuoDbConnectionInternal GetConnection()
            {
                CheckDisposed();
                _maxConnections.WaitOne();
                lock (_syncRoot)
                {
                    var connection = _available.Any()
                      ? _available.Dequeue().Connection
                      : InitializeNewConnection(_connectionString);
                    _busy.Add(connection);
                    return connection;
                }
            }

            public void ReleaseConnection(NuoDbConnectionInternal connection)
            {
                lock (_syncRoot)
                {
                    var removed = _busy.Remove(connection);
                    if (removed)
                    {
                        DateTimeOffset now = DateTimeOffset.UtcNow;
                        // if it's broken or too old to be recycled, dispose it; otherwise, put in the pool of available connections
                        if (connection.networkErrorOccurred || connection.Created.Add(_maxLifeTime) < now)
                        {
#if DEBUG
                            System.Diagnostics.Trace.WriteLine("ReleaseConnection: discarding connection");
#endif
                            connection.Dispose();
                        }
                        else
                            _available.Enqueue(new Item(now, connection));
                        _maxConnections.Release();
                    }
                }
            }

            public void CleanupPool()
            {
                CheckDisposed();

                Item[] available, keep;
                var now = DateTimeOffset.UtcNow;
                lock (_syncRoot)
                {
                    available = _available.ToArray();
                    keep = available.Where(x => x.Created.Add(_lifeTime) > now).ToArray();
                    _available = new Queue<Item>(keep);
                }
                var release = available.Except(keep);
                foreach (var x in release)
                    x.Connection.Dispose();
            }

            public int GetPooledCount()
            {
                CheckDisposed();

                lock (_syncRoot)
                {
                    return _available.Count() + _busy.Count();
                }
            }

            public void Clear()
            {
                CheckDisposed();

                lock (_syncRoot)
                {
                    ClearConnectionsImpl();
                    _available.Clear();
                    _busy.Clear();
                }
            }

            void CheckDisposed()
            {
                if (_disposed)
                    throw new ObjectDisposedException(typeof(ConnectionPool).Name);
            }

            static NuoDbConnectionInternal InitializeNewConnection(string connectionString)
            {
                var result = new NuoDbConnectionInternal(connectionString);
                result.Open();
                return result;
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;
                lock (_syncRoot)
                {
                    _connectionString = null;
                    _lifeTime = default(TimeSpan);
                    ClearConnectionsImpl();
                    _available = null;
                    _busy = null;
                    _maxConnections.Close();
#if NET_40
                    _maxConnections.Dispose();
#endif
                }
            }

            #endregion

            void ClearConnectionsImpl()
            {
                foreach (var item in _available)
                    item.Dispose();
                foreach (var item in _busy)
                {
                    item.Dispose();
                    _maxConnections.Release();
                }
            }
        }

        bool _disposed;
#if NET_40
        ConcurrentDictionary<string, ConnectionPool> _pools;
#else
        object _syncRoot = new object();
        Dictionary<string, ConnectionPool> _pools;
#endif
        Timer _cleanupTimer;

        ConnectionPoolManager()
        {
            _disposed = false;
#if NET_40
            _pools = new ConcurrentDictionary<string, ConnectionPool>();
#else
            _syncRoot = new object();
            _pools = new Dictionary<string, ConnectionPool>();
#endif
            _cleanupTimer = new Timer(CleanupCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public NuoDbConnectionInternal Get(string connectionString)
        {
            CheckDisposed();

#if NET_40
            return _pools.GetOrAdd(connectionString, PrepareNewPool).GetConnection();
#else
            lock (_syncRoot)
            {
                return GetPoolOrCreateNew(connectionString).GetConnection();
            }
#endif
        }

        public void Release(NuoDbConnectionInternal connection)
        {
#if NET_40
            _pools.GetOrAdd(connection.ConnectionString, PrepareNewPool).ReleaseConnection(connection);
#else
            lock (_syncRoot)
            {
                GetPoolOrCreateNew(connection.ConnectionString).ReleaseConnection(connection);
            }
#endif
        }

#if !NET_40
        ConnectionPool GetPoolOrCreateNew(string connectionString)
        {
            var pool = default(ConnectionPool);
            if (!_pools.TryGetValue(connectionString, out pool))
            {
                pool = PrepareNewPool(connectionString);
            }
            return pool;
        }
#endif

        public int GetPooledConnectionCount(string connectionString)
        {
            CheckDisposed();

            var pool = default(ConnectionPool);
            return _pools.TryGetValue(connectionString, out pool)
              ? pool.GetPooledCount()
              : 0;
        }

        public void ClearPool(string connectionString)
        {
            CheckDisposed();

            var pool = default(ConnectionPool);
            if (_pools.TryGetValue(connectionString, out pool))
            {
                pool.Clear();
            }
        }

        public void ClearAllPools()
        {
            CheckDisposed();

#if NET_40
            _pools.Values.AsParallel().ForAll(p => p.Clear());
#else
            lock (_syncRoot)
            {
                foreach(var x in _pools.Values)
                    x.Clear();
            }
#endif
        }

        ConnectionPool PrepareNewPool(string connectionString)
        {
            var pool = new ConnectionPool(connectionString);
#if !NET_40
            _pools.Add(connectionString, pool);
#endif
            return pool;
        }

        void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(typeof(ConnectionPoolManager).Name);
        }

        void CleanupCallback(object o)
        {
            // in case the timer ticks after dispose, just ignore it
            if (_disposed)
                return;

            Cleanup();
        }

        void Cleanup()
        {
            CheckDisposed(); 

#if NET_40
            _pools.Values.AsParallel().ForAll(p => p.CleanupPool());
#else
            lock (_syncRoot)
            {
                foreach(var x in _pools.Values)
                    x.CleanupPool();
            }
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _cleanupTimer.Dispose();
            _cleanupTimer = null;
#if NET_40
            _pools.Values.AsParallel().ForAll(x => x.Dispose());
#else
            foreach(var x in _pools.Values)
                x.Dispose();
#endif
            _pools = null;
        }

        #endregion
    }
}
