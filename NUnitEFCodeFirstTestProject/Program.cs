using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NUnitEFCodeFirstTestProject
{
    class Program
    {
        static int Main(string[] args)
        {
            string[] my_args = { Assembly.GetExecutingAssembly().Location };

            int returnCode = NUnit.ConsoleRunner.Runner.Main(my_args);

            if (returnCode != 0)
                Console.Beep();

            return returnCode;
        }
    }
}
