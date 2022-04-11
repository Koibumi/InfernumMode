using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AdultEidolonWyrm
{
    public class EidolicExplosion : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Explosion");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.15f;
            Projectile.Calamity().canBreakPlayerDefense = true;
        }

        public override void AI()
        {
            Projectile.scale += 0.18f;
            Projectile.Opacity = Utils.GetLerpValue(150f, 142f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, 40f, Projectile.timeLeft, true);
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.SetBlendState(BlendState.Additive);

            Texture2D texture = Main.projectileTexture[Projectile.type];
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            for (int i = 0; i < 3; i++)
            {
                Color explosionColor = Color.Lerp(Color.White, Color.Cyan, i / 2f) * Projectile.Opacity * 0.7f;
                spriteBatch.Draw(texture, drawPosition, null, explosionColor, 0f, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.ResetBlendState();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Utilities.CircularCollision(Projectile.Center, targetHitbox, Projectile.scale * 135f);
        }

        public override bool CanDamage() => Projectile.Opacity > 0.45f;
    }
}
