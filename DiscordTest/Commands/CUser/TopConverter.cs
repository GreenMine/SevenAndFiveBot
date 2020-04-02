using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.Commands.CUser
{
    class TopConverter : IArgumentConverter<TypeOfTop>
    {
        public Task<Optional<TypeOfTop>> ConvertAsync(string value, CommandContext ctx)
        {
            return value switch
            {
                "деньги" => Task.FromResult(Optional.FromValue(TypeOfTop.Money)),
                "онлайн" => Task.FromResult(Optional.FromValue(TypeOfTop.VoiceOnline)),
                _ => Task.FromResult(Optional.FromValue(TypeOfTop.VoiceOnline))
            };
        }

    }
}
