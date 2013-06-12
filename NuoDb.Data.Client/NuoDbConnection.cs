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
		NuoDbConnectionInternal _internalConnection;

		public NuoDbConnection()
		{ }

		public NuoDbConnection(string connectionString)
		{
			ConnectionString = connectionString;
		}

		protected override void Dispose(bool disposing)
		{
			if (_internalConnection != null)
			{
				_internalConnection.Dispose(disposing);
				_internalConnection = null;
			}
		}

		public override void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			InternalConnection.EnlistTransaction(transaction);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return InternalConnection.BeginDbTransaction(isolationLevel);
		}

		public override void ChangeDatabase(string databaseName)
		{
			InternalConnection.ChangeDatabase(databaseName);
		}

		public override void Open()
		{
			throw new NotImplementedException();
		}

		public override void Close()
		{
			throw new NotImplementedException();
		}

		protected override DbCommand CreateDbCommand()
        {
            return new NuoDbCommand(this);
		}

		public override int ConnectionTimeout
		{
			get { return InternalConnection.ConnectionTimeout; }
		}

		public override string ConnectionString
		{
			get { return ConnectionString; }
			set
			{
				ConnectionString = value;
				if (InternalConnection != null)
					InternalConnection.ConnectionString = ConnectionString;
			}
		}

		public override DataTable GetSchema()
		{
			return InternalConnection.GetSchema();
		}

		public override DataTable GetSchema(string collectionName)
		{
			return InternalConnection.GetSchema(collectionName);
		}

		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			return InternalConnection.GetSchema(collectionName, restrictionValues);
		}

		internal NuoDbConnectionInternal InternalConnection
		{
			get
			{
				if (_internalConnection == null)
					throw new InvalidOperationException();
				return _internalConnection;
			}
		}

		public override string DataSource
		{
#warning Should work without InternalConnection
			get { return InternalConnection.DataSource; }
		}

		public override string Database
		{
#warning Should work without InternalConnection
			get { return InternalConnection.Database; }
		}

		public override string ServerVersion
		{
#warning Should work without InternalConnection
			get { return InternalConnection.ServerVersion; }
		}

		public override ConnectionState State
		{
#warning Should work without InternalConnection
			get { return InternalConnection.State; }
		}

		protected override DbProviderFactory DbProviderFactory
		{
			get { return NuoDbProviderFactory.Instance; }
		}

		public object Clone()
		{
			return new NuoDbConnection(ConnectionString);
		}
	}
}
