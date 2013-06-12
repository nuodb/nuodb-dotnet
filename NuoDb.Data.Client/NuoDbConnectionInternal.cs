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
* Contributors:
*	Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using NuoDb.Data.Client.Xml;
using NuoDb.Data.Client.Net;
using NuoDb.Data.Client.Security;
using NuoDb.Data.Client.Util;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Data;

namespace NuoDb.Data.Client
{
	internal sealed class NuoDbConnectionInternal : IDisposable
	{		
		internal DbTransaction transaction;
		internal const int PORT = 48004;
		internal const string LAST_INFO_SEPARATOR = ";";
		internal const string DEFAULT_CIPHER = "RC4";

		NuoDbConnection owner;

		private string connectionString;
		private StringDictionary properties;

		private static ProcessConnection processConnections = null;

		private CryptoSocket socket;
		private CryptoInputStream inputStream;
		private CryptoOutputStream outputStream;
		private EncodedDataStream dataStream;
		internal int protocolVersion = Protocol.PROTOCOL_VERSION;
		private ProcessConnection processConnection = null;
		private bool authenticating = false;
		private string serverAddress;
		private int serverPort;
		private ConnectionState state = ConnectionState.Closed;
		private string lastBroker;

		private List<int> listResultSets = new List<int>();
		private List<int> listCommands = new List<int>();

		public event StateChangeEventHandler StateChange;

		public NuoDbConnectionInternal()
		{
		}

