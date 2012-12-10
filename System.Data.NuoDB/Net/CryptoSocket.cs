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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace System.Data.NuoDB.Net
{
    class CryptoSocket : Socket
    {
        internal Stream inputStream;
        internal Stream outputStream;

        public CryptoSocket()
            : base(AddressFamily.InterNetwork, SocketType.Unknown, ProtocolType.Unknown)
        {
        }

        public CryptoSocket(string address, int port)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)
        {
            IPHostEntry addresses = Dns.GetHostEntry(address);
            if (addresses.AddressList.Length == 0)
            {
                int pos = address.LastIndexOf(':');
                if (pos == -1)
                    throw new IOException(String.Format("Host name {0} cannot be resolved", address));

                port = Convert.ToInt32(address.Substring(pos + 1));
                address = address.Substring(0, pos);
                addresses = Dns.GetHostEntry(address);
                if (addresses.AddressList.Length == 0)
                    throw new IOException(String.Format("Host name {0} cannot be resolved", address));
            }

            try
            {
                Connect(addresses.AddressList, port);
            }
            catch (SocketException exception)
            {
                throw new IOException(exception.Message + ", " + address);
            }
        }

        public CryptoSocket(Stream input, Stream output)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)
        {
            inputStream = input;
            outputStream = output;
        }

        public CryptoSocket(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        public CryptoSocket(IPEndPoint socketAddr)
            : base(socketAddr.Address.AddressFamily, SocketType.Stream, ProtocolType.IP)
        {

            try
            {
                Connect(socketAddr);
            }
            catch (SocketException exception)
            {
                throw new IOException(exception.Message + ", " + socketAddr.ToString());
            }
        }

        public virtual CryptoInputStream InputStream
        {
            get
            {
                if (inputStream != null)
                {
                    return new CryptoInputStream(inputStream);
                }

                CryptoInputStream stream = new CryptoInputStream(this, new NetworkStream(this));

                return stream;
            }
        }

        public virtual CryptoOutputStream OutputStream
        {
            get
            {
                if (outputStream != null)
                {
                    return new CryptoOutputStream(outputStream);
                }

                CryptoOutputStream stream = new CryptoOutputStream(this, new NetworkStream(this));

                return stream;
            }
        }
    }
}
