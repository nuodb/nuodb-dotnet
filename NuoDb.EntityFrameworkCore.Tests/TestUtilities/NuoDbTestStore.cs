using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.Data.Client;
using System.Data;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class NuoDbTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 300;

        private static string CurrentDirectory
            => Environment.CurrentDirectory;

        public static NuoDbTestStore GetNorthwindStore()
            => (NuoDbTestStore)NuoDbNorthwindTestStoreFactory.Instance
                .GetOrCreate(NuoDbNorthwindTestStoreFactory.Name).Initialize(null, (Func<DbContext>)null);

        public static NuoDbTestStore GetOrCreate(string name)
            => new(name);

        public static NuoDbTestStore GetOrCreateInitialized(string name)
            => new NuoDbTestStore(name).InitializeNuoDb(null, (Func<DbContext>)null, null);

        public static NuoDbTestStore GetOrCreate(string name, string scriptPath, bool? multipleActiveResultSets = null)
            => new(name, scriptPath: scriptPath);

        public static NuoDbTestStore Create(string name, bool useFileName = false)
            => new(name, shared: false);

        public static NuoDbTestStore CreateInitialized(string name, bool useFileName = false, bool? multipleActiveResultSets = null)
            => new NuoDbTestStore(name, shared: false)
                .InitializeNuoDb(null, (Func<DbContext>)null, null);

        private readonly string _scriptPath;

        private NuoDbTestStore(
            string name,
            string scriptPath = null,
            bool shared = true)
            : base(name, shared)
        {
            if (scriptPath != null)
            {
                _scriptPath = Path.Combine(Path.GetDirectoryName(typeof(NuoDbTestStore).Assembly.Location), scriptPath);
            }

            ConnectionString = CreateConnectionString(name);
            Connection = new NuoDbConnection(ConnectionString);
            ExecuteNonQuery(Connection, $"USE \"{Name}\";");
        }

        public NuoDbTestStore InitializeNuoDb(
            IServiceProvider serviceProvider,
            Func<DbContext> createContext,
            Action<DbContext> seed)
            => (NuoDbTestStore)Initialize(serviceProvider, createContext, seed);

        public NuoDbTestStore InitializeNuoDb(
            IServiceProvider serviceProvider,
            Func<NuoDbTestStore, DbContext> createContext,
            Action<DbContext> seed)
            => InitializeNuoDb(serviceProvider, () => createContext(this), seed);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
        {
            if (CreateSchema())
            {
                if (_scriptPath != null)
                {
                    ExecuteScript(_scriptPath);
                }
                else
                {
                    using var context = createContext();
                    context.Database.EnsureCreatedResiliently();
                    seed?.Invoke(context);
                }
            }
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder
                .UseNuoDb(Connection, b => b.ApplyConfiguration());

        private bool CreateSchema()
        {
            using (var master = new NuoDbConnection(CreateConnectionString()))
            {
                master.Open();
                var schemaExists =
                    ExecuteScalar<long>(master, $"SELECT COUNT(*) FROM SYSTEM.SCHEMAS WHERE SCHEMA = '{Name}'") > 0;
                try
                {
                    if (schemaExists)
                    {
                        ExecuteNonQuery(master, $"DROP SCHEMA \"{Name}\" CASCADE");
                    }

                    ExecuteNonQuery(master, $"CREATE SCHEMA \"{Name}\";");
                    ExecuteNonQuery(master, $"USE \"{Name}\";");
                    return true;
                }
                catch (NuoDbSqlException ex)
                {
                    return false;
                }
            }
        }

        private bool CreateDatabase(Action<DbContext> clean)
        {
            // using (var master = new NuoDbConnection(CreateConnectionString()))
            // {
            //     if (ExecuteScalar<int>(master, $"SELECT COUNT(*) FROM sys.databases WHERE name = N'{Name}'") > 0)
            //     {
            //         // Only reseed scripted databases during CI runs
            //         if (_scriptPath != null
            //             && !TestEnvironment.IsCI)
            //         {
            //             return false;
            //         }
            //
            //         var _filename = null;
            //         if (_fileName == null)
            //         {
            //             using var context = new DbContext(
            //                 AddProviderOptions(
            //                         new DbContextOptionsBuilder()
            //                             .EnableServiceProviderCaching(false))
            //                     .Options);
            //             clean?.Invoke(context);
            //             Clean(context);
            //             return true;
            //         }
            //
            //         // Delete the database to ensure it's recreated with the correct file path
            //         DeleteDatabase();
            //     }
            //
            //     ExecuteNonQuery(master, GetCreateDatabaseStatement(Name, _fileName));
            //     WaitForExists((NuoDbConnection)Connection);
            // }

            return true;
        }

        public override void Clean(DbContext context)
            => context.Database.EnsureClean();

        public void ExecuteScript(string scriptPath)
        {
            var script = File.ReadAllText(scriptPath);
            var statements = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan
                    .FromMilliseconds(1000.0))
                .Split(script).Where(b => !string.IsNullOrEmpty(b));
            Execute(
                Connection, command =>
                {

                    foreach (var batch in statements)
                    {
                        var sql = Regex.Replace(batch, "^--.*[\\r\\n]", "", RegexOptions.Multiline);
                        if (string.IsNullOrWhiteSpace(sql))
                        {
                            continue;
                        }
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }

                    return 0;
                }, "");
        }

        private static void WaitForExists(NuoDbConnection connection)
            => new TestNuoDbRetryingExecutionStrategy().Execute(connection, WaitForExistsImplementation);

        private static void WaitForExistsImplementation(NuoDbConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }

                    NuoDbConnection.ClearPool(connection);

                    connection.Open();
                    connection.Close();
                    return;
                }
                catch (NuoDbSqlException e)
                {
                    // if (++retryCount >= 30
                    //     || e.Number != 233 && e.Number != -2 && e.Number != 4060 && e.Number != 1832 && e.Number != 5120)
                    // {
                    //     throw;
                    // }

                    Thread.Sleep(100);
                }
            }
        }
        
        public override void OpenConnection()
            => new TestNuoDbRetryingExecutionStrategy().Execute(Connection, connection => connection.Open());

        public override Task OpenConnectionAsync()
            => new TestNuoDbRetryingExecutionStrategy().ExecuteAsync(Connection, connection => connection.OpenAsync());

        public T ExecuteScalar<T>(string sql, params object[] parameters)
            => ExecuteScalar<T>(Connection, sql, parameters);

        private static T ExecuteScalar<T>(DbConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, parameters);

        public Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
            => ExecuteScalarAsync<T>(Connection, sql, parameters);

        private static Task<T> ExecuteScalarAsync<T>(DbConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, async command => (T)await command.ExecuteScalarAsync(), sql, false, parameters);

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(Connection, sql, parameters);

        private static int ExecuteNonQuery(DbConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
            => ExecuteNonQueryAsync(Connection, sql, parameters);

        private static Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, IReadOnlyList<object> parameters = null)
            => ExecuteAsync(connection, command => command.ExecuteNonQueryAsync(), sql, false, parameters);

        public IEnumerable<T> Query<T>(string sql, params object[] parameters)
            => Query<T>(Connection, sql, parameters);

        private static IEnumerable<T> Query<T>(DbConnection connection, string sql, object[] parameters = null)
            => Execute(
                connection, command =>
                {
                    using var dataReader = command.ExecuteReader();
                    var results = Enumerable.Empty<T>();
                    while (dataReader.Read())
                    {
                        results = results.Concat(new[] { dataReader.GetFieldValue<T>(0) });
                    }

                    return results;
                }, sql, false, parameters);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
            => QueryAsync<T>(Connection, sql, parameters);

        private static Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object[] parameters = null)
            => ExecuteAsync(
                connection, async command =>
                {
                    using var dataReader = await command.ExecuteReaderAsync();
                    var results = Enumerable.Empty<T>();
                    while (await dataReader.ReadAsync())
                    {
                        results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                    }

                    return results;
                }, sql, false, parameters);

        private static T Execute<T>(
            DbConnection connection,
            Func<DbCommand, T> execute,
            string sql,
            bool useTransaction = false,
            object[] parameters = null)
            => new TestNuoDbRetryingExecutionStrategy().Execute(
                new
                {
                    connection,
                    execute,
                    sql,
                    useTransaction,
                    parameters
                },
                state => ExecuteCommand(state.connection, state.execute, state.sql, state.useTransaction, state.parameters));

        private static T ExecuteCommand<T>(
            DbConnection connection,
            Func<DbCommand, T> execute,
            string sql,
            bool useTransaction,
            object[] parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.Open();

            try
            {
                using var transaction = useTransaction ? connection.BeginTransaction() : null;
                T result;
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    command.Transaction = transaction;
                    result = execute(command);
                }

                transaction?.Commit();

                return result;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private static Task<T> ExecuteAsync<T>(
            DbConnection connection,
            Func<DbCommand, Task<T>> executeAsync,
            string sql,
            bool useTransaction = false,
            IReadOnlyList<object> parameters = null)
            => new TestNuoDbRetryingExecutionStrategy().ExecuteAsync(
                new
                {
                    connection,
                    executeAsync,
                    sql,
                    useTransaction,
                    parameters
                },
                state => ExecuteCommandAsync(state.connection, state.executeAsync, state.sql, state.useTransaction, state.parameters));

        private static async Task<T> ExecuteCommandAsync<T>(
            DbConnection connection,
            Func<DbCommand, Task<T>> executeAsync,
            string sql,
            bool useTransaction,
            IReadOnlyList<object> parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                await connection.CloseAsync();
            }

            await connection.OpenAsync();
            try
            {
                using var transaction = useTransaction ? await connection.BeginTransactionAsync() : null;
                T result;
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    result = await executeAsync(command);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return result;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static DbCommand CreateCommand(
            DbConnection connection,
            string commandText,
            IReadOnlyList<object> parameters = null)
        {
            var command = (NuoDbCommand)connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = new NuoDbParameter();
                    parameter.ParameterName = "p" + i;
                    parameter.Value = parameters[i];
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        public override void Dispose()
        {
            base.Dispose();

            // if (_fileName != null // Clean up the database using a local file, as it might get deleted later
            //     || (TestEnvironment.IsSqlAzure && !Shared))
            // {
            //     DeleteDatabase();
            // }
        }

        public static string CreateConnectionString(string schema = null)
        {
            var builder = new NuoDbConnectionStringBuilder(TestEnvironment.DefaultConnection);
            if (schema != null)
            {
                builder.Schema = schema;
            }

            return builder.ToString();
        }
    }
}
