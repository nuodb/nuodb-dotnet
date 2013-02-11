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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NuoDb.Data.Client.Net;

namespace NuoDb.Data.Client
{
    /*
     *  The class DataStream exists to build new messages incrementally and to hold
     *  existing messages to be processed incrementally.  It also handles translation
     *  between UTF-8 and Unicode / UTF-16.
     *
     *  Messages are stored in one or more Segments (messages read are always
     *  held in a single Segment).  Segments are allocated on demand with a minimum
     *  segment size, and once allocated, appended to until filled.   New segments are
     *  allocated as needed and always large enough to hold data on hand.
     *
     *  DataStream has no state except the data contained.  Data is always written
     *  to the end, otherwise there is no concept of "current position".
     *
     *  There is an implicit assumption that messages will be handled by segment, not
     *  by byte.
     *
     *  For extra credit, DataStream also performs import and export of messages in base64.
     *  Backup likes this.
     *
     */
    class DataStream : IEnumerable<DataStream.Segment>
    {
		public int totalLength;
		protected internal DataSegment firstSegment;
		protected internal DataSegment currentSegment;
		protected internal int available; // space available in current segment
		protected internal char[] capturedString;
		protected internal int capturedLength; // the actual (used) length of the captured string
		protected internal char[] codePoints;

		internal static byte[] base64Digits;
		internal static byte[] base64Lookup;

		internal const int DEFAULT_SEGMENT_SIZE = 1024;
		public const int DEFAULT_STRING_SIZE = 512;

		internal static readonly byte[] utf8Flags = {0, 0, 0xC0, 0xE0, 0xF0, 0xF8, 0xFC};

		internal static readonly int[] utf8Lengths = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

		internal static readonly int[] utf8Values = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 0, 1, 0, 1};


		static DataStream()
		{
			base64Lookup = new byte[256];
			base64Digits = new byte[64];
			string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

			for (int n = 0; n < base64Lookup.Length; ++n)
			{
				base64Lookup[n] = 0xff;
			}

			for (int n = 0; n < 64; ++n)
			{
				char c = alphabet[n];
				base64Lookup[c] = (byte) n;
				base64Digits[n] = (byte) c;
			}
		}

		public interface Segment
		{
			int Length {get;}
			byte[] Buffer {get;}
		}

		protected internal class DataSegment : Segment
		{
			private readonly DataStream outerInstance;

			internal int length; // length in use
			internal byte[] buffer;
			internal DataSegment next;

			internal DataSegment(DataStream outerInstance)
			{
					this.outerInstance = outerInstance;
			}

			internal DataSegment(DataStream outerInstance, int minLength)
			{
					this.outerInstance = outerInstance;
				allocate(minLength);
			}

			internal virtual void reset()
			{
				length = 0;
				next = null;
			}

			internal virtual int read(CryptoInputStream stream, int len)
			{
				int actualLength = stream.Read(buffer, length, Math.Min(buffer.Length - length, len));

				if (actualLength < 0)
				{
					throw new IOException("End of stream reached");
				}

				length += actualLength;

				return actualLength;
			}

			internal virtual void zap()
			{
				if (length > 0)
				{
                    throw new InvalidOperationException("DataStream is partially filled");
				}

				buffer = null;
			}

			internal virtual void allocate(int minLength)
			{
				buffer = new byte[Math.Max(minLength, DEFAULT_SEGMENT_SIZE)];
			}

			public int Length
			{
				get
				{
					return length;
				}
			}

			public byte[] Buffer
			{
				get
				{
					return buffer;
				}
			}
		}

		public class IterateSegments : IEnumerator<Segment>
		{
			private readonly DataStream outerInstance;

            internal DataSegment firstSegment;
            internal DataSegment currentSegment;

			internal IterateSegments(DataStream outerInstance, DataSegment firstSegment)
			{
				this.outerInstance = outerInstance;
                this.firstSegment = firstSegment;
                this.currentSegment = null;
			}

            #region IEnumerator<Segment> Members

            public Segment Current
            {
                get { return currentSegment; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return currentSegment; }
            }

            public bool MoveNext()
            {
                if (currentSegment == null)
                    currentSegment = firstSegment;
                else
                    currentSegment = currentSegment.next;
                return currentSegment != null;
            }

            public void Reset()
            {
                currentSegment = null;
            }

            #endregion
        }

		/// <summary>
		/// Return a used DataStream to new condition with a single segment
		/// </summary>

		public virtual void reset()
		{
			totalLength = 0;

			if (firstSegment != null)
			{
				firstSegment.reset();
				currentSegment = firstSegment;
			}

			available = (firstSegment == null || firstSegment.buffer == null) ? 0 : firstSegment.buffer.Length;
		}

		/// <summary>
		/// Append a slice of a byte array to the DataStream.
		/// </summary>
		/// <param name="byteArray"> </param>
		/// <param name="offset"> </param>
		/// <param name="length"> </param>

