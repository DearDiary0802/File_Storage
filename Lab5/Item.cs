using System;

namespace Lab5
{
    public class Item
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public Item(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
