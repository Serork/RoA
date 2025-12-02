using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Body)]
sealed class NFAJacket : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("le Blanc's Shirt");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ItemGlowMaskHandler.BodyGlowMaskHandler2.RegisterData(Item.bodySlot, () => Color.White * 0.9f);
    }

    public override void SetDefaults() {
        int width = 34; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }
}