		public void write(byte[] b, int offset, int length)
		{
			for (int off = offset, len = length; len > 0;)
			{
				if (available == 0)
				{
					extend(len);
				}

				int l = Math.Min(available, len);
				Array.Copy(b, off, currentSegment.buffer, currentSegment.length, l);
				currentSegment.length += l;
				off += l;
				len -= l;
				totalLength += l;
				available -= l;
			}
		}

		/// <summary>
		/// Append the UTF-8 analog of a String to the DataStream.
		/// </summary>
		/// <param name="string"> </param>

		public void write(string @string)
		{
			captureString(@string);
			writeCapturedString();
		}

		/// <summary>
		/// Append a single byte to the DataStream.  Accept an "int" as a courtesy.
		/// </summary>
		/// <param name="aByte"> </param>

		public void write(int b)
		{
			if (available == 0)
			{
				extend(1);
			}

			currentSegment.buffer[currentSegment.length++] = (byte) b;
			++totalLength;
			--available;
		}

		/// <summary>
		/// Force contiguous allocate of a minimum of space.  This also does a total reset of
		/// the object.
		/// </summary>
		/// <param name="len"> </param>

		private void allocate(int len)
		{
			if (firstSegment == null)
			{
				firstSegment = currentSegment = new DataSegment(this, len);
			}
			else if (firstSegment.buffer == null || firstSegment.buffer.Length < len)
			{
				firstSegment.allocate(len);
			}

			currentSegment = firstSegment;
			firstSegment.next = null;
			totalLength = 0;
			available = currentSegment.buffer.Length;
		}

		/// <summary>
		/// Find space.  This should only be called when all space has been exhausted
		/// </summary>
		/// <param name="length">	minimum space required </param>

		private void extend(int len)
		{
			if (firstSegment == null)
			{
				firstSegment = currentSegment = new DataSegment(this, len);
			}
			else if (currentSegment.buffer == null)
			{
				currentSegment.allocate(len);
			}
			else
			{
				DataSegment newSegment = new DataSegment(this, len);
				currentSegment.next = newSegment;
				currentSegment = newSegment;
			}

			available = currentSegment.buffer.Length;
		}

		/// <summary>
		/// Append a "captured" string (see captureString, below).
		/// </summary>
		/// <param name="stringLength"> </param>

		protected internal virtual void writeCapturedString()
		{
			// This doesn't handle unicode > 16 bits

			for (int n = 0; n < capturedLength;)
			{
				char c = capturedString[n++];
				int utf8Length = (c <= 0x7f) ? 1 : (c <= 0x7ff) ? 2 : (c <= 0xffff) ? 3 : 4;

				if (utf8Length == 1)
				{
					write(c);
				}
				else
				{
					int bits = (utf8Length - 1) * 6;
					write((c >> bits) | utf8Flags[utf8Length]);

					while (bits > 0)
					{
						bits -= 6;
						write(((c >> bits) & 0x3f) | 0x80);
					}
				}
			}
		}

		/// <summary>
		/// Snapshot string, computing UTF-8 length.  For efficiency, try to reuse an existing
		/// stringArray
		/// </summary>
		/// <param name="string">
		/// @return </param>

		protected internal virtual int captureString(string @string)
		{
			capturedLength = @string.Length;

			if (capturedString == null || capturedString.Length < capturedLength)
			{
				capturedString = new char[Math.Max(capturedLength, DEFAULT_STRING_SIZE)];
			}

			@string.CopyTo(0, capturedString, 0, capturedLength - 0);
			int utf8Length = 0;

			for (int n = 0; n < capturedLength; ++n)
			{
				char code = capturedString[n];
				utf8Length += (code <= 0x7f) ? 1 : (code <= 0x7ff) ? 2 : (code <= 0xffff) ? 3 : 4;
			}

			return utf8Length;
		}

		/// <summary>
		/// Converts a UTF-8 byte array to a String.  Try to re-use an
		/// intermediate character array for efficency.
		/// </summary>
		/// <param name="source"> </param>
		/// <param name="sourceOffset"> </param>
		/// <param name="sourceLength">
		/// @return </param>

		public virtual string getString(byte[] source, int sourceOffset, int length)
		{
			if (codePoints == null || codePoints.Length < length)
			{
				codePoints = new char[Math.Max(length, 512)];
			}

			int offset = sourceOffset;
			int count = 0;

			while (offset - sourceOffset < length)
			{
				int b = 0xff & source[offset++];
				int len = utf8Lengths[b];
				int code = utf8Values[b];

				for (; len > 1; --len)
				{
					code = (code << 6) | (source[offset++] & 0x3f);
				}

				codePoints[count++] = (char)code;
			}

			return new string(codePoints, 0, count);
		}

		/// <summary>
		/// Read a message from an input stream.  The message will be fully
		/// contained in the first segment
		/// </summary>
		/// <param name="stream"> </param>
		/// <exception cref="IOException"> </exception>

