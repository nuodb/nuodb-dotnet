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

namespace NuoDb.Data.Client
{
    public class NuoDbBulkLoaderColumnMapping
    {
        private int sourceOrdinal;
        private int destinationOrdinal;
        private string destinationColumn;
        private string sourceColumn;

        public int SourceOrdinal
        {
            get { return sourceOrdinal; }
        }

        public int DestinationOrdinal
        {
            get { return destinationOrdinal; }
        }

        public string DestinationColumn
        {
            get { return destinationColumn; }
        }

        public string SourceColumn
        {
            get { return sourceColumn; }
        }

        public NuoDbBulkLoaderColumnMapping(int source, int target)
        {
            if (source < 0)
                throw new ArgumentOutOfRangeException("source", "The column ordinal must be a non-negative number");
            if (target < 0)
                throw new ArgumentOutOfRangeException("target", "The column ordinal must be a non-negative number");
            this.sourceOrdinal = source;
            this.destinationOrdinal = target;
        }
        public NuoDbBulkLoaderColumnMapping(string source, int target)
        {
            if (target < 0)
                throw new ArgumentOutOfRangeException("target", "The column ordinal must be a non-negative number");
            this.sourceColumn = source;
            this.destinationOrdinal = target;
        }
        public NuoDbBulkLoaderColumnMapping(int source, string target)
        {
            if (source < 0)
                throw new ArgumentOutOfRangeException("source", "The column ordinal must be a non-negative number");
            this.sourceOrdinal = source;
            this.destinationColumn = target;
        }
        public NuoDbBulkLoaderColumnMapping(string source, string target)
        {
            this.sourceColumn = source;
            this.destinationColumn = target;
        }
    }
}
