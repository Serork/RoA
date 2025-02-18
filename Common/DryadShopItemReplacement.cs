using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Seeds;
using RoA.Content.Items.Placeable.Walls;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class DryadShopItemReplacement : GlobalNPC {
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) {
        if (npc.type != NPCID.Dryad) {
            return;
        }
        if (!Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            return;
        }
        if (items == null) {
            return;
        }
        for (int i = 0; i < items.Length; i++) {
            if (items[i] is null || items[i].IsAir) {
                continue;
            }
            if (items[i].type == ItemID.GrassSeeds) {
                items[i].SetDefaults(ModContent.ItemType<BackwoodsGrassSeeds>());
            }
            if (items[i].type == ItemID.GrassWall) {
                items[i].SetDefaults(ModContent.ItemType<BackwoodsGrassWall>());
            }
            if (items[i].type == ItemID.FlowerWall) {
                items[i].SetDefaults(ModContent.ItemType<BackwoodsFlowerGrassWall>());
            }
        }
    }
}
