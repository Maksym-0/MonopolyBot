using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models.Bot
{
    internal class User
    {
        public long ChatId { get; set; }
        public string UserId { get; set; }
        public string? GameId { get; set; }
        public string Name { get; set; }
        public string JWT { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
