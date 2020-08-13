using SevenAndFiveBot.AccoutSystem;
using SevenAndFiveBot.Entities;
using DSharpPlus;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SevenAndFiveBot.Entities.PrivateSystem;
using SevenAndFiveBot.AccoutSystem.Games;
using SevenAndFiveBot.AccoutSystem.Entities;
using SevenAndFiveBot.AccoutSystem.Shop;
using SevenAndFiveBot.Commands.CUser;
using SevenAndFiveBot.Commands.Help;
using SevenAndFiveBot.Commands.Games;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using SevenAndFiveBot.Commands;
using SevenAndFiveBot.Commands.CShop;
using DSharpPlus.CommandsNext.Exceptions;
using SevenAndFiveBot.AccoutSystem.Games.Roulette;
using SevenAndFiveBot.Exceptions.Command;
using SevenAndFiveBot.Entities.TempRoles;

namespace SevenAndFiveBot
{
    class Bot
    {

        static DiscordClient discord;
        static CommandsNextExtension commands;

        private static Config _config;
        private static AccountConnector connector;
        private static ShopWorker shop;

        private const string path_to_config = "config.json";
        private static List<PrivateChannel> private_channels = new List<PrivateChannel>();
        private static List<RandomGame> random_duel = new List<RandomGame>();
        private static List<VoiceOnline> voiceOnlines = new List<VoiceOnline>();
        private static RouletteGame rouletteGame = new RouletteGame();

        private static FileList<Roles> tempRoles;
        private static FileList<Mute> mute;
		
        private static Levels[] levels =
        {
            new Levels() {CountMinutes = 10, RoleId = 693078664996192337, Name = "Asteroid"},
            new Levels() {CountMinutes = 100, RoleId = 693079345996103720, Name = "Pluto"},
            new Levels() {CountMinutes = 500, RoleId = 693079566016839701, Name = "Neptune"},
            new Levels() {CountMinutes = 1000, RoleId = 693079612305178665, Name = "Uranus"},
            new Levels() {CountMinutes = 2500, RoleId = 693079703837212683, Name = "Saturn"},
            new Levels() {CountMinutes = 5000, RoleId = 693079763891257464, Name = "Jupiter"},
            new Levels() {CountMinutes = 10000, RoleId = 693079823299379252, Name = "Mars"},
            new Levels() {CountMinutes = 15000, RoleId = 693079906959228968, Name = "Earth"},
            new Levels() {CountMinutes = 30000, RoleId = 693080279513825411, Name = "Venus"},
            new Levels() {CountMinutes = 45000, RoleId = 693080368953294850, Name = "Mercury"},
            new Levels() {CountMinutes = 60000, RoleId = 693080442152157195, Name = "Sun"},
            new Levels() {CountMinutes = 75000, RoleId = 693080769345880115, Name = "Space"},
        };
        /*@Asteroid - 10 минут.
        @Pluto –  100 минут.
        @Neptune – 500 минут.
        @Uranus – 1000 минут.
        @Saturn – 2500 минут.
        @Jupiter – 5000 минут.
        @Mars - 10000 минут.
        @Earth – 15000 минут.
        @Venus – 30000 минут.
        @Mercury – 45000 минут.
        @Sun – 60000 минут.
        @Space – 75000 минут.*/
        internal static async Task MainTask()
        {
            _config = new Config(); // Load config

            tempRoles = new FileList<Roles>("tempRoles.json");
            mute = new FileList<Mute>("mute.json");

//			string conn_string = "server=localhost;user=root;database=sevenandfive;port=3306;password=;default command timeout=3600;";
			string conn_string = $"server={System.Net.Dns.GetHostEntry("eu-cdbr-west-03.cleardb.net").AddressList[0].ToString()};user=bdbec8bf261377;database=heroku_2251c7932429166;port=3306;password=4d06e369;default command timeout=3600;";
            MySqlConnection connection = new MySqlConnection(conn_string);
            connector = new AccountConnector(connection);
            connector.LevelUpdate += Connector_LevelUpdate;
            connection.Open();

            shop = new ShopWorker(connection);

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
                
            });
            //discord.MessageCreated += Discord_MessageCreated;
            discord.VoiceStateUpdated += Discord_VoiceStateUpdated;
            discord.GuildMemberAdded += Discord_GuildMemberAdded;

