using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleEFCoreTest.Entities
{
    [Table("PLAYERS")]
    public class Player
    {
        [Key]
        [Column("PLAYERID")]
        public string Id { get; set; }

        [Column("FIRSTNAME")]
        public string FirstName { get; set; }

        [Column("LASTNAME")]
        public string LastName { get; set; }

        [Column("HEIGHT")]
        public int Height { get; set; }

        [Column("WEIGHT")]
        public int Weight { get; set; }


        [Column("FIRSTNHL")]
        public int FirstNHL { get; set; }

        [Column("POSITION")]
        public string Position { get; set; }


    }
}
