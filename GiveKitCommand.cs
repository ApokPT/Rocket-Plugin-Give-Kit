using Rocket;
using Rocket.Logging;
using Rocket.RocketAPI;
using SDG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace pt.manuelbarbosa.rocketplugin
{
    class GiveKitCommand : IRocketCommand
    {
        public bool RunFromConsole
        {
            get { return false; }
        }

        public string Name
        {
            get { return "givekit"; }
        }

        public string Help
        {
            get { return "Give a kit setup to a player."; }
        }

        public void Execute(RocketPlayer caller, string command)
        {
            var callParams = command.Split('/');


            bool hasListPermission = caller.Permissions.Contains("givekit") || caller.Permissions.Contains("givekit.*");
            bool hasSharePermission = caller.Permissions.Contains("givekit.share") || caller.Permissions.Contains("givekit.*");

            double gCD = GiveKit.getGlobalCooldown(caller);
            if (gCD > 0)
            {
                RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_cooldown_command", gCD));
                return;
            }

            if (String.IsNullOrEmpty(command.Trim()))
            {

                if (hasListPermission)
                {
                    
                    List<string> kitLists = new List<string>();
                    String kits = "";
                    byte v = 0;

                    foreach (Kit kit in GiveKit.Instance.Configuration.Kits)
                    {
                        if (caller.Permissions.Contains("givekit." + kit.Name.ToLower()) || caller.Permissions.Contains("givekit.*"))
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

                    if (caller.Permissions.Contains("givekit.share") || caller.Permissions.Contains("givekit.*"))
                    {
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_instructions_share"));
                    }else{
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_instructions"));
                    }

                    foreach (string kitString in kitLists){
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_available_kits", kitString));
                    }
                    
                    return;
                }
                else
                {
                    RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions"));
                    return;
                }
            }
            else
            {

                double kCD = GiveKit.getKitCooldown(caller, callParams[0]);

                if (kCD > 0)
                {
                    RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_cooldown_kit", kCD));
                    return;
                }

                bool hasKitPermission = caller.Permissions.Contains("givekit." + callParams[0].ToLower()) || caller.Permissions.Contains("givekit.*");

                if (callParams.Length == 1)
                {
                    if (hasKitPermission)
                    {
                        GiveKit.Give(caller, callParams[0] as String, caller.CharacterName);
                    }
                    else
                    {
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions_kit", callParams[0].ToLower()));
                        return;
                    }
                }
                else
                {
                    if (hasKitPermission)
                    {


                        if (hasSharePermission)
                        {
                            GiveKit.Give(caller, callParams[0] as String, callParams[1] as String);
                        }
                        else
                        {
                            RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions_share"));
                            return;
                        }

                    }
                    else
                    {
                        RocketChatManager.Say(caller, GiveKit.Instance.Translate("command_givekit_no_permissions_kit", callParams[0].ToLower()));
                        return;
                    }
                }
            }


        }
    }
}