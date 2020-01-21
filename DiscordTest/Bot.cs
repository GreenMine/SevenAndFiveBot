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

namespace SevenAndFiveBot
{
    class Bot
    {

        static DiscordClient discord;
        private static Config _config;
        private static AccountConnector connector;

        private const string path_to_config = "config.json";
        private static List<PrivateChannel> private_channels = new List<PrivateChannel>();
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
                                await e.Message.RespondAsync(embed: new DiscordEmbedBuilder() { Title = "Вы успешно взяли свою ежедневную наградуs в 50 космикскоинов", Color = DiscordColor.Green }.Build());
                                await account.addMoney(50);
                                await account.setDailyReward();
                            }else
                            {
                                await e.Message.RespondAsync(embed: new DiscordEmbedBuilder() { Title = "Вы уже брали сегодня вознаграждение", Color = DiscordColor.Red }.Build());
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
                                        await e.Message.RespondAsync("Success kicked.");
                                    }
                                    else
                                    {
                                        await e.Message.RespondAsync("Данный пользователь отсутсвует в вашем приват-канале.");
                                    }
                                }
                                else
                                {
                                    await e.Message.RespondAsync("У вас отсутсвуют приват-каналы.");
                                }
                            }
                            else
                            {
                                await e.Message.RespondAsync("Вы не указали ссылку на пользователя которого хотите кикнуть.");
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
