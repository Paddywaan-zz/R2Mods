#define DEBUG
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Paddywan
{
    /// <summary>
    /// Modifies cardWeight of specific interactables to balance gameplay when using TMF.
    /// </summary>
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Paddywan.TMFBalancedInteractables", "TMFBalancedInteractables", "1.0.2")]
    public class AdjustCardWeight : BaseUnityPlugin
    {
        private static ConfigWrapper<double> confScalar;
        public void Awake()
        {            
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            confScalar = Config.Wrap("Scalars",
                "printerScalar",
                "1.0 = Reduces spawn-rate of printers for each extra player over 4. 16 players results in 25% of the normal spawns, 8 players results in 50% of the normal. Using a scalar value of 4.0 with 16 players results in no change to the spawnrate.",
                1.0
                );

            /// Credits to Fluffatron for getting an effective hook working.
            On.RoR2.DirectorCardCategorySelection.SumAllWeightsInCategory += (orig, self, category) =>
            {
                var numberOfPlayers = RoR2.Run.instance.participatingPlayerCount;
                if (numberOfPlayers > 4) //only make changes if > 4 players
                {
                    AddInfoLog($"Making changes based on {numberOfPlayers} players");
                    var cardsToBeAdjusted = new List<string>
                    {
                        CardNames.Duplicator,
                        CardNames.DuplicatorLarge,
                        CardNames.DuplicatorMilitary,
                        CardNames.LunarChest
                    };
                    foreach (var card in cardsToBeAdjusted)
                    {
                        AddDebugLog($"Player count is > 4. Adjusting value for {card}");
                        AdjustSelectionWeight(category, card, numberOfPlayers);
                    }
                }
                else
                {
                    AddDebugLog("No changes made to weighting.");
                }
                float num = 0.0f;
                for (int index = 0; index < category.cards.Length; ++index)
                {
                    var name = category.cards[index].spawnCard.name;
                    int weight = category.cards[index].selectionWeight;
                    num += (float)category.cards[index].selectionWeight;
                }
                return num;
            };
        }

        /// <summary>
        /// Privdes a console command to change the printerScalar ingame. Effect from next maps.
        /// </summary>
        /// <param name="args">args[0] double</param>
        [ConCommand(commandName = "printerScalar", flags = ConVarFlags.ExecuteOnServer, helpText = "args[0] double")]
        private static void CCPrinterScalar(ConCommandArgs args)
        {
            string val = getArg(args.userArgs, 0);
            double dVal = confScalar.Value;
            if (val.Length == 0) Debug.Log($"printerScalar = {confScalar.Value}");
            else
            { 
                try
                {
                    double.TryParse(val, out dVal);
                }
                finally
                {
                    confScalar.Value = dVal;
                    Debug.Log($"printerScalar = {confScalar.Value}");
                }
            }
        }
        private void AdjustSelectionWeight(DirectorCardCategorySelection.Category category, string cardName, int numberOfPlayers)
        {
            foreach(var card in category.cards)
            {
                if(card.spawnCard.name == cardName)
                {
                    //Scale down the spawnrate as the player# increases. 4 = default, 8 = 50%, 16=25%.
                    var adjustmentConstant = (double)4 / (double)numberOfPlayers;
                    //Then scale again based on config scalar. Config scalar will result in different "true" spawnrates depending on # of players. 2.0 value with 8 players results in 100%; but only 50% with 16.
                    var adjustedWeight = (int)((double)card.selectionWeight * confScalar.Value * (double)adjustmentConstant);

                    if (adjustedWeight < 1)
                    {
                        AddInfoLog($"Set {cardName} = 1 (default) [down from {card.selectionWeight}]");
                        card.selectionWeight = 1;
                    }
                    else
                    {
                        AddInfoLog($"Set {cardName} = {adjustedWeight} [down from {card.selectionWeight}]");
                        card.selectionWeight = adjustedWeight;
                    }
                }
            }            
        }
        private void AddDebugLog(string message)
        {
#if DEBUG
            Logger.Log(BepInEx.Logging.LogLevel.Debug, message);
#endif
        }
        private void AddInfoLog(string message)
        {
#if DEBUG
            Logger.Log(BepInEx.Logging.LogLevel.Info, message);
#endif
        }
        public static string getArg(List<string> args, int index)
        {
            if (index < args.Count && index >= 0)
            {
                return args[index];
            }

            return "";
        }
    }
}