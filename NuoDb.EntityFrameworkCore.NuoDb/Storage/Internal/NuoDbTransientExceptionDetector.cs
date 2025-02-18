// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
{
    public static class NuoDbTransientExceptionDetector
    {
        public static bool ShouldRetryOn(Exception ex)
        {
            if (ex is NuoDbSqlException sqlException)
            {
                // run checks on exception for transient failures returning true, if should retry

                return false;
            }

            return ex is TimeoutException;
        }
    }
}
