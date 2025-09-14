using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.Tiles;
using RoA.Common.UI;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class NixieTube : ModTile, TileHooks.IPostDraw {
    private static BlendState? _multiplyBlendState;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        //Main.tileSolidTop[Type] = true;
        Main.tileLighted[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<NixieTubeTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.addTile(Type);

        DustType = -1;

        AddMapEntry(new Color(153, 38, 0), CreateMapEntryName());
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        if (frameX != 0 && frameY != 0) {
            Terraria.Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Items.Placeable.Decorations.NixieTube>());
        }
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
        return true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<NixieTubeTE>().Kill(i, j);

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80)) {
            player.cursorItemIconID = ItemID.Cog;
            if (player.cursorItemIconID != -1) {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
            }
        }
    }

    public override bool RightClick(int i, int j) {
        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80) && TryGetTE(out NixieTubeTE? nixieTubeTE, i, j)) {
            //NixieTubePicker.Activate(new Point16(i, j));
            NixieTubePicker_RemadePicker.Toggle(i, j, nixieTubeTE.IsFlickerOff);
            nixieTubeTE.LightColor = null;
        }

        return true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];
        int frameY = tile.TileFrameY;
        int frameX = tile.TileFrameX;
        bool flag = frameY >= 56;
        if (flag && TryGetTE(out NixieTubeTE? nixieTubeTE, i, j) && nixieTubeTE!.Active) {
            if (!nixieTubeTE.Dye2.IsEmpty() && nixieTubeTE.Dye2.type == ItemID.ShadowDye) {
                r = g = b = 0f;
                return;
            }
            Color? lightColor = nixieTubeTE.LightColor;
            if (lightColor == null) {
                r = 224 / 255f;
                g = 74 / 255f;
                b = 0 / 255f;
                return;
            }
            Color lightColor2 = lightColor.Value;
            r = lightColor2.R / 255f;
            g = lightColor2.G / 255f;
            b = lightColor2.B / 255f;
        }
    }

    void TileHooks.IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 pos) {
        int i = pos.X;
        int j = pos.Y;
        if (TryGetTE(out NixieTubeTE? nixieTubeTE, i, j)) {
            nixieTubeTE.UpdateLightColor(true);
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        if (!TileDrawing.IsVisible(tile)) {
            return false;
        }

        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int frameY = tile.TileFrameY;
        int frameX = tile.TileFrameX;
        bool flag = frameY >= 56;
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[Type].Value;
        TileObjectData tileObjectData = TileObjectData.GetTileData(Type, 0);

        int height = WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type ? 18 : 16;

        Color color = Lighting.GetColor(i, j);
        spriteBatch.Draw(texture,
                         new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                         new Rectangle(frameX % 36, frameY % 56, 16, height),
                         color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        if (TryGetTE(out NixieTubeTE? nixieTubeTE, i, j)) {
            SpriteBatchSnapshot snapshot;
            DrawData drawData;
            if (flag) {
                snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.GraphicsDevice.RasterizerState, null, snapshot.transformationMatrix);
                drawData = new(TextureAssets.Tile[Type].Value,
                               new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                               new Rectangle(frameX, frameY, 16, height),
                               Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                if (!nixieTubeTE.Dye1.IsEmpty()) {
                    GameShaders.Armor.GetShaderFromItemId(nixieTubeTE.Dye1.type).Apply(null, drawData);
                }
                if (nixieTubeTE!.Active) {
                    drawData.Draw(spriteBatch);
                    TileHelper.AddPostSolidTileDrawPoint(this, i, j);
                }
                spriteBatch.Begin(snapshot, true);
            }

            _multiplyBlendState ??= new() {
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.SourceAlpha
            };

            bool hasCasingDye = !nixieTubeTE.Dye2.IsEmpty();
            if (flag || hasCasingDye) {
                snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
                spriteBatch.Begin(snapshot with { sortMode = SpriteSortMode.Immediate, blendState = _multiplyBlendState }, true);
                drawData = new(texture,
                               new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                               new Rectangle(36 + frameX % 36, frameY % 56, 16, height),
                               color * 1f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                if (hasCasingDye) {
                    GameShaders.Armor.GetShaderFromItemId(nixieTubeTE.Dye2.type).Apply(null, drawData);
                }
                drawData.Draw(spriteBatch);

                spriteBatch.Begin(snapshot with { sortMode = SpriteSortMode.Immediate, blendState = BlendState.Additive }, true);
                drawData = new(texture,
                               new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                               new Rectangle(36 + frameX % 36, frameY % 56, 16, height),
                               color * 1f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                if (hasCasingDye) {
                    GameShaders.Armor.GetShaderFromItemId(nixieTubeTE.Dye2.type).Apply(null, drawData);
                }
                drawData.Draw(spriteBatch);

                spriteBatch.Begin(snapshot with { sortMode = SpriteSortMode.Immediate }, true);
                drawData = new(texture,
                               new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                               new Rectangle(36 + frameX % 36, frameY % 56, 16, height),
                               color * 0.25f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                if (hasCasingDye) {
                    GameShaders.Armor.GetShaderFromItemId(nixieTubeTE.Dye2.type).Apply(null, drawData);
                }
                drawData.Draw(spriteBatch);

                spriteBatch.Begin(snapshot, true);
            }
        }

        if (Main.InSmartCursorHighlightArea(i, j, out var actuallySelected)) {
            int num = (color.R + color.G + color.B) / 3;
            if (num > 10) {
                Texture2D highlightTexture = TextureAssets.HighlightMask[Type].Value;
                Color highlightColor = Colors.GetSelectionGlowColor(actuallySelected, num);
                spriteBatch.Draw(highlightTexture,
                                 new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                                 new Rectangle(frameX % 36, frameY % 56, 16, height),
                                 highlightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                //Main.spriteBatch.DrawSelf(sourceRectangle: rect, texture: highlightTexture, position: drawPosition, color: highlightColor, _rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
            }
        }

        return false;
    }


    public static bool TryGetTE(out NixieTubeTE? nixieTubeTE, int i, int j) {
        NixieTubeTE? result = GetTE(i, j);
        if (result is not null) {
            nixieTubeTE = result;
            return true;
        }

        nixieTubeTE = null;
        return false;
    }

    public static NixieTubeTE? GetTE(int i, int j) {
        ushort nixieTubeTileType = (ushort)ModContent.TileType<NixieTube>();
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != nixieTubeTileType) {
            bool teLeft = TileHelper.GetTE<NixieTubeTE>(i, j) != null;
            bool teRight = TileHelper.GetTE<NixieTubeTE>(i + 1, j) != null;
            if (teLeft || teRight) {
                i += teRight.ToInt();
                return TileHelper.GetTE<NixieTubeTE>(i, j);
            }
        }

        while (WorldGenHelper.GetTileSafely(i, j + 1).TileType == nixieTubeTileType) {
            j++;
            if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != nixieTubeTileType) {
                bool teLeft = TileHelper.GetTE<NixieTubeTE>(i, j) != null;
                bool teRight = TileHelper.GetTE<NixieTubeTE>(i + 1, j) != null;
                if (teLeft || teRight) {
                    i += teRight.ToInt();
                    return TileHelper.GetTE<NixieTubeTE>(i, j);
                }
            }
        }

        return null;
    }
}
