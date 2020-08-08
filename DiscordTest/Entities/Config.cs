using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SevenAndFiveBot.Entities
{
    internal class Config
    {

        [JsonProperty("guild_id")]
        internal ulong GuildId = 145522741481701376;

        [JsonProperty("private_id")]
        internal ulong PrivateId = 622753007871852596;

        [JsonProperty("private_category_id")]
        internal ulong PrivateCategoryId = 622752938867163138;

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
