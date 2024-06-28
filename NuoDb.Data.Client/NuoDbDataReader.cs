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
*  Jiri Cincura (jiri@cincura.net) 
****************************************************************************/

using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;

namespace NuoDb.Data.Client
{
    class NuoDbDataReader : DbDataReader
    {
        internal readonly NuoDbConnection connection;
        private int handle;
        private int numberColumns;
        private int numberRecords;
        internal string[] columnNames;
        private Value[] values;
        private DataTable metadata;
        private NuoDbCommand statement;
        private EncodedDataStream pendingRows;
        private volatile bool closed;
        private volatile int currentRow;
        private volatile bool afterLast;
        private int recordsAffected = -1;
        private Type[] declaredColumnTypes;
        private string[] declaredColumnTypeNames = null;

        public NuoDbDataReader(NuoDbConnection connection, int handle, EncodedDataStream dataStream, NuoDbCommand statement, bool readColumnNames)
        {
            this.connection = connection;
            this.statement = statement;
            InitResultSet(handle, dataStream, readColumnNames);
        }

        private void InitResultSet(int handle, EncodedDataStream dataStream, bool readColumnNames)
        {
            this.handle = handle;
            this.pendingRows = dataStream;
            this.metadata = null;
            
            if (this.handle != -1)
                this.connection.InternalConnection.RegisterResultSet(this.handle);

            this.numberRecords = 0;
            this.numberColumns = this.pendingRows != null ? this.pendingRows.getInt() : 0;
            this.values = new Value[numberColumns];
            this.closed = false;
            this.currentRow = 0;
            //this.afterLast = false;
            this.declaredColumnTypes = null;
            this.declaredColumnTypeNames = null;

            if (readColumnNames)
            {
                this.columnNames = new string[numberColumns];
                for (int n = 0; n < numberColumns; ++n)
                {
                    columnNames[n] = dataStream.getString();
                }
            }
            else
            {
                //RemPreparedStatement ps = (RemPreparedStatement)statement;
                //columnNames = ps.columnNames;
            }

            // Set afterLast to true if the ResultSet is empty.
            this.afterLast = (this.pendingRows == null || this.pendingRows.getInt() == 0);
        }

        protected override void Dispose(bool disposing)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbDataReader::Dispose()");
#endif
            Close();
            base.Dispose(disposing);
        }

        private void closeCurrentResultSet()
        {
            if (closed || handle == -1 || connection == null || (connection as IDbConnection).State == ConnectionState.Closed ||
                !connection.InternalConnection.IsResultSetRegistered(handle))
            {
                return;
            }

            connection.InternalConnection.CloseResultSet(handle);
            handle = -1;
        }

