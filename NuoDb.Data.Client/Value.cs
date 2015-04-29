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
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace NuoDb.Data.Client
{

    //
    //
    // Value
    //
    //
    abstract class Value
    {
        /*		private class ValueRef : java.sql.Ref
                {
                    private readonly Value outerInstance;

                    private readonly string columnTypeName; // the type name from the metadata

                    public ValueRef(Value outerInstance, string columnTypeName)
                    {
                            this.outerInstance = outerInstance;
                            this.columnTypeName = columnTypeName;
                    }

                    public override string BaseTypeName
                    {
                        get
                        {
                                return columnTypeName;
                        }
                    }

                    public override object getObject(IDictionary<string, Type> map)
                    {
                            RemConnection.throwNotYetImplemented();

                            return null;
                    }

                    public override object Object
                    {
                        get
                        {
                            return outerInstance.Object;
                        }
                        set
                        {
                            RemConnection.throwNotYetImplemented();
                        }
                    }

                }
        */
        public const int Null = 0;
        public const int typeString = 1; // generic = null terminated
        public const int typeChar = 2; // fixed length string = also null terminated
        public const int Varchar = 3; // variable length = counted string
        public const int typeShort = 4;
        public const int typeInt = 5;
        public const int typeLong = 6;
        public const int typeFloat = 7;
        public const int typeDouble = 8;
        public const int typeDate = 9;
        public const int typeTimestamp = 10;
        public const int typeTimeType = 11;
        public const int Asciiblob = 12; // on disk blob
        public const int Binaryblob = 13; // on disk blob
        public const int BlobPtr = 14; // pointer to Blob object
        public const int typeSqlTimestamp = 15; // 64 bit version
        public const int ClobPtr = 16;
        public const int typeBiginteger = 17;
        public const int typeNumber = 18; //arbitrary precision, optionally scaled number
        public const int typeBytes = 19; //arbitrary length binary octets
        public const int typeBinChar = 20; //fixed length binary octets
        public const int typeVarBinChar = 21; //variable length binary octets
        public const int typeBoolean = 22;

        //private ValueRef @ref = null;

        protected static readonly string dateFormat = "yyyy-MM-dd";
        protected static readonly string timeFormat = "HH:mm:ss.FFFFFFF";
        protected static readonly string timestampFormat = "yyyy-MM-dd HH:mm:ss.FFFFFFF";

        protected static readonly DateTime nullDate = new DateTime(0);

        public virtual int Type
        {
            get
            {
                return Null;
            }
        }

        internal abstract void encodeValue(EncodedDataStream dataStream);

        public abstract string String { get; }

        public virtual int Scale
        {
            get
            {
                return 0;
            }
        }

        public virtual byte Byte
        {
            get
            {
                throwConversionNotImplemented("byte");
                return 0;
            }
        }

        public virtual short Short
        {
            get
            {
                throwConversionNotImplemented("short");
                return 0;
            }
        }

        public virtual int Int
        {
            get
            {
                throwConversionNotImplemented("int");
                return 0;
            }
        }

        public virtual long Long
        {
            get
            {
                throwConversionNotImplemented("long");
                return 0;
            }
        }

        public virtual decimal BigDecimal
        {
            get
            {
                throwConversionNotImplemented("bigdecimal");
                return Decimal.Zero;
            }
        }

        public virtual byte[] Bytes
        {
            get
            {
                string @string = String;

                if (@string == null)
                {
                    return null;
                }

                char[] bytes = @string.ToCharArray();
                byte[] value = new byte[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                    value[i] = (byte)bytes[i];
                return value;
            }
        }

        public virtual bool Boolean
        {
            get
            {
                return Long != 0;
            }
        }

        public virtual float Float
        {
            get
            {
                return (float)Double;
            }
        }

        public virtual double Double
        {
            get
            {
                int scale = Scale;
                double d = (double)Long;

                if (scale > 0)
                {
                    for (; scale > 0; --scale)
                    {
                        d /= 10;
                    }
                }
                else if (scale < 0)
                {
                    for (; scale < 0; ++scale)
                    {
                        d *= 10;
                    }
                }

                return d;
            }
        }

        public virtual DateTime Date
        {
            get
            {
                throwConversionNotImplemented("date");
                return new DateTime();
            }
        }

        public virtual TimeSpan TimeSpan
        {
            get
            {
                throwConversionNotImplemented("TimeSpan");
                return new TimeSpan();
            }
        }

        public virtual object Object
        {
            get
            {
                throwConversionNotImplemented("object");
                return null;
            }
        }

        internal virtual void throwConversionNotImplemented(string type)
        {
            throw new NuoDbSqlException("conversion to " + type + " is not implemented");
        }

        public static string getString(long value, int scale)
        {
            if (scale == 0 || value == 0)
            {
                return Convert.ToString(value);
            }

            char[] chars = new char[23];
            int digits = 0;

            for (long n = (value >= 0) ? value : -value; n > 0 || digits <= scale; n /= 10)
            {
                if (digits == scale)
                {
                    chars[digits++] = '.';
                    if (n == 0)
                    {
                        break;
                    }
                }

                chars[digits++] = (char)('0' + n % 10);
            }

            if (value < 0)
            {
                chars[digits++] = '-';
            }

            for (int i = 0, j = digits - 1; i < j; ++i, --j)
            {
                char c = chars[i];
                chars[i] = chars[j];
                chars[j] = c;
            }

            return new string(chars, 0, digits);
        }

        /*		public virtual Stream InputStream
                {
                    get
                    {
                        sbyte[] result = AsBytes;
                        return result == null ? null : new ByteArrayInputStream(result);
                    }
                }

                public abstract byte[] AsBytes {get;}

                public virtual Stream AsciiStream
                {
                    get
                    {
                        string result = String;
                        return result == null ? null : new ByteArrayInputStream(result.Bytes);
                    }
                }

                public virtual Reader CharacterStream
                {
                    get
                    {
                        string result = String;
                        return result == null ? null : new StringReader(result);
                    }
                }

                public virtual Ref getRef(string columnTypeName)
                {
                    if (@ref == null)
                    {
                        @ref = new ValueRef(this, columnTypeName);
                    }

                    return @ref;
                }
        */
        /// <summary>
        /// change the scale of the given long value </summary>
        /// <param name="number"> the number to rescale </param>
        /// <param name="fromScale"> the current scale </param>
        /// <param name="toScale"> desired scale </param>
        /// <returns> a scaled number </returns>
        public static long reScale(long number, int fromScale, int toScale)
        {
            int delta = toScale - fromScale;

            if (delta > 0)
            {
                for (int n = 0; n < delta; ++n)
                {
                    number *= 10;
                }
            }
            else if (delta < 0)
            {
                for (int n = 0; n > delta; --n)
                {
                    number /= 10;
                }
            }

            return number;
        }

        public static bool IsNumeric(Type type)
        {
            if (type == null)
                return false;
            HashSet<string> numericTypes = new HashSet<string>(new string[] {
                "System.Byte", "System.Int16", "System.Int32", "System.Int64", "System.SByte", 
                "System.UInt16", "System.UInt32", "System.UInt64", "System.Decimal",
                "System.Double", "System.Single" });
            return numericTypes.Contains(type.FullName);
        }

        public static bool IsNumeric(object value)
        {
            return (value is ValueType && IsNumeric(value.GetType()));
        }
    }


}