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
using System;

#if EF6
using System.Data.Entity.Core.Metadata.Edm;

namespace NuoDb.Data.Client.EntityFramework6.SqlGen
#else
using System.Data.Metadata.Edm;

namespace NuoDb.Data.Client.EntityFramework.SqlGen
#endif
{
    /// <summary>
    /// A Join symbol is a special kind of Symbol.
    /// It has to carry additional information
    /// <list type="bullet">
    /// <item>ColumnList for the list of columns in the select clause if this
    /// symbol represents a sql select statement.  This is set by <see cref="SqlGenerator.AddDefaultColumns"/>. </item>
    /// <item>ExtentList is the list of extents in the select clause.</item>
    /// <item>FlattenedExtentList - if the Join has multiple extents flattened at the 
    /// top level, we need this information to ensure that extent aliases are renamed
    /// correctly in <see cref="SqlSelectStatement.WriteSql"/></item>
    /// <item>NameToExtent has all the extents in ExtentList as a dictionary.
    /// This is used by <see cref="SqlGenerator.Visit(DbPropertyExpression)"/> to flatten
    /// record accesses.</item>
    /// <item>IsNestedJoin - is used to determine whether a JoinSymbol is an 
    /// ordinary join symbol, or one that has a corresponding SqlSelectStatement.</item>
    /// </list>
    /// 
    /// All the lists are set exactly once, and then used for lookups/enumerated.
    /// </summary>
    internal sealed class JoinSymbol : Symbol
    {
        private List<Symbol> columnList;

        internal List<Symbol> ColumnList
        {
            get
            {
                if (null == columnList)
                {
                    columnList = new List<Symbol>();
                }
                return columnList;
            }
            set { columnList = value; }
        }

        private readonly List<Symbol> extentList;

        internal List<Symbol> ExtentList
        {
            get { return extentList; }
        }

        private List<Symbol> flattenedExtentList;

        internal List<Symbol> FlattenedExtentList
        {
            get
            {
                if (null == flattenedExtentList)
                {
                    flattenedExtentList = new List<Symbol>();
                }
                return flattenedExtentList;
            }
            set { flattenedExtentList = value; }
        }

        private readonly Dictionary<string, Symbol> nameToExtent;

        internal Dictionary<string, Symbol> NameToExtent
        {
            get { return nameToExtent; }
        }

        internal bool IsNestedJoin { get; set; }

        public JoinSymbol(string name, TypeUsage type, List<Symbol> extents)
            : base(name, type)
        {
            extentList = new List<Symbol>(extents.Count);
            nameToExtent = new Dictionary<string, Symbol>(extents.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var symbol in extents)
            {
                nameToExtent[symbol.Name] = symbol;
                ExtentList.Add(symbol);
            }
        }
    }
}

#endif