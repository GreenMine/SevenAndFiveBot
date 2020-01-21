using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using SevenAndFiveBot.AccoutSystem.Entities;
using MySql.Data.MySqlClient;

namespace SevenAndFiveBot.AccoutSystem
{
    class AccountConnector
    {
        private MySqlWorker worker;

        public AccountConnector(MySqlConnection conn)
        {
            this.worker = new MySqlWorker(conn);
        }

        public async Task<User> FindUser(ulong user_id)
        {
            User user;
            //return ToUser(await GetUser(user_id));
            DataRow user_row = await worker.GetUser(user_id);
            //Console.WriteLine("Nil? " + user == null ? "Nil" : "Dont nil");
            if (user_row == null)
                user = await worker.CreateUser(user_id);
            else
                user = ToUser(user_row);
            return user;
        }

        public User ToUser(DataRow table)
        {
            return new User(this.worker) { Id = Convert.ToUInt64(table["id"]), UserId = (ulong)table["user_id"], Money = (int)table["money"], VoiceOnline = (uint)table["voice_online"], Level = (uint)table["level"], DailyReward = table["daily_reward"].ToString(), Warn = table["warn"].ToString() };
        }
    }
}
