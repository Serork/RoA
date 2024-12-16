using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;


namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodClock : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        //TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Height = 5;
        TileObjectData.newTile.Origin = new Point16(0, 4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(124, 93, 68), Language.GetText("ItemName.GrandfatherClock"));

        AdjTiles = [TileID.GrandfatherClocks];
    }

    public override bool RightClick(int x, int y) {
        TileHelper.PrintTime();
        return true;
    }

    public override void NearbyEffects(int i, int j, bool closer) {
        if (closer) {
            Main.SceneMetrics.HasClock = true;
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
}