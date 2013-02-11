namespace System.Data.NuoDB
{

	//
	//
	// ValueBlob
	//
	//
	public class ValueBlob : Value
	{
		internal Blob blob;

		public ValueBlob()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ValueBlob(java.sql.Blob val) throws java.sql.SQLException
		public ValueBlob(java.sql.Blob val)
		{
			blob = new Blob(val);
		}

		internal override int Type
		{
			get
			{
				return BlobPtr;
			}
		}

		internal override object Object
		{
			get
			{
				return blob;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Clob getClob() throws java.sql.SQLException
		internal override Clob Clob
		{
			get
			{
				return new Clob(new string(blob.Bytes));
			}
		}

		internal override Blob Blob
		{
			get
			{
				return blob;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void encodeValue(EncodedDataStream dataStream) throws java.sql.SQLException
		internal override void encodeValue(EncodedDataStream dataStream)
		{
			dataStream.encodeBytes(blob.Bytes);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: String getString() throws java.sql.SQLException
		internal override string String
		{
			get
			{
				return blob.ToString();
			}
		}

		public override sbyte[] AsBytes
		{
			get
			{
				return blob.Bytes;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte getByte() throws java.sql.SQLException
		internal override sbyte Byte
		{
			get
			{
				throwConversionNotImplemented("byte");
				return 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: short getShort() throws java.sql.SQLException
		internal override short Short
		{
			get
			{
				throwConversionNotImplemented("short");
				return 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int getInt() throws java.sql.SQLException
		internal override int Int
		{
			get
			{
				throwConversionNotImplemented("int");
				return 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: long getLong() throws java.sql.SQLException
		internal override long Long
		{
			get
			{
				throwConversionNotImplemented("long");
				return 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: double getDouble() throws java.sql.SQLException
		internal override double Double
		{
			get
			{
				throwConversionNotImplemented("double");
				return 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: float getFloat() throws java.sql.SQLException
		internal override float Float
		{
			get
			{
				throwConversionNotImplemented("float");
				return 0;
			}
		}

	}


}