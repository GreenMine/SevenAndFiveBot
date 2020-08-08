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

		public Config() {
			Token = Config.getEnv("T");
			GuildId = ulong.Parse(Config.getEnv("GID"));
			PrivateId = ulong.Parse(Config.getEnv("PID"));
			PrivateCategoryId = ulong.Parse(Config.getEnv("PCID"));
		}

		private static string getEnv(string name) {
			return Environment.GetEnvironmentVariable("SAF_DS_" + name);
		}
    }
}
