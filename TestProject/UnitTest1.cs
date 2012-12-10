using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.NuoDB;
using System.Data.Common;
using System.Data;
using System.Collections.Specialized;

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
        private const string host = "localhost:48004";
        private const string user = "dba";
        private const string password = "admin";
        private const string database = "test";
        private const string schema = "hockey";
        private string connectionString = "Server=  "+host+"; Database=\""+database+"\"; User = "+user+" ;Password   = '"+password+"';Schema=\""+schema+"\";Something Else";

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
            using (NuoDBConnection connection = new NuoDBConnection(connectionString.Replace("Server=","Server=localhost:8,")))
            {
                DbCommand command = new NuoDBCommand("select * from hockey", connection);

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
            using (NuoDBConnection connection = new NuoDBConnection(connectionString))
            {
                DbCommand command = new NuoDBCommand("select * from hockey", connection);

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
            using (NuoDBConnection connection = new NuoDBConnection(connectionString))
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
            using (NuoDBConnection connection = new NuoDBConnection(connectionString))
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
            using (NuoDBConnection connection = new NuoDBConnection(connectionString))
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
        public void TestDbProvider()
        {
            DbProviderFactory factory = new NuoDBProviderFactory();
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


        /*
                [TestMethod]
                public void TestDisconnected()
                {
                    using (NuoDBConnection connection = new NuoDBConnection(connectionString))
                    {
                        DataAdapter da = new NuoDBDataAdapter("select * from hockey", conn);
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
                    }
                }
        */
    }
}
