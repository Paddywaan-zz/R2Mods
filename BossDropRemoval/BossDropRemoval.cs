using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.Paddywan.BossDropRemoval", "BossDropRemoval", "1.0.5")]

    public class BossDropRemoval : BaseUnityPlugin
    {
        List<string> disallowedBossDrops = new List<string>();
        private ConfigWrapper<bool> beetleDisabled;
        private ConfigWrapper<bool> titanDisabled;
        private ConfigWrapper<bool> groveDisabled;
        public void Awake()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };
            beetleDisabled = Config.Wrap("Bosses",
                "BeetleGland",
                "Set to true to disable the BeetleGland",
                false
                );
            if(beetleDisabled.Value) disallowedBossDrops.Add("BeetleQueen2Body(Clone)");
            titanDisabled = Config.Wrap("Bosses",
                "TitanicKnurl",
                "Set to true to disable the Titanic Knurl",
                false
                );
            if(titanDisabled.Value) disallowedBossDrops.Add("TitanBody(Clone)");
            groveDisabled = Config.Wrap("Bosses",
                "LittleDisciple",
                "Set to true to disable the littleDisciple",
                false
                );
            if(groveDisabled.Value) disallowedBossDrops.Add("GraveKeeperBody(Clone)");
            On.RoR2.BossGroup.OnMemberDeathServer += (orig, self, memberMaster, damageReport) =>
            {
                //Debug.Log(damageReport.victimBody.name);
                self.bossDropChance = 0.15f;
                foreach (string s in disallowedBossDrops)
                {
                    if (damageReport.victimBody.name == s)
                    {
                        self.bossDropChance = 0f;
                        //Debug.Log("Bossdrop is removed.");
                    }
                }
                orig(self, memberMaster, damageReport);
            };
        }
    }
}
