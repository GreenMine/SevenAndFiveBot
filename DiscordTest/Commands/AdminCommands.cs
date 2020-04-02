using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SevenAndFiveBot.AccoutSystem;
using SevenAndFiveBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.Commands
{
    [Hidden]
    [RequirePermissions(Permissions.BanMembers)]
    class AdminCommands : BaseCommandModule
    {
        private AccountConnector Connector;
        private ulong[] warnRoles =
        {
            694870243616620605, // WARN 1
            694870246200311958, // WARN 2
            694870288503930920  // WARN 3
        };

        public AdminCommands(AccountConnector connector)
        {
            Connector = connector;
        }


        [Command("ban")]
        [Description("Бан пользователя")]
        public async Task Ban(CommandContext ctx, DiscordMember member)
        {
            await member.BanAsync(reason: "БАН ПО ПРИЧИНЕ ПИДОРАС!");
            await ctx.RespondAsync(embed: Helper.SuccessEmbed($"Пользователь {member.Username}#{member.Discriminator} успешно забанен."));
        }

        [Command("warn")]
        [Description("Добавить варн пользователю")]
        public async Task Warn(CommandContext ctx, [Description("User to warn")] DiscordMember member)
        {
            User warn_user = await Connector.FindUser(member.Id);
            if (warn_user.Warns == 3)
                throw new InvalidOperationException("У пользователя максимальное количество репортов!");
            await warn_user.addWarn();
            member.GrantRoleAsync(ctx.Guild.Roles[warnRoles[warn_user.Warns - 1]], $"warn by {ctx.User.Username}#{ctx.User.Discriminator}!!!");
            await ctx.RespondAsync(embed: Helper.SuccessEmbed($"Пользователю {member.Username}#{member.Discriminator} успешно добавлен варн."));
        }

        [Command("unwarn")]
        [Description("Удалить варн пользователю")]
        public async Task Unwarn(CommandContext ctx, [Description("User to warn")] DiscordMember member)
        {
            User warn_user = await Connector.FindUser(member.Id);
            if (warn_user.Warns == 0)
                throw new InvalidOperationException("У пользователя и так нет репортов!");
            member.RevokeRoleAsync(ctx.Guild.Roles[warnRoles[warn_user.Warns - 1]], $"unwarn by {ctx.User.Username}#{ctx.User.Discriminator}!!!");
            await warn_user.unWarn();
            await ctx.RespondAsync(embed: Helper.SuccessEmbed($"Пользователю {member.Username}#{member.Discriminator} успешно снятен варн."));
        }

    }
}
