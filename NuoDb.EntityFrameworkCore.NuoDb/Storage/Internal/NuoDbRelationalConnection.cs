// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.Data.Client;
using NuoDb.EntityFrameworkCore.NuoDb.Extensions.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Infrastructure.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
{
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
    public class NuoDbRelationalConnection : RelationalConnection, INuoDbRelationalConnection
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;
        private readonly bool _loadSpatialite;
        private readonly int? _commandTimeout;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbRelationalConnection(
            RelationalConnectionDependencies dependencies,
            IRawSqlCommandBuilder rawSqlCommandBuilder,
            IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
            : base(dependencies)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _logger = logger;

            var optionsExtension = dependencies.ContextOptions.Extensions.OfType<NuoDbOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
                _loadSpatialite = optionsExtension.LoadSpatialite;

                var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);
                _commandTimeout = relationalOptions.CommandTimeout;

                if (relationalOptions.Connection != null)
                {
                    InitializeDbConnection(relationalOptions.Connection);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override DbConnection CreateDbConnection()
        {
            var connection = new NuoDbConnection(GetValidatedConnectionString());
            InitializeDbConnection(connection);

            return connection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual INuoDbRelationalConnection CreateReadOnlyConnection()
        {
            var connectionStringBuilder = new NuoDbConnectionStringBuilder(GetValidatedConnectionString())
            {
                //Mode = NuoDbOpenMode.ReadOnly,
                Pooling = false
            };

            var contextOptions = new DbContextOptionsBuilder().UseNuoDb(connectionStringBuilder.ToString()).Options;

            return new NuoDbRelationalConnection(Dependencies with { ContextOptions = contextOptions }, _rawSqlCommandBuilder, _logger);
        }
        
        // TODO: Implement 
        private void InitializeDbConnection(DbConnection connection)
        {

            if (connection is NuoDbConnection NuoDbConnection)
            {
                // if (_commandTimeout.HasValue)
                // {
                //     NuoDbConnection.DefaultTimeout = _commandTimeout.Value;
                // }
                //
                // NuoDbConnection.CreateFunction<string, string, bool?>(
                //     "regexp",
                //     (pattern, input) =>
                //         {
                //             if (input == null
                //                 || pattern == null)
                //             {
                //                 return null;
                //             }
                //
                //             return Regex.IsMatch(input, pattern);
                //         },
                //     isDeterministic: true);
                //
                // NuoDbConnection.CreateFunction<object, object, object?>(
                //     "ef_mod",
                //     (dividend, divisor) =>
                //         {
                //             if (dividend == null
                //                 || divisor == null)
                //             {
                //                 return null;
                //             }
                //
                //             if (dividend is string s)
                //             {
                //                 return decimal.Parse(s, CultureInfo.InvariantCulture)
                //                     % Convert.ToDecimal(divisor, CultureInfo.InvariantCulture);
                //             }
                //
                //             return Convert.ToDouble(dividend, CultureInfo.InvariantCulture)
                //                 % Convert.ToDouble(divisor, CultureInfo.InvariantCulture);
                //         },
                //     isDeterministic: true);
                //
                // NuoDbConnection.CreateFunction(
                //     name: "ef_add",
                //     (decimal? left, decimal? right) => left + right,
                //     isDeterministic: true);
                //
                // NuoDbConnection.CreateFunction(
                //     name: "ef_divide",
                //     (decimal? dividend, decimal? divisor) => dividend / divisor,
                //     isDeterministic: true);
                //
                // NuoDbConnection.CreateFunction(
                //     name: "ef_compare",
                //     (decimal? left, decimal? right) => left.HasValue && right.HasValue
                //         ? decimal.Compare(left.Value, right.Value)
                //         : default(int?),
                //     isDeterministic: true);
                //
                // NuoDbConnection.CreateFunction(
                //     name: "ef_multiply",
                //     (decimal? left, decimal? right) => left * right,
                //     isDeterministic: true);
                //
                // NuoDbConnection.CreateFunction(
                //     name: "ef_negate",
                //     (decimal? m) => -m,
                //     isDeterministic: true);
            }
            else
            {
                _logger.UnexpectedConnectionTypeWarning(connection.GetType());
            }
        }
    }
}
