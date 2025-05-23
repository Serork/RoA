using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Buffs;
using RoA.Content.Forms;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class AshwalkerHood : NatureItem, IDoubleTap, IPostSetupContent {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Ashwalker Hood");
        //Tooltip.SetDefault("6% increased nature potential damage");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 24; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 4;

        Item.value = Item.sellPrice(0, 0, 90, 0);
    }

    public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.06f;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<AshwalkerRobe>() && legs.type == ModContent.ItemType<AshwalkerLeggings>();

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * (1f - shadow) * drawPlayer.GetModPlayer<WreathHandler>().ActualProgress5;
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.AshwalkerHoodSetBonus").WithFormatArgs(Helper.ArmorSetBonusKey).Value;
        player.GetModPlayer<AshwalkerSetBonusHandler>().AshWalkerSet = true;
        //player.GetModPlayer<DruidArmorSetPlayer>().ashwalkerArmor = true;
        //Lighting.AddLight(player.Center, new Vector3(0.2f, 0.1f, 0.1f));

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.HasSetBonusFrom<AshwalkerHood>() && direction == Helper.CurrentDoubleTapDirectionForSetBonuses && player.GetModPlayer<BaseFormHandler>().CanTransform) {
            BaseFormHandler.ToggleForm<LilPhoenixForm>(player);
        }
    }

    void IPostSetupContent.PostSetupContent() {
        BaseFormHandler.RegisterForm<LilPhoenixForm>();
    }
}

sealed class AshwalkerSetBonusHandler : ModPlayer {
    internal bool AshWalkerSet;

    public override void ResetEffects() { }

    public override void PostUpdateBuffs() {
        if (AshWalkerSet && Player.FindBuff(BuffID.OnFire, out int buffIndex)) {
            Player.AddBuff(ModContent.BuffType<Unburning>(), Player.buffTime[buffIndex]);
            Player.ClearBuff(BuffID.OnFire);
        }
        AshWalkerSet = false;
    }
}