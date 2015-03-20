using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuoDb.Data.Client;
using System.Data.Common;

namespace NUnitEFCodeFirstTestProject
{
    class Utils
    {
        static string host = "localhost:48004";
        static string user = "dba";
        static string password = "goalie";
        static string database = "test";
        static string schema = "hockey";
        static internal string connectionString = "Server=  " + host + "; Database=\"" + database + "\"; User = " + user + " ;Password   = '" + password + "';Schema=\"" + schema + "\"";

        internal static void DropTable(NuoDbConnection cnn, string tableName)
        {
            try
            {
                DbCommand dropCommand = new NuoDbCommand("drop table " + tableName, cnn);
                dropCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                // table is allowed to be missing
            }
        }

        internal static void CreateHockeyTable()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                try
                {
                    DbCommand dropCommand = new NuoDbCommand("drop table hockey", connection);
                    dropCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // table is allowed to be missing
                }
                DbCommand createCommand = new NuoDbCommand("create table Hockey" +
                                                            "(" +
                                                            "   Id       bigint not NULL generated always as identity primary key," +
                                                            "   Number   Integer," +
                                                            "   Name     String," +
                                                            "   Position String," +
                                                            "   Team     String" +
                                                            ")", connection);
                createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("Insert into Hockey (Number, Name, Position, Team) Values (?,?,?,?)", connection);
                insertCommand.Prepare();

                insertCommand.Parameters[0].Value = 37;
                insertCommand.Parameters[1].Value = "PATRICE BERGERON";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 48;
                insertCommand.Parameters[1].Value = "CHRIS BOURQUE";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 11;
                insertCommand.Parameters[1].Value = "GREGORY CAMPBELL";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 18;
                insertCommand.Parameters[1].Value = "NATHAN HORTON";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 23;
                insertCommand.Parameters[1].Value = "CHRIS KELLY";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 46;
                insertCommand.Parameters[1].Value = "DAVID KREJCI";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 17;
                insertCommand.Parameters[1].Value = "MILAN LUCIC";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 64;
                insertCommand.Parameters[1].Value = "LANE MACDERMID";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 63;
                insertCommand.Parameters[1].Value = "BRAD MARCHAND";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 20;
                insertCommand.Parameters[1].Value = "DANIEL PAILLE";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 49;
                insertCommand.Parameters[1].Value = "RICH PEVERLEY";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 91;
                insertCommand.Parameters[1].Value = "MARC SAVARD";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 19;
                insertCommand.Parameters[1].Value = "TYLER SEGUIN";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 22;
                insertCommand.Parameters[1].Value = "SHAWN THORNTON";
                insertCommand.Parameters[2].Value = "Forward";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 55;
                insertCommand.Parameters[1].Value = "JOHNNY BOYCHUK";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 33;
                insertCommand.Parameters[1].Value = "ZDENO CHARA";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 21;
                insertCommand.Parameters[1].Value = "ANDREW FERENCE";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 27;
                insertCommand.Parameters[1].Value = "DOUGIE HAMILTON";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 45;
                insertCommand.Parameters[1].Value = "AARON JOHNSON";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 54;
                insertCommand.Parameters[1].Value = "ADAM MCQUAID";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 44;
                insertCommand.Parameters[1].Value = "DENNIS SEIDENBERG";
                insertCommand.Parameters[2].Value = "Defense";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 35;
                insertCommand.Parameters[1].Value = "ANTON KHUDOBIN";
                insertCommand.Parameters[2].Value = "Goalie";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 40;
                insertCommand.Parameters[1].Value = "TUUKKA RASK";
                insertCommand.Parameters[2].Value = "Goalie";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters[0].Value = 1;
                insertCommand.Parameters[1].Value = "MAX SUMMIT";
                insertCommand.Parameters[2].Value = "Fan";
                insertCommand.Parameters[3].Value = "Bruins";
                insertCommand.ExecuteNonQuery();

            }
        }

        internal static void CreatePersonTable()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                try
                {
                    DbCommand dropCommand = new NuoDbCommand("drop table Person", connection);
                    dropCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // table is allowed to be missing
                }
                DbCommand createCommand = new NuoDbCommand("create table Person" +
                                                            "(" +
                                                            "   Id       char(38) not NULL primary key," +
                                                            "   Name     String" +
                                                            ")", connection);
                createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("Insert into Person (Id, Name) Values (?,?)", connection);
                insertCommand.Prepare();

                insertCommand.Parameters[0].Value = new Guid("{F571197E-7A4F-4961-9363-7411EACCA841}");
                insertCommand.Parameters[1].Value = "Klaus Müller";

                insertCommand.ExecuteNonQuery();

            }
        }

        internal static void CreateGameTable()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                try
                {
                    DbCommand dropCommand = new NuoDbCommand("drop table Game", connection);
                    dropCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // table is allowed to be missing
                }
                DbCommand createCommand = new NuoDbCommand("create table Game" +
                                                            "(" +
                                                            "   Id       bigint generated always as identity not NULL primary key," +
                                                            "   Date     DATE" +
                                                            ")", connection);
                createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("Insert into Game (Date) Values (?)", connection);
                insertCommand.Prepare();

                insertCommand.Parameters[0].Value = "1970-01-01";

                insertCommand.ExecuteNonQuery();

            }
        }

        internal static int GetTableRows()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select count(*) from hockey", connection);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }
    }
}
