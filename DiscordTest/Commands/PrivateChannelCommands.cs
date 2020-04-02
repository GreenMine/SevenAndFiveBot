using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using SevenAndFiveBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static SevenAndFiveBot.Entities.PrivateSystem;

namespace SevenAndFiveBot.Commands
{
    class PrivateChannelCommands : BaseCommandModule
    {

        private static List<PrivateChannel> private_channels = new List<PrivateChannel>();

        public PrivateChannelCommands(List<PrivateChannel> privateChannels)
        {
            private_channels = privateChannels;
        }

        [Command("skick")]
        [Description("Кик пользователя из приват канала")]
        public async Task Skick(CommandContext ctx, [Description("@ на пользователя которого хотите кикнуть")]DiscordMember user_to_kick)
        {
            int key = hasPrivateChannel(private_channels, ctx.User);
            if (key == -1)
                throw new InvalidOperationException("У вас отсутсвуют приват-каналы.");
            var current_user_channel = user_to_kick?.VoiceState?.Channel;
            if (current_user_channel == null || current_user_channel.Id != private_channels[key].channel.Id)
                throw new InvalidOperationException("Данный пользователь отсутсвует в вашем приват-канале.");
            await user_to_kick.ModifyAsync(delegate (MemberEditModel model) { model.VoiceChannel = null; }); // Kick user from channel
            await current_user_channel.AddOverwriteAsync(user_to_kick, (Permissions)0, Permissions.UseVoice); // Forbidden connect to the channel
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("Успешно кикнут.")); // Congrad! All be good :)

        }
    }
}
