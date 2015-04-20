# Give Kit
### Give a Kit to another player or yourself

This is the same as Kits but with a different approach:

Every time a kit is delivered, both the receiver and the giver are informed.
Uses a configuration similar to kits, so you can copy your Kits to the GiveKit config file.


## Available Commands
Command | Action
------- | -------
/givekit <kit>/<player>					| gives kit to player
/givekit <kit>						| gives kit to caller
/givekit						| show available kits


## Available Permissions
Permission | Action
------- | -------
<Command>givekit</Command>				| allow caller to list his available kits
<Command>givekit.share</Command>			| allow caller to give kits to another player
<Command>givekit.[kit_name]</Command>			| allow caller to use [kit_name]
<Command>givekit.*</Command>				| allow caller to list, use and share any avaiable kit
<Command>givekit.onjoin.[kit_name]</Command>		| will give [kit_name] to the player when he joins the server
_(if you add multiple givekit.onlin permissions, it will give them all to that player)_


## Other Options
Option | Action
------- | -------
StripBeforeGiving					| Strips the player of all items before giving the new kit 
ResetCooldownOnDeath				| Resets global cooldown and all kits cooldown on players death
Enabled								| Enables and disables the addon (does not apply to admins)
_(still some issues with the strip feature because of some problems with Rocket 3.3.0 Beta)_


## Todo List:
* ~~Implement per-kit cooldown and global cooldown;~~
* ~~Option to strip player before giving a new kit;~~
* Command /givekit random
* ~~On join server givekit (still thinking how will this be)~~
