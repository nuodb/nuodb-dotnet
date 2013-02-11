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

using System.Data.Common;

namespace System.Data.NuoDB
{
    public class NuoDbParameter : DbParameter, ICloneable
    {
        private string name;
        private object value;
        private ParameterDirection direction;
        private DbType dbType;
        private int size;
        private string sourceColumn;
        private bool sourceColumnNullMapping;
        private bool isNullable;
        private DataRowVersion sourceVersion = DataRowVersion.Default;

        public override DbType DbType
        {
            get
            {
                return dbType;
            }
            set
            {
                dbType = value;
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return isNullable;
            }
            set
            {
                isNullable = value;
            }
        }

        public override string ParameterName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public override void ResetDbType()
        {
            dbType = DbType.Object;
        }

        public override int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                return sourceColumn;
            }
            set
            {
                sourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                return sourceColumnNullMapping;
            }
            set
            {
                sourceColumnNullMapping = value;
            }
        }

        public override DataRowVersion SourceVersion
        {
            get
            {
                return sourceVersion;
            }
            set
            {
                sourceVersion = value;
            }
        }

        public override object Value
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

        #region ICloneable Members

        public object Clone()
        {
            NuoDbParameter param = new NuoDbParameter();

            param.DbType = this.DbType;
            param.Direction = this.Direction;
            param.IsNullable = this.IsNullable;
            param.ParameterName = this.ParameterName;
            param.Size = this.Size;
            param.SourceColumn = this.SourceColumn;
            param.SourceColumnNullMapping = this.SourceColumnNullMapping;
            param.SourceVersion = this.SourceVersion;
            param.Value = this.Value;

            return param;
        }

        #endregion
    }
}
