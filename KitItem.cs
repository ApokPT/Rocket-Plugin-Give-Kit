using System.Xml.Serialization;

namespace ApokPT.RocketPlugins

{
    public class KitItem
    {

        [XmlAttribute("id")]
        public ushort ItemId;

        [XmlAttribute("amount")]
        public byte Amount;

        private KitItem() { }

        public KitItem(ushort itemId, byte amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
