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
using NuoDb.Data.Client.Net;
using NuoDb.Data.Client.Util;
using NuoDb.Data.Client.Security;
using System.Data;

namespace NuoDb.Data.Client
{
    class EncodedDataStream : DataStream
    {
        public int type;
        public string @string;
        public long integer64;
        public int integer32;
        public int blobId;
        public int scale;
        public double dbl;
        public Guid uuid;
        public byte[] bytes;
        public bool @bool;
        public decimal bigDecimal;

        internal int currentMessageType;

        protected internal byte[] buffer; // used for decode()
        protected internal int offset; // used for decode()
        protected internal int priorCode; // for debugging
        private int protocolVersion;

        internal const int defaultSegmentSize = 512;
        internal const int lengthLength = 4;

        public const int edsTypeNull = 0;
        public const int edsTypeUnknown = 1;
        public const int edsTypeInt32 = 2;
        public const int edsTypeInt64 = 3;
        public const int edsTypeScaled = 4;
        public const int edsTypeUtf8 = 5;
        public const int edsTypeOpaque = 6;
        public const int edsTypeDouble = 7;
        public const int edsTypeBlob = 8;
        public const int edsTypeClob = 9;
        public const int edsTypeTime = 10;
        public const int edsTypeMilliseconds = 11;
        public const int edsTypeNanoseconds = 12;
        public const int edsTypeBigInt = 13;
        public const int edsTypeUUID = 14;
        public const int edsTypeBoolean = 15;
        public const int edsTypeScaledTime = 16;
        public const int edsTypeScaledDate = 17;
        public const int edsTypeScaledTimestamp = 18;



