using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal
{
    public class NuoDbByteArrayTypeMapping: ByteArrayTypeMapping
    {
        private const int MaxSize = 8000;

        private readonly DbType? _dbType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbByteArrayTypeMapping(
            string? storeType = null,
            int? size = null,
            bool fixedLength = false,
            ValueComparer? comparer = null,
            DbType? dbType = null,
            StoreTypePostfix? storeTypePostfix = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(byte[]), null, comparer),
                    storeType ?? (fixedLength ? "binary" : "varbinary"),
                    storeTypePostfix ?? StoreTypePostfix.Size,
                    System.Data.DbType.Binary,
                    size: size,
                    fixedLength: fixedLength),
                dbType)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected NuoDbByteArrayTypeMapping(RelationalTypeMappingParameters parameters, DbType? dbType)
            : base(parameters)
        {
            _dbType = dbType;
        }

        private static int CalculateSize(int? size)
            => size.HasValue && size < MaxSize ? size.Value : MaxSize;

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <returns>The newly created mapping.</returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NuoDbByteArrayTypeMapping(parameters, _dbType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as byte[])?.Length;
            var maxSpecificSize = CalculateSize(Size);

            if (_dbType.HasValue
                && parameter is NuoDbParameter sqlParameter) // To avoid crashing wrapping providers
            {
                sqlParameter.DbType = _dbType.Value;
            }

            if (value == null
                || value == DBNull.Value)
            {
                parameter.Size = maxSpecificSize;
            }
            else
            {
                if (length != null
                    && length <= maxSpecificSize)
                {
                    // Fixed-sized parameters get exact length to avoid padding/truncation.
                    parameter.Size = IsFixedLength ? length.Value : maxSpecificSize;
                }
                else if (length != null
                    && length <= MaxSize)
                {
                    parameter.Size = IsFixedLength ? length.Value : MaxSize;
                }
                else
                {
                    parameter.Size = -1;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var builder = new StringBuilder();
            builder.Append("0x");

            foreach (var @byte in (byte[])value)
            {
                builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
