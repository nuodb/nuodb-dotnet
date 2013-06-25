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

namespace NuoDb.Data.Client
{

    //
    //
    // ValueDouble
    //
    //
    class ValueDouble : Value
    {
        internal double value;

        public ValueDouble()
        {
        }

        public ValueDouble(double val)
        {
            value = val;
        }

        public ValueDouble(object val)
        {
            if (IsNumeric(val))
            {
                value = (double)val;
            }
            else if (val is bool)
            {
                value = true.Equals(val) ? 1.0 : 0.0;
            }
            else
            {
                value = Convert.ToDouble(val.ToString());
            }
        }

        public override int Type
        {
            get
            {
                return typeDouble;
            }
        }

        public override string String
        {
            get
            {
                return Convert.ToString(value);
            }
        }

        public override double Double
        {
            get
            {
                return value;
            }
        }

        public override long Long
        {
            get
            {
                return (long)value;
            }
        }

        public override object Object
        {
            get
            {
                return Double;
            }
        }

        internal override void encodeValue(EncodedDataStream dataStream)
        {
            dataStream.encodeDouble(value);
        }

        public override decimal BigDecimal
        {
            get
            {
                return new decimal(value);
            }
        }

        /*		public override byte[] AsBytes
                {
                    get
                    {
                        return Conversions.toBytes(value);
                    }
                }
         */
    }


}