		public NuoDbConnectionInternal(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Dispose()
		{
			Dispose(true);
		}
		internal void Dispose(bool disposing)
		{
#if DEBUG
			System.Diagnostics.Trace.WriteLine("NuoDBConnection::DisposeImpl() [state = " + state + "]");
#endif
			try
			{
				if (state == ConnectionState.Open)
					Close();
			}
			catch (Exception)
			{
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void sendAsync(EncodedDataStream stream)
		{
			if (protocolVersion < Protocol.PROTOCOL_VERSION8)
			{
				sendAndReceive(stream);

				return;
			}

			try
			{
				lock (this)
				{
					stream.send(outputStream);
				}
			}
			catch (IOException exception)
			{
				throw new NuoDbSqlException(exception);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void sendAndReceive(EncodedDataStream stream)
		{
			try
			{
				lock (this)
				{
					stream.send(outputStream);
					stream.getMessage(inputStream);
					int status = stream.getInt();

					if (status != 0)
					{
						string message = stream.getString();
						string sqlState = "";

						if (protocolVersion >= Protocol.PROTOCOL_VERSION2)
						{
							sqlState = stream.getString();
						}

						// If empty string, use the state from NuoDbSqlCode

						if (StringUtils.size(sqlState) == 0)
						{
							sqlState = NuoDbSqlCode.FindSQLState(status);
						}

						throw new NuoDbSqlException(message, sqlState, status);
					}
				}
			}
			catch (IOException exception)
			{
				throw new NuoDbSqlException(exception);
			}
		}

		private ProcessConnection getProcessConnection(string databaseName)
		{
			for (processConnection = processConnections; processConnection != null && !processConnection.isThisDatabase(databaseName); processConnection = processConnection.Next)
			{
				;
			}

			if (processConnection == null)
			{
				processConnections = processConnection = new ProcessConnection(databaseName, processConnections);
			}

			return processConnection;
		}

		internal void setLastTransaction(long transactionId, int nodeId, long commitSequence)
		{
			processConnection.setLast(transactionId, nodeId, commitSequence);
		}

		internal class ProcessConnection
		{
			private readonly string dbName;
			private Guid databaseUUId;
			private readonly ProcessConnection next;
			private LastCommitInfo infos;

			internal ProcessConnection(string dbName, ProcessConnection next)
			{
				this.dbName = dbName;
				this.next = next;
			}

			public virtual Guid DatabaseUUId
			{
				set
				{
					if (this.databaseUUId != null && !this.databaseUUId.Equals(value))
					{
						for (LastCommitInfo lastCommitInfo = infos; lastCommitInfo != null; lastCommitInfo = lastCommitInfo.next)
						{
							lastCommitInfo.reset();
						}
					}

					this.databaseUUId = value;
				}
				get
				{
					return databaseUUId;
				}
			}


			public virtual bool isThisDatabase(string database)
			{
				if (dbName != null)
				{
					return dbName.Equals(database);
				}

				return false;
			}

			public virtual ProcessConnection Next
			{
				get
				{
					return next;
				}
			}

			public virtual void setLast(long lastTxnId, int nodeId, long commitSequence)
			{
				if (commitSequence <= 0 || lastTxnId <= 0 || nodeId <= 0)
				{
					return;
				}

				LastCommitInfo lastCommitInfo = getCommitInfo(nodeId);

				if (lastCommitInfo == null)
				{
					lock (this)
					{
						lastCommitInfo = getCommitInfo(nodeId);

						if (lastCommitInfo == null)
						{
							infos = lastCommitInfo = new LastCommitInfo(infos);
						}
					}
				}

				lastCommitInfo.setLast(lastTxnId, nodeId, commitSequence);
			}

			public virtual string getLastCommitInfo()
			{
				StringBuilder lastCommitParam = new StringBuilder();
				getLastCommitInfo(lastCommitParam, infos, 0);
				return lastCommitParam.ToString();
			}

			private void getLastCommitInfo(StringBuilder lastCommitParam, LastCommitInfo lastCommitInfo, int size)
			{
				// We are at the end, encode the size

				if (lastCommitInfo == null)
				{
					lastCommitParam.Append(Convert.ToString(size));
					lastCommitParam.Append(LAST_INFO_SEPARATOR);
				}
				else
				{

					// Recurse into each LastCommitInfo instance so we can find out the size ...

					getLastCommitInfo(lastCommitParam, lastCommitInfo.next, ++size);

					// ... then include this node's info

					lastCommitInfo.getLastCommitInfo(lastCommitParam);
				}
			}

			private LastCommitInfo getCommitInfo(int nodeId)
			{
				for (LastCommitInfo info = infos; info != null; info = info.next)
				{
					if (info.nodeId == nodeId)
					{
						return info;
					}
				}

				return null;
			}

			internal class LastCommitInfo
			{
				internal long transactionId;
				internal long commitSequence;
				internal int nodeId;
				internal readonly LastCommitInfo next;

				internal LastCommitInfo(LastCommitInfo next)
				{
					this.next = next;
				}

				// We need to synchronize because all three pieces must be kept together when sending

				[MethodImpl(MethodImplOptions.Synchronized)]
				internal virtual void setLast(long lastTxnId, int nodeId, long commitSequence)
				{
					// A thread (A) could have committed the update but another thread (B) beat it to
					// updating the sequence even though the thread A committed first. No matter, we only
					// ever want the last committed sequence.

					if (commitSequence > this.commitSequence)
					{
						this.transactionId = lastTxnId;
						this.commitSequence = commitSequence;
						this.nodeId = nodeId;
					}
				}

				[MethodImpl(MethodImplOptions.Synchronized)]
				internal virtual void reset()
				{
					commitSequence = 0;
					transactionId = 0;
				}

				[MethodImpl(MethodImplOptions.Synchronized)]
				internal virtual void getLastCommitInfo(StringBuilder lastCommitParam)
				{
					lastCommitParam.Append(Convert.ToString(transactionId));
					lastCommitParam.Append(LAST_INFO_SEPARATOR);
					lastCommitParam.Append(Convert.ToString(nodeId));
					lastCommitParam.Append(LAST_INFO_SEPARATOR);
					lastCommitParam.Append(Convert.ToString(commitSequence));
					lastCommitParam.Append(LAST_INFO_SEPARATOR);
				}

			}
		}

		public int ConnectionTimeout
		{
			get { return 0; }
		}

		public void EnlistTransaction(System.Transactions.Transaction txn)
		{
#if DEBUG
			System.Diagnostics.Trace.WriteLine("NuoDBConnection::EnlistTransaction(" + transaction + ")");
#endif
			if (transaction == null)
				transaction = BeginDbTransaction(IsolationLevel.ReadCommitted);
			txn.EnlistVolatile(transaction as NuoDbTransaction, System.Transactions.EnlistmentOptions.None);
			//base.EnlistTransaction(transaction);
		}

		internal DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
#if DEBUG
			System.Diagnostics.Trace.WriteLine("NuoDBConnection::BeginDbTransactionImpl(" + isolationLevel + ")");
#endif
			if (transaction != null)
				throw new InvalidOperationException("A transaction has already been started");

			// the server expects an integer modeled after the Java constants
			// TRANSACTION_READ_UNCOMMITTED = 1;
			// TRANSACTION_READ_COMMITTED = 2;
			// TRANSACTION_REPEATABLE_READ = 4;
			// TRANSACTION_SERIALIZABLE = 8;
			int level = -1;
			switch (isolationLevel)
			{
				case IsolationLevel.Unspecified: level = 0; break;
				case IsolationLevel.ReadUncommitted: level = 1; break;
				case IsolationLevel.ReadCommitted: level = 2; break;
				case IsolationLevel.RepeatableRead: level = 4; break;
				case IsolationLevel.Serializable: level = 8; break;
				default:
					throw new NotSupportedException("The specified transaction isolation level is not supported");
			}
			// turn off auto-commit
			dataStream.startMessage(Protocol.SetAutoCommit);
			dataStream.encodeBoolean(false);
			sendAsync(dataStream);

			if (level != 0)
			{
				dataStream.startMessage(Protocol.SetTransactionIsolation);
				dataStream.encodeInt(level);
				sendAndReceive(dataStream);
			}

			transaction = new NuoDbTransaction(Owner, isolationLevel);
			return transaction;
		}

		public void ChangeDatabase(string databaseName)
		{
#if DEBUG
			System.Diagnostics.Trace.WriteLine("NuoDBConnection::ChangeDatabase(" + databaseName + ")");
#endif
			if (state == ConnectionState.Open)
				Close();
			properties["database"] = databaseName;
		}

		internal void RegisterCommand(int handle)
		{
			listCommands.Add(handle);
		}

		internal bool IsCommandRegistered(int handle)
		{
			return listCommands.Contains(handle);
		}

		internal void CloseCommand(int handle)
		{
			if (socket == null || !socket.Connected)
			{
				return;
			}
			EncodedDataStream dataStream = new EncodedDataStream();
			dataStream.startMessage(Protocol.CloseStatement);
			dataStream.encodeInt(handle);
			sendAsync(dataStream);

			if (listCommands.Contains(handle))
				listCommands.Remove(handle);
		}

		internal void RegisterResultSet(int handle)
		{
			listResultSets.Add(handle);
		}

		internal bool IsResultSetRegistered(int handle)
		{
			return listResultSets.Contains(handle);
		}

		internal void CloseResultSet(int handle)
		{
			if (socket == null || !socket.Connected)
			{
				return;
			}
			EncodedDataStream dataStream = new EncodedDataStream();
			dataStream.startMessage(Protocol.CloseResultSet);
			dataStream.encodeInt(handle);
			sendAsync(dataStream);

			if (listResultSets.Contains(handle))
				listResultSets.Remove(handle);
		}

		public void Close()
		{
			if (socket == null || !socket.Connected)
			{
				return;
			}
			this.OnStateChange(this.state, ConnectionState.Closed);

#if DEBUG
			System.Diagnostics.Trace.WriteLine("NuoDBConnection::Close()");
#endif

			List<int> tmpResultSet = new List<int>(listResultSets);
			listResultSets.Clear();

			foreach (Int32 r in tmpResultSet)
			{
				CloseResultSet(r);
			}

			List<int> tmpCommand = new List<int>(listCommands);
			listCommands.Clear();

			foreach (Int32 r in tmpCommand)
			{
				CloseCommand(r);
			}

			dataStream.startMessage(Protocol.CloseConnection);
			sendAndReceive(dataStream);

			try
			{
				socket.Close();
			}
			catch (IOException e)
			{
				throw new NuoDbSqlException(e.Message);
			}
			finally
			{
				socket = null;
			}
		}

		public string ConnectionString
		{
			get
			{
				return connectionString;
			}
			set
			{
				connectionString = value;
				properties = new StringDictionary();
				if (connectionString != null && connectionString.Length > 0)
				{
					const string regex = "([\\w\\s\\d]+)\\s*=\\s*(\"([^\"]*)\"|'([^']*)'|([^\"';]*))";
					MatchCollection tokens = Regex.Matches(connectionString, regex);
					foreach (Match keyPair in tokens)
					{
						// each group is built from the expression between ( and ) in the regex
						// group 0 is the entire match
						// group 1 is the property name
						// group 2 is the unprocessed property value (the OR of the three possible ways of specifying the property value)
						// group 3 is the property value enclosed in ""
						// group 4 is the property value enclosed in ''
						// group 5 is the property value not enclosed in quotes
						if (keyPair.Groups[0].Success && keyPair.Groups[1].Success)
						{
							string kKey = keyPair.Groups[1].Value.ToLowerInvariant().Trim();
							string kValue = keyPair.Groups[3].Success ? keyPair.Groups[3].Value :
												keyPair.Groups[4].Success ? keyPair.Groups[4].Value :
													keyPair.Groups[5].Success ? keyPair.Groups[5].Value.Trim() :
														string.Empty;
							properties.Add(kKey, kValue);
						}
					}
				}
			}
		}

		public string DataSource
		{
			get { return properties["server"]; }
		}

		public string Database
		{
			get { return properties["database"]; }
		}

		private void doOpen(string hostName)
		{
			string databaseName = properties["database"];

			int index = hostName.IndexOf(':');
			int port = PORT;
			if (index != -1)
			{
				try
				{
					port = Int32.Parse(hostName.Substring(index + 1));
				}
				catch (FormatException e)
				{
					throw new ArgumentException("Invalid port number in connection string", "ConnectionString", e);
				}
				hostName = hostName.Substring(0, index);
			}
			dataStream = new EncodedDataStream();
			authenticating = false;

			try
			{
				Tag tag = new Tag("Connection");
				tag.addAttribute("Service", "SQL2");
				tag.addAttribute("Database", databaseName);
				string userName = null;
				string password = null;
				string cipher = DEFAULT_CIPHER;

				foreach (DictionaryEntry property in properties)
				{
					string name = (string)property.Key;
					string value = (string)property.Value;

					if (name.Equals("user"))
					{
						userName = value;
						tag.addAttribute("User", userName);
					}
					else if (name.Equals("password"))
					{
						password = value;
					}
					else if (name.Equals("schema"))
					{
						tag.addAttribute("Schema", value);
					}
					else if (name.Equals("cipher"))
					{
						cipher = value;
					}
					else
					{
						tag.addAttribute(name, value);
					}
				}
				// see comment below ... for now these are the only two types that
				// we can support in the client code

				if ((!cipher.Equals("RC4")) && (!cipher.Equals("None")))
					throw new NuoDbSqlException("Unknown cipher: " + cipher);

				tag.addAttribute("Cipher", cipher);

				this.OnStateChange(this.state, ConnectionState.Connecting);

				string xml = tag.ToString();
				CryptoSocket brokerSocket = new CryptoSocket(hostName, port);
				inputStream = brokerSocket.InputStream;
				outputStream = brokerSocket.OutputStream;

				dataStream.write(xml);
				dataStream.send(outputStream);

				dataStream.getMessage(inputStream);
				string response = dataStream.readString();
				brokerSocket.Close();
				Tag responseTag = new Tag();
				responseTag.parse(response);

				if (responseTag.Name.Equals("Error"))
				{
					throw new NuoDbSqlException(responseTag.getAttribute("text", "error text not found"));
				}

				serverAddress = responseTag.getAttribute("Address", null);
				serverPort = responseTag.getIntAttribute("Port", 0);

				if (serverAddress == null || serverPort == 0)
				{
					throw new NuoDbSqlException("no NuoDB nodes are available for database \"" + databaseName + "\"");
				}

				socket = new CryptoSocket(serverAddress, serverPort);
				//socket.TcpNoDelay = true;
				inputStream = socket.InputStream;
				outputStream = socket.OutputStream;
				dataStream.reset();
				dataStream.write(xml);
				dataStream.send(outputStream);

				RemotePassword remotePassword = new RemotePassword();
				string userKey = remotePassword.genClientKey();

				dataStream.startMessage(Protocol.OpenDatabase);
				dataStream.encodeInt(Protocol.PROTOCOL_VERSION);
				dataStream.encodeString(databaseName);

				getProcessConnection(databaseName);
				string dbUUId = processConnection.DatabaseUUId.ToString();

				// see if the app set the TimeZone. If so, it will be sent to the server
				// so set the local TZ to be the same. If not, send the current default
				// TZ  to the server. (Of course, this affects this connection only)
				/*
								String timeZone = properties.getProperty(TIMEZONE_NAME);

								if (timeZone == null)
								{
									// Save the default at the time the connection was opened
									TimeZone tz = TimeZone.getDefault();
									sqlContext.setTimeZone(tz);
									sqlContext.setTimeZoneId(tz.getID());
									properties.setProperty(TIMEZONE_NAME, tz.getID());
								}
								else
								{
									TimeZone tz = TimeZone.getTimeZone(timeZone);
									sqlContext.setTimeZone(tz);
									sqlContext.setTimeZoneId(tz.getID());
								}
				*/
				int count = properties.Count + ((dbUUId == null) ? 1 : 2); // Add LastCommitInfo

				if (password != null)
				{
					--count;
				}

				dataStream.encodeInt(count);

				foreach (DictionaryEntry property in properties)
				{
					string name = (string)property.Key;

					if (!name.Equals("password"))
					{
						string value = (string)property.Value;
						dataStream.encodeString(name);
						dataStream.encodeString(value);
					}
				}

				// LastCommitInfo and DatabaseUUId are sent as properties. This avoids sending another
				// message and keeps them from being protocol version sensitive

				string lastCommitParam = getProcessConnection(databaseName).getLastCommitInfo();
				dataStream.encodeString("LastCommitInfo");
				dataStream.encodeString(lastCommitParam);

				if (dbUUId != null)
				{
					dataStream.encodeString("DatabaseUUId");
					dataStream.encodeString(dbUUId);
				}

				// This  would have been the last commit txn id if that scheme was ever fully
				// implemented but it wasn't and this is now obsolete. keep it for compatibility with older servers

				dataStream.encodeLong(0);
				dataStream.encodeString(userKey);
				sendAndReceive(dataStream);

				protocolVersion = dataStream.getInt();
				string serverKey = dataStream.getString();
				string salt = dataStream.getString();
				dataStream.ProtocolVersion = protocolVersion;

				if (protocolVersion >= Protocol.PROTOCOL_VERSION5)
				{
					processConnection.DatabaseUUId = dataStream.getUUId();
				}

				string upperUserName = userName.ToUpper();
				byte[] key = remotePassword.computeSessionKey(upperUserName, password, salt, serverKey);

				// NOTE: unlike the C++ implementation we only support RC4 in .NET
				// and it's a hard-coded class (instead of the factory interface
				// on the C++ CryptoSocket) so there's no checking to see which
				// cipher was requested here

				inputStream.encrypt(new CipherRC4(key));
				outputStream.encrypt(new CipherRC4(key));

				dataStream.startMessage(Protocol.Authentication);
				dataStream.encodeString("Success!");
				authenticating = true;
				sendAndReceive(dataStream);

				// if the caller requested a cipher of None and we got here then the
				// server accpeted it and expects us to disable crypto now

				if (cipher.Equals("None"))
				{
					inputStream.encrypt(null);
					outputStream.encrypt(null);
				}

				this.OnStateChange(this.state, ConnectionState.Open);
			}
			catch (NuoDbSqlException e)
			{
#if DEBUG
				System.Diagnostics.Trace.WriteLine("NuoDBConnection::Open(): exception " + e.ToString());
#endif
				this.OnStateChange(this.state, ConnectionState.Closed);
				if (authenticating)
				{
					throw new NuoDbSqlException("Authentication failed for database \"" + databaseName + "\"", e);
				}

				throw e;
			}
			catch (IOException exception)
			{
#if DEBUG
				System.Diagnostics.Trace.WriteLine("NuoDBConnection::Open(): exception " + exception.ToString());
#endif
				if (socket != null && socket.Connected)
				{
					try
					{
						socket.Close();
						socket = null;
					}
					catch (IOException)
					{
						// just ignore
					}
				}
				this.OnStateChange(this.state, ConnectionState.Closed);

				throw new NuoDbSqlException(exception.ToString());
			}
			catch (XmlException exception)
			{
#if DEBUG
				System.Diagnostics.Trace.WriteLine("NuoDBConnection::Open(): exception " + exception.ToString());
#endif
				if (socket != null && socket.Connected)
				{
					try
					{
						socket.Close();
						socket = null;
					}
					catch (IOException)
					{
						// just ignore
					}
				}
				this.OnStateChange(this.state, ConnectionState.Closed);

				throw new NuoDbSqlException(exception.ToString());
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Open()
		{
			if (state != ConnectionState.Closed)
				throw new ArgumentException("", "");

			if (!properties.ContainsKey("server"))
				throw new ArgumentException("The connection string doesn't include the URL of the server", "ConnectionString");
			if (!properties.ContainsKey("database"))
				throw new ArgumentException("The connection string doesn't include the name of the database", "ConnectionString");

			NuoDbSqlException firstException = null;
			if (lastBroker != null)
			{
				try
				{
					doOpen(lastBroker);
					return;
				}
				catch (NuoDbSqlException e)
				{
					if (firstException == null)
						firstException = e;
				}
			}

			string hostNames = properties["server"];
			string[] servers = hostNames.Split(',');
			foreach (string hostName in servers)
			{
				string trimmed = hostName.Trim();
				try
				{
					doOpen(trimmed);
					lastBroker = trimmed;
					return;
				}
				catch (NuoDbSqlException e)
				{
					if (firstException == null)
						firstException = e;
				}
			}
			if (firstException != null)
				throw firstException;

		}

		private void OnStateChange(ConnectionState originalState, ConnectionState currentState)
		{
			this.state = currentState;
			if (this.StateChange != null)
			{
				this.StateChange(this, new StateChangeEventArgs(originalState, currentState));
			}
		}

		public DataTable GetSchema()
		{
			return this.GetSchema("");
		}

		public DataTable GetSchema(string collectionName)
		{
			return this.GetSchema(collectionName, null);
		}

		public DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			return GetSchemaHelper(this.connectionString, collectionName, restrictionValues);
		}

		static internal DataTable GetSchemaHelper(string connectionString, string collectionName, string[] restrictionValues)
		{
#if DEBUG
			System.Diagnostics.Trace.Write("NuoDBConnection::GetSchema(\"" + collectionName + "\", {");
			if (restrictionValues != null)
				for (int i = 0; i < restrictionValues.Length; i++)
				{
					if (i != 0)
						System.Diagnostics.Trace.Write(", ");
					System.Diagnostics.Trace.Write(restrictionValues[i] == null ? "null" : restrictionValues[i]);
				}
			System.Diagnostics.Trace.WriteLine("})");
#endif

			DataTable table = new DataTable(collectionName);
			table.Locale = System.Globalization.CultureInfo.CurrentCulture;

			// we must create a new connection to retrieve the schema resultsets without closing the active one
			// (e.g. invoking DataReader.GetSchemaTable on a newly created resultset will prevent fecthing the second
			// batch of rows)
			using (var tmConn = new NuoDbConnection(connectionString))
			{
				tmConn.InternalConnection.Open();

				if (collectionName == DbMetaDataCollectionNames.DataSourceInformation)
				{
					// see http://msdn.microsoft.com/en-us/library/ms254501.aspx
					table.Columns.Add("CompositeIdentifierSeparatorPattern", typeof(string));
					table.Columns.Add("DataSourceProductName", typeof(string));
					table.Columns.Add("DataSourceProductVersion", typeof(string));
					table.Columns.Add("DataSourceProductVersionNormalized", typeof(string));
					table.Columns.Add("DataSourceInternalProductVersion", typeof(string));
					table.Columns.Add("GroupByBehavior", typeof(GroupByBehavior));
					table.Columns.Add("IdentifierPattern", typeof(string));
					table.Columns.Add("IdentifierCase", typeof(IdentifierCase));
					table.Columns.Add("OrderByColumnsInSelect", typeof(bool));
					table.Columns.Add("ParameterMarkerFormat", typeof(string));
					table.Columns.Add("ParameterMarkerPattern", typeof(string));
					table.Columns.Add("ParameterNameMaxLength", typeof(int));
					table.Columns.Add("ParameterNamePattern", typeof(string));
					table.Columns.Add("QuotedIdentifierPattern", typeof(string));  // Regex
					table.Columns.Add("QuotedIdentifierCase", typeof(int));
					table.Columns.Add("StatementSeparatorPattern", typeof(string));
					table.Columns.Add("StringLiteralPattern", typeof(string));  // Regex
					table.Columns.Add("SupportedJoinOperators", typeof(SupportedJoinOperators));

					table.BeginLoadData();
					DataRow row = table.NewRow();
					int databaseMajorVersion = 0, databaseMinorVersion = 0;
					try
					{
						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetDatabaseMetaData);
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						for (int item; (item = tmConn.InternalConnection.dataStream.getInt()) != Protocol.DbmbFini; )
						{
							switch (item)
							{
								case Protocol.DbmbProductName:
									row["DataSourceProductName"] = tmConn.InternalConnection.dataStream.getString();
									break;

								case Protocol.DbmbProductVersion:
									row["DataSourceProductVersion"] = tmConn.InternalConnection.dataStream.getString();
									break;

								case Protocol.DbmbDatabaseMinorVersion:
									databaseMinorVersion = tmConn.InternalConnection.dataStream.getInt();
									break;

								case Protocol.DbmbDatabaseMajorVersion:
									databaseMajorVersion = tmConn.InternalConnection.dataStream.getInt();
									break;

								case Protocol.DbmbDefaultTransactionIsolation:
									int defaultTransactionIsolation = tmConn.InternalConnection.dataStream.getInt();
									break;

								default:
									tmConn.InternalConnection.dataStream.decode();
									break;
							}
						}
					}
					catch (Exception)
					{
					}

					row["DataSourceInternalProductVersion"] = String.Format("{0:D3}.{1:D3}", databaseMajorVersion, databaseMinorVersion);
					// The regular expression to match the composite separators in a composite identifier. 
					// For example, "\." (for SQL Server) or "@|\." (for Oracle).
					// A composite identifier is typically what is used for a database object name, 
					// for example: pubs.dbo.authors or pubs@dbo.authors.
					row["CompositeIdentifierSeparatorPattern"] = "\\.";
					// A normalized version for the data source, such that it can be compared with String.Compare(). 
					// The format of this is consistent for all versions of the provider to prevent version 10 from sorting between version 1 and version 2.
					//row["DataSourceProductVersionNormalized"] = ??
					// Specifies the relationship between the columns in a GROUP BY clause and the non-aggregated columns in the select list.
					row["GroupByBehavior"] = GroupByBehavior.Unknown;
					// A regular expression that matches an identifier and has a match value of the identifier. For example "[A-Za-z0-9_#$]".
					row["IdentifierPattern"] = "(^\\[\\p{Lo}\\p{Lu}\\p{Ll}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Nd}@$#_]*$)|(^\"[^\"\\0]|\"\"+\"$)";
					// Indicates whether non-quoted identifiers are treated as case sensitive or not.
					row["IdentifierCase"] = IdentifierCase.Insensitive;
					// Specifies whether columns in an ORDER BY clause must be in the select list. 
					// A value of true indicates that they are required to be in the select list, 
					// a value of false indicates that they are not required to be in the select list.
					row["OrderByColumnsInSelect"] = false;
					// For data sources that do not expect named parameters and expect the use of the ‘?’ character, the format string
					// can be specified as simply ‘?’, which would ignore the parameter name.
					row["ParameterMarkerFormat"] = "?";
					// if the data source doesn’t support named parameters, this would simply be "?".
					row["ParameterMarkerPattern"] = "?";
					// If the data source does not support named parameters, this property returns zero.
					row["ParameterNameMaxLength"] = 0;
					// A regular expression that matches the valid parameter names
					row["ParameterNamePattern"] = "";
					// A regular expression that matches a quoted identifier and has a match value of the identifier itself without the quotes. 
					// For example, if the data source used double-quotes to identify quoted identifiers, this would be: "(([^\"]|\"\")*)".
					row["QuotedIdentifierPattern"] = "\"([^\"]*)\"";
					// Indicates whether quoted identifiers are treated as case sensitive or not.
					row["QuotedIdentifierCase"] = IdentifierCase.Sensitive;
					// A regular expression that matches the statement separator.
					row["StatementSeparatorPattern"] = ";";
					// A regular expression that matches a string literal and has a match value of the literal itself. 
					// For example, if the data source used single-quotes to identify strings, this would be: "('([^']|'')*')"'
					row["StringLiteralPattern"] = "'([^']*)'";
					// Specifies what types of SQL join statements are supported by the data source.
					row["SupportedJoinOperators"] = SupportedJoinOperators.None;
					table.Rows.Add(row);
					table.EndLoadData();
				}
				else if (collectionName == DbMetaDataCollectionNames.ReservedWords)
				{
					table.Columns.Add("ReservedWord", typeof(string));
					table.Columns.Add("MaximumVersion", typeof(string));
					table.Columns.Add("MinimumVersion", typeof(string));

					string[] words = { "select", "Add field", "Drop field", "Alter table", "Alter field",
                                       "Alter user", "And", "Or", "Not", "Create Index", "Upgrade", "Unique",
                                       "Table key", "Table unique key", "Primary Key", "Create Table",
                                       "Upgrade Table", "Rename Table", "Create View", "Upgrade View",
                                       "Drop View", "Create Schema", "Drop Schema", "Integer", "Smallint",
                                       "Bigint", "Tinyint", "float", "double", "Blob", "bytes", "boolean",
                                       "binary", "varbinary", "string", "char", "text", "date", "timestamp",
                                       "time", "varchar", "numeric", "Decimal Number", "Enum", "Delete", "Repair",
                                       "Field", "Not Null", "Default Value", "searchable", "not searchable",
                                       "collation", "character_set", "Foreign Key", "Identifier", "record number",
                                       "Insert", "Replace", "Name", "Number", "Order", "Descending", "limit",
                                       "Quoted String", "Select Clause", "Distinct", "Gtr", "Geq", "Eql",
                                       "Leq", "Lss", "Neq", "in list", "in select", "exists", "Between", "Like",
                                       "starts with", "Containing", "Matching", "Regular expression", "Null",
                                       "True", "False", "Execute", "Parameter", "Count", "sum", "min", "max",
                                       "avg", "Drop Index", "Drop primary key", "Drop foreign key", "Drop Table",
                                       "If Exists", "Describe", "Statement", "for_select", "for_insert", "while",
                                       "if then else", "variable", "declaration", "throw", "assignment",
                                       "Start transaction", "read only", "read write", "read committed", "write committed",
                                       "read uncommitted", "repeatableread", "serializable", "Commit", "Rollback",
                                       "Savepoint", "Rollack Savepoint", "Release Savepoint", "Table", "Derived Table",
                                       "Assign", "Update", "list", "Constant", "Function", "Grant", "Grant role", "Alter",
                                       "Read", "Identity", "Check", "Constraint", "value constraint", "auto increment",
                                       "generated", "always", "Domain", "is null", "is active_role", "start with", "reindex",
                                       "cursor", "alias", "push_namespace", "pop_namespace", "set_namespace", "coalesce",
                                       "nod_nullif", "case", "case_search", "add", "subtract", "multiply", "divide", "modulus",
                                       "negate", "plus", "cast", "concatenation", "create user", "drop user", "create role",
                                       "upgrade role", "drop role", "revoke", "revoke role", "assume", "revert", "negate",
                                       "priv insert", "priv delete", "priv execute", "priv select", "priv update", "priv grant",
                                       "priv all", "characters", "octets", "role", "view", "procedure", "user", "zone", "admin",
                                       "default role", "create sequence", "upgrade sequence", "drop sequence", "next value",
                                       "create trigger", "upgrade trigger", "alter trigger", "drop trigger", "create procedure",
                                       "upgrade procedure", "alter procedure", "drop procedure", "pre-insert", "post-insert",
                                       "pre-update", "post-update", "pre-delete", "post-delete", "pre-commit", "post-commit",
                                       "active", "inactive", "position", "select-expr", "create filterset", "upgrade filterset",
                                       "drop filterset", "enable filterset", "disable filterset", "table filter", "left outer",
                                       "right outer", "full outer", "inner", "join", "join_term", "trigger class", "enable trigger class",
                                       "disable trigger class", "create zone", "upgrade zone", "drop zone", "range", "ip address",
                                       "node_name", "wildcard", "create domain", "upgrade domain", "alter domain", "upgrade schema",
                                       "delete cascade", "index segment", "for update", "write committed", "read committed", 
                                       "consistent read", "set dialect", "set names", "restrict", "cascade", "characters", "octets",
                                       "create event", "collate", "truncate", "restart", "nameless constraint", "drop constraint",
                                       "drop unique constraint", "drop domain", "with check option", "not real", "lower", "upper",
                                       "substr", "substring index", "charlength", "default keyword", "sqrt", "power", "Scientific Notation Number",
                                       "Explain", "msleep", "KillStatement", "now" };
					table.BeginLoadData();
					foreach (string keyword in words)
					{
						DataRow row = table.NewRow();
						row["ReservedWord"] = keyword;
						table.Rows.Add(row);
					}
					table.EndLoadData();

				}
				else if (collectionName == DbMetaDataCollectionNames.DataTypes)
				{
					table.Columns.Add("TypeName", typeof(string));  // The provider-specific data type name.
					table.Columns.Add("ProviderDbType", typeof(int)); // The provider-specific type value that should be used when specifying 
					//a parameter’s type. For example, SqlDbType.Money or OracleType.Blob.
					table.Columns.Add("ColumnSize", typeof(long));
					table.Columns.Add("CreateFormat", typeof(string));
					table.Columns.Add("CreateParameters", typeof(string));
					table.Columns.Add("DataType", typeof(string));  // The name of the .NET Framework type of the data type.
					table.Columns.Add("IsAutoincrementable", typeof(bool));
					table.Columns.Add("IsBestMatch", typeof(bool));
					table.Columns.Add("IsCaseSensitive", typeof(bool));
					table.Columns.Add("IsFixedLength", typeof(bool));
					table.Columns.Add("IsFixedPrecisionScale", typeof(bool));
					table.Columns.Add("IsLong", typeof(bool));
					table.Columns.Add("IsNullable", typeof(bool));
					table.Columns.Add("IsSearchable", typeof(bool));
					table.Columns.Add("IsSearchableWithLike", typeof(bool));
					table.Columns.Add("IsUnsigned", typeof(bool));
					table.Columns.Add("MaximumScale", typeof(short));
					table.Columns.Add("MinimumScale", typeof(short));
					table.Columns.Add("IsConcurrencyType", typeof(bool));
					table.Columns.Add("IsLiteralsSupported", typeof(bool));
					table.Columns.Add("LiteralPrefix", typeof(string));
					table.Columns.Add("LitteralSuffix", typeof(string));
					table.Columns.Add("NativeDataType", typeof(string));    // NativeDataType is an OLE DB specific column for exposing the OLE DB type of the data type .

					table.ReadXml(XmlReader.Create(typeof(NuoDbConnectionInternal).Assembly.GetManifestResourceStream("NuoDb.Data.Client.DataTypes.xml")));
				}
				else if (collectionName == "Tables")
				{
					table.Columns.Add("TABLE_CATALOG", typeof(string));
					table.Columns.Add("TABLE_SCHEMA", typeof(string));
					table.Columns.Add("TABLE_NAME", typeof(string));
					table.Columns.Add("TABLE_TYPE", typeof(string));
					table.Columns.Add("REMARKS", typeof(string));
					table.Columns.Add("VIEW_DEF", typeof(string));

					tmConn.InternalConnection.dataStream.startMessage(Protocol.GetTables);
					tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
					for (int i = 1; i < 3; i++)
						if (restrictionValues != null && restrictionValues.Length > i)
							tmConn.InternalConnection.dataStream.encodeString(restrictionValues[i]);
						else
							tmConn.InternalConnection.dataStream.encodeNull();
					if (restrictionValues == null || restrictionValues.Length < 4 || restrictionValues[3] == null)
						tmConn.InternalConnection.dataStream.encodeInt(0);
					else
					{
						tmConn.InternalConnection.dataStream.encodeInt(1);
						tmConn.InternalConnection.dataStream.encodeString(restrictionValues[3]);
					}

					tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
					int handle = tmConn.InternalConnection.dataStream.getInt();

					if (handle != -1)
					{
						using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
						{
							table.BeginLoadData();
							while (reader.Read())
							{
#if DEBUG
								System.Diagnostics.Trace.WriteLine("-> " + reader["TABLE_NAME"] + ", " + reader["TABLE_TYPE"]);
#endif
								DataRow row = table.NewRow();
								row["TABLE_SCHEMA"] = reader["TABLE_SCHEM"];
								row["TABLE_NAME"] = reader["TABLE_NAME"];
								row["TABLE_TYPE"] = reader["TABLE_TYPE"];
								row["REMARKS"] = reader["REMARKS"];
								row["VIEW_DEF"] = reader["VIEW_DEF"];
								table.Rows.Add(row);
							}
							table.EndLoadData();
						}
					}
				}
				else if (collectionName == "Columns")
				{
					table.Columns.Add("COLUMN_CATALOG", typeof(string));
					table.Columns.Add("COLUMN_SCHEMA", typeof(string));
					table.Columns.Add("COLUMN_TABLE", typeof(string));
					table.Columns.Add("COLUMN_NAME", typeof(string));
					table.Columns.Add("COLUMN_POSITION", typeof(int));
					table.Columns.Add("COLUMN_TYPE", typeof(string));
					table.Columns.Add("COLUMN_LENGTH", typeof(int));
					table.Columns.Add("COLUMN_PRECISION", typeof(int));
					table.Columns.Add("COLUMN_NULLABLE", typeof(bool));
					table.Columns.Add("COLUMN_IDENTITY", typeof(bool));
					table.Columns.Add("COLUMN_DEFAULT", typeof(string));
					table.Columns.Add("COLUMN_SCALE", typeof(int));

					tmConn.InternalConnection.dataStream.startMessage(Protocol.GetColumns);
					tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
					for (int i = 1; i < 4; i++)
						if (restrictionValues != null && restrictionValues.Length > i)
							tmConn.InternalConnection.dataStream.encodeString(restrictionValues[i]);
						else
							tmConn.InternalConnection.dataStream.encodeNull();

					tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
					int handle = tmConn.InternalConnection.dataStream.getInt();

					if (handle != -1)
					{
						using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
						{
							table.BeginLoadData();
							while (reader.Read())
							{
								DataRow row = table.NewRow();
								row["COLUMN_SCHEMA"] = reader["TABLE_SCHEM"];
								row["COLUMN_TABLE"] = reader["TABLE_NAME"];
								row["COLUMN_NAME"] = reader["COLUMN_NAME"];
								row["COLUMN_POSITION"] = reader["ORDINAL_POSITION"];
								row["COLUMN_LENGTH"] = reader["COLUMN_SIZE"];
								row["COLUMN_PRECISION"] = reader["BUFFER_LENGTH"];
								row["COLUMN_SCALE"] = reader["DECIMAL_DIGITS"];
								if (isNumeric((string)reader["TYPE_NAME"]))
								{
									if ((int)reader["DECIMAL_DIGITS"] != 0)
										row["COLUMN_TYPE"] = reader["TYPE_NAME"] + "(" + reader["BUFFER_LENGTH"] + "," + reader["DECIMAL_DIGITS"] + ")";
									else
										row["COLUMN_TYPE"] = reader["TYPE_NAME"];
								}
								else
								{
									if (isVarLenType((string)reader["TYPE_NAME"]) && (int)reader["COLUMN_SIZE"] != 0)
										row["COLUMN_TYPE"] = reader["TYPE_NAME"] + "(" + reader["COLUMN_SIZE"] + ")";
									else
										row["COLUMN_TYPE"] = reader["TYPE_NAME"];
								}
								if (!reader.IsDBNull(reader.GetOrdinal("IS_NULLABLE")))
									row["COLUMN_NULLABLE"] = reader["IS_NULLABLE"].Equals("YES");
								if (!reader.IsDBNull(reader.GetOrdinal("IS_AUTOINCREMENT")))
									row["COLUMN_IDENTITY"] = reader["IS_AUTOINCREMENT"].Equals("YES");
								row["COLUMN_DEFAULT"] = reader["COLUMN_DEF"];

#if DEBUG
								System.Diagnostics.Trace.WriteLine("-> " + row["COLUMN_NAME"] + " " + row["COLUMN_TYPE"]);
#endif
								table.Rows.Add(row);
							}
							table.EndLoadData();
						}
					}
				}
				else if (collectionName == "Indexes")
				{
					table.Columns.Add("INDEX_CATALOG", typeof(string));
					table.Columns.Add("INDEX_SCHEMA", typeof(string));
					table.Columns.Add("INDEX_TABLE", typeof(string));
					table.Columns.Add("INDEX_NAME", typeof(string));
					table.Columns.Add("INDEX_TYPE", typeof(string));
					table.Columns.Add("INDEX_UNIQUE", typeof(bool));
					table.Columns.Add("INDEX_PRIMARY", typeof(bool));

					List<KeyValuePair<string, string>> tables = RetrieveMatchingTables(tmConn, getItemAtIndex(restrictionValues, 1), getItemAtIndex(restrictionValues, 2));

					foreach (KeyValuePair<string, string> t in tables)
					{
						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetIndexInfo);
						tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
						tmConn.InternalConnection.dataStream.encodeString(t.Key);
						tmConn.InternalConnection.dataStream.encodeString(t.Value);
						tmConn.InternalConnection.dataStream.encodeBoolean(false);    // unique
						tmConn.InternalConnection.dataStream.encodeBoolean(false);    // approximate
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						int handle = tmConn.InternalConnection.dataStream.getInt();

						// to avoid to insert the same index more than once
						HashSet<string> unique = new HashSet<string>();
						if (handle != -1)
						{
							using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
							{
								table.BeginLoadData();
								while (reader.Read())
								{
									// enforce the restriction on the index name
									if (restrictionValues != null && restrictionValues.Length > 3 && restrictionValues[3] != null &&
										!restrictionValues[3].Equals(reader["INDEX_NAME"]))
										continue;
									if (!unique.Add((string)reader["INDEX_NAME"]))
										continue;
									DataRow row = table.NewRow();
									row["INDEX_SCHEMA"] = reader["TABLE_SCHEM"];
									row["INDEX_TABLE"] = reader["TABLE_NAME"];
									row["INDEX_NAME"] = reader["INDEX_NAME"];
									switch ((int)reader["TYPE"])
									{
										case 0:
											row["INDEX_TYPE"] = "Primary";
											break;
										case 1:
											row["INDEX_TYPE"] = "Unique";
											break;
										case 2:
											row["INDEX_TYPE"] = "Secondary";
											break;
										case 3:
											row["INDEX_TYPE"] = "Foreign Key";
											break;
									}
									row["INDEX_UNIQUE"] = ((int)reader["NON_UNIQUE"] == 0) ? true : false;
									row["INDEX_PRIMARY"] = ((int)reader["TYPE"] == 0) ? true : false;
									table.Rows.Add(row);
								}
								table.EndLoadData();
							}
						}

						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetPrimaryKeys);
						tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
						tmConn.InternalConnection.dataStream.encodeString(t.Key);
						tmConn.InternalConnection.dataStream.encodeString(t.Value);
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						handle = tmConn.InternalConnection.dataStream.getInt();

						if (handle != -1)
						{
							using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
							{
								// use INDEX_NAME, if present; if missing, the name of the index is in PK_NAME
								string indexName = "PK_NAME";
								foreach (string column in reader.columnNames)
								{
									if (column == "INDEX_NAME")
									{
										indexName = "INDEX_NAME";
										break;
									}
								}
								table.BeginLoadData();
								while (reader.Read())
								{
									// enforce the restriction on the index name
									if (restrictionValues != null && restrictionValues.Length > 3 && restrictionValues[3] != null &&
										!restrictionValues[3].Equals(reader[indexName]))
										continue;
									if (!unique.Add((string)reader[indexName]))
										continue;
									DataRow row = table.NewRow();
									row["INDEX_SCHEMA"] = reader["TABLE_SCHEM"];
									row["INDEX_TABLE"] = reader["TABLE_NAME"];
									row["INDEX_NAME"] = reader[indexName];
									row["INDEX_TYPE"] = "Primary";
									row["INDEX_UNIQUE"] = true;
									row["INDEX_PRIMARY"] = true;
									table.Rows.Add(row);
								}
								table.EndLoadData();
							}
						}
					}
				}
				else if (collectionName == "IndexColumns")
				{
					table.Columns.Add("INDEXCOLUMN_CATALOG", typeof(string));
					table.Columns.Add("INDEXCOLUMN_SCHEMA", typeof(string));
					table.Columns.Add("INDEXCOLUMN_TABLE", typeof(string));
					table.Columns.Add("INDEXCOLUMN_INDEX", typeof(string));
					table.Columns.Add("INDEXCOLUMN_NAME", typeof(string));
					table.Columns.Add("INDEXCOLUMN_POSITION", typeof(int));
					table.Columns.Add("INDEXCOLUMN_ISPRIMARY", typeof(bool));

					List<KeyValuePair<string, string>> tables = RetrieveMatchingTables(tmConn, getItemAtIndex(restrictionValues, 1), getItemAtIndex(restrictionValues, 2));

					foreach (KeyValuePair<string, string> t in tables)
					{
						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetIndexInfo);
						tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
						tmConn.InternalConnection.dataStream.encodeString(t.Key);
						tmConn.InternalConnection.dataStream.encodeString(t.Value);
						tmConn.InternalConnection.dataStream.encodeBoolean(false);    // unique
						tmConn.InternalConnection.dataStream.encodeBoolean(false);    // approximate
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						int handle = tmConn.InternalConnection.dataStream.getInt();

						if (handle != -1)
						{
							using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
							{
								table.BeginLoadData();
								while (reader.Read())
								{
									// enforce the restriction on the index name
									if (restrictionValues != null && restrictionValues.Length > 3 && restrictionValues[3] != null &&
										!restrictionValues[3].Equals(reader["INDEX_NAME"]))
										continue;
#if DEBUG
									System.Diagnostics.Trace.WriteLine("-> " + reader["TABLE_SCHEM"] + "." + reader["TABLE_NAME"] + "=" + reader["COLUMN_NAME"]);
#endif
									DataRow row = table.NewRow();
									row["INDEXCOLUMN_SCHEMA"] = reader["TABLE_SCHEM"];
									row["INDEXCOLUMN_TABLE"] = reader["TABLE_NAME"];
									row["INDEXCOLUMN_INDEX"] = reader["INDEX_NAME"];
									row["INDEXCOLUMN_NAME"] = reader["COLUMN_NAME"];
									row["INDEXCOLUMN_POSITION"] = reader["ORDINAL_POSITION"];
									row["INDEXCOLUMN_ISPRIMARY"] = false;
									table.Rows.Add(row);
								}
								table.EndLoadData();
							}
						}

						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetPrimaryKeys);
						tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
						tmConn.InternalConnection.dataStream.encodeString(t.Key);
						tmConn.InternalConnection.dataStream.encodeString(t.Value);
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						handle = tmConn.InternalConnection.dataStream.getInt();

						if (handle != -1)
						{
							using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
							{
								// use INDEX_NAME, if present; if missing, the name of the index is in PK_NAME
								string indexName = "PK_NAME";
								foreach (string column in reader.columnNames)
								{
									if (column == "INDEX_NAME")
									{
										indexName = "INDEX_NAME";
										break;
									}
								}
								table.BeginLoadData();
								while (reader.Read())
								{
									// enforce the restriction on the index name
									if (restrictionValues != null && restrictionValues.Length > 3 && restrictionValues[3] != null &&
										!restrictionValues[3].Equals(reader[indexName]))
										continue;
#if DEBUG
									System.Diagnostics.Trace.WriteLine("-> " + reader["TABLE_SCHEM"] + "." + reader["TABLE_NAME"] + " (Primary) =" + reader["COLUMN_NAME"]);
#endif
									DataRow row = table.NewRow();
									row["INDEXCOLUMN_SCHEMA"] = reader["TABLE_SCHEM"];
									row["INDEXCOLUMN_TABLE"] = reader["TABLE_NAME"];
									row["INDEXCOLUMN_INDEX"] = reader[indexName];
									row["INDEXCOLUMN_NAME"] = reader["COLUMN_NAME"];
									row["INDEXCOLUMN_POSITION"] = reader["KEY_SEQ"];
									row["INDEXCOLUMN_ISPRIMARY"] = true;
									table.Rows.Add(row);
								}
								table.EndLoadData();
							}
						}
					}
				}
				else if (collectionName == "ForeignKeys")
				{
					table.Columns.Add("FOREIGNKEY_CATALOG", typeof(string));
					table.Columns.Add("FOREIGNKEY_SCHEMA", typeof(string));
					table.Columns.Add("FOREIGNKEY_TABLE", typeof(string));
					table.Columns.Add("FOREIGNKEY_NAME", typeof(string));
					table.Columns.Add("FOREIGNKEY_OTHER_CATALOG", typeof(string));
					table.Columns.Add("FOREIGNKEY_OTHER_SCHEMA", typeof(string));
					table.Columns.Add("FOREIGNKEY_OTHER_TABLE", typeof(string));

					List<KeyValuePair<string, string>> tables = RetrieveMatchingTables(tmConn, getItemAtIndex(restrictionValues, 1), getItemAtIndex(restrictionValues, 2));

					foreach (KeyValuePair<string, string> t in tables)
					{
						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetImportedKeys);
						tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
						tmConn.InternalConnection.dataStream.encodeString(t.Key);
						tmConn.InternalConnection.dataStream.encodeString(t.Value);
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						int handle = tmConn.InternalConnection.dataStream.getInt();

						if (handle != -1)
						{
							using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
							{
								table.BeginLoadData();
								while (reader.Read())
								{
									// enforce the restriction on the index name
									string name = "[" + reader["FKTABLE_SCHEM"] + "]" + reader["FKTABLE_NAME"] + "." + reader["FKCOLUMN_NAME"] + "->" +
												  "[" + reader["PKTABLE_SCHEM"] + "]" + reader["PKTABLE_NAME"] + "." + reader["PKCOLUMN_NAME"];
									// enforce the restriction on the index name
									if (restrictionValues != null && restrictionValues.Length > 3 && restrictionValues[3] != null &&
										!restrictionValues[3].Equals(name))
										continue;
									DataRow row = table.NewRow();
									row["FOREIGNKEY_SCHEMA"] = reader["FKTABLE_SCHEM"];
									row["FOREIGNKEY_TABLE"] = reader["FKTABLE_NAME"];
									row["FOREIGNKEY_NAME"] = name;
									row["FOREIGNKEY_OTHER_SCHEMA"] = reader["PKTABLE_SCHEM"];
									row["FOREIGNKEY_OTHER_TABLE"] = reader["PKTABLE_NAME"];
									table.Rows.Add(row);
								}
								table.EndLoadData();
							}
						}
					}
				}
				else if (collectionName == "ForeignKeyColumns")
				{
					table.Columns.Add("FOREIGNKEYCOLUMN_CATALOG", typeof(string));
					table.Columns.Add("FOREIGNKEYCOLUMN_SCHEMA", typeof(string));
					table.Columns.Add("FOREIGNKEYCOLUMN_TABLE", typeof(string));
					table.Columns.Add("FOREIGNKEYCOLUMN_KEY", typeof(string));
					table.Columns.Add("FOREIGNKEYCOLUMN_NAME", typeof(string));
					table.Columns.Add("FOREIGNKEYCOLUMN_ORDINAL", typeof(int));
					table.Columns.Add("FOREIGNKEYCOLUMN_OTHER_COLUMN_NAME", typeof(string));

					List<KeyValuePair<string, string>> tables = RetrieveMatchingTables(tmConn, getItemAtIndex(restrictionValues, 1), getItemAtIndex(restrictionValues, 2));

					foreach (KeyValuePair<string, string> t in tables)
					{
						tmConn.InternalConnection.dataStream.startMessage(Protocol.GetImportedKeys);
						tmConn.InternalConnection.dataStream.encodeNull(); // catalog is always null
						tmConn.InternalConnection.dataStream.encodeString(t.Key);
						tmConn.InternalConnection.dataStream.encodeString(t.Value);
						tmConn.InternalConnection.sendAndReceive(tmConn.InternalConnection.dataStream);
						int handle = tmConn.InternalConnection.dataStream.getInt();

						if (handle != -1)
						{
							using (NuoDbDataReader reader = new NuoDbDataReader(tmConn, handle, tmConn.InternalConnection.dataStream, null, true))
							{
								table.BeginLoadData();
								while (reader.Read())
								{
									// enforce the restriction on the index name
									string name = "[" + reader["FKTABLE_SCHEM"] + "]" + reader["FKTABLE_NAME"] + "." + reader["FKCOLUMN_NAME"] + "->" +
												  "[" + reader["PKTABLE_SCHEM"] + "]" + reader["PKTABLE_NAME"] + "." + reader["PKCOLUMN_NAME"];
									// enforce the restriction on the index name
									if (restrictionValues != null && restrictionValues.Length > 3 && restrictionValues[3] != null &&
										!restrictionValues[3].Equals(name))
										continue;
#if DEBUG
									System.Diagnostics.Trace.WriteLine("-> " + reader["FKTABLE_SCHEM"] + "." + reader["FKTABLE_NAME"] + "=" + reader["FKCOLUMN_NAME"]);
#endif
									DataRow row = table.NewRow();
									row["FOREIGNKEYCOLUMN_SCHEMA"] = reader["FKTABLE_SCHEM"];
									row["FOREIGNKEYCOLUMN_TABLE"] = reader["FKTABLE_NAME"];
									row["FOREIGNKEYCOLUMN_KEY"] = name;
									row["FOREIGNKEYCOLUMN_NAME"] = reader["FKCOLUMN_NAME"];
									row["FOREIGNKEYCOLUMN_ORDINAL"] = reader["KEY_SEQ"];
									row["FOREIGNKEYCOLUMN_OTHER_COLUMN_NAME"] = reader["PKCOLUMN_NAME"];
									table.Rows.Add(row);
								}
								table.EndLoadData();
							}
						}
					}
				}
			}
			return table;
		}

