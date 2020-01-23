using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SevenAndFiveBot
{
    class Program
    {
        static void Main(string[] args)
        {
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

            AccountConnector connector = new AccoutConnector(connection);
            connection.Open();
            doAsync(connector);
            connection.Close();*/
            Bot.MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();
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
