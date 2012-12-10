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

namespace System.Data.NuoDB
{
    class NuoDBDataReader : DbDataReader
    {
        internal readonly NuoDBConnection connection;
        private int handle;
        private int numberColumns;
        private int numberRecords;
        internal string[] columnNames;
        private Value[] values;
        private bool lastValueNull;
        private DataTable metadata;
        private NuoDBCommand statement;
        private EncodedDataStream pendingRows;
        //private SQLWarning warnings;
        private volatile bool closed = false;
        private volatile int currentRow = 0;
        private volatile bool afterLast_Renamed = false;

        public NuoDBDataReader(NuoDBConnection connection, int handle, EncodedDataStream dataStream, NuoDBCommand statement, bool readColumnNames)
        {
            this.connection = connection;
            this.handle = handle;
            this.pendingRows = dataStream;
            this.statement = statement;

            this.connection.RegisterResultSet(this.handle);

            this.numberColumns = dataStream.Int;
            this.values = new Value[numberColumns];

            if (readColumnNames)
            {
                this.columnNames = new string[numberColumns];
                for (int n = 0; n < numberColumns; ++n)
                {
                    columnNames[n] = dataStream.String;
                }
            }
            else
            {
                //RemPreparedStatement ps = (RemPreparedStatement)statement;
                //columnNames = ps.columnNames;
            }

        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Trace.WriteLine("NuoDBDataReader::Dispose()");
            Close();
            base.Dispose(disposing);
        }

