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
namespace NuoDb.Data.Client.EntityFramework.SqlGen
{

    /// <summary>
    /// Represents the sql fragment for any node in the query tree.
    /// </summary>
    /// <remarks>
    /// The nodes in a query tree produce various kinds of sql
    /// <list type="bullet">
    /// <item>A select statement.</item>
    /// <item>A reference to an extent. (symbol)</item>
    /// <item>A raw string.</item>
    /// </list>
    /// We have this interface to allow for a common return type for the methods
    /// in the expression visitor <see cref="DbExpressionVisitor{TResultType}"/>
    /// 
    /// Add the endd of translation, the sql fragments are converted into real strings.
    /// </remarks>
    internal interface ISqlFragment
    {
        /// <summary>
        /// Write the string represented by this fragment into the stream.
        /// </summary>
        /// <param name="writer">The stream that collects the strings.</param>
        /// <param name="sqlGenerator">Context information used for renaming.
        /// The global lists are used to generated new names without collisions.</param>
        void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
    }
}
