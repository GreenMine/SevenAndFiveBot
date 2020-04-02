using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.Exceptions.Command
{
    class SendMessageException : Exception
    {

        public DiscordUser User { get; set; }

        public SendMessageException(string message, DiscordUser user) : base(message)
        {
            User = user;
        }
    }
}
