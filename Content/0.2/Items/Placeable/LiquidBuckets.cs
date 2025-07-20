using Newtonsoft.Json.Linq;

using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

//sealed class PermafrostAbsorbantSponge : AbsorbantSponge {
//    protected override byte LiquidIDToSoakUp() => 4;
//}

sealed class TarAbsorbantSponge : AbsorbantSponge {
    protected override byte LiquidIDToSoakUp() => 5;

    protected override void SafeSetDefaults2() {
        Item.SetSizeValues(30, 26);
    }
}

sealed class TarBucket : LiquidBucket {
    protected override byte LiquidIDToSoakUp() => 5;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(24, 22);
    }
}

sealed class BottomlessTarBucket : LiquidBucket {
    protected override byte LiquidIDToSoakUp() => 5;

    protected override bool IsEndless() => true;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 28);
    }
}

//sealed class PermafrostBucket : LiquidBucket {
//    protected override byte LiquidIDToSoakUp() => 4;
//}

abstract class AbsorbantSponge : LiquidBucket {
    protected sealed override bool IsEndless() => true;

    protected sealed override void SafeSetDefaults() {
        Item.width = 20;
        Item.height = 20;

        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 12;
        Item.useTime = 5;
        Item.autoReuse = true;
        Item.rare = 7;
        Item.value = Item.sellPrice(0, 10);
        Item.tileBoost += 2;

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults2() { }
}

abstract class LiquidBucket : ModItem {
    protected abstract byte LiquidIDToSoakUp();

    protected virtual bool IsEndless() => false;

    public override void Load() {
        On_Item.CanCombineStackInWorld += On_Item_CanCombineStackInWorld;
        On_Player.ItemCheck_UseBuckets += On_Player_ItemCheck_UseBuckets;
    }

