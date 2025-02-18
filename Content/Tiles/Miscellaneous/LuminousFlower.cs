using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class LuminousFlower : SimpleTileBaseToGenerateOverTime {
    public const float MINLIGHTMULT = 0.5f;

    public override int[] AnchorValidTiles => [TileID.Grass, TileID.JungleGrass, TileID.CorruptGrass, TileID.CrimsonGrass, (ushort)ModContent.TileType<BackwoodsGrass>()];

    public override ushort ExtraChance => 300;

    public override ushort DropItem => (ushort)ModContent.ItemType<Items.Miscellaneous.LuminousFlower>();

    public override byte XSize => 2;
    public override byte YSize => 3;

    public override Color MapColor => new(211, 141, 162);

    protected override void SafeSetStaticDefaults() {
        Main.tileLavaDeath[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.Width = XSize;
        TileObjectData.newTile.Height = YSize;
        TileObjectData.newTile.DrawYOffset = 6;
        TileObjectData.addTile(Type);

        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        AnimationFrameHeight = 54;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 6;

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        LuminiousFlowerLightUp(i, j, out float _, modTile: TileLoader.GetTile(Type));
    }

    public static void LuminiousFlowerLightUp(int i, int j, out float progress, ModTile modTile = null) {
        if (!Helper.OnScreenWorld(i, j)) {
            progress = 0f;
            return;
        }
        float[] lengths = new float[Main.CurrentFrameFlags.ActivePlayersCount];
        for (int k = 0; k < Main.CurrentFrameFlags.ActivePlayersCount; k++) {
            Player player = Main.player[k];
            if (!player.active || player.dead) {
                continue;
            }
            lengths[k] = player.Distance(new Point(i, j).ToWorldCoordinates());
        }
        float dist = lengths.Min();
        float maxDist = 300f;
        void lightUp(float progress) {
            float r = 0.9f * progress;
            float g = 0.7f * progress;
            float b = 0.3f * progress;
            Lighting.AddLight(i, j, r, g, b);
            if (modTile != null) {
                Tile tile = Main.tile[i, j];
                Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
                if (Main.drawToScreen) {
                    zero = Vector2.Zero;
                }
                int height = tile.TileFrameY == 36 ? 18 : 16;
                Main.spriteBatch.Draw(modTile.GetTileGlowTexture(),
                                      new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                                      new Rectangle(tile.TileFrameX, tile.TileFrameY + Main.tileFrame[modTile.Type] * 18 * 3 - 2, 16, height),
                                      Color.White * progress, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
        if (dist < maxDist) {
            progress = MathHelper.Clamp(1f - dist / maxDist, MINLIGHTMULT, 0.85f);
            lightUp(progress);
        }
        else {
            progress = MINLIGHTMULT;
            lightUp(MINLIGHTMULT);
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        frameCounter++;
        if (frameCounter > 5) {
            frameCounter = 0;

            frame++;
            if (frame > 14) {
                frame = 0;
            }
        }
    }
}
