using SevenAndFiveBot.AccoutSystem.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SevenAndFiveBot.Entities;

namespace SevenAndFiveBot.AccoutSystem
{

    enum TypeOfRep
    {
        Plus,
        Minus
    }
    
    class User
    {

        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public int Money { get; set; } = 0;
        public uint VoiceOnline { get; set; } = 0;
        public uint Level { get; set; } = 0;
        public string DailyReward { get; set; } = "";
        public short Warns { get; set; } = 0;
        public uint PlusRep { get; set; } = 0;
        public uint MinusRep { get; set; } = 0;

        private MySqlWorker worker;
        internal User(MySqlWorker work)
        {
            this.worker = work;
        }

        public async Task addMoney(int count)
        {
            Money += count;
            await worker.UpdateValueAsync(Id, "money", Money);
        }

        public async Task setMoney(int count)
        {
            Money = count;
            await worker.UpdateValueAsync(Id, "money", Money);
        }

        public async Task addWarn()
        {
            Warns++;
            await worker.UpdateValueAsync(Id, "warn", Warns);
        }

        public async Task unWarn()
        {
            Warns--;
            await worker.UpdateValueAsync(Id, "warn", Warns);
        }

        public async Task addLevel()
        {
            Level++;
            await worker.UpdateValueAsync(Id, "level", Level);
            worker.connector.InvokeLevel(this);
        }

        public async Task addRep(TypeOfRep type)
        {
            if (type == TypeOfRep.Plus)
            {
                PlusRep++;
                await worker.UpdateValueAsync(Id, "plus_rep", PlusRep);
            }else
            {
                MinusRep++;
                await worker.UpdateValueAsync(Id, "minus_rep", MinusRep);
            }
        }

        public async Task addVoiceOnline(int count)
        {
            VoiceOnline += (uint)count;
            await worker.UpdateValueAsync(Id, "voice_online", VoiceOnline);
        }
        public async Task setDailyReward()
        {
            await worker.UpdateValueAsync(Id, "daily_reward", Helper.getDailyTime());
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