    private void On_Player_ItemCheck_UseBuckets(On_Player.orig_ItemCheck_UseBuckets orig, Player self, Item sItem) {
        bool isEmptyBucket = sItem.type == ItemID.EmptyBucket;
        if (!sItem.Is<LiquidBucket>() && !isEmptyBucket) {
            orig(self, sItem);
            return;
        }

        if (self.noBuilding || !(self.position.X / 16f - (float)Player.tileRangeX - (float)sItem.tileBoost <= (float)Player.tileTargetX) || !((self.position.X + (float)self.width) / 16f + (float)Player.tileRangeX + (float)sItem.tileBoost - 1f >= (float)Player.tileTargetX) || !(self.position.Y / 16f - (float)Player.tileRangeY - (float)sItem.tileBoost <= (float)Player.tileTargetY) || !((self.position.Y + (float)self.height) / 16f + (float)Player.tileRangeY + (float)sItem.tileBoost - 2f >= (float)Player.tileTargetY)) {
            return;
        }

        if (!Main.GamepadDisableCursorItemIcon) {
            self.cursorItemIconEnabled = true;
            Main.ItemIconCacheUpdate(sItem.type);
        }

        if (!self.ItemTimeIsZero || self.itemAnimation <= 0 || !self.controlUseItem)
            return;

        int x = Player.tileTargetX;
        int y = Player.tileTargetY;
        if (isEmptyBucket) {
            if (sItem.type == ItemID.EmptyBucket) {
                int num = Main.tile[x, y].LiquidType;
                int num2 = 0;
                for (int i = x - 1; i <= x + 1; i++) {
                    for (int j = y - 1; j <= y + 1; j++) {
                        if (Main.tile[i, j].LiquidType == num)
                            num2 += Main.tile[i, j].LiquidAmount;
                    }
                }

                if (Main.tile[x, y].LiquidAmount <= 0 || num2 <= 100)
                    return;

                int liquidType = Main.tile[x, y].LiquidType;
                if (TileHelper.IsPermafrost(x, y)) {
                    //sItem.stack--;
                    //self.PutItemInInventoryFromItemUsage(ModContent.ItemType<PermafrostBucket>(), self.selectedItem);
                }
                else if (TileHelper.IsTar(x, y)) {
                    sItem.stack--;
                    self.PutItemInInventoryFromItemUsage(ModContent.ItemType<TarBucket>(), self.selectedItem);
                }
                else if (TileHelper.IsHoney(x, y)) {
                    sItem.stack--;
                    self.PutItemInInventoryFromItemUsage(1128, self.selectedItem);
                }
                else if (TileHelper.IsLava(x, y)) {
                    sItem.stack--;
                    self.PutItemInInventoryFromItemUsage(207, self.selectedItem);
                }
                else {
                    if (TileHelper.IsShimmer(x, y))
                        return;

                    sItem.stack--;
                    self.PutItemInInventoryFromItemUsage(206, self.selectedItem);
                }

                SoundEngine.PlaySound(SoundID.Splash, self.position);
                self.ApplyItemTime(sItem);
                int num3 = Main.tile[x, y].LiquidAmount;
                Main.tile[x, y].LiquidAmount = 0;
                Tile tile = Main.tile[x, y];
                tile.LiquidType = 0;
                WorldGen.SquareTileFrame(x, y, resetFrame: false);
                if (Main.netMode == 1)
                    NetMessage.sendWater(x, y);
                else
                    Liquid.AddWater(x, y);

                if (num3 >= 255)
                    return;

                for (int k = x - 1; k <= x + 1; k++) {
                    for (int l = y - 1; l <= y + 1; l++) {
                        if ((k != x || l != y) && Main.tile[k, l].LiquidAmount > 0 && Main.tile[k, l].LiquidType == num) {
                            int num4 = Main.tile[k, l].LiquidAmount;
                            if (num4 + num3 > 255)
                                num4 = 255 - num3;

                            num3 += num4;
                            Tile tile2 = Main.tile[k, l];
                            tile2.LiquidAmount -= (byte)num4;
                            tile2.LiquidType = liquidType;
                            if (tile2.LiquidAmount == 0) {
                                tile2.LiquidType = 0;
                            }

                            WorldGen.SquareTileFrame(k, l, resetFrame: false);
                            if (Main.netMode == 1)
                                NetMessage.sendWater(k, l);
                            else
                                Liquid.AddWater(k, l);
                        }
                    }
                }
            }
        }
        else {
            LiquidBucket sLiquidBucketItem = sItem.As<LiquidBucket>();
            byte bucketLiquidType = sLiquidBucketItem.LiquidIDToSoakUp();
            bool isHoveringNeededLiquid = bucketLiquidType == 4 ? TileHelper.IsPermafrost(x, y) : TileHelper.IsTar(x, y);
            bool isAbsorbantSponge = sItem.Is<AbsorbantSponge>();
            if ((isAbsorbantSponge && isHoveringNeededLiquid) || sItem.type == 5304) {
                int num = Main.tile[x, y].LiquidType;
                int num2 = 0;
                for (int i = x - 1; i <= x + 1; i++) {
                    for (int j = y - 1; j <= y + 1; j++) {
                        if (Main.tile[i, j].LiquidType == num)
                            num2 += Main.tile[i, j].LiquidAmount;
                    }
                }

                if (Main.tile[x, y].LiquidAmount <= 0)
                    return;

                int liquidType = Main.tile[x, y].LiquidType;

                SoundEngine.PlaySound(SoundID.Splash, self.position);
                self.ApplyItemTime(sItem);
                int num3 = Main.tile[x, y].LiquidAmount;
                Main.tile[x, y].LiquidAmount = 0;
                Tile tile = Main.tile[x, y];
                tile.LiquidType = 0;
                WorldGen.SquareTileFrame(x, y, resetFrame: false);
                if (Main.netMode == 1)
                    NetMessage.sendWater(x, y);
                else
                    Liquid.AddWater(x, y);

                if (num3 >= 255)
                    return;

                for (int k = x - 1; k <= x + 1; k++) {
                    for (int l = y - 1; l <= y + 1; l++) {
                        if ((k != x || l != y) && Main.tile[k, l].LiquidAmount > 0 && Main.tile[k, l].LiquidType == num) {
                            int num4 = Main.tile[k, l].LiquidAmount;
                            if (num4 + num3 > 255)
                                num4 = 255 - num3;

                            num3 += num4;
                            Tile tile2 = Main.tile[k, l];
                            tile2.LiquidAmount -= (byte)num4;
                            tile2.LiquidType = liquidType;
                            if (tile2.LiquidAmount == 0) {
                                tile2.LiquidType = 0;
                            }

                            WorldGen.SquareTileFrame(k, l, resetFrame: false);
                            if (Main.netMode == 1)
                                NetMessage.sendWater(k, l);
                            else
                                Liquid.AddWater(k, l);
                        }
                    }
                }
            }
            else {
                if (Main.tile[x, y].LiquidAmount >= 200 || (Main.tile[x, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x, y].TileType] && !Main.tileSolidTop[Main.tile[x, y].TileType] && Main.tile[x, y].TileType != TileID.Grate))
                    return;

                if (!isAbsorbantSponge) {
                    if (Main.tile[x, y].LiquidAmount == 0 || Main.tile[x, y].LiquidType == bucketLiquidType) {
                        SoundEngine.PlaySound(SoundID.Splash, self.position);
                        Tile tile = Main.tile[x, y];
                        tile.LiquidType = bucketLiquidType;
                        tile.LiquidAmount = byte.MaxValue;
                        WorldGen.SquareTileFrame(x, y);

                        if (!sLiquidBucketItem.IsEndless()) {
                            sItem.stack--;
                            self.PutItemInInventoryFromItemUsage(205, self.selectedItem);
                        }

                        self.ApplyItemTime(sItem);
                        if (Main.netMode == 1)
                            NetMessage.sendWater(x, y);
                    }
                }
            }
        }
    }

    private bool On_Item_CanCombineStackInWorld(On_Item.orig_CanCombineStackInWorld orig, Item self) {
        int num = self.type;
        if (num == 75)
            return false;

        if (self.createTile < 0 && self.createWall <= 0 && (self.ammo <= 0 || self.notAmmo) && !self.consumable &&
            (self.type < ItemID.EmptyBucket || self.type > ItemID.LavaBucket) &&
            self.type != ItemID.HoneyBucket && !self.Is<LiquidBucket>() &&
            self.type != 530 && self.dye <= 0 && !self.PaintOrCoating)
            return self.material;

        return true;
    }

    public sealed override void SetDefaults() {
        Item.SetSizeValues(24, 22);

        if (IsEndless()) {
            Item.useStyle = 1;
            Item.useTurn = true;
            Item.useAnimation = 12;
            Item.useTime = 5;
            Item.autoReuse = true;
            Item.rare = 7;
            Item.value = Item.sellPrice(0, 10);
            Item.tileBoost += 2;
        }
        else {
            Item.useStyle = 1;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = Item.CommonMaxStack;
            Item.autoReuse = true;
        }

        SafeSetDefaults();
    }

    protected virtual void SafeSetDefaults() { }
}
