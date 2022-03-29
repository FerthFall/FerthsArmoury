using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FerthsArmory.Main;

namespace FerthsArmory.Items
{
    internal class AbsolutionStacks : ItemBase<AbsolutionStacks>
    {
        public ConfigEntry<float> ValuePerSoul;

        public override string ItemName => "Absolution Stacks";

        public override string ItemLangTokenName => "ABSOLUTION_STACKS_ITEM";

        public override string ItemPickupDesc => String.Empty;

        public override string ItemFullDescription => $"Increases <style=cIsDamage>base attack and crit chance</style> by <style=cStack>(+{ValuePerSoul.Value.ToString()} per stack)</style>";

        public override string ItemLore => "Stackz on Stackz bby";

        public override ItemTier Tier => ItemTier.NoTier;

        public override bool CanRemove => false;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("AbsolutionStacksDisplay.prefab");

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("AbsolutionStacksIcon.png");

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void CreateConfig(ConfigFile config)
        {
            ValuePerSoul = config.Bind<float>("Item: " + ItemName, "Amount of damage and crit chance granted per stack", 1, String.Empty);
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += recalcualteAbsolutionStats;
        }
        private void recalcualteAbsolutionStats(RoR2.CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body != null && args != null && body.inventory != null)
            {
                float absoCount = GetCount(body);

                if (absoCount <= 0)
                {
                    return;
                }

                args.baseDamageAdd += ValuePerSoul.Value * absoCount;
                args.critAdd += ValuePerSoul.Value * absoCount;
            }
        }
    }
}