        public override void Close()
        {
            if (closed)
            {
                return;
            }

            connection.CloseResultSet(handle);

            statement = null;
            closed = true;
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override DataTable GetSchemaTable()
        {
            if(metadata != null)
                return metadata;

            EncodedDataStream dataStream = new EncodedDataStream();
            dataStream.startMessage(Protocol.GetMetaData);
            dataStream.encodeInt(handle);
            connection.sendAndReceive(dataStream);
            int numberColumns = dataStream.Int;

            metadata = new DataTable("schema");

            metadata.Columns.Add("ColumnOrdinal", typeof(int));
            metadata.Columns.Add("BaseCatalogName", typeof(string));
            metadata.Columns.Add("BaseSchemaName", typeof(string));
            metadata.Columns.Add("BaseTableName", typeof(string));
            metadata.Columns.Add("ColumnName", typeof(string));
            metadata.Columns.Add("DataTypeName", typeof(string));
            metadata.Columns.Add("DataType", typeof(string));
            metadata.Columns.Add("ProviderType", typeof(int));
            metadata.Columns.Add("ColumnSize", typeof(int));
            metadata.Columns.Add("NumericPrecision", typeof(int));
            metadata.Columns.Add("NumericScale", typeof(int));
            metadata.Columns.Add("IsAutoIncrement", typeof(bool));
            metadata.Columns.Add("IsReadOnly", typeof(bool));
            metadata.Columns.Add("AllowDBNull", typeof(bool));

            System.Diagnostics.Trace.WriteLine("Retrieving metadata");
            metadata.BeginLoadData();
			for (int n = 0; n < numberColumns; ++n)
			{
                DataRow row = metadata.NewRow();
                row["ColumnOrdinal"] = n;
                // data fields must be read in this exact order!
                row["BaseCatalogName"] = dataStream.String;
                row["BaseSchemaName"] = dataStream.String;
                row["BaseTableName"] = dataStream.String;
                row["ColumnName"] = dataStream.String;
                string columnLabel = dataStream.String;
                string collationSequence = dataStream.String;
                string dataType = dataStream.String;
                row["DataTypeName"] = dataType;
                row["DataType"] = NuoDBConnection.mapNuoDbToNetType(dataType);
                row["ProviderType"] = NuoDBConnection.mapJavaSqlToDbType(dataStream.Int);
                row["ColumnSize"] = dataStream.Int;
                row["NumericPrecision"] = dataStream.Int;
                row["NumericScale"] = dataStream.Int;
                int flags = dataStream.Int;
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

                System.Diagnostics.Trace.WriteLine("-> " + row["ColumnName"] + ", " + row["DataTypeName"] + ", " + row["ProviderType"]);
                metadata.Rows.Add(row);
			}
            metadata.EndLoadData();
            return metadata;
        }

        public override bool IsClosed
        {
            get { return closed; }
        }

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read()
        {
            //int maxRows = statement == null ? 0 : statement.MaxRows;
            int maxRows = 0;

            for (; ; )
            {
                if (maxRows > 0 && currentRow >= maxRows)
                {
                    afterLast_Renamed = true;

                    return false;
                }

                if (!pendingRows.EndOfMessage)
                {
                    int result = pendingRows.Int;

                    if (result == 0)
                    {
                        afterLast_Renamed = true;

                        return false;
                    }

                    ++numberRecords;
                    currentRow++;

                    for (int n = 0; n < numberColumns; ++n)
                    {
                        values[n] = pendingRows.Value;
                    }

                    //clearWarnings();

                    return true;
                }

                pendingRows = new RemEncodedStream(connection.protocolVersion);
                pendingRows.startMessage(Protocol.Next);
                pendingRows.encodeInt(handle);
                connection.sendAndReceive(pendingRows);
            }
        }

        public override int RecordsAffected
        {
            get 
            {
                // DataReader is used only for SELECT statements
                return -1; 
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
                throw new SQLException("ResultSet is closed");
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

            throw new SQLException("Column not found: " + columnLabel);
        }

        private string findColumn(int columnIndex)
        {
            checkIsClosed();

            if (columnIndex < 0 || columnIndex > (numberColumns - 1))
            {
                throw new SQLException(String.Format("ResultSet column index of {0}, out of bounds.  Valid range 0-{1}", columnIndex, numberColumns));
            }

            return columnNames[columnIndex];
        }

        private void checkForAccess()
        {
            checkIsClosed();

            if (currentRow == 0)
            {
                throw new SQLException("Before start of result set");
            }

            if (afterLast_Renamed)
            {
                throw new SQLException("After end of result set");
            }
        }

        private Value getValue(int columnIndex)
        {
            checkForAccess();

            if (columnIndex < 0 || columnIndex > (numberColumns - 1))
            {
                throw new SQLException(String.Format("ResultSet column index of {0}, out of bounds.  Valid range 0-{1}", columnIndex, numberColumns));
            }

            Value value = values[columnIndex];
            lastValueNull = value == null || value.Type == Value.Null;

            return value;
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
            throw new NotImplementedException();
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
            DataRow[] rows = GetSchemaTable().Select(String.Format("ColumnOrdinal = {0}", i));
            if (rows != null && rows.Length > 0)
            {
                return Convert.ToString(rows[0]["DataType"]);
            }
            throw new ArgumentOutOfRangeException("columnOrdinal", "Cannot find the requested column in the table metadata");
        }

        public override Type GetFieldType(int i)
        {
            DataRow[] rows = GetSchemaTable().Select(String.Format("ColumnOrdinal = {0}", i));
            if (rows != null && rows.Length > 0)
            {
                return Type.GetType(NuoDBConnection.mapDbTypeToNetType((int)rows[0]["ProviderType"]));
            }
            throw new ArgumentOutOfRangeException("columnOrdinal", "Cannot find the requested column in the table metadata");
        }

        public override float GetFloat(int i)
        {
            return getValue(i).Float;
        }

        public override Guid GetGuid(int i)
        {
            throw new NotImplementedException();
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

        public override object GetValue(int i)
        {
            return getValue(i).Object;
        }

        public override int GetValues(object[] values)
        {
            int toRead = Math.Min(this.numberColumns, values.Length);
            for (int i = 0; i < toRead; i++)
                values[i] = getValue(i).Object;
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
                return getValue(findColumn(name)).Object;
            }
        }

        public override object this[int i]
        {
            get
            {
                return getValue(i).Object;
            }
        }

        public override Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override bool HasRows
        {
            get { throw new NotImplementedException(); }
        }
    }
}
