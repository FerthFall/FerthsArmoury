using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using static FerthsArmory.Main;

namespace FerthsArmory.Items
{
    public class AbsolutionBase : ItemBase<AbsolutionBase>
    {
        public ConfigEntry<float> SoulOnKillBaseChance;
        public ConfigEntry<float> SoulOnKillStackingChance;
        public ConfigEntry<float> SoulOnBossKillBaseAmount;
        public ConfigEntry<float> SoulOnBossKillStackingAmount;
        public ConfigEntry<float> MaxCritHealBaseAmount;
        public ConfigEntry<float> MaxCritHealStackingAmount;

        public override string ItemName => "Absolution";

        public override string ItemLangTokenName => "ABSOLUTION_ITEM";

        public override string ItemPickupDesc => "Death is drawn to life.";

        public override string ItemFullDescription => $"Whenever you <style=cIsDamage>kill an enemy</style>, you have a <style=cIsUtility>{SoulOnKillBaseChance.Value.ToString()}%</style><style=cStack>(+{SoulOnKillStackingChance.Value.ToString()}% per stack)</style> to gain an Absolution Stack. Gain <style=cIsUtility>{SoulOnBossKillBaseAmount.Value.ToString()}</style><style=cStack>(+{SoulOnBossKillStackingAmount.Value.ToString()} per stack)</style> Absolution Stacks on Elite or Boss kill. <style=cIsHealing>Heal</style> for <style=cIsUtility>{MaxCritHealBaseAmount.Value.ToString()}</style><style=cStack>(+{MaxCritHealStackingAmount.Value.ToString()} per stack)</style> health per hit when exceeding 100% crit chance.";

        public override string ItemLore => "Senna Passive Copy";

        public override ItemTier Tier => ItemTier.Tier3;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AbsolutionBaseDisplay.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AbsolutionBaseIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override void CreateConfig(ConfigFile config)
        {
            SoulOnKillBaseChance = config.Bind<float>("Item: " + ItemName, "Percent chance to gain a stack on regular mob kill", 5, "How often should a regular mob kill grant a stack.");
            SoulOnKillStackingChance = config.Bind<float>("Item: " + ItemName, "Percent chance to gain a stack on regular mob kill with additional Absolutions", 1, "How often should a regular mob kill grant a stack per additional Absolution.");
            SoulOnBossKillBaseAmount = config.Bind<float>("Item: " + ItemName, "Base amount of stacks granted on elite or greater kill", 2, "How many stacks should an elite or greater mob grant.");
            SoulOnBossKillStackingAmount = config.Bind<float>("Item: " + ItemName, "Amount of stacks granted on elite or greater kill with additional Absolutions", 1, "How many stacks should an eliete or greater mob grant per additional Absoluton.");
            MaxCritHealBaseAmount = config.Bind<float>("Item: " + ItemName, "Amount of health healed per hit with 100% crit chance or greater", 3, "How much health should be restored per hit when the player exceeds 100% crit chance.");
            MaxCritHealStackingAmount = config.Bind<float>("Item: " + ItemName, "Amount of health healed per hit with 100% crit chance or greater with additional Absolutions", 3, "How much health should be restored per hit when the player exceeds 100% crit chance per additional Absolution.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Hooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageGlobal;            
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
            {
                return;
            }

            CharacterBody attacker = report.attackerBody;

            //We need an inventory to do check for our item
            if (attacker.inventory)
            {
                //store the amount of our item we have
                int absoCount = GetCount(attacker); //attacker.inventory.GetItemCount(ItemDef.itemIndex);
                if (absoCount <= 0)
                {
                    return;
                }

                if (report.victimIsBoss || report.victimIsChampion || report.victimIsElite)
                { 
                    for(int i = 0; i < SoulOnBossKillBaseAmount.Value; i++)
                    {
                        attacker.inventory.GiveItem(AbsolutionStacks.instance.ItemDef.itemIndex);
                    }

                    for(int i = 0; i < absoCount - 1; i++)
                    {
                        for(int x = 0; x < SoulOnBossKillStackingAmount.Value; x++)
                        {
                            attacker.inventory.GiveItem(AbsolutionStacks.instance.ItemDef.itemIndex);
                        }
                    }
                    
                    return;
                }

                if (Util.CheckRoll(SoulOnKillBaseChance.Value + ((absoCount - 1) * SoulOnKillStackingChance.Value), attacker.master))
                {
                    attacker.inventory.GiveItem(AbsolutionStacks.instance.ItemDef.itemIndex);
                }
            }
        }

        private void GlobalEventManager_onServerDamageGlobal(DamageReport report)
        {
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
                return;

            CharacterBody attacker = report.attackerBody;

            //We need an inventory to do check for our item
            if (attacker.inventory)
            {
                //store the amount of our item we have
                int absoCount = GetCount(attacker);
                if (absoCount <= 0)
                {
                    return;
                }

                if (attacker.crit >= 100)
                {
                    attacker.healthComponent.Heal(MaxCritHealBaseAmount.Value + ((absoCount - 1) * MaxCritHealStackingAmount.Value), default);
                }
            }
        }

    }
}
