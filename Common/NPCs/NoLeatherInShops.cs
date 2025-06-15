using RoA.Content.Items.Placeable.Crafting;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class NoLeatherInShops : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        foreach (NPCShop.Entry entry in shop.ActiveEntries) {
            if (entry.Item.type == ItemID.Leather) {
                entry.Item.SetDefaults(ModContent.ItemType<TanningRack>());
            }
        }
    }
}
