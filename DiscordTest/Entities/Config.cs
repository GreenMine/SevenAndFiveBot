using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SevenAndFiveBot.Entities
{
    internal class Config
    {

		internal string Token = "";
        internal ulong GuildId = 0;
        internal ulong PrivateId = 0;
        internal ulong PrivateCategoryId = 0;
		internal ulong CasinoId = 0;

		public Config() {
			Token = Config.getEnv("T");
			GuildId = ulong.Parse(Config.getEnv("GUILD_ID"));
			PrivateId = ulong.Parse(Config.getEnv("PRIVATE_ID"));
			PrivateCategoryId = ulong.Parse(Config.getEnv("PRIVATE_CATEG_ID"));
			CasinoId = ulong.Parse(Config.getEnv("CASINO_ID"));
		}

		private static string getEnv(string name) {
			return Environment.GetEnvironmentVariable("SAF_DS_" + name);
		}
    }
}
