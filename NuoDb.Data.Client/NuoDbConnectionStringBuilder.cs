﻿/****************************************************************************
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
*  Jiri Cincura (jiri@cincura.net) 
****************************************************************************/

using System.Data.Common;
using System;
using System.ComponentModel;

namespace NuoDb.Data.Client
{
    public class ApplicationIntentListConverter : TypeConverter
    {
        // return true to signal we have a set of allowed values to display
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        // return true if the value can only be one of the allowed values (drop-list)
        // return false if the user is allowed to type in a different value (drop-down)
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        // return the allowed values
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "", "ReadOnly", "ReadWrite" });
        }
    }

    public class SQLEngineListConverter : TypeConverter
    {
        // return true to signal we have a set of allowed values to display
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        // return true if the value can only be one of the allowed values (drop-list)
        // return false if the user is allowed to type in a different value (drop-down)
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        // return the allowed values
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "", "VEE", "Legacy" });
        }
    }

    public class IsolationLevelsListConverter : TypeConverter
    {
        // return true to signal we have a set of allowed values to display
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        // return true if the value can only be one of the allowed values (drop-list)
        // return false if the user is allowed to type in a different value (drop-down)
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        // return the allowed values
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "", "ReadCommitted", "WriteCommitted", "ConsistentRead", "Serializable"});
        }
    }

    public class EncryptionsListConverter : TypeConverter
    {
        // return true to signal we have a set of allowed values to display
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        // return true if the value can only be one of the allowed values (drop-list)
        // return false if the user is allowed to type in a different value (drop-down)
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        // return the allowed values
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "", "None", "RC4" });
        }
    }

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
        [Category("Pooling")]
        [Description("Indicates whether the connections should be recycled instead of discarded as soon as they are closed")]
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
        [Category("Pooling")]
        [Description("Specifies the number of seconds that a connection can stay in the pool of available connections before being discarded")]
        public int ConnectionLifetime
        {
            get { return this.GetInt32(ConnectionLifetimeKey); }
            set { this.SetValue(ConnectionLifetimeKey, value); }
        }
        internal int ConnectionLifetimeOrDefault
        {
            get { return ContainsKey(ConnectionLifetimeKey) ? ConnectionLifetime : ConnectionLifetimeDefault; }
        }

        internal const string MaxLifetimeKey = "MaxLifetime";
        internal const int MaxLifetimeDefault = 10000;
        [Category("Pooling")]
        [Description("Specifies the maximum number of seconds that a connection can live before being discarded")]
        public int MaxLifetime
        {
            get { return this.GetInt32(MaxLifetimeKey); }
            set { this.SetValue(MaxLifetimeKey, value); }
        }
        internal int MaxLifetimeOrDefault
        {
            get { return ContainsKey(MaxLifetimeKey) ? MaxLifetime : MaxLifetimeDefault; }
        }

        internal const string MaxConnectionsKey = "MaxConnections";
        internal const int MaxConnectionsDefault = 100;
        [Category("Pooling")]
        [Description("Specifies the maximum number of connections that can be opened at the same time")]
        public int MaxConnections
        {
            get { return this.GetInt32(MaxConnectionsKey); }
            set { this.SetValue(MaxConnectionsKey, value); }
        }
        internal int MaxConnectionsOrDefault
        {
            get { return ContainsKey(MaxConnectionsKey) ? MaxConnections : MaxConnectionsDefault; }
        }

        internal const string TimeZoneKey = "TimeZone";
        [Category("General")]
        [Description("Specifies the timezone of the client application")]
        public string TimeZone
        {
            get { return this.GetString(TimeZoneKey); }
            set { this.SetValue(TimeZoneKey, value); }
        }

        internal const string IsolationLevelKey = "IsolationLevel";
        [Category("General")]
        [Description("Specifies the default isolation level of the connection")]
        [TypeConverter(typeof(IsolationLevelsListConverter))]
        public string IsolationLevel
        {
            get { return this.GetString(IsolationLevelKey); }
            set { this.SetValue(IsolationLevelKey, value); }
        }

        internal const string LBTagKey = "LBTag";
        [Category("Load Balancing")]
        [Description("Specifies the tag of the connection that the load balancer will use")]
        public string LBTag
        {
            get { return this.GetString(LBTagKey); }
            set { this.SetValue(LBTagKey, value); }
        }

        internal const string ClientInfoKey = "ClientInfo";
        [Category("Connection Description")]
        [Description("Specifies the user-specific info of the connection that will appear in the SYSTEM.CONNECTIONS and SYSTEM.LOCALCONNECTIONS tables")]
        public string ClientInfo
        {
            get { return this.GetString(ClientInfoKey); }
            set { this.SetValue(ClientInfoKey, value); }
        }

        internal const string CipherKey = "Cipher";
        [Category("Security")]
        [Description("Specifies the encryption method used to secure the communication channel")]
        [TypeConverter(typeof(EncryptionsListConverter))]
        public string Cipher
        {
            get { return this.GetString(CipherKey); }
            set { this.SetValue(CipherKey, value); }
        }

        internal const string ApplicationIntentKey = "ApplicationIntent";
        [Category("General")]
        [Description("Specifies whether this connection is allowed to perform update operation")]
        [TypeConverter(typeof(ApplicationIntentListConverter))]
        public string ApplicationIntent
        {
            get { return this.GetString(ApplicationIntentKey); }
            set { this.SetValue(ApplicationIntentKey, value); }
        }
        internal const string SQLEngineKey = "SQLEngine";
        [Category("General")]
        [Description("Specifies the SQL engine that will be used to process commands in this connection")]
        [TypeConverter(typeof(SQLEngineListConverter))]
        public string SQLEngine
        {
            get { return this.GetString(SQLEngineKey); }
            set { this.SetValue(SQLEngineKey, value); }
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
