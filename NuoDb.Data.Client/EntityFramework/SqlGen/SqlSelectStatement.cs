/****************************************************************************
*  Author: Jiri Cincura (jiri@cincura.net)
*  Adapted from Firebird ADO.NET Data provider
****************************************************************************/

#if !__MonoCS__

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SqlClient;

#if EF6
namespace NuoDb.Data.Client.EntityFramework6.SqlGen
#else
namespace NuoDb.Data.Client.EntityFramework.SqlGen
#endif
{
    internal sealed class SqlSelectStatement : ISqlFragment
    {
        #region � Fields �

        private bool isDistinct;
        private List<Symbol> allJoinExtents;
        private List<Symbol> fromExtents;
        private Dictionary<Symbol, bool> outerExtents;
        private ISqlFragment first;
        private ISqlFragment skip;
        private SqlBuilder select = new SqlBuilder();
        private SqlBuilder from = new SqlBuilder();
        private SqlBuilder where;
        private SqlBuilder groupBy;
        private SqlBuilder orderBy;
        //indicates whether it is the top most select statement, 
        // if not Order By should be omitted unless there is a corresponding TOP
        private bool isTopMost;

        #endregion

        #region � Properties �

        public SqlBuilder OrderBy
        {
            get
            {
                if (null == orderBy)
                {
                    this.orderBy = new SqlBuilder();
                }
                return this.orderBy;
            }
        }

        #endregion

        #region � Internal Properties �

        /// <summary>
        /// Do we need to add a DISTINCT at the beginning of the SELECT
        /// </summary>
        internal bool IsDistinct
        {
            get { return this.isDistinct; }
            set { this.isDistinct = value; }
        }

        internal List<Symbol> AllJoinExtents
        {
            get { return this.allJoinExtents; }
            // We have a setter as well, even though this is a list,
            // since we use this field only in special cases.
            set { this.allJoinExtents = value; }
        }

        internal List<Symbol> FromExtents
        {
            get
            {
                if (null == this.fromExtents)
                {
                    this.fromExtents = new List<Symbol>();
                }
                return this.fromExtents;
            }
        }

        internal Dictionary<Symbol, bool> OuterExtents
        {
            get
            {
                if (null == outerExtents)
                {
                    this.outerExtents = new Dictionary<Symbol, bool>();
                }
                return outerExtents;
            }
        }

        internal ISqlFragment First
        {
            get { return this.first; }
            set
            {
                Debug.Assert(first == null, "SqlSelectStatement.Top has already been set");
                this.first = value;
            }
        }

        internal ISqlFragment Skip
        {
            get { return this.skip; }
            set
            {
                Debug.Assert(skip == null, "SqlSelectStatement.Skip has already been set");
                this.skip = value;
            }
        }

        internal SqlBuilder Select
        {
            get { return this.select; }
        }

        internal SqlBuilder From
        {
            get { return this.from; }
        }

        internal SqlBuilder Where
        {
            get
            {
                if (null == this.where)
                {
                    this.where = new SqlBuilder();
                }
                return this.where;
            }
        }

        internal SqlBuilder GroupBy
        {
            get
            {
                if (null == this.groupBy)
                {
                    this.groupBy = new SqlBuilder();
                }
                return this.groupBy;
            }
        }

        internal bool IsTopMost
        {
            get { return this.isTopMost; }
            set { this.isTopMost = value; }
        }

        #endregion

        #region � ISqlFragment Members �

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            #region Check if FROM aliases need to be renamed

            // Create a list of the aliases used by the outer extents
            // JoinSymbols have to be treated specially.
            List<string> outerExtentAliases = null;

            if ((null != outerExtents) && (0 < outerExtents.Count))
            {
                foreach (Symbol outerExtent in outerExtents.Keys)
                {
                    JoinSymbol joinSymbol = outerExtent as JoinSymbol;
                    if (joinSymbol != null)
                    {
                        foreach (Symbol symbol in joinSymbol.FlattenedExtentList)
                        {
                            if (null == outerExtentAliases)
                            {
                                outerExtentAliases = new List<string>();
                            }
                            outerExtentAliases.Add(symbol.NewName);
                        }
                    }
                    else
                    {
                        if (null == outerExtentAliases)
                        {
                            outerExtentAliases = new List<string>();
                        }
                        outerExtentAliases.Add(outerExtent.NewName);
                    }
                }
            }

            // An then rename each of the FromExtents we have
            // If AllJoinExtents is non-null - it has precedence.
            // The new name is derived from the old name - we append an increasing int.
            List<Symbol> extentList = this.AllJoinExtents ?? this.fromExtents;
            if (null != extentList)
            {
                foreach (Symbol fromAlias in extentList)
                {
                    if ((null != outerExtentAliases) && outerExtentAliases.Contains(fromAlias.Name))
                    {
                        int i = sqlGenerator.AllExtentNames[fromAlias.Name];
                        string newName;

                        do
                        {
                            ++i;
                            newName = fromAlias.Name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        while (sqlGenerator.AllExtentNames.ContainsKey(newName));

                        sqlGenerator.AllExtentNames[fromAlias.Name] = i;
                        fromAlias.NewName = newName;

                        // Add extent to list of known names (although i is always incrementing, "prefix11" can
                        // eventually collide with "prefix1" when it is extended)
                        sqlGenerator.AllExtentNames[newName] = 0;
                    }

                    // Add the current alias to the list, so that the extents
                    // that follow do not collide with me.
                    if (null == outerExtentAliases)
                    {
                        outerExtentAliases = new List<string>();
                    }
                    outerExtentAliases.Add(fromAlias.NewName);
                }
            }

            #endregion

            // Increase the indent, so that the Sql statement is nested by one tab.
            writer.Indent++;

            writer.Write("SELECT ");
            if (this.IsDistinct)
            {
                writer.Write("DISTINCT ");
            }

            if ((this.select == null) || this.Select.IsEmpty)
            {
                Debug.Assert(false);  // we have removed all possibilities of SELECT *.
                writer.Write("*");
            }
            else
            {
                this.Select.WriteSql(writer, sqlGenerator);
            }

            writer.WriteLine();
            writer.Write("FROM ");
            this.From.WriteSql(writer, sqlGenerator);

            if ((this.where != null) && !this.Where.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("WHERE ");
                this.Where.WriteSql(writer, sqlGenerator);
            }

            if ((this.groupBy != null) && !this.GroupBy.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("GROUP BY ");
                this.GroupBy.WriteSql(writer, sqlGenerator);
            }

            if ((this.orderBy != null) && !this.OrderBy.IsEmpty && (this.IsTopMost || this.First != null || this.Skip != null))
            {
                writer.WriteLine();
                writer.Write("ORDER BY ");
                this.OrderBy.WriteSql(writer, sqlGenerator);
            }

            if (this.First != null)
            {
                writer.WriteLine();
                writer.Write("LIMIT ");
                this.First.WriteSql(writer, sqlGenerator);
                writer.Write(" ");
            }
            if (this.Skip != null)
            {
                writer.WriteLine();
                writer.Write("OFFSET ");
                this.Skip.WriteSql(writer, sqlGenerator);
                writer.Write(" ");
            }

            writer.Indent--;
        }

        #endregion
    }
}

#endif