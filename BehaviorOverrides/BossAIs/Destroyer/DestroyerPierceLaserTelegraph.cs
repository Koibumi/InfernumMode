using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Destroyer
{
    public class DestroyerPierceLaserTelegraph : ModProjectile
    {
        public int NPCToAttachToIndex => (int)projectile.ai[0];
        public NPC NPCToAttachTo => Main.npc.IndexInRange(NPCToAttachToIndex) ? Main.npc[NPCToAttachToIndex] : null;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Telegraph");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 30;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Die if the NPC to attach do is not valid.
            if (NPCToAttachTo is null || !NPCToAttachTo.active)
			{
                projectile.Kill();
                return;
			}

            // Pulse in and out.
            projectile.scale = (float)Math.Sin(MathHelper.Pi * projectile.timeLeft / 30f) * 6f;
            projectile.Center = NPCToAttachTo.Center;
        }

        public override bool CanDamage() => false;

        public override bool ShouldUpdatePosition() => false;

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Create an inner and outer telegraph.
            Color outerTelegraphColor = new Color(255, 70, 53, 0);
            Color innerTelegraphColor = new Color(255, 142, 132, 0) * 1.15f;
            float outerTelegraphScale = projectile.scale;
            float innerTelegraphScale = outerTelegraphScale * 0.56f;
            Vector2 telegraphStart = projectile.Center;
            Vector2 telegraphEnd = telegraphStart + projectile.velocity.SafeNormalize(Vector2.UnitY) * 5000f;

            spriteBatch.DrawLineBetter(telegraphStart, telegraphEnd, outerTelegraphColor, outerTelegraphScale);
            spriteBatch.DrawLineBetter(telegraphStart, telegraphEnd, innerTelegraphColor, innerTelegraphScale);
            return false;
        }

		public override void Kill(int timeLeft)
		{
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int laser = Utilities.NewProjectileBetter(projectile.Center, projectile.velocity * 14f, ProjectileID.DeathLaser, 120, 0f);
            if (Main.projectile.IndexInRange(laser))
                Main.projectile[laser].tileCollide = false;
		}
	}
}