		private static string getItemAtIndex(string[] values, int index)
		{
			if (values != null && values.Length > index)
				return values[index];
			return null;
		}

		internal static List<KeyValuePair<string, string>> RetrieveMatchingTables(NuoDbConnection conn, string schema, string table)
		{
			List<KeyValuePair<string, string>> tables = new List<KeyValuePair<string, string>>();

			// the API for retrieving indexes works on a fully specified schema+table
			// so, unless we have both values, we need to first retrieve the set of tables identified by the restrictions
			if (schema != null && table != null)
			{
				tables.Add(new KeyValuePair<string, string>(schema, table));
			}
			else
			{
				conn.InternalConnection.dataStream.startMessage(Protocol.GetTables);
				conn.InternalConnection.dataStream.encodeNull(); // catalog is always null
				conn.InternalConnection.dataStream.encodeString(schema);
				conn.InternalConnection.dataStream.encodeString(table);
				conn.InternalConnection.dataStream.encodeInt(0);

				conn.InternalConnection.sendAndReceive(conn.InternalConnection.dataStream);
				int handle = conn.InternalConnection.dataStream.getInt();

				if (handle != -1)
				{
					using (NuoDbDataReader reader = new NuoDbDataReader(conn, handle, conn.InternalConnection.dataStream, null, true))
					{
						while (reader.Read())
							tables.Add(new KeyValuePair<string, string>((string)reader["TABLE_SCHEM"], (string)reader["TABLE_NAME"]));
					}
				}
			}
			return tables;
		}

