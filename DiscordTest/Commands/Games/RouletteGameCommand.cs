using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SevenAndFiveBot.AccoutSystem;
using SevenAndFiveBot.AccoutSystem.Games.Roulette;
using SevenAndFiveBot.Entities;
using SevenAndFiveBot.Exceptions.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.Commands.Games
{
    class RouletteGameCommand : BaseCommandModule
    {

        private AccountConnector Connector;
        private RouletteGame Game;
        public RouletteGameCommand(AccountConnector connector, RouletteGame game)
        {
            Connector = connector;
            Game = game;
        }

        [Command("bet")]
        [Description("Поставить ставку")]
        public async Task Bet(CommandContext ctx, [Description("Ставка")] uint bet, [Description("Цвет")] TypeOfBet typeBet)
        {
            if (Game.State == GameState.Stopped)
                throw new SendMessageException("Сейчас нет доступных игр.", ctx.User);
            if (bet < 50)
                throw new SendMessageException("Ставка должна быть больше 50.", ctx.User);
            User account = await Connector.FindUser(ctx.User.Id);
            if (account.Money < bet)
                throw new SendMessageException("Недостаточно средств!", ctx.User);
            await account.addMoney(-(int)bet);
            Game.addBet(await Connector.FindUser(ctx.User.Id), ctx.User.Username + "#" + ctx.User.Discriminator, typeBet, bet);
            await ((DiscordMember)ctx.User).SendMessageAsync(embed: Helper.SuccessEmbed("Ставка успешно принята"));
        }

        [Command("predict_next")]
        [Description("TSSSSSSSSSSS")]
        [Hidden]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task PredictNext(CommandContext ctx, TypeOfBet type)
        {
            Game.setPredict(type);
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("omg u whore! stop do that!"));
        }

    }
}
