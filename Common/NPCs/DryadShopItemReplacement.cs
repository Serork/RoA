using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Placeable.Seeds;
using RoA.Content.Items.Placeable.Walls;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class DryadShopItemReplacement : GlobalNPC {
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) {
        if (npc.type != NPCID.Dryad) {
            return;
        }
        if (items == null) {
            return;
        }

        short[] planterBoxes = [ItemID.BlinkrootPlanterBox, ItemID.CorruptPlanterBox, ItemID.CrimsonPlanterBox, ItemID.DayBloomPlanterBox,
                                ItemID.FireBlossomPlanterBox, ItemID.MoonglowPlanterBox, ItemID.ShiverthornPlanterBox, ItemID.WaterleafPlanterBox];
        Helper.ExcludeFrom(items, out short position, planterBoxes);
        List<short> moddedPlanterBoxes = [];
        for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
            ModItem item = ItemLoader.GetItem(i);
            if (item.FullName.Contains("PlanterBox")) {
                moddedPlanterBoxes.Add((short)item.Type);
            }
        }
        if (moddedPlanterBoxes.Count > 0) {
            Helper.ExcludeFrom(items, out position, [.. moddedPlanterBoxes]);
        }
        Helper.InsertAt(items, (short)ModContent.ItemType<EmptyPlanterBox>(), (short)(position != 0 ? position - 1 : 8));

        if (!Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
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
