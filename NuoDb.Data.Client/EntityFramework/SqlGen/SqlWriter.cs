/*
Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
Microsoft Open Technologies would like to thank its contributors, a list of whom
are at http://aspnetwebstack.codeplex.com/wikipage?title=Contributors.

Licensed under the Apache License, Version 2.0 (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions
and limitations under the License.
*/

#if !__MonoCS__

namespace NuoDb.Data.Client.EntityFramework.SqlGen
{
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// This extends StringWriter primarily to add the ability to add an indent
    /// to each line that is written out.
    /// </summary>
    internal class SqlWriter : StringWriter
    {
        // We start at -1, since the first select statement will increment it to 0.
        private int indent = -1;

        /// <summary>
        /// The number of tabs to be added at the beginning of each new line.
        /// </summary>
        internal int Indent
        {
            get { return indent; }
            set { indent = value; }
        }

        private bool atBeginningOfLine = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public SqlWriter(StringBuilder b)
            : base(b, CultureInfo.InvariantCulture)
            // I don't think the culture matters, but FxCop wants something
        {
        }

        /// <summary>
        /// Reset atBeginningofLine if we detect the newline string.
        /// <see cref="SqlBuilder.AppendLine"/>
        /// Add as many tabs as the value of indent if we are at the 
        /// beginning of a line.
        /// </summary>
        /// <param name="value"></param>
        public override void Write(string value)
        {
            if (value == "\r\n")
            {
                base.WriteLine();
                atBeginningOfLine = true;
            }
            else
            {
                if (atBeginningOfLine)
                {
                    if (indent > 0)
                    {
                        base.Write(new string('\t', indent));
                    }
                    atBeginningOfLine = false;
                }
                base.Write(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void WriteLine()
        {
            base.WriteLine();
            atBeginningOfLine = true;
        }
    }
}

#endif