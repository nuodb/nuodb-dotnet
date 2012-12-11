/****************************************************************************
* Copyright (c) 2012, NuoDB, Inc.
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
****************************************************************************/

using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using System.Data.NuoDB.Xml;
using System.Data.NuoDB.Net;
using System.Data.NuoDB.Security;
using System.Data.NuoDB.Util;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace System.Data.NuoDB
{
    // this declaration prevents VS2010 from trying to edit this class with the designer.
    // it would try to do that because the parent class derives from Component, but it's abstract
    // and it cannot be instanciated
    [System.ComponentModel.DesignerCategory("")]
    public sealed class NuoDBConnection : DbConnection
    {
        internal DbTransaction transaction;
        internal const int PORT = 48004;
        internal const string LAST_INFO_SEPARATOR = ";";
        internal const string DEFAULT_CIPHER = "RC4";

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
        private string upperUserName;
        private int connectionId;
        private ConnectionState state = ConnectionState.Closed;
        private string lastBroker;

        private Dictionary<int, string> mapSQLTypes = null;
        private List<int> listResultSets = new List<int>();

        public NuoDBConnection()
        {
        }

        public NuoDBConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Trace.WriteLine("NuoDBConnection::Dispose()");
            if (state == ConnectionState.Open)
                Close();
            base.Dispose(disposing);
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
                throw new SQLException(exception);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void sendAndReceive(EncodedDataStream stream)
        {
            System.Diagnostics.Trace.WriteLine("NuoDBConnection::sendAndReceive()");
            try
			{
                lock (this)
                {
                    stream.send(outputStream);
                    stream.getMessage(inputStream);
                    int status = stream.Int;

                    if (status != 0)
                    {
                        string message = stream.String;
                        string sqlState = "";

                        if (protocolVersion >= Protocol.PROTOCOL_VERSION2)
                        {
                            sqlState = stream.String;
                        }

                        // If empty string, use the state from SQLCode

                        if (StringUtils.size(sqlState) == 0)
                        {
                            sqlState = SQLCode.findSQLState(status);
                        }

                        throw new SQLException(message, sqlState, status);
                    }
                }
			}
			catch (IOException exception)
			{
				throw new SQLException(exception);
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
				processConnections = processConnection = new ProcessConnection(databaseName,processConnections);
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

        public override int ConnectionTimeout
        {
            get { return 0; }
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
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

            transaction = new NuoDBTransaction(this, isolationLevel);
            return transaction;
        }

        public override void ChangeDatabase(string databaseName)
        {
            System.Diagnostics.Trace.WriteLine("NuoDBConnection::ChangeDatabase("+databaseName+")");
            if (state == ConnectionState.Open)
                Close();
            properties["database"] = databaseName;
        }

        internal void RegisterResultSet(int handle)
        {
            listResultSets.Add(handle);
        }

        internal void CloseResultSet(int handle)
        {
            EncodedDataStream dataStream = new EncodedDataStream();
            dataStream.startMessage(Protocol.CloseResultSet);
            dataStream.encodeInt(handle);
            sendAsync(dataStream);

            if (listResultSets.Contains(handle))
                listResultSets.Remove(handle);
        }

        public override void Close()
        {
            if (socket == null || !socket.Connected)
            {
                return;
            }
            state = ConnectionState.Closed;
            System.Diagnostics.Trace.WriteLine("NuoDBConnection::Close()");

            List<int> tmpResultSet = new List<int>(listResultSets);
            listResultSets.Clear();

            foreach (Int32 r in tmpResultSet)
            {
                CloseResultSet(r);
            }

            dataStream.startMessage(Protocol.CloseConnection);
            sendAndReceive(dataStream);

            try
            {
                socket.Close();
            }
            catch (IOException e)
            {
                throw new SQLException(e.Message);
            }
            finally
            {
                socket = null;
            }
        }

        public override string ConnectionString
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
                            string kKey = keyPair.Groups[1].Value.Trim();
                            string kValue = keyPair.Groups[3].Success ? keyPair.Groups[3].Value :
                                                keyPair.Groups[4].Success ? keyPair.Groups[4].Value :
                                                    keyPair.Groups[5].Success ? keyPair.Groups[5].Value.Trim() : "";
                            properties.Add(kKey, kValue);
                        }
                    }
                }
            }
        }

        protected override DbCommand CreateDbCommand()
        {
            return new NuoDBCommand(this);
        }

        public override string DataSource
        {
            get { return properties["server"]; }
        }

        public override string Database
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
                    throw new SQLException("Unknown cipher: " + cipher);

                tag.addAttribute("Cipher", cipher);

                state = ConnectionState.Connecting;
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
                    throw new SQLException(responseTag.getAttribute("text", "error text not found"));
                }

                serverAddress = responseTag.getAttribute("Address", null);
                serverPort = responseTag.getIntAttribute("Port", 0);

                if (serverAddress == null || serverPort == 0)
                {
                    throw new SQLException("no NuoDB nodes are available for database \"" + databaseName + "\"");
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

                protocolVersion = dataStream.Int;
                string serverKey = dataStream.String;
                string salt = dataStream.String;
                dataStream.ProtocolVersion = protocolVersion;

                if (protocolVersion >= Protocol.PROTOCOL_VERSION5)
                {
                    processConnection.DatabaseUUId = dataStream.UUId;
                }

                upperUserName = userName.ToUpper();
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

                state = ConnectionState.Open;
            }
            catch (SQLException e)
            {
                System.Diagnostics.Trace.WriteLine("NuoDBConnection::Open(): exception " + e.ToString());
                state = ConnectionState.Closed;
                if (authenticating)
                {
                    throw new SQLException("Authentication failed for database \"" + databaseName + "\"", e);
                }

                throw e;
            }
            catch (IOException exception)
            {
                System.Diagnostics.Trace.WriteLine("NuoDBConnection::Open(): exception " + exception.ToString());
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
                state = ConnectionState.Closed;

                throw new SQLException(exception.ToString());
            }
            catch (XmlException exception)
            {
                System.Diagnostics.Trace.WriteLine("NuoDBConnection::Open(): exception "+exception.ToString());
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
                state = ConnectionState.Closed;

                throw new SQLException(exception.ToString());
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Open()
        {
            if (state != ConnectionState.Closed)
                throw new ArgumentException("", "");

            if (!properties.ContainsKey("server"))
                throw new ArgumentException("The connection string doesn't include the URL of the server", "ConnectionString");
            if (!properties.ContainsKey("database"))
                throw new ArgumentException("The connection string doesn't include the name of the database", "ConnectionString");

            SQLException firstException = null;
            if (lastBroker != null)
            {
                try
                {
                    doOpen(lastBroker);
                    return;
                }
                catch (SQLException e)
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
                catch (SQLException e)
                {
                    if (firstException == null)
                        firstException = e;
                }
            }
            if (firstException != null)
                throw firstException;

        }

        public override DataTable GetSchema()
        {
            return this.GetSchema("");
        }

        public override DataTable GetSchema(string collectionName)
        {
            return this.GetSchema(collectionName, null);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            System.Diagnostics.Trace.Write("NuoDBConnection::GetSchema(\""+collectionName+"\", {");
            if (restrictionValues != null)
                for (int i = 0; i < restrictionValues.Length; i++)
                {
                    if(i!=0)
                        System.Diagnostics.Trace.Write(", ");
                    System.Diagnostics.Trace.Write(restrictionValues[i] == null ? "null" : restrictionValues[i]);
                }
            System.Diagnostics.Trace.WriteLine("})");
            if (state != ConnectionState.Open)
            {
                System.Diagnostics.Trace.WriteLine("NuoDBConnection::GetSchema() called on a closed connection");
            }

            DataTable table = new DataTable(collectionName);
            table.Locale = System.Globalization.CultureInfo.CurrentCulture;
            if (collectionName == DbMetaDataCollectionNames.DataSourceInformation)
            {
                // see http://msdn.microsoft.com/en-us/library/ms254501.aspx
                table.Columns.Add("CompositeIdentifierSeparatorPattern", typeof(string));
                table.Columns.Add("DataSourceProductName", typeof(string));
                table.Columns.Add("DataSourceProductVersion", typeof(string));
                table.Columns.Add("DataSourceProductVersionNormalized", typeof(string));
                table.Columns.Add("GroupByBehavior", typeof(int)); // see GroupByBehavior
                table.Columns.Add("IdentifierPattern", typeof(string));
                table.Columns.Add("IdentifierCase", typeof(int));    // see IdentifierCase 
                table.Columns.Add("OrderByColumnsInSelect", typeof(bool));
                table.Columns.Add("ParameterMarkerFormat", typeof(string));
                table.Columns.Add("ParameterMarkerPattern", typeof(string));
                table.Columns.Add("ParameterNameMaxLength", typeof(int));
                table.Columns.Add("ParameterNamePattern", typeof(string));
                table.Columns.Add("QuotedIdentifierPattern", typeof(string));
                table.Columns.Add("QuotedIdentifierCase", typeof(string));  // Regex
                table.Columns.Add("StatementSeparatorPattern", typeof(string));
                table.Columns.Add("StringLiteralPattern", typeof(string));  // Regex
                table.Columns.Add("SupportedJoinOperators", typeof(int));    // see SupportedJoinOperators 

                table.BeginLoadData();
                DataRow row = table.NewRow();
                try
                {
                    dataStream.startMessage(Protocol.GetDatabaseMetaData);
                    sendAndReceive(dataStream);
                    for (int item; (item = dataStream.Int) != Protocol.DbmbFini; )
                    {
                        switch (item)
                        {
                            case Protocol.DbmbProductName:
                                row["DataSourceProductName"] = dataStream.String;
                                break;

                            case Protocol.DbmbProductVersion:
                                row["DataSourceProductVersion"] = dataStream.String;
                                break;

                            case Protocol.DbmbDatabaseMinorVersion:
                                int databaseMinorVersion = dataStream.Int;
                                break;

                            case Protocol.DbmbDatabaseMajorVersion:
                                int databaseMajorVersion = dataStream.Int;
                                break;

                            case Protocol.DbmbDefaultTransactionIsolation:
                                int defaultTransactionIsolation = dataStream.Int;
                                break;

                            default:
                                dataStream.decode();
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                }

                row["ParameterNameMaxLength"] = 0;  // parameter names are not supported, use ?
                table.Rows.Add(row);
                table.EndLoadData();
            }
            else if (collectionName == DbMetaDataCollectionNames.ReservedWords)
            {
                table.Columns.Add("ReservedWord", typeof(string));
                table.Columns.Add("MaximumVersion", typeof(string));
                table.Columns.Add("MinimumVersion", typeof(string));
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

                dataStream.startMessage(Protocol.GetTypeInfo);
                sendAndReceive(dataStream);
                int handle = dataStream.Int;

                if (handle != -1)
                {
                    if (mapSQLTypes == null)
                        mapSQLTypes = new Dictionary<int, string>();

                    using (NuoDBDataReader reader = new NuoDBDataReader(this, handle, dataStream, null, true))
                    {
                        table.BeginLoadData();
                        while (reader.Read())
                        {
                            System.Diagnostics.Trace.WriteLine("-> " + reader["TYPE_NAME"] + "/" + reader["LOCAL_TYPE_NAME"] + "/" + reader["DATA_TYPE"] + "/" + reader["SQL_DATA_TYPE"]);
                            mapSQLTypes[(int)reader["DATA_TYPE"]] = reader["TYPE_NAME"].ToString().ToLower();
                            DataRow row = table.NewRow();
                            object s1 = reader["LOCAL_TYPE_NAME"];
                            object s2 = reader["PRECISION"];
                            object s3 = reader["SQL_DATETIME_SUB"];
                            object s4 = reader["NUM_PREC_RADIX"];
                            object s5 = reader["SQL_DATA_TYPE"];
                            row["NativeDataType"] = row["TypeName"] = reader["TYPE_NAME"].ToString().ToLower();
                            row["ProviderDbType"] = mapJavaSqlToDbType((int)reader["DATA_TYPE"]);
                            row["DataType"] = mapNuoDbToNetType(reader["TYPE_NAME"].ToString());
                            row["CreateParameters"] = reader["CREATE_PARAMS"];
                            row["IsBestMatch"] = true;
                            row["LiteralPrefix"] = reader["LITERAL_PREFIX"];
                            row["LitteralSuffix"] = reader["LITERAL_SUFFIX"];
                            row["IsSearchable"] = reader["SEARCHABLE"];
                            row["IsSearchableWithLike"] = reader["SEARCHABLE"];
                            row["IsCaseSensitive"] = reader["CASE_SENSITIVE"];
                            row["IsFixedPrecisionScale"] = reader["FIXED_PREC_SCALE"];
                            row["IsNullable"] = reader["NULLABLE"];
                            row["IsUnsigned"] = reader["UNSIGNED_ATTRIBUTE"];
                            row["IsAutoincrementable"] = reader["AUTO_INCREMENT"];
                            row["MaximumScale"] = reader["MAXIMUM_SCALE"];
                            row["MinimumScale"] = reader["MINIMUM_SCALE"];
                            table.Rows.Add(row);
                        }
                        table.EndLoadData();
                }
                }
            }
            else if (collectionName == "Tables")
            {
                table.Columns.Add("TABLE_CATALOG", typeof(string));
                table.Columns.Add("TABLE_SCHEMA", typeof(string));
                table.Columns.Add("TABLE_NAME", typeof(string));
                table.Columns.Add("TABLE_TYPE", typeof(string));
                table.Columns.Add("REMARKS", typeof(string));
                table.Columns.Add("VIEW_DEF", typeof(string));

                dataStream.startMessage(Protocol.GetTables);
                dataStream.encodeNull(); // catalog is always null
                for (int i = 1; i < 3; i++)
                    if (restrictionValues.Length > i)
                        dataStream.encodeString(restrictionValues[i]);
                    else
                        dataStream.encodeNull();
                if (restrictionValues.Length < 4 || restrictionValues[3] == null)
                    dataStream.encodeInt(0);
                else
                {
                    dataStream.encodeInt(1);
                    dataStream.encodeString(restrictionValues[3]);
                }

                sendAndReceive(dataStream);
                int handle = dataStream.Int;

                if (handle != -1)
                {
                    using (NuoDBDataReader reader = new NuoDBDataReader(this, handle, dataStream, null, true))
                    {
                        table.BeginLoadData();
                        while (reader.Read())
                        {
                            System.Diagnostics.Trace.WriteLine("-> " + reader["TABLE_NAME"] + ", " + reader["TABLE_TYPE"]);
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
                // trigger the creation of the map SqlType -> native name
                if (mapSQLTypes == null)
                    GetSchema(DbMetaDataCollectionNames.DataTypes);

                table.Columns.Add("COLUMN_CATALOG", typeof(string));
                table.Columns.Add("COLUMN_SCHEMA", typeof(string));
                table.Columns.Add("COLUMN_TABLE", typeof(string));
                table.Columns.Add("COLUMN_NAME", typeof(string));
                table.Columns.Add("COLUMN_POSITION", typeof(int));
                table.Columns.Add("COLUMN_TYPE", typeof(string));
                table.Columns.Add("COLUMN_LENGTH", typeof(int));
                table.Columns.Add("COLUMN_PRECISION", typeof(int));
                table.Columns.Add("COLUMN_NULLABLE", typeof(bool));

                dataStream.startMessage(Protocol.GetColumns);
                dataStream.encodeNull(); // catalog is always null
                for (int i = 1; i < 4; i++)
                    if (restrictionValues.Length > i)
                        dataStream.encodeString(restrictionValues[i]);
                    else
                        dataStream.encodeNull();

                sendAndReceive(dataStream);
                int handle = dataStream.Int;

                if (handle != -1)
                {
                    using (NuoDBDataReader reader = new NuoDBDataReader(this, handle, dataStream, null, true))
                    {
                        table.BeginLoadData();
                        while (reader.Read())
                        {
                            System.Diagnostics.Trace.WriteLine("-> " + reader["COLUMN_NAME"]);
                            DataRow row = table.NewRow();
                            row["COLUMN_SCHEMA"] = reader["TABLE_SCHEM"];
                            row["COLUMN_TABLE"] = reader["TABLE_NAME"];
                            row["COLUMN_NAME"] = reader["COLUMN_NAME"];
                            row["COLUMN_POSITION"] = reader["ORDINAL_POSITION"];
                            if (mapSQLTypes.ContainsKey((int)reader["DATA_TYPE"]))
                                row["COLUMN_TYPE"] = mapSQLTypes[(int)reader["DATA_TYPE"]];
                            row["COLUMN_LENGTH"] = reader["COLUMN_SIZE"];
                            row["COLUMN_PRECISION"] = reader["DECIMAL_DIGITS"];
                            if (!reader.IsDBNull(reader.GetOrdinal("IS_NULLABLE")))
                                row["COLUMN_NULLABLE"] = reader["IS_NULLABLE"].Equals("YES");
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

                dataStream.startMessage(Protocol.GetIndexInfo);
                dataStream.encodeNull(); // catalog is always null
                for (int i = 1; i < 3; i++)
                    if (restrictionValues.Length > i)
                        dataStream.encodeString(restrictionValues[i]);
                    else
                        dataStream.encodeNull();
                dataStream.encodeBoolean(false);    // unique
                dataStream.encodeBoolean(false);    // approximate
                sendAndReceive(dataStream);
                int handle = dataStream.Int;

                if (handle != -1)
                {
                    using (NuoDBDataReader reader = new NuoDBDataReader(this, handle, dataStream, null, true))
                    {
                        table.BeginLoadData();
                        while (reader.Read())
                        {
                            DataRow row = table.NewRow();
                            row["INDEX_SCHEMA"] = reader["TABLE_SCHEM"];
                            row["INDEX_TABLE"] = reader["TABLE_NAME"];
                            row["INDEX_NAME"] = reader["INDEX_NAME"];
                            row["INDEX_TYPE"] = reader["TYPE"];
                            row["INDEX_UNIQUE"] = reader["NON_UNIQUE"];
                            row["INDEX_PRIMARY"] = reader["NON_UNIQUE"];
                            table.Rows.Add(row);
                        }
                        table.EndLoadData();
                    }
                }
            }
            return table;
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

        internal static string mapNuoDbToNetType(string p)
        {
            p = p.ToLower();
            if (p == "string" || p == "varchar" || p == "longvarchar")
                return "System.String";
            if (p == "integer")
                return "System.Int32";
            if (p == "null")
                return "System.DBNull";
            if(p == "tinyint" || p == "smallint")
                return "System.Int16";
            if(p == "bigint")
                return "System.Int64";
            if(p == "float")
                return "System.Single";
            if(p == "double")
                return "System.Double";
            if(p == "char")
                return "System.Char";
            if(p == "date" || p == "time" || p == "timestamp")
                return "System.DateTime";
            if(p == "numeric" || p == "decimal")
                return "System.Decimal";
            if(p == "boolean")
                return "System.Boolean";
            if (p == "clob" || p == "blob")
                return "System.String";
            throw new NotImplementedException();
        }

        internal static DbType mapJavaSqlToDbType(int jSQL)
        {
            switch (jSQL)
            {
                case -7: //BIT
                    return DbType.Boolean;
                case -6: //TINYINT
                case 5: //SMALLINT
                    return DbType.Int16;
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
                    return DbType.Byte;
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

        public override string ServerVersion
        {
            get { return "1.0"; }
        }

        public override ConnectionState State
        {
            get { return state; }
        }

        internal void Commit()
        {
            dataStream.startMessage(Protocol.CommitTransaction);
            sendAndReceive(dataStream);

            // Both protocol V2 and V3 are sending a txn here

            long transactionId = dataStream.Long;

            // But only V3 sends the node id and commit sequence

            if (protocolVersion >= Protocol.PROTOCOL_VERSION3)
            {
                int nodeId = dataStream.Int;
                long commitSequence = dataStream.Long;
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

        protected override DbProviderFactory DbProviderFactory
        {
            get { return NuoDBProviderFactory.Instance; }
        }
    }
}
