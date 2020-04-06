using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.Entities
{
    class Mute
    {
        [JsonProperty("user_id")]
        public ulong UserId = 0;
        [JsonProperty("end_time")]
        public DateTime EndTime;

        public  Mute(ulong userId, DateTime endTime)
        {
            UserId = userId;
            EndTime = endTime;
        }
    }
}
