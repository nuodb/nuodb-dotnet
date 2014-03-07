/****************************************************************************
*  Author: Jiri Cincura (jiri@cincura.net)
*  Adapted from Firebird ADO.NET Data provider
****************************************************************************/

#if !__MonoCS__

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

#if EF6
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Common.CommandTrees;
using NuoDb.Data.Client;

namespace NuoDb.Data.Client.EntityFramework6.SqlGen
#else
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace NuoDb.Data.Client.EntityFramework.SqlGen
#endif
{
    /// <summary>
    /// Class generating SQL for a DML command tree.
    /// </summary>
    internal static class DmlSqlGenerator
    {
        #region · Static Fields ·

        private const int CommandTextBuilderInitialCapacity = 256;

        #endregion

        #region · Static Methods ·

        internal static string GenerateUpdateSql(DbUpdateCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(CommandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, null != tree.Returning);
            bool first = true;

            commandText.Append("UPDATE ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // set c1 = ..., c2 = ..., ...            
            commandText.Append("SET ");

            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandText.Append(", ");
                }

                setClause.Property.Accept(translator);
                commandText.Append(" = ");
                setClause.Value.Accept(translator);

                translator.RegisterMemberValue(setClause.Property, setClause.Value);
            }

            if (first)
            {
                // If first is still true, it indicates there were no set
                // clauses. Introduce a fake set clause so that:
                // - we acquire the appropriate locks
                // - server-gen columns (e.g. timestamp) get recomputed

                EntitySetBase table = ((DbScanExpression)tree.Target.Expression).Target;
                // hope this column isn't indexed to not waste power
                EdmMember someColumn = table.ElementType.Members.Last(x => !MetadataHelpers.IsStoreGenerated(x));
                commandText.AppendFormat("{0} = {0}", GenerateMemberSql(someColumn));
            }
            commandText.AppendLine();

            // where c1 = ..., c2 = ...
            commandText.Append("WHERE ");
            tree.Predicate.Accept(translator);
            commandText.AppendLine();

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateDeleteSql(DbDeleteCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(CommandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, false);

            commandText.Append("DELETE FROM ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // where c1 = ... AND c2 = ...
            commandText.Append("WHERE ");
            tree.Predicate.Accept(translator);

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateInsertSql(DbInsertCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(CommandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, null != tree.Returning);
            bool first = true;

            commandText.Append("INSERT INTO ");
            tree.Target.Expression.Accept(translator);

            // (c1, c2, c3, ...)
            commandText.Append("(");

            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandText.Append(", ");
                }
                setClause.Property.Accept(translator);
            }
            commandText.AppendLine(")");

            // values c1, c2, ...
            first = true;
            commandText.Append("VALUES (");
            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandText.Append(", ");
                }

                setClause.Value.Accept(translator);

                translator.RegisterMemberValue(setClause.Property, setClause.Value);
            }
            commandText.AppendLine(")");

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        // Generates SQL describing a member
        // Requires: member must belong to an entity type (a safe requirement for DML
        // SQL gen, where we only access table columns)
        internal static string GenerateMemberSql(EdmMember member)
        {
            return SqlGenerator.QuoteIdentifier(member.Name);
        }

        #endregion
    }
}

#endif