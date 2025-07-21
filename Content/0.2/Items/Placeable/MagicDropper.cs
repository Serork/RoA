using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

sealed class MagicTarDropper : MagicDropper {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    protected override ushort DrippingTarTileTypeToPlace() => RoA.RoALiquidMod.Find<ModTile>("DrippingTar").Type;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);
    }
}

abstract class MagicDropper : ModItem {
    protected abstract ushort DrippingTarTileTypeToPlace();

    public sealed override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = DrippingTarTileTypeToPlace();
        Item.width = 24;
        Item.height = 24;
        Item.value = Item.sellPrice(0, 0, 0, 40);

        SafeSetDefaults();
    }

    public override bool? UseItem(Player player) {
        int i = Player.tileTargetX, j = Player.tileTargetY;
        Tile tile = WorldGenHelper.GetTileSafely(i, j - 1);
        return tile.HasTile && !tile.BottomSlope;
    }

    protected virtual void SafeSetDefaults() { }
}