        internal const int edsNull = 1;
        internal const int edsTrue = 2;
        internal const int edsFalse = 3;
        internal const int edsIntMinus10 = 10;
        internal const int edsIntMinus9 = 11;
        internal const int edsIntMinus8 = 12;
        internal const int edsIntMinus7 = 13;
        internal const int edsIntMinus6 = 14;
        internal const int edsIntMinus5 = 15;
        internal const int edsIntMinus4 = 16;
        internal const int edsIntMinus3 = 17;
        internal const int edsIntMinus2 = 18;
        internal const int edsIntMinus1 = 19;
        internal const int edsInt0 = 20;
        internal const int edsInt1 = 21;
        internal const int edsInt2 = 22;
        internal const int edsInt3 = 23;
        internal const int edsInt4 = 24;
        internal const int edsInt5 = 25;
        internal const int edsInt6 = 26;
        internal const int edsInt7 = 27;
        internal const int edsInt8 = 28;
        internal const int edsInt9 = 29;
        internal const int edsInt10 = 30;
        internal const int edsInt11 = 31;
        internal const int edsInt12 = 32;
        internal const int edsInt13 = 33;
        internal const int edsInt14 = 34;
        internal const int edsInt15 = 35;
        internal const int edsInt16 = 36;
        internal const int edsInt17 = 37;
        internal const int edsInt18 = 38;
        internal const int edsInt19 = 39;
        internal const int edsInt20 = 40;
        internal const int edsInt21 = 41;
        internal const int edsInt22 = 42;
        internal const int edsInt23 = 43;
        internal const int edsInt24 = 44;
        internal const int edsInt25 = 45;
        internal const int edsInt26 = 46;
        internal const int edsInt27 = 47;
        internal const int edsInt28 = 48;
        internal const int edsInt29 = 49;
        internal const int edsInt30 = 50;
        internal const int edsInt31 = 51;
        internal const int edsIntLen1 = 52;
        internal const int edsIntLen2 = 53;
        internal const int edsIntLen3 = 54;
        internal const int edsIntLen4 = 55;
        internal const int edsIntLen5 = 56;
        internal const int edsIntLen6 = 57;
        internal const int edsIntLen7 = 58;
        internal const int edsIntLen8 = 59;
        internal const int edsScaledLen0 = 60;
        internal const int edsScaledLen1 = 61;
        internal const int edsScaledLen2 = 62;
        internal const int edsScaledLen3 = 63;
        internal const int edsScaledLen4 = 64;
        internal const int edsScaledLen5 = 65;
        internal const int edsScaledLen6 = 66;
        internal const int edsScaledLen7 = 67;
        internal const int edsScaledLen8 = 68;
        internal const int edsUtf8Count1 = 69;
        internal const int edsUtf8Count2 = 70;
        internal const int edsUtf8Count3 = 71;
        internal const int edsUtf8Count4 = 72;
        internal const int edsOpaqueCount1 = 73;
        internal const int edsOpaqueCount2 = 74;
        internal const int edsOpaqueCount3 = 75;
        internal const int edsOpaqueCount4 = 76;
        internal const int edsDoubleLen0 = 77;
        internal const int edsDoubleLen1 = 78;
        internal const int edsDoubleLen2 = 79;
        internal const int edsDoubleLen3 = 80;
        internal const int edsDoubleLen4 = 81;
        internal const int edsDoubleLen5 = 82;
        internal const int edsDoubleLen6 = 83;
        internal const int edsDoubleLen7 = 84;
        internal const int edsDoubleLen8 = 85;
        internal const int edsMilliSecLen0 = 86;
        internal const int edsMilliSecLen1 = 87;
        internal const int edsMilliSecLen2 = 88;
        internal const int edsMilliSecLen3 = 89;
        internal const int edsMilliSecLen4 = 90;
        internal const int edsMilliSecLen5 = 91;
        internal const int edsMilliSecLen6 = 92;
        internal const int edsMilliSecLen7 = 93;
        internal const int edsMilliSecLen8 = 94;
        internal const int edsNanoSecLen0 = 95;
        internal const int edsNanoSecLen1 = 96;
        internal const int edsNanoSecLen2 = 97;
        internal const int edsNanoSecLen3 = 98;
        internal const int edsNanoSecLen4 = 99;
        internal const int edsNanoSecLen5 = 100;
        internal const int edsNanoSecLen6 = 101;
        internal const int edsNanoSecLen7 = 102;
        internal const int edsNanoSecLen8 = 103;
        internal const int edsTimeLen0 = 104;
        internal const int edsTimeLen1 = 105;
        internal const int edsTimeLen2 = 106;
        internal const int edsTimeLen3 = 107;
        internal const int edsTimeLen4 = 108;
        internal const int edsUtf8Len0 = 109;
        internal const int edsUtf8Len1 = 110;
        internal const int edsUtf8Len2 = 111;
        internal const int edsUtf8Len3 = 112;
        internal const int edsUtf8Len4 = 113;
        internal const int edsUtf8Len5 = 114;
        internal const int edsUtf8Len6 = 115;
        internal const int edsUtf8Len7 = 116;
        internal const int edsUtf8Len8 = 117;
        internal const int edsUtf8Len9 = 118;
        internal const int edsUtf8Len10 = 119;
        internal const int edsUtf8Len11 = 120;
        internal const int edsUtf8Len12 = 121;
        internal const int edsUtf8Len13 = 122;
        internal const int edsUtf8Len14 = 123;
        internal const int edsUtf8Len15 = 124;
        internal const int edsUtf8Len16 = 125;
        internal const int edsUtf8Len17 = 126;
        internal const int edsUtf8Len18 = 127;
        internal const int edsUtf8Len19 = 128;
        internal const int edsUtf8Len20 = 129;
        internal const int edsUtf8Len21 = 130;
        internal const int edsUtf8Len22 = 131;
        internal const int edsUtf8Len23 = 132;
        internal const int edsUtf8Len24 = 133;
        internal const int edsUtf8Len25 = 134;
        internal const int edsUtf8Len26 = 135;
        internal const int edsUtf8Len27 = 136;
        internal const int edsUtf8Len28 = 137;
        internal const int edsUtf8Len29 = 138;
        internal const int edsUtf8Len30 = 139;
        internal const int edsUtf8Len31 = 140;
        internal const int edsUtf8Len32 = 141;
        internal const int edsUtf8Len33 = 142;
        internal const int edsUtf8Len34 = 143;
        internal const int edsUtf8Len35 = 144;
        internal const int edsUtf8Len36 = 145;
        internal const int edsUtf8Len37 = 146;
        internal const int edsUtf8Len38 = 147;
        internal const int edsUtf8Len39 = 148;
        internal const int edsUft8LenMax = 148;
        internal const int edsOpaqueLen0 = 149;
        internal const int edsOpaqueLen1 = 150;
        internal const int edsOpaqueLen2 = 151;
        internal const int edsOpaqueLen3 = 152;
        internal const int edsOpaqueLen4 = 153;
        internal const int edsOpaqueLen5 = 154;
        internal const int edsOpaqueLen6 = 155;
        internal const int edsOpaqueLen7 = 156;
        internal const int edsOpaqueLen8 = 157;
        internal const int edsOpaqueLen9 = 158;
        internal const int edsOpaqueLen10 = 159;
        internal const int edsOpaqueLen11 = 160;
        internal const int edsOpaqueLen12 = 161;
        internal const int edsOpaqueLen13 = 162;
        internal const int edsOpaqueLen14 = 163;
        internal const int edsOpaqueLen15 = 164;
        internal const int edsOpaqueLen16 = 165;
        internal const int edsOpaqueLen17 = 166;
        internal const int edsOpaqueLen18 = 167;
        internal const int edsOpaqueLen19 = 168;
        internal const int edsOpaqueLen20 = 169;
        internal const int edsOpaqueLen21 = 170;
        internal const int edsOpaqueLen22 = 171;
        internal const int edsOpaqueLen23 = 172;
        internal const int edsOpaqueLen24 = 173;
        internal const int edsOpaqueLen25 = 174;
        internal const int edsOpaqueLen26 = 175;
        internal const int edsOpaqueLen27 = 176;
        internal const int edsOpaqueLen28 = 177;
        internal const int edsOpaqueLen29 = 178;
        internal const int edsOpaqueLen30 = 179;
        internal const int edsOpaqueLen31 = 180;
        internal const int edsOpaqueLen32 = 181;
        internal const int edsOpaqueLen33 = 182;
        internal const int edsOpaqueLen34 = 183;
        internal const int edsOpaqueLen35 = 184;
        internal const int edsOpaqueLen36 = 185;
        internal const int edsOpaqueLen37 = 186;
        internal const int edsOpaqueLen38 = 187;
        internal const int edsOpaqueLen39 = 188;
        internal const int edsOpaqueLenMax = 188;
        internal const int edsBlobLen0 = 189;
        internal const int edsBlobLen1 = 190;
        internal const int edsBlobLen2 = 191;
        internal const int edsBlobLen3 = 192;
        internal const int edsBlobLen4 = 193;
        internal const int edsClobLen0 = 194;
        internal const int edsClobLen1 = 195;
        internal const int edsClobLen2 = 196;
        internal const int edsClobLen3 = 197;
        internal const int edsClobLen4 = 198;
        internal const int edsScaledCount1 = 199;
        internal const int edsUUID = 200;

        //
        // 09/2012 Added support for new scaled date/times encoding. These replace the old encoding
        // edsTimeLen, edsMilliSecLen and edsNanoSecLen.
        //

        internal const int edsScaledDateLen1 = 201;
        internal const int edsScaledDateLen2 = 202;
        internal const int edsScaledDateLen3 = 203;
        internal const int edsScaledDateLen4 = 204;
        internal const int edsScaledDateLen5 = 205;
        internal const int edsScaledDateLen6 = 206;
        internal const int edsScaledDateLen7 = 207;
        internal const int edsScaledDateLen8 = 208;

