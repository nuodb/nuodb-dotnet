// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
{
    // TODO: Implement all methods in this file.

    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class NuoDbDatabaseCreator : RelationalDatabaseCreator
    {
        // ReSharper disable once InconsistentNaming
        private const int NuoDb_CANTOPEN = 14;

        private readonly INuoDbRelationalConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbDatabaseCreator(
            RelationalDatabaseCreatorDependencies dependencies,
            INuoDbRelationalConnection connection,
            IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Create()
        {
            // Dependencies.Connection.Open();
            //
            // _rawSqlCommandBuilder.Build("PRAGMA journal_mode = 'wal';")
            //     .ExecuteNonQuery(
            //         new RelationalCommandParameterObject(
            //             Dependencies.Connection,
            //             null,
            //             null,
            //             null,
            //             Dependencies.CommandLogger, CommandSource.Migrations));
            //
            // Dependencies.Connection.Close();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Exists()
        {
            
            var connectionOptions = new NuoDbConnectionStringBuilder(_connection.ConnectionString);
            connectionOptions.Pooling = false;


            using (var readOnlyConnection = new NuoDbConnection(connectionOptions.ToString()))
            {
                try
                {
                    readOnlyConnection.Open();
                }
                catch (NuoDbSqlException ex)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool HasTables()
        {
            var connectionBuidler = new NuoDbConnectionStringBuilder(_connection.ConnectionString);
            var database = connectionBuidler.Database;
            var count = (long)_rawSqlCommandBuilder
                .Build($"select count(*) from SYSTEM.TABLES where SCHEMA = '{database}'")
                .ExecuteScalar(
                    new RelationalCommandParameterObject(
                        Dependencies.Connection,
                        null,
                        null,
                        null,
                        Dependencies.CommandLogger, CommandSource.Migrations))!;
            
            return count != 0;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Delete()
        {
            // string? path = null;
            //
            // Dependencies.Connection.Open();
            // try
            // {
            //     path = Dependencies.Connection.DbConnection.DataSource;
            // }
            // catch
            // {
            //     // any exceptions here can be ignored
            // }
            // finally
            // {
            //     Dependencies.Connection.Close();
            // }
            //
            // if (!string.IsNullOrEmpty(path))
            // {
            //     NuoDbConnection.ClearPool(new NuoDbConnection(Dependencies.Connection.ConnectionString));
            //     // See issues #25797 and #26016
            //     // NuoDbConnection.ClearAllPools();
            //     File.Delete(path);
            // }
        }
    }
}
