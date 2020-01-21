using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.Entities
{
    class PrivateSystem
    {
        public struct PrivateChannel
        {
            public DiscordChannel channel;
            public DiscordUser user;
        }

        public static int hasPrivateChannel(List<PrivateChannel> privateChannels, DiscordUser discordUser)
        {
            for(int i = 0; i < privateChannels.Count; i++)
            {
                if(privateChannels[i].user == discordUser)
                    return i;
            }
            return -1;
        }
    }
}
