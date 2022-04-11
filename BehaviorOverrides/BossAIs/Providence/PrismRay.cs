using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class PrismRay : BaseLaserbeamProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float HueOffset => ref Projectile.ai[1];
        public override float MaxScale => 2.6f;
        public override float MaxLaserLength => 2800f;
        public override float Lifetime => LaserLifetime;
        public override Color LaserOverlayColor => Main.hslToRgb((float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.04f + HueOffset) * 0.5f + 0.5f, 1f, 0.5f) * Utils.GetLerpValue(Lifetime, Lifetime - 20f, Time, true);
        public override Color LightCastColor => LaserOverlayColor;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayStart").Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayMid").Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/UltimaRayEnd").Value;

        public float InitialRotationalSpeed = 0f;
        // To allow easy, static access from different locations.
        public const int LaserLifetime = 240;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Prismatic Ray");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 25;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.Calamity().canBreakPlayerDefense = true;
        }

        public override void AttachToSomething()
        {
            Projectile.Center = Main.npc[CalamityGlobalNPC.holyBoss].Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            if (InitialRotationalSpeed == 0f)
            {
                InitialRotationalSpeed = RotationalSpeed;
                RotationalSpeed = 0f;
                Projectile.netUpdate = true;
            }

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

            float offsetAngleFromTarget = Math.Abs(MathHelper.WrapAngle(Projectile.velocity.ToRotation() - Projectile.AngleTo(target.Center)));
            if (Time <= 60f)
                RotationalSpeed = Time / 60f * InitialRotationalSpeed;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.EnterShaderRegion();

            // Apply a super special shader to the laser.
            Texture2D laserTexture = ModContent.Request<Texture2D>("InfernumMode/BehaviorOverrides/BossAIs/Providence/ProvidenceShaderTextureTransparent").Value;
            if (!Main.dayTime)
                laserTexture = ModContent.Request<Texture2D>("InfernumMode/BehaviorOverrides/BossAIs/Providence/ProvidenceShaderTextureTransparentNight").Value;

            MiscShaderData gradientShader = GameShaders.Misc["Infernum:GradientWingShader"];
            gradientShader.UseImage1("Images/Misc/Noise");
            gradientShader.UseOpacity(0.7f);
            gradientShader.SetShaderTexture(laserTexture);

            // And draw the laser.
            gradientShader.Apply(null);
            base.PreDraw(spriteBatch, lightColor);

            Utilities.ExitShaderRegion(spriteBatch);
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
