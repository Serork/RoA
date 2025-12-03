using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Head)]
sealed class AncientHallowedVisor : NatureItem, ItemGlowMaskHandler.IDrawArmorGlowMask, IDoubleTap {
    void ItemGlowMaskHandler.IDrawArmorGlowMask.SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo) {
        HallowedVisor.SetDrawSettings_Inner(player, ref texture, ref color, ref drawInfo);
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Head, this);
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 22);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetTextValue("ArmorSetBonus.Hallowed");
        player.onHitDodge = true;

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.CanTransfromIntoDruidForm<AncientHallowedVisor>(direction)) {
            BaseFormHandler.ToggleForm<HallowedGryphon>(player);
        }
    }

    public override void ArmorSetShadows(Player player) {
        player.armorEffectDrawOutlines = true;
        player.armorEffectDrawShadow = true;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => (body.type == ItemID.HallowedPlateMail || body.type == ItemID.AncientHallowedPlateMail) && 
                                                                        (legs.type == ItemID.HallowedGreaves || legs.type == ItemID.AncientHallowedGreaves);

    public override void EquipFrameEffects(Player player, EquipType type) {
        if (player.isDisplayDollOrInanimate) {
            return;
        }

        if (!player.HasSetBonusFrom<AncientHallowedVisor>()) {
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