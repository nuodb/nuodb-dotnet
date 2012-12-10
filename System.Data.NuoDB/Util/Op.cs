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

using System.Text;

namespace System.Data.NuoDB.Util
{

	//
	//
	// Op
	//
	//
	public class Op
	{
		public static string stringDivide(string @string, char c)
		{
			int n = @string.IndexOf(c);

			if (n < 0)
			{
				return @string;
			}

			return @string.Substring(0, n);
		}

		public static string stringRemainder(string @string, char c)
		{
			int n = @string.IndexOf(c);

			if (n < 0)
			{
				return "";
			}

			return @string.Substring(n + 1);
		}

		public static int getInt(string @string)
		{
			int value = 0;
			bool negative = false;
			int length = @string.Length;

			for (int n = 0; n < length; ++n)
			{
				char c = @string[n];

				if (c >= '0' && c <= '9')
				{
					value = value * 10 + c - '0';
				}
				else if (c == '-')
				{
					negative = true;
				}
			}

			return (negative) ? - value : value;
		}

		public static string obfuscate(string[] args)
		{
			StringBuilder builder = new StringBuilder(512);

			for (int n = 0; n < args.Length; ++n)
			{
				string @string = args [n];
				int l = @string.Length;

				for (int i = 0; i < l; ++i)
				{
					char c = @string[i];
					if (c == '@')
					{
						builder.Append("&#");
						builder.Append((char)('0' + (c / 100) % 10));
						builder.Append((char)('0' + (c / 10) % 10));
						builder.Append((char)('0' + c % 10));
						builder.Append(';');
					}
					else
					{
						builder.Append(c);
					}
				}
			}

			return builder.ToString();
		}

		public static long getLong(string @string)
		{
			long value = 0;
			bool negative = false;
			int length = @string.Length;

			for (int n = 0; n < length; ++n)
			{
				char c = @string[n];

				if (c >= '0' && c <= '9')
				{
					value = value * 10 + c - '0';
				}
				else if (c == '-')
				{
					negative = true;
				}
			}

			return (negative) ? - value : value;
		}

		// Convert string to seconds since midnight (i.e. Time)

		public static int getTimeNoTZN(string @string)
		{
			int[] values = new int[3];
			int segment = 0;

			for (int length = @string.Length, n = 0; n < length; ++n)
			{
				char c = @string[n];

				if (c >= '0' && c <= '9')
				{
					values[segment] = values[segment] * 10 + c - '0';
				}
				else if (c == ':')
				{
					++segment;
				}
			}

			return values[0] * 60 * 60 + values[1] * 60 + values[2];
		}

		public static string timeToStringNoTZN(int time)
		{
			// .n.b. This is a primary jdbc client hotspot consuming more than
			// 20% CPU itself when performing a flights test; by writing custom
			// BCD encoding logic the elapsed time of this method reduces 78%.
			// The contributed CPU utilization drops from 20% to 3%.
			int hh = time / (60 * 60);
			int mm = (time % (60 * 60)) / 60;
			int ss = (time % (60 * 60)) % 60;
			char[] packed = new char[8];
			packed[0] = (char)('0' + (hh - (hh % 10)) / 10);
			packed[1] = (char)('0' + (hh % 10));
			packed[2] = ':';
			packed[3] = (char)('0' + (mm - (mm % 10)) / 10);
			packed[4] = (char)('0' + (mm % 10));
			packed[5] = ':';
			packed[6] = (char)('0' + (ss - (ss % 10)) / 10);
			packed[7] = (char)('0' + (ss % 10));

			return new string(packed);
		}
	}


}