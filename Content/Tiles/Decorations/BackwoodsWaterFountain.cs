using FullSerializer.Internal;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class BackwoodsWaterFountain : ModTile {
    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.addTile(Type);

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(1, 3);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
        TileObjectData.newTile.StyleLineSkip = 2;

        TileObjectData.addTile(Type);

        RegisterItemDrop(ModContent.ItemType<Items.Placeable.Decorations.BackwoodsWaterFountain>());

        DustType = DustID.Stone;

        AddMapEntry(new Color(144, 148, 144), Language.GetText("MapObject.WaterFountain"));

        AnimationFrameHeight = 72;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void NearbyEffects(int i, int j, bool closer) {
        if (!Main.dedServ && Main.tile[i, j].TileFrameY >= 72) {
            Main.SceneMetrics.ActiveFountainColor = ModContent.Find<ModWaterStyle>(RoA.ModName + "/DruidBiomeWaterStyle").Slot;
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) => frame = Main.tileFrame[TileID.WaterFountain];

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {

    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        var texture = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(Type, 0, Main.tile[i, j].TileColor);
        if (texture == null)
            texture = TextureAssets.Tile[Type].Value;

        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        int addFrX = 0;
        int addFrY = 0;
        if (tile.TileFrameY >= 72) {
            addFrY = Main.tileFrame[Type];
            int num5 = i;
            if (tile.TileFrameX % 36 != 0)
                num5--;

            int frameCount = 6;
            addFrY += num5 % frameCount;
            if (addFrY >= frameCount)
                addFrY -= frameCount;

            addFrY *= 72;
        }
        else {
            addFrY = 0;
        }
        int animate = tile.TileFrameY >= AnimationFrameHeight ? addFrY : 0;

        Color color = Lighting.GetColor(i, j);
        spriteBatch.Draw(texture, new Vector2(i * 16, j * 16) - Main.screenPosition + zero + Vector2.UnitY * 2f,
            new Rectangle(tile.TileFrameX, tile.TileFrameY + animate, 16, 16), color, 0f, default, 1f, SpriteEffects.None, 0f);

        if (Main.InSmartCursorHighlightArea(i, j, out var actuallySelected)) {
            int num = (color.R + color.G + color.B) / 3;
            if (num > 10) {
                Texture2D highlightTexture = TextureAssets.HighlightMask[Type].Value;
                Color highlightColor = Colors.GetSelectionGlowColor(actuallySelected, num);
                spriteBatch.Draw(highlightTexture, new Vector2(i * 16, j * 16) - Main.screenPosition + zero + Vector2.UnitY * 2f,
                    new Rectangle(tile.TileFrameX, tile.TileFrameY + animate, 16, 16), highlightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                //Main.spriteBatch.DrawSelf(sourceRectangle: rect, texture: _highlightTexture, position: drawPosition, color: highlightColor, _rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
            }
        }

        return false;
    }

    public override bool RightClick(int i, int j) {
        SoundEngine.PlaySound(in SoundID.Mech, new Vector2?(new Vector2(i * 16, j * 16)));

        HitWire(i, j);

        return base.RightClick(i, j);
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Decorations.BackwoodsWaterFountain>();
    }

    public override void HitWire(int i, int j) {
        Tile tile = Main.tile[i, j];
        int topX = i - tile.TileFrameX % 36 / 18;
        int topY = j - tile.TileFrameY % 72 / 18;

        short frameAdjustment = (short)(tile.TileFrameY >= 72 ? -72 : 72);

        for (int x = topX; x < topX + 2; x++) {
            for (int y = topY; y < topY + 4; y++) {
                Main.tile[x, y].TileFrameY += frameAdjustment;

                if (Wiring.running) {
                    Wiring.SkipWire(x, y);
                }
            }
        }

        if (Main.netMode != NetmodeID.SinglePlayer) {
            NetMessage.SendTileSquare(-1, topX, topY, 2, 4);
        }
    }
}