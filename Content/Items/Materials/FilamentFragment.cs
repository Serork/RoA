using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using RoA.Common.World;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class FilamentFragment : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
        ItemID.Sets.SortingPriorityMaterials[Type] = 98;
    }

    public override Color? GetAlpha(Color lightColor) {
        return new Color(255, 255, 255, 200);
    }

    public override void SetDefaults() {
        Item.width = 20;
        Item.height = 22;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(0, 0, 20);
        Item.rare = 9;
    }

    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, new Color(235, 225, 65).ToVector3() * 0.5f * Main.essScale);
    }
}
