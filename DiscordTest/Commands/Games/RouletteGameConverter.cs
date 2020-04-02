using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using SevenAndFiveBot.AccoutSystem.Games.Roulette;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.Commands.Games
{
    class RouletteGameConverter : IArgumentConverter<TypeOfBet>
    {
        public async Task<Optional<TypeOfBet>> ConvertAsync(string value, CommandContext ctx)
        {
            switch(value[0])
            {
                case 'к':
                case 'r':
                    return TypeOfBet.Red;
                case 'ч':
                case 'b':
                    return TypeOfBet.Black;
                case 'з':
                case 'g':
                    return TypeOfBet.Green;
                default:
                    return TypeOfBet.Red;
            }

        }
    }
}
