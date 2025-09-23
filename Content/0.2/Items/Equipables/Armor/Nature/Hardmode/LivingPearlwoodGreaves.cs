using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Legs)]
sealed class LivingPearlwoodGreaves : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask {
    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
        float progress = WreathHandler.GetWreathChargeProgress_ForArmorGlow(player);
        color = Color.Lerp(drawInfo.colorArmorHead, Color.White, 0.25f) * progress;
        color.A = (byte)(125 * progress);
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(22, 18);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateEquip(Player player) {

    }
}