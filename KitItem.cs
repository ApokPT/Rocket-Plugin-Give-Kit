using System.Xml.Serialization;

namespace ApokPT.RocketPlugins

{
    public class KitItem
    {

        public KitItem() { }

        public KitItem(ushort itemId, byte amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        [XmlAttribute("id")]
        public ushort ItemId;

        [XmlAttribute("amount")]
        public byte Amount;
    }
}
