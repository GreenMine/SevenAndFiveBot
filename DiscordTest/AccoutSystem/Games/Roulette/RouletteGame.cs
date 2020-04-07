using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.AccoutSystem.Games.Roulette
{
    public enum TypeOfBet
    {
        Red,
        Black,
        Green
    }

    enum GameState
    {
        Waiting,
        Stopped
    }

    struct Winner
    {
        public TypeOfBet Type;
        public int Number;
    }


    class RouletteGame : IGame
    {

        private const int MAX_PREV_GAMES = 8;

        public List<RouletteUser>[] betters = {
            new List<RouletteUser>(), //RED
            new List<RouletteUser>(), //BLACK
            new List<RouletteUser>(), //GREEN
        };
        private string[] bettersUsername = {
            "", //RED
            "", //BLACK
            ""  //GREEN
        };
        private DiscordMessage rouletteMessage;
        private DiscordEmbedBuilder builder;
        private TypeOfBet predict = (TypeOfBet)228;

        public GameState State { get; set; } = GameState.Waiting;
        public int TimeLeft { get; set; } = 40;
        private List<TypeOfBet> PrevGames = new List<TypeOfBet>();
        private string renderMessagePrevGames = ":arrow_right:";
        public RouletteGame()
        {
            builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Yellow,
                Title = "Рулетка",
                ImageUrl = "https://www.thesun.co.uk/wp-content/uploads/2005/08/Roulette-seo.jpg",
                Description = "**Правила игры:** Игроки совершают ставки на игровом поле, после чего крутится колесо рулетки, в которое запущен шарик. Шарик, сделав несколько оборотов, попадает в одну из пронумерованных ячеек, определяя таким образом выигрышный номер.\n**Как поставить:** Для ставки требуется написать команду !bet ставка цвет(Пример !bet 200 красное)\n**Коеф. цветов:** Красное - 2x, Черное - 2x, Зеленое - 14x."
            };
            updateFields();
        }

        public Winner getWinner()
        {
            if (predict != (TypeOfBet)228)
            {
                TypeOfBet pred = predict;
                predict = (TypeOfBet)228;
                return new Winner { Number = pred == TypeOfBet.Green ? 0 : (pred == TypeOfBet.Red ? 4 : 8), Type = pred  };
            }
            int random = new Random().Next(0, 16);
            TypeOfBet type = 0;
            if (random == 0)
                type = TypeOfBet.Green;
            else if (random > 0 && random <= 7)
                type = TypeOfBet.Red;
            else
                type = TypeOfBet.Black;
            return new Winner { Number = random, Type = type };
        }

        public async Task SetWinners(Winner winners)
        {
            builder.ClearFields().AddField("Выпал цвет: **" + winners.Type.ToString() + " - " + winners.Number + "**", "Следующая игра начнется через 10сек.")
                                 .AddField("Победители", bettersUsername[(int)winners.Type] + ":point_up::point_up::point_up:");
            await rouletteMessage.ModifyAsync(embed: builder.Build());
            State = GameState.Stopped;
            if (PrevGames.Count == MAX_PREV_GAMES)
                PrevGames.RemoveAt(0);
            PrevGames.Add(winners.Type);
            await Task.Delay(100);
            renderPrevGames();
            await Task.Delay(9000);
        }

        public async Task Start(DiscordChannel to_channel)
        {
            rouletteMessage = await to_channel.SendMessageAsync(embed: builder.Build());
        }

        public void addBet(User user, string username, TypeOfBet typeBet, uint bet)
        {
            betters[(int)typeBet].Add(new RouletteUser { BetUser = user, Username = username, Bet = bet });
            bettersUsername[(int)typeBet] += username + " - " + bet + "\n";
            modifyMessage();
            //rouletteMessage.ModifyAsync($"СТАВКИ:\nКрасное: {string.Join(", ", betters[0])}\nЧерное: {string.Join(", ", betters[1])}\nЗеленое: {string.Join(", ", betters[2])}\n");
        }

        public void setPredict(TypeOfBet pred)
        {
            predict = pred;
        }

        public async Task StartTimer()
        {
            int time = 0;
            if (TimeLeft > 20)
                time = 20;
            else if (TimeLeft <= 20)
                time = 10;
            await Task.Delay(time * 930);
            TimeLeft -= time;
            if (TimeLeft <= 0)
                return;
            await modifyMessage();
            await StartTimer();
        }

        public async Task Restart()
        {
            betters[0].Clear();
            betters[1].Clear();
            betters[2].Clear();
            bettersUsername[0] = "";
            bettersUsername[1] = "";
            bettersUsername[2] = "";

            TimeLeft = 40;
            State = GameState.Waiting;

            builder.ClearFields();
            updateFields();

            await rouletteMessage.DeleteAsync();
        }

        private void renderPrevGames()
        {
            renderMessagePrevGames = ":arrow_right:";
            for(int i = PrevGames.Count-1; i >= 0; i--)
            {
                switch(PrevGames[i])
                {
                    case TypeOfBet.Black:
                        renderMessagePrevGames += ":black_circle:";
                    break;
                    case TypeOfBet.Green:
                        renderMessagePrevGames += ":green_circle:";
                    break;
                    case TypeOfBet.Red:
                        renderMessagePrevGames += ":red_circle:";
                    break;
                }
            }
        }


        private async Task modifyMessage()
        {
            updateFields();
            await rouletteMessage.ModifyAsync(embed: builder.Build());
        }

        private void updateFields()
        {
            builder.ClearFields().AddField("**Предыдущие игры**", renderMessagePrevGames)
                                 .AddField("Времени осталось: " + TimeLeft + "сек.", "Состояние игры: **" + (State == GameState.Waiting ? "Ожидание" : "Закончено") + "**")
                                 .AddField("Красное", bettersUsername[0] == "" ? "Никого" : bettersUsername[0], true)
                                 .AddField("Черное", bettersUsername[1] == "" ? "Никого" : bettersUsername[1], true)
                                 .AddField("Зеленое", bettersUsername[2] == "" ? "Никого" : bettersUsername[2], true);
        }
    }
}
