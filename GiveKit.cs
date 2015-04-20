using Rocket;
using Rocket.Logging;
using Rocket.RocketAPI;
using Rocket.RocketAPI.Events;
using SDG;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;
using System.Threading;

namespace ApokPT.RocketPlugins
{
    class GiveKit : RocketPlugin<GiveKitConfiguration>
    {
        public static GiveKit Instance = null;

        public static Dictionary<string, DateTime> GlobalCooldown = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> IndividualCooldown = new Dictionary<string, DateTime>();

        protected override void Load()
        {
            Instance = this;
            if (GiveKit.Instance.Configuration.ResetCooldownOnDeath)
            {
                Rocket.RocketAPI.Events.RocketPlayerEvents.OnPlayerDeath += RocketPlayerEvents_OnPlayerDeath;
            }
            Rocket.RocketAPI.Events.RocketServerEvents.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
        }

        private Timer timer;

        private void RocketServerEvents_OnPlayerConnected(RocketPlayer player)
        {
            byte v = 0;
            List<Kit> kits = new List<Kit>();
            if (getGlobalCooldown(player) <= 0)
            {
                foreach (Kit kit in GiveKit.Instance.Configuration.Kits)
                {
                    if (player.Permissions.Contains("givekit.onjoin." + kit.Name.ToLower()) && getKitCooldown(player, kit.Name) <= 0)
                    {
                        kits.Add(kit);
                        v++;
                    }
                }
                timer = new System.Threading.Timer(obj => { foreach (Kit kit in kits) Give(player, kit.Name, player.CharacterName); timer.Dispose(); }, null, 500, Timeout.Infinite);
            }
        }


        private void RocketPlayerEvents_OnPlayerDeath(RocketPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (cause != EDeathCause.SUICIDE)
            {
                if (GlobalCooldown.ContainsKey(player.ToString()))
                {
                    GlobalCooldown.Remove(player.ToString());
                }

                foreach (Kit kit in Configuration.Kits)
                {
                    if (IndividualCooldown.ContainsKey(player.ToString() + kit.Name))
                    {
                        IndividualCooldown.Remove(player.ToString() + kit.Name);
                    }
                }

            }
        }

        public override Dictionary<string, string> DefaultTranslations
        {
            get
            {
                return new Dictionary<string, string>(){
                    {"command_givekit_available_kits","Kits: {0}."},
                    {"command_givekit_instructions_share","Type /givekit <kit name>/<player*> *player is optional"},
                    {"command_givekit_instructions","Type /givekit <kit name>"},
                    {"command_givekit_player_not_found","No player found named {0}!"},
                    {"command_givekit_kit_not_found","{0} kit not found!"},
                    {"command_givekit_delivered_to_player","Kit {0} delivered to {1}!"},
                    {"command_givekit_recieved_by_player","You have recieved a {0} kit!"},
                    {"command_givekit_no_permissions","You have no permitions to use /givekit!"},
                    {"command_givekit_no_permissions_kit","You have no permitions to use {0} kit!"},
                    {"command_givekit_no_permissions_share","You have no permitions to share kits!"},
                    {"command_givekit_cooldown_command","You have to wait {0} seconds to use /givekit again!"},
                    {"command_givekit_cooldown_kit","You have to wait {0} seconds for this kit again"}
                };
            }
        }

        internal static void Strip(RocketPlayer player)
        {
            if (GiveKit.Instance.Configuration.StripBeforeGiving)
            {
                player.Inventory.Clear();
            }
        }

        internal static Kit getKitByString(string kitId)
        {
            foreach (Kit kit in Instance.Configuration.Kits)
            {
                if (kit.Name.ToLower() == kitId.ToLower())
                {
                    return kit;
                }
            }
            return null;
        }

        internal static void Give(RocketPlayer caller, string kitId, string player)
        {

            RocketPlayer target = RocketPlayer.FromName(player);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            Kit selectedKit = getKitByString(kitId);

            if (target == null)
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_player_not_found", textInfo.ToTitleCase(player)));
                return;
            }


            if (selectedKit == null)
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_kit_not_found", textInfo.ToTitleCase(kitId)));
                return;
            }

            Strip(target);

            foreach (KitItem item in selectedKit.Items)
            {
                target.GiveItem(item.ItemId, item.Amount);
            }

            RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_delivered_to_player", textInfo.ToTitleCase(kitId), target.CharacterName));

            if (!caller.Equals(target))
            {
                RocketChatManager.Say(target, GiveKit.Instance.Translate("command_givekit_recieved_by_player", textInfo.ToTitleCase(kitId)));
            }

            updateCooldowns(caller, selectedKit);

        }

        private static void updateCooldowns(RocketPlayer caller, Kit kit)
        {
            if (GlobalCooldown.ContainsKey(caller.ToString()))
            {
                GlobalCooldown[caller.ToString()] = DateTime.Now;
            }
            else
            {
                GlobalCooldown.Add(caller.ToString(), DateTime.Now);
            }

            if (GlobalCooldown.ContainsKey(caller.ToString()))
            {
                IndividualCooldown[caller.ToString() + kit.Name] = DateTime.Now;
            }
            else
            {
                IndividualCooldown.Add(caller.ToString() + kit.Name, DateTime.Now);
            }
        }

        internal static double getGlobalCooldown(RocketPlayer caller)
        {
            if (GiveKit.GlobalCooldown.ContainsKey(caller.ToString()))
            {
                double lastRequest = (DateTime.Now - GiveKit.GlobalCooldown[caller.ToString()]).TotalSeconds;
                if (lastRequest < GiveKit.Instance.Configuration.GlobalCooldown)
                {
                    return (int)(GiveKit.Instance.Configuration.GlobalCooldown - lastRequest);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        internal static double getKitCooldown(RocketPlayer caller, string kitId)
        {
            Kit kit = getKitByString(kitId);

            if (kit != null && GiveKit.IndividualCooldown.ContainsKey(caller.ToString() + kit.Name))
            {
                double lastRequest = (DateTime.Now - GiveKit.IndividualCooldown[caller.ToString() + kit.Name]).TotalSeconds;

                if (lastRequest < kit.Cooldown)
                {
                    return (int)(kit.Cooldown - lastRequest);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}