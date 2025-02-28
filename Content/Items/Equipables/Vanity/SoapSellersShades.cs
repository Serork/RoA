using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class SoapSellersShades : ModItem {
    public override void Load() {
        On_Item.GetColor += On_Item_GetColor;
    }

    private Color On_Item_GetColor(On_Item.orig_GetColor orig, Item self, Color newColor) {
        if (self.type == ItemID.FamiliarWig || self.type == ItemID.FamiliarShirt || self.type == ItemID.FamiliarPants) {
            int num = self.color.R - (255 - newColor.R);
            int num2 = self.color.G - (255 - newColor.G);
            int num3 = self.color.B - (255 - newColor.B);
            int num4 = self.color.A - (255 - newColor.A);
            if (num < 0)
                num = 0;

            if (num > 255)
                num = 255;

            if (num2 < 0)
                num2 = 0;

            if (num2 > 255)
                num2 = 255;

            if (num3 < 0)
                num3 = 0;

            if (num3 > 255)
                num3 = 255;

            if (num4 < 0)
                num4 = 0;

            if (num4 > 255)
                num4 = 255;

            return new Color(num, num2, num3, num4) * (1f - self.shimmerTime);
        }

        return orig(self, newColor);
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;

        ItemID.Sets.ShimmerTransformToItem[ItemID.FamiliarWig] = Type;
    }

    public override void SetDefaults() {
        int width = 20; int height = 10;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;

        Item.vanity = true;

        Item.value = Item.sellPrice(0, 0, 25, 0);
    }
}