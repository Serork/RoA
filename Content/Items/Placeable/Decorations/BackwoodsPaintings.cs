using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class Him : Painting {
    protected override Point16 Size() => new(50, 34);
    protected override ushort PaintingTileTypeToPlace() => (ushort)ModContent.TileType<Tiles.Decorations.Him>();
}

sealed class NightsShroud : Painting {
    protected override Point16 Size() => new(28, 28);
    protected override ushort PaintingTileTypeToPlace() => (ushort)ModContent.TileType<Tiles.Decorations.NightsShroud>();
}

sealed class Her : Painting {
    protected override Point16 Size() => new(20, 20);
    protected override ushort PaintingTileTypeToPlace() => (ushort)ModContent.TileType<Tiles.Decorations.Her>();
}

sealed class FourPixels : Painting {
    protected override Point16 Size() => new(38, 28);
    protected override ushort PaintingTileTypeToPlace() => (ushort)ModContent.TileType<Tiles.Decorations.FourPixels>();
}

abstract class Painting : ModItem {
    protected abstract ushort PaintingTileTypeToPlace();
    protected abstract Point16 Size();

    public sealed override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public sealed override void SetDefaults() {
        Item.Size = Size().ToVector2();

        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;

        Item.createTile = PaintingTileTypeToPlace();
        Item.value = Item.sellPrice(0, 0, 20, 0);
    }
}