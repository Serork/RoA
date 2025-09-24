using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Head)]
sealed class HallowedTopfhelm : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask {
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
        Item.SetSizeValues(22, 24);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateEquip(Player player) {
        player.setBonus = Language.GetTextValue("ArmorSetBonus.Hallowed");
        player.onHitDodge = true;

        player.armorEffectDrawOutlines = true;
        player.armorEffectDrawShadow = true;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemID.HallowedPlateMail && legs.type == ItemID.HallowedGreaves;

    public override void EquipFrameEffects(Player player, EquipType type) {
        if (player.isDisplayDollOrInanimate) {
            return;
        }

        if (!player.HasSetBonusFrom<HallowedTopfhelm>()) {
            return;
        }

        if (player.velocity.X != 0f && player.velocity.Y != 0f && Main.rand.Next(10) == 0) {
            int num12 = Dust.NewDust(new Vector2(player.position.X - player.velocity.X * 2f, player.position.Y - 2f - player.velocity.Y * 2f), player.width, player.height, 43, 0f, 0f, 100, default(Color), 0.3f);
            Main.dust[num12].fadeIn = 0.8f;
            Main.dust[num12].velocity *= 0f;
            Main.dust[num12].shader = GameShaders.Armor.GetSecondaryShader(player.ArmorSetDye(), player);
        }
    }
}