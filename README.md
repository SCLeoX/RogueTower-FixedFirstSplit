# Fixed First Split
This is a mod for [Rogue Tower](https://store.steampowered.com/app/1843760/Rogue_Tower/). It will force the first path split to always occur at level 20 (configurable).

When I play Rogue Tower and encounter an early split, I always get frustrated and reset the run. This gets annoying really quickly. However, I literally cannot stop myself from doing it. If you also have no self control like me, this is the mod for you. Once installed, it will prevent splits to occur for first 19 levels. To compensate, you will always get at level 20. No more resets due to early splits. Yay!

## Installation
1. [Install BepInEx](https://rogue-tower.thunderstore.io/package/bbepis/BepInEx_Rogue_Tower/).
2. Run the game and exit to create the required directory structure.
3. Download this mod and extract FixedFirstSplit.dll into `<Rogue Tower installation location>/BepInEx/plugins`.
4. Enjoy

Alternatively, you might be able to install this mod with [Thunderstore Mod Manager](https://thunderstore.io/). I have not tested it. Let me know if it doesn't work.

## Configuration
This mod is configurable. To configure this mod, run the game with at least once to generate the default configuration file. This file is located at `<Rogue Tower installation location>/BepInEx/config/me.tepis.roguetower.fixedfirstsplit.cfg`. You can open this file with any text editor.

The following are the configurable properties of this mod:

```toml
[General]

## The level for first split to occur. Due to game restrictions, this value cannot be lower than 4.
First_Split_Level = 20

## Whether to force a split the level configfured in `First_Split_Level`. If set to false, the level configured in `First_Split_Level` will be the first level that a split may spawn.
Force_First_Split_At_Configured_Level = true
```

## Caveats
Sometimes a split is not possible (e.g. surrounded). In that case, such split will be delayed until the first possible opportunity.

In addition, this mod works by changing the potential tile set. In order to prevent the game from freaking out, when forcing a split, it works about >99.9% of the time. If you configured the first split to always spawn at level 20, and didn't get one at level 20, consider yourself just won the lottery. Though, the mod will try again in level 21. You will have to win another lottery to not get another split at level 21, so on and so forth.
