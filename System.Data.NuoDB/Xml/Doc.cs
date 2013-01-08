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
using System.Text;
using System.IO;
using System.Xml;

namespace System.Data.NuoDB.Xml
{

	//
	//
	// Doc
	//
	//
	public class Doc
	{
		internal StringBuilder document;
		internal int offset;
		internal int length;
		internal Attribute declaration;
		internal StreamReader inputStream;

		internal const string CDATA = "<![CDATA[";
		internal const string endCDATA = "]]>";
		internal const string COMMENT = "<!--";
		internal const string endCOMMENT = "-->";

		public Doc(string doc)
		{
			//document = doc;
			document = new StringBuilder(doc);
			offset = 0;
			length = document.Length;
		}

		public Doc(StreamReader stream)
		{
			document = new StringBuilder();
			offset = 0;
			length = document.Length;
			inputStream = stream;
			getLine();
		}

		public virtual void print(string text, int len)
		{
			print(text, offset, len);
		}

		public virtual void print(string text, int pos, int len)
		{
			int end = Math.Min(pos + len, length);
			Console.WriteLine(text + document.ToString(pos, end - pos));
		}

		public virtual Attribute Declaration
		{
			set
			{
				declaration = value;
			}
		}

		public virtual char Char
		{
			get
			{
				if (offset >= length)
				{
					throw new XmlException("unexpect end of xml document");
				}
    
				return document[offset];
			}
		}

		public virtual bool match(char c)
		{
			if (offset < length && document[offset] == c)
			{
				++offset;
				return true;
			}

			return false;
		}

		public virtual bool isChar(char c)
		{
			return offset < length && document[offset] == c;
		}

		public virtual string parseText()
		{
			bool escape = false;
			bool cdata = false;
			bool escapes = false;

			for (;;)
			{
				for (int n = offset; n < length; ++n)
				{
					char c = document[n];

					if (escape)
					{
						if (c == ';')
						{
							escape = false;
						}
					}
					else if (cdata)
					{
						if (c == ']' && checkString(n, endCDATA))
						{
							n += endCDATA.Length - 1;
							cdata = false;
						}
					}
					else if (c == '<')
					{
						if (checkString(n, COMMENT))
						{
							escapes = true;
							n = commentEnd(n + COMMENT.Length) - 1;
						}
						else if (checkString(n, CDATA))
						{
							cdata = true;
							escapes = true;
						}
						else
						{
							string @string = (escapes) ? processEscapes(offset, n) : document.ToString(offset, n - offset);
							offset = n;

							return @string;
						}
					}
					else if (c == '&')
					{
						escape = true;
						escapes = true;
					}
				}

				if (!getLine())
				{
					break;
				}
			}

			return (escapes) ? processEscapes(offset, length) : document.ToString(offset, length - offset);
		}

		public virtual string processEscapes(int from, int to)
		{
			StringBuilder builder = new StringBuilder(to - from);

			for (int n = from; n < to; ++n)
			{
				char c = document[n];

				if (c == '&')
				{
					if (checkString(n, "&lt;"))
					{
						builder.Append('<');
					}
					else if (checkString(n, "&gt;"))
					{
						builder.Append('>');
					}
					else if (checkString(n, "&quote;"))
					{
						builder.Append('"');
					}
					else if (checkString(n, "&quot;"))
					{
						builder.Append('"');
					}
					else if (checkString(n, "&apos;"))
					{
						builder.Append('\'');
					}
					else if (checkString(n, "&amp;"))
					{
						builder.Append('&');
					}
					else if (checkString(n, "&nbsp;"))
					{
						builder.Append(' ');
					}
					else
					{
						print("Bad excape: ", n, 50);
						throw new XmlException("invalid escape seqeuence");
					}
					while (document[n] != ';')
					{
						++n;
					}
				}
				else if (c == '<')
				{
					if (checkString(n, CDATA))
					{
						n += CDATA.Length;
						//int start = n;
						for (; !((c = document[n]) == ']' && checkString(n, endCDATA)); ++n)
						{
							builder.Append(c);
						}
						//System.out.println ("CDATA: " + document.substring(start, n));
						n += endCDATA.Length - 1;
					}
					else if (checkString(n, COMMENT))
					{
						n += commentEnd(n + COMMENT.Length) - 1;
					}
				}
				else
				{
					builder.Append(c);
				}
			}

			return builder.ToString();
		}