		internal static string mapDbTypeToNetType(int dbType)
		{
			switch ((DbType)dbType)
			{
				case DbType.Boolean: return "System.Boolean";
				case DbType.Int16: return "System.Int16";
				case DbType.Int32: return "System.Int32";
				case DbType.Int64: return "System.Int64";
				case DbType.Single: return "System.Single";
				case DbType.Double: return "System.Double";
				case DbType.Decimal: return "System.Decimal";
				case DbType.Byte: return "System.Byte";
				case DbType.String: return "System.String";
				case DbType.Date: return "System.Date";
				case DbType.Time: return "System.Time";
				case DbType.DateTime: return "System.DateTime";
				case DbType.Binary: return "System.Binary";
				case DbType.Object: return "System.Object";
			}
			throw new NotImplementedException();
		}

		internal static bool isNumeric(string p)
		{
			string[] numeric_types = new string[] { "integer", "int", "smallint", "tinyint", "float", "double", "bigint", "numeric", "decimal" };
			foreach (string type in numeric_types)
				if (type.Equals(p))
					return true;
			return false;
		}

		internal static bool isVarLenType(string p)
		{
			string[] varlen_types = new string[] { "char", "varchar", "nchar", "nvarchar" };
			foreach (string type in varlen_types)
				if (type.Equals(p))
					return true;
			return false;
		}

