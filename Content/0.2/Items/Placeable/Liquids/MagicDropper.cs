using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Liquids;

sealed class MagicTarDropper : MagicDropper {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    protected override ushort DrippingTarTileTypeToPlace() => RoA.RoALiquidMod.Find<ModTile>("DrippingTar").Type;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);
    }
}

abstract class MagicDropper : ModItem {
    public override void Load() {
        On_Player.PlaceThing_Tiles += On_Player_PlaceThing_Tiles;
        On_PlayerDrawSet.CreateCompositeData += On_PlayerDrawSet_CreateCompositeData;
    }

    private void On_PlayerDrawSet_CreateCompositeData(On_PlayerDrawSet.orig_CreateCompositeData orig, ref PlayerDrawSet self) {
        if (self.heldItem.IsModded(out ModItem modItem) && modItem is MagicDropper) {
            self.itemEffect ^= SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
        }

        orig(ref self);
    }

    private void On_Player_PlaceThing_Tiles(On_Player.orig_PlaceThing_Tiles orig, Player self) {
        Item item = self.inventory[self.selectedItem];
        int tileToCreate = item.createTile;
        if (tileToCreate < 0 || !(self.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost - (float)self.blockRange <= (float)Player.tileTargetX) || !((self.position.X + (float)self.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f + (float)self.blockRange >= (float)Player.tileTargetX) || !(self.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost - (float)self.blockRange <= (float)Player.tileTargetY) || !((self.position.Y + (float)self.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f + (float)self.blockRange >= (float)Player.tileTargetY))
            return;
        if (item.IsModded(out ModItem modItem) && modItem is MagicDropper) {
            int i = Player.tileTargetX, j = Player.tileTargetY;
            Tile tile = WorldGenHelper.GetTileSafely(i, j - 1);
            if (!(tile.HasTile && !tile.BottomSlope)) {
                return;
            }
        }

        orig(self);
    }

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
