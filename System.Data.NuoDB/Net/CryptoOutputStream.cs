/****************************************************************************
* Copyright (c) 2012, NuoDB, Inc.
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

using System.IO;
using System.Data.NuoDB.Security;

namespace System.Data.NuoDB.Net
{
	class CryptoOutputStream : Stream
	{
		internal Stream stream;
		internal CryptoSocket socket;
		internal Cipher cipher;
		internal byte[] lengthBuffer;

		public CryptoOutputStream(CryptoSocket cryptoSocket, Stream outputStream)
		{
			socket = cryptoSocket;
			//stream = outputStream;
            stream = new BufferedStream(outputStream);
		}

		public CryptoOutputStream(Stream outputStream)
		{
            stream = new BufferedStream(outputStream);
		}

		public virtual void encrypt(Cipher encryptionEngine)
		{
			cipher = encryptionEngine;
		}

		public virtual void writeLength(int messageLength)
		{
			if (lengthBuffer == null)
			{
				lengthBuffer = new byte[4];
			}

			for (int n = 3, length = messageLength; n >= 0; --n, length >>= 8)
			{
				lengthBuffer[n] = (byte) length;
			}

			stream.Write(lengthBuffer, 0, 4);
		}

        public override void WriteByte(byte b)
        {
            if (cipher == null)
            {
                stream.WriteByte(b);
            }
            else
            {
                byte[] array = new byte[1];
                array[0] = b;
                Write(array, 0, 1);
            }
        }

		public void Write(byte[] b)
		{
			Write(b, 0, b.Length);
		}

		public override void Write(byte[] b, int offset, int length)
		{
			if (cipher == null)
			{
				stream.Write(b, offset, length);
			}
			else
			{
				cipher.write(stream, b, offset, length);
			}
		}

		public override void Flush()
		{
			stream.Flush();
		}

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }

}