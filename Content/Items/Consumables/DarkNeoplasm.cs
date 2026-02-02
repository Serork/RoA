using Microsoft.Xna.Framework;

using RoA.Common.Items;
using RoA.Content.Projectiles.LiquidsSpecific;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

class DarkNeoplasm : ModItem {
    public override void SetDefaults() {
        Item.consumable = true;
        Item.width = 26;
        Item.height = 30;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 2;
    }

    public override void PostUpdate() {
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
                Projectile.NewProjectile(null, new Point16(x, y).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<TarExplosion>(), 250, 0f, Main.myPlayer, ai2: 1f);
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

    public virtual ItemCommon.TarEnchantmentStat GetAppliedEnchantment() {
        //UnifiedRandom random = Main.rand;
        //ushort[] hpOptions = [5, 10, 20];
        //ushort hp = random.NextFromList(hpOptions);
        //float[] damageModifierOptions = [1.04f, 1.08f, 1.1f];
        //float damageModifier = random.NextFromList(damageModifierOptions);
        //ushort[] defenseOptions = [1, 2, 4];
        //ushort defense = random.NextFromList(defenseOptions);
        //bool shouldGiveHP = Main.rand.NextBool();
        //bool shouldGiveDamage = Main.rand.NextBool();
        //bool shouldGiveDefense = Main.rand.NextBool();
        //while (!shouldGiveHP && !shouldGiveDamage && !shouldGiveDefense) {
        //    shouldGiveHP = Main.rand.NextBool();
        //    shouldGiveDamage = Main.rand.NextBool();
        //    shouldGiveDefense = Main.rand.NextBool();
        //}
        //ItemCommon.TarEnchantmentStat tarEnchantmentStat = new(HP: (ushort)(shouldGiveHP ? hp : 0), DamageModifier: shouldGiveDamage ? damageModifier : 1f, Defense: (ushort)(shouldGiveDefense ? defense : 0));
        ItemCommon.TarEnchantmentStat tarEnchantmentStat = new();
        return tarEnchantmentStat;
    }
}
