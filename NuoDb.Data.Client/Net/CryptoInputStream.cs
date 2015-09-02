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
using NuoDb.Data.Client.Security;

namespace NuoDb.Data.Client.Net
{
    class CryptoInputStream : Stream
    {
        internal Stream stream;
        internal CryptoSocket socket;
        internal Cipher cipher;
        internal byte[] lengthBuffer;

        public CryptoInputStream(CryptoSocket cryptoSocket, Stream inputStream)
        {
            socket = cryptoSocket;
            stream = new BufferedStream(inputStream, 8192);
            lengthBuffer = new byte[4];
        }

        public CryptoInputStream(Stream inputStream)
        {
            stream = new BufferedStream(inputStream, 8192);
            lengthBuffer = new byte[4];
        }

        public virtual void encrypt(Cipher encryptionEngine)
        {
            cipher = encryptionEngine;
        }

        public override int ReadByte()
        {
            if (cipher == null)
            {
                return stream.ReadByte();
            }

            byte[] buffer = new byte[1];
            int n = cipher.read(stream, buffer, 0, 1);

            //System.out.println("CryptoInputStream::read()");

            return (n == 0) ? -1 : buffer[0];
        }

        public int Read(byte[] b)
        {
            //System.out.println("CryptoInputStream::read([])");
            return Read(b, 0, b.Length);
        }

        public override int Read(byte[] b, int offset, int length)
        {
            if (cipher == null)
            {
                return stream.Read(b, offset, length);
            }

            //System.out.println("CryptoInputStream::read([], offset, length)");

            return cipher.read(stream, b, offset, length);
        }

        public virtual int readLength()
        {
            int remaining = 4;
            while (remaining > 0)
            {
                int lengthRead = stream.Read(lengthBuffer, 4 - remaining, remaining);

                if (lengthRead == 0)
                {
                    throw new IOException("End of stream reached");
                }

                remaining -= lengthRead;
            }

            int length = 0;

            for (int n = 0; n < 4; ++n)
            {
                length = (length << 8) | (lengthBuffer[n] & 0xff);
            }

            return length;
        }

        public virtual byte[] readMessage()
        {
            int length = readLength();
            byte[] data = new byte[length];
            Read(data);

            return data;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return stream.Length; }
        }

        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
