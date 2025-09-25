using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class EnragedVisuals : ModPlayer {
    internal float _opacity;
    internal bool _isActive, _isActive2;

    public override void UpdateDead() {
        UpdateVisuals();
    }

    public override void PostUpdate() {
        UpdateVisuals();
    }

    public override void ResetEffects() {
        _isActive2 = false;
    }

    internal void UpdateVisuals() {
        if (Main.dedServ) {
            return;
        }

        string shader = ShaderLoader.EnragedLothorSky;
        int type = ModContent.NPCType<Lothor>();
        if (!_isActive) {
            Player.ManageSpecialBiomeVisuals(shader, false);
        }
        _isActive = false;
        Filters.Scene[shader].GetShader().UseOpacity(_opacity).UseColor(Color.Red);
        NPC npc = null;
        if (NPC.AnyNPCs(type)) {
            npc = Main.npc.FirstOrDefault(x => x.active && x.type == type);
        }
        void deactivate() {
            if (Filters.Scene[shader].IsActive()) {
                Filters.Scene[shader].Deactivate();
            }
            if (_opacity > 0f) {
                _opacity -= 0.05f;
            }
            else {
                _opacity = 0f;
            }
        }
        if (npc == null && !LothorEnrageScene.MonolithNearby && !_isActive2) {
            deactivate();
            return;
        }
        bool enragedLothor = LothorEnrageScene.MonolithNearby || _isActive2 || npc.As<Lothor>()._shouldEnrage;
        if (enragedLothor) {
            _isActive = true;
            if (_opacity < 1f) {
                _opacity += 0.05f;
            }
            else {
                _opacity = 1f;
            }
            Player.ManageSpecialBiomeVisuals(shader, _isActive);
            if (!Filters.Scene[shader].IsActive()) {
                Filters.Scene.Activate(shader);
            }
            else {
                Filters.Scene[shader].GetShader().UseOpacity(_opacity);
            }
        }
        else {
            deactivate();
        }
    }
}

sealed class LothorEnrageScene : ModSceneEffect {
    private class LothorEnrageMonolithSystem : ModSystem {
        public override void ResetNearbyTileEffects() {
            MonolithNearby = false;
        }
    }

    internal static float _opacity;

    public static bool MonolithNearby;

    public override bool IsSceneEffectActive(Player player) {
        return MonolithNearby;
    }

    public override void SpecialVisuals(Player player, bool isActive) { }
}

