using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Legs)]
sealed class BRIPEsRocketBoots : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("BRIPE's Rocket Boots");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Legs, this);
        //if (!Main.dedServ) {
        //	LegsGlowmask.RegisterData(Item.legSlot, new DrawLayerData() {
        //		Texture = ModContent.Request<Texture2D>(Texture + "_Legs_Glow")
        //	});
        //}
    }

    public override void SetDefaults() {
        int width = 22; int height = 16;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }
}
