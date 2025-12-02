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
        glowMaskColor = Color.White * (1f - shadow) * 0.9f;
    }

    public override void SetDefaults() {
        int width = 18; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }


    public override bool IsVanitySet(int head, int body, int legs)
       => head == EquipLoader.GetEquipSlot(Mod, nameof(BRIPEsHelmet), EquipType.Head) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(BRIPEsHeart), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(BRIPEsRocketBoots), EquipType.Legs);

    public override void UpdateVanitySet(Player player) {
        player.armorEffectDrawOutlines = true;
    }
}