sealed class LothorEnrageMonolith : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 5;
        TileObjectData.newTile.Origin = new Point16(1, 4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(216, 14, 19));
        //DustType = ModContent.DustType<CosmicCrystalDust>();

        AnimationFrameHeight = 18 * 5;

        DustType = ModContent.DustType<LothorEnrageMonolithDust>();
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Decorations.LothorEnrageMonolith>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = fail ? num / 3 : (int)(num / 2);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
        return true;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        drawData.finalColor = Color.White;
    }

    public override void NearbyEffects(int i, int j, bool closer) {
        if (Main.tile[i, j].TileFrameY >= 18 * 5 + 2) {
            LothorEnrageScene.MonolithNearby = true;
        }
    }

    public override bool RightClick(int i, int j) {
        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
        HitWire(i, j);
        return true;
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Decorations.LothorEnrageMonolith>();
    }

    public override void HitWire(int i, int j) {
        int x = i - Main.tile[i, j].TileFrameX / 18 % 3;
        int y = j - Main.tile[i, j].TileFrameY / 18 % 5;
        for (int l = x; l < x + 3; l++) {
            for (int m = y; m < y + 5; m++) {
                if (Main.tile[l, m].HasTile && Main.tile[l, m].TileType == Type) {
                    if (Main.tile[l, m].TileFrameY < 18 * 5 + 2) {
                        Main.tile[l, m].TileFrameY += 18 * 5 + 2;
                    }
                    else {
                        Main.tile[l, m].TileFrameY -= 18 * 5 + 2;
                    }
                }
            }
        }
        if (Wiring.running) {
            Wiring.SkipWire(x, y);
            Wiring.SkipWire(x, y + 1);
            Wiring.SkipWire(x, y + 2);
            Wiring.SkipWire(x, y + 3);
            Wiring.SkipWire(x, y + 4);
            Wiring.SkipWire(x + 1, y);
            Wiring.SkipWire(x + 1, y + 1);
            Wiring.SkipWire(x + 1, y + 2);
            Wiring.SkipWire(x + 1, y + 3);
            Wiring.SkipWire(x + 1, y + 4);
            Wiring.SkipWire(x + 2, y);
            Wiring.SkipWire(x + 2, y + 1);
            Wiring.SkipWire(x + 2, y + 2);
            Wiring.SkipWire(x + 2, y + 3);
            Wiring.SkipWire(x + 2, y + 4);
        }
        NetMessage.SendTileSquare(-1, x, y, 3, 5);
    }

    public override void AnimateTile(ref int frame, ref int frameCounter) {
        if (++frameCounter >= 8) {
            frameCounter = 0;
            frame = ++frame % 3;
        }
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        frameYOffset = Main.tileFrame[type] * AnimationFrameHeight;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        int frameX = Main.tile[i, j].TileFrameX;
        int frameY = Main.tile[i, j].TileFrameY;
        int height = frameY % (18 * 5 + 2) == 54 ? 18 : 16;
        //if (frameY >= 18 * 5 + 2) {
        //    frameY += Main.tileFrame[Type] * (18 * 5 + 2);
        //}
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        Color color = Lighting.GetColor(i, j);
        var t = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(Type, 0, Main.tile[i, j].TileColor);
        if (t == null)
            t = TextureAssets.Tile[Type].Value;
        int frameHeight = 18 * 5 + 2;
        bool isOn = frameY == 0 || frameY > frameHeight;
        spriteBatch.Draw(t, new Vector2(i * 16f, j * 16f) + zero - Main.screenPosition + new Vector2(0f, 2f), new Rectangle(frameX, frameY - (isOn ? frameHeight : 0), 16, height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        if (Main.InSmartCursorHighlightArea(i, j, out var actuallySelected)) {
            int num = (color.R + color.G + color.B) / 3;
            if (num > 10) {
                Texture2D highlightTexture = TextureAssets.HighlightMask[Type].Value;
                Color highlightColor = Colors.GetSelectionGlowColor(actuallySelected, num);
                spriteBatch.Draw(highlightTexture, new Vector2(i * 16f, j * 16f) + zero - Main.screenPosition + new Vector2(0f, 2f),
                    new Rectangle(frameX, frameY - (isOn ? frameHeight : 0), 16, height), highlightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                //Main.spriteBatch.DrawSelf(sourceRectangle: rect, texture: highlightTexture, position: drawPosition, color: highlightColor, _rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
            }
        }

        if (isOn) {
            frameY -= 18 * 5 + 2;
        }
        if (Math.Abs(frameY) == 92) {
            frameY = 0;
        }

        int width = 16;
        int offsetY = 0;
        int height2 = 16;
        short frameX2 = Main.tile[i, j].TileFrameX;
        short frameY2 = Main.tile[i, j].TileFrameY;
        int addFrX = 0;
        int addFrY = 0;
        TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX2, ref frameY2);
        TileLoader.SetAnimationFrame(Type, i, j, ref addFrX, ref addFrY);

        t = ModContent.Request<Texture2D>(Texture + "_Draw").Value;
        spriteBatch.Draw(t, new Vector2(i * 16f, j * 16f) + zero - Main.screenPosition + new Vector2(0f, 2f), new Rectangle(frameX, frameY + addFrY, 16, height),
            Color.White * Main.LocalPlayer.GetModPlayer<EnragedVisuals>()._opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        //Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), DrawColor.Red.ToVector3());

        return false;
    }
}