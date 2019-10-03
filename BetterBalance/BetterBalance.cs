using BepInEx;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using R2API;
using System;
using BepInEx.Configuration;
using UnityEngine;
using MonoMod.RuntimeDetour;
using MonoMod;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]
    //[BepInDependency("com.Squiddle.shapedglassbalance")]
    [BepInPlugin("com.Paddywan.BetterBalance", "BetterBalance", "1.0.4")]
    public class BetterBalance : BaseUnityPlugin
    {
        private float _stickyMultiplier = 5.0f, _stickyMin = 0f, _stickyMax = 10f,
            _bleedDamageMultiplier = 2.0f, _bleedDamageMin = 0f, _bleedDamageMax = 4f,
            _bleedChancePerStack = 5f, _bleedChanceMin = 0f, _bleedChanceMax = 15f,
            _iceRingMultiplier = 2.5f, _iceRingMin = 0f, _iceRingMax = 5f,
            _ukuleleMultiplier = 1.0f, _ukuleleMin = 0f, _ukuleleMax = 1.0f,
            _crowbarScalar = 0.08f, _crowbarScalarMin = 0f, _crowbarScalarMax = 0.16f,
            _crowbarCap = 0.3f, _crowbarCapMin = 0f, _crowbarCapMax = 0.4f,
            _guilotineScalar = 0.10f, _guilotineScalarMin = 0f, _guilotineScalarMax = 0.3f,
            _guilotineCap = 0.45f, _guilotineCapMin = 0f, _guilotineCapMax = 0.6f,
            _APScalar = 0.1f, _APMin = 0f, _APMax = 0.2f,
            _aegisMultiplier = 0.33f, _aegisMax = 0.33f, _aegisMin = 0.01f,
            _brooch = 5f, _broochMin = 1f, _broochMax = 10f,
            _chronoChance = 0.05f, _chronoMin = 0f, _chronoMax = 0.1f
            ;
        private int _predatoryBuffsPerStack = 3, _predatoryMin = 2, _predatoryMax = 4;

        private ConfigWrapper<float> cStickyMultiplier, cBleedMultiplier, cBleedChancePerStack, cIceRingMultiplier, cUkuleleMultiplier, cCrowbarScalar, cCrowbarCap, cGuillotineScalar, cGuillotineCap, cAPDamage, cBrooch, cChronoChance;
        private ConfigWrapper<int> cPredatoryBuffsPerStack;
        private ConfigWrapper<bool> cAPElites, cCrowbarDeminishingThreshold, cGuillotineDeminishingThreshold, cPredatoryEnabled, cCursedOSP, cBleedProcChain, cAegisDecay, cAegisBuff, cChronoFix;
        /*private static ConfigWrapper<float> MyConfig;
        private float myConfig
        {
            get { return GetValue(MyConfig.Value, 10.0f, 20.0f); }
        }
        private float GetValue(float value, float min, float max)
        {
            if(value < min) { return min; }
            if(value > max) { return max; }
            return value;
        }*/

        public void Awake()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };
            cStickyMultiplier = Config.Wrap("Multipliers", "StickybombMultiplier", $"Modifies the damage multiplier of the stickybomb: >{_stickyMin}f, 1.8f vanilla, {_stickyMultiplier}f default, <={_stickyMax}f", _stickyMultiplier);
            cBleedMultiplier = Config.Wrap("Multipliers", "BleedDamageMultiplier", $"Modifies the damage multiplier value of tri-tip: >{_bleedDamageMin}f, 1.0f vanilla, {_bleedDamageMultiplier}f default <={_bleedDamageMax}f", _bleedDamageMultiplier);
            cBleedChancePerStack = Config.Wrap("Multipliers", "BleedChancePerStack", $"Modifies the chance of inflicting bleed with tri-tip: >{_bleedChanceMin}f, 15f vanilla, {_bleedChancePerStack}f default, <={_bleedChanceMax}f", _bleedChancePerStack);
            cBleedProcChain = Config.Wrap("Multipliers", "BleedProcChainEnabled", $"Enables proc-chain triggered bleeds", true);
            cIceRingMultiplier = Config.Wrap("Multipliers", "IceRingMultiplier", $"Modifies the damage multiplier of the Ice Band: >{_iceRingMin}f, 1.25f vanilla, {_iceRingMultiplier}f default, <={_iceRingMax}f", _iceRingMultiplier);
            cUkuleleMultiplier = Config.Wrap("Multipliers", "UkeleleMultiplier", $"Modifies the damage multiplier of the Ukulele: >{_ukuleleMin}f, 0.8f vanilla, {_ukuleleMultiplier}f default, <={_ukuleleMax}f", _ukuleleMultiplier);
            _stickyMultiplier = (cStickyMultiplier.Value > _stickyMin && cStickyMultiplier.Value <= _stickyMax) ? cStickyMultiplier.Value : _stickyMultiplier;
            _bleedDamageMultiplier = (cBleedMultiplier.Value > _bleedDamageMin && cBleedMultiplier.Value <= _bleedDamageMax) ? cBleedMultiplier.Value : _bleedDamageMultiplier;
            _bleedChancePerStack = (cBleedChancePerStack.Value > _bleedChanceMin && cBleedChancePerStack.Value <= _bleedChanceMax) ? cBleedChancePerStack.Value : _bleedChancePerStack;
            _iceRingMultiplier = (cIceRingMultiplier.Value > _iceRingMin && cIceRingMultiplier.Value <= _iceRingMax) ? cIceRingMultiplier.Value : _iceRingMultiplier;
            _ukuleleMultiplier = (cUkuleleMultiplier.Value > _ukuleleMin && cUkuleleMultiplier.Value <= _ukuleleMax) ? cUkuleleMultiplier.Value : _ukuleleMultiplier;

            cCrowbarDeminishingThreshold = Config.Wrap("Crowbar", "CrowbarDeminishingThresholdEnabled", "Enables a variable threshold for Crowbars which scales with deminishing returns, similar to a teddy.", true);
            cCrowbarScalar = Config.Wrap("Crowbar", "CrowbarScalar", $"Modifies the per stack scalar with deminishing returns: >{_crowbarScalarMin}f, {_crowbarScalar}f default, <={_crowbarScalarMax}f", _crowbarScalar);
            cCrowbarCap = Config.Wrap("Crowbar", "CrowbarCap", $"Modifies the cap of the maximum health for which the crowbars effect is applied, as a % of fullHP: >{_crowbarCapMin}f, {_crowbarCap}f default, <={_crowbarCapMax}f", _crowbarCap);
            _crowbarScalar = (cCrowbarScalar.Value > _crowbarScalarMin && cCrowbarScalar.Value <= _crowbarScalarMax) ? cCrowbarScalar.Value : _crowbarScalar;
            _crowbarCap = (cCrowbarCap.Value > _crowbarCapMin && cCrowbarCap.Value <= _crowbarCapMax) ? cCrowbarCap.Value : _crowbarCap;


            cGuillotineDeminishingThreshold = Config.Wrap("Guillotine", "GuillotineThresholdEnabled", "Enables a variable threshold for Guillotine which scales with deminishing returns, similar to a teddy.", true);
            cGuillotineScalar = Config.Wrap("Guillotine", "GuillotineScalar", $"Modifies the per stack scalar with deminishing returns: >{_guilotineScalarMin}f, {_guilotineScalar}f default, <={_guilotineScalarMax}f", _guilotineScalar);
            cGuillotineCap = Config.Wrap("Guillotine", "GuillotineCap", $"Modifies the cap of the minimum health for which the crowbars effect is applied, as a % of fullHP: >{_guilotineCapMin}f, {_guilotineCap}f default, <={_guilotineCapMax}f", _guilotineCap);
            _guilotineScalar = (cGuillotineScalar.Value > _guilotineScalarMin && cGuillotineScalar.Value <= _guilotineScalarMax) ? cGuillotineScalar.Value : _guilotineScalar;
            _guilotineCap = (cGuillotineCap.Value > _guilotineCapMin && cGuillotineCap.Value <= _guilotineCapMax) ? cGuillotineCap.Value : _guilotineCap;

            cPredatoryEnabled = Config.Wrap("Predatory", "PredatoryEnabled", "Enables linear predatory scaling: 3,6,9...", true);
            cPredatoryBuffsPerStack = Config.Wrap("Predatory", "PredatoryBuffsPerStack", $"Alters the scaling of predatory isntincts to scale linearly instead of 3+2xStacks: >{_predatoryMin}i, {_predatoryBuffsPerStack}i default, <={_predatoryMax}", _predatoryBuffsPerStack);
            _predatoryBuffsPerStack = (cPredatoryBuffsPerStack.Value > _predatoryMin && cPredatoryBuffsPerStack.Value <= _predatoryMax) ? cPredatoryBuffsPerStack.Value : _predatoryBuffsPerStack;

            cAPElites = Config.Wrap("APRounds", "APElitesEnabled", "Alters the AP rounds to be inclusive of elite mobs", true);
            cAPDamage = Config.Wrap("APRounds", "APDamageScalar", $"Alters the AP damage scalar to be lower than default due to increased effectiveness: >{_APMin}f, {_APScalar}f default, <={_APMax}f", _APScalar);
            _APScalar = (cAPDamage.Value > _APMin && cAPDamage.Value <= _APMax) ? cAPDamage.Value : _APScalar;

            cCursedOSP = Config.Wrap("OSP", "CursedOSPDisabled", "Disables One Shot Protection for Cursed characters(read as shaped glass, lunar potion curse)", true);

            cAegisDecay = Config.Wrap("Barrier", "DisableAegisDecay", "Barrier given by Aegis does not decay", true);
            cAegisBuff = Config.Wrap("Barrier", "BuffAegis", "Aegis provides 33% instead of 20% of FullCombinedHealth as barrier", true);
            cBrooch = Config.Wrap("Barrier","BroochBarrierVal", $"The value of barrier restored per kill: >{_broochMin}f, {_brooch}f default, <={_broochMax}f", _brooch);
            _brooch = (cBrooch.Value > _broochMin && cBrooch.Value <= _broochMax) ? cBrooch.Value : _brooch;

            cChronoFix = Config.Wrap("ChronoBauble", "ChronoReworkEnabled", "Reworks the chronobauble to apply slow stacks to enemies onHit", true);
            cChronoChance = Config.Wrap("ChronoBauble", "ChronoProcChance", "The chance per hit per item to apply a single slow stack", _chronoChance);
            _chronoChance = (cChronoChance.Value > _chronoMin && cChronoChance.Value <= _chronoMax) ? cChronoChance.Value : _chronoChance;

            Delegate asd;
            new Hook(RoR2.GlobalEventManager.OnHitEnemy., asd);

            IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
            {
                var c = new ILCursor(il);
                c.Index = 0;

                //Bleed procchain
                if (cBleedProcChain.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdarg(1),
                        x => x.MatchLdflda<DamageInfo>("procChainMask"),
                        x => x.MatchLdcI4(5),
                        x => x.MatchCall<ProcChainMask>("HasProc")
                        );
                    c.Next.OpCode = OpCodes.Nop;
                    c.Index++;
                    c.RemoveRange(3);
                    c.Emit(OpCodes.Ldc_I4_0);
                }

                //BleedChance
                c.GotoNext(
                    x => x.MatchLdcR4(15f),
                    x => x.MatchLdloc(out _),
                    x => x.MatchConvR4()
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _bleedChancePerStack);

                //BleedDmg
                c.GotoNext(
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>("procCoefficient"),
                    x => x.MatchMul(),
                    x => x.MatchLdcR4(1f)
                    );
                //BleedProcChain
                if (cBleedProcChain.Value)
                {
                    c.RemoveRange(2);
                    c.Emit(OpCodes.Ldloc_0);
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Func<CharacterBody, DamageInfo, float>>((attacker, damageInfo) => {
                        if (attacker.teamComponent.teamIndex == TeamIndex.Player) return 1f;
                        return damageInfo.procCoefficient;
                    });
                    c.Index += 1;
                    c.Remove();
                }
                else
                {
                    c.Index += 3;
                    c.Remove();
                }
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<CharacterBody, DamageInfo, float>>((attacker, damageInfo) =>
                {
                    if (attacker.teamComponent.teamIndex == TeamIndex.Player) return _bleedDamageMultiplier;
                    return 1f;
                });
                //c.Emit(OpCodes.Ldc_R4, _bleedDamageMultiplier);

                //Chrono
                if (cChronoFix.Value)
                {
                    int arg1, icount;
                    c.GotoNext(
                        x => x.MatchLdloc(out arg1),
                        x => x.MatchLdcI4(26),
                        x => x.MatchLdcR4(2f),
                        x => x.MatchLdloc(out icount),
                        x => x.MatchConvR4(),
                        x => x.MatchMul(),
                        x => x.MatchCallvirt<CharacterBody>("AddTimedBuff")
                        );
                    c.RemoveRange(7);
                }

                //Ukulele
                c.GotoNext(
                    x => x.MatchLdcR4(0.8f),
                    x => x.MatchStloc(out _)
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _ukuleleMultiplier);

                //StickyDmg
                c.GotoNext(
                    x => x.MatchLdcR4(1.8f),
                    x => x.MatchStloc(out _)// (OpCodes.Stloc_S, (byte)37)
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _stickyMultiplier);

                //IceBand
                c.GotoNext(
                    x => x.MatchLdcR4(1.25f),
                    x => x.MatchLdcR4(1.25f),
                    x => x.MatchLdloc(out _)
                    );
                c.RemoveRange(2);
                c.Emit(OpCodes.Ldc_R4, _iceRingMultiplier);
                c.Emit(OpCodes.Ldc_R4, 2.5f);
            };

            //Brooch
            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) => {
                ILCursor c = new ILCursor(il);
                int val, val2;
                c.GotoNext(
                    x => x.MatchLdloc(out val),
                    x => x.MatchCallvirt<CharacterBody>("get_healthComponent"),
                    x => x.MatchLdcR4(out _),
                    x => x.MatchLdloc(out val2),
                    x => x.MatchConvR4()
                    );
                c.Index += 2;
                c.Next.Operand = _brooch;
            };

            //Predatory Stacks
            IL.RoR2.CharacterBody.AddTimedBuff += (il) =>
            {
                var c = new ILCursor(il);
                if (cPredatoryEnabled.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdcI4(1),
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdcI4(2)
                        );
                    c.Index += 1;
                    c.Remove();
                    c.Index += 1;
                    c.Remove();
                    c.Emit(OpCodes.Ldc_I4, _predatoryBuffsPerStack);
                    c.Index += 1;
                    c.Remove();
                }
            };

            //cBar, AP, OSP
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
                #region Crowbar
                if (cCrowbarDeminishingThreshold.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchCallvirt<HealthComponent>("get_combinedHealth"),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallvirt<HealthComponent>("get_fullCombinedHealth"),
                        x => x.MatchLdcR4(0.9f)
                        );
                    c.Index += 4;
                    c.Remove();
                    c.Emit(OpCodes.Ldloc_1);
                    c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                    {
                        if (cb.master.inventory)
                        {
                            int bars = cb.master.inventory.GetItemCount(ItemIndex.Crowbar);
                            if (bars > 0)
                            {
                                return 1f - ((1f - 1f / (_crowbarScalar * (float)bars + 1f)) * _crowbarCap);
                            }
                        }
                        return 0.9f;
                    });
                }
                #endregion

                #region AP
                if (cAPElites.Value)
                {
                    //Debug.Log(il);
                    ILLabel lab1 = il.DefineLabel();
                    ILLabel lab2 = il.DefineLabel();
                    c.GotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>("body"),
                        x => x.MatchCallvirt<CharacterBody>("get_isBoss")
                        );

                    c.Index += 3;
                    c.Remove();
                    c.Emit(OpCodes.Brtrue_S, lab1);
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldfld, typeof(HealthComponent).GetFieldCached("body"));
                    c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethodCached("get_isElite"));
                    c.Emit(OpCodes.Brfalse_S, lab2);

                    //c.Index = 1;
                    c.MarkLabel(lab1);

                    //AP Multiplier
                    c.GotoNext(
                        x => x.MatchLdloc(4),
                        x => x.MatchLdcR4(1.0f),
                        x => x.MatchLdcR4(0.2f)
                        );
                    //Debug.Log(c);
                    c.Index += 2;
                    c.Remove();
                    c.Emit(OpCodes.Ldc_R4, _APScalar);

                    //Return label
                    c.GotoNext(
                        x => x.MatchLdarg(1),
                        x => x.MatchLdfld<DamageInfo>("crit")
                        );
                    c.MarkLabel(lab2);

                    //Debug.Log(il);
                }
                #endregion

                #region OSP
                if (cCursedOSP.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdloc(4),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallvirt<HealthComponent>("get_fullCombinedHealth"),
                        x => x.MatchLdcR4(0.9f),
                        x => x.MatchMul()
                        );
                    c.Index += 5;
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldfld, typeof(HealthComponent).GetFieldCached("body"));
                    c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethodCached("get_cursePenalty"));
                    c.Emit(OpCodes.Mul);
                }
                #endregion

                #region Guilotine
                if (cGuillotineDeminishingThreshold.Value)
                {
                    c.GotoNext(
                    x => x.MatchLdloc(1),
                    x => x.MatchCallvirt<CharacterBody>("get_executeEliteHealthFraction"),
                    x => x.MatchStloc(29)
                    );
                    c.Index++;
                    c.Remove();
                    c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                    {
                        if (cb.inventory && cb.inventory.GetItemCount(ItemIndex.ExecuteLowHealthElite) > 0)
                        {
                            return ((1f - 1f / (_guilotineScalar * (float)cb.inventory.GetItemCount(ItemIndex.ExecuteLowHealthElite) + 1f)) * _guilotineCap);
                        }
                        return cb.executeEliteHealthFraction;
                    });
                }
                //Debug.Log(il);
                #endregion
            };

            //Aegis
            IL.RoR2.HealthComponent.Heal += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(MoveType.After,
                    x => x.MatchLdfld<HealthComponent>("barrierOnOverHealCount"),
                    x => x.MatchConvR4(),
                    x => x.MatchLdcR4(out _),
                    x => x.MatchMul()
                    );
                if (cAegisBuff.Value) c.Prev.Operand = _aegisMultiplier;
            };

            //AegisDecay & multiplier
            IL.RoR2.HealthComponent.ServerFixedUpdate += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<HealthComponent>("barrier"),
                    x => x.MatchLdcR4(0)
                    );
                c.Index += 2;
                //Debug.Log(c);
                c.Remove();
                //Debug.Log(c);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<HealthComponent, float>>((hc) =>
                {
                    if (hc.body && hc.body.inventory && cAegisDecay.Value) return hc.fullCombinedHealth * (hc.body.inventory.GetItemCount(ItemIndex.BarrierOnOverHeal) * (cAegisBuff.Value ? _aegisMultiplier : 0.2f));
                    return 0f;
                });
                //Debug.Log(c);
            };

            if(cChronoFix.Value) On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        //ChronoRework
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            var attacker = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            var cbVictim = victim ? victim.GetComponent<CharacterBody>() : null;
            var chronoCount = (attacker && attacker.inventory) ? attacker.inventory.GetItemCount(ItemIndex.SlowOnHit) : 0;
            if (attacker && victim && !cbVictim.isBoss && chronoCount > 0 && Util.CheckRoll((1f - 1f / (damageInfo.procCoefficient * _chronoChance * (float)chronoCount + 1f)) * 100f, attacker.master))
            {
                cbVictim.AddTimedBuff(BuffIndex.BeetleJuice, 2f);
            }
        }
    }

    class MinMaxConfig<T> where T : IComparable
    {
        public T GetValue(T value, T min, T max)
        {
            if (value.CompareTo(min) < 0)
            {
                return min;
            }
            if (value.CompareTo(max) > 0)
            {
                return max;
            }
            return value;
        }
    }
}
