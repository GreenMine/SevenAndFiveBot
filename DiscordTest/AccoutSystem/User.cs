using SevenAndFiveBot.AccoutSystem.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SevenAndFiveBot.Entities;

namespace SevenAndFiveBot.AccoutSystem
{
    
    public class Warns
    {
        public bool First { get; set; } = false;
        public bool Second { get; set; } = false;
        public bool Third { get; set; } = false;
    }
    class User
    {

        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public int Money { get; set; } = 0;
        public uint VoiceOnline { get; set; } = 0;
        public uint Level { get; set; } = 0;
        public string DailyReward { get; set; } = "";
        public Warns Warns { get; set; } = new Warns();

        private MySqlWorker worker;
        internal User(MySqlWorker work)
        {
            this.worker = work;
        }

        public async Task addMoney(int count)
        {
            Money += count;
            await worker.UpdateValueAsync(UserId, "money", Money);
        }

        public async Task setMoney(int count)
        {
            Money = count;
            await worker.UpdateValueAsync(UserId, "money", Money);
        }

        public async Task addLevel()
        {
            Level++;
            await worker.UpdateValueAsync(UserId, "level", Level);
        }

        public async Task setDailyReward()
        {
            await worker.UpdateValueAsync(UserId, "daily_reward", Helper.getDailyTime());
        }

        public string getPrettyOnline()
        {
            if(VoiceOnline > 60) {
                int hours = (int)VoiceOnline / 60;
                int minutes = (int)VoiceOnline - hours * 60;
                return hours + "ч. " + minutes + "м.";
            }
            return VoiceOnline + "м.";
        }

        public static bool operator ==(User u1, User u2)
        {
            if (u1.Id == u2.Id)
                return true;
            return false;
        }

        public static bool operator !=(User u1, User u2)
        {
            if (u1.Id != u2.Id)
                return true;
            return false;
        }
    }
}
