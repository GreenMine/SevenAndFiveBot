using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using SevenAndFiveBot.AccoutSystem;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using SevenAndFiveBot.Entities;
using SevenAndFiveBot.AccoutSystem.Entities;

namespace SevenAndFiveBot.Commands.CUser
{
    class UserCommands : BaseCommandModule
    {
        private AccountConnector Connector;
        private Levels[] Levels;

        public UserCommands(AccountConnector connector, Levels[] levels)
        {
            Connector = connector;
            Levels = levels;
        }

        [Command("профиль")]
        [Description("Получение информации о пользователе")]
        public async Task Profile(CommandContext ctx, [Description("@ на пользователя")] DiscordMember need_user = null) {
            if(need_user == null)
            {
                await ctx.RespondAsync(embed: await GetProfilePretty(ctx.User)); // Get his profile
            }
            else
            {
                //ctx.RespondAsync($"{need_user.Mention}, you will be mentioned by {ctx.User.Mention}!");
                await ctx.RespondAsync(embed: await GetProfilePretty(need_user));// Get mentioned user
            }
        }

        [Command("бонус")]
        [Description("Получение бонуса(раз в день)")]
        public async Task Bonus(CommandContext ctx)
        {
            User account = await Connector.FindUser(ctx.User.Id);
            if (account.DailyReward == Helper.getDailyTime())
                throw new InvalidOperationException("Вы уже брали сегодня вознаграждение.");
            ctx.RespondAsync(embed: new DiscordEmbedBuilder() { Title = "Вы успешно взяли свою ежедневную наградуs в 50 космикскоинов.", Color = DiscordColor.Green }.Build());
            await account.addMoney(50);
            await account.setDailyReward();
        }


        [Command("передать")]
        [Description("Передать деньги другому пользователю")]
        public async Task Transfer(CommandContext ctx, [Description("Пользователь")] DiscordUser transfered_to_user, [Description("Количество денег")] int money_transfer)
        {
            User account = await Connector.FindUser(ctx.User.Id);
            if (money_transfer <= 0)
                throw new InvalidOperationException("Сумма передачи должна быть больше нуля.");
            if (account.Money < money_transfer)
                throw new InvalidOperationException("У вас недостаточно средств.");
            User transfered_to_account = await Connector.FindUser(transfered_to_user.Id);
            await account.addMoney(-money_transfer);
            await transfered_to_account.addMoney(money_transfer);
            await ctx.RespondAsync(embed: Helper.SuccessEmbed($"Успешно передано {money_transfer} космикскоинов пользователю {transfered_to_user.Username}#{transfered_to_user.Discriminator}"));
        }
        

        [Command("топ")]
        [Description("Вывод топа пользователей дискорда")]
        public async Task Top(CommandContext ctx, [Description("Тип топа")] TypeOfTop type, [Description("Количество")] int count = 5)
        {
            DiscordEmbedBuilder embed_of_top = new DiscordEmbedBuilder() { Color = (DiscordColor)13419374 };
            int number = 1;

            if (count > 30)
                throw new InvalidOperationException("Количество пользователей в топе не может превышать 30.");

            switch (type)
            {
                case TypeOfTop.VoiceOnline:
                    embed_of_top.Title = "Топ " + count + " по онлайну в войсах: ";
                    await foreach (TopReturn top in Connector.worker.getTopByDesc("voice_online", count))
                    {
                        embed_of_top.AddField("😱", $"{number}. <@{top.UserId}>: {top.Value} м.");
                        number++;
                    }
                    break;
                default:
                    embed_of_top.Title = "Топ " + count + " по деньгам: ";
                    await foreach (TopReturn top in Connector.worker.getTopByDesc("money", count))
                    {
                        embed_of_top.AddField("😱", $"{number}. <@{top.UserId}>: {top.Value}");
                        number++;
                    }
                    break;
            }
            await ctx.RespondAsync(embed: embed_of_top.Build());
        }

        [Command("+rep")]
        [Description("Добавляет + к репутации пользователя")]
        public async Task PlusRep(CommandContext ctx, [Description("Пользователю которому хотите отправить +реп")] DiscordUser user)
        {
            await SendRep(ctx, user, TypeOfRep.Plus);
        }

        [Command("-rep")]
        [Description("Добавляет - к репутации пользователя")]
        public async Task MinusRep(CommandContext ctx, [Description("Пользователю которому хотите отправить -реп")] DiscordUser user)
        {
            await SendRep(ctx, user, TypeOfRep.Minus);
        }

        [Command("level__up")]
        [Description("add level")]
        [Hidden]
        public async Task Level_Up(CommandContext ctx)
        {
            User account = await Connector.FindUser(ctx.User.Id);
            await account.addLevel();
        }

        public async Task SendRep(CommandContext ctx, DiscordUser user, TypeOfRep type)
        {
            if (ctx.User.Id == user.Id)
                throw new InvalidOperationException("Охуел отправлять сам себе реп?");
            User current_user = await Connector.FindUser(ctx.User.Id);
            Reps reps_user = await Connector.FindRep(user.Id);
            TypeOfRep rep = reps_user.hasUser((uint)current_user.Id);
            if (rep == type)
                throw new InvalidOperationException("Вы уже отправляли реп данному пользователю");
            if(rep != (TypeOfRep)(-1))
                reps_user.deleteRep((uint)current_user.Id, rep);
            reps_user.addRep((uint)current_user.Id, type);
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("Успешно отправлен реп."));
        }

        public async Task<DiscordEmbed> GetProfilePretty(DiscordUser user)
        {
            User account = await Connector.FindUser(user.Id);
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Author = new EmbedAuthor() { Name = user.Username + "#" + user.Discriminator, Url = "https://discordapp.com", IconUrl = user.AvatarUrl },
                Title = "Профиль:",
                Color = (DiscordColor)12196021
            };
            builder.AddField("**Level**", account.Level > 0 ? Levels[account.Level - 1].Name : "-", true);
            builder.AddField("**Cosmicscoins**", account.Money.ToString(), true);
            builder.AddField("**Voice online**", account.getPrettyOnline(), false);
            builder.AddField("**На сервере с**", ((DiscordMember)user).JoinedAt.ToString("MM.dd.yyyy"), true);
            builder.AddField("**Reputation**", "+" + account.PlusRep + "/-" + account.MinusRep, true);
            builder.AddField("**Count of warns**", account.Warns + "/3", true);
            return builder.Build();
        }
    }
}
