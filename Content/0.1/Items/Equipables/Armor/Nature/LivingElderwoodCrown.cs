using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Forms;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class LivingElderwoodCrown : NatureItem, IDoubleTap {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Living Elderwood Crown");
        //Tooltip.SetDefault("6% increased nature potential damage");
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        BaseFormHandler.RegisterForm<FlederForm>();
    }

    protected override void SafeSetDefaults() {
        int width = 26; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 15, 0);

        Item.defense = 2;
    }

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.05f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingElderwoodBreastplate>() && legs.type == ModContent.ItemType<LivingElderwoodGreaves>();

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * (1f - shadow) * drawPlayer.GetWreathHandler().ActualProgress5;
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.LivingElderwoodCrownSetBonus").WithFormatArgs(Helper.ArmorSetBonusKey).Value;
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.1f;

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.CanTransfromIntoDruidForm<LivingElderwoodCrown>(direction)) {
            BaseFormHandler.ToggleForm<FlederForm>(player);
        }
    }
}