/****************************************************************************
* Copyright (c) 2012, NuoDB, Inc.
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

	//
	//
	// Protocol
	//
	//
	public abstract class Protocol
	{
		internal const int PROTOCOL_VERSION1 = 1;
		internal const int PROTOCOL_VERSION2 = 2; // 3/27/2011    Passing SQLState on exceptions; piggybacking generated key result sets
		internal const int PROTOCOL_VERSION3 = 3; // 2/14/2012    Passing txn, node id and commit sequence
		internal const int PROTOCOL_VERSION4 = 4; // 2/29/2012    Added GetCurrentSchema
		internal const int PROTOCOL_VERSION5 = 5; // 3/12/2012    Server returns DB UUID
		internal const int PROTOCOL_VERSION6 = 6; // 3/22/2012    Server encodes error message for each statement in batch
		internal const int PROTOCOL_VERSION7 = 7; // 5/11/2012    Fix issues with generated key result set and last commit info
		internal const int PROTOCOL_VERSION8 = 8; // 8/10/2012    Support for async result set close and prepared query sans column names
		internal const int PROTOCOL_VERSION9 = 9; // 8/30/2012    New Date/Time/Timestamp encodings
		 internal const int PROTOCOL_VERSION10 = 10; // 10/18/2012    Transact and ConnectionId

		internal const int PROTOCOL_VERSION = PROTOCOL_VERSION10;
		// When updating the PROTOCOL_VERSION, be sure to update Remote/ProtocolVersion.h

		internal const int PROTOCOL_VERSION_TRANSACT = PROTOCOL_VERSION10; // No expected integration for some time

		internal const int Failure = 0;
		internal const int Success = 1;
		internal const int Shutdown = 2;
		internal const int OpenDatabase = 3;
		internal const int CreateDatabase = 4;
		internal const int CloseConnection = 5;
		internal const int PrepareTransaction = 6;
		internal const int CommitTransaction = 7;
		internal const int RollbackTransaction = 8;
		internal const int PrepareStatement = 9;
		internal const int PrepareDrl = 10;
		internal const int CreateStatement = 11;
		internal const int GenConnectionHtml = 12;
		internal const int GetResultSet = 13;
		internal const int Search = 14;
		internal const int CloseStatement = 15;
		internal const int ClearParameters = 16;
		internal const int GetParameterCount = 17;
		internal const int Execute = 18;
		internal const int ExecuteQuery = 19;
		internal const int ExecuteUpdate = 20;
		internal const int SetCursorName = 21;
		internal const int ExecutePreparedStatement = 22;
		internal const int ExecutePreparedQuery = 23;
		internal const int ExecutePreparedUpdate = 24;
		internal const int SendParameters = 25;
		internal const int GetMetaData = 26;
		internal const int Next = 27;
		internal const int CloseResultSet = 28;
		internal const int GenHtml = 29;
		internal const int NextHit = 30;
		internal const int FetchRecord = 31;
		internal const int CloseResultList = 32;
		internal const int GetDatabaseMetaData = 33; // Connection
		internal const int GetCatalogs = 34; // DatabaseMetaData
		internal const int GetSchemas = 35; // DatabaseMetaData
		internal const int GetTables = 36; // DatabaseMetaData
		internal const int getTablePrivileges = 37; // DatabaseMetaData
		internal const int GetColumns = 38; // DatabaseMetaData
		internal const int GetColumnsPrivileges = 39; // DatabaseMetaData
		internal const int GetPrimaryKeys = 40; // DatabaseMetaData
		internal const int GetImportedKeys = 41; // DatabaseMetaData
		internal const int GetExportedKeys = 42; // DatabaseMetaData
		internal const int GetIndexInfo = 43; // DatabaseMetaData
		internal const int GetTableTypes = 44; // DatabaseMetaData
		internal const int GetTypeInfo = 45; // DatabaseMetaData
		internal const int GetMoreResults = 46; // Statement
		internal const int GetUpdateCount = 47; // Statement
		internal const int Ping = 48; // Connection
		internal const int HasRole = 49; // Connection
		internal const int GetObjectPrivileges = 50; // DatabaseMetaData
		internal const int GetUserRoles = 51; // DatabaseMetaData
		internal const int GetRoles = 52; // DatabaseMetaData
		internal const int GetUsers = 53; // DatabaseMetaData
		internal const int OpenDatabase2 = 54; // variation with attribute list
		internal const int CreateDatabase2 = 55; // variation with attribute list
		internal const int InitiateService = 56; // Connection
		internal const int GetTriggers = 57; // DatabaseMetaData
		internal const int Validate = 58;
		internal const int GetAutoCommit = 59;
		internal const int SetAutoCommit = 60;
		internal const int IsReadOnly = 61;
		internal const int SetReadOnly = 62;
		internal const int GetTransactionIsolation = 63;
		internal const int SetTransactionIsolation = 64;
		internal const int GetSequenceValue = 65;
		internal const int AddLogListener = 66; // Connection
		internal const int DeleteLogListener = 67; // Connection
		internal const int GetDatabaseProductName = 68; // DatabaseMetaData
		internal const int GetDatabaseProductVersion = 69; // DatabaseMetaData
		internal const int Analyze = 70; // Connection
		internal const int StatementAnalyze = 71; // Statement
		internal const int SetTraceFlags = 72; // Connection
		internal const int ServerOperation = 73; // Connection
		internal const int SetAttribute = 74; // Connection
		internal const int GetSequences = 75; // DatabaseMetaData
		internal const int GetHolderPrivileges = 76; // DatabaseMetaData
		internal const int AttachDebugger = 77; // Connection
		internal const int DebugRequest = 78; // Connection
		internal const int GetSequenceValue2 = 79; // Connection
		internal const int GetConnectionLimit = 80; // Connection
		internal const int SetConnectionLimit = 81; // Connection
		internal const int DeleteBlobData = 82; // Connection
		internal const int ExecuteBatchStatement = 83;
		internal const int ExecuteBatchPreparedStatement = 84;
		internal const int GetParameterMetaData = 85; // PreparedStatement
		internal const int Authentication = 86;
		internal const int GetGeneratedKeys = 87; // Statement
		internal const int PrepareStatementKeys = 88; // Connection
		internal const int PrepareStatementKeyNames = 89; // Connection
		internal const int PrepareStatementKeyIds = 90; // Connection
		internal const int ExecuteKeys = 91; // Statement
		internal const int ExecuteKeyNames = 92; // Statement
		internal const int ExecuteKeyIds = 93; // Statement
		internal const int ExecuteUpdateKeys = 94; // Statement
		internal const int ExecuteUpdateKeyNames = 95; // Statement
		internal const int ExecuteUpdateKeyIds = 96; // Statement
		internal const int SetSavepoint = 97; // Connection
		internal const int ReleaseSavepoint = 98; // Connection
		internal const int RollbackToSavepoint = 99; // Connection
		internal const int SupportsTransactionIsolation = 100; // Connection
		internal const int GetCatalog = 101; // Connection
		internal const int GetCurrentSchema = 102; // Connection
		internal const int Transact = 103; // Connection

		// updates to this list must be make in MsgType.h
		// Database Metadata Items

		internal const int DbmbFini = 0;
		internal const int DbmbProductName = 1;
		internal const int DbmbProductVersion = 2;
		internal const int DbmbDatabaseMinorVersion = 3;
		internal const int DbmbDatabaseMajorVersion = 4;
		internal const int DbmbDefaultTransactionIsolation = 5;

		internal const int SendColumnNames = 0;
		internal const int SkipColumnNames = 1;
	}


}