using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Twins
{
    public class CursedFlameBurstTelegraph : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Telegraph");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 42;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
        }

        public override void AI()
        {
            Projectile.Opacity = Sin(Projectile.timeLeft / 30f * Pi) * 1.6f;
            if (Projectile.Opacity > 1f)
                Projectile.Opacity = 1f;
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Pow(Utils.GetLerpValue(0f, 0.27f, completionRatio, true), 0.4f) * Utils.GetLerpValue(1f, 0.86f, completionRatio, true);
            return SmoothStep(3f, Projectile.width, squeezeInterpolant);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Green, Color.LimeGreen, Pow(completionRatio, 2f));
            color *= 1f - 0.5f * Pow(completionRatio, 3f);
            return color * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Utils.DrawLine(Main.spriteBatch, Projectile.Center - Vector2.UnitX * 1600f, Projectile.Center + Vector2.UnitX * 1600f, Color.LimeGreen, Color.LimeGreen, Projectile.Opacity * 1.6f + 0.1f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Utilities.NewProjectileBetter(Projectile.Center, Projectile.velocity, ModContent.ProjectileType<CursedFlameBurst>(), TwinsAttackSynchronizer.CursedFlameBurstDamage, 0f);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => false;
    }
}
