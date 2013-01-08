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

namespace System.Data.NuoDB
{

	//
	//
	// ValueLong
	//
	//
	public class ValueLong : Value
	{
		internal long value;
		internal short scale;

		public ValueLong()
		{
		}

		public ValueLong(long val, int scl)
		{
			value = val;
			scale = (short) scl;
		}

		public ValueLong(object val, int scl)
		{
			if (IsNumeric(val))
			{
				value = (long)val;
			}
			else if (val is bool)
			{
				value = true.Equals(val) ? 1 : 0;
			}
			else
			{
				value = Convert.ToInt64(val.ToString());
			}
			scale = (short) scl;
		}

        public override int Type
		{
			get
			{
				return typeLong; // C semantics
			}
		}

        public override int Scale
		{
			get
			{
				return scale;
			}
		}

        internal override void encodeValue(EncodedDataStream dataStream)
		{
			dataStream.encodeLong(Long);
		}

        public override string String
		{
			get
			{
				return getString(value, scale);
			}
		}

        public override bool Boolean
		{
			get
			{
				return value != 0; // we don't want to rescale
			}
		}

        public override byte Byte
		{
			get
			{
				if ((Long > sbyte.MaxValue) || (Long < sbyte.MinValue))
				{
					throw new SQLException(String.Format("Overflow for type byte: {0} ", Long));
				}
				return (byte) Long;
			}
		}

        public override short Short
		{
			get
			{
				return (short) Long;
			}
		}

        public override int Int
		{
			get
			{
				return (int) Long;
			}
		}

        public override double Double
		{
			get
			{
				return Int;
			}
		}

        public override float Float
		{
			get
			{
				return Int;
			}
		}

        public override long Long
		{
			get
			{
				return scale == 0 ? value : reScale(value, scale, 0);
			}
		}

        public override decimal BigDecimal
		{
			get
			{
                Decimal d = new Decimal(value);
                if (scale > 0)
                    for (int i = 0; i < scale; i++)
                        d = Decimal.Multiply(d, 10m);
                else if (scale < 0)
                    for (int i = 0; i < -scale; i++)
                        d = Decimal.Divide(d, 10m);
                return d;
            }
		}

        public override object Object
		{
			get
			{
				return Long;
			}
		}

/*		public override byte[] AsBytes
		{
			get
			{
				return Conversions.toBytes(Long);
			}
		}
 */
	}


}