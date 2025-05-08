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
using System;
using System.Data;

namespace NuoDb.Data.Client
{
    public class NuoDbParameter : DbParameter, ICloneable
    {
        private int? _size;
        private object? _value;
        public NuoDbParameter()
        {

        }

        public override DbType DbType { get; set; }

        public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        

        public override bool IsNullable { get; set; }
      
        public override string ParameterName { get; set; }
        
        public override void ResetDbType()
        {
            DbType = DbType.Object;
        }

        /// <summary>
        ///     Gets or sets the maximum size, in bytes, of the parameter.
        /// </summary>
        /// <value>The maximum size, in bytes, of the parameter.</value>
        public override int Size
        {
            get
                => _size
                   ?? (_value is string stringValue
                       ? stringValue.Length
                       : _value is byte[] byteArray
                           ? byteArray.Length
                           : 0);

            set
            {
                if (value < -1)
                {
                    // NB: Message is provided by the framework
                    throw new ArgumentOutOfRangeException(nameof(value), value, message: null);
                }

                _size = value;
            }
        }

        public override string SourceColumn { get; set; }
        public override bool SourceColumnNullMapping { get; set; }

        public override DataRowVersion SourceVersion { get; set; } = DataRowVersion.Default;

        /// <summary>
        ///     Gets or sets the value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        public override object? Value
        {
            get => _value;
            set { _value = value; }
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
