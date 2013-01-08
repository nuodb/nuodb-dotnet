namespace System.Data.NuoDB
{

	//
	//
	// ValueBytes
	//
	//
	public class ValueBytes : Value
	{
		internal byte[] value;

		public ValueBytes()
		{
		}

		public ValueBytes(byte[] val)
		{
			value = val;
		}

		public ValueBytes(object x)
		{
			value = ((byte[])x);
		}

		public override int Type
		{
			get
			{
				return BlobPtr;
			}
		}

		internal override void encodeValue(EncodedDataStream dataStream)
		{
			dataStream.encodeBytes(value);
		}

        public override object Object
		{
			get
			{
				return Bytes;
			}
		}

        public override byte[] Bytes
		{
			get
			{
				return value;
			}
		}

        public override string String
		{
			get
			{
                char[] buff = new char[value.Length];
                for (int i = 0; i < value.Length; i++)
                    buff[i] = (char)value[i];
				return new string(buff);
			}
		}

/*        public override byte[] AsBytes
		{
			get
			{
				return value;
			}
		}
*/
        public override byte Byte
		{
			get
			{
				throwConversionNotImplemented("byte");
				return 0;
			}
		}

        public override short Short
		{
			get
			{
				throwConversionNotImplemented("short");
				return 0;
			}
		}

        public override int Int
		{
			get
			{
				throwConversionNotImplemented("int");
				return 0;
			}
		}

        public override long Long
		{
			get
			{
				throwConversionNotImplemented("long");
				return 0;
			}
		}

        public override double Double
		{
			get
			{
				throwConversionNotImplemented("double");
				return 0;
			}
		}

        public override float Float
		{
			get
			{
				throwConversionNotImplemented("float");
				return 0;
			}
		}
	}


}