using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Head)]
sealed class LivingPearlwoodHelmet : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask, IDoubleTap, IPostSetupContent {
    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
        float progress = WreathHandler.GetWreathChargeProgress_ForArmorGlow(player);
        color = Color.Lerp(drawInfo.colorArmorHead, Color.White, 0.25f) * progress;
        color.A = (byte)(125 * progress);
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(Item.headSlot, this);
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 24);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID<LivingPearlwoodHelmet>();
        glowMaskColor = Color.White * (1f - shadow) * drawPlayer.GetModPlayer<WreathHandler>().ActualProgress5;
        glowMaskColor = glowMaskColor.MultiplyAlpha(1.5f);
    }

    public override void UpdateEquip(Player player) {

    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingPearlwoodChestplate>() && legs.type == ModContent.ItemType<LivingPearlwoodGreaves>();

    public override void UpdateArmorSet(Player player) {
        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.HasSetBonusFrom<LivingPearlwoodHelmet>() && direction == Helper.CurrentDoubleTapDirectionForSetBonuses && player.GetModPlayer<BaseFormHandler>().CanTransform) {
            BaseFormHandler.ToggleForm<HallowEnt>(player);
        }
    }

    void IPostSetupContent.PostSetupContent() {
        BaseFormHandler.RegisterForm<HallowEnt>();
    }
}