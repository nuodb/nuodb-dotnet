namespace System.Data.NuoDB
{

	//
	//
	// ValueClob
	//
	//
	public class ValueClob : Value
	{
		internal Clob clob;

		public ValueClob()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ValueClob(java.sql.Clob val) throws java.sql.SQLException
		public ValueClob(java.sql.Clob val)
		{
			clob = new Clob(val);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ValueClob(Object val) throws java.sql.SQLException
		public ValueClob(object val)
		{
			clob = (Clob)(val);
		}

		internal override int Type
		{
			get
			{
				return ClobPtr;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: String getString() throws java.sql.SQLException
		internal override string String
		{
			get
			{
				return clob.getSubString(0, (int) clob.length());
			}
		}

		internal override object Object
		{
			get
			{
				return clob;
			}
		}

		internal override Clob Clob
		{
			get
			{
				return clob;
			}
		}

		internal override Blob Blob
		{
			get
			{
				return new Blob(clob.Bytes);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void encodeValue(EncodedDataStream dataStream) throws java.sql.SQLException
		internal override void encodeValue(EncodedDataStream dataStream)
		{
			dataStream.encodeString(clob.String);
		}

		public override sbyte[] AsBytes
		{
			get
			{
				return clob.Bytes;
			}
		}
	}


}