            var deps = new ServiceCollection()
                                              .AddSingleton(connector)
                                              .AddSingleton(shop)
                                              .AddSingleton(private_channels)
                                              .AddSingleton(rouletteGame)
                                              .AddSingleton(levels)
                                              .AddSingleton(tempRoles)
                                              .AddSingleton(mute)
                                              .BuildServiceProvider();


            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "!", "7.5" },
                Services = deps
            });

            commands.CommandErrored += Commands_CommandErrored;

            commands.RegisterConverter(new TopConverter());
            commands.RegisterUserFriendlyTypeName<TypeOfTop>("тип топа");

            commands.RegisterConverter(new RouletteGameConverter());
            commands.RegisterUserFriendlyTypeName<TypeOfBet>("цвет");

            commands.RegisterCommands<UserCommands>();
            commands.RegisterCommands<TypicalCommands>();
            commands.RegisterCommands<ShopCommands>();
            commands.RegisterCommands<AddItemGroup>();
            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<PrivateChannelCommands>();
            commands.RegisterCommands<DuelGameCommands>();
            commands.RegisterCommands<RouletteGameCommand>();

            commands.SetHelpFormatter<HelpFormatter>();

            await discord.ConnectAsync();


            CasinoWorker();
            
            privateChecher();

            checkerThread();

			await Task.Delay(-1);
        }


        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is System.ArgumentException)
            {
                if (e.Command.Name != "bet")
                {
                    var helpCommand = commands.RegisteredCommands.First(p => p.Key == "help").Value;
                    var fakeContext = commands.CreateFakeContext(e.Context.User, e.Context.Channel, e.Context.Message.Content, "help", helpCommand, e.Command.Name);
                    await commands.ExecuteCommandAsync(fakeContext);
                }else
                {
                    await ((DiscordMember)e.Context.User).SendMessageAsync(embed: Helper.ErrorEmbed("Неверный синтаксис команды!"));
                }
            }
            else if (e.Exception is System.InvalidOperationException)
            {
                await e.Context.RespondAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = e.Exception.Message,
                    Color = DiscordColor.Red
                }.Build());
            }else if(e.Exception is SendMessageException exe)
            {
                await ((DiscordMember)exe.User).SendMessageAsync(embed: Helper.ErrorEmbed(exe.Message));
            }else if(e.Exception is ChecksFailedException ex)
            {
                await e.Context.RespondAsync(embed: new DiscordEmbedBuilder() { Color = DiscordColor.Red, Title = "Ошибка: Недостаточно прав!" }.Build());
            }else if(e.Exception is CommandNotFoundException) {}
            else
            {
                await e.Context.RespondAsync(embed: new DiscordEmbedBuilder() { Color = DiscordColor.Red, Title = "Ошибка" }.AddField("Текст ошибки:", $"{e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}").Build());
            }
            //e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "SevenAndFiveBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
        }

        private static async void Connector_LevelUpdate(User user)
        {
            DiscordGuild guild = discord.Guilds[_config.GuildId];
            DiscordMember member = guild.Members[user.UserId];
            DiscordRole give_role = guild.GetRole(levels[user.Level-1].RoleId);
            await member.GrantRoleAsync(give_role, "Level UP blyat'!");
            await member.SendMessageAsync($"**Поздравляем!** __Вы достигли уровня \"{give_role.Name}\", ваш онлайн составляет {user.VoiceOnline} минут!__");
        }

        public static async void CasinoWorker()
        {
            await Task.Delay(10000);
            DiscordChannel casino_channel = discord.Guilds[_config.GuildId].GetChannel(_config.CasinoId);
            while (true)
            {
                await rouletteGame.Start(casino_channel);
                await rouletteGame.StartTimer();
                if (rouletteGame.betters[0].Count != 0 || rouletteGame.betters[1].Count != 0 || rouletteGame.betters[2].Count != 0)
                {
                    Winner winners = rouletteGame.getWinner();
                    TypeOfBet winners_type = winners.Type;
                    int coef = winners_type == TypeOfBet.Green ? 14 : 2;
                    foreach (RouletteUser user in rouletteGame.betters[(int)winners_type])
                        await user.BetUser.addMoney((int)user.Bet * coef);
                    await rouletteGame.SetWinners(winners);
                }
                await rouletteGame.Restart();
            }
        }

        public static async void privateChecher()
        {
            while (true)
            {
				for(int x = 0; x < 8; x++) Console.WriteLine(connector.users.buffer[x].UserId + " " + connector.users.buffer[x].Money);
                for (int i = private_channels.Count - 1; i >= 0; i--)
                {
                    DiscordChannel channel = private_channels[i].channel;
                    if (channel.Users.Count() <= 0)
                    {
                        try
                        {
                            await channel.DeleteAsync();
                            private_channels.Remove(private_channels[i]);
                        }catch(Exception e){}
                    }
                }
                await Task.Delay(10000);
            }
        }

        public static async void checkerThread()
        {
            await Task.Delay(10000);
            DiscordGuild currentGuild = discord.Guilds[_config.GuildId];

            DiscordRole muteRole = currentGuild.GetRole(696670385617371186);

            while (true)
            {
                foreach(Roles role in tempRoles.GetList())
                {
                    if(role.EndTime <= DateTime.Now)
                    {
                        await currentGuild.GetRole(role.RoleId).DeleteAsync();
                        tempRoles.Delete(role);
                    }
                }
                tempRoles.SaveToFile();

                foreach(Mute muted in mute.GetList())
                {
                    if(muted.EndTime <= DateTime.Now)
                    {
                        DiscordMember need_unmuted = currentGuild.Members[muted.UserId];
                        mute.Delete(muted);
                        need_unmuted.RevokeRoleAsync(muteRole);
                        await need_unmuted.SendMessageAsync(embed: new DiscordEmbedBuilder() { Title = "<:seven_unmute:696300818231459851>Ограничение активности снято.", Description = "Пиздите на здоровье:smiling_imp:", Color = (DiscordColor)65314 }.WithFooter("ARMY Family", "https://sun9-31.userapi.com/c848528/v848528033/147aa6/Xk0MsOtkIDg.jpg"));
                    }
                }
                mute.SaveToFile();

                await Task.Delay(60000);
            }
        }

        private static async Task Discord_VoiceStateUpdated(DSharpPlus.EventArgs.VoiceStateUpdateEventArgs e)
        {
            Console.WriteLine(e.User.Username + " connected to " + e?.Channel?.Name);
            bool has_in_voice_online = false;
            VoiceOnline this_channel_user = null;
            foreach (VoiceOnline online in voiceOnlines)
            {
                if (e.User.Id == online.user.Id)
                {
                    has_in_voice_online = true;
                    this_channel_user = online;
                    break;
                }
            }
            if(has_in_voice_online)
            {
                if(e.Channel == null)// || e.Before != e.After
                {
                    User account = connector.FindUser(e.User.Id);
                    voiceOnlines.Remove(this_channel_user);
                    TimeSpan count_seconds = DateTime.Now - this_channel_user.start;
                    int minutes_in_voice_chat = (int)(count_seconds.TotalSeconds / 60);
                    await account.addVoiceOnline(minutes_in_voice_chat);
                    await account.addMoney(minutes_in_voice_chat);
                    while (account.VoiceOnline >= levels[account.Level].CountMinutes)
                        await account.addLevel();
                }
            }else
            {
                voiceOnlines.Add(new VoiceOnline() { start = DateTime.Now, user = e.User });
            }
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
                        await new_channel.Result.PlaceMemberAsync((DiscordMember)e.User); // Move admin(creator of the channel) to new private channel
                        private_channels.Add(new PrivateChannel() { channel = new_channel.Result, user = e.User });
                    }
                    else
                    {
                        DiscordMember this_member = (DiscordMember)e.User;
                        await this_member.PlaceInAsync(private_channels[current_user].channel);
                        await this_member.SendMessageAsync("Вы не можете создать еще один приватный канал, т.к у вас уже есть 1 приватный канал. Перемещаю вас в ваш приват-канал");
                        //await this_member.ModifyAsync(delegate (MemberEditModel model) { model.VoiceChannel = null; }); // Kick user from channel

                    }
                }
            }
        }

        private static async Task Discord_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Channel.Id == _config.CasinoId && !e.Author.IsBot)
                e.Message.DeleteAsync();
        }

        private static async Task Discord_GuildMemberAdded(DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            User account = connector.FindUser(e.Member.Id);

            if(account.Warns > 0)
            {
                // DO SOMETHING
            }

            if(account.Level > 0)
            {
                // DO SOMETHING
            }
        }
    }
}
