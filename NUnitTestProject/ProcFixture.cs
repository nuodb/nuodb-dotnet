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
    }
}
