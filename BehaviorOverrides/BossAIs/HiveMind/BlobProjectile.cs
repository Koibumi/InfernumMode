﻿using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.HiveMind
{
    public class BlobProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hive Blob");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.Calamity().canBreakPlayerDefense = true;
            CooldownSlot = 1;
        }
        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];

            if (!Main.npc.IndexInRange(CalamityGlobalNPC.hiveMind))
            {
                Projectile.Kill();
                return;
            }
        }
        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 10; k++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 14, Main.rand.NextFloat(-1f, 1f), -1f, 0, default, 1f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color drawColor = Color.MediumPurple;
            drawColor.A = 0;

            Utilities.DrawAfterimagesCentered(Projectile, drawColor, ProjectileID.Sets.TrailingMode[Projectile.type], 3);
            return true;
        }
    }
}
