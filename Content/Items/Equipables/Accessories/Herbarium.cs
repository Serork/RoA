using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

// also see Hooks.cs
sealed class Herbarium : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Herbarium");
        //Tooltip.SetDefault("Enemies can drop healing herbs when wreath is fully charged");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 3, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<HerbariumPlayer>().healingHerb = true;
    }
}

internal class HerbariumPlayer : ModPlayer {
    public bool healingHerb;

    public override void ResetEffects()
        => healingHerb = false;
}
