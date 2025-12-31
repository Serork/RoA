using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ScholarsArchive : ModTile, TileHooks.IPreDraw {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        TileObjectData.addTile(Type);
        AdjTiles = [TileID.Bookcases];
        AddMapEntry(new Color(213, 189, 185), CreateMapEntryName());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Miscellaneous.ScholarsArchive>());
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        TileHelper.AddPostNonSolidTileDrawPoint(this, i, j);

        return base.PreDraw(i, j, spriteBatch);
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {

    }

    void TileHooks.IPreDraw.PreDrawExtra(SpriteBatch spriteBatch, Point16 tilePosition) {
        int i = tilePosition.X,
            j = tilePosition.Y;
        Texture2D glowTexture = _glowTexture.Value;
        bool flag = false;
        int type = ModContent.TileType<ScholarsArchive>();
        if (Main.tile[i, j].TileType == type &&
            Main.tile[i + 1, j].TileType == type &&
            Main.tile[i - 1, j].TileType == type &&
            Main.tile[i, j - 1].TileType != type) {
            flag = true;
        }
        if (flag) {
            float waveFrequency = 10f;
            float offset = i * j;

            Rectangle clip = Utils.Frame(glowTexture, 1, 3, frameY: 1);
            Vector2 origin = clip.Centered();
            SpriteBatch batch = Main.spriteBatch;
            Vector2 position = new Point16(i, j).ToWorldCoordinates() - Vector2.UnitY * 6f;         
            float rotation = (float)(Main.timeForVisualEffects * 0.1 + offset) * 0.05f;
            Color lightingColor = Lighting.GetColor(position.ToTileCoordinates());
            lightingColor = Color.Lerp(lightingColor, Color.White, 0.1f);
            Color color = Color.Lerp(Color.SkyBlue, Color.Lerp(Color.SkyBlue, Color.Blue, 0.1f), Helper.Wave(0f, 1f, waveFrequency, offset)).ModifyRGB(Helper.Wave(0.9f, 1.1f, waveFrequency, offset));
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Color = color.MultiplyRGB(lightingColor),
                Rotation = rotation
            };
            batch.Draw(glowTexture, position, drawInfo);

            color = Color.Lerp(Color.SkyBlue, Color.Lerp(Color.SkyBlue, Color.Blue, 0.1f), Helper.Wave(0f, 1f, waveFrequency, offset - 2f)).ModifyRGB(Helper.Wave(0.8f, 1.1f, waveFrequency, offset + 2f));
            clip = Utils.Frame(glowTexture, 1, 3, frameY: 0);
            origin = clip.Centered();
            rotation = (float)(Main.timeForVisualEffects * 0.5 + offset) * 0.05f;
            drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Color = color.MultiplyRGB(lightingColor),
                Rotation = rotation
            };
            batch.Draw(glowTexture, position, drawInfo);

            color = Color.Lerp(Color.SkyBlue, Color.Lerp(Color.SkyBlue, Color.Blue, 0.1f), Helper.Wave(0f, 1f, waveFrequency, offset - 4f)).ModifyRGB(Helper.Wave(1f, 1.1f, waveFrequency, offset + 4f));
            clip = Utils.Frame(glowTexture, 1, 3, frameY: 2);
            origin = clip.Centered();
            rotation = (float)(-Main.timeForVisualEffects * 1 + offset * 2) * 0.05f;
            drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Color = color.MultiplyRGB(lightingColor),
                Rotation = rotation
            };
            batch.Draw(glowTexture, position, drawInfo);
        }
    }
}
