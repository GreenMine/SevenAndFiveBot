using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SevenAndFiveBot.Entities.TempRoles
{
    class TempRoles
    {

        public List<Roles> Roles;

        private string Filename;

        public TempRoles(string filename)
        {
            Filename = filename;
            LoadFromFile();
        }

        public void addTempRole(ulong roleId, DateTime endTime)
        {
            Roles.Add(new Roles() { RoleId = roleId, EndTime = endTime });
            saveToFile();
        }

        public void deleteTempRole(Roles role)
        {
            Roles.Remove(role);
        }

        public void saveToFile()
        {
            File.WriteAllText(Filename, JsonConvert.SerializeObject(Roles));
        }

        private void LoadFromFile()
        {
            if (!File.Exists(Filename))
                File.WriteAllText(Filename, "");
            using (var sr = new StreamReader(Filename))
            {
                List<Roles> tempRoles = JsonConvert.DeserializeObject<List<Roles>>(sr.ReadToEnd());
                if (tempRoles == null)
                    Roles = new List<Roles>();
                else
                    Roles = tempRoles;
            }
        }
    }
}
