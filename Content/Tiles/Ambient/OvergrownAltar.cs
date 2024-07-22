using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class OvergrownAltar : ModTile {
	public override void SetStaticDefaults () {
		AnimationFrameHeight = 36;

		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = false;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.DrawYOffset = 2;
		//TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<OvergrownAltarTE>().Hook_AfterPlacement, -1, 0, false);
		TileObjectData.addTile(Type);

		//LocalizedText modTranslation = CreateMapEntryName(null);
		//// modTranslation.SetDefault("Overgrown Altar");
		AddMapEntry(new Color(197, 254, 143));
		DustType = 59;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

	public override bool CanExplode(int i, int j) => false;

	public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
	
	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}
