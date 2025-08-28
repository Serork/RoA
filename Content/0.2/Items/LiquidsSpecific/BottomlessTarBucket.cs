using ModLiquidLib.ModLoader;

using RoA.Content.Liquids;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.LiquidsSpecific;

public class BottomlessTarBucket : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.AlsoABuildingItem[Type] = true;
        ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<TarAbsorbantSponge>();

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 12;
        Item.useTime = 5;
        Item.width = 30;
        Item.height = 28;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(0, 10);
        Item.tileBoost += 2;
    }

    public override void HoldItem(Player player) {
        if (!player.JustDroppedAnItem) {
            if (player.whoAmI != Main.myPlayer) {
                return;
            }
            if (player.noBuilding ||
                !(player.position.X / 16f - (float)Player.tileRangeX - (float)Item.tileBoost <= (float)Player.tileTargetX) ||
                !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)Item.tileBoost - 1f >= (float)Player.tileTargetX) ||
                !(player.position.Y / 16f - (float)Player.tileRangeY - (float)Item.tileBoost <= (float)Player.tileTargetY) ||
                !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)Item.tileBoost - 2f >= (float)Player.tileTargetY)) {
                return;
            }

            if (!Main.GamepadDisableCursorItemIcon) {
                player.cursorItemIconEnabled = true;
                Main.ItemIconCacheUpdate(Item.type);
            }
            if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem) {
                return;
            }
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (tile.LiquidAmount >= 200) {
                return;
            }
            if (tile.HasUnactuatedTile) {
                bool[] tileSolid = Main.tileSolid;
                if (tileSolid[tile.TileType]) {
                    bool[] tileSolidTop = Main.tileSolidTop;
                    if (!tileSolidTop[tile.TileType]) {
                        if (tile.TileType != TileID.Grate) {
                            return;
                        }
                    }
                }
            }

            if (tile.LiquidAmount != 0) {
                if (tile.LiquidType != LiquidLoader.LiquidType<Tar>()) {
                    return;
                }
            }
            SoundEngine.PlaySound(SoundID.SplashWeak, player.position);
            tile.LiquidType = LiquidLoader.LiquidType<Tar>();
            tile.LiquidAmount = byte.MaxValue;
            WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY);
            player.ApplyItemTime(Item);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.sendWater(Player.tileTargetX, Player.tileTargetY);
            }
        }
    }
}
