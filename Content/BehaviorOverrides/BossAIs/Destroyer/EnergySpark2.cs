using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Destroyer
{
    public class EnergySpark2 : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Red Spark");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 18;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            Projectile.Opacity = 0f;
            Projectile.hide = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.Opacity = Clamp(Projectile.Opacity + 0.1f, 0f, 1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

            if (Projectile.timeLeft == 330)
                SoundEngine.PlaySound(CommonCalamitySounds.ExoLaserShootSound with { Volume = 0.5f }, Projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 56) * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 330)
            {
                float width = Sin(Pi * Utils.GetLerpValue(360f, 330f, Projectile.timeLeft, true)) * 5f + 1f;
                Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 5100f;
                Main.spriteBatch.DrawLineBetter(Projectile.Center, end, Color.Red * 0.4f, width * 1.8f);
                Main.spriteBatch.DrawLineBetter(Projectile.Center, end, Color.White * 0.6f, width);
                return false;
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < 7; i++)
            {
                Vector2 drawOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * i * 7f;
                Vector2 afterimageDrawPosition = Projectile.Center + drawOffset - Main.screenPosition;
                Color backAfterimageColor = Projectile.GetAlpha(lightColor) * ((7f - i) / 7f);
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, backAfterimageColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Color frontAfterimageColor = Projectile.GetAlpha(lightColor) * 0.15f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (TwoPi * i / 8f + Projectile.rotation - PiOver2).ToRotationVector2() * 4f;
                Vector2 afterimageDrawPosition = Projectile.Center + drawOffset - Main.screenPosition;
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, frontAfterimageColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override bool ShouldUpdatePosition() => Projectile.timeLeft < 330;

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI, List<int> overWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }
    }
}