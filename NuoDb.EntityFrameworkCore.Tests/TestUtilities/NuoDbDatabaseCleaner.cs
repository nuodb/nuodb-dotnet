using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuoDb.Data.Client;
using NuoDb.EntityFrameworkCore.NuoDb.Design.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Diagnostics.Internal;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class NuoDbDatabaseCleaner: RelationalDatabaseCleaner
    {
        protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
        {
           var services = new ServiceCollection();
               services.AddSingleton<TypeMappingSourceDependencies>()
                .AddSingleton<RelationalTypeMappingSourceDependencies>()
                .AddSingleton<ValueConverterSelectorDependencies>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<IDbContextLogger, NullDbContextLogger>()
                .AddSingleton<LoggingDefinitions, NuoDbLoggingDefinitions>()
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                .AddSingleton<IValueConverterSelector, ValueConverterSelector>()
                .AddSingleton<IInterceptors, Interceptors>()
                .AddLogging();
            new NuoDbDesignTimeServices().ConfigureDesignTimeServices(services);

            return services
                .BuildServiceProvider() // No scope validation; cleaner violates scopes, but only resolve services once.
                .GetRequiredService<IDatabaseModelFactory>();
        }

        public override void Clean(DatabaseFacade facade)
        {
            var connection = facade.GetDbConnection();
            var schema = new NuoDbConnectionStringBuilder(connection.ConnectionString).Schema;

            var opened = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                opened = true;
            }
           
            var command = connection.CreateCommand();
            var transaction = connection.BeginTransaction();

            try
            {
                var dropcmd = connection.CreateCommand();
                dropcmd.CommandText = $"drop schema \"{schema}\" cascade;";
                dropcmd.ExecuteNonQuery();

                command.CommandText = $"create schema \"{schema}\";";
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        protected override bool AcceptForeignKey(DatabaseForeignKey foreignKey)
            => false;

        protected override bool AcceptIndex(DatabaseIndex index)
            => false;
    }
}
