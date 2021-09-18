﻿using CalamityMod.CalPlayer;
using CalamityMod.NPCs.DevourerofGods;
using InfernumMode.FuckYouModeAIs.DoG;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode
{
	public class PoDItems : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.CelestialSigil)
            {
                item.consumable = false;
                item.maxStack = 1;
            }

            if (item.type == ItemID.StarCannon)
                item.damage = 24;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type == ItemID.CelestialSigil)
            {
                foreach (TooltipLine line2 in tooltips)
                {
                    if (line2.mod == "Terraria" && line2.Name == "Tooltip0")
                    {
                        line2.text = "Summons the Moon Lord immediately\n" +
                                     "Creates an arena at the player's position\n" +
                                     "Not consumable.";
                    }
                }
            }
        }

        internal static void DoGTeleportDenialText(Player player)
        {
            if (!player.chaosState)
            {
                player.AddBuff(BuffID.ChaosState, CalamityPlayer.chaosStateDurationBoss, true);
                Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<RoDFailPulse>(), 0, 0f, player.whoAmI);

                string[] possibleEdgyShitToSay = new string[]
                {
                        "YOU CANNOT EVADE ME SO EASILY!",
                        "YOU CANNOT HOPE TO OUTSMART A MASTER OF DIMENSIONS!",
                        "NOT SO FAST!"
                };
                Main.NewText(Main.rand.Next(possibleEdgyShitToSay), Color.Cyan);
            }
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemID.RodofDiscord && (NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()) || NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHeadS>())))
            {
                if (PoDWorld.InfernumMode)
                {
                    DoGTeleportDenialText(player);
                    return false;
                }
            }
            return base.CanUseItem(item, player);
        }
        public override bool UseItem(Item item, Player player)
        {
            if (item.type == ItemID.CelestialSigil && !NPC.AnyNPCs(NPCID.MoonLordCore))
            {
                NPC.NewNPC((int)player.Center.X, (int)player.Center.Y, NPCID.MoonLordCore);
            }
            return base.UseItem(item, player);
        }
    }
}
