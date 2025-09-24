using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class AshwalkerRobe : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Ashwalker Robe");
        //Tooltip.SetDefault("10% increased nature base damage");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Body, this);
    }

    protected override void SafeSetDefaults() {
        int width = 30; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 6;

        Item.value = Item.sellPrice(0, 0, 60, 0);
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DruidClass.Nature) += 0.1f;

    public void SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
        color = Color.White * player.GetModPlayer<WreathHandler>().ActualProgress5;
    }
}