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

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace NuoDb.Data.Client
{
    public class NuoDbBulkLoader
    {
        private List<BatchProcessedEventHandler> handlers = new List<BatchProcessedEventHandler>();
        private NuoDbConnection connection;
        private NuoDbBulkLoaderColumnMappingCollection mappings = new NuoDbBulkLoaderColumnMappingCollection();
        private int batchSize = 5000;
        private string tableName = "";
        private const int EXECUTE_FAILED = -3;

        // Summary:
        //     Gets or sets the mappings between source and target to be used when loading the data
        //
        // Returns:
        //     The list of mappings between source and target columns
        //
        public NuoDbBulkLoaderColumnMappingCollection ColumnMappings
        {
            get { return mappings; }
            set { mappings = value; }
        }

        // Summary:
        //     Gets or sets the name of the table that will be loaded with the data. The name can contain
        //     an optional schema in the format schema.table
        //
        // Returns:
        //     The name of the target table.
        //
        public string DestinationTableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        // Summary:
        //     Gets or sets the size of each batch of rows that are sent to the server on each iteration
        //
        // Returns:
        //     The dimension of the batch of rows
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     The size of the batch must be greater than 0
        public int BatchSize
        {
            get { return batchSize; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("BatchSize", "The size of a batch must be a positive number");
                batchSize = value;
            }
        }

        // Summary:
        //     Add or remove an handler that will be invoked after a batch of rows has been sent to the server
        //
        public event BatchProcessedEventHandler BatchProcessed
        {
            add
            {
                handlers.Add(value);
            }

            remove
            {
                handlers.Remove(value);
            }
        }

        // Summary:
        //     Initializes a new bulk loader using an existing connection.
        public NuoDbBulkLoader(NuoDbConnection connection)
        {
            this.connection = connection;
        }

        // Summary:
        //     Initializes a new bulk loader using a new connection created using the provided connection string.
        public NuoDbBulkLoader(string connectionString)
        {
            this.connection = new NuoDbConnection(connectionString);
        }

        private interface Feeder
        {
            bool MoveNext();
            int FieldCount { get; }
            object this[int i] { get; }
            object this[string name] { get; }
        }

        private class WrapDataRowAsFeeder : Feeder
        {
            int pos = -1;
            DataRow[] wrappedRow;

            public WrapDataRowAsFeeder(DataRow[] rows)
            {
                wrappedRow = rows;
            }

            public int FieldCount
            {
                get { return wrappedRow[0].ItemArray.Length; }
            }
            public object this[string name]
            {
                get { return wrappedRow[pos][name]; }
            }
            public object this[int i]
            {
                get { return wrappedRow[pos][i]; }
            }
            public bool MoveNext()
            {
                if ((pos + 1) >= wrappedRow.Length)
                    return false;

                pos++;
                return true;
            }
        }

        private class WrapDataRowCollectionAsFeeder : Feeder
        {
            int pos = -1;
            DataRowCollection wrappedRow;
            DataRowState rowState;

            public WrapDataRowCollectionAsFeeder(DataRowCollection rows, DataRowState state)
            {
                wrappedRow = rows;
                rowState = state;
            }

            public int FieldCount
            {
                get { return wrappedRow[0].ItemArray.Length; }
            }
            public object this[string name]
            {
                get { return wrappedRow[pos][name]; }
            }
            public object this[int i]
            {
                get { return wrappedRow[pos][i]; }
            }
            public bool MoveNext()
            {
                while (true)
                {
                    if ((pos + 1) >= wrappedRow.Count)
                        return false;

                    if ((wrappedRow[++pos].RowState & rowState) != 0)
                        return true;
                }
                // never reached
            }
        }

        private class WrapDataReaderAsFeeder : Feeder
        {
            private IDataReader wrappedReader;

            public WrapDataReaderAsFeeder(IDataReader reader)
            {
                wrappedReader = reader;
            }

            public int FieldCount
            {
                get { return wrappedReader.FieldCount; }
            }

            public object this[int i]
            {
                get { return wrappedReader[i]; }
            }

            public object this[string name]
            {
                get { return wrappedReader[name]; }
            }

            public bool MoveNext()
            {
                return wrappedReader.Read();
            }

        }

        private void WriteToServer(Feeder feeder)
        {
            if (this.tableName.Length == 0)
                throw new ArgumentException("The name of the destination table hasn't been specified", "DestinationTableName");

            StringBuilder builder = new StringBuilder();
            builder.Append("INSERT INTO `");
            builder.Append(this.tableName.Replace("`", "``"));
            builder.Append("` ");
            if (mappings.Count == 0)
            {
                // the target table has the same number and names of the columns as in the specified input rows
                builder.Append("VALUES (");
                for (int i = 0; i < feeder.FieldCount; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    builder.Append("?");
                }
                builder.Append(")");
            }
            else
            {
                DataRowCollection targetColumns = null;
                builder.Append(" (");
                for (int i = 0; i < mappings.Count; i++)
                {
                    NuoDbBulkLoaderColumnMapping mapping = mappings[i];
                    if (i != 0)
                        builder.Append(", ");
                    builder.Append("`");
                    if (mapping.DestinationColumn == null)
                    {
                        // we are requested to map to a target column that is identified with its ordinal number, so 
                        // fetch the schema of the target table to find out what is its name
                        if (targetColumns == null)
                        {
                            // split the destination table into its different parts
                            string[] parts = this.tableName.Split(new char[] { '.' });

                            DataTable targetSchema = this.connection.GetSchema("Columns", new string[] { null, // catalog
                                                                                    parts.Length == 2 ? parts[0] : null, // schema
                                                                                    parts.Length == 2 ? parts[1] : parts[0] // table
                                                                                });
                            targetColumns = targetSchema.Rows;
                        }

                        if (mapping.DestinationOrdinal < 0 || mapping.DestinationOrdinal > targetColumns.Count)
                            throw new IndexOutOfRangeException(String.Format("The specified ordinal of the target column ({0}) is outside the range of the column count ({1}) of table {2}",
                                new object[] { mapping.DestinationOrdinal, targetColumns.Count, this.tableName }));

                        string columnName = (string)(targetColumns[mapping.DestinationOrdinal]["COLUMN_NAME"]);
                        builder.Append(columnName.Replace("`", "``"));
                    }
                    else
                        builder.Append(mapping.DestinationColumn.Replace("`", "``"));
                    builder.Append("`");
                }
                builder.Append(") VALUES (");
                for (int i = 0; i < mappings.Count; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    builder.Append("?");
                }
                builder.Append(")");
            }
            string sqlString = builder.ToString();
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbBulkLoader::WriteToServer: " + sqlString);
#endif

            if (this.connection.State != ConnectionState.Open)
                this.connection.Open();

            using (NuoDbCommand command = new NuoDbCommand(sqlString, this.connection))
            {
                command.Prepare();
                int totalSize = 0;

                // do the check for out-of-range values just once
                foreach (NuoDbBulkLoaderColumnMapping mapping in mappings)
                {
                    if (mapping.SourceColumn == null && mapping.SourceOrdinal < 0 || mapping.SourceOrdinal > feeder.FieldCount)
                        throw new IndexOutOfRangeException(String.Format("The specified ordinal of the source column ({0}) is outside the range of the column count ({1})",
                            new object[] { mapping.SourceOrdinal, feeder.FieldCount }));
                }

                while (true)
                {
                    EncodedDataStream dataStream = new RemEncodedStream(connection.InternalConnection.protocolVersion);
                    dataStream.startMessage(Protocol.ExecuteBatchPreparedStatement);
                    dataStream.encodeInt(command.handle);
                    int batchCount = 0;

                    for (; batchCount < this.batchSize && feeder.MoveNext(); batchCount++)
                    {
                        dataStream.encodeInt(command.Parameters.Count);
                        if (mappings.Count == 0)
                        {
                            for (int i = 0; i < feeder.FieldCount; i++)
                            {
                                dataStream.encodeDotNetObject(feeder[i]);
                            }
                        }
                        else
                        {
                            foreach (NuoDbBulkLoaderColumnMapping mapping in mappings)
                            {
                                if (mapping.SourceColumn == null)
                                {
                                    dataStream.encodeDotNetObject(feeder[mapping.SourceOrdinal]);
                                }
                                else
                                {
                                    dataStream.encodeDotNetObject(feeder[mapping.SourceColumn]);
                                }
                            }
                        }
                    }

                    // the iterator hasn't found any more data to import, let's break out
                    if (batchCount == 0)
                        break;

                    dataStream.encodeInt(-1);
                    dataStream.encodeInt(batchCount);
                    totalSize += batchCount;
#if DEBUG
                    System.Diagnostics.Trace.WriteLine("NuoDbBulkLoader::WriteToServer: sending a batch of " + batchCount + " rows");
#endif
                    this.connection.InternalConnection.sendAndReceive(dataStream);

                    bool hasErrors = false;
                    string errorMessage = string.Empty;

                    for (int i = 0; i < batchCount; i++)
                    {
                        int result = dataStream.getInt();
                        if (result == EXECUTE_FAILED)
                        {
                            if (this.connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION6)
                            {
                                int sqlCode = dataStream.getInt();
                                string message = dataStream.getString();

                                errorMessage = AppendError(errorMessage, message, i);
                            }
                            hasErrors = true;
                        }
                    }

                    if (this.connection.InternalConnection.protocolVersion >= Protocol.PROTOCOL_VERSION3)
                    {
                        long txnId = dataStream.getLong();
                        int nodeId = dataStream.getInt();
                        long commitSequence = dataStream.getLong();
                        this.connection.InternalConnection.setLastTransaction(txnId, nodeId, commitSequence);
                    }

                    if (handlers.Count != 0)
                    {
                        BatchProcessedEventHandler[] tmpArray = new BatchProcessedEventHandler[handlers.Count];
                        handlers.CopyTo(tmpArray);
                        BatchProcessedEventArgs args = new BatchProcessedEventArgs();
                        args.BatchSize = batchCount;
                        args.TotalSize = totalSize;
                        args.HasErrors = hasErrors;
                        foreach (BatchProcessedEventHandler h in tmpArray)
                        {
                            h.Invoke(this, args);
                        }
                    }

                    if (hasErrors)
                    {
                        throw new NuoDbSqlException(errorMessage, NuoDbSqlCode.FindError("BATCH_UPDATE_ERROR"));
                    }
                }
            }
        }

        // Summary:
        //     Loads the specified set of DataRow objects into the target table
        //
        // Exceptions:
        //   System.ArgumentException:
        //     If the name of the target table hasn't been specified
        //
        //   System.IndexOutOfRangeException:
        //     If the mapping specifies a column ordinal that is outside of the allowed range
        //
        //   NuoDb.Data.Client.NuoDbSqlException:
        //     If the server returns an error
        public void WriteToServer(DataRow[] rows)
        {
            if (rows.Length == 0)
                return;     // empty array, nothing to do

            Feeder feeder = new WrapDataRowAsFeeder(rows);
            WriteToServer(feeder);
        }

        // Summary:
        //     Loads the specified DataTable object into the target table
        //
        // Exceptions:
        //   System.ArgumentException:
        //     If the name of the target table hasn't been specified
        //
        //   System.IndexOutOfRangeException:
        //     If the mapping specifies a column ordinal that is outside of the allowed range
        //
        //   NuoDb.Data.Client.NuoDbSqlException:
        //     If the server returns an error
        public void WriteToServer(DataTable table)
        {
            WriteToServer(table, DataRowState.Added | DataRowState.Deleted | DataRowState.Detached | DataRowState.Modified | DataRowState.Unchanged);
        }

        // Summary:
        //     Loads the specified DataTable object into the target table, skipping the rows that aren't in the
        //     requested state
        //
        // Exceptions:
        //   System.ArgumentException:
        //     If the name of the target table hasn't been specified
        //
        //   System.IndexOutOfRangeException:
        //     If the mapping specifies a column ordinal that is outside of the allowed range
        //
        //   NuoDb.Data.Client.NuoDbSqlException:
        //     If the server returns an error
        public void WriteToServer(DataTable table, DataRowState state)
        {
            if (table.Rows.Count == 0)
                return;     // empty array, nothing to do

            Feeder feeder = new WrapDataRowCollectionAsFeeder(table.Rows, state);
            WriteToServer(feeder);
        }

        // Summary:
        //     Fetch the data from the specified IDataReader object and stores it into the target table
        //
        // Exceptions:
        //   System.ArgumentException:
        //     If the name of the target table hasn't been specified
        //
        //   System.IndexOutOfRangeException:
        //     If the mapping specifies a column ordinal that is outside of the allowed range
        //
        //   NuoDb.Data.Client.NuoDbSqlException:
        //     If the server returns an error
        public void WriteToServer(IDataReader reader)
        {
            Feeder feeder = new WrapDataReaderAsFeeder(reader);
            WriteToServer(feeder);
        }

        private string AppendError(string currentMessage, string error, int index)
        {
            var builder = new StringBuilder(currentMessage);
            if (builder.Length > 0)
                builder.AppendLine();
            builder.AppendFormat("{0} (item #{1})", error, index);
            return builder.ToString();
        }
    }
}
