using SevenAndFiveBot.AccoutSystem.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.AccoutSystem
{
    class Reps
    {

        private MySqlWorker Worker;

        public ulong Id = 0;
        public uint PlusRep = 0;
        public uint MinusRep = 0;
        public List<uint> ListPlusRep;
        public List<uint> ListMinusRep;

        public Reps(MySqlWorker worker)
        {
            Worker = worker;
        }

        public TypeOfRep hasUser(uint id)
        {
            TypeOfRep type = (TypeOfRep)(-1);
            type = searchInList(id, ListPlusRep) ? TypeOfRep.Plus : type;
            type = searchInList(id, ListMinusRep) ? TypeOfRep.Minus : type;
            return type;
        }

        public void addRep(uint id, TypeOfRep type)
        {
            if (type == TypeOfRep.Plus)
                Add(id, ref ListPlusRep, "plus_rep");
            else
                Add(id, ref ListMinusRep, "minus_rep");
        }

        public void deleteRep(uint id, TypeOfRep type)
        {
            if (type == TypeOfRep.Plus)
                Delete(id, ref ListPlusRep, "plus_rep");
            else
                Delete(id, ref ListMinusRep, "minus_rep");
        }

        private void Add(uint id, ref List<uint> list, string name)
        {
            list.Add(id);
            Worker.UpdateRepsAsync(Id, name, string.Join(',', list));
        }

        private void Delete(uint id, ref List<uint> list, string name)
        {
            list.Remove(id);
            Worker.UpdateRepsAsync(Id, name, string.Join(',', list), true);
        }

        private bool searchInList(uint id, List<uint> list)
        {
            foreach (uint current_id in list)
                if (current_id == id)
                    return true;
            return false;
        }

    }
}
