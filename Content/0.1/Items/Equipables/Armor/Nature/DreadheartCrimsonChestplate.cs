using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Items;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class DreadheartCrimsonChestplate : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dreadheart Chestplate");
        //Tooltip.SetDefault("8% increased nature potential damage" + "\nIncreases nature knockback");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 80, 0);

        Item.defense = 5;
    }

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.08f;
        player.GetKnockback(DruidClass.Nature) += 0.5f;
		//player.GetAttackSpeed(DruidClass.Nature_Claws) += 0.07f;
    }
}