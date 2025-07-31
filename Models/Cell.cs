using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models
{
    internal class Cell
    {
        public string GameId { get; set; }
        public bool Unique { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int? Price { get; set; }
        public int? Rent { get; set; }
        public string? Owner { get; set; }
        public int Level { get; set; }
    }
}
