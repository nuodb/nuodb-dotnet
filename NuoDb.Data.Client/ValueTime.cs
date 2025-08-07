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
    // ValueTime
    //
    //
    class ValueTime : Value
    {
        internal TimeOnly value;

        public ValueTime(TimeOnly val)
        {
            value = val;
        }

        public ValueTime(object val)
        {
            if (val is TimeOnly)
            {
                value = (TimeOnly)val;
            }
            else
            {
                throw new System.ArgumentException("Unable to convert: " + val.GetType().Name + " into a Time");
            }
        }

        public override int Type
        {
            get
            {
                return typeTimeType; // C semantics
            }
        }

        public override TimeOnly Time
        {
            get
            {
                return value;
            }
        }

        public override DateTime TimeStamp
        {
            get
            {
                return new DateTime(nullDate, value);
            }
        }

        public override object Object
        {
            get
            {
                return value;
            }
        }

        internal override void encodeValue(EncodedDataStream dataStream)
        {
            dataStream.encodeTime(value);
        }

        public override string String
        {
            get
            {
                return value.ToString(timeFormat);
            }
        }

        /*		public override sbyte[] AsBytes
                {
                    get
                    {
                        return Conversions.toBytes(value.Time);
                    }
                }
        */
    }


}