        public override void Close()
        {
            closeCurrentResultSet();
            statement = null;
            closed = true;
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override DataTable GetSchemaTable()
        {
            if (metadata != null)
                return metadata;

#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbDataReader.GetSchemaTable(" + statement.CommandText + ")");
#endif
            EncodedDataStream dataStream = new EncodedDataStream();
            dataStream.startMessage(Protocol.GetMetaData);
            dataStream.encodeInt(handle);
            connection.InternalConnection.sendAndReceive(dataStream);
            int numberColumns = dataStream.getInt();

            metadata = new DataTable("SchemaTable");

            // Info on the schema of this table is at http://msdn.microsoft.com/en-us/library/system.data.odbc.odbcdatareader.getschematable.aspx
            // The zero-based ordinal of the column. This column cannot contain a null value.
            metadata.Columns.Add("ColumnOrdinal", typeof(int));
            // The name of the column; this might not be unique. If the column name cannot be determined, a null value is returned. 
            // This name always reflects the most recent naming of the column in the current view or command text. 
            metadata.Columns.Add("ColumnName", typeof(string));
            // The maximum possible length of a value in the column. For columns that use a fixed-length data type, this is the size of the data type. 
            metadata.Columns.Add("ColumnSize", typeof(int));
            // If DbType is a numeric data type, this is the maximum precision of the column. The precision depends on the 
            // definition of the column. If DbType is not a numeric data type, do not use the data in this column.
            // If the underlying ODBC driver returns a precision value for a non-numeric data type, this value is used in the schema table. 
            metadata.Columns.Add("NumericPrecision", typeof(int));
            // If DbType is Decimal, the number of digits to the right of the decimal point. Otherwise, this is a null value. 
            // If the underlying ODBC driver returns a precision value for a non-numeric data type, this value is used in the schema table. 
            metadata.Columns.Add("NumericScale", typeof(int));
            // The name of the catalog in the data store that contains the column. NULL if the base catalog name cannot be determined.
            // The default for this column is a null value. 
            metadata.Columns.Add("BaseCatalogName", typeof(string));
            // The name of the schema in the data source that contains the column. NULL if the base catalog name cannot be determined. 
            // The default for this column is a null value. 
            metadata.Columns.Add("BaseSchemaName", typeof(string));
            // The name of the table or view in the data store that contains the column. A null value if the base table name cannot be determined.
            // The default of this column is a null value. 
            metadata.Columns.Add("BaseTableName", typeof(string));
            // The name of the column in the data store. This might be different from the column name returned in the ColumnName column if an alias was used. 
            // A null value if the base column name cannot be determined or if the rowset column is derived, but not identical to, a column in the data store. 
            // The default for this column is a null value. 
            metadata.Columns.Add("BaseColumnName", typeof(string));
            // Maps to the common language runtime type of DbType. 
            metadata.Columns.Add("DataType", typeof(Type));
            // The underlying driver type. 
            metadata.Columns.Add("ProviderType", typeof(int));
            // true if the column contains a Binary Long Object (BLOB) that contains very long data. The definition of very long data is driver-specific. 
            metadata.Columns.Add("IsLong", typeof(bool));
            // true if the column assigns values to new rows in fixed increments; otherwise false. The default for this column is false. 
            metadata.Columns.Add("IsAutoIncrement", typeof(bool));
            // true if the column cannot be modified; otherwise false. 
            metadata.Columns.Add("IsReadOnly", typeof(bool));
            // true: No two rows in the base table (the table returned in BaseTableName) can have the same value in this column. 
            // IsUnique is guaranteed to be true if the column represents a key by itself or if there is a constraint of type UNIQUE that applies only to this column.
            // false: The column can contain duplicate values in the base table. The default for this column is false. 
            metadata.Columns.Add("IsUnique", typeof(bool));
            // true: The column is one of a set of columns in the rowset that, taken together, uniquely identify the row. 
            // The set of columns with IsKey set to true must uniquely identify a row in the rowset. There is no requirement that this set of columns is a minimal set of columns. This set of columns may be generated from a base table primary key, a unique constraint, or a unique index.
            // false: The column is not required to uniquely identify the row. 
            metadata.Columns.Add("IsKey", typeof(bool));
            // Set if the column contains a persistent row identifier that cannot be written to, and has no meaningful value except to identity the row. 
            metadata.Columns.Add("IsRowVersion", typeof(bool));
            // true if the consumer can set the column to a null value or if the driver cannot determine whether the consumer can set the column to a null value. 
            // Otherwise, false. A column may contain null values, even if it cannot be set to a null value. 
            metadata.Columns.Add("AllowDBNull", typeof(bool));

            // The SQLDataReader also returns these columns:
            // Returns a string representing the data type of the specified column.
            metadata.Columns.Add("DataTypeName", typeof(string));
            // true: The column is an identity column.
            // false: The column is not an identity column.
            metadata.Columns.Add("IsIdentity", typeof(bool));
            // true: The column is an expression column.
            // false: The column is not an expression column.
            metadata.Columns.Add("IsExpression", typeof(bool));

            metadata.BeginLoadData();
            for (int n = 0; n < numberColumns; ++n)
            {
                DataRow row = metadata.NewRow();
                row["ColumnOrdinal"] = n;
                // data fields must be read in this exact order!
                row["BaseCatalogName"] = dataStream.getString();
                row["BaseSchemaName"] = dataStream.getString();
                row["BaseTableName"] = dataStream.getString();
                row["BaseColumnName"] = dataStream.getString();
                row["ColumnName"] = dataStream.getString();
                string collationSequence = dataStream.getString();
                row["DataTypeName"] = dataStream.getString();
                row["ProviderType"] = NuoDbConnectionInternal.mapJavaSqlToDbType(dataStream.getInt());
                row["ColumnSize"] = dataStream.getInt();
                row["NumericPrecision"] = dataStream.getInt();
                row["NumericScale"] = dataStream.getInt();
                if (((DbType)row["ProviderType"] == DbType.Int16 || (DbType)row["ProviderType"] == DbType.Int32 || (DbType)row["ProviderType"] == DbType.Int64) &&
                    (int)row["NumericScale"] != 0)
                {
                    row["ProviderType"] = DbType.Decimal;
                }
                row["DataType"] = Type.GetType(NuoDbConnectionInternal.mapNuoDbToNetType((string)row["DataTypeName"], (int)row["NumericPrecision"], (int)row["NumericScale"]));
                int flags = dataStream.getInt();
                const int rsmdSearchable = (1 << 1);
                const int rsmdAutoIncrement = (1 << 2);
                const int rsmdCaseSensitive = (1 << 3);
                const int rsmdCurrency = (1 << 4);
                const int rsmdDefinitelyWritable = (1 << 5);
                const int rsmdWritable = (1 << 6);
                const int rsmdReadOnly = (1 << 7);
                const int rsmdSigned = (1 << 8);
                const int rsmdNullable = (1 << 9);
                row["IsAutoIncrement"] = (flags & rsmdAutoIncrement) != 0;
                row["IsReadOnly"] = (flags & rsmdReadOnly) != 0;
                row["AllowDBNull"] = (flags & rsmdNullable) != 0;
                // for the moment, set the column to be a normal one; later we will look for primary indexes
                row["IsKey"] = row["IsIdentity"] = row["IsUnique"] = false;
                row["IsLong"] = row["IsRowVersion"] = row["IsExpression"] = false;
                metadata.Rows.Add(row);

#if DEBUG
                System.Diagnostics.Trace.WriteLine("-> " + row["ColumnName"] + ", " +
                                                            row["DataTypeName"] + "(" + row["NumericPrecision"] + "," + row["NumericScale"] + ") " +
                                                            row["DataType"]);
#endif
            }
            metadata.EndLoadData();
            // fill in the IsPrimary column
            Dictionary<string, DataTable> schemas = new Dictionary<string, DataTable>();
            foreach (DataRow row in metadata.Rows)
            {
                string key = row["BaseSchemaName"] + "|" + row["BaseTableName"];
                DataTable indexInfo = null;
                if (!schemas.ContainsKey(key))
                {
                    indexInfo = connection.GetSchema("IndexColumns", new string[] { null, (string)row["BaseSchemaName"], (string)row["BaseTableName"] });
                    schemas.Add(key, indexInfo);
                }
                else
                    indexInfo = schemas[key];
                DataRow[] rows = indexInfo.Select(String.Format("INDEXCOLUMN_NAME = '{0}' AND INDEXCOLUMN_ISPRIMARY = true", row["BaseColumnName"]));
                if (rows != null && rows.Length > 0)
                {
                    row.BeginEdit();
                    row["IsKey"] = row["IsIdentity"] = true;
                    row.EndEdit();
                }
            }
            return metadata;
        }

        public override bool IsClosed
        {
            get { return closed; }
        }

        public override bool NextResult()
        {
            closeCurrentResultSet();

            EncodedDataStream dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
            dataStream.startMessage(Protocol.GetMoreResults);
            dataStream.encodeInt(statement.handle);
            connection.InternalConnection.sendAndReceive(dataStream);
            if (dataStream.getInt() != 1)
                return false;
            
            dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
            dataStream.startMessage(Protocol.GetResultSet);
            dataStream.encodeInt(statement.handle);
            connection.InternalConnection.sendAndReceive(dataStream);
            int rsHandle = dataStream.getInt();
            if (rsHandle == -1)
                return false;
            InitResultSet(rsHandle, dataStream, true);
            return true;
        }

        public override bool Read()
        {
            //afterLast can only be false if pendingRows was non-null in InitResultSet().
            if (afterLast)
                return false;

            //int maxRows = statement == null ? 0 : statement.MaxRows;
            int maxRows = 0;

            for (; ; )
            {
                if (maxRows > 0 && currentRow >= maxRows)
                {
                    afterLast = true;

                    return false;
                }

                if (!pendingRows.EndOfMessage)
                {
                    // InitResultSet() performs the pendingRows.getInt() for currentRow == 0
                    int result = currentRow > 0 ? pendingRows.getInt() : -1;
                    if (result == 0)
                    {
                        afterLast = true;

                        return false;
                    }

                    ++numberRecords;
                    currentRow++;

                    for (int n = 0; n < numberColumns; ++n)
                    {
                        values[n] = pendingRows.getValue(connection.InternalConnection.sqlContext);
                    }

                    //clearWarnings();

                    return true;
                }

                pendingRows = new RemEncodedStream(connection.InternalConnection.protocolVersion);
                pendingRows.startMessage(Protocol.Next);
                pendingRows.encodeInt(handle);
                connection.InternalConnection.sendAndReceive(pendingRows);
            }
        }

        internal int UpdatedRecords
        {
            set
            {
                recordsAffected = value;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return recordsAffected;
            }
        }

        public override int FieldCount
        {
            get { return numberColumns; }
        }

        private void checkIsClosed()
        {
            if (closed)
            {
                throw new NuoDbSqlException("ResultSet is closed");
            }
        }

        private int findColumn(string columnLabel)
        {
            checkIsClosed();

            for (int i = 0; i < columnNames.Length; i++)
            {
                if (columnNames[i].ToUpper() == columnLabel.ToUpper())
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException("Column not found: " + columnLabel);
        }

        private string findColumn(int columnIndex)
        {
            checkIsClosed();

            if (columnIndex < 0 || columnIndex > (numberColumns - 1))
            {
                throw new IndexOutOfRangeException(String.Format("ResultSet column index of {0}, out of bounds.  Valid range 0-{1}", columnIndex, numberColumns-1));
            }

            return columnNames[columnIndex];
        }

        private void checkForAccess()
        {
            checkIsClosed();

            if (currentRow == 0)
            {
                throw new NuoDbSqlException("Before start of result set");
            }

            if (afterLast)
            {
                throw new NuoDbSqlException("After end of result set");
            }
        }

        private Value getValue(int columnIndex)
        {
            checkForAccess();

            if (columnIndex < 0 || columnIndex > (numberColumns - 1))
            {
                throw new IndexOutOfRangeException(String.Format("ResultSet column index of {0}, out of bounds.  Valid range 0-{1}", columnIndex, numberColumns-1));
            }

            return values[columnIndex];
        }

        public override bool GetBoolean(int i)
        {
            return getValue(i).Boolean;
        }

        public override byte GetByte(int i)
        {
            return getValue(i).Byte;
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int i)
        {
            return getValue(i).String.FirstOrDefault();
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int i)
        {
            return getValue(i).Date;
        }

        public override decimal GetDecimal(int i)
        {
            return getValue(i).BigDecimal;
        }

        public override double GetDouble(int i)
        {
            return getValue(i).Double;
        }

        public override string GetDataTypeName(int i)
        {
            if (declaredColumnTypeNames == null)
            {
                DataRowCollection rows = GetSchemaTable().Rows;
                declaredColumnTypeNames = new string[rows.Count];
                foreach (DataRow row in rows)
                {
                    int ordinal = (int)row["ColumnOrdinal"];
                    declaredColumnTypeNames[ordinal] = (string)row["DataTypeName"];
                }
            }
            return declaredColumnTypeNames[i];
        }

        public override Type GetFieldType(int i)
        {
            if (declaredColumnTypes == null)
            {
                if (statement != null)
                    declaredColumnTypes = statement.ExpectedColumnTypes;

                if (declaredColumnTypes == null)
                {
                    DataRowCollection rows = GetSchemaTable().Rows;
                    declaredColumnTypes = new Type[rows.Count];
                    foreach (DataRow row in rows)
                    {
                        int ordinal = (int)row["ColumnOrdinal"];
                        declaredColumnTypes[ordinal] = Type.GetType(NuoDbConnectionInternal.mapDbTypeToNetType((int)row["ProviderType"]));
                    }
                }
            }
            return declaredColumnTypes[i];
        }

        public override float GetFloat(int i)
        {
            return getValue(i).Float;
        }

        public override Guid GetGuid(int i)
        {
#if NET_40
            return Guid.Parse(GetString(i));
#else
            return new Guid(GetString(i));
#endif
        }

        public override short GetInt16(int i)
        {
            return getValue(i).Short;
        }

        public override int GetInt32(int i)
        {
            return getValue(i).Int;
        }

        public override long GetInt64(int i)
        {
            return getValue(i).Long;
        }

        public override string GetName(int i)
        {
            return findColumn(i);
        }

        public override int GetOrdinal(string name)
        {
            return findColumn(name);
        }

        public override string GetString(int i)
        {
            return getValue(i).String;
        }

        public virtual T GetFieldValue<T>(int ordinal)
        {
            object value = getValue(ordinal).Object;
            if (value == null)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(getValue(ordinal).Object, typeof(T));
        }

        public virtual T GetFieldValue<T>(string name)
        {
            return GetFieldValue<T>(findColumn(name));
        }

        public override object GetValue(int i)
        {
            object value = getValue(i).Object;
            if (value == null)
            {
                return value;
            }
            Type declaredType = null;
            if (declaredColumnTypes != null)
            {
                declaredType = declaredColumnTypes.ElementAtOrDefault(i);
            }
            else if (Value.IsNumeric(value))
            {
                // if we have received a numeric value, it could have been sent as a smaller datatype
                // to save bandwidth, so get the official declared type
                try
                {
                    declaredType = GetFieldType(i);
                    // if we have a mismatch between the actual type and the declared type, prefer the
                    // actual type
                    if (!Value.IsNumeric(declaredType))
                        declaredType = null;
                }
                catch (Exception)
                {
                    // if we cannot get the declared type, return the data we have
                }
            }
            if (declaredType != null)
            {
                if (declaredType == typeof(Guid))
                {
                    return GetGuid(i);
                }
                else
                {
                    return Convert.ChangeType(value, declaredType);
                }
            }

            return value;
        }

        public override int GetValues(object[] values)
        {
            int toRead = Math.Min(this.numberColumns, values.Length);
            for (int i = 0; i < toRead; i++)
                values[i] = GetValue(i);
            for (int i = toRead; i < values.Length; i++)
                values[i] = null;
            return toRead;
        }

        public override bool IsDBNull(int i)
        {
            return (getValue(i).Type == Value.Null);
        }

        public override object this[string name]
        {
            get
            {
                return GetValue(findColumn(name));
            }
        }

        public override object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, false);
        }

        public override bool HasRows
        {
            get { return (!afterLast); }
        }
    }
}
