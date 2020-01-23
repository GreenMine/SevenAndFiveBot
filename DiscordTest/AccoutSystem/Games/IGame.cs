using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.AccoutSystem
{
    enum TypeOfGame
    {
        OneVsOne,
        Everyone
    }
    interface IGame
    {
        public User getWinner();
    }
}
