using ConsoleEFCoreTest.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConsoleEFCoreTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();
            var connectionString = config["ConnectionStrings:NuoDb"];


            var context = new DataContext(connectionString);
            //var players = context.Players.OrderBy(x=>x.LastName).Skip(3).Take(10).ToList();
            var scorings = context.Scorings
                .Include(x=>x.Player)
                .Where(x=>x.Player.FirstName == "Justin")
                .OrderByDescending(x=>x.Year)
                .ThenBy(x=>x.Player.LastName)
                .Take(10)
                .ToList();
            
            Console.ReadLine();
        }
    }
}
