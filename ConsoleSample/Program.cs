using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuoDb.Data.Client;

namespace ConsoleSample
{
    class Program
    {
        static int Main(string[] args)
        {
            // if (args.Count() != 1) 
            // {
            //     Console.Out.WriteLine("Usage: ConsoleSample.exe <dbname>\n");
            //     return 1;
            // }

            try {
                HelloDB helloDB = new HelloDB("demo");

                helloDB.CreateTable();
                helloDB.AddNames();
                string name = helloDB.GetName(12);

                Console.Out.WriteLine("The NAME with id of 12 is '{0}'\n", name);

            }
            catch (Exception xcp)
            {
                Console.Out.WriteLine("Got exception: {0}\n", xcp.Message);
                return 2;
            }

            return 0;
        }
    }
}
