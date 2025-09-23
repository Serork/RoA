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

[AutoloadEquip(EquipType.Head)]
sealed class FlamewardenHood : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(Item.headSlot, this);
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 24);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.Lime7, Item.sellPrice());
    }

    public override void UpdateEquip(Player player) {

    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<FlamewardenMantle>() && legs.type == ModContent.ItemType<FlamewardenPants>();

    public override void UpdateArmorSet(Player player) {

    }

    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) => color = Color.White * WreathHandler.GetWreathChargeProgress_ForArmorGlow(player);
}