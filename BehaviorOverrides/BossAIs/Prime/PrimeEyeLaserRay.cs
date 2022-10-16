using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Prime
{
    public class PrimeEyeLaserRay : BaseLaserbeamProjectile
    {
        public ref float AngularVelocity => ref Projectile.ai[0];
        public int OwnerIndex => (int)Projectile.ai[1];
        public override float Lifetime => 120;
        public override Color LaserOverlayColor => Color.White;
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/PrimeBeamBegin", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/PrimeBeamMid", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("InfernumMode/ExtraTextures/PrimeBeamEnd", AssetRequestMode.ImmediateLoad).Value;
        public override float MaxLaserLength => 2400f;
        public override float MaxScale => 1f;
        public override string Texture => "InfernumMode/ExtraTextures/PrimeBeamBegin";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Deathray");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = (int)Lifetime;
            Projectile.Calamity().DealsDefenseDamage = true;
            CooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }
        public override void AttachToSomething()
        {
            if (!Main.npc.IndexInRange(OwnerIndex))
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = Main.npc[OwnerIndex].Center + new Vector2((AngularVelocity > 0f).ToDirectionInt() * 16f, -7f).RotatedBy(Main.npc[OwnerIndex].rotation) + Projectile.velocity * 2f;
            Projectile.velocity = Projectile.velocity.RotatedBy(AngularVelocity).SafeNormalize(Vector2.UnitY);
        }
    }
}
