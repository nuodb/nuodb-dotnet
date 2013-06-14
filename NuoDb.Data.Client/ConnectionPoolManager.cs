/****************************************************************************
*	Author: Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System;
using System.Collections.Concurrent;
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

				public void Dispose()
				{
					if (_disposed)
						return;
					_disposed = true;
					Created = default(DateTime);
					Connection.Dispose();
					Connection = null;
				}
			}

			bool _disposed;
			object _syncRoot;
			string _connectionString;
			TimeSpan _lifeTime;
			Queue<Item> _available;
			List<NuoDbConnectionInternal> _busy;

			public ConnectionPool(string connectionString)
			{
				_disposed = false;
				_syncRoot = new object();
				_connectionString = connectionString;
				_lifeTime = TimeSpan.FromSeconds(new NuoDbConnectionStringBuilder(_connectionString).ConnectionLifetimeOrDefault);
				_available = new Queue<Item>();
				_busy = new List<NuoDbConnectionInternal>();
			}

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
				}
			}

			void ClearConnectionsImpl()
			{
				foreach (var item in _available)
					item.Dispose();
				foreach (var item in _busy)
					item.Dispose();
			}

			public NuoDbConnectionInternal GetConnection()
			{
				CheckDisposed();

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
				CheckDisposed();

				lock (_syncRoot)
				{
					var removed = _busy.Remove(connection);
					if (removed)
						_available.Enqueue(new Item(DateTimeOffset.UtcNow, connection));
				}
			}

			public void CleanupPool()
			{
				CheckDisposed();

				lock (_syncRoot)
				{
					var now = DateTimeOffset.UtcNow;
					var available = _available.ToArray();
					var keep = available.Where(x => x.Created.Add(_lifeTime) > now);
					var release = available.Except(keep);
					release.AsParallel().ForAll(x => x.Connection.Dispose());
					_available = new Queue<Item>(keep);
				}
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
		}

		bool _disposed;
		ConcurrentDictionary<string, ConnectionPool> _pools;
		Timer _cleanupTimer;

		ConnectionPoolManager()
		{
			_disposed = false;
			_pools = new ConcurrentDictionary<string, ConnectionPool>();
			_cleanupTimer = new Timer(CleanupCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
		}

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;
			_cleanupTimer.Dispose();
			_cleanupTimer = null;
			_pools.Values.AsParallel().ForAll(x => x.Dispose());
			_pools = null;
		}

		public NuoDbConnectionInternal Get(string connectionString)
		{
			CheckDisposed();

			return _pools.GetOrAdd(connectionString, PrepareNewPool).GetConnection();
		}

		public void Release(NuoDbConnectionInternal connection)
		{
			CheckDisposed();

			_pools.GetOrAdd(connection.ConnectionString, PrepareNewPool).ReleaseConnection(connection);
		}

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

			_pools.Values.AsParallel().ForAll(p => p.Clear());
		}

		static ConnectionPool PrepareNewPool(string connectionString)
		{
			var pool = new ConnectionPool(connectionString);
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

			_pools.Values.AsParallel().ForAll(p => p.CleanupPool());
		}
	}
}
