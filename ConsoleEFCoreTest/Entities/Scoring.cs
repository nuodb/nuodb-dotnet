using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEFCoreTest.Entities
{
    [Table("SCORING")]
    public class Scoring
    {
        [Column("PLAYERID")]
        public string PlayerId { get; set; }

        public Player Player {get; set; }

        [Column("YEAR")]
        public int Year { get; set; }

        [Column("STINT")]
        public int Stint { get; set; }

        [Column("TEAMID")]
        public string TeamId { get; set; }
        
        [Column("Position")]
        public string Position { get; set; }

        [Column("GAMESPLAYED")]
        public int GamesPlayed { get; set; }

        [Column("GOALS")]
        public int Goals { get; set; }

        [Column("ASSISTS")]
        public int Assists { get; set; }

        [Column("PENALTYMINUTES")]
        public int PenaltyMinutes { get; set; }

    }
}
