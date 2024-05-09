// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbQueryStringFactory : IRelationalQueryStringFactory
    {
        private readonly IRelationalTypeMappingSource _typeMapper;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbQueryStringFactory(IRelationalTypeMappingSource typeMapper)
        {
            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Create(DbCommand command)
        {
            if (command.Parameters.Count == 0)
            {
                return command.CommandText;
            }

            var commandText = command.CommandText;

            //var builder = new StringBuilder();
            foreach (DbParameter parameter in command.Parameters)
            {
                var value = parameter.Value;
                var dataValue = value == null || value == DBNull.Value
                    ? "NULL"
                    : _typeMapper.FindMapping(value.GetType())?.GenerateSqlLiteral(value)
                      ?? value.ToString();
                commandText = commandText.Replace(parameter.ParameterName, dataValue);
                // builder
                //     .Append("VAR ")
                //     .Append(parameter.ParameterName.TrimStart('@'))
                //     .Append('=')
                //     .AppendLine($"{dataValue};");
            }

            return commandText;
            // return builder
            //     .AppendLine()
            //     .Append(command.CommandText.Replace("@__", "__")).ToString();


        }
    }
}
