using MonopolyBot.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models.Service
{
    internal class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string Message { get; set; }
        public User? User { get; set; }
    }
}
