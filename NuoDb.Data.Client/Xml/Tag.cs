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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NuoDb.Data.Client.Util;

namespace NuoDb.Data.Client.Xml
{

	//
	//
	// Tag
	//
	//

	class Tag : IEnumerable<Tag>
	{
		internal string name;
		internal Tag children_Renamed;
		internal Attribute attributes;
		internal Tag sibling;
		internal string innerText;
		internal string outerText;
		internal bool cdata;

		private const string FALSE = "false";
		private const string TRUE = "true";

		internal static string[] escapes = new string[256];

		static Tag()
		{
			escapes['&'] = "&amp;";
			escapes['\''] = "&apos;";
			escapes['"'] = "&quot;";
			escapes['<'] = "&lt;";
			escapes['>'] = "&gt;";
		}

		internal class Children : IEnumerator<Tag>
		{
			private readonly Tag outerInstance;

			internal Tag firstSibling;
            internal Tag currentSibling;

			public Children(Tag outerInstance, Tag tag)
			{
				this.outerInstance = outerInstance;
				this.firstSibling = tag.children_Renamed;
                this.currentSibling = null;
			}

            #region IEnumerator<Tag> Members

            public Tag Current
            {
                get { return currentSibling; }
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
                get { return currentSibling; }
            }

            public bool MoveNext()
            {
                if (currentSibling == null)
                    currentSibling = firstSibling;
                else
                    currentSibling = currentSibling.sibling;
                return currentSibling != null;
            }

            public void Reset()
            {
                currentSibling = null;
            }

            #endregion
        }

		internal class Attr : IEnumerator<Attribute>
		{
			private readonly Tag outerInstance;

			internal Attribute firstAttribute;
            internal Attribute currentAttribute;

			public Attr(Tag outerInstance, Tag tag)
			{
				this.outerInstance = outerInstance;
				this.firstAttribute = tag.attributes;
                this.currentAttribute = null;
			}

            #region IEnumerator<Attribute> Members

            public Attribute Current
            {
                get { return currentAttribute; }
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
                get { return currentAttribute; }
            }

            public bool MoveNext()
            {
                if (currentAttribute == null)
                    currentAttribute = firstAttribute;
                else
                    currentAttribute = currentAttribute.sibling;
                return currentAttribute != null;
            }

            public void Reset()
            {
                currentAttribute = null;
            }

            #endregion
        }

		public Tag()
		{
		}

		public Tag(string newName)
		{
			name = newName;
		}

		public virtual void parse(string tag)
		{
			//System.out.printf("Tag.parse, msg length %d\n", tag.length());
			//System.out.println(tag);
			Doc doc = new Doc(tag);
			doc.skipWhite();

			// Make sure this is actually a tag

			if (!doc.match('<'))
			{
				throw new XmlException("expected xml tag, got " + doc.Tail);
			}

			name = doc.Token;

			if (name.Equals("?xml"))
			{
				parseAttributes(doc);
				doc.Declaration = attributes;
				attributes = null;
			}
			else
			{
				if (!parseAttributes(doc))
				{
					parseChildren(doc);
				}
			}
		}

		public virtual void parse(Doc doc)
		{
			//doc.getToken();
			doc.skipWhite();

			// Make sure this is actually a tag

			if (!doc.match('<'))
			{
				throw new XmlException("expected xml tag, got: " + doc.Tail);
			}

			name = doc.Token;

			if (!parseAttributes(doc))
			{
				parseChildren(doc);
			}

			outerText = doc.parseText();
		}

		public virtual bool isNamed(string @string)
		{
			return name.Equals(@string);
		}

