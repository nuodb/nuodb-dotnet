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
    // ValueBoolean
    //
    //
    class ValueBoolean : Value
    {
        internal bool value;

        public ValueBoolean(bool value)
        {
            this.value = value;
        }

        public ValueBoolean(object value)
        {
            if (value is bool?)
            {
                this.value = (bool)value;
            }
            else if (IsNumeric(value))
            {
                this.value = Convert.ToInt64(value) == 0 ? false : true;
            }
            else
            {
                this.value = Convert.ToBoolean(value.ToString());
            }
        }

        public override int Type
        {
            get
            {
                return typeBoolean; // C semantics
            }
        }

        internal override void encodeValue(EncodedDataStream dataStream)
        {
            dataStream.encodeBoolean(value);
        }

        public override string String
        {
            get
            {
                return Convert.ToString(value);
            }
        }

        public override bool Boolean
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

        /*        public override byte[] AsBytes
                {
                    get
                    {
                        return Convert.ToBytes(value);
                    }
                }
        */
        public override byte Byte
        {
            get
            {
                return (byte)(value ? 1 : 0);
            }
        }

        public override short Short
        {
            get
            {
                return Byte;
            }
        }

        public override int Int
        {
            get
            {
                return Byte;
            }
        }

        public override long Long
        {
            get
            {
                return Byte;
            }
        }

        public override float Float
        {
            get
            {
                return Byte;
            }
        }

        public override double Double
        {
            get
            {
                return Byte;
            }
        }
    }


}