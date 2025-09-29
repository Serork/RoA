using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Legs)]
sealed class AshwalkerLeggings : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Ashwalker Leggings");
        //Tooltip.SetDefault("6% increased nature potential damage");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Legs, this);
    }

    protected override void SafeSetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 4;

        Item.value = Item.sellPrice(0, 0, 60, 0);
    }

    public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.06f;

    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
        color = Color.White * player.GetWreathHandler().ActualProgress5 * 0.85f;
    }
}