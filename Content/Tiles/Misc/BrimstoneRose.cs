﻿using InfernumMode.Content.Dusts;
using InfernumMode.Core.GlobalInstances.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace InfernumMode.Content.Tiles.Misc
{
    public class BrimstoneRose : ModTile
    {
        public const int Width = 2;

        public const int Height = 3;

        public const int WindPushLifetime = 45;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSpelunker[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(0, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(213, 80, 80));
        }

        // This tile should not be destroyed by any natural means, as it is a special decoration.
        // If someone somehow destroys it (such as with those Fargos autoplatforms), tough luck for them.
        public override bool CanExplode(int i, int j) => false;

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            int xFrameOffset = Main.tile[i, j].TileFrameX;
            int yFrameOffset = Main.tile[i, j].TileFrameY;
            if (xFrameOffset != 0 || yFrameOffset != 36)
                return;

            Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileDrawInfo t = new();
            DrawEffects(i, j, spriteBatch, ref t);
            return false;
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            j++;

            Texture2D rose = ModContent.Request<Texture2D>("InfernumMode/Content/Tiles/Misc/BrimstoneRose2").Value;
            Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPosition = new Vector2(i + 0.5f, j + 0.25f) * 16f + drawOffset - Main.screenPosition;
            Vector2 worldPosition = new Vector2(i + 1f, j).ToWorldCoordinates();

            WindGridSystem.Windgrid.GetWindTime(i, j, WindPushLifetime, out int windTimeLeft, out int directionX, out int directionY);

            if (windTimeLeft >= 1)
            {
                Dust brimstone = Dust.NewDustPerfect(worldPosition - Vector2.UnitY * 35f + Main.rand.NextVector2Circular(12f, 12f), ModContent.DustType<BrimstoneCinderDust>());
                brimstone.scale *= 1.25f;
                brimstone.velocity = -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f, 4f);
                brimstone.noGravity = true;
            }

            float windInterpolant = windTimeLeft / (float)WindPushLifetime;
            float windRotation = Utils.GetLerpValue(0f, 0.5f, windInterpolant, true) * Utils.GetLerpValue(1f, 0.5f, windInterpolant, true) * new Vector2(directionX, directionY).ToRotation()
                * 0.34f;

            Color drawColor = Lighting.GetColor(i, j) * 3f;

            spriteBatch.Draw(rose, drawPosition, null, drawColor, windRotation, rose.Size() * Vector2.UnitY, 1f, SpriteEffects.None, 0f);
        }
    }
}
