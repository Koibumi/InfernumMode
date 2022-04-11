﻿using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Items.Armor;
using CalamityMod.NPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.World;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using InfernumMode.Buffs;
using InfernumMode.Dusts;
using InfernumMode.MachineLearning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace InfernumMode
{
    public class PoDPlayer : ModPlayer
    {
        public bool RedElectrified = false;
        public bool ShadowflameInferno = false;
        public bool DarkFlames = false;
        public float CurrentScreenShakePower;
        public float MusicMuffleFactor;

        public Vector2 ScreenFocusPosition;
        public float ScreenFocusInterpolant = 0f;

        private MLAttackSelector thanatosLaserTypeSelector = null;
        private MLAttackSelector aresSpecialAttackTypeSelector = null;
        private MLAttackSelector twinsSpecialAttackTypeSelector = null;
        public MLAttackSelector ThanatosLaserTypeSelector
        {
            get
            {
                if (thanatosLaserTypeSelector is null)
                    thanatosLaserTypeSelector = new MLAttackSelector(3, "ThanatosLaser");
                return thanatosLaserTypeSelector;
            }
            set => thanatosLaserTypeSelector = value;
        }
        public MLAttackSelector AresSpecialAttackTypeSelector
        {
            get
            {
                if (aresSpecialAttackTypeSelector is null)
                    aresSpecialAttackTypeSelector = new MLAttackSelector(2, "AresSpecialAttack");
                return aresSpecialAttackTypeSelector;
            }
            set => twinsSpecialAttackTypeSelector = value;
        }
        public MLAttackSelector TwinsSpecialAttackTypeSelector
        {
            get
            {
                if (twinsSpecialAttackTypeSelector is null)
                    twinsSpecialAttackTypeSelector = new MLAttackSelector(2, "TwinsSpecialAttack");
                return twinsSpecialAttackTypeSelector;
            }
            set => twinsSpecialAttackTypeSelector = value;
        }

        public static bool ApplyEarlySpeedNerfs => InfernumMode.CalamityMod.Version < new Version("1.5.0.004");

        #region Skies
        internal static readonly FieldInfo EffectsField = typeof(SkyManager).GetField("_effects", BindingFlags.NonPublic | BindingFlags.Instance);
        public override void UpdateBiomeVisuals()
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            bool useFolly = NPC.AnyNPCs(InfernumMode.CalamityMod.Find<ModNPC>("Bumblefuck").Type) && (Main.npc[NPC.FindFirstNPC(InfernumMode.CalamityMod.Find<ModNPC>("Bumblefuck").Type)].Infernum().ExtraAI[8] > 0f);
            Player.ManageSpecialBiomeVisuals("InfernumMode:Dragonfolly", useFolly);

            if (!BossRushEvent.BossRushActive)
            {
                int hiveMindID = InfernumMode.CalamityMod.Find<ModNPC>("HiveMind").Type;
                int hiveMind = NPC.FindFirstNPC(hiveMindID);
                NPC hiveMindNPC = hiveMind >= 0 ? Main.npc[hiveMind] : null;
                bool useHIV = hiveMindNPC != null && (hiveMindNPC.Infernum().ExtraAI[10] == 1f || hiveMindNPC.life < hiveMindNPC.lifeMax * 0.2f);
                Player.ManageSpecialBiomeVisuals("InfernumMode:HiveMind", useHIV);

                bool useDeus = NPC.AnyNPCs(InfernumMode.CalamityMod.Find<ModNPC>("AstrumDeusHeadSpectral").Type);
                Player.ManageSpecialBiomeVisuals("InfernumMode:Deus", useDeus);

                int oldDukeID = ModContent.NPCType<OldDuke>();
                int oldDuke = NPC.FindFirstNPC(oldDukeID);
                NPC oldDukeNPC = oldDuke >= 0 ? Main.npc[oldDuke] : null;
                bool useOD = oldDukeNPC != null && oldDukeNPC.Infernum().ExtraAI[6] >= 2f;
                Player.ManageSpecialBiomeVisuals("InfernumMode:OldDuke", useOD);
            }
        }
        #endregion
        #region Reset Effects
        public override void ResetEffects()
        {
            RedElectrified = false;
            ShadowflameInferno = false;
            DarkFlames = false;
            ScreenFocusInterpolant = 0f;
            MusicMuffleFactor = 0f;
        }
        #endregion
        #region Update Dead
        public override void UpdateDead()
        {
            RedElectrified = false;
            ShadowflameInferno = false;
            DarkFlames = false;

            if (PoDWorld.InfernumMode)
                Player.respawnTimer = Utils.Clamp(Player.respawnTimer - 1, 0, 3600);
        }
        #endregion
        #region Pre Hurt
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (InfernumMode.CanUseCustomAIs && CalamityGlobalNPC.adultEidolonWyrmHead >= 0 && Main.npc[CalamityGlobalNPC.adultEidolonWyrmHead].Calamity().CurrentlyEnraged)
                damage = (int)MathHelper.Max(5500f / (1f - Player.endurance + 1e-6f), damage);
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
        }
        #endregion Pre Hurt
        #region Pre Kill
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if (RedElectrified)
                    damageSource = PlayerDeathReason.ByCustomReason($"{Player.name} could not withstand the red lightning.");
                if (DarkFlames)
                    damageSource = PlayerDeathReason.ByCustomReason($"{Player.name} was incinerated by ungodly fire.");
            }
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }
        #endregion
        #region Kill
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            ExoMechManagement.RecordAttackDeath(Player);
        }
        #endregion Kill
        #region Life Regen
        public override void UpdateLifeRegen()
        {
            void causeLifeRegenLoss(int regenLoss)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= regenLoss;
            }
            if (RedElectrified)
                causeLifeRegenLoss(Player.controlLeft || Player.controlRight ? 64 : 16);

            if (ShadowflameInferno)
                causeLifeRegenLoss(23);
            if (DarkFlames)
            {
                causeLifeRegenLoss(30);
                Player.statDefense -= 8;
            }
        }
        #endregion
        #region Screen Shaking
        public override void ModifyScreenPosition()
        {
            if (ScreenFocusInterpolant > 0f)
            {
                Vector2 idealScreenPosition = ScreenFocusPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, idealScreenPosition, ScreenFocusInterpolant);
            }

            if (CurrentScreenShakePower > 0f)
                CurrentScreenShakePower = Utils.Clamp(CurrentScreenShakePower - 0.2f, 0f, 15f);
            else
                return;

            if (CalamityConfig.Instance.DisableScreenShakes)
                return;

            Main.screenPosition += Main.rand.NextVector2CircularEdge(CurrentScreenShakePower, CurrentScreenShakePower);
        }
        #endregion
        #region Saving and Loading
        public override void SaveData(TagCompound tag)
        {
            ThanatosLaserTypeSelector?.Save(tag);
            AresSpecialAttackTypeSelector?.Save(tag);
            TwinsSpecialAttackTypeSelector?.Save(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            ThanatosLaserTypeSelector = MLAttackSelector.Load(tag, "ThanatosLaser");
            AresSpecialAttackTypeSelector = MLAttackSelector.Load(tag, "AresSpecialAttack");
            TwinsSpecialAttackTypeSelector = MLAttackSelector.Load(tag, "TwinsSpecialAttack");
        }
        #endregion Saving and Loading
        #region Misc Effects
        public void MakeAnxious(int time)
        {
            int deleteIndex = Player.MaxBuffs - 1;
            int[] buffsToSkip = new int[]
            {
                BuffID.ChaosState,
                BuffID.PotionSickness,
                BuffID.ManaSickness,
                ModContent.BuffType<ManaBurn>()
            };
            while (buffsToSkip.Contains(Player.buffType[deleteIndex]) && deleteIndex > 0)
                deleteIndex--;

            Player.DelBuff(deleteIndex);
            Player.AddBuff(ModContent.BuffType<Anxiety>(), time);
        }
        public override void PostUpdateMiscEffects()
        {
            if (Player.mount.Active && Player.mount.Type == MountID.Slime && NPC.AnyNPCs(InfernumMode.CalamityMod.Find<ModNPC>("DesertScourgeHead").Type))
            {
                Player.mount.Dismount(Player);
            }

            // Ensure that Revengeance Mode is always active while Infernum is active.
            if (PoDWorld.InfernumMode && !CalamityWorld.revenge)
                CalamityWorld.revenge = true;

            // Ensure that Malice Mode is never active while Infernum is active.
            if (PoDWorld.InfernumMode && CalamityWorld.malice)
            {
                CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.MaliceText2", Color.Crimson);
                CalamityWorld.malice = false;
            }

            if (PoDWorld.InfernumMode && CalamityWorld.DoGSecondStageCountdown > 600)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active &&
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("Signus") .Type||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("StormWeaverHead").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("StormWeaverBody").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("StormWeaverTail").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("StormWeaverNakedHead").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("StormWeaverNakedBody").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("StormWeaverNakedTail").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("CeaselessVoid").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("DarkEnergy").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("DarkEnergy2").Type) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.Find<ModNPC>("DarkEnergy3").Type)))
                    {
                        Main.npc[i].active = false;
                    }
                }
                CalamityWorld.DoGSecondStageCountdown = 599;
            }

            if (ShadowflameInferno)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust shadowflame = Dust.NewDustDirect(Player.position, Player.width, Player.height, 28);
                    shadowflame.velocity = Player.velocity.SafeNormalize(Vector2.UnitX * Player.direction);
                    shadowflame.velocity = shadowflame.velocity.RotatedByRandom(0.4f) * -Main.rand.NextFloat(2.5f, 5.4f);
                    shadowflame.scale = Main.rand.NextFloat(0.95f, 1.3f);
                    shadowflame.noGravity = true;
                }
            }

            if (DarkFlames)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust shadowflame = Dust.NewDustDirect(Player.position, Player.width, Player.height, ModContent.DustType<RavagerMagicDust>());
                    shadowflame.velocity = Player.velocity.SafeNormalize(Vector2.UnitX * Player.direction);
                    shadowflame.velocity = shadowflame.velocity.RotatedByRandom(0.4f) * -Main.rand.NextFloat(2.5f, 5.4f);
                    shadowflame.velocity += Main.rand.NextVector2Circular(3f, 3f);
                    shadowflame.scale = Main.rand.NextFloat(0.95f, 1.25f);
                    shadowflame.noGravity = true;
                }
            }
        }
        #endregion
    }
}