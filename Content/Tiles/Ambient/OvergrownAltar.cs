using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class OvergrownAltar : ModTile {
	public override void SetStaticDefaults () {
		AnimationFrameHeight = 36;

		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<OvergrownAltarTE>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.addTile(Type);

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;

        AddMapEntry(new Color(197, 254, 143), CreateMapEntryName());

		DustType = 59;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

	public override bool CanExplode(int i, int j) => false;

	public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<OvergrownAltarTE>().Place(i, j);

    public override void KillMultiTile(int i, int j, int frameX, int frameY) => ModContent.GetInstance<OvergrownAltarTE>().Kill(i, j);

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        if (!NPC.downedBoss2) {
            return;
        }

        if (!IsValid(i, j)) {
            return;
        }

        OvergrownAltarTE overgrownAltarTE = TileHelper.GetTE<OvergrownAltarTE>(i, j);
        if (overgrownAltarTE != null) {
            float counting = overgrownAltarTE.Counting2 * 0.95f;
            float altarFactor = AltarHandler.GetAltarFactor();
            float value = 0.5f + 0.3f * altarFactor;
            value += (1f - MathHelper.Clamp(counting, 0f, 0.98f)) * (0.5f + 0.35f * altarFactor);
            bool flag = false;
            float altarStrength = AltarHandler.GetAltarStrength();
            float mult = flag ? 1f : Helper.EaseInOut3(MathHelper.Clamp(altarStrength * 2f, 0f, 1f));
            float r2 = MathHelper.Lerp(0.45f, 0.9f, mult);
            float g2 = MathHelper.Lerp(0.85f, 0.2f, mult);
            float b2 = MathHelper.Lerp(0.4f, 0.3f, mult);
            float altarStrength2 = altarStrength * 1.5f;
            value *= Math.Max(0.75f, 1f - (altarStrength2 > 0.5f ? 1f - altarStrength2 : altarStrength2));
            r = r2 * value;
            g = g2 * value;
            b = b2 * value;
        }
    }

    private bool IsValid(int i, int j, bool onlyOne = false) {
        bool flag = false;
        if (WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(Type) && WorldGenHelper.GetTileSafely(i + 1, j).ActiveTile(Type) && ((onlyOne && !WorldGenHelper.GetTileSafely(i, j - 1).ActiveTile(Type)) || !onlyOne)) {
            flag = true;
        }

        return flag;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        OvergrownAltarTE overgrownAltarTE = TileHelper.GetTE<OvergrownAltarTE>(i, j);
        if (overgrownAltarTE != null) {
            float counting = MathHelper.Clamp(overgrownAltarTE.Counting, 0f, 0.98f);
            //float value = (double)counting < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)counting) : 1f;
            float factor = counting;
            float strength = AltarHandler.GetAltarStrength();
            Color color = Lighting.GetColor(i, j);
            Tile tile = Main.tile[i, j];
            bool flag = false;
            int frame = /*5 - */(int)(factor * 6) + (flag || strength > 0.3f ? 6 : 0);
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen) {
                zero = Vector2.Zero;
            }
            Texture2D texture = TextureAssets.Tile[Type].Value;
            Rectangle rectangle = new(tile.TileFrameX, !NPC.downedBoss2 ? tile.TileFrameY + 36 * 2 : tile.TileFrameY + 36 * frame, 16, 16);
            Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
            spriteBatch.Draw(texture, position, rectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (!NPC.downedBoss2) {
                return false;
            }
            
            if (!IsValid(i, j)) {
                return false;
            }

            texture = ModContent.Request<Texture2D>(ResourceManager.TilesTextures + "OvergrownAltar_Glow").Value;
            Color color2 = new(255, 255, 200, 200);
            float mult = Helper.EaseInOut3(strength);
            float factor3 = AltarHandler.GetAltarFactor();
            spriteBatch.Draw(texture, position, rectangle, color2 * MathHelper.Lerp(0f, 1f, factor3), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.PiOver2) {
                spriteBatch.Draw(texture, position + Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly, new Vector2()) * Helper.Wave(0f, 1.5f, speed: factor3), rectangle, (color2 * factor3).MultiplyAlpha(MathHelper.Lerp(0f, 1f, factor3)).MultiplyAlpha(0.35f).MultiplyAlpha(Helper.Wave(0.25f, 0.75f, speed: factor3)) * factor3, Main.rand.NextFloatRange(0.1f * factor3), Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            float factor2 = mult;
            //bool flag3 = Lothor.ShouldLothorBeDead();
            if (factor2 > 0f/* || flag3*/) {
                //float factor4 = Math.Max(0.1f, (double)counting < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)counting) : 1f);
                //factor3 = (factor4 > 0.5f ? 1f - factor4 : factor4) + 0.5f;
                factor3 = /*flag3 ? OvergrownCoords.Strength : */1f;
                spriteBatch.Draw(texture, position, rectangle, color2 * factor2 * MathHelper.Lerp(0f, 1f, factor3), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.Pi) {
                    spriteBatch.Draw(texture, position + Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly, new Vector2()) * Helper.Wave(0f, 1.5f, speed: factor3), rectangle, (color2 * factor3).MultiplyAlpha(MathHelper.Lerp(0f, 1f, factor3)).MultiplyAlpha(0.35f).MultiplyAlpha(Helper.Wave(0.25f, 0.75f, speed: factor3)) * factor3 * factor2, Main.rand.NextFloatRange(0.1f * factor3), Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        return false;
    }
}
