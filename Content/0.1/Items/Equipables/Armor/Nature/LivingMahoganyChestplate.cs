using Microsoft.Xna.Framework;

using RoA.Common.Items;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class LivingMahoganyChestplate : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Living Mahogany Chestplate");
        //Tooltip.SetDefault("6% increased nature potential damage");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 25, 0);

        Item.defense = 2;
    }

    public override void UpdateEquip(Player player) {
        player.GetDamage(DruidClass.Nature) += 0.04f;
    }
}