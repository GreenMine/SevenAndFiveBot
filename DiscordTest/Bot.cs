using SevenAndFiveBot.AccoutSystem;
using SevenAndFiveBot.Entities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SevenAndFiveBot.Entities.PrivateSystem;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using System.Diagnostics;
using SevenAndFiveBot.AccoutSystem.Games;
using SevenAndFiveBot.AccoutSystem.Entities;

namespace SevenAndFiveBot
{
    class Bot
    {

        static DiscordClient discord;
        private static Config _config;
        private static AccountConnector connector;

        private const string path_to_config = "config.json";
        private static List<PrivateChannel> private_channels = new List<PrivateChannel>();
        private static List<RandomGame> random_duel = new List<RandomGame>();
        internal static async Task MainTask(string[] args)
        {

            if (!File.Exists(path_to_config))
                new Config().SaveToFile(path_to_config);
            _config = Config.LoadFromFile(path_to_config); // Load config

            string connectionString = "server=localhost;user=root;database=sevenandfive;password=;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connector = new AccountConnector(connection);
            connection.Open();

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            }); ;
            discord.MessageCreated += Discord_MessageCreated;
            discord.VoiceStateUpdated += Discord_VoiceStateUpdated;
            await discord.ConnectAsync();
            privateChecher();
            await Task.Delay(-1);
        }

        public static async void privateChecher()
        {
            while (true)
            {
                for (int i = private_channels.Count - 1; i >= 0; i--)
                {
                    DiscordChannel channel = private_channels[i].channel;
                    if (channel.Users.Count() <= 0)
                    {
                        await channel.DeleteAsync();
                        private_channels.Remove(private_channels[i]);
                    }
                }
                await Task.Delay(10000);
            }
        }

        private static async Task Discord_VoiceStateUpdated(DSharpPlus.EventArgs.VoiceStateUpdateEventArgs e)
        {
            if (e.Channel != null)
            {
                if (e.Channel.Id == _config.PrivateId)
                {
                    int current_user = hasPrivateChannel(private_channels, e.User);
                    if (current_user == -1)
                    {
                        DiscordChannel parent = e.Guild.GetChannel(_config.PrivateCategoryId); // Get category
                        DiscordOverwriteBuilder builder = new DiscordOverwriteBuilder()
                        {
                            Allowed = (Permissions)66061584,
                            Denied = (Permissions)0,
                        }.For((DiscordMember)e.User); // Create overwrite for creator of channel
                        DiscordOverwriteBuilder[] overwrites = { builder };

                        Task<DiscordChannel> new_channel = e.Guild.CreateChannelAsync("Private by " + e.User.Username, ChannelType.Voice, parent, userLimit: 3, overwrites: overwrites); // Create new private channel
                        private_channels.Add(new PrivateChannel() { channel = new_channel.Result, user = e.User });
                        await new_channel.Result.PlaceMemberAsync((DiscordMember)e.User); // Move admin(creator of channel) to new private channel
                    }
                    else
                    {
                        DiscordMember this_member = (DiscordMember)e.User;
                        await this_member.PlaceInAsync(private_channels[current_user].channel);
                        await this_member.SendMessageAsync("Вы не можете создать еще один приватный канал, т.к у вас уже есть 1 приватный канал. Перемещаю вас в ваш приват-канал");
                        //await this_member.ModifyAsync(delegate (MemberEditModel model) { model.VoiceChannel = null; }); // Kick user from channel

                    }
                }
                Console.WriteLine(e.User.Username + " connected to " + e.Channel.Name);
            }
        }

        private static async Task Discord_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string message = e.Message.Content;
            if (!e.Author.IsBot)
            {
                if (e.Message.Content.StartsWith('!'))
                {
                    string[] command = e.Message.Content.Remove(0, 1).ToLower().Split(' ');
                    //Console.WriteLine("Your command=" + command);
                    User account = await connector.FindUser(e.Author.Id);
                    e.Message.DeleteAsync();
                    switch (command[0])
                    {
                        case "профиль":
                            //Console.WriteLine("Count users=" + e.Message.MentionedUsers.Count);
                            if (e.Message.MentionedUsers.Count > 0)
                            {
                                DiscordUser user = e.Message.MentionedUsers.ElementAt(0);
                                //Need create new variable, where find the user, and delete from GetProfilePretty()
                                await e.Message.RespondAsync(embed: await GetProfilePretty(user)); // Get mentioned user
                            }
                            else
                            {
                                await e.Message.RespondAsync(embed: await GetProfilePretty(e.Author)); // Get his profile
                            }
                            break;
                        case "бонус":
                            if(account.DailyReward != Helper.getDailyTime())
                            {
                                await e.Message.RespondAsync(embed: new DiscordEmbedBuilder() { Title = "Вы успешно взяли свою ежедневную наградуs в 50 космикскоинов.", Color = DiscordColor.Green }.Build());
                                await account.addMoney(50);
                                await account.setDailyReward();
                            }else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы уже брали сегодня вознаграждение."));//Вы уже брали сегодня вознаграждение
                            }
                            break;
                        case "передать":
                            //Stopwatch sw = new Stopwatch();
                            //sw.Start();
                            if (e.Message.MentionedUsers.Count > 0)
                            {
                                if(command.Length >= 3)
                                {
                                    int money_transfer = 0;
                                    Int32.TryParse(command[2], out money_transfer);
                                    if (money_transfer > 0)
                                    {
                                        if(account.Money >= money_transfer)
                                        {
                                            DiscordUser transfered_to_user = e.Message.MentionedUsers.ElementAt(0);
                                            User transfered_to_account = await connector.FindUser(transfered_to_user.Id);
                                            await account.addMoney(-money_transfer);
                                            await transfered_to_account.addMoney(money_transfer);
                                            await e.Message.RespondAsync(embed: Helper.SuccessEmbed($"Успешно передано {money_transfer} космикскоинов пользователю {transfered_to_user.Username}#{transfered_to_user.Discriminator}"));
                                        }
                                        else
                                        {
                                            await e.Message.RespondAsync(embed: Helper.ErrorEmbed("У вас недостаточно средств."));
                                        }
                                    }else
                                    {
                                        await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Сумма передачи должна быть больше нуля."));
                                    }
                                }else
                                {
                                    await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы не указали сумму которую хотите передать."));
                                }
                            }else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы не указали ссылку на пользователя которому хотите отправить деньги."));
                            }
                            //sw.Stop();
                            //await e.Message.RespondAsync(embed: Helper.SuccessEmbed("Время выполнения команды передать: " + (sw.ElapsedMilliseconds/* / 100.0*/).ToString() + " мс."));
                            break;
                        case "дуэль":
                            if (e.Message.MentionedUsers.Count > 0)
                            {
                                if (command.Length >= 3)
                                {
                                    int money_duel = 0;
                                    Int32.TryParse(command[2], out money_duel);
                                    if (money_duel > 0 && account.Money >= money_duel)
                                    {
                                        DiscordUser rival_to_user = e.Message.MentionedUsers.ElementAt(0);
                                        User rival = await connector.FindUser(rival_to_user.Id);
                                        if(rival.Money >= money_duel)
                                        {
                                            await account.addMoney(-money_duel);
                                            random_duel.Add(new RandomGame(account, rival, money_duel, DateTime.Now));
                                            await e.Message.RespondAsync(embed: Helper.SuccessEmbed($"Успешно отправлено приглашение на {money_duel} космикскоинов пользователю {rival_to_user.Username}#{rival_to_user.Discriminator}. Чтобы принять приглашение напишите !принять @{e.Author.Username}#{e.Author.Discriminator}"));
                                            //NEED SEND MESSAGE
                                        }else
                                        {
                                            await e.Message.RespondAsync(embed: Helper.ErrorEmbed($"У васшего соперника({rival_to_user.Username}#{rival_to_user.Discriminator}) недостаточно средств."));
                                        }
                                    }else
                                    {
                                        await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Ставка меньше нуля либо у вас недостаточно средств."));
                                    }
                                }
                                else
                                {
                                    await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы не указали сумму дуэли."));
                                }
                            }
                            else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы не указали ссылку на пользователя которому хотите отправить заявку на дуэль."));
                            }
                            break;
                        case "принять":
                            if (e.Message.MentionedUsers.Count > 0)
                            {
                                bool has_game = false;
                                int key_of_game = 0;
                                DiscordUser rival_user = e.Message.MentionedUsers.ElementAt(0);
                                User rival = await connector.FindUser(rival_user.Id);
                                for (int i = 0; i < random_duel.Count; i++)
                                {
                                    if (random_duel[i].First == rival && random_duel[i].Second == account)
                                    {
                                        has_game = true;
                                        key_of_game = i;
                                        break;
                                    }
                                }
                                if(has_game)
                                {
                                    RandomGame this_game = random_duel[key_of_game];
                                    if(account.Money >= this_game.Bet)
                                    {
                                        await account.addMoney(-random_duel[key_of_game].Bet);
                                        User winner = this_game.getWinner();
                                        await winner.addMoney(this_game.Bet * 2);
                                        DiscordMember member_winner = e.Guild.Members[winner.UserId];
                                        await e.Message.RespondAsync(embed: Helper.SuccessEmbed($"Пользователь {member_winner.Username}#{member_winner.Discriminator} победил и получает {this_game.Bet} космикскоинов."));
                                        random_duel.RemoveAt(key_of_game);
                                    }else
                                    {
                                        await e.Message.RespondAsync(embed: Helper.ErrorEmbed("У вас недостаточно средств."));
                                    }
                                }else
                                {
                                    await e.Message.RespondAsync(embed: Helper.ErrorEmbed($"Пользователь {rival_user.Username}#{rival_user.Discriminator} не отправлял вам дуэль."));
                                }
                            }else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы не указали ссылку на пользователя у которого хотите принять дуэль."));                            
                            }
                            break;
                        case "топ":
                            DiscordEmbedBuilder embed_of_top = new DiscordEmbedBuilder() { Color = (DiscordColor)13419374 };
                            int number = 1;
                            if (command.Length > 1)
                            {
                                int count = 5;
                                if(command.Length > 2)
                                {
                                    Int32.TryParse(command[2], out count);
                                    if(count > 30)
                                    {
                                        await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Количество пользователей в топе не может превышать 30."));
                                        break;
                                    }
                                }
                                switch (command[1])
                                {
                                    case "онлайн":
                                        embed_of_top.Title = "Топ " + count + " по онлайну в войсах: ";
                                        await foreach (TopReturn top in connector.worker.getTopByDesc("voice_online", count))
                                        {
                                            embed_of_top.AddField("😱", $"{number}. <@{top.UserId}>: {top.Value} м.");
                                            number++;
                                        }
                                        break;
                                    default:
                                        embed_of_top.Title = "Топ " + count + " по деньгам: ";
                                        await foreach (TopReturn top in connector.worker.getTopByDesc("money", count))
                                        {
                                            embed_of_top.AddField("😱", $"{number}. <@{top.UserId}>: {top.Value}");
                                            number++;
                                        }
                                        break;
                                }
                                await e.Message.RespondAsync(embed: embed_of_top.Build());
                            }else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Нужно указать тип топа"));
                            }
                            break;
                        case "skick":
                            if (e.Message.MentionedUsers.Count > 0)
                            {
                                DiscordUser need_kick_user = e.Message.MentionedUsers.ElementAt(0);
                                int key = hasPrivateChannel(private_channels, e.Author);
                                if (key != -1)
                                {
                                    var current_user_channel = ((DiscordMember)need_kick_user)?.VoiceState?.Channel;
                                    if (current_user_channel != null && current_user_channel.Id == private_channels[key].channel.Id)
                                    {
                                        await ((DiscordMember)need_kick_user).ModifyAsync(delegate (MemberEditModel model) { model.VoiceChannel = null; }); // Kick user from channel
                                        await current_user_channel.AddOverwriteAsync((DiscordMember)need_kick_user, (Permissions)0, Permissions.UseVoice);
                                        await e.Message.RespondAsync(embed: Helper.SuccessEmbed("Success kicked."));
                                    }
                                    else
                                    {
                                        await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Данный пользователь отсутсвует в вашем приват-канале."));
                                    }
                                }
                                else
                                {
                                    await e.Message.RespondAsync(embed: Helper.ErrorEmbed("У вас отсутсвуют приват-каналы."));
                                }
                            }
                            else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Вы не указали ссылку на пользователя которого хотите кикнуть."));
                            }
                            break;
                        case "report":
                            string[] report_reason = e.Message.Content.Split(' ', 2);
                            if(report_reason.Length > 1)
                            {
                                DiscordChannel to_report = e.Guild.GetChannel(669948353236303893); // NEED ADD REPORT CHANNEL IN JSON CONFIG
                                e.Message.RespondAsync(embed: Helper.SuccessEmbed("Репорт успешно отправлен")); // MAYBE AWAIT?? IDK
                                await to_report.SendMessageAsync(embed: new DiscordEmbedBuilder() { Title = $"Репорт от пользователя {e.Author.Username}#{e.Author.Discriminator}({e.Author.Id})", Color = DiscordColor.Yellow }.AddField("Текст репорта: ", report_reason[1]).Build());
                            }else
                            {
                                await e.Message.RespondAsync(embed: Helper.ErrorEmbed("Отсутвует сообщение репорта."));
                            }
                            break;
                    }
                }
            }
        }

        public static async Task<DiscordEmbed> GetProfilePretty(DiscordUser user)
        {
            User account = await connector.FindUser(user.Id);
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Author = new EmbedAuthor() { Name = user.Username + "#" + user.Discriminator, Url = "https://discordapp.com", IconUrl = user.AvatarUrl },
                Title = "Профиль:",
                Color = DiscordColor.Blue
            };
            builder.AddField("Космикскоинов: " + account.Money, "На сервере: с " + ((DiscordMember)user).JoinedAt);
            builder.AddField("Онлайн в войсах: " + account.getPrettyOnline(), "<:thinking:>");
            return builder.Build();
        }
    }
}
