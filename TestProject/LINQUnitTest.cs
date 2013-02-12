using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using NuoDb.Data.Client;

namespace TestProject
{
    /// <summary>
    /// Summary description for LINQUnitTest
    /// </summary>
    [TestClass]
    public class LINQUnitTest
    {
        testEntities context;
        int tableRows = 0;

        public LINQUnitTest()
        {
            context = new testEntities();
            try
            {
                using (NuoDbConnection connection = new NuoDbConnection(UnitTest1.connectionString))
                {
                    DbCommand command = new NuoDbCommand("select count(*) from hockey", connection);

                    connection.Open();
                    tableRows = (int)command.ExecuteScalar();
                }
            }
            catch (Exception)
            {
            }
        }

        private TestContext testContextInstance;

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
        public void LINQTestWhere1()
        {
            var lowNums = from p in context.HOCKEY
                          where p.NUMBER < 5
                          select p;

            int count = 0;
            Console.WriteLine("Number < 5:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestWhere2()
        {
            var lowNums = from p in context.HOCKEY
                          where p.NUMBER == 1
                          select p;

            int count = 0;
            Console.WriteLine("Number == 1:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestWhere3()
        {
            var lowNums = from p in context.HOCKEY
                          where p.NUMBER < 5 && p.ID > 0
                          select p;

            int count = 0;
            Console.WriteLine("Number < 5 && Id > 0:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestWhere4()
        {
            var lowNums = context.HOCKEY.Where((player) => player.NUMBER < 5);

            int count = 0;
            Console.WriteLine("Number < 5:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestSelect1()
        {
            var lowNums = from p in context.HOCKEY
                          select p.NAME;

            int count = 0;
            Console.WriteLine("All names:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestSelect2()
        {
            var lowNums = from p in context.HOCKEY
                          select new
                          {
                              name = p.NAME,
                              number = p.NUMBER
                          };

            int count = 0;
            Console.WriteLine("All names:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.name);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestSelect3()
        {
            var lowNums = from p in context.HOCKEY
                          select new
                          {
                              name = p.NAME.ToLower(),
                              number = p.NUMBER
                          };

            int count = 0;
            Console.WriteLine("All names:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.name);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestSelect4()
        {
            var lowNums = from p in context.HOCKEY
                          select new
                          {
                              p.NAME
                          };

            int count = 0;
            Console.WriteLine("All names:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestSelect5()
        {
            var lowNums = context.HOCKEY.Select((player) => new { Num = player.NUMBER, Name = player.NAME });

            int count = 0;
            Console.WriteLine("All names:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Name);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestSelect6()
        {
            var lowNums = from p1 in context.HOCKEY
                          from p2 in context.HOCKEY
                          where p1.NUMBER == p2.NUMBER
                          select new
                          {
                              Name = p1.NAME,
                              Position = p2.POSITION
                          };
            
            int count = 0;
            Console.WriteLine("All names and positions:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Name+" ("+x.Position+")");
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestSelect7()
        {
            var lowNums = from p1 in context.HOCKEY
                          where p1.NUMBER == 1
                          from p2 in context.HOCKEY
                          where p1.NUMBER == p2.NUMBER
                          select new
                          {
                              Name = p1.NAME,
                              Position = p2.POSITION
                          };

            int count = 0;
            Console.WriteLine("One name and position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Name + " (" + x.Position + ")");
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestPartition1()
        {
            var lowNums = context.HOCKEY.Take(3);

            int count = 0;
            Console.WriteLine("Just three:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void LINQTestPartition2()
        {
            var lowNums = (from p in context.HOCKEY
                          where p.TEAM == "Bruins"
                          select p)
                          .Take(3);

            int count = 0;
            Console.WriteLine("Just three:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void LINQTestPartition3()
        {
            var lowNums = context.HOCKEY.OrderBy(player => player.NUMBER).Skip(5);

            int count = 0;
            Console.WriteLine("All but five:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(tableRows-5, count);
        }

        [TestMethod]
        public void LINQTestPartition4()
        {
            var lowNums = context.HOCKEY.OrderBy(player => player.NUMBER).Skip(5).Take(3);

            int count = 0;
            Console.WriteLine("Just three, skipping five:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NAME);
                count++;
            }
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void LINQTestOrder1()
        {
            var lowNums = from p in context.HOCKEY
                          orderby p.NUMBER
                          select p;

            int lastNumber = -1;
            Console.WriteLine("All numbers:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NUMBER);
                Assert.IsTrue(x.NUMBER >= lastNumber);
                lastNumber = (int)x.NUMBER;
            }
        }

        [TestMethod]
        public void LINQTestOrder2()
        {
            var lowNums = from p in context.HOCKEY
                          orderby p.NUMBER descending
                          select p;

            int lastNumber = Int32.MaxValue;
            Console.WriteLine("All numbers:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.NUMBER);
                Assert.IsTrue(x.NUMBER <= lastNumber);
                lastNumber = (int)x.NUMBER;
            }
        }

        [TestMethod]
        public void LINQTestOrder3()
        {
            var lowNums = from p in context.HOCKEY
                          orderby p.POSITION, p.NUMBER
                          select p;

            string lastPosition = "";
            int lastNumber = -1;
            Console.WriteLine("All position & numbers:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.POSITION + " - " + x.NUMBER);
                Assert.IsTrue(x.POSITION.CompareTo(lastPosition) != -1);
                if (x.POSITION != lastPosition)
                {
                    lastNumber = -1;
                    lastPosition = x.POSITION;
                }
                Assert.IsTrue(x.NUMBER >= lastNumber);
                lastNumber = (int)x.NUMBER;
            }
        }

        [TestMethod]
        public void LINQTestOrder4()
        {
            var lowNums = context.HOCKEY.OrderBy(player => player.POSITION);

            string lastPosition = "";
            Console.WriteLine("All position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.POSITION);
                Assert.IsTrue(x.POSITION.CompareTo(lastPosition) != -1);
                lastPosition = x.POSITION;
            }
        }

        [TestMethod]
        public void LINQTestGroup1()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.NUMBER % 2 into g
                          select new
                          {
                              IsEven = g.Key,
                              Values = g
                          };

            int count = 0;
            Console.WriteLine("All numbers, grouped by evennes:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.IsEven+" contains "+x.Values.Count()+" rows:");
                foreach (var y in x.Values)
                {
                    Console.WriteLine("\t" + y.NUMBER + " = " + y.NAME);
                    Assert.AreEqual(x.IsEven, y.NUMBER % 2);
                }
                count++;
            }
            Assert.AreEqual(2, count);
        }

        /*
         * This causes a "mix of statistic/scalar expression in select list" error
        [TestMethod]
        public void LINQTestGroup2()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.NUMBER % 2 into g
                          select new
                          {
                              IsEven = g.Key,
                              Values = g.Count()
                          };

            int count = 0;
            Console.WriteLine("All numbers, grouped by evennes:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.IsEven+" contains "+x.Values+" rows:");
                count++;
            }
            Assert.AreEqual(2, count);
        }
        */

        [TestMethod]
        public void LINQTestGroup3()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.POSITION.Substring(0,1) into g
                          select new
                          {
                              FirstLetter = g.Key,
                              Values = g
                          };

            Console.WriteLine("All numbers, grouped by first character:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.FirstLetter + " contains " + x.Values.Count() + " rows:");
                foreach (var y in x.Values)
                {
                    Console.WriteLine("\t" + y.POSITION + " = " + y.NAME);
                    Assert.AreEqual(x.FirstLetter[0], y.POSITION[0]);
                }
            }
        }

        [TestMethod]
        public void LINQTestGroup4()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.POSITION into g
                          select new
                          {
                              Position = g.Key,
                              Values = g
                          };

            Console.WriteLine("All numbers, grouped by first character:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Position + " contains " + x.Values.Count() + " rows:");
                foreach (var y in x.Values)
                {
                    Console.WriteLine("\t" + y.POSITION + " = " + y.NAME);
                    Assert.AreEqual(x.Position, y.POSITION);
                }
            }
        }

        /*
         * This generates a OUTER APPLY statement
        [TestMethod]
        public void LINQTestGroup5()
        {
            var lowNums = from p in context.HOCKEY
                          select new
                          {
                              p.TEAM,
                              Roles = from p2 in context.HOCKEY
                                        where p2.TEAM == p.TEAM
                                        group p2 by p2.POSITION into posgroup
                                        select new
                                        {
                                            Position = posgroup.Key,
                                            Players = posgroup
                                        }
                          };

            Console.WriteLine("All players, grouped by team and position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.TEAM);
                foreach (var y in x.Roles)
                {
                    Console.WriteLine("\t" + y.Position);
                    foreach (var z in y.Players)
                    {
                        Console.WriteLine("\t" + z.NAME);
                        Assert.AreEqual(z.POSITION, y.Position);
                        Assert.AreEqual(z.TEAM, x.TEAM);
                    }
                }
            }
        }
         */

        [TestMethod]
        public void LINQTestGroup6()
        {
            var lowNums = context.HOCKEY.GroupBy(player => player.TEAM, player => player.NAME);

            Console.WriteLine("All players, grouped by team:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Key+"="+x.Count());
                Assert.AreEqual(tableRows, x.Count());
                foreach (var z in x)
                {
                    Console.WriteLine("\t" + z);
                }
            }
        }

        [TestMethod]
        public void LINQTestDistinct1()
        {
            var lowNums = (from player in context.HOCKEY
                           select player.TEAM).Distinct();

            int count = 0;
            Console.WriteLine("All teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        /*
         * This returns two rows, even if the first time there is a NULL value
        [TestMethod]
        public void LINQTestUnion1()
        {
            var lowNums = (from player in context.HOCKEY select player.TEAM)
                           .Union(from player2 in context.HOCKEY select player2.TEAM);

            int count = 0;
            Console.WriteLine("All unique teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                Assert.IsNotNull(x);
                count++;
            }
            Assert.AreEqual(1, count);
        }
         */

        /*
         * INTERSECT is not supported
        [TestMethod]
        public void LINQTestIntersect1()
        {
            var lowNums = (from player in context.HOCKEY select player.TEAM)
                           .Intersect(from player2 in context.HOCKEY select player2.TEAM);

            int count = 0;
            Console.WriteLine("All teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }
         */

        /*
         * EXCEPT is not supported
        [TestMethod]
        public void LINQTestExcept1()
        {
            var lowNums = (from player in context.HOCKEY select player.TEAM)
                           .Except(from player2 in context.HOCKEY select player2.TEAM);

            int count = 0;
            Console.WriteLine("All teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(0, count);
        }
         */

        [TestMethod]
        public void LINQTestToArray1()
        {
            var lowNums = (from player in context.HOCKEY select player.TEAM).ToArray();

            Console.WriteLine("All teams:");
            for (int d = 0; d < lowNums.Length; d++) 
            {
                Console.WriteLine(lowNums[d]);
            }
            Assert.AreEqual(tableRows, lowNums.Length);
        }

        [TestMethod]
        public void LINQTestToList1()
        {
            var lowNums = (from player in context.HOCKEY select player.TEAM).ToList();

            Console.WriteLine("All teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
            }
            Assert.AreEqual(tableRows, lowNums.Count);
        }

        [TestMethod]
        public void LINQTestToDictionary1()
        {
            var lowNums = context.HOCKEY.ToDictionary(player => player.NAME);

            Console.WriteLine("All players:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Key+" "+x.Value.POSITION);
            }
            Assert.AreEqual(tableRows, lowNums.Count);
        }

        [TestMethod]
        public void LINQTestFirst1()
        {
            var x = context.HOCKEY.First();
            Assert.IsNotNull(x);

            Console.WriteLine("First player:");
            Console.WriteLine(x.NAME + " " + x.POSITION);
        }

        [TestMethod]
        public void LINQTestFirst2()
        {
            var x = context.HOCKEY.First(player => player.NAME.StartsWith("MAX "));
            Assert.IsNotNull(x);

            Console.WriteLine("First player with 'MAX':");
            Console.WriteLine(x.NAME + " " + x.POSITION);
        }

        [TestMethod]
        public void LINQTestFirst3()
        {
            var x = (from player in context.HOCKEY
                     where player.NAME == "This does not exist"
                     select player.NAME).FirstOrDefault();
            Assert.IsNull(x);
        }

        [TestMethod]
        public void LINQTestFirst4()
        {
            var x = context.HOCKEY.FirstOrDefault(player => player.NUMBER == 1900);
            Assert.IsNull(x);
        }

        [TestMethod]
        public void LINQTestAny1()
        {
            bool x = context.HOCKEY.Any(player => player.TEAM == "Bruins");
            Assert.IsTrue(x);
        }

        [TestMethod]
        public void LINQTestAny2()
        {
            var lowNums = from player in context.HOCKEY
                          group player by player.TEAM into teams
                          where teams.Any(t => t.POSITION == "Fan")
                          select new
                          {
                              Team = teams.Key,
                              Players = teams
                          };

            int count = 0;
            Console.WriteLine("All teams with a fan:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestAll1()
        {
            bool x = context.HOCKEY.All(player => player.TEAM == "Bruins");
            Assert.IsTrue(x);
        }

        [TestMethod]
        public void LINQTestAll2()
        {
            var lowNums = from player in context.HOCKEY
                          group player by player.TEAM into teams
                          where teams.All(t => t.NAME.ToUpper() == t.NAME)
                          select new
                          {
                              Team = teams.Key,
                              Players = teams
                          };

            int count = 0;
            Console.WriteLine("All teams with player names in capital letters:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestAll3()
        {
            var lowNums = from player in context.HOCKEY
                          group player by player.TEAM into teams
                          where teams.All(t => t.NAME.ToLower() == t.NAME)
                          select new
                          {
                              Team = teams.Key,
                              Players = teams
                          };

            int count = 0;
            Console.WriteLine("All teams with player names in lowercase letters:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                count++;
            }
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void LINQTestCount1()
        {
            int count = (from player in context.HOCKEY
                         select player.TEAM).Distinct().Count();

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestCount2()
        {
            int count = context.HOCKEY.Count(player => player.POSITION == "Fan");

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestSum1()
        {
            int? count = (from player in context.HOCKEY
                          where player.POSITION == "Goalie"
                          select player.NUMBER).Sum();

            Assert.AreEqual(35+40, count);
        }

        [TestMethod]
        public void LINQTestSum2()
        {
            int? count = context.HOCKEY.Sum(player => player.NUMBER);

            Assert.AreEqual(883, count);
        }
        
        /*
         * Like in the g.Count case, this triggers a "mix of statistic/scalar expression in select list" error
        [TestMethod]
        public void LINQTestSum3()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.POSITION into g
                          select new
                          {
                              Position = g.Key,
                              Values = g.Sum(p => p.NUMBER)
                          };

            int count = 0;
            Console.WriteLine("Sum of all numbers, grouped by position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Position + " -> " + x.Values);
                if (x.Position == "Fan")
                    Assert.AreEqual(1, x.Values);
                count++;
            }

            Assert.AreEqual(4, count);
        }
         */

        [TestMethod]
        public void LINQTestMin1()
        {
            int? number = (from player in context.HOCKEY
                           select player.NUMBER).Min();

            Assert.AreEqual(1, number);
        }

        [TestMethod]
        public void LINQTestMin2()
        {
            int? number = context.HOCKEY.Min(player => player.NUMBER);

            Assert.AreEqual(1, number);
        }

        /*
         * Like in the g.Count case, this triggers a "mix of statistic/scalar expression in select list" error
        [TestMethod]
        public void LINQTestMin3()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.POSITION into g
                          select new
                          {
                              Position = g.Key,
                              Values = g.Min(p => p.NUMBER)
                          };

            int count = 0;
            Console.WriteLine("Minimum number, grouped by position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Position + " -> " + x.Values);
                if (x.Position == "Fan")
                    Assert.AreEqual(1, x.Values);
                count++;
            }

            Assert.AreEqual(4, count);
        }
         */

        [TestMethod]
        public void LINQTestMax1()
        {
            int? number = (from player in context.HOCKEY
                           select player.NUMBER).Max();

            Assert.AreEqual(91, number);
        }

        [TestMethod]
        public void LINQTestMax2()
        {
            int? number = context.HOCKEY.Max(player => player.NUMBER);

            Assert.AreEqual(91, number);
        }

        /*
         * Like in the g.Count case, this triggers a "mix of statistic/scalar expression in select list" error
        [TestMethod]
        public void LINQTestMax3()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.POSITION into g
                          select new
                          {
                              Position = g.Key,
                              Values = g.Max(p => p.NUMBER)
                          };

            int count = 0;
            Console.WriteLine("Maximum number, grouped by position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Position + " -> " + x.Values);
                if (x.Position == "Fan")
                    Assert.AreEqual(1, x.Values);
                count++;
            }

            Assert.AreEqual(4, count);
        }
         */

        [TestMethod]
        public void LINQTestAvg1()
        {
            double? number = (from player in context.HOCKEY
                           select player.NUMBER).Average();

            Assert.AreEqual(37, Math.Round((double)number));
        }

        [TestMethod]
        public void LINQTestAvg2()
        {
            double? number = context.HOCKEY.Average(player => player.NUMBER);

            Assert.AreEqual(37, Math.Round((double)number));
        }

        /*
         * Like in the g.Count case, this triggers a "mix of statistic/scalar expression in select list" error
        [TestMethod]
        public void LINQTestAvg3()
        {
            var lowNums = from p in context.HOCKEY
                          group p by p.POSITION into g
                          select new
                          {
                              Position = g.Key,
                              Values = g.Average(p => p.NUMBER)
                          };

            int count = 0;
            Console.WriteLine("Average number, grouped by position:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Position + " -> " + x.Values);
                if (x.Position == "Fan")
                    Assert.AreEqual(1, x.Values);
                count++;
            }

            Assert.AreEqual(4, count);
        }
         */

        /*
         * The union returns NULL for the second table
        [TestMethod]
        public void LINQTestConcat1()
        {
            var lowNums = (from player in context.HOCKEY
                           where player.POSITION == "Fan"
                           select player.NAME)
                          .Concat(from player2 in context.HOCKEY
                                  where player2.POSITION == "Fan"
                                  select player2.TEAM);

            int count = 0;
            Console.WriteLine("All players and their teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                Assert.IsNotNull(x);
                count++;
            }
            Assert.AreEqual(2, count);
        }
         */

        [TestMethod]
        public void LINQTestJoin1()
        {
            var lowNums = from player in context.HOCKEY
                          join p in context.HOCKEY on player.ID equals p.ID
                          select new
                          {
                              Team = player.TEAM,
                              Name = p.NAME
                          };

            int count = 0;
            Console.WriteLine("All teams with player names:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Team + " " + x.Name);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestJoin2()
        {
            var lowNums = from player in context.HOCKEY
                          where player.POSITION == "Fan"
                          join p in context.HOCKEY on player.TEAM equals p.TEAM into players
                          select new
                          {
                              Team = player.TEAM,
                              Players = players
                          };

            int count = 0;
            Console.WriteLine("The team with a fan with all player names embedded:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Team + " has " + x.Players.Count() + " players");
                count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LINQTestJoin3()
        {
            var lowNums = from player in context.HOCKEY
                          where player.POSITION == "Fan"
                          join p in context.HOCKEY on player.TEAM equals p.TEAM into players
                          from p2 in players
                          select new
                          {
                              Team = player.TEAM,
                              Player = p2.NAME
                          };

            int count = 0;
            Console.WriteLine("The team with a fan with all player names exploded:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Team + " " + x.Player);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }

        [TestMethod]
        public void LINQTestJoin4()
        {
            var lowNums = from player in context.HOCKEY
                          join p in context.HOCKEY on player.NUMBER equals p.NUMBER+1 into NextPlayer
                          from p2 in NextPlayer.DefaultIfEmpty()
                          orderby player.NUMBER
                          select new
                          {
                              Team = player.TEAM,
                              Player = player.NAME,
                              PlayerNumber = player.NUMBER,
                              NextPlayer = p2 == null ? "<none>" : p2.NAME,
                              NextPlayerNumber = p2 == null ? -1 : p2.NUMBER
                          };

            int count = 0;
            Console.WriteLine("Each player with the player having the previous number:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Team + " " + x.Player + " ("+ x.PlayerNumber + ") " + x.NextPlayer + " ("+x.NextPlayerNumber+ ")");
                if (x.NextPlayerNumber != -1)
                    Assert.AreEqual(x.PlayerNumber, x.NextPlayerNumber + 1);
                count++;
            }
            Assert.AreEqual(tableRows, count);
        }
    }
}
