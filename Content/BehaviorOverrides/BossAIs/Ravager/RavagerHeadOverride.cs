﻿using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Ravager;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Ravager
{
    public class RavagerHeadBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<RavagerHead>();

        public override void SetDefaults(NPC npc)
        {
            // Set defaults that, if were to be changed by Calamity, would cause significant issues to the fight.
            npc.width = 80;
            npc.height = 80;
            npc.scale = 1f;
            npc.defense = 40;
            npc.DR_NERD(0.15f);
            npc.alpha = 255;
        }

        public override bool PreAI(NPC npc)
        {
            // Fuck off if the main boss is gone.
            if (CalamityGlobalNPC.scavenger < 0 || !Main.npc[CalamityGlobalNPC.scavenger].active)
            {
                npc.active = false;
                npc.netUpdate = true;
                return false;
            }

            NPC ravagerBody = Main.npc[CalamityGlobalNPC.scavenger];

            // Don't attack if the Ravager isn't ready to do so yet.
            npc.dontTakeDamage = false;
            npc.damage = 0;
            if (ravagerBody.Infernum().ExtraAI[5] < RavagerBodyBehaviorOverride.AttackDelay)
            {
                npc.damage = 0;
                npc.dontTakeDamage = true;
            }

            if (npc.timeLeft < 1800)
                npc.timeLeft = 1800;

            npc.Center = ravagerBody.Center + new Vector2(1f, -20f);
            npc.Opacity = ravagerBody.Opacity;

            return false;
        }
    }
}
