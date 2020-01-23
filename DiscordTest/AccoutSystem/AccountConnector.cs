using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using SevenAndFiveBot.AccoutSystem.Entities;
using MySql.Data.MySqlClient;
using static SevenAndFiveBot.AccoutSystem.User;

namespace SevenAndFiveBot.AccoutSystem
{
    class AccountConnector
    {
        internal MySqlWorker worker;

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
            return new User(this.worker) { Id = Convert.ToUInt64(table["id"]), UserId = (ulong)table["user_id"], Money = (int)table["money"], VoiceOnline = (uint)table["voice_online"], Level = (uint)table["level"], DailyReward = table["daily_reward"].ToString(), Warns = ToWarns((string)table["warn"]) };
        }

        public Warns ToWarns(string warns)
        {
            string[] array_of_warns = warns.Split(',');
            Warns return_warn = new Warns();
            foreach(string value in array_of_warns)
            {
                switch(value)
                {
                    case "1":
                        return_warn.First = true;
                    break;
                    case "2":
                        return_warn.Second = true;
                    break;
                    case "3":
                        return_warn.Third = true;
                   break;
                }
            }
            return return_warn;
        }
/*        public LevelOfWarns[] ToWarns(string warns)
        {
            string[] array_of_warns = warns.Split(',');
            LevelOfWarns[] return_warns = new LevelOfWarns[3];
            for(int i = 0; i < array_of_warns.Length; 
            {
                return_warns[i] = (LevelOfWarns)Convert.ToInt32(array_of_warns[i]);
            }
            return return_warns;
        }*/
    }
}
