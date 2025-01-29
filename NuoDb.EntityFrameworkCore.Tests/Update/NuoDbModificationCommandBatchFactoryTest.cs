using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Update.Internal;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Update
{
    public class NuoDbModificationCommandBatchFactoryTest
    {
        [ConditionalFact]
        public void CanCreate()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNuoDb("Database=Crunchie", b => b.MaxBatchSize(1));

            var typeMapper = new NuoDbTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            var logger = new FakeRelationalCommandDiagnosticsLogger();

            var factory = new NuoDbModificationCommandBatchFactory(
                new ModificationCommandBatchFactoryDependencies(
                    new RelationalCommandBuilderFactory(
                        new RelationalCommandBuilderDependencies(
                            typeMapper,
                            new NuoDbExceptionDetector())),
                    new NuoDbSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new NuoDbUpdateSqlGenerator(
                        new UpdateSqlGeneratorDependencies(
                            new NuoDbSqlGenerationHelper(
                                new RelationalSqlGenerationHelperDependencies()),
                            typeMapper)),
                    new CurrentDbContext(new FakeDbContext()),
                    logger,
                    new FakeDiagnosticsLogger<DbLoggerCategory.Update>()));

            var batch = factory.Create();
        }
    
        
    }
    
    class FakeDbContext : DbContext
    {
    }
    class NuoDbExceptionDetector: IExceptionDetector
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsCancellation(Exception exception, CancellationToken cancellationToken = default)
            => exception is OperationCanceledException || cancellationToken.IsCancellationRequested;
    }
    // class TestNuoDbModificationCommandBatch : NuoDbModificationCommandBatch
    // {
    //     public TestNuoDbModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies, int maxBatchSize)
    //         : base(dependencies, maxBatchSize)
    //     {
    //     }
    //
    //     public new Dictionary<string, object> ParameterValues
    //         => base.ParameterValues;
    //
    //     public new RawSqlCommand StoreCommand
    //         => base.StoreCommand;
    //
    //     public new IList<ResultSetMapping> ResultSetMappings
    //         => base.ResultSetMappings;
    // }
    

    
}
