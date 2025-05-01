using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Update;
using System.Linq;

namespace NuoDb.EntityFrameworkCore.NuoDb.Extensions.Internal
{
    [ExcludeFromCodeCoverage]
    public static class ReadOnlyModificationCommandExtensions
    {
        /// <summary>
        /// Determines whether we can simply append a select SCOPE_IDENTITY, or need to explicitly select
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool IsIdentitySelectOnly(this IReadOnlyModificationCommand command)
        {
            
            var readOperations = command.ColumnModifications.Count(x=>x.IsRead == true);
            if (readOperations > 1 || readOperations <1)
            {
                return false;
            }

            return command.ColumnModifications.First(x=>x.IsRead == true).IsKey;

        }
    }
}
