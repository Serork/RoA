using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Body)]
sealed class BRIPEsHeart : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("BRIPE's Heart");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        //if (!Main.dedServ)
        //	BodyGlowmask.RegisterData(Item.bodySlot, () => new DrawColor(255, 255, 255, 0) * 0.8f * 0.75f);

        ItemGlowMaskHandler.BodyGlowMaskHandler2.RegisterData(Item.bodySlot, () => Color.White);
    }

    public override void SetDefaults() {
        int width = 34; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }
}
