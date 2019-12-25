[![Get help on Koikatu discord server](https://img.shields.io/badge/help-discord-brightgreen.svg)](https://discord.gg/urDt8CK)
[![Latest release](https://img.shields.io/github/release/ManlyMarco/KKAPI.svg?style=flat)](https://github.com/ManlyMarco/KKAPI/releases)
[![Downloads](https://img.shields.io/github/downloads/ManlyMarco/KKAPI/total.svg?style=flat)](https://github.com/ManlyMarco/KKAPI/releases)
[![Issues](https://img.shields.io/github/issues/ManlyMarco/KKAPI.svg?style=flat)](https://github.com/ManlyMarco/KKAPI/issues)
[![License](https://img.shields.io/github/license/ManlyMarco/KKAPI.svg?style=flat)](https://github.com/ManlyMarco/KKAPI/blob/master/LICENSE)
## Modding API for Illusion games
This is an API designed to make writing plugins for recent UnityEngine games made by the company Illusion easier and less bug-prone. It abstracts away a lot of the complexity of hooking the game save/load logic, creating interface elements at runtime, and many other tasks. All this while supplying many useful methods and tools. Supported games:
- Koikatu/Koikatsu Party (KKAPI)
- Emotion Creators (ECAPI)
- AI-Shoujo/AI-Syoujyo (AIAPI)

## Some mods that use the Modding API
The API is required by many plugins made for Koikatsu, Emotion Creators and AI-Girl/AI-Shoujo. Here are some of them:
* [Illusion Overlay Mods](https://github.com/ManlyMarco/Illusion-Overlay-Mods) - Uses many different features like saving to coordinates, partial load toggles, advances maker interface elements, using Windows Open File dialogs, etc.
* [KK_BecomeTrap](https://github.com/ManlyMarco/KK_BecomeTrap) - Simple mod that give a good example of how clean and easy KKAPI makes things.
* [KK_SkinEffects](https://github.com/ManlyMarco/KK_SkinEffects) - Uses StudioAPI to create custom controls in the studio interface.
* [ABMX](https://github.com/ManlyMarco/ABMX) - Uses a lot of runtime-generated maker UI elements, fairly complex.
* [UncensorSelector, KK_EyeShaking, KK_RandomCharacterGenerator, more...](https://github.com/DeathWeasel1337/KK_Plugins)
* [AI PushUp](https://bitbucket.org/mikkemikke/ai-pushup-bra/src/master/)
* and many more...

## How to install
1. Download [the latest release](https://github.com/IllusionMods/IllusionModdingAPI/releases) for your game. **Warning:** You only need the version specific for your game (check the prefix, for example KK = for Koikatsu). Downloading version for the wrong game or multiple versions will break things!
2. Install latest version of BepInEx 5.0 and BepisPlugins. If MoreAccessories is used, make sure it is up to date as well.
3. Extract the archive into your game directory. The .dll file should end up inside BepInEx\plugins.

## Developers
- Basic construction of the API is [explained in the introduction](https://github.com/ManlyMarco/KKAPI/wiki/Introduction).
- A short tutorial on how the API can be used to make a plugin [can be found here](https://github.com/ManlyMarco/KKAPI/wiki/Typical-usage-example-and-explanation).
- [Code reference can be found here.](https://github.com/ManlyMarco/KKAPI/blob/master/doc/Home.md)
- You can get help with using this API on the [Koikatsu! discord server](https://discord.gg/urDt8CK).

## Previews
To get a good preview you can download one of the mods listed above, for example [Koikatu Overlay Mods](https://github.com/ManlyMarco/Koikatu-Overlay-Mods).

![kkapi preview 1](https://user-images.githubusercontent.com/39247311/52817863-74461d80-30a5-11e9-81fd-d68a530d066a.png)
![kkapi preview 2](https://user-images.githubusercontent.com/39247311/52817865-74461d80-30a5-11e9-9b4f-e42ef0dcc7ea.png)
