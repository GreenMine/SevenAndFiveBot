using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.AccoutSystem.Games
{
    class RandomGame : IGame
    {
        public User First { get; set; }
        public User Second { get; set; }
        public int Bet { get; set; }
        public DateTime StartSearch { get; set; }

        public RandomGame(User first, User second, int bet, DateTime start_search)
        {
            First = first;
            Second = second;
            Bet = bet;
            StartSearch = start_search;
        }
        public User getWinner()
        {
            Random random = new Random();
            int random_value = random.Next(1, 3);
            if (random_value == 1)
                return First;
            return Second;
        }
    }
}
