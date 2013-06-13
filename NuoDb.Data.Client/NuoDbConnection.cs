/****************************************************************************
*	Author: Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace NuoDb.Data.Client
{
	// this declaration prevents VS2010 from trying to edit this class with the designer.
	// it would try to do that because the parent class derives from Component, but it's abstract
	// and it cannot be instanciated
	[DesignerCategory("")]
	public sealed class NuoDbConnection : DbConnection, ICloneable
	{
		bool _disposed;
		ConnectionState _state;
		string _connectionString;
		NuoDbConnectionStringBuilder _parsedConnectionString;
		NuoDbConnectionInternal _internalConnection;

		public NuoDbConnection()
		{
			_disposed = false;
			_state = ConnectionState.Closed;
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

			OnStateChange(_state, ConnectionState.Connecting);

			if (_parsedConnectionString.PoolingOrDefault)
			{
				_internalConnection = ConnectionPoolManager.Instance.Get(_connectionString);
			}
			else
			{
				_internalConnection = new NuoDbConnectionInternal(_connectionString);
			}
			try
			{
				_internalConnection.Open();
			}
			catch
			{
				CloseImpl();

				throw;
			}

			OnStateChange(_state, ConnectionState.Open);
		}

		public override void Close()
		{
			CheckDisposed();

			CloseImpl();
		}

		public void CloseImpl()
		{
			if (_internalConnection != null)
			{
				ConnectionPoolManager.Instance.Release(_internalConnection);
				_internalConnection.Close();
				_internalConnection = null;

				OnStateChange(_state, ConnectionState.Closed);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing)
			{
				CloseImpl();
			}
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
			CheckConnection();

			return _internalConnection.GetSchema();
		}

		public override DataTable GetSchema(string collectionName)
		{
			CheckConnection();

			return _internalConnection.GetSchema(collectionName);
		}

		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			CheckConnection();

			return _internalConnection.GetSchema(collectionName, restrictionValues);
		}

		internal NuoDbConnectionInternal InternalConnection
		{
			get
			{
				CheckConnection(false);

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

				return _internalConnection.ServerVersion;
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
				throw new ObjectDisposedException(this.GetType().Name);
		}

		void CheckConnection(bool openedNeeded = true)
		{
			CheckDisposed();
			if (openedNeeded && State != ConnectionState.Open)
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
	}
}
