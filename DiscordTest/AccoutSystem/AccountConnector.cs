using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using SevenAndFiveBot.AccoutSystem.Entities;
using MySql.Data.MySqlClient;
using static SevenAndFiveBot.AccoutSystem.User;
using System.Linq;
using System.Collections.Generic;

namespace SevenAndFiveBot.AccoutSystem
{
    class AccountConnector
    {
        internal MySqlWorker worker;

        public event Action<User> LevelUpdate;
        public AccountConnector(MySqlConnection conn)
        {
            this.worker = new MySqlWorker(conn, this);
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

        public async Task<Reps> FindRep(ulong user_id)
        {
            DataRow user_row = await worker.GetReps(user_id);
            //Console.WriteLine("Nil? " + user == null ? "Nil" : "Dont nil");
            if (user_row == null)
            {
                await worker.CreateUser(user_id);
                user_row = await worker.GetReps(user_id);
            }
            return ToReps(user_row); 
        }

        private User ToUser(DataRow table)
        {
            return new User(this.worker) { Id = Convert.ToUInt64(table["id"]), UserId = (ulong)table["user_id"], Money = (int)table["money"], VoiceOnline = (uint)table["voice_online"], Level = (uint)table["level"], DailyReward = table["daily_reward"].ToString(), Warns = (short)table["warn"], PlusRep = (uint)table["plus_rep"], MinusRep = (uint)table["minus_rep"] };
        }

        private Reps ToReps(DataRow table)
        {
            return new Reps(this.worker) { Id = Convert.ToUInt64(table["id"]), PlusRep = (uint)table["plus_rep"], MinusRep = (uint)table["minus_rep"], ListPlusRep = StringToList(table["list_plus_rep"]), ListMinusRep = StringToList(table["list_minus_rep"]) };
        }

        public void InvokeLevel(User user)
        {
            LevelUpdate?.Invoke(user);
        }

        private List<uint> StringToList(object obj)
        {
            if (obj.ToString() == "")
                return new List<uint>();
            return obj.ToString().Split(',').Select(xarg => UInt32.Parse(xarg)).ToList();
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
