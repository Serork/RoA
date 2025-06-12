using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class ExcludeForestPylonFromBuyingSystem : GlobalNPC {
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) {
        if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            Helper.ExcludeFrom(items, out _, ItemID.TeleportationPylonPurity);
        }
    }
}
