﻿/****************************************************************************
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Transactions;

namespace NuoDb.Data.Client
{
    public class NuoDbTransaction : DbTransaction, ISinglePhaseNotification
    {
        private NuoDbConnection connection;
        private System.Data.IsolationLevel isolationLevel;

        public NuoDbTransaction(NuoDbConnection nuoDBConnection, System.Data.IsolationLevel isolationLevel)
        {
            this.connection = nuoDBConnection;
            this.isolationLevel = isolationLevel;
        }

        public override void Commit()
        {
			connection.InternalConnection.Commit();
        }

        protected override DbConnection DbConnection
        {
            get { return connection; }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get { return isolationLevel; }
        }

        public override void Rollback()
        {
			connection.InternalConnection.Rollback();
        }

        #region ISinglePhaseNotification Members

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbTransaction::SinglePhaseCommit()");
#endif
            Commit();
            singlePhaseEnlistment.Done();
        }

        #endregion

        #region IEnlistmentNotification Members

        public void Commit(Enlistment enlistment)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbTransaction::IEnlistmentNotification::Commit()");
#endif
            Commit();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbTransaction::IEnlistmentNotification::InDoubt()");
#endif
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbTransaction::IEnlistmentNotification::Prepare()");
#endif
        }

        public void Rollback(Enlistment enlistment)
        {
#if DEBUG
            System.Diagnostics.Trace.WriteLine("NuoDbTransaction::IEnlistmentNotification::Rollback()");
#endif
            Rollback();
            enlistment.Done();
        }

        #endregion
    }
}
