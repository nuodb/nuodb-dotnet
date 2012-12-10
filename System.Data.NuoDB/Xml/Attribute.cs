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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace System.Data.NuoDB.Xml
{


	//
	//
	// Attribute
	//
	//

	public class Attribute
	{
		internal Attribute sibling;
		internal string name;
		internal string value;

		internal class Attr : IEnumerator<Attribute>
		{
			private readonly Attribute outerInstance;

			internal Attribute firstAttribute;
            internal Attribute currentAttribute;

			public Attr(Attribute outerInstance, Tag tag)
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

            object Collections.IEnumerator.Current
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


		public Attribute(string attributeName, string attributeValue)
		{
			name = attributeName;
			value = attributeValue;
		}

		public Attribute(Attribute attribute)
		{
			name = attribute.name;
			value = attribute.value;
		}

		public Attribute(Doc doc)
		{
			parse(doc);
		}

		public virtual void parse(Doc doc)
		{
			name = doc.Token;

			if (doc.match('='))
			{
				value = doc.Token;
			}
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

		public virtual string Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}


		public virtual void print()
		{
			Console.Write(" " + name + "=\"" + value + "\"");
		}

		public virtual void gen(StringBuilder buffer)
		{
			buffer.Append(name);
			buffer.Append("=\"");
			buffer.Append(value);
			buffer.Append("\"");
		}

	}


}