        internal const int edsScaledTimeLen1 = 209;
        internal const int edsScaledTimeLen2 = 210;
        internal const int edsScaledTimeLen3 = 211;
        internal const int edsScaledTimeLen4 = 212;
        internal const int edsScaledTimeLen5 = 213;
        internal const int edsScaledTimeLen6 = 214;
        internal const int edsScaledTimeLen7 = 215;
        internal const int edsScaledTimeLen8 = 216;

        internal const int edsScaledTimestampLen1 = 217;
        internal const int edsScaledTimestampLen2 = 218;
        internal const int edsScaledTimestampLen3 = 219;
        internal const int edsScaledTimestampLen4 = 220;
        internal const int edsScaledTimestampLen5 = 221;
        internal const int edsScaledTimestampLen6 = 222;
        internal const int edsScaledTimestampLen7 = 223;
        internal const int edsScaledTimestampLen8 = 224;

        internal const int edsScaledCount2 = 225;

        internal const int edsLobStream0 = 226;
        internal const int edsLobStream1 = 227;
        internal const int edsLobStream2 = 228;
        internal const int edsLobStream3 = 229;
        internal const int edsLobStream4 = 230;

        internal const int edsArrayLen1 = 231;
        internal const int edsArrayLen2 = 232;
        internal const int edsArrayLen3 = 233;
        internal const int edsArrayLen4 = 234;
        internal const int edsArrayLen5 = 235;
        internal const int edsArrayLen6 = 236;
        internal const int edsArrayLen7 = 237;
        internal const int edsArrayLen8 = 238;

        internal const int edsMax = 239;

        internal const int edsIntMin = -10;
        internal const int edsIntMax = 31;

        internal const long SECONDS_PER_MINUTE = (60);
        internal static readonly long MILLISECONDS_PER_MINUTE = (SECONDS_PER_MINUTE * 1000);
        internal static readonly long MILLISECONDS_PER_HOUR = (MILLISECONDS_PER_MINUTE * 60);
        internal static readonly long SECONDS_PER_HOUR = (SECONDS_PER_MINUTE * 60);
        internal static readonly long SECONDS_PER_DAY = (SECONDS_PER_HOUR * 24);
        internal static readonly long MILLISECONDS_PER_DAY = (SECONDS_PER_DAY * 1000);
        internal const int SECONDS_SCALE = 0;
        internal const int MILLISECONDS_SCALE = 3;
        internal const int NANOSECONDS_SCALE = 9;

        internal static readonly long NANOSECONDS_PER_TICK = (1000000 / TimeSpan.TicksPerMillisecond);
        internal static readonly DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0);

        public EncodedDataStream()
        {
            //firstSegment = currentSegment = new Segment(0);
            protocolVersion = Protocol.PROTOCOL_VERSION;
        }
        public EncodedDataStream(int protocolVersion)
        {
            this.protocolVersion = protocolVersion;
        }

        public virtual void startMessage(int messageType)
        {
            currentMessageType = messageType;
            reset();
            encodeInt(messageType);
        }

        public virtual void write(byte[] b)
        {
            write(b, 0, b.Length);
        }

        private Decimal ScaleDecimal(Decimal bd, int scale)
        {
            Decimal result = bd;
            if (scale > 0)
            {
                for (int i = 0; i < scale; i++)
                    result /= 10.0M;
            }
            else if (scale < 0)
            {
                for (int i = 0; i > scale; i--)
                    result *= 10.0M;
            }
            return result;
        }

        private static void ConvertToScaledDecimal(Decimal bd, out int scale, out Decimal temp)
        {
            scale = 0;
            temp = bd;
            while ((temp % 1) != 0)
            {
                scale++;
                temp *= 10;
            }
            // scale must be a positive number only
        }

        internal virtual int byteCount(int n)
        {
            if (n >= 0)
            {
                if (n < (1 << 7))
                {
                    return 1;
                }

                if (n < (1 << 15))
                {
                    return 2;
                }

                if (n < (1 << 23))
                {
                    return 3;
                }

                return 4;
            }

            if (n >= -(1 << 7))
            {
                return 1;
            }

            if (n >= -(1 << 15))
            {
                return 2;
            }

            if (n >= -(1 << 23))
            {
                return 3;
            }

            return 4;
        }

        internal virtual int byteCount(long n)
        {
            if (n > 0)
            {
                if (n == 0)
                {
                    return 0;
                }

                if (n < (1L << 7))
                {
                    return 1;
                }

                if (n < (1L << 15))
                {
                    return 2;
                }

                if (n < (1L << 23))
                {
                    return 3;
                }

                if (n < (1L << 31))
                {
                    return 4;
                }

                if (n < (1L << 39))
                {
                    return 5;
                }

                if (n < (1L << 47))
                {
                    return 6;
                }

                if (n < (1L << 55))
                {
                    return 7;
                }

                return 8;
            }

            if (n >= -(1L << 7))
            {
                return 1;
            }

            if (n >= -(1L << 15))
            {
                return 2;
            }

            if (n >= -(1L << 23))
            {
                return 3;
            }

            if (n >= -(1L << 31))
            {
                return 4;
            }

            if (n >= -(1L << 39))
            {
                return 5;
            }

            if (n >= -(1L << 47))
            {
                return 6;
            }

            if (n >= -(1L << 55))
            {
                return 7;
            }

            return 8;
        }

