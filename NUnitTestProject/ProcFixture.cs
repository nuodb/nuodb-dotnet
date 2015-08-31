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
    public class ProcFixture1
    {
        [TestFixtureSetUp]
        public static void Init()
        {
        }

        [Test]
        public void TestPrepareParamIn()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(in p1 string) as throw p1; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Prepare();
                Assert.IsTrue(cmd.Parameters.Contains("p1"));
                Assert.AreEqual(ParameterDirection.Input, cmd.Parameters["p1"].Direction);
                cmd.Parameters["p1"].Value = "hello";
                try
                {
                    cmd.ExecuteNonQuery();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("hello", e.Message);
                }
            }
        }

        [Test]
        public void TestNoPrepareAnonymousParamIn()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(in p1 string) as throw p1; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("hello");
                try
                {
                    cmd.ExecuteNonQuery();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("hello", e.Message);
                }
            }
        }

        [Test]
        public void TestNoPrepareNamedParamIn()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(in p1 string, in p2 string) as throw p1; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                NuoDbParameter param1 = new NuoDbParameter();
                param1.ParameterName = "p2";
                param1.Direction = ParameterDirection.Input;
                param1.Value = "goodbye";
                cmd.Parameters.Add(param1);
                NuoDbParameter param2 = new NuoDbParameter();
                param2.ParameterName = "p1";
                param2.Direction = ParameterDirection.Input;
                param2.Value = "hello";
                cmd.Parameters.Add(param2);
                try
                {
                    cmd.ExecuteNonQuery();
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.AreEqual("hello", e.Message);
                }
            }
        }

        [Test]
        public void TestPrepareParamOut()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(out p1 string) as p1='hello'; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Prepare();
                Assert.IsTrue(cmd.Parameters.Contains("p1"));
                Assert.AreEqual(ParameterDirection.Output, cmd.Parameters["p1"].Direction);
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello", cmd.Parameters["p1"].Value);
            }
        }

        [Test]
        public void TestNoPrepareAnonymousParamOut()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(out p1 string) as p1='hello'; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello", cmd.Parameters["p1"].Value);
            }
        }

        [Test]
        public void TestNoPrepareNamedParamOut()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(out p1 string, out p2 string) as p1='hello'; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                NuoDbParameter param1 = new NuoDbParameter();
                param1.ParameterName = "p2";
                param1.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param1);
                NuoDbParameter param2 = new NuoDbParameter();
                param2.ParameterName = "p1";
                param2.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param2);
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello", cmd.Parameters["p1"].Value);
            }
        }

        [Test]
        public void TestPrepareParamInOut()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(inout p1 string) as if(p1='goodbye') p1='hello'; end_if; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Prepare();
                Assert.IsTrue(cmd.Parameters.Contains("p1"));
                Assert.AreEqual(ParameterDirection.InputOutput, cmd.Parameters["p1"].Direction);
                cmd.Parameters["p1"].Value = "goodbye";
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello", cmd.Parameters["p1"].Value);
            }
        }

        [Test]
        public void TestNoPrepareAnonymousParamInOut()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(inout p1 string) as if(p1='goodbye') p1='hello'; end_if; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("goodbye");
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello", cmd.Parameters["p1"].Value);
            }
        }

        [Test]
        public void TestNoPrepareNamedParamInOut()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(inout p1 string, out p2 string) as if(p1='goodbye') p1='hello'; end_if; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                NuoDbParameter param1 = new NuoDbParameter();
                param1.ParameterName = "p2";
                param1.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param1);
                NuoDbParameter param2 = new NuoDbParameter();
                param2.ParameterName = "p1";
                param2.Direction = ParameterDirection.InputOutput;
                param2.Value = "goodbye";
                cmd.Parameters.Add(param2);
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello", cmd.Parameters["p1"].Value);
            }
        }

        [Test]
        public void TestReader()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(in p1 integer) returns table(id integer, value string) as var i = 0; while(i<p1) insert into table values (i, 'xx'); i = i+1; end_while; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(10);
                int n = 0;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(n++, reader[0]);
                        Assert.AreEqual("xx", reader[1]);
                    }
                    Assert.AreEqual(10, n);
                }
            }
        }

        [Test]
        public void TestReaderNoStoredProcedure()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(in p1 integer) returns table(id integer, value string) as var i = 0; while(i<p1) insert into table values (i, 'xx'); i = i+1; end_while; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("call nunit_test(10)", connection);
                int n = 0;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(n++, reader[0]);
                        Assert.AreEqual("xx", reader[1]);
                    }
                    Assert.AreEqual(10, n);
                }
            }
        }

        [Test]
        public void TestReaderNoStoredProcedureWithParam()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(in p1 integer) returns table(id integer, value string) as var i = 0; while(i<p1) insert into table values (i, 'xx'); i = i+1; end_while; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("call nunit_test(?)", connection);
                cmd.Parameters.Add(10);
                int n = 0;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(n++, reader[0]);
                        Assert.AreEqual("xx", reader[1]);
                    }
                    Assert.AreEqual(10, n);
                }
            }
        }

        [Test]
        public void TestPrepareLotsOfParams()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();
                new NuoDbCommand("create procedure nunit_test(inout p1 string, in p2 string, out p3 int, inout p4 float, in p5 double, out p6 boolean, "+
                    "inout p7 string, in p8 string, out p9 int, inout p10 float, in p11 double, out p12 boolean, "+
                    "inout p13 string, in p14 string, out p15 int, inout p16 float, in p17 double, out p18 boolean, "+
                    "inout p19 string, in p20 string, out p21 int, inout p22 float, in p23 double, out p24 boolean) "+
                    " returns output(p00 string, p01 int, p02 float, p04 double, p05 boolean, p06 blob) "+
                    " as if(p1='goodbye') p1='hello'; end_if; end_procedure", connection).ExecuteNonQuery();

                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Prepare();
                int index = 1;
                foreach (DbParameter param in cmd.Parameters)
                {
                    Assert.AreEqual(String.Format("P{0}", index++), param.ParameterName);
                }
                Assert.AreEqual(25, index);
            }
        }

        [Test]
        public void TestMultipleReturnResultSets()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();

                try
                {
                    new NuoDbCommand("create procedure nunit_test() " +
                        " returns table t1(field1 string, field2 integer), t2(column1 string, column2 string, column3 integer) " +
                        " as " +
                        "   insert into t1 values ('rset 1, row1', 0), ('rset 1, row2',1); " +
                        "   insert into t2 values ('rset 2, row1', 'first', 100), ('rset 2, row2','second', 101); " +
                        " end_procedure", connection).ExecuteNonQuery();
                }
                catch (NuoDbSqlException e)
                {
                    if (e.Code.Code == -1)
                    {
                        // the server doesn't support multiple result sets as return value for procedures
                        return;
                    }
                    else
                        throw;
                }
                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("rset 1, row1", reader["field1"]);
                    Assert.AreEqual(0, reader["field2"]);
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("rset 1, row2", reader["field1"]);
                    Assert.AreEqual(1, reader["field2"]);
                    Assert.IsFalse(reader.Read());

                    Assert.IsTrue(reader.NextResult());
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("rset 2, row1", reader["column1"]);
                    Assert.AreEqual("first", reader["column2"]);
                    Assert.AreEqual(100, reader["column3"]);
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("rset 2, row2", reader["column1"]);
                    Assert.AreEqual("second", reader["column2"]);
                    Assert.AreEqual(101, reader["column3"]);
                    Assert.IsFalse(reader.Read());

                    Assert.IsFalse(reader.NextResult());
                }
            }
        }

        [Test]
        public void TestTableValuedArgument()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                new NuoDbCommand("drop procedure nunit_test if exists", connection).ExecuteNonQuery();

                try
                {
                    new NuoDbCommand("create procedure nunit_test(input_data(field1 string, field2 integer), out output_data string) " +
                        " as " +
                        "   output_data = ''; " +
                        "   for select field1 from input_data; " +
                        "     output_data = output_data || field1 || ' '; " +
                        "   end_for; " +
                        " end_procedure", connection).ExecuteNonQuery();
                }
                catch (NuoDbSqlException e)
                {
                    if (e.Code.Code == -1)
                    {
                        // the server doesn't support table valued arguments for procedures
                        return;
                    }
                    else
                        throw;
                }
                NuoDbCommand cmd = new NuoDbCommand("nunit_test", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Prepare();
                DataTable table = new DataTable();
                table.Columns.Add("f1", typeof(string));
                table.Columns.Add("f2", typeof(int));
                DataRow row1 = table.NewRow();
                row1[0] = "hello";
                row1[1] = 0;
                table.Rows.Add(row1);
                DataRow row2 = table.NewRow();
                row2[0] = "world!";
                row2[1] = 0;
                table.Rows.Add(row2);
                cmd.Parameters[0].Value = table;
                cmd.ExecuteNonQuery();
                Assert.AreEqual("hello world! ", cmd.Parameters[1].Value);
            }
        }
    }
}
