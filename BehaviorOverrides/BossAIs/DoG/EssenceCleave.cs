﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.DoG
{
    public class EssenceCleave : ModProjectile
    {
        public float LineWidth = 0f;
        public ref float Time => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chain");
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.MaxUpdates = 2;
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft == 40f)
            {
                Vector2 sliceSpawnPosition = Projectile.Center - Projectile.ai[0].ToRotationVector2() * 3900f;
                Utilities.NewProjectileBetter(sliceSpawnPosition, Projectile.ai[0].ToRotationVector2() * 6f, ModContent.ProjectileType<EssenceSlice>(), 450, 0f);

                sliceSpawnPosition = Projectile.Center + Projectile.ai[0].ToRotationVector2() * 3900f;
                Utilities.NewProjectileBetter(sliceSpawnPosition, Projectile.ai[0].ToRotationVector2() * -6f, ModContent.ProjectileType<EssenceSlice>(), 450, 0f);
            }

            if (Projectile.timeLeft < 40f)
                LineWidth -= 0.1f;
            else if (LineWidth < 5f)
                LineWidth += 0.3f;

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.SetBlendState(BlendState.Additive);

            Vector2 offset = Projectile.ai[0].ToRotationVector2() * 4000f;
            spriteBatch.DrawLineBetter(Projectile.Center - offset, Projectile.Center + offset, Color.Purple, LineWidth);
            spriteBatch.DrawLineBetter(Projectile.Center - offset, Projectile.Center + offset, Color.Magenta * 0.6f, LineWidth * 0.5f);

            spriteBatch.ResetBlendState();
            return true;
        }
    }
}

