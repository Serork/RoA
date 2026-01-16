using Microsoft.Xna.Framework;

using RoA.Content.Items.Consumables;
using RoA.Content.Tiles.Ambient;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class ClumpExtractor : ModItem {
    private static bool _collected, _collected2;

    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useAnimation = Item.useTime = 60;
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 38;
        Item.height = 36;
        Item.consumable = true;

        Item.SetShootableValues(0, 1f);
    }

    public override Vector2? HoldoutOffset() => new Vector2(0f, 0f);

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

    public override bool ConsumeItem(Player player) {
        return false;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        float attackProgress = 1f - player.itemAnimation / (float)player.itemAnimationMax;

        player.SetCompositeBothArms(player.itemRotation - MathHelper.PiOver2 * player.direction, player.GetCommon().TempStretchAmount);

        if (attackProgress > 0.5f && _collected2) {
            if (attackProgress >= 0.9f && _collected) {
                void dropItem(ushort itemType) {
                    int item = Item.NewItem(player.GetSource_Misc("darkneoplasm"), (int)player.position.X, (int)player.position.Y, player.width, player.height, itemType, 1, false, 0, false, false);
                    if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0) {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
                    }
                }

                dropItem((ushort)ModContent.ItemType<DarkNeoplasm>());

                _collected = false;
            }

            return;
        }

        _collected2 = false;

        Vector2 to = player.GetViableMousePosition();
        Vector2 direction = to.DirectionFrom(player.GetPlayerCorePoint());
        float rotation = to.AngleFrom(player.GetPlayerCorePoint());
        if (!player.FacedRight()) {
            rotation -= MathHelper.Pi;
        }
        player.itemRotation = rotation;
        Vector2 position = player.GetPlayerCorePoint(false) + player.MovementOffset();
        float progress1 = Utils.GetLerpValue(0f, 0.175f, attackProgress, true);
        progress1 = Ease.CircOut(progress1);
        int direction2 = direction.X.GetDirection();
        float offsetValue = -direction2 * Item.width / 2;
        Vector2 position2 = position + Vector2.UnitX.RotatedBy(rotation) * offsetValue * 0.4f;
        position = Vector2.Lerp(position, position2, progress1);
        float progress2 = Utils.GetLerpValue(0.175f, 0.5f, attackProgress, true);
        if (progress2 <= 0f) {
            player.GetCommon().TempStretchAmount = Player.CompositeArmStretchAmount.None;
        }
        else {
            player.GetCommon().TempStretchAmount = Player.CompositeArmStretchAmount.Full;

            if (progress2 >= 0.25f && !player.GetCommon().ItemUsed) {
                bool collect() {
                    Vector2 checkPosition = player.GetPlayerCorePoint(false);
                    checkPosition += checkPosition.DirectionTo(to) * Item.width * 1.25f;

                    Point16 checkPositionInTiles = checkPosition.ToTileCoordinates16();
                    int check = 1;
                    for (int i = -check; i <= check; i++) {
                        for (int j = -check; j <= check; j++) {
                            Point16 tilePosition = new(checkPositionInTiles.X + i, checkPositionInTiles.Y + j);
                            Tile tile = WorldGenHelper.GetTileSafely(tilePosition.X, tilePosition.Y);
                            if (!tile.HasTile) {
                                continue;
                            }
                            ushort swellingTarTileType = (ushort)ModContent.TileType<SwellingTar>();
                            if (tile.TileType != swellingTarTileType) {
                                continue;
                            }

                            //SoundEngine.PlaySound(SoundID.Item112.WithPitchOffset(-0.1f), tilePosition.ToWorldCoordinates());

                            Point16 topLeft = TileHelper.GetTileTopLeft2(tilePosition.X, tilePosition.Y, swellingTarTileType);
                            SwellingTarTE? swellingTar = TileHelper.GetTE<SwellingTarTE>(topLeft.X, topLeft.Y);
                            if (swellingTar is not null && swellingTar.IsReady) {
                                swellingTar.Collect(player);

                                return true;
                            }
                        }
                    }

                    return false;
                }

                if (collect()) {
                    Item item = player.GetSelectedItem();
                    if (--item.stack <= 0) {
                        item.TurnToAir();
                    }

                    _collected = true;
                    _collected2 = true;

                    player.GetCommon().ItemUsed = true;
                }
            }
        }
        progress2 = Ease.CircOut(progress2);
        Vector2 position3 = position - Vector2.UnitX.RotatedBy(rotation) * offsetValue * 0.6f;
        position = Vector2.Lerp(position, position3, progress2);
        player.itemLocation = position;
        player.ChangeDir(direction2);
    }
}