		public Tag(string tag, Doc doc)
		{
			name = tag;

			if (!parseAttributes(doc))
			{
				parseChildren(doc);
			}

			outerText = doc.parseText();
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

		public virtual string Text
		{
			get
			{
				return innerText;
			}
			set
			{
				innerText = value;
			}
		}


		public virtual string CData
		{
			set
			{
				//Debug.println("setCData: " + value);
				innerText = value;
				cdata = true;
			}
		}

		public virtual Tag addChild(string name)
		{
			Tag child = new Tag(name);

			return addChild(child);
		}

		public virtual Tag addChild(Tag tag)
		{
			tag.sibling = null;

			if (children_Renamed == null)
			{
				children_Renamed = tag;
			}
			else
			{
				for (Tag child = children_Renamed;; child = child.sibling)
				{
					if (child.sibling == null)
					{
						child.sibling = tag;

						break;
					}
				}
			}

			return tag;
		}

		public virtual Tag insert(string name, string attribute, int sequence)
		{
			Tag child = new Tag(name);
			child.addAttribute(attribute, sequence);
			insert(child, attribute, sequence);

			return child;
		}

		public virtual void insert(Tag tag, string attribute, int sequence)
		{
			Tag prior = null;

			for (Tag child = children_Renamed; child != null; prior = child, child = child.sibling)
			{
				if (sequence < child.getIntAttribute(attribute, 1000000))
				{
					break;
				}
			}

			if (prior == null)
			{
				tag.sibling = children_Renamed;
				children_Renamed = tag;
			}
			else
			{
				tag.sibling = prior.sibling;
				prior.sibling = tag;
			}
		}

		public virtual bool deleteItem(Tag item)
		{
			Tag prior = null;

			for (Tag child = children_Renamed; child != null; prior = child, child = child.sibling)
			{
				if (child == item)
				{
					if (prior == null)
					{
						children_Renamed = item.sibling;
					}
					else
					{
						prior.sibling = item.sibling;
					}

					return true;
				}
			}

			return false;
		}

		public virtual void addAttribute(string name, int value)
		{
			addAttribute(name, Convert.ToString(value));
		}

		public virtual void addAttribute(string name, bool value)
		{
			addAttribute(name, (value) ? TRUE : FALSE);
		}

		public virtual void addAttribute(string name, long value)
		{
			addAttribute(name, Convert.ToString(value));
		}

		public virtual void addAttribute(string name, string value)
		{
			addAttribute(new Attribute(name, value));
		}

		public virtual void addAttribute(string name, Tag source)
		{
			addAttribute(new Attribute(name, source.getAttribute(name)));
		}

		public virtual void addAttribute(Attribute attribute)
		{
			attribute.sibling = null;

			if (attributes == null)
			{
				attributes = attribute;
			}
			else
			{
				for (Attribute attr = attributes;; attr = attr.sibling)
				{
					if (attr.sibling == null)
					{
						attr.sibling = attribute;
						break;
					}
				}
			}
		}

		public virtual void setAttribute(string name, int value)
		{
			setAttribute(name, Convert.ToString(value));
		}

		public virtual void setAttribute(string name, bool value)
		{
			setAttribute(name, (value) ? TRUE : FALSE);
		}

		public virtual void setAttribute(string name, long value)
		{
			setAttribute(name, Convert.ToString(value));
		}

		public virtual void setAttribute(string name, Tag source)
		{
			setAttribute(name, source.getAttribute(name));
		}

		public virtual void setAttribute(string name, string value)
		{

			if (attributes == null)
			{
				attributes = new Attribute(name, value);
				attributes.sibling = null;
			}
			else
			{
				for (Attribute attr = attributes;; attr = attr.sibling)
				{

					if (attr.Name.Equals(name))
					{
						attr.Value = value;
						break;
					}

					if (attr.sibling == null)
					{
						attr.sibling = new Attribute(name, value);
						attr.sibling.sibling = null;
						break;
					}
				}
			}
		}

		public virtual bool parseAttributes(Doc doc)
		{
			Attribute attr = null;
			bool empty = false;

			for (;;)
			{
				doc.skipWhite();

				if (doc.match('/'))
				{
					empty = true;

					continue;
				}

				if (doc.match('>'))
				{
					break;
				}

				Attribute attribute = new Attribute(doc);

				if (attr == null)
				{
					attributes = attribute;
				}
				else
				{
					attr.sibling = attribute;
				}

				attr = attribute;
			}

			return empty;
		}

		public virtual void parseChildren(Doc doc)
		{
			innerText = doc.parseText();
			Tag child = null;

			for (;;)
			{
				Tag item;

				/// <summary>
				///*
				/// if (doc.checkComment())
				/// continue;
				/// **
				/// </summary>

				if (!doc.match('<'))
				{
					doc.print("Tag.parseChildren: ", 30);
					throw new XmlException("invalid xml document -- malformed");
				}

				string @string = doc.Token;

				if (@string.Length == 0)
				{
					//TODO Line numbers in the exception would be good here!
					throw new XmlException("invalid xml document - malformed \"<" + doc.parseText() + "\"");
				}

				if (@string[0] == '/')
				{
					if (!doc.match('>'))
					{
						throw new XmlException("invalid xml ending tag");
					}

					int l = name.Length;

					if (@string.Length != l + 1)
					{
						throw new XmlException("invalid xml document -- unmatched tag \"" + name + "\"");
					}

					for (int n = 0; n < l; ++n)
					{
						if (name[n] != @string[n + 1])
						{
							throw new XmlException("invalid xml document -- unmatched tag \"" + name + "\"");
						}
					}

					return;
				}

				item = new Tag(@string, doc);

				if (child == null)
				{
					children_Renamed = item;
				}
				else
				{
					child.sibling = item;
				}

				child = item;
			}
		}

		public virtual bool getTag()
		{
			return true;
		}

		public virtual void print()
		{
			prettyPrint(0);
			Console.WriteLine();
		}
		public virtual void prettyPrint(int column)
		{
			space(column);
			Console.Write("<" + name);

			for (Attribute attribute = attributes; attribute != null; attribute = attribute.sibling)
			{
				attribute.print();
			}

			if (children_Renamed == null)
			{
				Console.WriteLine("/>");
			}
			else
			{
				Console.WriteLine(">");
				++column;

				for (Tag item = children_Renamed; item != null; item = item.sibling)
				{
					item.prettyPrint(column);
				}

				--column;
				space(column);
				Console.WriteLine("</" + name + ">");
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuffer = new StringBuilder();
			gen(0, stringBuffer);

			return stringBuffer.ToString();
		}

		public virtual void gen(int level, StringBuilder buffer)
		{
			int start = buffer.Length;
			buffer.Append("<");
			buffer.Append(name);
			int attrIndent = level * 2 + 1 + buffer.Length - start;
			int attrLimit = buffer.Length + 60;

			for (Attribute attribute = attributes; attribute != null; attribute = attribute.sibling)
			{
				int len = buffer.Length;

				if (len < attrLimit)
				{
					buffer.Append(" ");
				}
				else
				{
					buffer.AppendLine();
					indent(attrIndent, buffer);
					attrLimit = len + 60;
				}

				buffer.Append(attribute.Name);
				buffer.Append("=\"");
				putQuotedText(attribute.Value, buffer);
				buffer.Append("\"");
			}

			if (innerText != null)
			{
				buffer.Append(">");

				if (cdata)
				{
					buffer.Append("<![CDATA[");
					buffer.Append(innerText);
					buffer.Append("]]>");
				}
				else
				{
					putQuotedText(innerText, buffer);
				}
			}
			else if (children_Renamed == null)
			{
				buffer.Append("/>\n");

				return;
			}
			else
			{
				buffer.Append(">\n");
			}

			++level;

			for (Tag child = children_Renamed; child != null; child = child.sibling)
			{
				space(level, buffer);
				child.gen(level, buffer);
			}

			if (children_Renamed != null || innerText == null)
			{
				space(level - 1, buffer);
			}

			buffer.Append("</");
			buffer.Append(name);
			buffer.Append(">\n");
		}

		public virtual void putQuotedText(string text, StringBuilder buffer)
		{
			int length = text.Length;

			for (int n = 0; n < length; ++n)
			{
				char c = text[n];

				if (c < escapes.Length && escapes[c] != null)
				{
					buffer.Append(escapes[c]);
				}
				else
				{
					buffer.Append(c);
				}
			}
		}

		public virtual void space(int level, StringBuilder buffer)
		{
			for (int n = 0; n < level; ++n)
			{
				buffer.Append("  ");
			}
		}

		public virtual void indent(int spaces, StringBuilder buffer)
		{
			for (int n = 0; n < spaces; ++n)
			{
				buffer.Append(" ");
			}
		}

		public virtual IEnumerator<Tag> children()
		{
			return new Children(this, this);
		}

		public virtual IEnumerator<Attribute> Attributes
		{
			get
			{
				return new Attr(this, this);
			}
		}

		public virtual string getAttribute(string name)
		{
			for (Attribute attribute = attributes; attribute != null; attribute = attribute.sibling)
			{
				if (attribute.name.Equals(name))
				{
					return attribute.value;
				}
			}

			throw new XmlException("can't find attribute \"" + name + "\"");
		}

		public virtual string getAttribute(string name, string defaultValue)
		{
			for (Attribute attribute = attributes; attribute != null; attribute = attribute.sibling)
			{
				if (attribute.name.Equals(name))
				{
					return attribute.value;
				}
			}

			return defaultValue;
		}

		public virtual int getIntAttribute(string name, int defaultValue)
		{
			string value = getAttribute(name, null);

			if (value == null)
			{
				return defaultValue;
			}

			return Op.getInt(value);
		}

		public virtual int getIntAttribute(string name)
		{
			string value = getAttribute(name, null);

			if (value == null)
			{
				throw new XmlException("can't find attribute \"" + name + "\"");
			}

			return Op.getInt(value);
		}

		public virtual bool getBooleanAttribute(string name, bool defaultValue)
		{
			string value = getAttribute(name, null);

			if (value == null)
			{
				return defaultValue;
			}

			// NOTE: Java encoding uses "true" while C++ uses "1", so start by
			// comparing the expected Java value and then check the C++ format
			return value.Equals(TRUE) || value.Trim().Equals("1");
		}

		public virtual bool getBooleanAttribute(string name)
		{
			string value = getAttribute(name, null);

			if (value == null)
			{
				throw new XmlException("can't find attribute \"" + name + "\"");
			}

			// see note above
			return value.Equals(TRUE) || value.Trim().Equals("1");
		}

		public virtual long getLongAttribute(string name, long defaultValue)
		{
			string value = getAttribute(name, null);

			if (value == null)
			{
				return defaultValue;
			}

			return Op.getLong(value);
		}

		public virtual long getLongAttribute(string name)
		{
			string value = getAttribute(name, null);

			if (value == null)
			{
				throw new XmlException("can't find attribute \"" + name + "\"");
			}

			return Op.getLong(value);
		}

		public virtual string Value
		{
			get
			{
				if (children_Renamed != null && children_Renamed.sibling == null)
				{
					return children_Renamed.Value;
				}
    
				return null;
			}
		}

		public virtual Tag getChild(string name)
		{
			Tag child = findChild(name);

			if (child != null)
			{
				return child;
			}

			throw new XmlException("can't find \"" + name + "\" in XML tag");
		}

		public virtual Tag findChild(string attribute, string value)
		{
			for (Tag item = children_Renamed; item != null; item = item.sibling)
			{
				if (item.getAttribute(attribute, "").Equals(value))
				{
					return item;
				}
			}

			return null;
		}

		public virtual Tag findChild(string name)
		{
			for (Tag item = children_Renamed; item != null; item = item.sibling)
			{
				if (item.getTag())
				{

					if (item.name.Equals(name))
					{
						return item;
					}
				}
			}

			return null;
		}

		public virtual void space(int column)
		{
			for (int n = 0; n < column; ++n)
			{
				Console.Write("  ");
			}
		}

		public virtual bool hasAttributes()
		{
			return attributes != null;
		}

		public virtual bool hasChildren()
		{
			return children_Renamed != null;
		}

		/// <summary>
		/// The number of attributes on this particular Tag element.
		/// </summary>
		/// <returns>  A positive number representing the number of attributes on the tag
		///          or zero if there are no children </returns>
		public virtual int AttributeCount
		{
			get
			{
				int i = 0;
    
				for (IEnumerator<Attribute> iA = Attributes; iA.MoveNext(); )
				{
					i++;
				}
    
				return i;
			}
		}

		/// <summary>
		/// The number of child tags contained within this particular Tag element.
		/// </summary>
		/// <returns>  A positive number representing the number of child tags under
		///          the tag - zero if there are no children </returns>
		public virtual int ChildCount
		{
			get
			{
				int i = 0;
    
				for (IEnumerator<Tag> iT = children(); iT.MoveNext(); )
				{
					i++;
				}
    
				return i;
			}
		}

		public virtual void print(StringBuilder stringBuffer)
		{
			print(0, stringBuffer);
		}

		public virtual void print(int level, StringBuilder stringBuffer)
		{
			for (int n = 0; n < level; ++n)
			{
				stringBuffer.Append("   ");
			}

			stringBuffer.Append(name);
			string sep = " ";

			for (Attribute attribute = attributes; attribute != null; attribute = attribute.sibling)
			{
				stringBuffer.Append(sep);
				stringBuffer.Append(attribute.name);
				stringBuffer.Append("=");
				stringBuffer.Append(attribute.value);
				sep = ", ";
			}

			stringBuffer.AppendLine();

			for (Tag child = children_Renamed; child != null; child = child.sibling)
			{
				   child.print(level + 1, stringBuffer);
			}
		}

        #region IEnumerable<Tag> Members

        public IEnumerator<Tag> GetEnumerator()
        {
            return new Children(this, this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Children(this, this);
        }

        #endregion
    }

}