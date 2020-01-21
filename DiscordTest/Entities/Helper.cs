using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.Entities
{
    class Helper
    {
        public static string getDailyTime()
        {
            DateTime now = DateTime.Now;
            return now.Day + ":" + now.Month;
        }
    }
}
