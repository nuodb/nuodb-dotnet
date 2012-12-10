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
using System.Text;

namespace System.Data.NuoDB.Util
{


	public class StringUtils
	{

		/// <summary>
		/// Is the given string empty (size 0 or null) </summary>
		/// <param name="string"> </param>
		/// <returns> if the string is empty (size 0 or null) </returns>
		public static bool isEmpty(string @string)
		{
			return size(@string) == 0;
		}

		/// <summary>
		/// return the size of the given string in a null friendly fashion </summary>
		/// <param name="string"> </param>
		/// <returns> size of the string or 0 if string is null </returns>
		public static int size(string @string)
		{
			return @string == null ? 0 : @string.Length;
		}

		/// <summary>
		/// overwrite the given string with the new string </summary>
		/// <param name="origString"> the original string </param>
		/// <param name="pos"> the position in the original string </param>
		/// <param name="newString"> the new string to overwrite with </param>
		/// <returns> resulting string </returns>
		public static string overwrite(string origString, int pos, string newString)
		{
			return overwrite(origString, pos, newString, 0, newString.Length);
		}

		/// <summary>
		/// overwrite the given string with the new string </summary>
		/// <param name="origString"> the original string </param>
		/// <param name="pos"> the postion in the original string </param>
		/// <param name="newString"> the new string to overwrite with </param>
		/// <returns> resulting string </returns>
		public static string overwrite(string origString, int pos, string newString, int newPos, int newLength)
		{
			if (pos > origString.Length)
			{
				throw new System.IndexOutOfRangeException(String.Format("position {0} exceeds the length of the string {1}", pos, origString.Length));
			}
			StringBuilder result = new StringBuilder(pos == 0 ? "" : origString.Substring(0, pos));

			result.Append(newString.Substring(newPos, newLength));

			if (result.Length < origString.Length)
			{
				result.Append(origString.Substring(result.Length));
			}

			return result.ToString();
		}

		/// <summary>
		/// mangle the string so that it doesn't contain spaces or control chars </summary>
		/// <param name="input"> </param>
		/// <returns> mangled string </returns>
		public static string mangle(string input)
		{
			StringBuilder @out = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				if (char.IsWhiteSpace(c))
				{
					@out.Append("_");
				}
				else if (c == '_')
				{
					@out.Append("__");
				}
				else if (char.IsControl(c))
				{
					@out.Append("_");
				}
				else
				{
					@out.Append(c);
				}
			}

			return @out.ToString();
		}

		public static string randomString(int maxLength, Random r)
		{
			StringBuilder builder = new StringBuilder(maxLength);
			for (int i = 0; i < maxLength; i++)
			{
				char c = (char)('a' + r.Next(26));
				builder.Append(c);
			}
			return builder.ToString();

		}

		/// <summary>
		/// determine if the given strings are equal in a null friendly fashion </summary>
		/// <param name="one"> </param>
		/// <param name="two"> </param>
		/// <returns> if the 2 strings are equal </returns>
		public static bool Equals(string one, string two)
		{
			if (one == null && two == null)
			{
				return true;
			}
			else if ((one == null && two != null) || (one != null && two == null))
			{
				return false;
			}
			else
			{
				return one.Equals(two);
			}
		}

		/// <summary>
		/// drain the given input stream into a StringBuffer, close the stream when done </summary>
		/// <param name="in"> the input stream </param>
		/// <returns> a StringBuilder </returns>
/*		public static StringBuilder drainStream(Reader @in)
		{
			return drainStream(@in, int.MaxValue);
		}
        
		/// <summary>
		/// drain the given input stream into a StringBuffer, close the stream when done </summary>
		/// <param name="in"> the input stream </param>
		/// <param name="the"> max length to drain </param>
		/// <returns> a StringBuilder </returns>
		public static StringBuilder drainStream(Reader @in, int maxLength)
		{
			StringBuilder buf = new StringBuilder();

			while (buf.Length < maxLength)
			{
				int c = @in.read();

				if (c == -1)
				{
					break;
				}
				else
				{
					buf.Append((char)c);
				}

			}

			IOUtils.close(@in);
			return buf;

		}
        */
	}
}