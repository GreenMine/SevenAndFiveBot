using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.Entities
{
    class Helper
    {
        public static string getDailyTime()
        {
            DateTime now = DateTime.Now;
            return now.Day + ":" + now.Month;
        }

        public static DiscordEmbed ErrorEmbed(string value)
        {
            return new DiscordEmbedBuilder()
            {
                Title = value,
                Color = DiscordColor.Red
            }.Build();
        }

        public static DiscordEmbed SuccessEmbed(string value)
        {
            return new DiscordEmbedBuilder()
            {
                Title = value,
                Color = DiscordColor.Green
            }.Build();
        }
    }
}
