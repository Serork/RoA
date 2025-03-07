using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class BRIPEsHelmet : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("BRIPE's Helmet");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        //if (!Main.dedServ) {
        //	HeadGlowmask.RegisterData(Item.headSlot, new DrawLayerData() {
        //		Texture = ModContent.Request<Texture2D>(Texture + "_Head_Glow")
        //	});
        //}
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * (1f - shadow);
    }

    public override void SetDefaults() {
        int width = 18; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Cyan;

        Item.vanity = true;
    }


    public override bool IsArmorSet(Item head, Item body, Item legs)
        => body.type == ModContent.ItemType<BRIPEsHeart>() && legs.type == ModContent.ItemType<BRIPEsRocketBoots>();

    public override void ArmorSetShadows(Player player)
        => player.armorEffectDrawOutlines = true;

    //public override void UpdateArmorSet(Player player)
    //    => player.rocketBoots = 1;			
}
