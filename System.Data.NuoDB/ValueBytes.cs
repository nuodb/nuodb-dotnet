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