using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnitEFCodeFirstTestProject
{
    [TestFixture]
    public class TestFixture
    {
        static int tableRows = 0;

        [TestFixtureSetUp]
        public static void Init()
        {
            Utils.CreateHockeyTable();
            Utils.CreatePersonTable();
            Utils.CreateGameTable();
            tableRows = Utils.GetTableRows();
            
        }

        [Test]
        public void TestWhere1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              where p.Number < 5
                              select p;

                int count = 0;
                Console.WriteLine("Number < 5:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(1, count); 
            }
        }

        [Test]
        public void TestGuidLiteral()
        {
            using (var context = new EFCodeFirstContext())
            {                                
                var count = (from p in context.Person
                             where p.Id == new Guid("{F571197E-7A4F-4961-9363-7411EACCA841}")
                             select p).Count();                

                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void TestBigIntIdentity()
        {
            GameEntity data;
            using (var context = new EFCodeFirstContext())
            {
                data = context.Set<GameEntity>().Find(1); 
            } 
            Assert.AreEqual(1, data.Id); 
        }

        [Test]
        public void TestWhere2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              where p.Number == 1
                              select p;

                int count = 0;
                Console.WriteLine("Number == 1:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(1, count);
            } 
        }

        [Test]
        public void TestWhere3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              where p.Number < 5 && p.Id > 0
                              select p;

                int count = 0;
                Console.WriteLine("Number < 5 && Id > 0:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(1, count); 
            }
        }

        [Test]
        public void LINQTestWhere4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.Where((player) => player.Number < 5);

                int count = 0;
                Console.WriteLine("Number < 5:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void LINQTestSelect1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              select p.Name;

                int count = 0;
                Console.WriteLine("All names:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x);
                    count++;
                }
                Assert.AreEqual(tableRows, count);
            }
        }

        [Test]
        public void LINQTestSelect2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              select new
                              {
                                  name = p.Name,
                                  number = p.Number
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
        }

        [Test]
        public void LINQTestSelect3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              select new
                              {
                                  name = p.Name.ToLower(),
                                  number = p.Number
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
        }

        [Test]
        public void LINQTestSelect4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              select new
                              {
                                  p.Name
                              };

                int count = 0;
                Console.WriteLine("All names:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(tableRows, count);
            }
        }

        [Test]
        public void LINQTestSelect5()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.Select((player) => new { Num = player.Number, Name = player.Name });

                int count = 0;
                Console.WriteLine("All names:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(tableRows, count);
            }
        }

        [Test]
        public void LINQTestSelect6()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p1 in context.Hockey
                              from p2 in context.Hockey
                              where p1.Number == p2.Number
                              select new
                              {
                                  Name = p1.Name,
                                  Position = p2.Position
                              };

                int count = 0;
                Console.WriteLine("All names and positions:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name + " (" + x.Position + ")");
                    count++;
                }
                Assert.AreEqual(tableRows, count);
            }
        }

        [Test]
        public void LINQTestSelect7()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p1 in context.Hockey
                              where p1.Number == 1
                              from p2 in context.Hockey
                              where p1.Number == p2.Number
                              select new
                              {
                                  Name = p1.Name,
                                  Position = p2.Position
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
        }

        [Test]
        public void LINQTestPartition1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.Take(3);

                int count = 0;
                Console.WriteLine("Just three:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(3, count);
            }
        }

        [Test]
        public void LINQTestPartition2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from p in context.Hockey
                               where p.Team == "Bruins"
                               select p)
                              .Take(3);

                int count = 0;
                Console.WriteLine("Just three:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(3, count);
            }
        }

        [Test]
        public void LINQTestPartition3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.OrderBy(player => player.Number).Skip(5);

                int count = 0;
                Console.WriteLine("All but five:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(tableRows - 5, count);
            }
        }

        [Test]
        public void LINQTestPartition4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.OrderBy(player => player.Number).Skip(5).Take(3);

                int count = 0;
                Console.WriteLine("Just three, skipping five:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Name);
                    count++;
                }
                Assert.AreEqual(3, count);
            }
        }

        [Test]
        public void LINQTestOrder1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              orderby p.Number
                              select p;

                int lastNumber = -1;
                Console.WriteLine("All numbers:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Number);
                    Assert.IsTrue(x.Number >= lastNumber);
                    lastNumber = (int)x.Number;
                }
            }
        }

        [Test]
        public void LINQTestOrder2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              orderby p.Number descending
                              select p;

                int lastNumber = Int32.MaxValue;
                Console.WriteLine("All numbers:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Number);
                    Assert.IsTrue(x.Number <= lastNumber);
                    lastNumber = (int)x.Number;
                }
            }
        }

        [Test]
        public void LINQTestOrder3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              orderby p.Position, p.Number
                              select p;

                string lastPosition = "";
                int lastNumber = -1;
                Console.WriteLine("All position & numbers:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Position + " - " + x.Number);
                    Assert.IsTrue(x.Position.CompareTo(lastPosition) != -1);
                    if (x.Position != lastPosition)
                    {
                        lastNumber = -1;
                        lastPosition = x.Position;
                    }
                    Assert.IsTrue(x.Number >= lastNumber);
                    lastNumber = (int)x.Number;
                }
            }
        }

        [Test]
        public void LINQTestOrder4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.OrderBy(player => player.Position);

                string lastPosition = "";
                Console.WriteLine("All position:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Position);
                    Assert.IsTrue(x.Position.CompareTo(lastPosition) != -1);
                    lastPosition = x.Position;
                }
            }
        }

        [Test]
        public void LINQTestGroup1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Number % 2 into g
                              select new
                              {
                                  IsEven = g.Key,
                                  Values = g
                              };

                int count = 0;
                Console.WriteLine("All numbers, grouped by evennes:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.IsEven + " contains " + x.Values.Count() + " rows:");
                    foreach (var y in x.Values)
                    {
                        Console.WriteLine("\t" + y.Number + " = " + y.Name);
                        Assert.AreEqual(x.IsEven, y.Number % 2);
                    }
                    count++;
                }
                Assert.AreEqual(2, count);
            }
        }

        [Test]
        public void LINQTestGroup2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Number % 2 into g
                              select new
                              {
                                  IsEven = g.Key,
                                  Values = g.Count()
                              };

                int count = 0;
                Console.WriteLine("All numbers, grouped by evennes:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.IsEven + " contains " + x.Values + " rows:");
                    count++;
                }
                Assert.AreEqual(2, count);
            }
        }

        [Test]
        public void LINQTestGroup3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Position.Substring(0, 1) into g
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
                        Console.WriteLine("\t" + y.Position + " = " + y.Name);
                        Assert.AreEqual(x.FirstLetter[0], y.Position[0]);
                    }
                }
            }
        }

        [Test]
        public void LINQTestGroup4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Position into g
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
                        Console.WriteLine("\t" + y.Position + " = " + y.Name);
                        Assert.AreEqual(x.Position, y.Position);
                    }
                }
            }
        }
        

        [Test]
        public void LINQTestGroup6()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.GroupBy(player => player.Team, player => player.Name);

                Console.WriteLine("All players, grouped by team:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Key + "=" + x.Count());
                    Assert.AreEqual(tableRows, x.Count());
                    foreach (var z in x)
                    {
                        Console.WriteLine("\t" + z);
                    }
                }
            }
        }

        [Test]
        public void LINQTestDistinct1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from player in context.Hockey
                               select player.Team).Distinct();

                int count = 0;
                Console.WriteLine("All teams:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x);
                    count++;
                }
                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void LINQTestUnion1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from player in context.Hockey select player.Team)
                               .Union(from player2 in context.Hockey select player2.Team);

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
        }        

        [Test]
        public void LINQTestToArray1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from player in context.Hockey select player.Team).ToArray();

                Console.WriteLine("All teams:");
                for (int d = 0; d < lowNums.Length; d++)
                {
                    Console.WriteLine(lowNums[d]);
                }
                Assert.AreEqual(tableRows, lowNums.Length);
            }
        }

        [Test]
        public void LINQTestToList1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from player in context.Hockey select player.Team).ToList();

                Console.WriteLine("All teams:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x);
                }
                Assert.AreEqual(tableRows, lowNums.Count);
            }
        }

        [Test]
        public void LINQTestToDictionary1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = context.Hockey.ToDictionary(player => player.Name);

                Console.WriteLine("All players:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Key + " " + x.Value.Position);
                }
                Assert.AreEqual(tableRows, lowNums.Count);
            }
        }

        [Test]
        public void LINQTestFirst1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var x = context.Hockey.First();
                Assert.IsNotNull(x);

                Console.WriteLine("First player:");
                Console.WriteLine(x.Name + " " + x.Position);
            }
        }

        [Test]
        public void LINQTestFirst2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var x = context.Hockey.First(player => player.Name.StartsWith("MAX "));
                Assert.IsNotNull(x);
                Assert.AreEqual(x.Name.Substring(0, 4), "MAX ");

                Console.WriteLine("First player with 'MAX':");
                Console.WriteLine(x.Name + " " + x.Position);
            }
        }

        [Test]
        public void LINQTestFirst3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var x = (from player in context.Hockey
                         where player.Name == "This does not exist"
                         select player.Name).FirstOrDefault();
                Assert.IsNull(x);
            }
        }

        [Test]
        public void LINQTestFirst4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var x = context.Hockey.FirstOrDefault(player => player.Number == 1900);
                Assert.IsNull(x);
            }
        }

        [Test]
        public void LINQTestAny1()
        {
            using (var context = new EFCodeFirstContext())
            {
                bool x = context.Hockey.Any(player => player.Team == "Bruins");
                Assert.IsTrue(x);
            }
        }

        [Test]
        public void LINQTestAny2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              group player by player.Team into teams
                              where teams.Any(t => t.Position == "Fan")
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
        }

        [Test]
        public void LINQTestAll1()
        {
            using (var context = new EFCodeFirstContext())
            {
                bool x = context.Hockey.All(player => player.Team == "Bruins");
                Assert.IsTrue(x);
            }
        }

        [Test]
        public void LINQTestAll2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              group player by player.Team into teams
                              where teams.All(t => t.Name.ToUpper() == t.Name)
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
        }

        [Test]
        public void LINQTestAll3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              group player by player.Team into teams
                              where teams.All(t => t.Name.ToLower() == t.Name)
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
        }

        [Test]
        public void LINQTestCount1()
        {
            using (var context = new EFCodeFirstContext())
            {
                int count = (from player in context.Hockey
                             select player.Team).Distinct().Count();

                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void LINQTestCount2()
        {
            using (var context = new EFCodeFirstContext())
            {
                int count = context.Hockey.Count(player => player.Position == "Fan");

                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void LINQTestSum1()
        {
            using (var context = new EFCodeFirstContext())
            {
                int? count = (from player in context.Hockey
                              where player.Position == "Goalie"
                              select player.Number).Sum();

                Assert.AreEqual(35 + 40, count);
            }
        }

        [Test]
        public void LINQTestSum2()
        {
            using (var context = new EFCodeFirstContext())
            {
                int? count = context.Hockey.Sum(player => player.Number);

                Assert.AreEqual(883, count);
            }
        }

        [Test]
        public void LINQTestSum3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Position into g
                              select new
                              {
                                  Position = g.Key,
                                  Values = g.Sum(p => p.Number)
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
        }

        [Test]
        public void LINQTestSum4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from p in context.Hockey
                               group p by p.Position into g
                               select new
                               {
                                   Position = g.Key,
                                   Values = g.Sum(p => p.Number)
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
        }

        [Test]
        public void LINQTestMin1()
        {
            using (var context = new EFCodeFirstContext())
            {
                int? number = (from player in context.Hockey
                               select player.Number).Min();

                Assert.AreEqual(1, number);
            }
        }

        [Test]
        public void LINQTestMin2()
        {
            using (var context = new EFCodeFirstContext())
            {
                int? number = context.Hockey.Min(player => player.Number);

                Assert.AreEqual(1, number);
            }
        }

        [Test]
        public void LINQTestMin3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Position into g
                              select new
                              {
                                  Position = g.Key,
                                  Values = g.Min(p => p.Number)
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
        }

        [Test]
        public void LINQTestMax1()
        {
            using (var context = new EFCodeFirstContext())
            {
                int? number = (from player in context.Hockey
                               select player.Number).Max();

                Assert.AreEqual(91, number);
            }
        }

        [Test]
        public void LINQTestMax2()
        {
            using (var context = new EFCodeFirstContext())
            {
                int? number = context.Hockey.Max(player => player.Number);

                Assert.AreEqual(91, number);
            }
        }

        [Test]
        public void LINQTestMax3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Position into g
                              select new
                              {
                                  Position = g.Key,
                                  Values = g.Max(p => p.Number)
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
        }

        [Test]
        public void LINQTestAvg1()
        {
            using (var context = new EFCodeFirstContext())
            {
                double? number = (from player in context.Hockey
                                  select player.Number).Average();

                Assert.AreEqual(37, Math.Round((double)number));
            }
        }

        [Test]
        public void LINQTestAvg2()
        {
            using (var context = new EFCodeFirstContext())
            {
                double? number = context.Hockey.Average(player => player.Number);

                Assert.AreEqual(37, Math.Round((double)number));
            }
        }

        [Test]
        public void LINQTestAvg3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from p in context.Hockey
                              group p by p.Position into g
                              select new
                              {
                                  Position = g.Key,
                                  Values = g.Average(p => p.Number)
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
        }

        [Test]
        public void LINQTestContains1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var x = from player in context.Hockey
                        where player.Name.Contains(" MA")
                        select player;
                Assert.IsNotNull(x);

                Console.WriteLine("Player with MA in last name:");
                foreach (var p in x)
                {
                    Console.WriteLine(p.Name + " " + p.Position);
                    Assert.IsTrue(p.Name.Contains(" MA"));
                }
            }
        }

        [Test]
        public void LINQTestStartsWith1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var x = from player in context.Hockey
                        where player.Name.StartsWith("MAX")
                        select player;
                Assert.IsNotNull(x);

                Console.WriteLine("Player with MAX in name:");
                foreach (var p in x)
                {
                    Console.WriteLine(p.Name + " " + p.Position);
                    Assert.IsTrue(p.Name.StartsWith("MAX"));
                }
            }
        }

        [Test]
        public void LINQTestConcat1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = (from player in context.Hockey
                               where player.Position == "Fan"
                               select player.Name)
                              .Concat(from player2 in context.Hockey
                                      where player2.Position == "Fan"
                                      select player2.Team);

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
        }

        [Test]
        public void LINQTestJoin1()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              join p in context.Hockey on player.Id equals p.Id
                              select new
                              {
                                  Team = player.Team,
                                  Name = p.Name
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
        }

        [Test]
        public void LINQTestJoin2()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              where player.Position == "Fan"
                              join p in context.Hockey on player.Team equals p.Team into players
                              select new
                              {
                                  Team = player.Team,
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
        }

        [Test]
        public void LINQTestJoin3()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              where player.Position == "Fan"
                              join p in context.Hockey on player.Team equals p.Team into players
                              from p2 in players
                              select new
                              {
                                  Team = player.Team,
                                  Player = p2.Name
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
        }

        [Test]
        public void LINQTestJoin4()
        {
            using (var context = new EFCodeFirstContext())
            {
                var lowNums = from player in context.Hockey
                              join p in context.Hockey on player.Number equals p.Number + 1 into NextPlayer
                              from p2 in NextPlayer.DefaultIfEmpty()
                              orderby player.Number
                              select new
                              {
                                  Team = player.Team,
                                  Player = player.Name,
                                  PlayerNumber = player.Number,
                                  NextPlayer = p2 == null ? "<none>" : p2.Name,
                                  NextPlayerNumber = p2 == null ? -1 : p2.Number
                              };

                int count = 0;
                Console.WriteLine("Each player with the player having the previous number:");
                foreach (var x in lowNums)
                {
                    Console.WriteLine(x.Team + " " + x.Player + " (" + x.PlayerNumber + ") " + x.NextPlayer + " (" + x.NextPlayerNumber + ")");
                    if (x.NextPlayerNumber != -1)
                        Assert.AreEqual(x.PlayerNumber, x.NextPlayerNumber + 1);
                    count++;
                }
                Assert.AreEqual(tableRows, count);
            }
        }        
    }
}
