[![Get help on Koikatu discord server](https://img.shields.io/badge/help-discord-brightgreen.svg)](https://discord.gg/urDt8CK)
## Modding API for Illusion games
This is an API designed to make writing plugins for recent UnityEngine games made by the company Illusion easier and less bug-prone. It abstracts away a lot of the complexity of hooking the game save/load logic, creating interface elements at runtime, and many other tasks. All this while supplying many useful methods and tools. Supported games:
- PlayHome (PHAPI)
- Koikatu/Koikatsu Party (KKAPI)
- Emotion Creators (ECAPI)
- AI-Shoujo/AI-Syoujyo (AIAPI)
- HoneySelect2 (HS2API)
- Koikatsu Sunshine (KKSAPI)

## Some mods that use the Modding API
The API is required by many plugins made for Koikatsu, Emotion Creators and AI-Girl/AI-Shoujo. Here are some of them:
* [Illusion Overlay Mods](https://github.com/ManlyMarco/Illusion-Overlay-Mods) - Uses many different features like saving to coordinates, partial load toggles, advances maker interface elements, using Windows Open File dialogs, etc.
* [KK_BecomeTrap](https://github.com/ManlyMarco/KK_BecomeTrap) - Simple mod that give a good example of how clean and easy KKAPI makes things.
* [KK_SkinEffects](https://github.com/ManlyMarco/KK_SkinEffects) - Uses StudioAPI to create custom controls in the studio interface.
* [ABMX](https://github.com/ManlyMarco/ABMX) - Uses a lot of runtime-generated maker UI elements, fairly complex.
* [UncensorSelector, KK_EyeShaking, KK_RandomCharacterGenerator, more...](https://gitgoon.dev/IllusionMods/KK_Plugins)
* [AI PushUp](https://bitbucket.org/mikkemikke/ai-pushup-bra/src/master/)
* and many more...

## How to install
1. Install the latest version of [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases/latest) and [BepisPlugins](https://gitgoon.dev/IllusionMods/BepisPlugins/releases/latest) . If MoreAccessories is used, make sure it is up to date as well.
2. Download [the latest release](https://gitgoon.dev/IllusionMods/IllusionModdingAPI/releases) for your game. **Warning:** You only need the version specific for your game (check the prefix, for example KK = for Koikatsu). Downloading version for the wrong game or multiple versions will break things!
3. Extract the archive into your game directory. The .dll file should end up inside BepInEx\plugins.

If you'd like to test not-yet-released features you can download the latest nightly builds from the [CI workflow](https://github.com/IllusionMods/IllusionModdingAPI/actions/workflows/ci.yaml).

## Developers
- Basic construction of the API is [explained in the introduction](https://gitgoon.dev/IllusionMods/IllusionModdingAPI/wiki/Introduction).
- A short tutorial on how the API can be used to make a plugin [can be found here](https://gitgoon.dev/IllusionMods/IllusionModdingAPI/wiki/Typical-usage-example-and-explanation).
- [Code reference can be found here.](https://gitgoon.dev/IllusionMods/IllusionModdingAPI/blob/master/doc/Home.md)
- You can get help with using this API on the [Koikatsu! discord server](https://discord.gg/urDt8CK).
