//using Microsoft.Xna.Framework;

//using RoA.Core;

//using Terraria;
//using Terraria.GameContent;
//using Terraria.ModLoader;

//namespace RoA.Common.Lavas;

//sealed class Permafrost : LavaStyle {
//	public override int ChooseWaterfallStyle() => 0;

//	public override int GetSplashDust() => 0;

//	public override int GetDropletGore() => 0;

//	public override bool ChooseLavaStyle() => true;

//	public override bool SafeAutoload(ref string name, ref string texture) {
//		texture = ResourceManager.WaterTextures + "Permafrost";
//		return true;
//	}

//	public override bool DrawEffects(int x, int y) {
//		Tile tile = Framing.GetTileSafely(x, y - 1);

//		//if (Main.rand.NextBool(45) && tile.LiquidAmount == 0 && tile.BlockType != Terraria.ID.BlockType.Solid)
//		//	Dust.NewDustPerfect(new Vector2(x, y + 1) * 16, ModContent.DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(2, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.2f, 0.3f));

//		if (tile.LiquidAmount > 0)
//			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x + Main.offScreenRange - (int)Main.screenPosition.X, y + Main.offScreenRange - (int)Main.screenPosition.Y, 16, 16), new Color(255, 175, 0) * GetOpacity(x, y));

//		return true;
//	}

//	public override void DrawBlockEffects(int x, int y, Tile up, Tile left, Tile right, Tile down) {
//		float opacity = 0;

//		if (!left.HasTile && left.LiquidAmount > 0)
//			opacity = GetOpacity(x - 1, y);
//		else if (!right.HasTile && right.LiquidAmount > 0)
//			opacity = GetOpacity(x + 1, y);
//		else if (!up.HasTile && up.LiquidAmount > 0)
//			opacity = GetOpacity(x, y - 1);

//		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x + Main.offScreenRange - (int)Main.screenPosition.X, y + Main.offScreenRange - (int)Main.screenPosition.Y, 16, 16), new Color(255, 175, 0) * opacity);
//	}

//	public override ushort SplashDustType() => (ushort)ModContent.DustType<Content.Dusts.Permafrost>();

//    private float GetOpacity(int x, int y) {
//		float opacity = 0;

//		int up = 0;

//		while (Framing.GetTileSafely(x, y - up).LiquidAmount > 0 && opacity <= 0.5f) {
//			opacity += 0.075f;
//			up++;
//		}

//		return opacity;
//	}
//}