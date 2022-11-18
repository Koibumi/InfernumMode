using CalamityMod;
using CalamityMod.NPCs.Abyss;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace InfernumMode.BehaviorOverrides.AbyssAIs
{
    public class ColossalSquidBehaviorOverride : NPCBehaviorOverride
    {
        public enum ColossalSquidAttackType
        {
            SwipeAtTarget,
            SprayInk
        }

        public override int NPCOverrideType => ModContent.NPCType<ColossalSquid>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        // Piecewise function variables for determining the offset of tentacles when swiping at the target.
        public static CurveSegment Anticipation => new(EasingType.PolyOut, 0f, 0f, -0.53f, 2);

        public static CurveSegment Slash => new(EasingType.PolyIn, 0.17f, Anticipation.EndingHeight, 2.2f, 3);

        public static CurveSegment Recovery => new(EasingType.SineOut, 0.4f, Slash.EndingHeight, -1.7f);

        #region AI and Behaviors
        public override bool PreAI(NPC npc)
        {
            ref float attackType = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];
            ref float isHostile = ref npc.Infernum().ExtraAI[5];
            ref float hasSummonedTentacles = ref npc.Infernum().ExtraAI[6];
            ref float leftTentacleIndex = ref npc.Infernum().ExtraAI[7];
            ref float rightTentacleIndex = ref npc.Infernum().ExtraAI[8];
            ref float universalAttackTimer = ref npc.Infernum().ExtraAI[9];

            // Reset things.
            npc.dontTakeDamage = false;
            npc.chaseable = isHostile == 1f;
            npc.damage = npc.defDamage;

            // Don't naturally despawn if sleeping.
            if (isHostile != 1f)
                npc.timeLeft = 7200;

            // Summon tentacles on the first frame.
            if (hasSummonedTentacles == 0f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    leftTentacleIndex = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X - 125, (int)npc.Center.Y + 1040, ModContent.NPCType<ColossalSquidTentacle>(), 1, 1f, npc.whoAmI);
                    rightTentacleIndex = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X + 100, (int)npc.Center.Y + 1040, ModContent.NPCType<ColossalSquidTentacle>(), 1, 0f, npc.whoAmI);
                }

                hasSummonedTentacles = 1f;
                npc.netUpdate = true;
            }

            // Become hostile if hit.
            if (npc.justHit && npc.Infernum().ExtraAI[0] != 1f)
            {
                isHostile = 1f;
                npc.netUpdate = true;
            }

            // Stop at this point if not hostile, and just sit in place.
            if (isHostile != 1f)
                return false;

            // Handle targeting.
            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];

            NPC leftTentacle = Main.npc[(int)leftTentacleIndex];
            NPC rightTentacle = Main.npc[(int)rightTentacleIndex];

            switch ((ColossalSquidAttackType)attackType)
            {
                case ColossalSquidAttackType.SwipeAtTarget:
                    DoBehavior_SwipeAtTarget(npc, target, leftTentacle, rightTentacle, ref attackTimer);
                    break;
                case ColossalSquidAttackType.SprayInk:
                    DoBehavior_SprayInk(npc, target, leftTentacle, rightTentacle, ref attackTimer);
                    break;
            }

            // Increment the attack timers.
            universalAttackTimer++;
            attackTimer++;

            return false;
        }

        public static void DoBehavior_SwipeAtTarget(NPC npc, Player target, NPC leftTentacle, NPC rightTentacle, ref float attackTimer)
        {
            int swingCount = 4;
            int swingAnimationTime = 84;
            float swingCompletion = attackTimer / swingAnimationTime % 1f;
            ref float swingFromRight = ref npc.Infernum().ExtraAI[0];
            ref float swingCounter = ref npc.Infernum().ExtraAI[1];

            if (swingCounter <= 0f)
            {
                swingAnimationTime += 54;
                if (attackTimer <= 48f)
                    npc.dontTakeDamage = true;
            }

            // Move the tentacles in a swiping motion.
            NPC tentacleToMove = swingFromRight == 1f ? rightTentacle : leftTentacle;
            NPC otherTentacle = swingFromRight == 1f ? leftTentacle : rightTentacle;
            Vector2 tentacleDirection = PiecewiseAnimation(swingCompletion, Anticipation, Slash, Recovery).ToRotationVector2() * new Vector2((swingFromRight == 1f).ToDirectionInt(), 1f);
            Vector2 legDestination = npc.Center + tentacleDirection * (Convert01To010(swingCompletion) * 100f + 200f);
            tentacleToMove.Center = tentacleToMove.Center.MoveTowards(legDestination, 54f);
            otherTentacle.Center = Vector2.Lerp(otherTentacle.Center, npc.Center + new Vector2((swingFromRight == 1f).ToDirectionInt() * -120f, 145f), 0.1f);

            // Try to hover above the target.
            if (swingCompletion >= 0.4f)
                npc.SimpleFlyMovement(npc.SafeDirectionTo(target.Center - Vector2.UnitY * 250f) * 15f, 0.5f);
            else
                npc.velocity *= 0.9f;
            npc.rotation = npc.velocity.X * 0.01f;

            if (attackTimer % swingAnimationTime == 30f)
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, npc.Center);

            // Swap tentacles once down swinging.
            if (attackTimer % swingAnimationTime == swingAnimationTime - 1f)
            {
                swingCounter++;
                swingFromRight = 1f - swingFromRight;
                if (swingCounter >= swingCount)
                    SelectNextAttack(npc);

                npc.netUpdate = true;
            }
        }

        public static void DoBehavior_SprayInk(NPC npc, Player target, NPC leftTentacle, NPC rightTentacle, ref float attackTimer)
        {
            int inkShootDelay = 90;
            int inkShootRate = 60;
            int shootTime = 360;
            ref float tentacleLockCenterX = ref npc.Infernum().ExtraAI[0];

            // Move the tentacles such that they lock the player in.
            if (tentacleLockCenterX == 0f)
            {
                tentacleLockCenterX = target.Center.X;
                npc.netUpdate = true;
            }
            else
            {
                leftTentacle.Center = Vector2.Lerp(leftTentacle.Center, new Vector2(tentacleLockCenterX - 400f, target.Center.Y + 100f), 0.1f);
                rightTentacle.Center = Vector2.Lerp(rightTentacle.Center, new Vector2(tentacleLockCenterX + 400f, target.Center.Y + 100f), 0.1f);
            }

            // Hover above the target.
            npc.SimpleFlyMovement(npc.SafeDirectionTo(new Vector2(tentacleLockCenterX, target.Center.Y - 400f)) * 15f, 0.5f);
            npc.rotation = npc.velocity.X * 0.01f;

            if (attackTimer < inkShootDelay)
            {
                npc.dontTakeDamage = true;
                return;
            }

            if (attackTimer >= inkShootDelay + shootTime)
                SelectNextAttack(npc);

            // Shoot ink.
            if (attackTimer % inkShootRate == inkShootRate - 1f)
            {
                SoundEngine.PlaySound(SoundID.Item111, npc.position);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 inkSpawnPosition = npc.Center + Vector2.UnitY.RotatedBy(npc.rotation) * 24f;
                    Vector2 inkVelocity = (target.Center - inkSpawnPosition).SafeNormalize(Vector2.UnitY) * 9f;
                    Utilities.NewProjectileBetter(inkSpawnPosition, inkVelocity, ModContent.ProjectileType<InkBlob>(), 0, 0f);
                }
            }
        }

        public static void SelectNextAttack(NPC npc)
        {
            switch ((ColossalSquidAttackType)npc.ai[0])
            {
                case ColossalSquidAttackType.SwipeAtTarget:
                    npc.ai[0] = (int)ColossalSquidAttackType.SprayInk;
                    break;
                case ColossalSquidAttackType.SprayInk:
                    npc.ai[0] = (int)ColossalSquidAttackType.SwipeAtTarget;
                    break;
            }
            npc.ai[1] = 0f;
            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;

            npc.netUpdate = true;
        }
        #endregion AI and Behaviors

        #region Frames and Drawcode
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("InfernumMode/BehaviorOverrides/AbyssAIs/SleepingColossalSquid").Value;
            Rectangle frame = texture.Frame();
            if (npc.Infernum().ExtraAI[5] == 1f)
            {
                texture = ModContent.Request<Texture2D>("InfernumMode/BehaviorOverrides/AbyssAIs/ColossalSquid").Value;
                frame = npc.frame;
            }

            Vector2 drawPosition = npc.Center - Main.screenPosition + Vector2.UnitY * 30f;
            SpriteEffects direction = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(texture, drawPosition, frame, npc.GetAlpha(Color.Gray), npc.rotation, frame.Size() * 0.5f, npc.scale, direction, 0f);
            return false;
        }
        #endregion Frames and Drawcode
    }
}
