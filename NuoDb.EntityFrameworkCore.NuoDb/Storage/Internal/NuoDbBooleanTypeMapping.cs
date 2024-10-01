using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
{
    public class NuoDbBooleanTypeMapping: BoolTypeMapping
    {
        public NuoDbBooleanTypeMapping(
            string storeType,
            DbType? dbType = System.Data.DbType.Boolean)
            : base(storeType, dbType)
        {

        }

        protected NuoDbBooleanTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {

        }

        /// <summary>
        /// Creates copy of this mapping
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NuoDbBooleanTypeMapping(parameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
            => $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
    }
}
