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
    using System.Diagnostics;
    using System.Data.Common.CommandTrees;

    /// <summary>
    /// The SymbolPair exists to solve the record flattening problem.
    /// <see cref="SqlGenerator.Visit(DbPropertyExpression)"/>
    /// Consider a property expression D(v, "j3.j2.j1.a.x")
    /// where v is a VarRef, j1, j2, j3 are joins, a is an extent and x is a columns.
    /// This has to be translated eventually into {j'}.{x'}
    /// 
    /// The source field represents the outermost SqlStatement representing a join
    /// expression (say j2) - this is always a Join symbol.
    /// 
    /// The column field keeps moving from one join symbol to the next, until it
    /// stops at a non-join symbol.
    /// 
    /// This is returned by <see cref="SqlGenerator.Visit(DbPropertyExpression)"/>,
    /// but never makes it into a SqlBuilder.
    /// </summary>
    internal class SymbolPair : ISqlFragment
    {
        public Symbol Source;
        public Symbol Column;

        public SymbolPair(Symbol source, Symbol column)
        {
            Source = source;
            Column = column;
        }

        #region ISqlFragment Members

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            // Symbol pair should never be part of a SqlBuilder.
            Debug.Assert(false);
        }

        #endregion
    }
}

#endif