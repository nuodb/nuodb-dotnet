/****************************************************************************
*	Author: Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NuoDb.Data.Client
{
	internal sealed class ConnectionPoolManager
	{
		static ConnectionPoolManager _instance = new ConnectionPoolManager();
		public static ConnectionPoolManager Instance { get { return _instance; } }

		sealed class ConnectionPool
		{
			sealed class Item
			{
				public DateTimeOffset Created { get; private set; }
				public NuoDbConnectionInternal Connection { get; private set; }

				public Item(DateTimeOffset created, NuoDbConnectionInternal connection)
				{
					Created = created;
					Connection = connection;
				}
			}

			object _syncRoot;
			string _connectionString;
			TimeSpan _lifeTime;
			Queue<Item> _available;
			List<NuoDbConnectionInternal> _busy;

			public ConnectionPool(string connectionString)
			{
				_syncRoot = new object();
				_connectionString = connectionString;
				_lifeTime = TimeSpan.FromSeconds(new NuoDbConnectionStringBuilder(_connectionString).ConnectionLifetimeOrDefault);
				_available = new Queue<Item>();
				_busy = new List<NuoDbConnectionInternal>();
			}

			public NuoDbConnectionInternal GetConnection()
			{
				lock (_syncRoot)
				{
					var connection = _available.Count() > 0
						? _available.Dequeue().Connection
						: new NuoDbConnectionInternal(_connectionString);
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
						_available.Enqueue(new Item(DateTimeOffset.UtcNow, connection));
				}
			}

			public void CleanupPool()
			{
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
		}

		object _syncRoot = new object();
		Dictionary<string, ConnectionPool> _pools;
		Timer _cleanupTimer;

		ConnectionPoolManager()
		{
			_syncRoot = new object();
			_pools = new Dictionary<string, ConnectionPool>();
			_cleanupTimer = new Timer(CleanupCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
		}

		public NuoDbConnectionInternal Get(string connectionString)
		{
			lock (_syncRoot)
			{
				return GetPoolOrCreateNew(connectionString).GetConnection();
			}
		}

		public void Release(NuoDbConnectionInternal connection)
		{
			lock (_syncRoot)
			{
				GetPoolOrCreateNew(connection.ConnectionString).ReleaseConnection(connection);
			}
		}

		ConnectionPool GetPoolOrCreateNew(string connectionString)
		{
			var pool = default(ConnectionPool);
			if (!_pools.TryGetValue(connectionString, out pool))
			{
				pool = PrepareNewPool(connectionString);
			}
			return pool;
		}

		ConnectionPool PrepareNewPool(string connectionString)
		{
			var pool = new ConnectionPool(connectionString);
			_pools.Add(connectionString, pool);
			return pool;
		}

		void CleanupCallback(object o)
		{
			Cleanup();
		}

		void Cleanup()
		{
			lock (_syncRoot)
			{
				_pools.Values.AsParallel().ForAll(p => p.CleanupPool());
			}
		}
	}
}
