using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.Entities.TempRoles
{
    class Roles
    {
        [JsonProperty("role_id")]
        public ulong RoleId = 0;
        [JsonProperty("end_time")]
        public DateTime EndTime;

    }
}
