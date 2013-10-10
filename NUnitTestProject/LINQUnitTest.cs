using System;
using NUnit.Framework;
using System.Linq;
using System.Data.Common;
using NuoDb.Data.Client;
using System.Data;

namespace NUnitTestProject
{
    [TestFixture]
    public class LINQTestFixture
    {
        testEntities context;
        static int tableRows = 0;

        public LINQTestFixture()
        {
            context = new testEntities();
        }

        [TestFixtureSetUp]
        public static void Init() 
        {
            Utils.CreateHockeyTable();
            using (NuoDbConnection connection = new NuoDbConnection(TestFixture1.connectionString))
            {
                DbCommand command = new NuoDbCommand("select count(*) from hockey", connection);

                connection.Open();
                tableRows = (int)command.ExecuteScalar();
            }
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void LINQTestUnion1()
        {
            var lowNums = (from player in context.HOCKEY select player.TEAM)
                           .Union(from player2 in context.HOCKEY select player2.TEAM);

            int count = 0;
            Console.WriteLine("All unique teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                Assert.IsNotNull(x, "Bug DB-2621");
                count++;
            }
            Assert.AreEqual(1, count, "Bug DB-2621");
        }
        
        /*
         * INTERSECT is not supported
        [Test]
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
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void LINQTestFirst1()
        {
            var x = context.HOCKEY.First();
            Assert.IsNotNull(x);

            Console.WriteLine("First player:");
            Console.WriteLine(x.NAME + " " + x.POSITION);
        }

        [Test]
        public void LINQTestFirst2()
        {
            var x = context.HOCKEY.First(player => player.NAME.StartsWith("MAX "));
            Assert.IsNotNull(x);
            Assert.AreEqual(x.NAME.Substring(0, 4), "MAX ");

            Console.WriteLine("First player with 'MAX':");
            Console.WriteLine(x.NAME + " " + x.POSITION);
        }

        [Test]
        public void LINQTestFirst3()
        {
            var x = (from player in context.HOCKEY
                     where player.NAME == "This does not exist"
                     select player.NAME).FirstOrDefault();
            Assert.IsNull(x);
        }

        [Test]
        public void LINQTestFirst4()
        {
            var x = context.HOCKEY.FirstOrDefault(player => player.NUMBER == 1900);
            Assert.IsNull(x);
        }

        [Test]
        public void LINQTestAny1()
        {
            bool x = context.HOCKEY.Any(player => player.TEAM == "Bruins");
            Assert.IsTrue(x);
        }

        [Test]
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

        [Test]
        public void LINQTestAll1()
        {
            bool x = context.HOCKEY.All(player => player.TEAM == "Bruins");
            Assert.IsTrue(x);
        }

        [Test]
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

        [Test]
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

        [Test]
        public void LINQTestCount1()
        {
            int count = (from player in context.HOCKEY
                         select player.TEAM).Distinct().Count();

            Assert.AreEqual(1, count);
        }

        [Test]
        public void LINQTestCount2()
        {
            int count = context.HOCKEY.Count(player => player.POSITION == "Fan");

            Assert.AreEqual(1, count);
        }

        [Test]
        public void LINQTestSum1()
        {
            int? count = (from player in context.HOCKEY
                          where player.POSITION == "Goalie"
                          select player.NUMBER).Sum();

            Assert.AreEqual(35+40, count);
        }

        [Test]
        public void LINQTestSum2()
        {
            int? count = context.HOCKEY.Sum(player => player.NUMBER);

            Assert.AreEqual(883, count);
        }
        
        [Test]
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
        
        [Test]
        public void LINQTestSum4()
        {
            var lowNums = (from p in context.HOCKEY
                          group p by p.POSITION into g
                          select new
                          {
                              Position = g.Key,
                              Values = g.Sum(p => p.NUMBER)
                          }).OrderBy(q => q.Values);

            int count = 0;
            Console.WriteLine("Sum of all numbers, grouped by position and listed in ascending order:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x.Position + " -> " + x.Values);
                if (x.Position == "Fan")
                    Assert.AreEqual(1, x.Values);
                count++;
            }

            Assert.AreEqual(4, count, "Bug DB-3090");
        }

        [Test]
        public void LINQTestMin1()
        {
            int? number = (from player in context.HOCKEY
                           select player.NUMBER).Min();

            Assert.AreEqual(1, number);
        }

        [Test]
        public void LINQTestMin2()
        {
            int? number = context.HOCKEY.Min(player => player.NUMBER);

            Assert.AreEqual(1, number);
        }

        [Test]
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

        [Test]
        public void LINQTestMax1()
        {
            int? number = (from player in context.HOCKEY
                           select player.NUMBER).Max();

            Assert.AreEqual(91, number);
        }

        [Test]
        public void LINQTestMax2()
        {
            int? number = context.HOCKEY.Max(player => player.NUMBER);

            Assert.AreEqual(91, number);
        }

        [Test]
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

        [Test]
        public void LINQTestAvg1()
        {
            double? number = (from player in context.HOCKEY
                           select player.NUMBER).Average();

            Assert.AreEqual(37, Math.Round((double)number));
        }

        [Test]
        public void LINQTestAvg2()
        {
            double? number = context.HOCKEY.Average(player => player.NUMBER);

            Assert.AreEqual(37, Math.Round((double)number));
        }

        [Test]
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

        [Test]
        public void LINQTestContains1()
        {
            var x = from player in context.HOCKEY
                    where player.NAME.Contains(" MA")
                    select player; 
            Assert.IsNotNull(x);

            Console.WriteLine("Player with MA in last name:");
            foreach(var p in x)
            {
                Console.WriteLine(p.NAME + " " + p.POSITION);
                Assert.IsTrue(p.NAME.Contains(" MA"));
            }
        }

        [Test]
        public void LINQTestStartsWith1()
        {
            var x = from player in context.HOCKEY
                    where player.NAME.StartsWith("MAX")
                    select player;
            Assert.IsNotNull(x);

            Console.WriteLine("Player with MAX in name:");
            foreach (var p in x)
            {
                Console.WriteLine(p.NAME + " " + p.POSITION);
                Assert.IsTrue(p.NAME.StartsWith("MAX"));
            }
        }

        [Test]
        public void LINQTestConcat1()
        {
            var lowNums = (from player in context.HOCKEY
                           where player.POSITION == "Fan"
                           select player.NAME)
                          .Concat(from player2 in context.HOCKEY
                                  where player2.POSITION == "Fan"
                                  select player2.TEAM);

            int count = 0;
            Console.WriteLine("All fans and their teams:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
                Assert.IsNotNull(x, "Bug DB-2621");
                count++;
            }
            Assert.AreEqual(2, count);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void LINQTestDML()
        {
            if (context.Connection.State != ConnectionState.Open)
            {
                context.Connection.Open();
            }
            DbTransaction tx = context.Connection.BeginTransaction();
            try
            {
                int initCount = context.HOCKEY.Count();
                int maxID = (from player in context.HOCKEY
                                select player.ID).Max();
                HOCKEY newPlayer = new HOCKEY() { NAME = "Unknown" };
                context.HOCKEY.AddObject(newPlayer);
                context.SaveChanges();
                int afterCount = context.HOCKEY.Count();
                Assert.IsTrue(afterCount == initCount + 1);
                Assert.IsNotNull(newPlayer.ID);
                Assert.IsTrue(newPlayer.ID > maxID);
                context.DeleteObject(newPlayer);
                context.SaveChanges();
                int endCount = context.HOCKEY.Count();

                Assert.AreEqual(initCount, endCount);
            }
            finally
            {
                tx.Rollback();
                context.Connection.Close();
            }
        }

        [Test]
        public void LINQTestABS()
        {
            var results = from player in context.HOCKEY
                          select Math.Abs((decimal)player.NUMBER);

            foreach (var x in results)
            {
                Assert.GreaterOrEqual(x, 0);
            }
        }

        [Test]
        public void LINQTestCEILING()
        {
            var results = from player in context.HOCKEY
                          select Math.Ceiling(((double)player.NUMBER) / 10.0);

            foreach (var x in results)
            {
                Assert.AreEqual(x, (int)x);
            }
        }

        [Test]
        public void LINQTestFLOOR()
        {
            var results = from player in context.HOCKEY
                          select Math.Floor(((double)player.NUMBER) / 10.0);

            foreach (var x in results)
            {
                Assert.AreEqual(x, (int)x);
            }
        }

        [Test]
        public void LINQTestROUND()
        {
            var results = from player in context.HOCKEY
                          select Math.Round(((double)player.NUMBER) / 10.0);

            foreach (var x in results)
            {
                Assert.AreEqual(x, (int)x);
            }
        }

        [Test]
        public void LINQTestPOWER()
        {
            var results = from player in context.HOCKEY
                          select Math.Pow(((double)player.NUMBER), 2);

            foreach (var x in results)
            {
                Assert.AreEqual(Math.Sqrt(x), (int)Math.Sqrt(x));
            }
        }

        [Test]
        public void LINQTestMOD()
        {
            var results = from player in context.HOCKEY
                          select (((double)player.NUMBER)/10) % 1;

            foreach (var x in results)
            {
                Assert.LessOrEqual(x, 1);
            }
        }

        [Test]
        public void LINQTestTRIM()
        {
            var results = from player in context.HOCKEY
                          select new
                          {
                            one=String.Concat("   ",player.NAME,"  ").Trim(),
                            two=player.NAME
                          };

            foreach (var x in results)
            {
                Assert.AreEqual(x.one, x.two);
            }
        }

        [Test]
        public void LINQTestLTRIM()
        {
            var results = from player in context.HOCKEY
                          select new
                          {
                              one = String.Concat("   ", player.NAME).TrimStart(),
                              two = player.NAME
                          };

            foreach (var x in results)
            {
                Assert.AreEqual(x.one, x.two);
            }
        }

        [Test]
        public void LINQTestRTRIM()
        {
            var results = from player in context.HOCKEY
                          select new
                          {
                              one = String.Concat(player.NAME, "   ").TrimEnd(),
                              two = player.NAME
                          };

            foreach (var x in results)
            {
                Assert.AreEqual(x.one, x.two);
            }
        }

        [Test]
        public void LINQTestSUBSTRING()
        {
            var results = from player in context.HOCKEY
                          select player.NAME.Substring(0,2);

            foreach (var x in results)
            {
                Assert.LessOrEqual(x.Length, 2);
            }
        }

        [Test]
        public void LINQTestINDEXOF()
        {
            var results = from player in context.HOCKEY
                          select player.NAME.IndexOf("SUM");

            foreach (var x in results)
            {
                Assert.GreaterOrEqual(x, -1);
            }
        }

        [Test]
        public void LINQTestENDSWITH()
        {
            var results = from player in context.HOCKEY
                          select new
                            {
                                name = player.NAME,
                                b = player.NAME.EndsWith("MIT"),
                            };

            foreach (var x in results)
            {
                Assert.AreEqual(x.name.EndsWith("MIT"), x.b);
            }
        }

        [Test]
        public void LINQTestSTARTSWITH()
        {
            var results = from player in context.HOCKEY
                          select new
                          {
                              name = player.NAME,
                              b = player.NAME.StartsWith("MAX"),
                          };

            foreach (var x in results)
            {
                Assert.AreEqual(x.name.StartsWith("MAX"), x.b);
            }
        }

        [Test]
        public void LINQTestCURRENTDATE()
        {
            var results = from player in context.HOCKEY
                          select DateTime.Now;

            foreach (var x in results)
            {
                Assert.LessOrEqual(x, DateTime.Now);
            }
        }

        [Test]
        public void LINQTestDATE()
        {
            var results = from player in context.HOCKEY
                          select new
                          {
                              d = DateTime.Now.Day,
                              m = DateTime.Now.Month,
                              y = DateTime.Now.Year
                          };

            foreach (var x in results)
            {
                Assert.LessOrEqual(new DateTime(x.y, x.m, x.d), DateTime.Now);
            }
        }

        [Test]
        public void LINQTestTIME()
        {
            var results = from player in context.HOCKEY
                          select new
                          {
                              h = DateTime.Now.Hour,
                              m = DateTime.Now.Minute,
                              s = DateTime.Now.Second
                          };

            foreach (var x in results)
            {
                Assert.LessOrEqual(new TimeSpan(x.h, x.m, x.s), DateTime.Now.TimeOfDay);
            }
        }

    }
}