		public virtual void getMessage(CryptoInputStream stream)
		{
			reset();
			int length = stream.readLength();

			if (length > available)
			{
				if (firstSegment != null)
				{
					firstSegment.zap();
				}

				available = 0;
				extend(length);
			}

			int offset = 0;

			while (offset < length)
			{
				int lengthRead = firstSegment.read(stream, length - offset);

				if (lengthRead == -1)
				{
					throw new IOException("End of stream reached");
				}

				offset += lengthRead;
			}

			totalLength = length;
		}

		/// <summary>
		/// Populate with existing data.
		/// </summary>
		/// <param name="data"> </param>
		/// <param name="offset"> </param>
		/// <param name="length"> </param>

		 public virtual void setData(byte[] data, int offset, int length)
		 {
			 allocate(length);
			Array.Copy(data, offset, firstSegment.buffer, 0, length);
			totalLength = length;
			   available = firstSegment.buffer.Length - firstSegment.length;
		 }

		/// <summary>
		/// Populate the DataSegment with the decode of a base-64 character arrary.
		/// </summary>
		/// <param name="base64CharArray"> </param>
		/// <param name="offset"> </param>
		/// <param name="length">
		/// @return </param>

		 public virtual int setBase64(char[] data, int offset, int length)
		 {
			allocate((length + 2) * 3 / 4);
			byte[] buffer = firstSegment.buffer;
			int @in = offset;
			int @out = 0;
			int bitsRemaining = 0;
			int bits = 0;

			for (int n = 0; n < length; ++n)
			{
				 char c = data[@in++];
				 int digit = base64Lookup[c];

				 if (c == '=')
				 {
					 if (bitsRemaining > 0)
					 {
						 buffer[@out++] = (byte)(bits << (6 - bitsRemaining));
						  bitsRemaining = 0;
					 }
				 }
				 else if (digit != 0xff)
				 {
					bits = (bits << 6) | digit;
					bitsRemaining += 6;
				 }

				 if (bitsRemaining >= 8)
				 {
					bitsRemaining -= 8;
					 buffer[@out++] = (byte)(bits >> bitsRemaining);
				 }
			}

			totalLength = firstSegment.length = @out;
			available = firstSegment.buffer.Length - firstSegment.length;

			return @out;
		 }

		 /// <summary>
		 /// Encode the contents of the DataStream in base64 and write to an output stream.
		 /// </summary>
		 /// <param name="outputStream"> </param>
		 /// <exception cref="IOException"> </exception>

		public virtual void putBase64(Stream outputStream)
		{
			int bitsRemaining = 0;
			int bits = 0;
			int length = 0;
			int @out = 0;

			for (DataSegment segment = firstSegment; segment != null; segment = segment.next)
			{
				length += segment.length;

				for (int n = 0; n < segment.length;)
				{
					if (bitsRemaining < 6)
					{
						bits = (bits << 8) | (segment.buffer[n++] & 0xff);
						bitsRemaining += 8;
					}

					while (bitsRemaining >= 6)
					{
						bitsRemaining -= 6;
						int digit = (bits >> bitsRemaining) & 0x3f;
						byte c = base64Digits[digit];
						++@out;
						outputStream.WriteByte(c);
					}
				}
			}

			int mod = length % 3;

			if (mod > 0)
			{
				outputStream.WriteByte(base64Digits[(bits << (6 - bitsRemaining)) & 0x3f]);

				for (int n = 3; n > mod; --n)
				{
					outputStream.WriteByte((byte)'=');
				}
			}
		}

		/// <summary>
		/// Write the contents of the DataStream to an output stream.
		/// </summary>
		/// <param name="stream"> </param>
		/// <exception cref="IOException"> </exception>

		public virtual void send(CryptoOutputStream stream)
		{
			try
			{
				stream.writeLength(totalLength);

				for (DataSegment segment = firstSegment; segment != null; segment = segment.next)
				{
					stream.Write(segment.buffer, 0, segment.length);
				}

				stream.Flush();
			}
			finally
			{
				reset();
			}
		}

		/// <summary>
		/// Extract contents as a single String.
		/// 
		/// @return
		/// </summary>

		public virtual string readString()
		{
			if (firstSegment.next != null)
			{
                throw new InvalidOperationException("DataStream is multi-buffer");
			}

			return getString(firstSegment.buffer, 0, firstSegment.length);
		}

		/// <summary>
		/// Get message in single buffer.
		/// </summary>

		public virtual byte[] Buffer
		{
			get
			{
				if (firstSegment.next == null)
				{
				   return firstSegment.buffer;
				}
    
				// This is multi-segment message.  Allocate and populate a single byte array.
    
				byte[] buffer = new byte[totalLength];
				int position = 0;
    
				for (DataSegment segment = firstSegment; segment != null; segment = segment.next)
				{
					Array.Copy(segment.buffer, 0, buffer, position, segment.length);
					position += segment.length;
				}
    
				return buffer;
			}
		}

        #region IEnumerable<Segment> Members

        public IEnumerator<DataStream.Segment> GetEnumerator()
        {
            return new IterateSegments(this, firstSegment);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new IterateSegments(this, firstSegment);
        }

        #endregion
    }
}
