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
        [TestFixtureSetUp]
        public static void Init()
        {
            Utils.CreateHockeyTable();
            Utils.CreatePersonTable();
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

        public void TestGuidLiteral()
        {
            using (var context = new EFCodeFirstContext())
            {
                var count = (from p in context.Person
                           where p.Id == Guid.Parse("{F571197E-7A4F-4961-9363-7411EACCA841}")
                           select p).Count();

                Assert.AreEqual(1, count);
            }
        }

    }
}
