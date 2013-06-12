/****************************************************************************
*	Author: Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuoDb.Data.Client
{
	internal class ConnectionPoolManager
	{
		internal static ConnectionPoolManager Instance = new ConnectionPoolManager();

		sealed class ConnectionPool
		{
			object _syncRoot;
			string _connectionString;
			Queue<NuoDbConnectionInternal> _available;
			List<NuoDbConnectionInternal> _busy;

			public ConnectionPool(string connectionString)
			{
				_syncRoot = new object();
				_connectionString = connectionString;
				_available = new Queue<NuoDbConnectionInternal>();
				_busy = new List<NuoDbConnectionInternal>();
			}

			public NuoDbConnectionInternal GetConnection()
			{
				lock (_syncRoot)
				{
					var connection = _available.Count() > 0
						? _available.Dequeue()
						: new NuoDbConnectionInternal(_connectionString);
					_busy.Add(connection);
					return connection;
				}
			}

			public void ReleaseConnection(NuoDbConnectionInternal connection)
			{
				lock (_syncRoot)
				{
					_busy.Remove(connection);
					_available.Enqueue(connection);
				}
			}
		}

		object _syncRoot = new object();
		Dictionary<string, ConnectionPool> _pools;

		ConnectionPoolManager()
		{
			_syncRoot = new object();
			_pools = new Dictionary<string, ConnectionPool>();
		}

		public NuoDbConnectionInternal Get(string connectionString)
		{
			lock (_syncRoot)
			{
				var pool = default(ConnectionPool);
				if (!_pools.TryGetValue(connectionString, out pool))
				{
					pool = new ConnectionPool(connectionString);
					_pools.Add(connectionString, pool);
				}
				return pool.GetConnection();
			}
		}

		public void Release(NuoDbConnectionInternal connection)
		{
			lock (_syncRoot)
			{
				var pool = default(ConnectionPool);
				if (!_pools.TryGetValue(connection.ConnectionString, out pool))
					throw new InvalidOperationException("This connection was not taken from pool.");
				pool.ReleaseConnection(connection);
			}
		}
	}
}
