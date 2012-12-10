namespace System.Data.NuoDB
{

	//
	//
	// ValueBytes
	//
	//
	public class ValueBytes : Value
	{
		internal sbyte[] value;

		public ValueBytes()
		{
		}

		public ValueBytes(sbyte[] val)
		{
			value = val;
		}

		public ValueBytes(object x)
		{
			value = ((sbyte[])x);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ValueBytes(Blob blob) throws java.sql.SQLException
		public ValueBytes(Blob blob)
		{
			value = blob.Bytes;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ValueBytes(java.sql.Blob blob) throws java.sql.SQLException
		public ValueBytes(java.sql.Blob blob)
		{
			value = blob.getBytes(0, (int) blob.length());
		}

		internal override int Type
		{
			get
			{
				return BlobPtr;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void encodeValue(EncodedDataStream dataStream) throws java.sql.SQLException
		internal override void encodeValue(EncodedDataStream dataStream)
		{
			dataStream.encodeBytes(value);
		}

		internal override object Object
		{
			get
			{
				return Bytes;
			}
		}

		internal override sbyte [] Bytes
		{
			get
			{
				return value;
			}
		}

		internal override string String
		{
			get
			{
				return new string(value);
			}
		}

		public override sbyte[] AsBytes
		{
			get
			{
				return value;
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