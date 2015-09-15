using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace NuoDb.Data.Client
{
    interface DataFeeder
    {
        bool MoveNext();
        int FieldCount { get; }
        object this[int i] { get; }
        object this[string name] { get; }
    }

    internal class WrapDataRecordAsFeeder : DataFeeder
    {
        List<IDataRecord>.Enumerator wrappedRecords;

        public WrapDataRecordAsFeeder(List<IDataRecord> rows)
        {
            wrappedRecords = rows.GetEnumerator();
        }

        public int FieldCount
        {
            get { return wrappedRecords.Current.FieldCount; }
        }
        public object this[string name]
        {
            get { return wrappedRecords.Current[name]; }
        }
        public object this[int i]
        {
            get { return wrappedRecords.Current[i]; }
        }
        public bool MoveNext()
        {
            return wrappedRecords.MoveNext();
        }
    }

    internal class WrapDataRowAsFeeder : DataFeeder
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

    internal class WrapDataRowCollectionAsFeeder : DataFeeder
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

    internal class WrapDataReaderAsFeeder : DataFeeder
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

    internal class FeederOrderer : DataFeeder
    {
        private DataFeeder wrappedFeeder;
        private NuoDbBulkLoaderColumnMappingCollection mappings;

        public FeederOrderer(DataFeeder parent, NuoDbBulkLoaderColumnMappingCollection mappings)
        {
            this.wrappedFeeder = parent;
            this.mappings = mappings;
        }

        public int FieldCount
        {
            get { return mappings.Count; }
        }

        public object this[int i]
        {
            get
            {
                if (mappings[i].SourceColumn == null)
                {
                    return wrappedFeeder[mappings[i].SourceOrdinal];
                }
                else
                {
                    return wrappedFeeder[mappings[i].SourceColumn];
                }
            }
        }

        public object this[string name]
        {
            get 
            {
                return wrappedFeeder[name]; 
            }
        }

        public bool MoveNext()
        {
            return wrappedFeeder.MoveNext();
        }

    }

}
