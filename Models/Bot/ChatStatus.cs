using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models.Bot
{
    internal class ChatStatus
    {
        public long ChatId { get; set; }

        public bool IsAwaitingLogin { get; set; } = false;
        public bool IsAwaitingRegister { get; set; } = false;
        public bool IsAwaitingDeleteAccount { get; set; } = false;
        public bool IsAwaitingJoinRoom { get; set; } = false;
        public bool IsAwaitingCreateRoom { get; set; } = false;
        public bool IsAwaitingCreateRoomPassword { get; set; } = false;
        public bool IsAwaitingLevelUpCell { get; set; } = false;
        public bool IsAwaitingLevelDownCell { get; set; } = false;

        public string? AccountName { get; set; } = null;
        public string? RoomId { get; set; } = null;
        public int? MaxNumberOfPlayers { get; set; } = null;

        public ChatStatus(long chatId)
        {
            this.ChatId = chatId;
        }
    }
}
