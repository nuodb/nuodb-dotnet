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
using System;
using System.ComponentModel;

namespace NuoDb.Data.Client
{
	public class NuoDbConnectionStringBuilder : DbConnectionStringBuilder
	{
		internal const string UserKey = "User";
		[Category("Security")]
		[Description("Indicates the username to be used when connecting to the data source")]
		public string User
		{
			get { return this.GetString(UserKey); }
			set { this.SetValue(UserKey, value); }
		}

		internal const string PasswordKey = "Password";
		[Category("Security")]
		[Description("Indicates the password to be used when connecting to the data source")]
		[PasswordPropertyText(true)]
		public string Password
		{
			get { return this.GetString(PasswordKey); }
			set { this.SetValue(PasswordKey, value); }
		}

		internal const string ServerKey = "Server";
		[Category("Source")]
		[Description("Indicates one or more addresses of the brokers to be contacted, separated by commas")]
		public string Server
		{
			get { return this.GetString(ServerKey); }
			set { this.SetValue(ServerKey, value); }
		}

		internal const string DatabaseKey = "Database";
		[Category("Source")]
		[Description("Indicates the chorus to be used as data source")]
		public string Database
		{
			get { return this.GetString(DatabaseKey); }
			set { this.SetValue(DatabaseKey, value); }
		}

		internal const string SchemaKey = "Schema";
		[Category("Source")]
		[Description("Indicates the default schema to be used when referring to non-fully qualified tables")]
		public string Schema
		{
			get { return this.GetString(SchemaKey); }
			set { this.SetValue(SchemaKey, value); }
		}

		internal const string PoolingKey = "Pooling";
		internal const bool PoolingDefault = true;
		[Category("Connection Pooling")]
		[Description("")]
		public bool Pooling
		{
			get { return this.GetBoolean(PoolingKey); }
			set { this.SetValue(PoolingKey, value); }
		}
		internal bool PoolingOrDefault
		{
			get { return ContainsKey(PoolingKey) ? Pooling : PoolingDefault; }
		}

		internal const string ConnectionLifetimeKey = "ConnectionLifetime";
		internal const int ConnectionLifetimeDefault = 10;
		[Category("Connection Pooling")]
		[Description("")]
		public int ConnectionLifetime
		{
			get { return this.GetInt32(ConnectionLifetimeKey); }
			set { this.SetValue(ConnectionLifetimeKey, value); }
		}
		internal int ConnectionLifetimeOrDefault
		{
			get { return ContainsKey(ConnectionLifetimeKey) ? ConnectionLifetime : ConnectionLifetimeDefault; }
		}

		public NuoDbConnectionStringBuilder()
		{
		}

		public NuoDbConnectionStringBuilder(string connectionString)
		{
			this.ConnectionString = connectionString;
		}

		string GetString(string keyword)
		{
			return GetValue(keyword, Convert.ToString);
		}

		bool GetBoolean(string keyword)
		{
			return GetValue(keyword, Convert.ToBoolean);
		}

		int GetInt32(string keyword)
		{
			return GetValue(keyword, Convert.ToInt32);
		}

		T GetValue<T>(string keyword, Converter<object, T> converter)
		{
			return converter(this[keyword]);
		}

		void SetValue<T>(string keyword, T value)
		{
			this[keyword] = value;
		}
	}
}
