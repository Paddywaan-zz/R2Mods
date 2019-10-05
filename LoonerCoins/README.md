# LoonerCoins

*NEW* - Updated to Skills 2.0

LoonerCoins aims to implement a more roguelike, temporary LunarCoin system. Your coin's will be "stollen" from you at the beginning of a game, and returned to you at the end. I have attempted to ensure that all unexpected disconnects, crashes, and so forth do not result in lost coins.

LoonerCoins will steal your coins when you start a new run, storing all players in the session to a cache. Any player who joins later will also be stored within the cache. At the end of game report, any players currently within the game that exist within the cache will have their coins returned to them. If the host crashes, all players coin values will remain within the cache. Rejoining the host & reaching the end of run report scene will result in coins being given back. If a client crashes, they can simply rejoin the game and their coins will be waiting for them at the end of run report. If a player wants to disconnect gracefully, they can use the command "ldc" or "loonerdisconnect" to leave the game and take their coins with them. Reminders of the command are given at the beginning of each stage.

## Installation & Config

Place inside of /Bepinex/Plugins

Run the game & exit to generate the configuration file.

> increaseDropRate - If enabled, the configuration values will be utilised. Disable to play with vanilla droprate.

> dropChance - The initial value to drop coins. 1% vanilla, 3% recommended.", 3f);

> dropMultiplier - The multiplier for which, after every lunar coin is dropped, modifies the current dropchance. Results in diminishing returns. 0.5f vanilla, 0.75 recommended.

## Changelog

v1.0.0 - released

## Credits

Icon artwork thanks to my sister. You can find other examples of her work and contact her [here](https://evelyngardner.carbonmade.com) for any requests you might have.