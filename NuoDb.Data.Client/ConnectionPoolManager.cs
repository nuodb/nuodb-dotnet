﻿/****************************************************************************
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
					var connection = _available.Any()
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

		ConcurrentDictionary<string, ConnectionPool> _pools;
		Timer _cleanupTimer;

		ConnectionPoolManager()
		{
			_pools = new ConcurrentDictionary<string, ConnectionPool>();
			_cleanupTimer = new Timer(CleanupCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
		}

		public NuoDbConnectionInternal Get(string connectionString)
		{
			return _pools.GetOrAdd(connectionString, PrepareNewPool).GetConnection();
		}

		public void Release(NuoDbConnectionInternal connection)
		{
			_pools.GetOrAdd(connection.ConnectionString, PrepareNewPool).ReleaseConnection(connection);
		}

		static ConnectionPool PrepareNewPool(string connectionString)
		{
			var pool = new ConnectionPool(connectionString);
			return pool;
		}

		void CleanupCallback(object o)
		{
			Cleanup();
		}

		void Cleanup()
		{
			// should we remove empty pools?
			_pools.Values.AsParallel().ForAll(p => p.CleanupPool());
		}
	}
}