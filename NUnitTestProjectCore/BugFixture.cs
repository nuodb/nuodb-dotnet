using System;
using NUnit.Framework;
using NuoDb.Data.Client;
using System.Data.Common;
using System.Data;
using System.Collections;

namespace NUnitTestProject
{
    [TestFixture]
    public class BugFixture1
    {
        [SetUp]
        public static void Init()
        {
        }

        [Test]
        public void DB4329()
        {
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                connection.Open();
                Utils.DropTable(connection, "ExpenseTest");
                DbCommand createCommand = new NuoDbCommand("Create table ExpenseTest" +
                                                           "(" +
                                                           "SourceExpenseId int," +
                                                           "ExpenseAmount numeric(15,2)" +
                                                           ")", connection);
                createCommand.ExecuteNonQuery();

                DbCommand insertCommand = new NuoDbCommand("Insert Into ExpenseTest(SourceExpenseId, ExpenseAmount) Values (?,?)", connection);
                insertCommand.Prepare();

                insertCommand.Parameters[0].Value = -1254524;
                insertCommand.Parameters[1].Value = -135.35;
                insertCommand.ExecuteNonQuery();

                insertCommand.Parameters[0].Value = 100100100;
                insertCommand.Parameters[1].Value = -1325465.35;
                insertCommand.ExecuteNonQuery();

                insertCommand.Parameters[0].Value = 100100101;
                insertCommand.Parameters[1].Value = 200000.35;
                insertCommand.ExecuteNonQuery();

                DbCommand selectCommand = new NuoDbCommand("select SourceExpenseId, ExpenseAmount from ExpenseTest", connection);
                using (DbDataReader reader = selectCommand.ExecuteReader())
                {
                    bool hasNext=reader.Read();
                    Assert.IsTrue(hasNext);
                    Assert.AreEqual(-1254524, reader[0]);
                    Assert.AreEqual(-135.35, reader[1]);
                    hasNext = reader.Read();
                    Assert.IsTrue(hasNext);
                    Assert.AreEqual(100100100, reader[0]);
                    Assert.AreEqual(-1325465.35, reader[1]);
                    hasNext = reader.Read();
                    Assert.IsTrue(hasNext);
                    Assert.AreEqual(100100101, reader[0]);
                    Assert.AreEqual(200000.35, reader[1]);
                }

            }
        }
    }
}