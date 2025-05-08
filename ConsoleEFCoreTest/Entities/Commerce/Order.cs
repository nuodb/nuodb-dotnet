using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEFCoreTest.Entities.Commerce
{
    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
