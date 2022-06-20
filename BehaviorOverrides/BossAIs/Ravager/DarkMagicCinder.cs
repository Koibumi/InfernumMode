using CalamityMod;
using InfernumMode.Buffs;
using InfernumMode.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Ravager
{
    public class DarkMagicCinder : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Magic Cinder");
            Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 360;
            projectile.scale = 1.6f;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Lighting.AddLight(projectile.Center, Color.Blue.ToVector3() * 0.64f);

            projectile.Opacity = (float)Math.Sin(MathHelper.Pi * projectile.timeLeft / 360f) * 5f;
            if (projectile.Opacity > 1f)
                projectile.Opacity = 1f;

            if (projectile.frameCounter++ % 5 == 4)
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];

            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override bool CanDamage() => projectile.Opacity > 0.75f;

        public override Color? GetAlpha(Color lightColor) => new Color(1f, 0.1f, 1f, 0.4f) * projectile.Opacity;

        public override void Kill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RavagerMagicDust>());
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.noGravity = true;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<DarkFlames>(), 120);
    }
}