		internal static string mapNuoDbToNetType(string p, int precision, int scale)
		{
			p = p.ToLower();
			if (p == "string" || p == "char" || p == "varchar" || p == "longvarchar")
				return "System.String";
			if (p == "tinyint" || p == "smallint" || p == "integer" || p == "numeric" || p == "decimal")
			{
				if (scale == 0)
					return "System.Int32";
				else
					return "System.Decimal";
			}
			if (p == "float")
				return "System.Single";
			if (p == "double")
				return "System.Double";
			if (p == "date" || p == "time" || p == "timestamp")
				return "System.DateTime";
			if (p == "bigint")
				return "System.Decimal";
			if (p == "boolean")
				return "System.Boolean";
			if (p == "clob" || p == "blob")
				return "System.String";
			if (p == "null")
				return "System.DBNull";
			return "";
		}

		internal static DbType mapJavaSqlToDbType(int jSQL)
		{
			switch (jSQL)
			{
				case -7: //BIT
					return DbType.Boolean;
				case -6: //TINYINT
				case 5: //SMALLINT
				case 4: //INTEGER
					return DbType.Int32;
				case -5: //BIGINT
					return DbType.Int64;
				case 6: //FLOAT
				case 7: //REAL
					return DbType.Single;
				case 8: //DOUBLE
					return DbType.Double;
				case 2: //NUMERIC
				case 3: //DECIMAL
					return DbType.Decimal;
				case 1: //CHAR
				case 12: //VARCHAR
				case -1: //LONGVARCHAR
					return DbType.String;
				case 91: //DATE
					return DbType.Date;
				case 92: //TIME
					return DbType.Time;
				case 93: //TIMESTAMP
					return DbType.DateTime;
				case -2: //BINARY
				case -3: //VARBINARY
				case -4: //LONGVARBINARY
					return DbType.Binary;
				case 0: //NULL
				case 1111: //OTHER
				case 2000: //JAVA_OBJECT
				case 2001: //DISTINCT
				case 2002: //STRUCT
				case 2003: //ARRAY
				case 2004: //BLOB
				case 2005: //CLOB
				case 2006: //REF
				case 70: //DATALINK
					return DbType.Object;
				case 16: //BOOLEAN
					return DbType.Boolean;
				//------------------------- JDBC 4.0 -----------------------------------
				case -15: //NCHAR
				case -9: //NVARCHAR 
				case -16: //LONGNVARCHAR
				case 2011: //NCLOB
					return DbType.String;
				case 2009: //SQLXML 
					return DbType.Xml;
				case -8: //ROWID
					return DbType.Object;
			}
			throw new NotImplementedException();
		}

		public string ServerVersion
		{
			get { return "1.0"; }
		}

		public ConnectionState State
		{
			get { return state; }
		}

		internal void Commit()
		{
			dataStream.startMessage(Protocol.CommitTransaction);
			sendAndReceive(dataStream);

			// Both protocol V2 and V3 are sending a txn here

			long transactionId = dataStream.getLong();

			// But only V3 sends the node id and commit sequence

			if (protocolVersion >= Protocol.PROTOCOL_VERSION3)
			{
				int nodeId = dataStream.getInt();
				long commitSequence = dataStream.getLong();
				processConnection.setLast(transactionId, nodeId, commitSequence);
			}

			transaction = null;
		}

		internal void Rollback()
		{
			dataStream.startMessage(Protocol.RollbackTransaction);
			sendAndReceive(dataStream);

			transaction = null;
		}

		internal NuoDbConnection Owner
		{
			get { return owner; }
			set { owner = value; }
		}
	}
}
