using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class Tapper : ModTile {
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileShine2[Type] = true;
		Main.tileShine[Type] = 1500;
		Main.tileNoFail[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorLeft = new AnchorData(Terraria.Enums.AnchorType.Tree, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = AnchorData.Empty;
		//TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TapperEntity>().Hook_AfterPlacement, -1, 0, true);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newAlternate.AnchorRight = new AnchorData(Terraria.Enums.AnchorType.Tree, TileObjectData.newTile.Width, 0);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.AnchorTop = AnchorData.Empty;
		//TileObjectData.newAlternate.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TapperEntity>().Hook_AfterPlacement, -1, 0, true);
		TileObjectData.addAlternate(1);

		TileObjectData.addTile(Type);
	}

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        return false;
    }
}
