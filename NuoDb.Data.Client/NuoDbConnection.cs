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
		string _connectionString;
		NuoDbConnectionStringBuilder _parsedConnectionString;
		NuoDbConnectionInternal _internalConnection;

		public NuoDbConnection()
		{
			_disposed = false;
		}

		public NuoDbConnection(string connectionString)
			: this()
		{
			ConnectionString = connectionString;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Close();
			}
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
			_internalConnection = new NuoDbConnectionInternal();
			_internalConnection.ConnectionString = _connectionString;
			_internalConnection.Open();
		}

		public override void Close()
		{
			_internalConnection.Close();
			_internalConnection = null;
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
			get { return _internalConnection != null ? _internalConnection.State : ConnectionState.Closed; }
		}

		protected override DbProviderFactory DbProviderFactory
		{
			get { return NuoDbProviderFactory.Instance; }
		}

		public object Clone()
		{
			return new NuoDbConnection(ConnectionString);
		}

		void CheckConnection(bool openedNeeded = true)
		{
			if (_disposed)
				throw new ObjectDisposedException(this.GetType().Name);
			if (openedNeeded && State != ConnectionState.Open)
				throw new InvalidOperationException("Open connection is needed.");
			if (_internalConnection == null)
				throw new InvalidOperationException();
		}
	}
}
