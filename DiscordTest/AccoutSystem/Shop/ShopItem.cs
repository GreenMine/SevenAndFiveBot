using System;
using System.Collections.Generic;
using System.Text;

namespace SevenAndFiveBot.AccoutSystem.Shop
{
    
    public enum ItemType
    {
        Role,
        Sign,
        CustomRole
    }

    class ShopItem
    {
        public uint UniqueId { get; set; }
        public uint Price { get; set; }
        public ItemType Type { get; set; }
        public string Reward { get; set; } = "";
    }
}