		public virtual void skipWhite()
		{
			for (bool comment = false; offset < length; ++offset)
			{
				char c = document[offset];

				if (comment)
				{
					if (c == '-' && checkString(offset, "-->"))
					{
						comment = false;
						offset += 2;
					}
				}
				else
				{
					if (c == '<' && checkString(offset, "<!--"))
					{
						//Debug.println ("skipWhite: found comment");
						comment = true;
						offset += 3;
					}

					if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
					{
						break;
					}
				}
			}
		}

		public virtual bool checkComment()
		{
			if (!checkString(offset, "<!--"))
			{
				return false;
			}

			offset += 4;
			skipComment();

			return true;
		}

		public virtual int commentEnd(int start)
		{
			for (int n = start; n < length; ++n)
			{
				if (document[n] == '-' && checkString(n, endCOMMENT))
				{
					return n + endCOMMENT.Length;
				}
			}

			return length;
		}

		public virtual void skipComment()
		{
			for (; offset < length; ++offset)
			{
				char c = document[offset];
				if (c == '-' && checkString(offset, "-->"))
				{
					offset += 2;
					break;
				}
			}
		}

		public virtual bool checkString(int offset, string @string)
		{
			int l = @string.Length;

			if (offset + l > length)
			{
				return false;
			}

			for (int n = 0; n < l; ++n)
			{
				if (document[offset + n] != @string[n])
				{
					return false;
				}
			}

			return true;
		}

		public virtual string Token
		{
			get
			{
				while (offset >= length && inputStream != null)
				{
					getLine();
				}
    
				char quote = Char;
				bool hasEscape = false;
    
				for (;;)
				{
					if (quote == '"' || quote == '\'')
					{
						for (int n = ++offset; n < length; ++n)
						{
							char c = document[n];
    
							if (c == quote)
							{
								string token = (hasEscape) ? getString(offset, n) : document.ToString(offset, n - offset);
								offset = n + 1;
    
								return token;
							}
							else if (c == '&')
							{
								hasEscape = true;
							}
						}
					}
					else
					{
						for (int n = offset; n < length; ++n)
						{
							char c = document[n];
    
							if ((n > offset && c == '/') || (c == '=' || c == '>' || c == ' ' || c == '\t' || c == '\n' || c == '\r'))
							{
								string token = document.ToString(offset, n - offset);
								offset = n;
    
								return token;
							}
						}
					}
    
					if (!getLine())
					{
						break;
					}
				}
    
				throw new XmlException("unterminated xml token");
			}
		}

		private bool getLine()
		{
			if (inputStream == null)
			{
				return false;
			}
    
			try
			{
				string line = inputStream.ReadLine();
    
				if (line == null)
				{
					return false;
				}
    
				document.Append(line);
				length = document.Length;
			}
			catch (IOException)
			{
				inputStream = null;
    
				return false;
			}
    
			return true;
		}

		public virtual string Tail
		{
            get
            {
			    if (length >= offset)
			    {
				    return "<end of string>";
			    }
    
			    return document.ToString(offset, document.Length - offset);
            }
		}

		internal virtual bool StartsWith(string substring, int offset)
		{
            return document.ToString(offset, substring.Length).Equals(substring);
		}

		public virtual string getString(int offset, int end)
		{
			StringBuilder builder = new StringBuilder();
			bool skip = false;

			for (int n = offset; n < end;)
			{
				char c = document[n++];

				if (c == '&')
				{
					if (StartsWith("amp;", n))
					{
						builder.Append('&');
					}
					else if (StartsWith("apos;", n))
					{
						builder.Append('\'');
					}
					else if (StartsWith("quot;", n))
					{
						builder.Append('"');
					}
					else if (StartsWith("lt;", n))
					{
						builder.Append('<');
					}
					else if (StartsWith("gt;", n))
					{
						builder.Append('>');
					}
					else
					{
						throw new XmlException("bad xml escape sequence");
					}

					skip = true;
				}
				else if (!skip)
				{
					builder.Append(c);
				}
				else if (c == ';')
				{
					skip = false;
				}
			}

			return builder.ToString();
		}
	}


}