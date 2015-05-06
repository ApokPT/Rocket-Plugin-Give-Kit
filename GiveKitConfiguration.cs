using Rocket.RocketAPI;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class GiveKitConfiguration : IRocketConfiguration
    {
        public bool Enabled = true;

        [XmlArrayItem(ElementName = "Kit")]
        public List<Kit> Kits;

        public int GlobalCooldown = 10;
        public bool StripBeforeGiving = true;
        public bool ResetCooldownOnDeath = true;

        public IRocketConfiguration DefaultConfiguration
        {
            get
            {
                GiveKitConfiguration configuration = new GiveKitConfiguration();
                configuration.Kits = new List<Kit>() { 
                    new Kit("Survival", 10, new List<KitItem>() { new KitItem(245, 1), new KitItem(81, 2), new KitItem(16, 1) }),
                    new Kit("Brute Force", 10, new List<KitItem>() { new KitItem(112, 1), new KitItem(113, 3), new KitItem(254, 3) }),
                    new Kit("Watcher", 10, new List<KitItem>() { new KitItem(109, 1), new KitItem(111, 3), new KitItem(236, 1) })
                };
                Enabled = true;
                GlobalCooldown = 10;
                StripBeforeGiving = true;
                ResetCooldownOnDeath = true;
                return configuration;
            }
        }
    }
}