using Rocket.RocketAPI;
using SDG;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ApokPT.RocketPlugins
{
    class GiveKit : RocketPlugin<GiveKitConfiguration>
    {

        public static Dictionary<string, DateTime> GlobalCooldown = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> IndividualCooldown = new Dictionary<string, DateTime>();

        // Singleton

        public static GiveKit Instance = null;

        protected override void Load()
        {
            Instance = this;
            if (GiveKit.Instance.Configuration.Enabled)
            {
                if (GiveKit.Instance.Configuration.ResetCooldownOnDeath)
                {
                    Rocket.RocketAPI.Events.RocketPlayerEvents.OnPlayerDeath += RocketPlayerEvents_OnPlayerDeath;
                }
                Rocket.RocketAPI.Events.RocketServerEvents.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
            }
        }

        //Events

        private Timer timer;

        private void RocketServerEvents_OnPlayerConnected(RocketPlayer player)
        {
            List<Kit> kits = new List<Kit>();
            if (getGlobalCooldown(player) <= 0)
            {
                foreach (Kit kit in GiveKit.Instance.Configuration.Kits)
                {
                    if (player.Permissions.Contains("givekit.onjoin." + kit.Name.ToLower()) && getKitCooldown(player, kit.Name) <= 0)
                    {
                        kits.Add(kit);
                    }
                }

                if (player.Permissions.Contains("givekit.onjoin.?"))
                {
                    int rnd = UnityEngine.Random.Range(0, GiveKit.Instance.Configuration.Kits.Count - 1);
                    kits.Add(GiveKit.Instance.Configuration.Kits[rnd]);
                }

                if (kits.Count >= 0)
                {
                    timer = new System.Threading.Timer(obj =>
                    {
                        foreach (Kit kit in kits) Give(player, kit.Name, player.CharacterName, true);
                        timer.Dispose();
                    }, null, 1000, Timeout.Infinite);
                }
            }
        }
        private void RocketPlayerEvents_OnPlayerDeath(RocketPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (player.IsAdmin || cause != EDeathCause.SUICIDE)
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

        internal void List(RocketPlayer caller)
        {
            if (!caller.IsAdmin && !caller.Permissions.Contains("givekit") && !caller.Permissions.Contains("givekit.*"))
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions"));
                return;
            }

            List<string> kitLists = new List<string>();
            String kits = "";
            byte v = 0;

            foreach (Kit kit in GiveKit.Instance.Configuration.Kits)
            {
                if (caller.IsAdmin || caller.Permissions.Contains("givekit." + kit.Name.ToLower()) || caller.Permissions.Contains("givekit.*"))
                {
                    if (v == 7)
                    {
                        if (kits != "") kits = kits.Remove(kits.Length - 1);
                        kitLists.Add(kits);
                        kits = "";
                        v = 0;
                    }
                    kits += ' ' + kit.Name + ',';
                    v++;
                }
            }

            if (kits != "") kits = kits.Remove(kits.Length - 1);

            kitLists.Add(kits);

            if (caller.IsAdmin || caller.Permissions.Contains("givekit.share") || caller.Permissions.Contains("givekit.*"))
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_instructions_share"));
            }
            else
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_instructions"));
            }

            foreach (string kitString in kitLists)
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_available_kits", kitString));
            }
            return;
        }

        internal void Give(RocketPlayer caller, string kitId, string player, bool forceGive = false)
        {
            RocketPlayer target = RocketPlayer.FromName(player);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            Kit selectedKit = getKitByString(kitId);
            if (selectedKit == null)
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_kit_not_found", textInfo.ToTitleCase(kitId)));
                return;
            }
            else if (target == null)
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_player_not_found", textInfo.ToTitleCase(player)));
                return;
            }
            else
            {
                double gCD = getGlobalCooldown(caller);

                if (gCD > 0 && !caller.IsAdmin)
                {
                    RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_cooldown_command", gCD));
                    return;
                }

                double kCD = getKitCooldown(caller, kitId);

                if (kCD > 0 && !caller.IsAdmin)
                {
                    RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_cooldown_kit", kCD));
                    return;
                }

                if (caller.CSteamID == target.CSteamID)
                {
                    if (!forceGive && !(caller.IsAdmin || caller.Permissions.Contains("givekit." + kitId.ToLower()) || caller.Permissions.Contains("givekit.*")))
                    {
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions_kit", kitId.ToLower()));
                        return;
                    }
                }
                else
                {
                    if (!(caller.IsAdmin || caller.Permissions.Contains("givekit.share") || caller.Permissions.Contains("givekit.*")))
                    {
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions_share"));
                        return;
                    }
                }

                if (GiveKit.Instance.Configuration.StripBeforeGiving) target.Inventory.Clear();
                foreach (KitItem item in selectedKit.Items) target.GiveItem(item.ItemId, item.Amount);
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_delivered_to_player", textInfo.ToTitleCase(kitId), target.CharacterName));
                if (!caller.Equals(target)) RocketChatManager.Say(target, GiveKit.Instance.Translate("command_givekit_recieved_by_player", textInfo.ToTitleCase(kitId)));
                updateCooldowns(caller, selectedKit);

            }
        }

        // Private Methods

        private Kit getKitByString(string kitId)
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
        private void updateCooldowns(RocketPlayer caller, Kit kit)
        {
            if (GlobalCooldown.ContainsKey(caller.ToString()))
            {
                GlobalCooldown[caller.ToString()] = DateTime.Now;
                IndividualCooldown[caller.ToString() + kit.Name] = DateTime.Now;
            }
            else
            {
                GlobalCooldown.Add(caller.ToString(), DateTime.Now);
                IndividualCooldown.Add(caller.ToString() + kit.Name, DateTime.Now);
            }
        }
        private double getGlobalCooldown(RocketPlayer caller)
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
        private double getKitCooldown(RocketPlayer caller, string kitId)
        {
            Kit kit = getKitByString(kitId);

            if (kit != null && GiveKit.IndividualCooldown.ContainsKey(caller.ToString() + kit.Name))
            {
                double lastRequest = (DateTime.Now - GiveKit.IndividualCooldown[caller.ToString() + kit.Name]).TotalSeconds;

                if (!caller.IsAdmin && lastRequest < kit.Cooldown)
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

        // Translations

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
    }
}