using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SevenAndFiveBot.Entities
{
    class FileList<T>
    {

        public List<T> MainList;

        private string Filename;

        public FileList(string filename)
        {
            Filename = filename;
            LoadFromFile();
        }

        public void addTempRole(T data)
        {
            MainList.Add(data);
            saveToFile();
        }

        public void deleteTempRole(T role)
        {
            MainList.Remove(role);
        }

        public IEnumerable<T> getList()
        {
            for (int i = MainList.Count - 1; i >= 0; i--)
                yield return MainList[i];
        }

        public void saveToFile()
        {
            File.WriteAllText(Filename, JsonConvert.SerializeObject(MainList));
        }

        private void LoadFromFile()
        {
            if (!File.Exists(Filename))
                File.WriteAllText(Filename, "");
            using (var sr = new StreamReader(Filename))
            {
                List<T> tempRoles = JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd());
                if (tempRoles == null)
                    MainList = new List<T>();
                else
                    MainList = tempRoles;
            }
        }
    }
}
