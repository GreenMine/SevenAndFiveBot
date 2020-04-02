using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.AccoutSystem.Shop
{
    class ShopConverter : IArgumentConverter<ItemType>
    {
        public Task<Optional<ItemType>> ConvertAsync(string value, CommandContext ctx)
        {
            return value switch
            {
                "role" => Task.FromResult(Optional.FromValue(ItemType.Role)),
                "sign" => Task.FromResult(Optional.FromValue(ItemType.Sign)),
                "custom_role" => Task.FromResult(Optional.FromValue(ItemType.CustomRole)),
                _ => Task.FromResult(Optional.FromValue(ItemType.Role))
            };
        }
    }
}
