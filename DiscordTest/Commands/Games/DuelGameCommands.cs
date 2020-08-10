using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SevenAndFiveBot.AccoutSystem;
using SevenAndFiveBot.AccoutSystem.Games;
using SevenAndFiveBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.Commands.Games
{
    class DuelGameCommands : BaseCommandModule
    {

        private AccountConnector Connector;

        private List<RandomGame> randomDuel;

        public DuelGameCommands(AccountConnector connector)
        {
            Connector = connector;
            randomDuel = new List<RandomGame>();
        }

        [Command("дуэль")]
        [Description("Отправить заявку на дуэль")]
        public async Task Duel(CommandContext ctx, [Description("@ на пользователя которому хотите отправить дуэль")] DiscordUser rival_to_user, [Description("Сумму на которую хотите сыграть дуэль")] int money_duel)
        {
            User account = Connector.FindUser(ctx.User.Id);
            if (money_duel <= 0 || account.Money < money_duel)
                throw new InvalidOperationException("Ставка меньше нуля либо у вас недостаточно средств.");
            User rival = Connector.FindUser(rival_to_user.Id);
            if (rival.Money < money_duel)
                throw new InvalidOperationException($"У васшего соперника({rival_to_user.Username}#{rival_to_user.Discriminator}) недостаточно средств.");
            await account.addMoney(-money_duel);
            randomDuel.Add(new RandomGame(account, rival, money_duel, DateTime.Now));
            await ctx.RespondAsync(embed: Helper.SuccessEmbed($"Успешно отправлено приглашение на {money_duel} космикскоинов пользователю {rival_to_user.Username}#{rival_to_user.Discriminator}. Чтобы принять приглашение напишите !принять @{ctx.User.Username}#{ctx.User.Discriminator}"));
            //NEED SEND MESSAGE
        }

        [Command("принять")]
        [Description("Принять заявку на дуэль")]
        public async Task AcceptDuel(CommandContext ctx, [Description("@ на пользователя у которого хотите принять дуэль")] DiscordUser rival_user)
        {
            bool has_game = false;
            int key_of_game = 0;
            User account = Connector.FindUser(ctx.User.Id);
            User rival = Connector.FindUser(rival_user.Id);
            for (int i = 0; i < randomDuel.Count; i++)
            {
                if (randomDuel[i].First == rival && randomDuel[i].Second == account)
                {
                    has_game = true;
                    key_of_game = i;
                    break;
                }
            }
            if (!has_game)
                throw new InvalidOperationException($"Пользователь {rival_user.Username}#{rival_user.Discriminator} не отправлял вам дуэль.");
            RandomGame this_game = randomDuel[key_of_game];
            if (account.Money < this_game.Bet)
                throw new InvalidOperationException("У вас недостаточно средств.");
            await account.addMoney(-randomDuel[key_of_game].Bet);
            User winner = this_game.getWinner();
            await winner.addMoney(this_game.Bet * 2);
            DiscordMember member_winner = ctx.Guild.Members[winner.UserId];
            await ctx.RespondAsync(embed: Helper.SuccessEmbed($"Пользователь {member_winner.Username}#{member_winner.Discriminator} победил и получает {this_game.Bet} космикскоинов."));
            randomDuel.RemoveAt(key_of_game);
        }

    }
}
