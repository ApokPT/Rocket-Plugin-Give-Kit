using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class Kit
    {
        public string Name;
        

        [XmlArrayItem(ElementName = "Item")]
        public List<KitItem> Items;
        public double Cooldown;

        private Kit() { }

        public Kit(string name, double cooldown, List<KitItem> items) {
            Name = name;
            Cooldown = cooldown;
            Items = items;
        }
    }
}
