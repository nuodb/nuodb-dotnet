namespace System.Data.NuoDB
{
	//
	//
	// Clob
	//
	//

	public class Clob : java.sql.Clob
	{
		internal string value;

		internal Clob()
		{
		}
		internal Clob(string value)
		{
			this.value = value;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Clob(java.sql.Clob clob) throws java.sql.SQLException
		internal Clob(java.sql.Clob clob)
		{
			long length = clob.length();

			if (length >= 0)
			{
				value = clob.getSubString(1, (int) length);
			}
		}

		public virtual bool hasData()
		{
			return value != null;
		}

		public virtual sbyte [] Bytes
		{
			get
			{
				if (value == null)
				{
					return null;
				}
    
				return value.Bytes;
			}
		}

		public virtual string String
		{
			get
			{
				return value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long length() throws java.sql.SQLException
		public virtual long length()
		{
			if (value == null)
			{
				return -1;
			}

			return value.Length;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getSubString(long pos, int length) throws java.sql.SQLException
		public virtual string getSubString(long pos, int length)
		{
			if (value == null)
			{
				return null;
			}

			if (pos == 0)
			{
				throw new SQLException("The first byte in the CLOB is at position 1");
			}

			// pos being passed expects 1-based array but value is 0-based.
			pos--;

			if (pos == 0 && length == value.Length)
			{
				return value;
			}

			checkLongValue(pos, "position");

			return value.Substring((int) pos, (int)pos + length - ((int) pos));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.Reader getCharacterStream() throws java.sql.SQLException
		public virtual Reader CharacterStream
		{
			get
			{
				return new StringReader(value == null ? "" : value);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.InputStream getAsciiStream() throws java.sql.SQLException
		public virtual java.io.InputStream AsciiStream
		{
			get
			{
				return new ByteArrayInputStream(value == null ? new sbyte[0] : value.Bytes);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long position(String searchstr, long start) throws java.sql.SQLException
		public virtual long position(string searchstr, long start)
		{
			if (start == 0)
			{
				throw new SQLException("The first byte in the CLOB is at position 1");
			}

			// start being passed expects 1-based array but value is 0-based.
			start--;

			if (value == null)
			{
				return -1;
			}
			checkLongValue(start, "start");

			int pos = value.IndexOf(searchstr, (int)start);
			return pos >= 0 ? pos + 1 : pos;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long position(java.sql.Clob searchstr, long start) throws java.sql.SQLException
		public virtual long position(java.sql.Clob searchstr, long start)
		{
			return position(searchstr.getSubString(1, (int) searchstr.length()), start--);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void free() throws java.sql.SQLException
		public override void free()
		{
				value = null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.Reader getCharacterStream(long pos, long length) throws java.sql.SQLException
		public override Reader getCharacterStream(long pos, long length)
		{
			RemConnection.throwNotYetImplemented();
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.OutputStream setAsciiStream(long arg0) throws java.sql.SQLException
		public override OutputStream setAsciiStream(long arg0)
		{
			RemConnection.throwNotYetImplemented();
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.io.Writer setCharacterStream(long arg0) throws java.sql.SQLException
		public override Writer setCharacterStream(long arg0)
		{
				RemConnection.throwNotYetImplemented();
				return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int setString(long pos, String str) throws java.sql.SQLException
		public override int setString(long pos, string str)
		{
			// pos being passed expects 1-based array but value is 0-based.
			pos--;

			checkLongValue(pos, "position");
			if (value == null)
			{
				if (pos == 0)
				{
					value = str;
				}
				else
				{
					throw new SQLException(String.Format("Position exceeds the current length of this CLOB {0}", 0));
				}
			}
			else
			{
				value = StringUtils.overwrite(value, (int) pos, str);
			}

			return str.Length;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int setString(long pos, String str, int offset, int len) throws java.sql.SQLException
		public override int setString(long pos, string str, int offset, int len)
		{
			// pos being passed expects 1-based array but value is 0-based.
			pos--;

			checkLongValue(pos, "position");
			if (value == null)
			{
				if (pos == 0)
				{
					value = str.Substring(offset, len);
				}
				else
				{
					throw new SQLException(String.Format("Position exceeds the current length of this CLOB {0}", 0));
				}
			}
			else
			{
				value = StringUtils.overwrite(value, (int) pos, str, offset, len);
			}

			return len;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void truncate(long length) throws java.sql.SQLException
		public override void truncate(long length)
		{
			if (length > StringUtils.size(value))
			{
				throw new SQLException(String.Format("Unable to truncate length {0} exceeds length of the CLOB {1}", length, StringUtils.size(value)));
			}
			checkLongValue(length, "length");

			try
			{
				value = value.Substring(0, (int)length);
			}
			catch (System.IndexOutOfRangeException e)
			{
				throw new SQLException("clob index out of bounds", e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void checkLongValue(long value, String argName) throws java.sql.SQLException
		internal static void checkLongValue(long value, string argName)
		{
			if (value > int.MaxValue)
			{
				throw new SQLException(String.Format("Position {0} is greater than max supported {1} of {2}", value, argName, int.MaxValue));
			}
		}

		public override bool Equals(object obj)
		{

			if (!(obj is Clob))
			{
				return false;
			}

			Clob other = (Clob) obj;

			return StringUtils.Equals(value, other.value);
		}

		public override int GetHashCode()
		{
			return value == null ? 0 : value.GetHashCode();
		}

	}


}