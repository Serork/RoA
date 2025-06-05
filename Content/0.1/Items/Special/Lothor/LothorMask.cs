using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

[AutoloadEquip(EquipType.Head)]
sealed class LothorMask : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Lothor Mask");
        Item.ResearchUnlockCount = 1;

        //ItemGlowMaskHandler.RegisterArmorGlowMask(Item.headSlot, this);
    }

    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;

        Item.vanity = true;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }
}