using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class DeerSkull : ModItem {
    public override void SetStaticDefaults() {
        //Tooltip.SetDefault("4% increased nature critical strike chance");
        ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 32; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.defense = 3;

        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(silver: 40);
    }

    public override void UpdateEquip(Player player) => player.GetCritChance(DruidClass.NatureDamage) += 4;

    //public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
    //	if (drawPlayer.active) {
    //		glowMask = RoAGlowMask.Get("DeerSkull_Head");
    //		glowMaskColor = Color.White;
    //	}
    //}
}
