using System;
using NUnit.Framework;
using NuoDb.Data.Client;
using System.Data.Common;
using System.Data;
using System.Collections;
using System.Threading;

namespace NUnitTestProject
{
    [TestFixture]
    public class TestFixture1
    {
        static string host = "localhost:48004";
        static string user = "dba";
        static string password = "goalie";
        static string database = "test";
        static string schema = "hockey";
        static internal string connectionString = "Server=  " + host + "; Database=\"" + database + "\"; User = " + user + " ;Password   = " + password + ";Schema=\"" + schema + "\"";

        [TestFixtureSetUp]
        public static void Init()
        {
            Utils.CreateHockeyTable();
        }

        [Test]
        public void TestHighAvailability()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString.Replace("Server=", "Server=localhost:8,")))
            {
                DbCommand command = new NuoDbCommand("select * from hockey", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestCommand1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select * from hockey", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestCommand2()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                command.CommandText = "select * from hockey";

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (i > 0)
                            Console.Out.Write(", ");
                        Console.Out.Write(reader[i]);
                    }
                    Console.WriteLine();
                }
                reader.Close();
            }
        }

        [Test]
        public void TestParameter()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select * from hockey where id = ?";
                command.Prepare();
                command.Parameters[0].Value = "2";

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestNamedParameters1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select * from hockey where number = ?.number and team = ?.team";
                command.Prepare();
                command.Parameters[0].Value = 1;
                command.Parameters[1].Value = "Bruins";

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestNamedParameters2()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select * from hockey where number = ?.number and team = ?.team";
                NuoDbParameter p1 = new NuoDbParameter();
                p1.ParameterName = "team";
                p1.Value = "Bruins";
                command.Parameters.Add(p1);
                NuoDbParameter p2 = new NuoDbParameter();
                p2.ParameterName = "number";
                p2.Value = 1;
                command.Parameters.Add(p2);
                command.Prepare();

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestNamedParameters3()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select name as \"'?\" from \"hockey\" where name='? ?.fake' or number = ?.number and team = ?.team";
                NuoDbParameter p1 = new NuoDbParameter();
                p1.ParameterName = "TEAM";
                p1.Value = "Bruins";
                command.Parameters.Add(p1);
                NuoDbParameter p2 = new NuoDbParameter();
                p2.ParameterName = "NUMBER";
                p2.Value = 1;
                command.Parameters.Add(p2);
                command.Prepare();

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}", reader["'?"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestNamedParameters4()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select * from hockey where number = ?.number and team = ?.team";
                command.Prepare();
                command.Parameters["NumbER"].Value = 1;
                command.Parameters["TEam"].Value = "Bruins";

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["ID"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestNamedParameters5()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select * from hockey where number = @number and team = @team";
                command.Prepare();
                command.Parameters["NumbER"].Value = 1;
                command.Parameters["TEam"].Value = "Bruins";

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["ID"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestPrepareNoParameter()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "select * from hockey where id = 2";
                command.Prepare();

                DbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestPrepareDDLNoParameter()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = connection.CreateCommand();
                connection.Open();

                command.CommandText = "create table xyz (col int)";
                command.Prepare();

                try
                {
                    int value = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Assert.Fail("Executing a prepared DDL that doesn't use parameters reports an error: {0}", e.Message);
                }
                finally
                {
                    Utils.DropTable(connection, "xyz");
                }
            }
        }

        [Test]
        public void TestTransactions()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand countCommand = connection.CreateCommand();
                countCommand.CommandText = "select count(*) from hockey";

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "insert into hockey (number, name) values (99, 'xxxx')";

                int count1 = (int)countCommand.ExecuteScalar();
                updateCommand.ExecuteNonQuery();
                int count2 = (int)countCommand.ExecuteScalar();

                Assert.AreEqual(count2, count1 + 1);

                transaction.Rollback();

                int count3 = (int)countCommand.ExecuteScalar();
                Assert.AreEqual(count3, count1);
            }
        }

        [Test]
        public void TestInsertWithGeneratedKeys()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand maxIdCmd = connection.CreateCommand();
                maxIdCmd.CommandText = "select max(id) from hockey";
                int maxId = (int)maxIdCmd.ExecuteScalar();

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "insert into hockey (number, name) values (99, 'xxxx')";

                DbDataReader reader = updateCommand.ExecuteReader();
                Assert.IsNotNull(reader, "The command should return a generated keys recordset");
                Assert.IsTrue(reader.Read(), "There must be at least one ID in the generated keys recordset");
                int lastId = (int)reader.GetValue(0);
                Assert.IsTrue(lastId > maxId, "The generated ID must be greater than the existing ones");

                transaction.Rollback();
            }
        }

        [Test]
        public void TestUpdateWithGeneratedKeys()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "update hockey set number = 99 where team = 'Bruins'";

                DbDataReader reader = updateCommand.ExecuteReader();
                Assert.IsNotNull(reader, "The command should return a generated keys recordset");
                Assert.IsFalse(reader.Read(), "The generated keys recordset should be empty");

                transaction.Rollback();
            }
        }

        [Test]
        public void TestPreparedInsertWithGeneratedKeys1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand maxIdCmd = connection.CreateCommand();
                maxIdCmd.CommandText = "select max(id) from hockey";
                int maxId = (int)maxIdCmd.ExecuteScalar();

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "insert into hockey (number, name) values (?, ?)";
                updateCommand.Parameters.Add(99);
                updateCommand.Parameters.Add("xxx");

                DbDataReader reader = updateCommand.ExecuteReader();
                Assert.IsNotNull(reader, "The command should return a generated keys recordset");
                Assert.IsTrue(reader.Read(), "There must be at least one ID in the generated keys recordset");
                int lastId = (int)reader.GetValue(0);
                Assert.IsTrue(lastId > maxId, "The generated ID must be greater than the existing ones");

                DbCommand selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "select name from hockey where id = ?";
                selectCommand.Parameters.Add(lastId);
                string value = (string)selectCommand.ExecuteScalar();
                Assert.AreEqual("xxx", value);

                transaction.Rollback();
            }
        }

        [Test]
        public void TestPreparedInsertWithGeneratedKeys2()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand maxIdCmd = connection.CreateCommand();
                maxIdCmd.CommandText = "select max(id) from hockey";
                int maxId = (int)maxIdCmd.ExecuteScalar();

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "insert into hockey (number, name) values (?, ?)";
                updateCommand.Prepare();
                updateCommand.Parameters[0].Value = 99;
                updateCommand.Parameters[1].Value = "xxx";

                DbDataReader reader = updateCommand.ExecuteReader();
                Assert.IsNotNull(reader, "The command should return a generated keys recordset");
                Assert.IsTrue(reader.Read(), "There must be at least one ID in the generated keys recordset");
                int lastId = (int)reader.GetValue(0);
                Assert.IsTrue(lastId > maxId, "The generated ID must be greater than the existing ones");

                DbCommand selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "select name from hockey where id = ?";
                selectCommand.Parameters.Add(lastId);
                string value = (string)selectCommand.ExecuteScalar();
                Assert.AreEqual("xxx", value);

                transaction.Rollback();
            }
        }

        [Test]
        public void TestPreparedUpdateWithGeneratedKeys1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "update hockey set number = 99 where team = ?";
                updateCommand.Parameters.Add("Bruins");

                DbDataReader reader = updateCommand.ExecuteReader();
                Assert.IsNotNull(reader, "The command should return a generated keys recordset");
                Assert.IsFalse(reader.Read(), "The generated keys recordset should be empty");

                transaction.Rollback();
            }
        }

        [Test]
        public void TestPreparedUpdateWithGeneratedKeys2()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                DbCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "update hockey set number = 99 where team = ?";
                updateCommand.Prepare();
                updateCommand.Parameters[0].Value = "Bruins";

                DbDataReader reader = updateCommand.ExecuteReader();
                Assert.IsNotNull(reader, "The command should return a generated keys recordset");
                Assert.IsFalse(reader.Read(), "The generated keys recordset should be empty");

                transaction.Rollback();
            }
        }

        [Test]
        public void TestDbProvider()
        {
            DbProviderFactory factory = new NuoDbProviderFactory();
            using (DbConnection cn = factory.CreateConnection())
            {
                DbConnectionStringBuilder conStrBuilder = factory.CreateConnectionStringBuilder();
                conStrBuilder["Server"] = host;
                conStrBuilder["User"] = user;
                conStrBuilder["Password"] = password;
                conStrBuilder["Schema"] = schema;
                conStrBuilder["Database"] = database;
                Console.WriteLine("Connection string = {0}", conStrBuilder.ConnectionString);

                cn.ConnectionString = conStrBuilder.ConnectionString;
                cn.Open();

                DbCommand cmd = factory.CreateCommand();
                cmd.Connection = cn;
                cmd.CommandText = "select * from hockey";

                DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
                reader.Close();
            }
        }

        [Test]
        public void TestDisconnected()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DataAdapter da = new NuoDbDataAdapter("select * from hockey", connection);
                DataSet ds = new DataSet();
                da.Fill(ds);
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    for (int i = 0; i < r.ItemArray.Length; i++)
                    {
                        if (i > 0)
                            Console.Out.Write(", ");
                        Console.Out.Write(r.ItemArray[i]);
                    } // for
                    Console.Out.WriteLine();
                } // foreach 

                DataTable hockey = ds.Tables[0];
                var query = from player in hockey.AsEnumerable()
                            where player.Field<string>("Position") == "Fan"
                            select new
                            {
                                Name = player.Field<string>("Name")
                            };
                int count = 0;
                foreach (var item in query)
                {
                    Console.Out.Write(item.Name);
                    count++;
                }
                Assert.AreEqual(1, count);

            }
        }

        [Test]
        public void TestDisconnectedUpdate()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbDataAdapter da = new NuoDbDataAdapter("select id, number, name, position, team from hockey", connection);
                NuoDbCommandBuilder builder = new NuoDbCommandBuilder();
                builder.DataAdapter = da;
                DataTable dt = new DataTable();
                da.Fill(dt);

                DataRow row = dt.NewRow();
                row["NAME"] = "John Doe";
                row["POSITION"] = "Developer";
                row["TEAM"] = "NuoDB";
                row["NUMBER"] = 100;
                dt.Rows.Add(row);

                int changed = da.Update(dt);
                Assert.AreEqual(1, changed);

                // TODO: http://msdn.microsoft.com/en-us/library/ks9f57t0%28v=vs.80%29.aspx describes a few options
                // to retrieve the AutoNumber column. For the moment, I reload the entire table
                dt = new DataTable();
                da.Fill(dt);

                DataRow[] rows = dt.Select("NUMBER = 100");
                Assert.IsNotNull(rows);
                Assert.AreEqual(1, rows.Length);
                foreach (DataRow r in rows)
                    r["NUMBER"] = 0;
                changed = da.Update(dt);
                Assert.AreEqual(1, changed);

                rows = dt.Select("NUMBER = 0");
                Assert.IsNotNull(rows);
                Assert.AreEqual(1, rows.Length);
                foreach (DataRow r in rows)
                    r.Delete();
                changed = da.Update(dt);
                Assert.AreEqual(1, changed);

            }
        }

        public void TestDataType(string sqlType, object value)
        {
            TestDataType(sqlType, value, value);
        }

        public void TestDataType(string sqlType, object value, object expected)
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                //DbTransaction transaction = connection.BeginTransaction();

                Utils.DropTable(connection, "temp");

                DbCommand createCommand = new NuoDbCommand("create table temp (col " + sqlType + ")", connection);
                int result = createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("insert into temp (col) values (?)", connection);
                insertCommand.Parameters.Add(value);
                int inserted = insertCommand.ExecuteNonQuery();

                DbCommand command = new NuoDbCommand("select col from temp", connection);
                object val = command.ExecuteScalar();
                // compare dates using the string representation
                if (val.GetType() == expected.GetType())
                    Assert.AreEqual(expected, val);
                else if (val is DateTime)
                    Assert.AreEqual(DateTime.Parse(expected.ToString()), val);
                else if (val is TimeSpan)
                    Assert.AreEqual(TimeSpan.Parse(expected.ToString()), val);
                else if (expected is ICollection)
                    CollectionAssert.AreEqual((ICollection)expected, (ICollection)val);
                else
                    Assert.AreEqual(expected, val);

                //transaction.Rollback();
            }
        }

        [Test]
        public void TestDataTypeString()
        {
            TestDataType("string", "dummy");
            TestDataType("varchar(255)", "dummy");
            //TestDataType("longvarchar", "dummy");
            TestDataType("clob", "dummy");
        }

        [Test]
        public void TestDataTypeBoolean()
        {
            TestDataType("boolean", false);
        }

        [Test]
        public void TestDataTypeByte()
        {
            //TestDataType("tinyint", 45);
        }

        [Test]
        public void TestDataTypeInt16()
        {
            TestDataType("smallint", 45);
        }

        [Test]
        public void TestDataTypeInt32()
        {
            TestDataType("integer", 45);
            TestDataType("int", 45);
        }

        [Test]
        public void TestDataTypeInt64()
        {
            TestDataType("bigint", 45000000000);
        }

        [Test]
        public void TestDataTypeFloat()
        {
            TestDataType("real", 45.3);
            TestDataType("float", 45.3);
        }

        [Test]
        public void TestDataTypeDouble()
        {
            TestDataType("double", 45.3987654321);
        }

        [Test]
        public void TestDataTypeDecimal()
        {
            TestDataType("numeric", 45.3987654321, 45);
            TestDataType("numeric(18,12)", 45.3987654321M);
            TestDataType("numeric(18,3)", 45.3987654321, 45.399M);
            TestDataType("decimal(18,12)", 45.3987654321M);
            TestDataType("dec(18,12)", 45.3987654321M);
        }

        [Test]
        public void TestDataTypeChar()
        {
            TestDataType("char", 'A', "A");
        }

        [Test]
        public void TestDataTypeDate()
        {
            DateTime now = DateTime.Now;
            TestDataType("date", now, now.Date);
            TestDataType("date", "1999-01-31");
            TestDataType("dateonly", "1999-01-31");
        }

        [Test]
        public void TestDataTypeTime()
        {
            DateTime now = DateTime.Now;
            TestDataType("time", now, now.TimeOfDay);
            TestDataType("time", new TimeSpan(10, 30, 22));
            TestDataType("time", "10:30:22");
            TestDataType("timeonly", "10:30:22");
        }

        [Test]
        public void TestDataTypeTimestamp()
        {
            DateTime now = DateTime.Now;
            TestDataType("timestamp", now);
            TestDataType("timestamp", "1999-01-31 10:30:00.100");
            TestDataType("datetime", "1999-01-31 10:30:00.100");
        }

        [Test]
        public void TestDataTypeBlob()
        {
            TestDataType("blob", "xxx", new byte[] { (byte)'x', (byte)'x', (byte)'x' });
            TestDataType("blob", new byte[] { (byte)'x', (byte)'x', (byte)'x' }, new byte[] { (byte)'x', (byte)'x', (byte)'x' });
            TestDataType("blob", "\x00\x01\x02", new byte[] { 0, 1, 2 });
            TestDataType("blob", new byte[] { 3, 0, 2 }, new byte[] { 3, 0, 2 });
        }

        [Test]
        public void TestSchema()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DataTable tables = connection.GetSchema("Tables");

                Boolean found = false;
                foreach (DataRow row in tables.Rows)
                {
                    if (row.Field<string>(2).Equals("HOCKEY"))
                        found = true;
                }
                Assert.IsTrue(found, "Table HOCKEY was not found in the list of tables");
            }
        }

        [Test]
        public void TestScalability()
        {
            using (NuoDbConnection cnn = new NuoDbConnection(connectionString))
            {
                cnn.Open();
                Utils.DropTable(cnn, "temp");

                DbCommand createCommand = new NuoDbCommand("create table temp (col1 integer, col2 integer)", cnn);
                int result = createCommand.ExecuteNonQuery();

                DbCommand cmm = cnn.CreateCommand();
                cmm.CommandText = "insert into temp(col1, col2) values(?, ?)";
                cmm.Parameters.Add(new NuoDbParameter { DbType = DbType.Int32, ParameterName = "col1" });
                cmm.Parameters.Add(new NuoDbParameter { DbType = DbType.Int32, ParameterName = "col2" });
                cmm.Prepare();

                int[] count = new int[] { 1000, 5000, 10000, 20000, 40000 };
                double[] times = new double[count.Length];
                for (var k = 0; k < count.Length; k++)
                {
                    DateTime start = DateTime.Now;
                    for (var i = 1; i <= count[k]; i++)
                    {
                        cmm.Parameters["col1"].Value = i;
                        cmm.Parameters["col2"].Value = 2 * i;
                        cmm.ExecuteNonQuery();
                    }
                    DateTime end = DateTime.Now;
                    times[k] = (end - start).TotalMilliseconds;
                    if (k == 0)
                        Console.WriteLine("{0} runs = {1} msec", count[k], times[k]);
                    else
                    {
                        double countRatio = (count[k] / count[0]);
                        double timeRatio = (times[k] / times[0]);
                        Console.WriteLine("{0} runs = {1} msec => {2} {3}", count[k], times[k], countRatio, timeRatio);
                        Assert.IsTrue(timeRatio < (countRatio * 1.50), "Scalability at {2} rows is not linear! (time for {0} rows = {1}; time for {2} rows = {3} => ratio = {4} is greater than {5}",
                            new object[] { count[0], times[0], count[k], times[k], timeRatio, countRatio });

                    }
                }
            }
        }
        
        private static void CreateTargetForBulkLoad()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                Utils.DropTable(connection, "temp");

                DbCommand createCommand = new NuoDbCommand("create table temp (col string)", connection);
                int result = createCommand.ExecuteNonQuery();
            }
        }

        private static void VerifyBulkLoad(int expectedCount, string expectedFirstRow)
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();

                DbCommand command = new NuoDbCommand("select count(*) from temp", connection);
                object val = command.ExecuteScalar();

                Assert.AreEqual(expectedCount, val);

                command = new NuoDbCommand("select col from temp", connection);
                val = command.ExecuteScalar();

                Assert.AreEqual(expectedFirstRow, val);
            }
        }

        [Test]
        public void TestBulkLoad_DataRowsNoMapping()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz", typeof(string));
            DataRow[] rows = new DataRow[10];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = metadata.NewRow();
                rows[i][0] = Convert.ToString(i);
            }

            loader.WriteToServer(rows);

            VerifyBulkLoad(rows.Length, "0");
        }

        [Test]
        public void TestBulkLoad_DataRowsWithMappingOrdinal2Ordinal()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add(1, 0);

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            DataRow[] rows = new DataRow[10];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = metadata.NewRow();
                rows[i][0] = -1;
                rows[i][1] = Convert.ToString(i);
            }

            loader.WriteToServer(rows);

            VerifyBulkLoad(rows.Length, "0");
        }

        [Test]
        public void TestBulkLoad_DataRowsWithMappingOrdinal2Name()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add(1, "col");

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            DataRow[] rows = new DataRow[10];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = metadata.NewRow();
                rows[i][0] = -1;
                rows[i][1] = Convert.ToString(i);
            }

            loader.WriteToServer(rows);

            VerifyBulkLoad(rows.Length, "0");
        }

        [Test]
        public void TestBulkLoad_DataRowsWithMappingName2Ordinal()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add("xyz2", 0);

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            DataRow[] rows = new DataRow[10];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = metadata.NewRow();
                rows[i][0] = -1;
                rows[i][1] = Convert.ToString(i);
            }

            loader.WriteToServer(rows);

            VerifyBulkLoad(rows.Length, "0");
        }

        [Test]
        public void TestBulkLoad_DataRowsWithMappingName2Name()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add("xyz2", "col");

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            DataRow[] rows = new DataRow[10];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = metadata.NewRow();
                rows[i][0] = -1;
                rows[i][1] = Convert.ToString(i);
            }

            loader.WriteToServer(rows);

            VerifyBulkLoad(rows.Length, "0");
        }

        [Test]
        public void TestBulkLoad_DataTableWithStateNoMapping()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz", typeof(string));
            const int ROW_TO_ADD = 10;
            metadata.BeginLoadData();
            for (int i = 0; i < ROW_TO_ADD; i++)
            {
                DataRow row = metadata.NewRow();
                row[0] = Convert.ToString(i);
                metadata.Rows.Add(row);
            }
            metadata.EndLoadData();
            metadata.AcceptChanges();
            metadata.Rows[ROW_TO_ADD / 2].BeginEdit();
            metadata.Rows[ROW_TO_ADD / 2][0] = "999";
            metadata.Rows[ROW_TO_ADD / 2].EndEdit();

            loader.WriteToServer(metadata, DataRowState.Modified);

            VerifyBulkLoad(1, "999");
        }

        [Test]
        public void TestBulkLoad_DataTableNoMapping()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz", typeof(string));
            const int ROW_TO_ADD = 10;
            for (int i = 0; i < ROW_TO_ADD; i++)
            {
                DataRow row = metadata.NewRow();
                row[0] = Convert.ToString(i);
                metadata.Rows.Add(row);
            }

            loader.WriteToServer(metadata);

            VerifyBulkLoad(ROW_TO_ADD, "0");
        }

        [Test]
        public void TestBulkLoad_DataTableWithMappingOrdinal2Ordinal()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add(1, 0);

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            const int ROW_TO_ADD = 10;
            for (int i = 0; i < ROW_TO_ADD; i++)
            {
                DataRow row = metadata.NewRow();
                row[0] = -1;
                row[1] = Convert.ToString(i);
                metadata.Rows.Add(row);
            }

            loader.WriteToServer(metadata);

            VerifyBulkLoad(ROW_TO_ADD, "0");
        }

        [Test]
        public void TestBulkLoad_DataTableWithMappingOrdinal2Name()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add(1, "col");

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            const int ROW_TO_ADD = 10;
            for (int i = 0; i < ROW_TO_ADD; i++)
            {
                DataRow row = metadata.NewRow();
                row[0] = -1;
                row[1] = Convert.ToString(i);
                metadata.Rows.Add(row);
            }

            loader.WriteToServer(metadata);

            VerifyBulkLoad(ROW_TO_ADD, "0");
        }

        [Test]
        public void TestBulkLoad_DataTableWithMappingName2Ordinal()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add("xyz2", 0);

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            const int ROW_TO_ADD = 10;
            for (int i = 0; i < ROW_TO_ADD; i++)
            {
                DataRow row = metadata.NewRow();
                row[0] = -1;
                row[1] = Convert.ToString(i);
                metadata.Rows.Add(row);
            }

            loader.WriteToServer(metadata);

            VerifyBulkLoad(ROW_TO_ADD, "0");
        }

        [Test]
        public void TestBulkLoad_DataTableWithMappingName2Name()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add("xyz2", "col");

            DataTable metadata = new DataTable("dummy");
            metadata.Columns.Add("xyz1", typeof(int));
            metadata.Columns.Add("xyz2", typeof(string));
            const int ROW_TO_ADD = 10;
            for (int i = 0; i < ROW_TO_ADD; i++)
            {
                DataRow row = metadata.NewRow();
                row[0] = -1;
                row[1] = Convert.ToString(i);
                metadata.Rows.Add(row);
            }

            loader.WriteToServer(metadata);

            VerifyBulkLoad(ROW_TO_ADD, "0");
        }

        [Test]
        public void TestBulkLoad_DataReaderNoMapping()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";

            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select position from hockey order by number", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                loader.WriteToServer(reader);
                reader.Close();

                command = new NuoDbCommand("select count(*) from hockey", connection);
                object val = command.ExecuteScalar();

                VerifyBulkLoad((int)val, "Fan");
            }
        }

        [Test]
        public void TestBulkLoad_DataReaderWithMappingOrdinal2Ordinal()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add(1, 0);

            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select number, position as xyz2 from hockey order by number", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                loader.WriteToServer(reader);
                reader.Close();

                command = new NuoDbCommand("select count(*) from hockey", connection);
                object val = command.ExecuteScalar();

                VerifyBulkLoad((int)val, "Fan");
            }

        }

        [Test]
        public void TestBulkLoad_DataReaderWithMappingOrdinal2Name()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add(1, "col");

            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select number, position as xyz2 from hockey order by number", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                loader.WriteToServer(reader);
                reader.Close();

                command = new NuoDbCommand("select count(*) from hockey", connection);
                object val = command.ExecuteScalar();

                VerifyBulkLoad((int)val, "Fan");
            }

        }

        [Test]
        public void TestBulkLoad_DataReaderWithMappingName2Ordinal()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add("xyz2", 0);

            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select number, position as xyz2 from hockey order by number", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                loader.WriteToServer(reader);
                reader.Close();

                command = new NuoDbCommand("select count(*) from hockey", connection);
                object val = command.ExecuteScalar();

                VerifyBulkLoad((int)val, "Fan");
            }
        }

        [Test]
        public void TestBulkLoad_DataReaderWithMappingName2Name()
        {
            CreateTargetForBulkLoad();

            NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
            loader.BatchSize = 2;
            loader.DestinationTableName = "TEMP";
            loader.ColumnMappings.Add("xyz2", "col");

            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                DbCommand command = new NuoDbCommand("select number, position as xyz2 from hockey order by number", connection);

                connection.Open();
                DbDataReader reader = command.ExecuteReader();
                loader.WriteToServer(reader);
                reader.Close();

                command = new NuoDbCommand("select count(*) from hockey", connection);
                object val = command.ExecuteScalar();

                VerifyBulkLoad((int)val, "Fan");
            }
        }

        [Test]
        public void TestBulkLoadPerformance()
        {
            using (NuoDbConnection cnn = new NuoDbConnection(connectionString))
            {
                cnn.Open();
                Utils.DropTable(cnn, "temp");

                DbCommand createCommand = new NuoDbCommand("create table temp (col1 integer, col2 integer)", cnn);
                int result = createCommand.ExecuteNonQuery();

                DbCommand cmm = cnn.CreateCommand();
                cmm.CommandText = "insert into temp(col1, col2) values(?, ?)";
                cmm.Parameters.Add(new NuoDbParameter { DbType = DbType.Int32, ParameterName = "col1" });
                cmm.Parameters.Add(new NuoDbParameter { DbType = DbType.Int32, ParameterName = "col2" });
                cmm.Prepare();

                const int ROW_NUMBER = 40000;
                DateTime start = DateTime.Now;
                for (var i = 1; i <= ROW_NUMBER; i++)
                {
                    cmm.Parameters["col1"].Value = i;
                    cmm.Parameters["col2"].Value = 2 * i;
                    cmm.ExecuteNonQuery();
                }
                DateTime end = DateTime.Now;
                double insertTime = (end - start).TotalMilliseconds;

                Utils.DropTable(cnn, "temp2");
                createCommand = new NuoDbCommand("create table temp2 (col1 integer, col2 integer)", cnn);
                createCommand.ExecuteNonQuery();

                NuoDbBulkLoader loader = new NuoDbBulkLoader(connectionString);
                loader.DestinationTableName = "TEMP2";

                DbCommand command = new NuoDbCommand("select * from temp", cnn);
                DbDataReader reader = command.ExecuteReader();

                loader.BatchProcessed += new BatchProcessedEventHandler(loader_BatchProcessed);
                start = DateTime.Now;
                loader.WriteToServer(reader);
                end = DateTime.Now;

                double loadTime = (end - start).TotalMilliseconds;

                reader.Close();

                Console.WriteLine("{0} insert = {1}\n{0} bulk load = {2}\n", ROW_NUMBER, insertTime, loadTime);

                Assert.IsTrue(loadTime < insertTime, "BulkLoad takes more time than manual insertion");
            }
        }

        void loader_BatchProcessed(object sender, NuoDb.Data.Client.BatchProcessedEventArgs e)
        {
            Console.WriteLine("Batch of {0} rows inserted, current count is {1}\n", e.BatchSize, e.TotalSize);
        }

        [Test]
        public void TestConnectionPooling()
        {
            NuoDbConnectionStringBuilder builder = new NuoDbConnectionStringBuilder(connectionString);
            builder.Pooling = true;
            builder.ConnectionLifetime = 2;
            String newConnString = builder.ConnectionString;
            int pooledItems = 0;
            NuoDbConnection.ClearAllPools();
            using (NuoDbConnection cnn = new NuoDbConnection(newConnString))
            {
                cnn.Open();

                // 1 busy
                pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                Assert.AreEqual(1, pooledItems);

                using (NuoDbConnection cnn2 = new NuoDbConnection(newConnString))
                {
                    cnn2.Open();

                    // 2 busy
                    pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                    Assert.AreEqual(2, pooledItems);
                }

                // 1 available, 1 busy
                pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                Assert.AreEqual(2, pooledItems);

                Thread.Sleep(3000);

                // 1 busy
                pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                Assert.AreEqual(1, pooledItems);
            }

            // 1 available
            pooledItems = NuoDbConnection.GetPooledConnectionCount(newConnString);
            Assert.AreEqual(1, pooledItems);

            using (NuoDbConnection cnn = new NuoDbConnection(newConnString))
            {
                cnn.Open();

                // 1 busy
                pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                Assert.AreEqual(1, pooledItems);
            }

            // 1 available
            pooledItems = NuoDbConnection.GetPooledConnectionCount(newConnString);
            Assert.AreEqual(1, pooledItems);

            Thread.Sleep(3000);

            // empty pool
            pooledItems = NuoDbConnection.GetPooledConnectionCount(newConnString);
            Assert.AreEqual(0, pooledItems);
        }

        [Test]
        public void TestConnectionPoolingMaxAge()
        {
            NuoDbConnectionStringBuilder builder = new NuoDbConnectionStringBuilder(connectionString);
            builder.Pooling = true;
            builder.ConnectionLifetime = 2;
            builder.MaxLifetime = 3;
            String newConnString = builder.ConnectionString;
            int pooledItems = 0;
            NuoDbConnection.ClearAllPools();
            using (NuoDbConnection cnn = new NuoDbConnection(newConnString))
            {
                cnn.Open();

                // 1 busy
                pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                Assert.AreEqual(1, pooledItems);

                Thread.Sleep(2000);
            }

            // 1 available
            pooledItems = NuoDbConnection.GetPooledConnectionCount(newConnString);
            Assert.AreEqual(1, pooledItems);

            using (NuoDbConnection cnn = new NuoDbConnection(newConnString))
            {
                cnn.Open();

                // 1 busy
                pooledItems = NuoDbConnection.GetPooledConnectionCount(cnn);
                Assert.AreEqual(1, pooledItems);

                Thread.Sleep(2000);
            }

            // 0 available, the connection is too old to be recycled
            pooledItems = NuoDbConnection.GetPooledConnectionCount(newConnString);
            Assert.AreEqual(0, pooledItems);
        }

        [Test]
        public void TestAsynchronousReader1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                NuoDbCommand command = new NuoDbCommand("select * from hockey", connection);

                connection.Open();
                IAsyncResult result = command.BeginExecuteReader();

                using (DbDataReader reader = command.EndExecuteReader(result))
                {
                    while (reader.Read())
                    {
                        Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                    }
                }
            }
        }

        [Test]
        public void TestAsynchronousReader2()
        {
            NuoDbConnection connection = new NuoDbConnection(connectionString);
            NuoDbCommand command = new NuoDbCommand("select * from hockey", connection);

            connection.Open();

            AsyncCallback callback = new AsyncCallback(HandleCallback);
            IAsyncResult result = command.BeginExecuteReader(callback, command);
        }

        void HandleCallback(IAsyncResult result)
        {
            NuoDbCommand command = (NuoDbCommand)result.AsyncState;
            using (DbDataReader reader = command.EndExecuteReader(result))
            {
                while (reader.Read())
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", reader[0], reader[1], reader[2], reader["id"]);
                }
            }
            command.Close();
            command.Connection.Close();
        }

        [Test]
        public void TestAsynchronousScalar1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                NuoDbCommand countCommand = (NuoDbCommand)connection.CreateCommand();
                countCommand.CommandText = "select count(*) from hockey";

                connection.Open();

                IAsyncResult result = countCommand.BeginExecuteScalar();

                int count = (int)countCommand.EndExecuteScalar(result);
            }
        }

        [Test]
        public void TestAsynchronousScalar2()
        {
            NuoDbConnection connection = new NuoDbConnection(connectionString);
            NuoDbCommand countCommand = (NuoDbCommand)connection.CreateCommand();
            countCommand.CommandText = "select count(*) from hockey";

            connection.Open();

            AsyncCallback callback = new AsyncCallback(HandleCallback2);
            IAsyncResult result = countCommand.BeginExecuteScalar(callback, countCommand);
        }

        [Test]
        public void TestAsynchronousScalar3()
        {
            NuoDbConnection connection = new NuoDbConnection(connectionString);
            NuoDbCommand countCommand = (NuoDbCommand)connection.CreateCommand();
            countCommand.CommandText = "select count(*) from hockey";

            connection.Open();
            AsyncCallback callback = new AsyncCallback(HandleCallback2);
            for(int i=0;i<20;i++)
                countCommand.BeginExecuteScalar(callback, countCommand);
        }

        void HandleCallback2(IAsyncResult result)
        {
            NuoDbCommand command = (NuoDbCommand)result.AsyncState;
            int count = (int)command.EndExecuteScalar(result);
        }

        [Test]
        public void TestAsynchronousUpdate1()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString))
            {
                connection.Open();
                Utils.DropTable(connection, "temp");

                NuoDbCommand createCommand = new NuoDbCommand("create table temp (col string)", connection);
                IAsyncResult result = createCommand.BeginExecuteNonQuery();

                int count = createCommand.EndExecuteNonQuery(result);
            }
        }


        [Test]
        public void TestAsynchronousUpdate2()
        {
            NuoDbConnection connection = new NuoDbConnection(connectionString);
            connection.Open();
            Utils.DropTable(connection, "temp");

            NuoDbCommand countCommand = (NuoDbCommand)connection.CreateCommand();
            countCommand.CommandText = "create table temp (col string)";

            AsyncCallback callback = new AsyncCallback(HandleCallback3);
            IAsyncResult result = countCommand.BeginExecuteNonQuery(callback, countCommand);
        }

        void HandleCallback3(IAsyncResult result)
        {
            NuoDbCommand command = (NuoDbCommand)result.AsyncState;
            int count = command.EndExecuteNonQuery(result);

            Utils.DropTable(command.Connection as NuoDbConnection, "temp");
            command.Close();
            command.Connection.Close();
        }

        [Test]
        public void TestTimeZone()
        {
            // Use a time in the UTC time zone; otherwise, it would be treated as if it were in the local timezone even
            // if we are telling NuoDB that we are in a different timezone
            DateTime dstReferenceDate = DateTime.SpecifyKind(new DateTime(1999, 10, 1, 2, 30, 58), DateTimeKind.Utc);
            DateTime nonDstReferenceDate = DateTime.SpecifyKind(new DateTime(1999, 12, 1, 2, 30, 58), DateTimeKind.Utc);
            DateTime dtDate;
            string strDate;
            bool hasNext;
            // GMT-5, or GMT-4 if DST is active
            using (NuoDbConnection connection = new NuoDbConnection(connectionString + ";TimeZone=America/New_York"))
            {
                connection.Open();
                Utils.DropTable(connection, "timezone");

                DbCommand createCommand = new NuoDbCommand("create table timezone (asTimestamp timestamp, asDate date, asTime time, asString string)", connection);
                int result = createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("insert into timezone (asTimestamp, asDate, asTime, asString) values (?,?,?,?)", connection);
                insertCommand.Parameters.Add(dstReferenceDate);
                insertCommand.Parameters.Add(dstReferenceDate);
                insertCommand.Parameters.Add(dstReferenceDate);
                insertCommand.Parameters.Add(dstReferenceDate);
                insertCommand.ExecuteNonQuery();
                insertCommand.Parameters.Clear();
                insertCommand.Parameters.Add(nonDstReferenceDate);
                insertCommand.Parameters.Add(nonDstReferenceDate);
                insertCommand.Parameters.Add(nonDstReferenceDate);
                insertCommand.Parameters.Add(nonDstReferenceDate);
                insertCommand.ExecuteNonQuery();

                DbCommand command = new NuoDbCommand("select asTimestamp, asDate, asTime, asString from timezone", connection);
                DbDataReader reader = command.ExecuteReader();
                hasNext = reader.Read();
                Assert.IsTrue(hasNext);
                dtDate = reader.GetDateTime(0);
                Assert.AreEqual("1999-09-30 22:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(0);
                Assert.AreEqual("1999-09-30 22:30:58", strDate);
                dtDate = reader.GetDateTime(1);
                Assert.AreEqual("1999-09-30", dtDate.ToString("yyyy-MM-dd"));
                strDate = reader.GetString(1);
                Assert.AreEqual("1999-09-30", strDate);
                dtDate = reader.GetDateTime(2);
                Assert.AreEqual("22:30:58", dtDate.ToString("HH:mm:ss"));
                strDate = reader.GetString(2);
                Assert.AreEqual("22:30:58", strDate);
                dtDate = reader.GetDateTime(3);
                Assert.AreEqual("1999-09-30 22:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(3);
                Assert.AreEqual("1999-09-30 22:30:58", strDate);

                hasNext = reader.Read();
                Assert.IsTrue(hasNext);
                dtDate = reader.GetDateTime(0);
                Assert.AreEqual("1999-11-30 21:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(0);
                Assert.AreEqual("1999-11-30 21:30:58", strDate);
                dtDate = reader.GetDateTime(1);
                Assert.AreEqual("1999-11-30", dtDate.ToString("yyyy-MM-dd"));
                strDate = reader.GetString(1);
                Assert.AreEqual("1999-11-30", strDate);
                dtDate = reader.GetDateTime(2);
                Assert.AreEqual("21:30:58", dtDate.ToString("HH:mm:ss"));
                strDate = reader.GetString(2);
                Assert.AreEqual("21:30:58", strDate);
                dtDate = reader.GetDateTime(3);
                Assert.AreEqual("1999-11-30 21:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(3);
                Assert.AreEqual("1999-11-30 21:30:58", strDate);
            }
            // all the date-based columns should magically move one hour back when we change timezone
            // GMT-6, or GMT-5 if DST is active
            using (NuoDbConnection connection = new NuoDbConnection(connectionString + ";TimeZone=America/Chicago"))
            {
                connection.Open();
                DbCommand command = new NuoDbCommand("select asTimestamp, asDate, asTime, asString from timezone", connection);
                DbDataReader reader = command.ExecuteReader();
                hasNext = reader.Read();
                Assert.IsTrue(hasNext);
                dtDate = reader.GetDateTime(0);
                Assert.AreEqual("1999-09-30 21:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(0);
                Assert.AreEqual("1999-09-30 21:30:58", strDate);
                dtDate = reader.GetDateTime(1);
                Assert.AreEqual("1999-09-30", dtDate.ToString("yyyy-MM-dd"));
                strDate = reader.GetString(1);
                Assert.AreEqual("1999-09-30", strDate);
                dtDate = reader.GetDateTime(2);
                Assert.AreEqual("21:30:58", dtDate.ToString("HH:mm:ss"));
                strDate = reader.GetString(2);
                Assert.AreEqual("21:30:58", strDate);
                dtDate = reader.GetDateTime(3);
                Assert.AreEqual("1999-09-30 22:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(3);
                Assert.AreEqual("1999-09-30 22:30:58", strDate);

                hasNext = reader.Read();
                Assert.IsTrue(hasNext);
                dtDate = reader.GetDateTime(0);
                Assert.AreEqual("1999-11-30 20:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(0);
                Assert.AreEqual("1999-11-30 20:30:58", strDate);
                dtDate = reader.GetDateTime(1);
                Assert.AreEqual("1999-11-30", dtDate.ToString("yyyy-MM-dd"));
                strDate = reader.GetString(1);
                Assert.AreEqual("1999-11-30", strDate);
                dtDate = reader.GetDateTime(2);
                Assert.AreEqual("20:30:58", dtDate.ToString("HH:mm:ss"));
                strDate = reader.GetString(2);
                Assert.AreEqual("20:30:58", strDate);
                dtDate = reader.GetDateTime(3);
                Assert.AreEqual("1999-11-30 21:30:58", dtDate.ToString("yyyy-MM-dd HH:mm:ss"));
                strDate = reader.GetString(3);
                Assert.AreEqual("1999-11-30 21:30:58", strDate);
            }
        }
    }
}
