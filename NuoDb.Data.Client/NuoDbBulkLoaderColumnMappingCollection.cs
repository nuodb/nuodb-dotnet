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

using System.Collections;

namespace NuoDb.Data.Client
{
    public class NuoDbBulkLoaderColumnMappingCollection : CollectionBase
    {
        public NuoDbBulkLoaderColumnMappingCollection()
            : base()
        {
        }
        public NuoDbBulkLoaderColumnMappingCollection(int capacity)
            : base(capacity)
        {
        }

        public NuoDbBulkLoaderColumnMapping this[int index]
        {
            get
            {
                return ((NuoDbBulkLoaderColumnMapping)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        public NuoDbBulkLoaderColumnMapping Add(NuoDbBulkLoaderColumnMapping mapping)
        {
            List.Add(mapping);
            return mapping;
        }
        public NuoDbBulkLoaderColumnMapping Add(int source, int target)
        {
            return Add(new NuoDbBulkLoaderColumnMapping(source, target));
        }
        public NuoDbBulkLoaderColumnMapping Add(string source, int target)
        {
            return Add(new NuoDbBulkLoaderColumnMapping(source, target));
        }
        public NuoDbBulkLoaderColumnMapping Add(int source, string target)
        {
            return Add(new NuoDbBulkLoaderColumnMapping(source, target));
        }
        public NuoDbBulkLoaderColumnMapping Add(string source, string target)
        {
            return Add(new NuoDbBulkLoaderColumnMapping(source, target));
        }
    }
}
