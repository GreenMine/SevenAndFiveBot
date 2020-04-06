using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SevenAndFiveBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using DSharpPlus.Interactivity;


namespace SevenAndFiveBot.Commands
{
    class TypicalCommands : BaseCommandModule
    {

        [Command("report")]
        [Description("Отправка репорта на пользователя")]
        public async Task Report(CommandContext ctx, [Description("Текст репорта")] params string[] report_text)
        {
            if (report_text.Length == 0)
                throw new System.ArgumentException();
            DiscordChannel to_report = ctx.Guild.GetChannel(669948353236303893); // NEED ADD REPORT CHANNEL IN JSON CONFIG
            ctx.RespondAsync(embed: Helper.SuccessEmbed("Репорт успешно отправлен")); // MAYBE AWAIT?? IDK

            DiscordMessage message = await to_report.SendMessageAsync(embed: new DiscordEmbedBuilder() { Title = $"Репорт от пользователя {ctx.User.Username}#{ctx.User.Discriminator}({ctx.User.Id})", Color = DiscordColor.Yellow, Footer = new EmbedFooter() { Text = ctx.User.Username + "#" + ctx.User.Discriminator, IconUrl = ctx.User.AvatarUrl } }.AddField("Текст репорта: ", string.Join(" ", report_text)).Build());
            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            
            await message.CreateReactionAsync(emoji);
        }

        [Command("info")]
        [Description("Информация о боте")]
        public async Task Info(CommandContext ctx)
        {
            var upt = new DateTime((DateTime.Now - Process.GetCurrentProcess().StartTime).Ticks).ToString("ddд. HH:mm:ss");
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder { Color = (DiscordColor)4020122, ThumbnailUrl = ctx.Client.CurrentUser.AvatarUrl, Title = "Информация о боте" }.AddField(":timer:Uptime: " + upt, ":ping_pong: Ping: " + ctx.Client.Ping + "мс.");
            await ctx.RespondAsync(embed: builder.Build());
        }

    }
}
