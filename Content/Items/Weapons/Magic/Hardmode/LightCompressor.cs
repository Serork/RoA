using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic.Hardmode;

sealed class LightCompressor : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(64, 26);
        Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 17));
        Item.DefaultToMagicWeapon(876, 20, 15f);
        Item.mana = 20;
        Item.knockBack = 6f;
        Item.damage = 42;
        Item.UseSound = SoundID.Item158;
    }
}
