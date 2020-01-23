# Modding rules
Payload Studios has reached out to the TerraTech modding community as a result of its rapid growth. THey want to set a number of restrictions to ensure that the modding community won't upset the development of the game.

## Rules for modders
1. Do not leak the game freely to the public.
2. Do not reveal secrets or hidden information, such as cheat codes, unreleased blocks, corporations or gamemodes.
3. Make your mod flag output and saves that the game has been modded.
4. Do not break the age-rating set by Payload Studios (currently ~8 years old audience).
5. Do not upset the development of Terra Tech in any significant way.
6. Do not unlock DLC (skins, R&D etc) or provide access to DLC content to players who don't own it. 
7. Any mods using DLC assets (such as adding a block using RR textures) should be locked by the same DLC.
8. Do not use mods in public multiplayer lobbies.

## Flagging modded games
The development team doesn't want to spend time on bugs reported using modded games, as they may not actually exist in the vanilla version of the game. To this extend modders are asked to flag the following:

1. `output_log.txt` with the message "This game is modded"
2. In `UIScreenBugReport` modify `WWWForm` in `PostIt();` and add a field `mods` set to any non-empty string value.
3. In `UIScreenBugReport` prefix the usermessage inserted into the `body` field of the `WWWForm` instance with the message "This game is modded\n\n"
4. Flag saves by setting `m_OverlayData` to "This game is modded"
