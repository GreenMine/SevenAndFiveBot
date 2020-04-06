using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SevenAndFiveBot.AccoutSystem;
using SevenAndFiveBot.AccoutSystem.Shop;
using SevenAndFiveBot.Entities;
using SevenAndFiveBot.Entities.TempRoles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.Commands.CShop
{
    class ShopCommands : BaseCommandModule
    {

        private AccountConnector Connector;
        private ShopWorker ShopWorker;
        private FileList<Roles> Roles;
        public ShopCommands(AccountConnector connector, ShopWorker worker, FileList<Roles> roles)
        {
            Connector = connector;
            ShopWorker = worker;
            Roles = roles;
        }

        [Command("shop")]
        [Description("Список предметов в магазине")]
        public async Task Shop(CommandContext ctx)
        {
            DiscordEmbedBuilder shop_embed = new DiscordEmbedBuilder() { Title = "Список предметов магазина", Color = DiscordColor.Yellow };
            for (int i = 0; i < ShopWorker.Items.Count; i++)
            {
                ShopItem shop_item = ShopWorker.Items[i];
                switch (shop_item.Type)
                {
                    case ItemType.Role:
                        shop_embed.AddField($"{i + 1}. {shop_item.Price}:dollar:", $"Роль: <@&{shop_item.Reward}> ", true);
                        break;
                    case ItemType.Sign:
                        shop_embed.AddField($"{i + 1}. {shop_item.Price}:dollar:", "Роспись от космикса", true);
                        break;
                    case ItemType.CustomRole:
                        shop_embed.AddField($"{i + 1}. {shop_item.Price}:dollar:", "Пользовательская роль на " + shop_item.Reward + "м.", true);
                        break;
                }
            }
            await ctx.RespondAsync(embed: shop_embed.Build());
        }

        [Command("buy")]
        [Description("Полупка предмета в магазине")]
        public async Task Buy(CommandContext ctx, [Description("Номер предмета в магазине")] int item_id, params string[] data)
        {
            if (item_id == 0 || ShopWorker.Items.Count <= item_id - 1)
                throw new InvalidOperationException("Неверно указана айди предмета.");
            ShopItem current_item = ShopWorker.getItemById(item_id - 1);
            User account = await Connector.FindUser(ctx.User.Id);

            if (account.Money < current_item.Price)
                throw new InvalidOperationException("Недостаточно средств.");
            switch (current_item.Type)
            {
                case ItemType.Role:
                    DiscordRole role = ctx.Guild.GetRole(Convert.ToUInt64(current_item.Reward));
                    Console.WriteLine($"Buy role {role.Id} btw :D");
                    await ((DiscordMember)ctx.User).GrantRoleAsync(role);
                    await account.addMoney(-(int)current_item.Price);
                    await ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно куплен"));
                    break;
                case ItemType.Sign:
                    if (data.Length == 0)
                        throw new InvalidOperationException("Для данного предмета так-же надо указать ссылку на ваш VK.");
                    ulong to_user_id = 144686801687281664;
                    DiscordMember to_member = ctx.Guild.Members[to_user_id];
                    await to_member.SendMessageAsync($"Куплена роспись пользователем {ctx.User.Username}#{ctx.User.Discriminator}{Environment.NewLine}VK: {String.Join(" ", data)}");
                    await account.addMoney(-(int)current_item.Price);
                    await ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно куплен."));
                    break;
                case ItemType.CustomRole:
                    if (data.Length == 0)
                        throw new InvalidOperationException("Нужно указать цвет в формате HEX(с # вначале).");
                    if (!data[0].StartsWith('#'))
                        throw new InvalidOperationException("Цвет должен быть в формате Hex(цвет можно выбрать тут https://htmlcolors.com/google-color-picker).");
                    if (data.Length <= 1)
                        throw new InvalidOperationException("Нужно указать название роли.");
                    await account.addMoney(-(int)current_item.Price);
                    string name_of_role = "";
                    for (int i = 1; i < data.Length; i++)
                        name_of_role += data[i] + " ";
                    DiscordRole new_role = await ctx.Guild.CreateRoleAsync(name_of_role, color: Convert.ToInt32(data[0].Remove(0, 1), 16));
                    Roles.addTempRole(new Roles(new_role.Id, DateTime.Now.AddMinutes(Double.Parse(current_item.Reward))));
                    ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно куплен."));
                    await ((DiscordMember)ctx.User).GrantRoleAsync(new_role);
                    break;
            }
        }

        [Command("deleteitem")]
        [Description("Удаление предмета из магазина")]
        [Hidden]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task DeleteItem(CommandContext ctx, int item_id)
        {
            if (item_id == 0)
                throw new InvalidOperationException("ID был указан неверно.");
            await ShopWorker.deleteItem(item_id - 1);
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно удален."));
        }

    }

    [Group("additem")]
    [Description("Добавление предмета в магазин")]
    [Hidden]
    [RequirePermissions(Permissions.BanMembers)]
    class AddItemGroup : BaseCommandModule
    {

        private ShopWorker ShopWorker;

        public AddItemGroup(ShopWorker worker)
        {
            ShopWorker = worker;
        }

        [Command("role")]
        [Description("Добавляем в мазагин роль")]
        public async Task Role(CommandContext ctx, [Description("Цена")]uint price, [Description("@ на роль которую хотите добавить")]DiscordRole role)
        {
            await ShopWorker.addItem(new ShopItem() { Type = ItemType.Role, Price = price, Reward = role.Id.ToString() });
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно добавлен в магазин(id: " + ShopWorker.Items.Count + ")."));
        }

        [Command("sign")]
        [Description("Добавляем в мазагин роспись")]
        public async Task Sign(CommandContext ctx, [Description("Цена")]uint price)
        {
            await ShopWorker.addItem(new ShopItem() { Type = ItemType.Sign, Price = price });
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно добавлен в магазин(id: " + ShopWorker.Items.Count + ")."));
        }

        [Command("custom_role")]
        [Description("Добавляем в мазагин кастомную роль")]
        public async Task CustomRole(CommandContext ctx, [Description("Цена")]uint price, [Description("Время на сколько будет даваться роль(в минутах)")]string duration)
        {
            await ShopWorker.addItem(new ShopItem() { Type = ItemType.CustomRole, Price = price, Reward = duration });
            await ctx.RespondAsync(embed: Helper.SuccessEmbed("Предмет успешно добавлен в магазин(id: " + ShopWorker.Items.Count + ")."));
        }

    }
}
