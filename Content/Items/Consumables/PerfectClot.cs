using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.LiquidsSpecific;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class PerfectClot : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void SetDefaults() {
        Item.useStyle = 4;
        Item.consumable = true;
        Item.useAnimation = 45;
        Item.useTime = 45;
        Item.UseSound = SoundID.Item92;
        Item.width = 18;
        Item.height = 34;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 2;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed) {
        Item.velocity.Y = MathF.Abs(Item.velocity.Y) * -1f;
        Item.velocity.X *= 0.925f;

        if (!Item.lavaWet) {
            return;
        }

        if (Item.playerIndexTheItemIsReservedFor == Main.myPlayer) {
            // TODO: net test
            if (Helper.SinglePlayerOrServer) {
                int x = (int)Item.Bottom.X / 16, y = (int)Item.Bottom.Y / 16;
                Tile tile = Main.tile[x - 1, y];
                Tile tile2 = Main.tile[x + 1, y];
                Tile tile3 = Main.tile[x, y - 1];
                Tile tile4 = Main.tile[x, y + 1];
                Tile tile5 = Main.tile[x, y];
                tile.LiquidAmount = tile2.LiquidAmount = tile3.LiquidAmount = 0;
                Projectile.NewProjectile(null, new Point16(x, y).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<TarExplosion>(), 500, 0f, Main.myPlayer);
                WorldGen.SquareTileFrame(x, y);
            }

            Item.active = false;
            Item.type = 0;
            Item.stack = 0;
            if (Main.netMode != NetmodeID.SinglePlayer) {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI);
            }
        }
    }

    public override bool? UseItem(Player player) {
        ref bool activated = ref player.GetCommon().PerfectClotActivated;
        if (player.ItemAnimationJustStarted && !activated) {
            activated = true;

            Item.stack--;
            if (Item.stack <= 0) {
                Item.active = false;
                Item.TurnToAir();
            }
        }

        return base.UseItem(player);
    }
}
