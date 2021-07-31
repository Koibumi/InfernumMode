﻿using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace InfernumMode.FuckYouModeAIs.Prime
{
    public class PrimeSawBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.PrimeSaw;

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI;

        public override bool PreAI(NPC npc)
        {
            float lifeRatio = npc.life / (float)npc.lifeMax;
            float hoverDirection = npc.ai[0];
            float ownerIndex = npc.ai[1];
            ref float attackTimer = ref npc.ai[2];

            if (!Main.npc.IndexInRange((int)ownerIndex) || !Main.npc[(int)ownerIndex].active)
            {
                npc.life = 0;
                npc.StrikeNPCNoInteraction(9999, 0f, 0);
                npc.netUpdate = true;
                return false;
            }

            NPC owner = Main.npc[(int)ownerIndex];
            npc.target = owner.target;

            Player target = Main.player[npc.target];

            // Disable contact damage.
            npc.damage = 0;

            bool shouldBeInactive = PrimeHeadBehaviorOverride.ShouldBeInactive(npc.type, owner.ai[2]);

            if (shouldBeInactive)
            {
                attackTimer = 0f;
                Vector2 hoverDestination = owner.Center + new Vector2(hoverDirection * -200f, 320f) + owner.velocity * 4f;
                if (!npc.WithinRange(hoverDestination, 40f))
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 20f, shouldBeInactive ? (owner.velocity.Length() * 0.05f + 0.7f) : 0.18f);
                PrimeHeadBehaviorOverride.ArmHoverAI(npc);
                return false;
            }

            attackTimer++;

            float chargeCycleTime = PrimeHeadBehaviorOverride.RemainingArms == 1 ? 110f : 180f;
            float wrappedTime = attackTimer % chargeCycleTime;
            bool canCharge = lifeRatio < 0.5f || PrimeHeadBehaviorOverride.RemainingArms <= 2;
            bool willCharge = canCharge && wrappedTime > chargeCycleTime - 65f;

            if (willCharge)
            {
                if (wrappedTime > chargeCycleTime - 45f)
                {
                    if (wrappedTime % 5f == 4f)
                        Main.PlaySound(SoundID.Item22, npc.Center);

                    if (wrappedTime == chargeCycleTime - 44f)
                    {
                        npc.velocity = npc.SafeDirectionTo(target.Center) * 25f;
                        npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
                        npc.netUpdate = true;
                    }

                    if (wrappedTime > chargeCycleTime - 15f)
                        npc.velocity *= 0.93f;
                }
                else
                {
                    npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(target.Center) - MathHelper.PiOver2, 0.1f);
                    npc.velocity *= 0.92f;
                }
            }
            else
            {
                float rotationalOffset = (float)Math.Sin(attackTimer / 37f) * 0.64f;
                float outwardness = MathHelper.Clamp(owner.Distance(target.Center), 120f, 460f);
                float idealRotation = owner.AngleTo(npc.Center) + rotationalOffset - MathHelper.PiOver2;
                Vector2 hoverDestination = owner.Center + owner.SafeDirectionTo(target.Center).RotatedBy(rotationalOffset) * outwardness;
                if (npc.WithinRange(target.Center, 240f))
                    hoverDestination = target.Center;

                npc.rotation = npc.rotation.AngleLerp(idealRotation, 0.08f);

                if (!npc.WithinRange(hoverDestination, 80f))
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 20f, 0.25f);
            }

            return false;
        }
    }
}