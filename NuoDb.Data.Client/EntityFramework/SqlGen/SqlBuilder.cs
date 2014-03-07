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

using System.Collections.Generic;
using System.Diagnostics;
using System;

#if EF6
namespace NuoDb.Data.Client.EntityFramework6.SqlGen
#else
namespace NuoDb.Data.Client.EntityFramework.SqlGen
#endif
{
    /// <summary>
    /// This class is like StringBuilder.  While traversing the tree for the first time, 
    /// we do not know all the strings that need to be appended e.g. things that need to be
    /// renamed, nested select statements etc.  So, we use a builder that can collect
    /// all kinds of sql fragments.
    /// </summary>
    internal class SqlBuilder : ISqlFragment
    {
        private List<object> _sqlFragments;

        private List<object> sqlFragments
        {
            get
            {
                if (null == _sqlFragments)
                {
                    _sqlFragments = new List<object>();
                }
                return _sqlFragments;
            }
        }

        /// <summary>
        /// Add an object to the list - we do not verify that it is a proper sql fragment
        /// since this is an internal method.
        /// </summary>
        /// <param name="s"></param>
        public void Append(object s)
        {
            Debug.Assert(s != null);
            sqlFragments.Add(s);
        }

        /// <summary>
        /// This is to pretty print the SQL.  The writer <see cref="SqlWriter.Write"/>
        /// needs to know about new lines so that it can add the right amount of 
        /// indentation at the beginning of lines.
        /// </summary>
        public void AppendLine()
        {
            sqlFragments.Add("\r\n");
        }

        /// <summary>
        /// Whether the builder is empty.  This is used by the <see cref="SqlGenerator.Visit(DbProjectExpression)"/>
        /// to determine whether a sql statement can be reused.
        /// </summary>
        public virtual bool IsEmpty
        {
            get { return ((null == _sqlFragments) || (0 == _sqlFragments.Count)); }
        }

        #region ISqlFragment Members

        /// <summary>
        /// We delegate the writing of the fragment to the appropriate type.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public virtual void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (null != _sqlFragments)
            {
                foreach (var o in _sqlFragments)
                {
                    var str = (o as String);
                    if (null != str)
                    {
                        writer.Write(str);
                    }
                    else
                    {
                        var sqlFragment = (o as ISqlFragment);
                        if (null != sqlFragment)
                        {
                            sqlFragment.WriteSql(writer, sqlGenerator);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

        #endregion
    }
}

#endif