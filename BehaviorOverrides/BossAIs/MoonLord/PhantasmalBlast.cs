﻿using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.MoonLord
{
    public class PhantasmalBlast : ModProjectile
    {
        public const int Lifetime = 240;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phantasmal Blast");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.width = 90;
            projectile.height = 34;
            projectile.hostile = true;
            projectile.tileCollide = true;
            projectile.timeLeft = Lifetime;
            projectile.Calamity().canBreakPlayerDefense = true;
        }
        public override void AI()
        {
            if (projectile.localAI[0] == 0f)
            {
                if (Main.rand.Next(2) == 0)
                    Main.PlaySound(SoundID.Item124, projectile.position);
                else
                    Main.PlaySound(SoundID.Item125, projectile.position);
                projectile.localAI[0] = 1f;
            }
            Player player = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (projectile.timeLeft < Lifetime - 60)
            {
                projectile.velocity = (projectile.velocity * 60f + projectile.SafeDirectionTo(player.Center) * 13f) / 60.5f;
            }
            if (!NPC.AnyNPCs(NPCID.MoonLordCore))
            {
                projectile.active = false;
                return;
            }
            NPC core = Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)];
            projectile.tileCollide = projectile.Hitbox.Intersects(core.Infernum().arenaRectangle);
            if (projectile.Distance(player.Center) < 30f)
                projectile.Kill();
            projectile.rotation = projectile.velocity.ToRotation();
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 65; k++)
            {
                Vector2 velocity = (MathHelper.TwoPi / 65f * k).ToRotationVector2() * Main.rand.NextFloat(6f, 30f);
                velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 229, velocity.X, velocity.Y, 0, default, 1f);
            }
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.NextFloat(21f, 32f)) * (i - 2f) / 2f;
                Projectile.NewProjectile(projectile.Center, new Vector2(0f, -6f).RotatedBy(angle), ModContent.ProjectileType<PhantasmalSpark>(), 39, 1f);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 2);
            return false;
        }
    }
}
