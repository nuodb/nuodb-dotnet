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
****************************************************************************/

using System.Data.Common;
using System;
using System.ComponentModel;

namespace NuoDb.Data.Client
{
    public class NuoDbConnectionStringBuilder : DbConnectionStringBuilder
    {
        [Category("Security")]
        [Description("Indicates the username to be used when connecting to the data source")]
        public string User
        {
            get { return this.GetString("User"); }
            set { this.SetValue("User", value); }
        }

        [Category("Security")]
        [Description("Indicates the password to be used when connecting to the data source")]
        [PasswordPropertyText(true)]
        public string Password
        {
            get { return this.GetString("Password"); }
            set { this.SetValue("Password", value); }
        }

        [Category("Source")]
        [Description("Indicates one or more addresses of the brokers to be contacted, separated by commas")]
        public string Server
        {
            get { return this.GetString("Server"); }
            set { this.SetValue("Server", value); }
        }

        [Category("Source")]
        [Description("Indicates the chorus to be used as data source")]
        public string Database
        {
            get { return this.GetString("Database"); }
            set { this.SetValue("Database", value); }
        }

        [Category("Source")]
        [Description("Indicates the default schema to be used when referring to non-fully qualified tables")]
        public string Schema
        {
            get { return this.GetString("Schema"); }
            set { this.SetValue("Schema", value); }
        }

		[Category("Connection Pooling")]
		[Description("")]
		public bool Pooling
		{
			get { return this.GetBoolean("Pooling"); }
			set { this.SetValue("Pooling", value); }
		}

		[Category("Connection Pooling")]
		[Description("")]
		public int ConnectionLifetime
		{
			get { return this.GetInt32("ConnectionLifetime"); }
			set { this.SetValue("ConnectionLifetime", value); }
		}

        public NuoDbConnectionStringBuilder()
		{
		}

        public NuoDbConnectionStringBuilder(string connectionString)
		{
			this.ConnectionString = connectionString;
		}

        private string GetString(string keyword)
        {
			return GetValue(keyword, Convert.ToString);
        }

		private bool GetBoolean(string keyword)
		{
			return GetValue(keyword, Convert.ToBoolean);
		}

		private int GetInt32(string keyword)
		{
			return GetValue(keyword, Convert.ToInt32);
		}

		private T GetValue<T>(string keyword, Converter<object, T> converter)
		{
			return converter(this[keyword]);
		}

        private void SetValue<T>(string keyword, T value)
        {
            this[keyword] = value;
        }

    }
}
