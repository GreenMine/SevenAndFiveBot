﻿//using MySql.Data.MySqlClient;
//using SevenAndFiveBot.AccoutSystem.Shop;

using System;
using SevenAndFiveBot.Entities;

namespace SevenAndFiveBot
{
    class Program
    {
        static void Main(string[] args)
        {
//			CircleBuffer<int> buffer = new CircleBuffer<int>();
//			buffer.Add(0);
//			buffer.Add(55);
//			buffer.Add(43);
//			buffer.Add(72);
//			bool is_found;
//			uint found_value = buffer.Get(v => v != 0, out is_found);
//			if(is_found) {
//				Console.WriteLine("Found value: " + buffer.buffer[found_value]);
//			} else {
//				Console.WriteLine("Value not found");
//			}
//			Environment.Exit(0);
            /*connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                Console.WriteLine("connection_ok");
                string requestStr = "SELECT * FROM `users` WHERE `name` = 'ilya'";
                MySqlCommand request = new MySqlCommand(requestStr, connection);
                MySqlDataReader reader = request.ExecuteReader();
                while(reader.Read())
                {
                    Console.WriteLine(reader[0].ToString() + " " + reader[1].ToString());
                }
            }else
            {
                Console.WriteLine("connection_closed");
            }
            connection.Close();
            */
            //SHOP SYSTEM
            /*string connectionString = "server=localhost;user=root;database=sevenandfive;password=;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            if (connection.State == System.Data.ConnectionState.Open)
                Console.WriteLine("connection+___+ok");
            connection.Open();
            ShopWorker shop = new ShopWorker(connection);
            shop.addItem(new ShopItem() { Price = 500, Type = ItemType.Role, Reward = "670273368204771348" });
            shop.deleteItem(4);
            foreach (ShopItem shop_item in shop.Items)
            {
                Console.WriteLine(shop_item.Price + " " + shop_item.Type + " " + shop_item.Reward);
            }
            connection.Close();*/

			Bot.MainTask().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /*        static async void doAsync(AccoutConnector connector)
                {
                    int iter = 1;
                    while (true)
                    {
                        User user = await connector.FindUser(1978);
                        await user.addLevel();
                        Console.WriteLine(user.Level);
                        //await Task.Delay(5000);
                        Console.WriteLine(iter);

                        iter++;
                    }
                }*/
    }
}
