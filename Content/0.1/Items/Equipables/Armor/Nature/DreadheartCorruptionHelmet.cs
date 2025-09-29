using Microsoft.Xna.Framework;

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
sealed class DreadheartCorruptionHelmet : NatureItem, IDoubleTap {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dreadheart Helmet");
        //Tooltip.SetDefault("4% increased nature critical strike chance");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        BaseFormHandler.RegisterForm<CorruptionInsectForm>();
    }

    protected override void SafeSetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 75, 0);

        Item.defense = 4;
    }

    public override void UpdateEquip(Player player) => player.GetCritChance(DruidClass.Nature) += 5;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<DreadheartCorruptionChestplate>() && legs.type == ModContent.ItemType<DreadheartCorruptionLeggings>();

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * (1f - shadow) * drawPlayer.GetWreathHandler().ActualProgress4;
    }

    public override void UpdateArmorSet(Player player) {
        string setBonus = Language.GetText("Mods.RoA.Items.Tooltips.DreadheartCrimsonSetBonus").WithFormatArgs(Helper.ArmorSetBonusKey).Value;
        player.setBonus = setBonus;

        player.GetModPlayer<DreadheartCrimsonHelmet.DreadheartSetBonusHandler>().IsEffectActive = true;

        BaseFormHandler.KeepFormActive(player);
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        if (player.CanTransfromIntoDruidForm<DreadheartCorruptionHelmet>(direction)) {
            BaseFormHandler.ToggleForm<CorruptionInsectForm>(player);
        }
    }
}