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
	// ValueNumber
	//
	//
	public class ValueNumber : Value
	{
		private readonly decimal value;

		public ValueNumber(decimal x)
		{
			value = x;
		}

		public override int Type
		{
			get
			{
				return typeNumber;
			}
		}

        public override int Scale
		{
			get
			{
                return 0; // value.scale();
			}
		}

		internal override void encodeValue(EncodedDataStream dataStream)
		{
            decimal d = value;
            int scale = 0;
            while ((d % 1) != 0)
            {
                scale++;
                d *= 10;
            }
            dataStream.encodeLong((long)d, scale);
		}

        public override string String
		{
			get
			{
				return value.ToString();
			}
		}

        public override long Long
		{
			get
			{
				return (long)value;
			}
		}

        public override decimal BigDecimal
		{
			get
			{
				return value;
			}
		}

        public override object Object
		{
			get
			{
				return value;
			}
		}

/*		public override sbyte[] AsBytes
		{
			get
			{
				RemConnection.throwRuntimeNotYetImplemented();
				return null;
			}
		}
 */
	}


}