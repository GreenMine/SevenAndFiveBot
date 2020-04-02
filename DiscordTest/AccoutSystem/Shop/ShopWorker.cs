using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.AccoutSystem.Shop
{
    class ShopWorker
    {
        public List<ShopItem> Items { get; set; }

        private MySqlConnection connection;
        private const string table_name = "shop";

        public ShopWorker(MySqlConnection connect)
        {
            Items = new List<ShopItem>();
            this.connection = connect;
            loadAllItems(table_name);
        }

        public ShopItem getItemById(int item_id)
        {
            if (Items.Count >= item_id && item_id >= 0)
                return Items[item_id];
            throw new Exception("Missing item_id");
        }
        public async Task addItem(ShopItem shop_item)
        {
            MySqlCommand request = new MySqlCommand($"INSERT INTO `{table_name}` (`price`, `type`, `reward`) VALUES ('{shop_item.Price}', '{(int)shop_item.Type}', '{shop_item.Reward}')", connection);
            await request.ExecuteNonQueryAsync();
            Items.Add(shop_item);
        }

        public async Task deleteItem(int item_id)
        {
            ShopItem current_item = Items.ElementAt(item_id);
            MySqlCommand request = new MySqlCommand($"DELETE FROM `{table_name}` WHERE `id` = {current_item.UniqueId}", connection);
            await request.ExecuteNonQueryAsync();
            Items.RemoveAt(item_id);
        }

        public void loadAllItems(string table_name = "shop")
        {
            MySqlCommand request = new MySqlCommand($"SELECT * FROM `{table_name}`", connection);
            MySqlDataReader reader = request.ExecuteReader();
            while(reader.Read())
            {
                Items.Add(new ShopItem() {UniqueId = (uint)reader[0], Price = (uint)reader[1], Type = (ItemType)Convert.ToByte(reader[2]), Reward = reader[3].ToString() });
            }
            reader.Close();
        }
    }
}
