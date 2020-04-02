using SevenAndFiveBot.AccoutSystem.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.AccoutSystem
{
    class Reps
    {

        private MySqlWorker Worker;

        public uint PlusRep = 0;
        public uint MinusRep = 0;
        public List<uint> ListPlusRep;
        public List<uint> ListMinusRep;

        public Reps(MySqlWorker worker)
        {
            Worker = worker;
        }

        public bool hasUser(uint id)
        {
            foreach (uint current_id in ListPlusRep)
                if (current_id == id)
                    return true;
            return false;
        }

        //public async Task addRep()

    }
}