        public virtual void encodeDotNetObject(object value)
        {
            if (value == null || System.DBNull.Value.Equals(value))
            {
                encodeNull();
            }
            else if (value is string)
            {
                encodeString((string)value);
            }
            else if (value is char)
            {
                encodeString(new string((char)value, 1));
            }
            else if (value is int)
            {
                encodeInt((int)value);
            }
            else if (value is long)
            {
                encodeLong((long)value);
            }
            else if (value is decimal)
            {
                encodeBigDecimal((decimal)value);
            }
            else if (value is bool)
            {
                encodeBoolean((bool)value);
            }
            else if (value is byte)
            {
                encodeInt((byte)value);
            }
            else if (value is short)
            {
                encodeInt((short)value);
            }
            else if (value is float)
            {
                encodeDouble((float)value);
            }
            else if (value is double)
            {
                encodeDouble((double)value);
            }
            else if (value is byte[])
            {
                encodeBytes((byte[])value);
            }
            else if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    encodeDate((DateTime)value);
                else
                    encodeTimestamp((DateTime)value);
            }
            else if (value is TimeSpan)
            {
                encodeTime((TimeSpan)value);
            }
            else if (value is Guid)
            {
                encodeGuid((Guid)value);
            }
            else if (value is IDataRecord[])
            {
                encodeArray((IDataRecord[])value);
            }
            else if (value is DataTable)
            {
                encodeArray((DataTable)value);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Unsupported type of parameter: {0}, sending as a plain string", value.GetType().Name));
                encodeString(value.ToString());
            }
        }

        public void encodeArray(IDataRecord[] rows)
        {
            int count = byteCount(rows.Length);

            write(edsArrayLen1 + count - 1);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write(rows.Length >> shift);
            }
            foreach (IDataRecord record in rows)
            {
                int numFields = record.FieldCount;

                write(edsArrayLen1 + numFields - 1);

                for (int shift = (numFields - 1) * 8; shift >= 0; shift -= 8)
                {
                    write(numFields >> shift);
                }
                for (int column = 0; column < numFields; column++)
                {
                    encodeDotNetObject(record[column]);
                }
            }
        }

        public void encodeArray(DataTable rows)
        {
            int count = byteCount(rows.Rows.Count);

            write(edsArrayLen1 + count - 1);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write(rows.Rows.Count >> shift);
            }
            foreach (DataRow record in rows.Rows)
            {
                int numFields = record.ItemArray.Length;

                write(edsArrayLen1 + numFields - 1);

                for (int shift = (numFields - 1) * 8; shift >= 0; shift -= 8)
                {
                    write(numFields >> shift);
                }
                for (int column = 0; column < numFields; column++)
                {
                    encodeDotNetObject(record[column]);
                }
            }
        }

        public virtual void encodeInt(int value)
        {
            int count = byteCount(value);

            if (value >= edsIntMin && value <= edsIntMax)
            {
                write(edsInt0 + value);

                return;
            }

            write(edsIntLen1 + count - 1);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write(value >> shift);
            }
        }

        public virtual void encodeBoolean(bool value)
        {
            write((value) ? edsTrue : edsFalse);
        }

        public virtual void encodeLong(long value)
        {
            int count = byteCount(value);

            if (value >= edsIntMin && value <= edsIntMax)
            {
                write(edsInt0 + (int)value);

                return;
            }

            write(edsIntLen1 + count - 1);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(value >> shift));
            }
        }

        public virtual void encodeLong(long value, int scale)
        {
            if (scale == 0)
            {
                encodeLong(value);

                return;
            }

            int count = byteCount(value);
            write(edsScaledLen0 + count);
            write(scale);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(value >> shift));
            }
        }

        internal virtual void encodeInt(int count, int value)
        {
            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write(value >> shift);
            }
        }

        public virtual void encodeString(string @string)
        {
            if (@string == null)
            {
                write(edsNull);

                return;
            }

            int length = captureString(@string);

            if (length <= edsUtf8Len39 - edsUtf8Len0)
            {
                write(edsUtf8Len0 + length);
            }
            else
            {
                int count = byteCount(length);
                write(edsUtf8Count1 + count - 1);
                encodeInt(count, length);
            }

            writeCapturedString();
        }

        public virtual void encodeBytes(byte[] value)
        {
            if (value == null)
            {
                write(edsNull);

                return;
            }

            int length = value.Length;

            if (length <= edsOpaqueLenMax - edsOpaqueLen0)
            {
                write(edsOpaqueLen0 + length);
            }
            else
            {
                int count = byteCount(length);
                write(edsOpaqueCount1 + count - 1);
                encodeInt(count, length);
            }

            write(value);
        }

        public virtual void encodeTimestamp(DateTime val)
        {
            if (val == null)
            {
                write(edsNull);

                return;
            }

            TimeSpan delta = val.ToUniversalTime() - baseDate;
            long value = delta.Ticks * NANOSECONDS_PER_TICK; // convert to nanoseconds
            int count = byteCount(value);

            write(edsNanoSecLen1 + count - 1);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(value >> shift));
            }
        }

        public virtual void encodeScaledTimestamp(DateTime val)
        {
            if (val == null)
            {
                write(edsNull);

                return;
            }

            var dateValue = val.Kind == DateTimeKind.Local ? val.ToUniversalTime() : val;
            
            TimeSpan delta = dateValue - baseDate;
            long nanos = delta.Ticks * NANOSECONDS_PER_TICK;
            int scale = NANOSECONDS_SCALE;

            int count = byteCount(nanos);
            write(edsScaledTimestampLen1 + count - 1);
            write(scale);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(nanos >> shift));
            }
        }

        public virtual void decode()
        {
            if (offset >= totalLength)
            {
                throw new NuoDbSqlException("buffer overrun runing message decode");
            }

            offset = decode(buffer, offset);
        }

        public virtual int decode(byte[] source, int sourceOffset)
        {
            int offset = sourceOffset;
            int code = source[offset++] & 0xff;
            int l;
            int count;

            switch (code)
            {
                case edsNull:
                    type = edsTypeNull;
                    break;

                case edsTrue:
                    type = edsTypeBoolean;
                    @bool = true;
                    break;

                case edsFalse:
                    type = edsTypeBoolean;
                    @bool = false;
                    break;

                case edsIntMinus10:
                case edsIntMinus9:
                case edsIntMinus8:
                case edsIntMinus7:
                case edsIntMinus6:
                case edsIntMinus5:
                case edsIntMinus4:
                case edsIntMinus3:
                case edsIntMinus2:
                case edsIntMinus1:
                case edsInt0:
                case edsInt1:
                case edsInt2:
                case edsInt3:
                case edsInt4:
                case edsInt5:
                case edsInt6:
                case edsInt7:
                case edsInt8:
                case edsInt9:
                case edsInt10:
                case edsInt11:
                case edsInt12:
                case edsInt13:
                case edsInt14:
                case edsInt15:
                case edsInt16:
                case edsInt17:
                case edsInt18:
                case edsInt19:
                case edsInt20:
                case edsInt21:
                case edsInt22:
                case edsInt23:
                case edsInt24:
                case edsInt25:
                case edsInt26:
                case edsInt27:
                case edsInt28:
                case edsInt29:
                case edsInt30:
                case edsInt31:
                    integer32 = code - edsInt0;
                    scale = 0;
                    type = edsTypeInt32;
                    break;

                case edsIntLen1:
                    type = edsTypeInt32;
                    scale = 0;
                    integer32 = (sbyte)source[offset++];
                    break;

                case edsIntLen2:
                    type = edsTypeInt32;
                    scale = 0;
                    integer32 = ((sbyte)source[offset] << 8) | (source[offset + 1] & 0xff);
                    offset += 2;
                    break;

                case edsIntLen3:
                    type = edsTypeInt32;
                    scale = 0;
                    integer32 = ((sbyte)source[offset]) << 16 | (source[offset + 1] & 0xff) << 8 | (source[offset + 2] & 0xff);
                    offset += 3;
                    break;

                case edsIntLen4:
                    type = edsTypeInt32;
                    scale = 0;
                    integer32 = ((sbyte)source[offset]) << 24 | (source[offset + 1] & 0xff) << 16 | (source[offset + 2] & 0xff) << 8 | (source[offset + 3] & 0xff);
                    offset += 4;
                    break;

                case edsIntLen5:
                case edsIntLen6:
                case edsIntLen7:
                case edsIntLen8:
                    l = code - edsIntLen1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    scale = 0;
                    type = edsTypeInt64;
                    break;

                case edsScaledLen0:
                    scale = (sbyte)source[offset++];
                    integer64 = 0;
                    type = edsTypeScaled;
                    break;

                case edsScaledLen1:
                case edsScaledLen2:
                case edsScaledLen3:
                case edsScaledLen4:
                case edsScaledLen5:
                case edsScaledLen6:
                case edsScaledLen7:
                case edsScaledLen8:
                    scale = (sbyte)source[offset++];
                    l = code - edsScaledLen1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    type = edsTypeScaled;
                    break;

                case edsUtf8Len0:
                case edsUtf8Len1:
                case edsUtf8Len2:
                case edsUtf8Len3:
                case edsUtf8Len4:
                case edsUtf8Len5:
                case edsUtf8Len6:
                case edsUtf8Len7:
                case edsUtf8Len8:
                case edsUtf8Len9:
                case edsUtf8Len10:
                case edsUtf8Len11:
                case edsUtf8Len12:
                case edsUtf8Len13:
                case edsUtf8Len14:
                case edsUtf8Len15:
                case edsUtf8Len16:
                case edsUtf8Len17:
                case edsUtf8Len18:
                case edsUtf8Len19:
                case edsUtf8Len20:
                case edsUtf8Len21:
                case edsUtf8Len22:
                case edsUtf8Len23:
                case edsUtf8Len24:
                case edsUtf8Len25:
                case edsUtf8Len26:
                case edsUtf8Len27:
                case edsUtf8Len28:
                case edsUtf8Len29:
                case edsUtf8Len30:
                case edsUtf8Len31:
                case edsUtf8Len32:
                case edsUtf8Len33:
                case edsUtf8Len34:
                case edsUtf8Len35:
                case edsUtf8Len36:
                case edsUtf8Len37:
                case edsUtf8Len38:
                case edsUtf8Len39:
                    l = code - edsUtf8Len0;
                    @string = getString(source, offset, l);
                    offset += l;
                    type = edsTypeUtf8;
                    break;

                case edsUtf8Count1:
                case edsUtf8Count2:
                case edsUtf8Count3:
                case edsUtf8Count4:
                    count = code - edsUtf8Count1;
                    int length = source[offset++] & 0xff;

                    for (int n = 0; n < count; ++n)
                    {
                        length = length << 8 | (source[offset++] & 0xff);
                    }

                    @string = getString(source, offset, length);
                    offset += length;
                    type = edsTypeUtf8;
                    break;

                case edsOpaqueLen0:
                case edsOpaqueLen1:
                case edsOpaqueLen2:
                case edsOpaqueLen3:
                case edsOpaqueLen4:
                case edsOpaqueLen5:
                case edsOpaqueLen6:
                case edsOpaqueLen7:
                case edsOpaqueLen8:
                case edsOpaqueLen9:
                case edsOpaqueLen10:
                case edsOpaqueLen11:
                case edsOpaqueLen12:
                case edsOpaqueLen13:
                case edsOpaqueLen14:
                case edsOpaqueLen15:
                case edsOpaqueLen16:
                case edsOpaqueLen17:
                case edsOpaqueLen18:
                case edsOpaqueLen19:
                case edsOpaqueLen20:
                case edsOpaqueLen21:
                case edsOpaqueLen22:
                case edsOpaqueLen23:
                case edsOpaqueLen24:
                case edsOpaqueLen25:
                case edsOpaqueLen26:
                case edsOpaqueLen27:
                case edsOpaqueLen28:
                case edsOpaqueLen29:
                case edsOpaqueLen30:
                case edsOpaqueLen31:
                case edsOpaqueLen32:
                case edsOpaqueLen33:
                case edsOpaqueLen34:
                case edsOpaqueLen35:
                case edsOpaqueLen36:
                case edsOpaqueLen37:
                case edsOpaqueLen38:
                case edsOpaqueLen39:
                    l = code - edsOpaqueLen0;
                    bytes = new byte[l];
                    Array.Copy(source, offset, bytes, 0, l);
                    offset += l;
                    type = edsTypeOpaque;
                    break;

                case edsOpaqueCount1:
                case edsOpaqueCount2:
                case edsOpaqueCount3:
                case edsOpaqueCount4:
                    count = code - edsOpaqueCount1 + 1;
                    l = source[offset++] & 0xff;

                    for (int n = 1; n < count; ++n)
                    {
                        l = l << 8 | (source[offset++] & 0xff);
                    }

                    bytes = new byte[l];
                    Array.Copy(source, offset, bytes, 0, l);
                    offset += l;
                    type = edsTypeOpaque;
                    break;

                case edsDoubleLen0:
                case edsDoubleLen1:
                case edsDoubleLen2:
                case edsDoubleLen3:
                case edsDoubleLen4:
                case edsDoubleLen5:
                case edsDoubleLen6:
                case edsDoubleLen7:
                case edsDoubleLen8:
                    count = code - edsDoubleLen0;
                    long lvalue = 0;

                    for (int n = 0; n < count; ++n)
                    {
                        lvalue = (lvalue << 8) | (source[offset++] & 0xff);
                    }

                    lvalue <<= (8 - count) * 8;
                    dbl = BitConverter.Int64BitsToDouble(lvalue);
                    type = edsTypeDouble;
                    break;

                case edsTimeLen0:
                    integer64 = 0;
                    scale = 0;
                    type = edsTypeTime;
                    break;

                case edsTimeLen1:
                case edsTimeLen2:
                case edsTimeLen3:
                case edsTimeLen4:
                    l = code - edsTimeLen1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    scale = 0;
                    type = edsTypeTime;
                    break;

                case edsMilliSecLen0:
                    integer64 = 0;
                    scale = 0;
                    type = edsTypeMilliseconds;
                    break;

                case edsMilliSecLen1:
                case edsMilliSecLen2:
                case edsMilliSecLen3:
                case edsMilliSecLen4:
                case edsMilliSecLen5:
                case edsMilliSecLen6:
                case edsMilliSecLen7:
                case edsMilliSecLen8:
                    l = code - edsMilliSecLen1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    scale = 0;
                    type = edsTypeMilliseconds;
                    break;

                case edsNanoSecLen0:
                    integer64 = 0;
                    scale = 0;
                    type = edsTypeNanoseconds;
                    break;

                case edsNanoSecLen1:
                case edsNanoSecLen2:
                case edsNanoSecLen3:
                case edsNanoSecLen4:
                case edsNanoSecLen5:
                case edsNanoSecLen6:
                case edsNanoSecLen7:
                case edsNanoSecLen8:
                    l = code - edsNanoSecLen1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    scale = 0;
                    type = edsTypeNanoseconds;
                    break;

                //
                // 09/2012 Added support for new scaled date/times encoding. These replace the old encoding
                // edsTimeLen, edsMilliSecLen and edsNanoSecLen.
                //

                case edsScaledTimeLen1:
                case edsScaledTimeLen2:
                case edsScaledTimeLen3:
                case edsScaledTimeLen4:
                case edsScaledTimeLen5:
                case edsScaledTimeLen6:
                case edsScaledTimeLen7:
                case edsScaledTimeLen8:
                    scale = (sbyte)source[offset++];
                    l = code - edsScaledTimeLen1 + 1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 1; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    type = edsTypeScaledTime;
                    break;

                case edsScaledDateLen1:
                case edsScaledDateLen2:
                case edsScaledDateLen3:
                case edsScaledDateLen4:
                case edsScaledDateLen5:
                case edsScaledDateLen6:
                case edsScaledDateLen7:
                case edsScaledDateLen8:
                    scale = (sbyte)source[offset++];
                    l = code - edsScaledDateLen1 + 1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 1; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    type = edsTypeScaledDate;
                    break;

                case edsScaledTimestampLen1:
                case edsScaledTimestampLen2:
                case edsScaledTimestampLen3:
                case edsScaledTimestampLen4:
                case edsScaledTimestampLen5:
                case edsScaledTimestampLen6:
                case edsScaledTimestampLen7:
                case edsScaledTimestampLen8:
                    scale = (sbyte)source[offset++];
                    l = code - edsScaledTimestampLen1 + 1;
                    integer64 = (sbyte)source[offset++];

                    for (int n = 1; n < l; ++n)
                    {
                        integer64 = (integer64 << 8) | (source[offset++] & 0xff);
                    }

                    type = edsTypeScaledTimestamp;
                    break;

                case edsClobLen0:
                    blobId = 0;
                    type = edsTypeClob;
                    break;

                case edsClobLen1:
                case edsClobLen2:
                case edsClobLen3:
                case edsClobLen4:
                    l = code - edsClobLen1;
                    blobId = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        blobId = (blobId << 8) | (source[offset++] & 0xff);
                    }

                    type = edsTypeClob;
                    break;

                case edsBlobLen0:
                    blobId = 0;
                    type = edsTypeBlob;
                    break;

                case edsBlobLen1:
                case edsBlobLen2:
                case edsBlobLen3:
                case edsBlobLen4:
                    l = code - edsBlobLen1;
                    blobId = (sbyte)source[offset++];

                    for (int n = 0; n < l; ++n)
                    {
                        blobId = (blobId << 8) | (source[offset++] & 0xff);
                    }

                    type = edsTypeBlob;
                    break;

                case edsScaledCount1:
                    {
                        // For some reason C++ EncodedStream expects the encoded
                        // length to be length+1.

                        length = (source[offset++] & 0xff) - 1;
                        scale = (sbyte)source[offset++];
                        type = edsTypeBigInt;
                        bytes = new byte[length];
                        Array.Copy(source, offset, bytes, 0, length);
                        offset += length;
                        int sign = ((bytes[0] & 0x80) > 0) ? -1 : 1;

                        if (sign == -1)
                        {
                            bytes[0] &= 0x7f;
                        }

                        BigInteger bi = new BigInteger(bytes);
                        bigDecimal = Decimal.Parse(bi.ToString());
                    }
                    break;

                case edsScaledCount2:
                    {
                        // For some reason C++ EncodedStream expects the encoded
                        // length to be length+1.

                        scale = (sbyte)source[offset++];
                        sbyte sign = (sbyte)source[offset++];
                        length = (source[offset++] & 0xff); // in bytes
                        type = edsTypeBigInt;
                        bytes = new byte[length];
                        Array.Copy(source, offset, bytes, 0, length);
                        offset += length;

                        if (sign == -1)
                            bytes[0] &= 0x7f;
                        else
                            sign = 1;

                        BigInteger bi = new BigInteger(bytes);
                        if (sign == -1)
                            bi = -bi;
                        bigDecimal = ScaleDecimal(Decimal.Parse(bi.ToString()), scale);
                    }
                    break;

                case edsUUID:
                    {
                        byte[] buff = new byte[16];

                        for (int n = 0; n < 16; ++n)
                        {
                            buff[n] = (byte)(source[offset++] & 0xff);
                        }

                        uuid = new Guid(buff);
                        type = edsTypeUUID;
                    }
                    break;

                default:
                    type = edsTypeUnknown;
                    break;
            }

            priorCode = code;

            return offset;
        }

        public virtual string getString()
        {
            decode();

            switch (type)
            {
                case edsTypeNull:
                    return null;

                case edsTypeUtf8:
                    return @string;

                default:
                    throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected string, got type: " + type);
            }
        }

        public virtual Guid getUUId()
        {
            decode();

            switch (type)
            {
                case edsTypeUUID:
                    return uuid;

                default:
                    throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected UUID, got type: " + type);
            }
        }

        public virtual int getInt()
        {
            decode();

            if (type == edsTypeInt32)
            {
                return integer32;
            }

            if (type == edsTypeInt64)
            {
                return (int)integer64;
            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected int32, got type: " + type);
        }

        public virtual long getLong()
        {
            decode();

            if (type == edsTypeInt32)
            {
                return integer32;
            }

            if (type == edsTypeInt64)
            {
                return integer64;
            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected int64 got type: " + type);
        }

        public virtual double getDouble()
        {
            decode();

            if (type == edsTypeDouble)
            {
                return dbl;
            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected double got type: " + type);
        }

        public virtual decimal getBigDecimal()
        {
            decode();

            if (type == edsTypeBigInt)
            {
                return bigDecimal;
            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected BigDecimal got type: " + type);
        }

        public virtual bool getBoolean()
        {
            decode();

            if (type == edsTypeBoolean)
            {
                return @bool;
            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected boolean, got type: " + type);
        }

        public virtual byte[] getBytes()
        {
            decode();

            if (type == edsTypeOpaque)
            {
                return bytes;
            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc expected bytes, got type: " + type);
        }

        public virtual void encodeNull()
        {
            write(edsNull);
        }

        internal virtual Value getValue(SQLContext sqlContext)
        {
            decode();

            switch (type)
            {
                case edsTypeNull:
                    return new ValueNull();

                case edsTypeUtf8:
                    return new ValueString(@string);

                case edsTypeOpaque:
                    return new ValueBytes(bytes);

                case edsTypeScaled:
                    return new ValueNumber(ScaleDecimal(integer64, scale));

                case edsTypeInt32:
                    return new ValueInt(integer32, 0);

                case edsTypeInt64:
                    return new ValueLong(integer64, 0);

                case edsTypeBoolean:
                    return new ValueBoolean(@bool);

                case edsTypeDouble:
                    return new ValueDouble(dbl);

                case edsTypeScaledTime:
                    {
                        long inNanos = Value.reScale(integer64, scale, NANOSECONDS_SCALE);
                        DateTime utcTime = new DateTime(baseDate.Ticks + inNanos / NANOSECONDS_PER_TICK, DateTimeKind.Utc);
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, sqlContext.TimeZone);
                        return new ValueTime(localTime);
                    }

                case edsTypeTime:
                    {
                        DateTime utcTime = new DateTime(baseDate.Ticks + integer64 * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, sqlContext.TimeZone);
                        return new ValueTime(localTime);
                    }

                case edsTypeScaledDate:
                    {
                        long inSeconds = Value.reScale(integer64, scale, SECONDS_SCALE);
                        DateTime utcTime = new DateTime(baseDate.Ticks + inSeconds * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, sqlContext.TimeZone);
                        return new ValueDate(localTime);
                    }

                case edsTypeMilliseconds:
                    {
                        DateTime utcTime = new DateTime(baseDate.Ticks + integer64 * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, sqlContext.TimeZone);
                        return new ValueDate(localTime);
                    }

                case edsTypeScaledTimestamp:
                    {
                        long inNanos = Value.reScale(integer64, scale, NANOSECONDS_SCALE);
                        DateTime utcTime = new DateTime(baseDate.Ticks + inNanos / NANOSECONDS_PER_TICK, DateTimeKind.Utc);
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, sqlContext.TimeZone);
                        return new ValueTimestamp(localTime);
                    }

                case edsTypeNanoseconds:
                    {
                        DateTime utcTime = new DateTime(baseDate.Ticks + integer64 / NANOSECONDS_PER_TICK, DateTimeKind.Utc);
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, sqlContext.TimeZone);
                        return new ValueTimestamp(localTime);
                    }

                case edsTypeBigInt:
                    return new ValueNumber(bigDecimal);

            }

            throw new NuoDbSqlException("On message type " + currentMessageType + ":NuoDB jdbc decode value type " + type + " not yet implemented");
        }

        public virtual bool EndOfMessage
        {
            get
            {
                return offset >= totalLength;
            }
        }

        public virtual void encodeDate(DateTime date)
        {
            if (date == null)
            {
                write(edsNull);

                return;
            }
            var dateValue = date.Kind == DateTimeKind.Local ? date.ToUniversalTime() : date;
            TimeSpan span = dateValue - baseDate;
            long value = (long)(span.TotalMilliseconds);
            int count = byteCount(value);
            write(edsMilliSecLen0 + count);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(value >> shift));
            }
        }

        public virtual void encodeScaledDate(DateTime date)
        {
            if (date == null)
            {
                write(edsNull);

                return;
            }
            var dateValue = date.Kind == DateTimeKind.Local ? date.ToUniversalTime() : date;
            TimeSpan span = (protocolVersion >= Protocol.PROTOCOL_VERSION10 ? dateValue : date) - baseDate;
            // always send seconds. ms in date is useless
            long value = (long)span.TotalMilliseconds / 1000;

            int count = byteCount(value);
            write(edsScaledDateLen1 + count - 1);
            write(SECONDS_SCALE);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(value >> shift));
            }

        }

        public virtual void encodeDouble(double dbl)
        {
            long value = BitConverter.DoubleToInt64Bits(dbl);

            write(edsDoubleLen0 + 8);

            for (int n = 56; n >= 0; n -= 8)
            {
                write((int)(value >> n) & 0xff);
            }
        }

        public virtual void encodeBigDecimal(Decimal bd)
        {
            int scale;
            Decimal temp;
            ConvertToScaledDecimal(bd, out scale, out temp);
            if (temp.CompareTo(long.MinValue) > 0 && temp.CompareTo(long.MaxValue) < 0)
                encodeLong((long)temp, scale);
            else
            {
                BigInteger bi = new BigInteger(Decimal.Truncate(Math.Abs(temp)).ToString(), 10);
                byte[] byteArray = bi.ToByteArray();

                write(edsScaledCount2);
                write(scale);
                write(bd.CompareTo(Decimal.Zero));
                write(byteArray.Length);
                write(byteArray);
            }
        }

        public virtual void encodeOldBigDecimal(decimal bd)
        {
            int scale;
            Decimal temp;
            ConvertToScaledDecimal(bd, out scale, out temp);
            int neg = bd.CompareTo(decimal.Zero) == -1 ? 1 : 0;

            // The server expects a byte array with a signed first byte.
            // BigInteger.toByteArray() creates an array of the value in two's compliment.
            // So get the unsigned value and set the sign bit manually.

            BigInteger bi = new BigInteger(Decimal.Truncate(Math.Abs(temp)).ToString(), 10);
            byte[] byteArray = bi.ToByteArray();

            if (neg == 1)
            {
                byteArray[0] |= 0x80;
            }

            write(edsScaledCount1);
            write(byteArray.Length + 1);
            write(scale);
            write(byteArray);
        }

        public virtual void encodeScaledTime(TimeSpan time)
        {
            if (time == null)
            {
                write(edsNull);

                return;
            }

            encodeScaledTime(baseDate + time);
        }

        public virtual void encodeTime(TimeSpan time)
        {
            if (time == null)
            {
                write(edsNull);

                return;
            }

            encodeTime(baseDate + time);
        }

        public virtual void encodeTime(DateTime time)
        {
            if (time == null)
            {
                write(edsNull);

                return;
            }

            long milliSecondsSinceMidnight = (long)(time.ToUniversalTime().TimeOfDay.TotalMilliseconds);

            TimeZone tz = TimeZone.CurrentTimeZone;
            if (tz.IsDaylightSavingTime(time))
            {
                System.Globalization.DaylightTime period = tz.GetDaylightChanges(time.Year);
                milliSecondsSinceMidnight += (long)period.Delta.TotalMilliseconds;
            }

            int count = byteCount(milliSecondsSinceMidnight);

            // unlike other int types which have 8 bytes we only have 4 bytes to encode time
            if (count > (edsTimeLen4 - edsTimeLen0))
            {
                throw new NuoDbSqlException(String.Format("Unable to encode \"{0}\".  Need {1} bytes to encode but only have {2} bytes.", time, count, edsTimeLen4 - edsTimeLen0));
            }

            write(edsTimeLen0 + count);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(milliSecondsSinceMidnight >> shift));
            }
        }

        public virtual void encodeScaledTime(DateTime time)
        {
            if (time == null)
            {
                write(edsNull);

                return;
            }

            TimeSpan span = time.ToUniversalTime() - baseDate;
            long value = (long)span.TotalMilliseconds;
            int count = byteCount(value);
            write(edsScaledTimeLen1 + count - 1);
            write(MILLISECONDS_SCALE);

            for (int shift = (count - 1) * 8; shift >= 0; shift -= 8)
            {
                write((int)(value >> shift));
            }
        }

        public virtual void encodeGuid(Guid guid)
        {
            encodeString(guid.ToNuoDbString());
        }

        public override void getMessage(CryptoInputStream stream)
        {
            base.getMessage(stream);
            buffer = Buffer;
            offset = 0;
        }

        public override void setData(byte[] data, int offset, int length)
        {
            base.setData(data, offset, length);
            buffer = Buffer;
            offset = 0;
        }

        public override int setBase64(char[] data, int dataOffset, int length)
        {
            int ret = base.setBase64(data, dataOffset, length);
            buffer = Buffer;
            offset = 0;

            return ret;
        }

        protected internal int ProtocolVersion
        {
            get { return protocolVersion; }
            set { protocolVersion = value; }
        }

    }
}
