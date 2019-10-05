using BepInEx;
using RoR2;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using paddywan;
using System.IO;
using System;
using RoR2.Networking;
using UnityEngine.Networking;
using R2API.Utils;
using MonoMod.Cil;
using System.Reflection;
using BepInEx.Configuration;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Paddywan."+modname, modname, modver)]
    public class LoonerCoins : BaseUnityPlugin
    {
        public const string modname = "LoonerCoins", modver = "1.0.2";
        private List<PlayerCoinContainer> PCValues;
        private static string PLAYER_COIN_CACHE = BepInEx.Paths.ConfigPath + "\\LoonerCoinCache.json";
        ConfigWrapper<bool> increaseDroprate;
        ConfigWrapper<float> dropChance, dropMulti;

        public void Awake()
        {
            increaseDroprate = Config.Wrap<bool>("LoonerCoins", "increaseDropRate", "If enabled, the configuration values will be utilised. Disable to play with vanilla droprate.", true);
            dropChance = Config.Wrap<float>("LoonerCoins", "dropChance", "The initial value to drop coins. 1% vanilla, 3% recommended.", 3f);
            dropMulti = Config.Wrap<float>("LoonerCoins", "dropMultiplier", "The multiplier for which, after every lunar coin is dropped, modifies the current dropchance. Results in diminishing returns. 0.5f vanilla, 0.75 recommended.", 0.75f);

            On.RoR2.Run.Start += Run_Start;
            On.RoR2.Run.BeginGameOver += Run_BeginGameOver;
            On.RoR2.Run.SetupUserCharacterMaster += Run_SetupUserCharacterMaster;
            On.RoR2.Chat.UserChatMessage.ConstructChatString += UserChatMessage_ConstructChatString;
            On.RoR2.Stage.Start += Stage_Start;
            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;
            BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var initDelegate = typeof(PlayerCharacterMasterController).GetNestedTypes(allFlags)[0].GetMethodCached(name: "<Init>b__56_0");

            if (increaseDroprate.Value)
            {
                MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(initDelegate, (Action<ILContext>)coinDropHook);
            }
        }

        private void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            var num = 0.33f;
            self.baseShopSpawnChance = num;
            orig(self);
            self.baseShopSpawnChance = num;
            Debug.Log($"Shop chance = {num}");
        }

        private void coinDropHook(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdcR4(1f),
                x => x.MatchLdloc(out _),
                x => x.MatchLdfld<PlayerCharacterMasterController>("lunarCoinChanceMultiplier"),
                x => x.MatchMul(),
                x => x.MatchLdcR4(0f)
                );
            //c.Index += 3;
            c.Next.Operand = dropChance.Value;
            c.GotoNext(
                x => x.MatchDup(),
                x => x.MatchLdfld<PlayerCharacterMasterController>("lunarCoinChanceMultiplier"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul()
                );
            c.Index += 2;
            c.Next.Operand = dropMulti.Value;
        }

        private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            orig(self);
            StartCoroutine("chatWarning");
        }

        private string UserChatMessage_ConstructChatString(On.RoR2.Chat.UserChatMessage.orig_ConstructChatString orig, Chat.UserChatMessage self)
        {
            if(!NetworkServer.active)
            {
                return orig(self);
            }
            List<string> chatArgs = new List<string>(self.text.Split(Char.Parse(" ")));
            string command = chatArgs[0];
            if(command.Equals("loonerdisconnect") || command.Equals("ldc"))
            {
                returnCoins(self.sender.GetComponent<NetworkUser>());
                Message.SendToAll("Gave u coins", Colours.Red);
                var para = new object[] { GameNetworkManager.singleton.GetClient(self.sender.GetComponent<NetworkUser>().Network_id.steamId), GameNetworkManager.KickReason.Kick };
                GameNetworkManager.singleton.InvokeMethod("ServerKickClient", para);
            }
            return orig(self);
        }

        private void Run_SetupUserCharacterMaster(On.RoR2.Run.orig_SetupUserCharacterMaster orig, Run self, NetworkUser user)
        {
            orig(self, user);
            StartCoroutine(latePlayerJoin(user));
        }

        private void Run_BeginGameOver(On.RoR2.Run.orig_BeginGameOver orig, Run self, GameResultType gameResultType)
        {
            orig(self, gameResultType);
            Message.SendToAll("All your LoonerCoins have been returned, good luck to ya!", Colours.Red);
            List<PlayerCoinContainer> tempPC = new List<PlayerCoinContainer>();
            foreach (NetworkUser nu in NetworkUser.readOnlyInstancesList)
            {
                foreach (PlayerCoinContainer pc in PCValues)
                {
                    if (nu.Network_id.steamId.value == pc.steamID && nu.isParticipating)
                    {
                        nu.AwardLunarCoins(pc.coins);
                        tempPC.Add(pc);
                        //Debug.Log($"Awarded {pc.coins} coins to {nu.Network_id.steamId.value}");
                    }
                }
            }
            foreach(PlayerCoinContainer temp in tempPC)
            {
                PCValues.Remove(temp);
            }
            File.WriteAllText(PLAYER_COIN_CACHE, string.Empty);
            StreamWriter sr = new StreamWriter(PLAYER_COIN_CACHE);
            foreach (PlayerCoinContainer pc in PCValues)
            {
                sr.WriteLine(coinToJSON(pc));
                //Debug.Log($"Wrote {pc.steamID} to file.");
            }
            sr.Close();
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            PCValues = new List<PlayerCoinContainer>();
            StartCoroutine("setCoins");
        }

        IEnumerator chatWarning()
        {
            yield return new WaitForSeconds(5f);
            if (Run.instance.stageClearCount >= 1)
            {
                Message.SendToAll("Remember, You can type \"loonerdisconnect\", or: \"ldc\" to have your coins returned before you leave.", Colours.LightBlue);
            }
            yield return null;
        }

        IEnumerator latePlayerJoin(NetworkUser user)
        {
            if (Run.instance && RoR2.Run.instance.time >= 5f)
            {
                yield return new WaitForSeconds(5f);
                addNUToCollection(user);
                //Debug.Log($"Player {user.Network_id.steamId.value} Connected with Run_SetupUserCharacterMaster()");
                //Debug.Log($"PccID: {user.Network_id.steamId.value}, PCCoins: {user.lunarCoins}");
                Message.SendToAll("Oh ho! A magical Leprechaun has stollen all your coins! Don't worry, he will return them at the end of the game!", Colours.Red);
            }
            yield return null;
        }

        IEnumerator setCoins()
        {
            yield return new WaitForSeconds(5f);
            populateCollection();
            WriteCollectionToCache();
            foreach(PlayerCoinContainer pcc in PCValues)
            {
                //Debug.Log($"PccID: {pcc.steamID}, PCCoins: {pcc.coins}");
            }
            Message.SendToAll("Oh ho! A magical Leprechaun has stollen all your coins! Don't worry, he will return them at the end of the game!",Colours.Red);
            yield return null;
        }

        private void returnCoins(NetworkUser networkUser)
        {
            PlayerCoinContainer temp = new PlayerCoinContainer();
            foreach(PlayerCoinContainer pc in PCValues)
            {
                if(pc.steamID.Equals(networkUser.Network_id.steamId))
                {
                    temp = pc;
                    networkUser.AwardLunarCoins(temp.coins);
                }
            }
            try
            {
                PCValues.Remove(temp);
            }
            catch (Exception ex)
            {
                Debug.Log("[LoonerCoins] Attempted to remove PlayerCoinContainer that did not exist in cache.");
            }
        }

        private void WriteCollectionToCache()
        {
            try
            {
                foreach (NetworkUser nu in NetworkUser.readOnlyInstancesList)
                {
                    addNUToCollection(nu);
                }
                StreamWriter sw = new StreamWriter(PLAYER_COIN_CACHE, false);
                foreach (PlayerCoinContainer pc in PCValues)
                {
                    sw.WriteLine(coinToJSON(pc));
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private void addNUToCollection(NetworkUser nu)
        {
            PlayerCoinContainer temp = new PlayerCoinContainer(nu.Network_id.steamId.value, nu.NetworknetLunarCoins);
            foreach (PlayerCoinContainer pc in PCValues)
            {
                if (pc.steamID.Equals(nu.Network_id.steamId.value))
                {
                    pc.coins += temp.coins;
                    nu.DeductLunarCoins(nu.NetworknetLunarCoins);
                    return;
                }
            }
            PCValues.Add(temp);
            nu.DeductLunarCoins(nu.NetworknetLunarCoins);
        }

        private void populateCollection()
        {
            try
            {
                StreamReader sr = new StreamReader(PLAYER_COIN_CACHE);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    PCValues.Add(jSONToCoin(line));
                }
                sr.Close();
            }
            catch (FileNotFoundException ex)
            {
                StreamWriter sr = new StreamWriter(PLAYER_COIN_CACHE);
                sr.Close();
            }
        }

        private string coinToJSON(PlayerCoinContainer pc)
        {
            string strJsontest = JsonUtility.ToJson(pc);
            return strJsontest;
        }

        private PlayerCoinContainer jSONToCoin(string line)
        {
            return JsonUtility.FromJson<PlayerCoinContainer>(line);
        }

        /// <summary>
        /// Credit to Fluffatron
        /// </summary>
        public static class Message
        {
            public static void SendToAll(string message, string colourHex)
            {
                Chat.SendBroadcastChat((Chat.ChatMessageBase)new Chat.SimpleChatMessage()
                {
                    baseToken = $"<color={colourHex}>{{0}}: {{1}}</color>",
                    paramTokens = new string[] { MessageFrom, message }
                });
            }
            private static string MessageFrom { get => "LoonerCoins"; }
        }

        public static class Colours
        {
            public static string LightBlue => "#03ffff";
            public static string Red => "#f01d1d";
            public static string Orange => "#ff7912";
            public static string Yellow => "#ffff26";
        }
    }
}
