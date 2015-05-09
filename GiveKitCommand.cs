using Rocket.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApokPT.RocketPlugins
{
    class GiveKitCommand : IRocketCommand
    {
        public void Execute(RocketPlayer caller, string[] cmd)
        {

            string command = String.Join(" ", cmd);

            if (!GiveKit.Instance.Configuration.Enabled) return;

            // No parameters
            if (String.IsNullOrEmpty(command.Trim()))
            {
                // List caller available kits
                GiveKit.Instance.List(caller);
            }
            else
            {
                string[] oper = command.Split('/');

                
                if (oper[0] == "?")
                {
                    int rnd = UnityEngine.Random.Range(0, GiveKit.Instance.Configuration.Kits.Count - 1);
                    oper[0] = GiveKit.Instance.Configuration.Kits[rnd].Name;
                }

                if (oper.Length == 1)
                {
                    // Give kit to self because of no target parameter
                    GiveKit.Instance.Give(caller, oper[0], caller.CharacterName);
                }
                else
                {
                    // Give kit to targe parameter
                    GiveKit.Instance.Give(caller, oper[0], oper[1]);
                }
            }
           
        }

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
    }
}