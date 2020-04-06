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

        public void Add(T data)
        {
            MainList.Add(data);
            SaveToFile();
        }

        public void Delete(T role)
        {
            MainList.Remove(role);
        }

        public IEnumerable<T> GetList()
        {
            for (int i = MainList.Count - 1; i >= 0; i--)
                yield return MainList[i];
        }

        public void SaveToFile()
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
