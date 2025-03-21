using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special;

abstract class MagicSphere : ModItem {
    protected virtual Color? LightingColor { get; } = null;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<FlederStaff>();
    }

    public override void SetDefaults() {
        int width = 24, height = width;
        Item.Size = new Vector2(width, height);
        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 2, 25, 0);
    }

    public override void PostUpdate() {
        if (!Main.dedServ && LightingColor != null) {
            Lighting.AddLight(Item.getRect().TopRight(), LightingColor.Value.ToVector3() * 0.75f);
        }
    }
}

sealed class SphereOfAspiration : MagicSphere { }
