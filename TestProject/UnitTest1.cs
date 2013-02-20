using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuoDb.Data.Client;
using System.Data.Common;
using System.Data;
using System.Collections.Specialized;
using System.Collections;

namespace TestProject
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        static string host = "localhost:48004";
        static string user = "dba";
        static string password = "goalie";
        static string database = "test";
        static string schema = "hockey";
        static internal string connectionString = "Server=  " + host + "; Database=\"" + database + "\"; User = " + user + " ;Password   = '" + password + "';Schema=\"" + schema + "\";Something Else";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestHighAvailability()
        {
            using (NuoDbConnection connection = new NuoDbConnection(connectionString.Replace("Server=","Server=localhost:8,")))
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

                transaction.Rollback();
            }
        }

        [TestMethod]
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

                transaction.Rollback();
            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

                try
                {
                    DbCommand dropCommand = new NuoDbCommand("drop table temp", connection);
                    dropCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // table is allowed to be missing
                }

                DbCommand createCommand = new NuoDbCommand("create table temp (col "+sqlType+")", connection);
                int result = createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("insert into temp (col) values (?)", connection);
                insertCommand.Parameters.Add(value);
                int inserted = insertCommand.ExecuteNonQuery();

                DbCommand command = new NuoDbCommand("select col from temp", connection);
                object val = command.ExecuteScalar();
                // compare dates using the string representation
                if(val is DateTime)
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

        [TestMethod]
        public void TestDataTypeString()
        {
            TestDataType("string", "dummy");
            TestDataType("varchar(255)", "dummy");
            //TestDataType("longvarchar", "dummy");
            TestDataType("clob", "dummy");
        }

        [TestMethod]
        public void TestDataTypeBoolean()
        {
            TestDataType("boolean", false);
        }

        [TestMethod]
        public void TestDataTypeByte()
        {
            //TestDataType("tinyint", 45);
        }

        [TestMethod]
        public void TestDataTypeInt16()
        {
            TestDataType("smallint", 45);
        }

        [TestMethod]
        public void TestDataTypeInt32()
        {
            TestDataType("integer", 45);
            TestDataType("int", 45);
        }

        [TestMethod]
        public void TestDataTypeInt64()
        {
            TestDataType("bigint", 45000000000);
        }

        [TestMethod]
        public void TestDataTypeFloat()
        {
            TestDataType("real", 45.3);
            TestDataType("float", 45.3);
        }

        [TestMethod]
        public void TestDataTypeDouble()
        {
            TestDataType("double", 45.3987654321);
        }

        [TestMethod]
        public void TestDataTypeDecimal()
        {
            TestDataType("numeric", 45.3987654321, 45);
            TestDataType("numeric(18,12)", 45.3987654321M);
            TestDataType("numeric(18,3)", 45.3987654321, 45.399M);
            TestDataType("decimal(18,12)", 45.3987654321M);
            TestDataType("dec(18,12)", 45.3987654321M);
        }

        [TestMethod]
        public void TestDataTypeChar()
        {
            TestDataType("char", 'A', "A");
        }

        [TestMethod]
        public void TestDataTypeDate()
        {
            DateTime now = DateTime.Now;
            TestDataType("date", now);
            TestDataType("date", "1999-01-31");
            TestDataType("dateonly", "1999-01-31");
        }

        [TestMethod]
        public void TestDataTypeTime()
        {
            TestDataType("time", new TimeSpan(10, 30, 22));
            TestDataType("time", "10:30:22");
            TestDataType("timeonly", "10:30:22");
        }

        [TestMethod]
        public void TestDataTypeTimestamp()
        {
            TestDataType("timestamp", "1999-01-31 10:30:00.100");
            TestDataType("datetime", "1999-01-31 10:30:00.100");
        }

        [TestMethod]
        public void TestDataTypeBlob()
        {
            TestDataType("blob", "xxx", new byte[] { (byte)'x', (byte)'x', (byte)'x' });
        }

        [TestMethod]
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

    }
}
