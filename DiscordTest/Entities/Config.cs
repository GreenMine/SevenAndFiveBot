using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SevenAndFiveBot.Entities
{
    internal class Config
    {
        [JsonProperty("token")]
        internal string Token = "token here";

        [JsonProperty("guild_id")]
        internal ulong GuildId = 0;

        [JsonProperty("private_id")]
        internal ulong PrivateId = 0;

        [JsonProperty("private_category_id")]
        internal ulong PrivateCategoryId = 0;

        public static Config LoadFromFile(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }

        public void SaveToFile(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}
