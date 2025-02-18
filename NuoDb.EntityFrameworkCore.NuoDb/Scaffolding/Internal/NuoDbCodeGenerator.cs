// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace NuoDb.EntityFrameworkCore.NuoDb.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbCodeGenerator : ProviderCodeGenerator
    {
        private static readonly MethodInfo _useNuoDbMethodInfo
            = typeof(NuoDbDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(NuoDbDbContextOptionsBuilderExtensions.UseNuoDb),
                typeof(DbContextOptionsBuilder),
                typeof(string),
                typeof(Action<NuoDbDbContextOptionsBuilder>));

        /// <summary>
        ///     Initializes a new instance of the <see cref="NuoDbCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        public NuoDbCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment? providerOptions)
            => new(
                _useNuoDbMethodInfo,
                providerOptions == null
                    ? new object[] { connectionString }
                    : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
    }
}
