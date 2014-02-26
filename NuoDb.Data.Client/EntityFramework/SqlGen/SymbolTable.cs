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

#if EF6
namespace EntityFramework.NuoDb.SqlGen
#else
namespace NuoDb.Data.Client.EntityFramework.SqlGen
#endif
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// The symbol table is quite primitive - it is a stack with a new entry for
    /// each scope.  Lookups search from the top of the stack to the bottom, until
    /// an entry is found.
    /// 
    /// The symbols are of the following kinds
    /// <list type="bullet">
    /// <item><see cref="Symbol"/> represents tables (extents/nested selects/unnests)</item>
    /// <item><see cref="JoinSymbol"/> represents Join nodes</item>
    /// <item><see cref="Symbol"/> columns.</item>
    /// </list>
    /// 
    /// Symbols represent names <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)"/> to be resolved, 
    /// or things to be renamed.
    /// </summary>
    internal sealed class SymbolTable
    {
        private readonly List<Dictionary<string, Symbol>> symbols = new List<Dictionary<string, Symbol>>();

        internal void EnterScope()
        {
            symbols.Add(new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase));
        }

        internal void ExitScope()
        {
            symbols.RemoveAt(symbols.Count - 1);
        }

        internal void Add(string name, Symbol value)
        {
            symbols[symbols.Count - 1][name] = value;
        }

        internal Symbol Lookup(string name)
        {
            for (var i = symbols.Count - 1; i >= 0; --i)
            {
                if (symbols[i].ContainsKey(name))
                {
                    return symbols[i][name];
                }
            }

            return null;
        }
    }
}

#endif