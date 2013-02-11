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

using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using NuoDb.Data.Client.Xml;
using System.Xml;
using NuoDb.Data.Client.Security;
using System;

namespace NuoDb.Data.Client.Net
{

    class SocketListener 
    {
		protected CryptoSocket socket;
		protected CryptoInputStream inputStream;
		protected CryptoOutputStream outputStream;
		protected Thread thread;
		protected bool shutdownRequested;
		protected DataStream dataInput;
		protected DataStream dataOutput;

		public SocketListener()
		{
		}

		public SocketListener(CryptoSocket cryptoSocket)
		{
			socket = cryptoSocket;
			init();
		}

		private void init()
		{
			inputStream = socket.InputStream;
			outputStream = socket.OutputStream;
			dataInput = new DataStream();
			dataOutput = new DataStream();
		}

		public virtual CryptoSocket Socket
		{
			set
			{
				socket = value;
				init();
			}
		}

		public virtual void connect(string address, int port)
		{
            Debug.Assert(socket == null);
            socket = new CryptoSocket(address, port);
            init();
        }

		public virtual void connect(IPAddress address, int port)
		{
			Debug.Assert(socket == null);
			socket = new CryptoSocket(address, port);
			init();
		}

		public virtual void close()
		{
			if (socket != null)
			{
				try
				{
					socket.Close();
				}
				catch (IOException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		public virtual void listener()
		{
			Debug.Assert(socket != null);

			while (!shutdownRequested)
			{
				try
				{
					dataInput.getMessage(inputStream);
					messageReceived();
				}
				catch (IOException exception)
				{
					connectionFailed(exception);

					break;
				}
			}

			thread = null;
		}

		public virtual void connectionFailed(IOException exception)
		{
		}

		public virtual Tag XmlMessage
		{
			get
			{
				dataInput.getMessage(inputStream);
				Tag tag = new Tag();
    
				try
				{
					tag.parse(dataInput.readString());
				}
				catch (XmlException exception)
				{
					throw new IOException("", exception);
				}
    
				return tag;
			}
		}

		public virtual void messageReceived()
		{
			string message = "";

			try
			{
				Tag tag = new Tag();
				message = dataInput.readString();
				//System.out.println(message);
				tag.parse(message);
				messageReceived(tag);
			}
			catch (XmlException exception)
			{
				throw new IOException("", exception);
			}
		}

		public virtual void messageReceived(Tag tag)
		{
		}

		public virtual Tag Xml
		{
			get
			{
				try
				{
					dataInput.getMessage(inputStream);
					Tag xml = new Tag();
					xml.parse(dataInput.readString());
    
					return xml;
				}
				catch (XmlException exception)
				{
					throw new IOException("", exception);
				}
			}
		}

		public virtual void start()
		{
			if (thread != null)
			{
				throw new InvalidOperationException("SocketListener is already active");
			}

			shutdownRequested = false;
			thread = new Thread(new ThreadStart(listener));
			thread.Start();
		}

		public virtual void shutdown()
		{
			shutdownRequested = true;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void send(Tag xml)
		{
			string @string = xml.ToString();
			//System.out.println(string);
			dataOutput.write(@string);
			dataOutput.send(outputStream);
			dataOutput.reset();
		}

		public virtual void encrypt(byte[] key)
		{
			inputStream.encrypt(new CipherRC4(key));
			outputStream.encrypt(new CipherRC4(key));
		}
	}

}