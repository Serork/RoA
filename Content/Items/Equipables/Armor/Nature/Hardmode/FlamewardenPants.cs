using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Core.Defaults;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Legs)]
sealed class FlamewardenPants : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Legs, this);
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(22, 18);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.Lime7, Item.sellPrice());
    }

    public override void UpdateEquip(Player player) {

    }

    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) 
        => color = Color.White * MathHelper.Lerp(0.9f, 1f, WreathHandler.GetWreathChargeProgress_ForArmorGlow(player));
}