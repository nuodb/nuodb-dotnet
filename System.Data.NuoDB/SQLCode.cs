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

namespace System.Data.NuoDB
{

	/// <summary>
	/// Maps Nuodb vendor error codes to default SQLState values.
	/// 
	/// These should be consistent with SQLException.h
	/// </summary>
	public class SQLCode
	{
		// modifications to this list must also update C++'s SQLException.h
		public static SQLCode[] values = {
                                             new SQLCode("SYNTAX_ERROR", -1, "42000"),
                                             new SQLCode("FEATURE_NOT_YET_IMPLEMENTED", -2, "0A000"),
                                             new SQLCode("BUG_CHECK", -3, "58000"),
                                             new SQLCode("COMPILE_ERROR", -4, "42000"),
                                             new SQLCode("RUNTIME_ERROR", -5, "58000"),
                                             new SQLCode("IO_ERROR", -6, "08000"),
                                             new SQLCode("NETWORK_ERROR", -7, "08000"),
                                             new SQLCode("CONVERSION_ERROR", -8, "22000"),
                                             new SQLCode("TRUNCATION_ERROR", -9, "22000"),
                                             new SQLCode("CONNECTION_ERROR", -10, "08000"),
                                             new SQLCode("DDL_ERROR", -11, "42000"),
                                             new SQLCode("APPLICATION_ERROR", -12, "58000"),
                                             new SQLCode("SECURITY_ERROR", -13, "58000"),
                                             new SQLCode("DATABASE_CORRUPTION", -14, "58000"),
                                             new SQLCode("VERSION_ERROR", -15, "58000"),
                                             new SQLCode("LICENSE_ERROR", -16, "58000"),
                                             new SQLCode("INTERNAL_ERROR", -17, "58000"),
                                             new SQLCode("DEBUG_ERROR", -18, "58000"),
                                             new SQLCode("LOST_BLOB", -19, "22000"),
                                             new SQLCode("INCONSISTENT_BLOB", -20, "22000"),
                                             new SQLCode("DELETED_BLOB", -21, "22000"),
                                             new SQLCode("LOG_ERROR", -22, "58000"),
                                             new SQLCode("DATABASE_DAMAGED", -23, "58000"),
                                             new SQLCode("UPDATE_CONFLICT", -24, "40002"),
                                             new SQLCode("NO_SUCH_TABLE", -25, "42000"),
                                             new SQLCode("INDEX_OVERFLOW", -26, "58000"),
                                             new SQLCode("UNIQUE_DUPLICATE", -27, "23000"),
                                             new SQLCode("UNCOMMITTED_UPDATES", -28, "58000"),
                                             new SQLCode("DEADLOCK", -29, "40001"),
                                             new SQLCode("OUT_OF_MEMORY_ERROR", -30, "58000"),
                                             new SQLCode("OUT_OF_RECORD_MEMORY_ERROR", -31, "58000"),
                                             new SQLCode("LOCK_TIMEOUT", -32, "58000"),
                                             new SQLCode("PLATFORM_ERROR", -36, "58000"),
                                             new SQLCode("NO_SCHEMA", -37, "58000"),
                                             new SQLCode("CONFIGURATION_ERROR", -38, "58000"),
                                             new SQLCode("READ_ONLY_ERROR", -39, "58000"),
                                             new SQLCode("NO_GENERATED_KEYS", -40, "58000"),
                                             new SQLCode("THROWN_EXCEPTION", -41, "58000"),
                                             new SQLCode("INVALID_TRANSACTION_ISOLATION", -42,"01000"),
                                             new SQLCode("UNSUPPORTED_TRANSACTION_ISOLATION", -43, "0A000"),
                                             new SQLCode("INVALID_UTF8", -44,"58000"),
                                             new SQLCode("CONSTRAINT_ERROR", -45,"23001"), // Must start with "23" for Hibernate
                                             new SQLCode("UPDATE_ERROR", -46, "58000"), // update error catch all
                                             new SQLCode("I18N_ERROR", -47, "58000"),
                                             new SQLCode("OPERATION_KILLED", -48, "HY008"),
                                             new SQLCode("INVALID_STATEMENT", -49, "58000"),
                                             new SQLCode("TRANSACT_ERROR", -50, "58000"),
                                             new SQLCode("JAVA_ERROR", -51,"58000")
                                         };
		// modifications to this list must also update C++'s SQLException.h

        private string name;
		private int code;
		private String sqlState;

		private SQLCode(string name, int code, String sqlState)
		{
            this.name = name;
			this.code = code;
			this.sqlState = sqlState;
		}

		public static SQLCode findCode(int code)
		{
            for (int i = 0; i < values.Length; i++ )
            {
                if (values[i].Code == code)
                    return values[i];
            }
	
			return null;
		}

		public static String findSQLState(int code)
		{
			SQLCode theCode = findCode(code);
			if(theCode == null)
				return null;
			else
				return theCode.SQLState;
		}

        public int Code
        {
            get { return code; }
        }

        public string SQLState
        {
            get { return sqlState; }
        }
	}

}