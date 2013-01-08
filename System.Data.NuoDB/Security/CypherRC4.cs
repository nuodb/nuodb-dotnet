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
using System.Linq;
using System.Text;
using System.IO;

namespace System.Data.NuoDB.Security
{
    class CipherRC4 : Cipher
    {
        internal byte[] buffer;
        internal byte[] state;
        internal int s1;
        internal int s2;

        public CipherRC4(byte[] key, int offset, int length)
        {
            setKey(key, offset, length);
        }

        public CipherRC4(byte[] key)
        {
            setKey(key, 0, key.Length);
        }

        public override int BlockSize
        {
            get
            {
                return 1;
            }
        }

        public override int read(Stream inputStream, byte[] bytes, int offset, int length)
        {
            int bytesRead = 0;

            while (bytesRead < length)
            {
                bytesRead += inputStream.Read(bytes, bytesRead, length - bytesRead);
                //System.out.println( "CipherRC4::read, len = " + bytesRead + " actual length = " + length + " offset = " + offset); 
            }

            transform(bytes, offset, bytesRead);

            return bytesRead;
        }

        public override void setKey(byte[] key, int offset, int length)
        {
            state = new byte[256];

            for (int n = 0; n < state.Length; ++n)
            {
                state[n] = (byte)n;
            }

            for (int k1 = 0, k2 = 0; k1 < 256; ++k1)
            {
                k2 = (k2 + key[(k1 + offset) % length] + state[k1]) & 0xff;
                byte temp = state[k1];
                state[k1] = state[k2];
                state[k2] = temp;
            }

            s1 = s2 = 0;
        }

        public override void write(Stream outputStream, byte[] bytes, int offset, int length)
        {
            if (buffer == null || buffer.Length < length)
            {
                buffer = new byte[length + 100];
            }

            for (int n = offset, end = offset + length; n < end; ++n)
            {
                s1 = (s1 + 1) & 0xff;
                s2 = (s2 + state[s1]) & 0xff;
                byte temp = state[s1];
                state[s1] = state[s2];
                state[s2] = temp;
                byte b = state[(state[s1] + state[s2]) & 0xff];
                buffer[n] = (byte)(bytes[n] ^ b);
            }

            outputStream.Write(buffer, 0, length);
        }

        public virtual void transform(byte[] data, int offset, int length)
        {
            for (int n = offset, end = offset + length; n < end; ++n)
            {
                s1 = (s1 + 1) & 0xff;
                s2 = (s2 + state[s1]) & 0xff;
                byte temp = state[s1];
                state[s1] = state[s2];
                state[s2] = temp;
                byte b = state[(state[s1] + state[s2]) & 0xff];
                data[n] ^= b;
            }
        }

    }

}
