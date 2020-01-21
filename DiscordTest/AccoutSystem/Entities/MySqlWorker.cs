using SevenAndFiveBot.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace SevenAndFiveBot.AccoutSystem.Entities
{
    class MySqlWorker
    {
        private MySqlConnection connection;

        public MySqlWorker(MySqlConnection connect)
        {
            this.connection = connect;
        }

        public async Task<DataRow> GetUser(ulong user_id)
        {
            MySqlCommand request = new MySqlCommand("SELECT * FROM `users` WHERE `user_id` = " + user_id, connection);
            DbDataReader reader = await request.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                await reader.CloseAsync();
                return dt.Rows[0];
            }
            await reader.CloseAsync();
            return null;
        }

        public async Task<User> CreateUser(ulong user_id)
        {
            DateTime now = DateTime.Now;
            MySqlCommand request = new MySqlCommand("INSERT INTO `users` (`user_id`, `money`, `voice_online`, `level`, `daily_reward`, `warn`) VALUES ('" + user_id + "', '0', '0', '0', '" + Helper.getDailyTime() + "', '');SELECT LAST_INSERT_ID();", connection);
            ulong id = (ulong)await request.ExecuteScalarAsync();
            return new User(this) { Id = id, UserId = user_id };
        }

        public async Task UpdateValueAsync(ulong user_id, string field, object value)
        {
            MySqlCommand request = new MySqlCommand("UPDATE `users` SET `" + field + "` = '" + value + "' WHERE `user_id` = " + user_id, connection);
            await request.ExecuteNonQueryAsync();
        }

/*        public async Task<object> GetValueAsync(ulong user_id, string field)
        {
            MySqlCommand request = new MySqlCommand("SELECT " + field + " FROM `users` WHERE `user_id` = " + user_id);
            return await request.ExecuteScalarAsync();
        }*/
    }
}
