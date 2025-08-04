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
    // ValueString
    //
    //
    class ValueString : Value
    {
        internal string value;

        public ValueString(string val)
        {
            value = val;
        }

        public ValueString(object val)
        {
            if (val is string)
            {
                value = (string)val;
            }
            else
            {
                value = val.ToString();
            }
        }

        public override int Type
        {
            get
            {
                return typeString; // C semantics
            }
        }

        internal override void encodeValue(EncodedDataStream dataStream)
        {
            dataStream.encodeString(value);
        }

        public override string String
        {
            get
            {
                return value;
            }
        }

        public override byte Byte
        {
            get
            {
                try
                {
                    return Convert.ToByte(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException("Unable to convert string: " + value, e);
                }
            }
        }

        public override short Short
        {
            get
            {
                try
                {
                    return Convert.ToInt16(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException("Unable to convert string: " + value, e);
                }
            }
        }

        public override int Int
        {
            get
            {
                try
                {
                    return Convert.ToInt32(value);

                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException("Unable to convert string: " + value, e);
                }
            }
        }

        public override long Long
        {
            get
            {
                try
                {
                    return Convert.ToInt64(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException("Unable to convert string: " + value, e);
                }
            }
        }

        public override float Float
        {
            get
            {
                try
                {
                    return Convert.ToSingle(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException("Unable to convert string: " + value, e);
                }
            }
        }

        public override double Double
        {
            get
            {
                try
                {
                    return Convert.ToDouble(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException("Unable to convert string: " + value, e);
                }
            }
        }

        public override object Object
        {
            get
            {
                return String;
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
        public override DateOnly Date
        {
            get
            {
                try
                {
                    return DateOnly.Parse(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException(String.Format("Unable to parse \"{0}\" into a Date", value), e);
                }
            }
        }

        public override DateTime TimeStamp
        {
            get
            {
                try
                {
                    return DateTime.Parse(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException(String.Format("Unable to parse \"{0}\" into a Timestamp", value), e);
                }

            }
        }

        public override TimeOnly Time
        {
            get
            {
                try
                {
                    return TimeOnly.Parse(value);
                }
                catch (FormatException e)
                {
                    throw new NuoDbSqlException(String.Format("Unable to parse \"{0}\" into a Time", value), e);
                }
            }
        }

        public override bool Boolean
        {
            get
            {
                return value == null ? false : value.ToLower().Equals("true");
            }
        }
    }

}