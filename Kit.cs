using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class Kit
    {
        public Kit() { }

        public string Name;

        [XmlArrayItem(ElementName = "Item")]
        public List<KitItem> Items;
        public int Cooldown = 0;
    }
}
